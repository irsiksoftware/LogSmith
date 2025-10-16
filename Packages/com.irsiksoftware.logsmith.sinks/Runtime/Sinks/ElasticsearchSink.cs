using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IrsikSoftware.LogSmith;
using UnityEngine;
using UnityEngine.Networking;

namespace IrsikSoftware.LogSmith.Sinks
{
    /// <summary>
    /// Elasticsearch sink for sending structured logs to Elasticsearch.
    /// Supports bulk indexing for efficient batch operations.
    /// </summary>
    public class ElasticsearchSink : ILogSink, IDisposable
    {
        private readonly string _nodeUrl;
        private readonly string _indexName;
        private readonly string _username;
        private readonly string _password;
        private readonly int _batchSize;
        private readonly float _flushInterval;
        private readonly List<LogMessage> _batch;
        private readonly object _lock = new object();
        private float _lastFlushTime;
        private bool _disposed;
        private MonoBehaviour _coroutineRunner;

        public string Name => "Elasticsearch";

        /// <summary>
        /// Creates a new Elasticsearch sink.
        /// </summary>
        /// <param name="nodeUrl">Elasticsearch node URL (e.g., "http://localhost:9200")</param>
        /// <param name="indexName">Index name pattern (e.g., "logsmith-{0:yyyy.MM.dd}")</param>
        /// <param name="username">Optional username for authentication</param>
        /// <param name="password">Optional password for authentication</param>
        /// <param name="batchSize">Number of messages to batch before sending (default: 50)</param>
        /// <param name="flushInterval">Time in seconds between automatic flushes (default: 10.0)</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines (required)</param>
        public ElasticsearchSink(string nodeUrl, string indexName = "logsmith-{0:yyyy.MM.dd}",
            string username = null, string password = null, int batchSize = 50, float flushInterval = 10.0f,
            MonoBehaviour coroutineRunner = null)
        {
            if (string.IsNullOrEmpty(nodeUrl))
                throw new ArgumentNullException(nameof(nodeUrl));

            if (coroutineRunner == null)
                throw new ArgumentNullException(nameof(coroutineRunner), "ElasticsearchSink requires a MonoBehaviour to run coroutines");

            _nodeUrl = nodeUrl.TrimEnd('/');
            _indexName = indexName;
            _username = username;
            _password = password;
            _batchSize = batchSize;
            _flushInterval = flushInterval;
            _batch = new List<LogMessage>(batchSize);
            _lastFlushTime = Time.time;
            _coroutineRunner = coroutineRunner;
        }

        public void Write(LogMessage message)
        {
            if (_disposed) return;

            lock (_lock)
            {
                _batch.Add(message);

                // Auto-flush if batch size reached or interval elapsed
                if (_batch.Count >= _batchSize || (Time.time - _lastFlushTime) >= _flushInterval)
                {
                    Flush();
                }
            }
        }

        public void Flush()
        {
            if (_disposed) return;

            List<LogMessage> toSend;
            lock (_lock)
            {
                if (_batch.Count == 0) return;

                toSend = new List<LogMessage>(_batch);
                _batch.Clear();
                _lastFlushTime = Time.time;
            }

            // Send batch asynchronously
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(SendBulkAsync(toSend));
            }
        }

        private IEnumerator SendBulkAsync(List<LogMessage> messages)
        {
            // Use bulk API for efficient batch indexing
            string bulkData = SerializeBulkRequest(messages);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bulkData);

            string endpoint = $"{_nodeUrl}/_bulk";

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/x-ndjson");

                // Add authentication if credentials provided
                if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
                {
                    string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                    request.SetRequestHeader("Authorization", $"Basic {auth}");
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[LogSmith.ElasticsearchSink] Failed to send logs: {request.error}");
                }
            }
        }

        private string SerializeBulkRequest(List<LogMessage> messages)
        {
            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                // Index name with date pattern
                string indexName = string.Format(_indexName, msg.Timestamp);

                // Action line (index operation)
                sb.Append("{\"index\":{\"_index\":\"");
                sb.Append(indexName);
                sb.Append("\"}}\n");

                // Document line
                sb.Append("{");

                // ECS (Elastic Common Schema) compatible fields
                sb.Append($"\"@timestamp\":\"{msg.Timestamp:O}\",");
                sb.Append($"\"message\":\"{EscapeJson(msg.Message)}\",");

                // Log fields
                sb.Append("\"log\":{");
                sb.Append($"\"level\":\"{MapLogLevel(msg.Level)}\",");
                sb.Append($"\"logger\":\"{EscapeJson(msg.Category)}\"");
                sb.Append("},");

                // Process fields
                sb.Append("\"process\":{");
                sb.Append($"\"thread\":{{\"id\":{msg.ThreadId}");
                if (!string.IsNullOrEmpty(msg.ThreadName))
                {
                    sb.Append($",\"name\":\"{EscapeJson(msg.ThreadName)}\"");
                }
                sb.Append("}");
                sb.Append("},");

                // Unity-specific fields
                sb.Append("\"unity\":{");
                sb.Append($"\"frame\":{msg.Frame},");
                sb.Append($"\"version\":\"{EscapeJson(Application.unityVersion)}\",");
                sb.Append($"\"platform\":\"{Application.platform}\"");
                sb.Append("},");

                // Source code location
                if (!string.IsNullOrEmpty(msg.CallerFilePath) || !string.IsNullOrEmpty(msg.CallerMemberName))
                {
                    sb.Append("\"source\":{");
                    bool hasField = false;

                    if (!string.IsNullOrEmpty(msg.CallerFilePath))
                    {
                        sb.Append($"\"file\":\"{EscapeJson(msg.CallerFilePath)}\"");
                        hasField = true;
                    }

                    if (!string.IsNullOrEmpty(msg.CallerMemberName))
                    {
                        if (hasField) sb.Append(",");
                        sb.Append($"\"function\":\"{EscapeJson(msg.CallerMemberName)}\"");
                        hasField = true;
                    }

                    if (msg.CallerLineNumber > 0)
                    {
                        if (hasField) sb.Append(",");
                        sb.Append($"\"line\":{msg.CallerLineNumber}");
                    }

                    sb.Append("},");
                }

                // Error fields (stack trace)
                if (!string.IsNullOrEmpty(msg.StackTrace))
                {
                    sb.Append("\"error\":{");
                    sb.Append($"\"stack_trace\":\"{EscapeJson(msg.StackTrace)}\"");
                    sb.Append("},");
                }

                // Custom context as labels
                if (msg.Context != null && msg.Context.Count > 0)
                {
                    sb.Append("\"labels\":{");
                    bool first = true;
                    foreach (var kvp in msg.Context)
                    {
                        if (!first) sb.Append(",");
                        sb.Append($"\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value?.ToString() ?? "null")}\"");
                        first = false;
                    }
                    sb.Append("},");
                }

                // Remove trailing comma
                if (sb[sb.Length - 1] == ',')
                {
                    sb.Length--;
                }

                sb.Append("}\n");
            }

            return sb.ToString();
        }

        private string MapLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return "trace";
                case LogLevel.Debug:
                    return "debug";
                case LogLevel.Info:
                    return "info";
                case LogLevel.Warn:
                    return "warning";
                case LogLevel.Error:
                    return "error";
                case LogLevel.Critical:
                    return "critical";
                default:
                    return "info";
            }
        }

        private string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                Flush();
                _disposed = true;
            }
        }
    }
}
