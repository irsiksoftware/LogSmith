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
    /// Seq sink for structured logging to Seq server.
    /// Supports CLEF (Compact Log Event Format) for rich structured logs.
    /// </summary>
    public class SeqSink : ILogSink, IDisposable
    {
        private readonly string _serverUrl;
        private readonly string _apiKey;
        private readonly int _batchSize;
        private readonly float _flushInterval;
        private readonly List<LogMessage> _batch;
        private readonly object _lock = new object();
        private float _lastFlushTime;
        private bool _disposed;
        private MonoBehaviour _coroutineRunner;

        public string Name => "Seq";

        /// <summary>
        /// Creates a new Seq sink.
        /// </summary>
        /// <param name="serverUrl">Seq server URL (e.g., "http://localhost:5341")</param>
        /// <param name="apiKey">Optional API key for authentication</param>
        /// <param name="batchSize">Number of messages to batch before sending (default: 10)</param>
        /// <param name="flushInterval">Time in seconds between automatic flushes (default: 5.0)</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines (required)</param>
        public SeqSink(string serverUrl, string apiKey = null, int batchSize = 10, float flushInterval = 5.0f, MonoBehaviour coroutineRunner = null)
        {
            if (string.IsNullOrEmpty(serverUrl))
                throw new ArgumentNullException(nameof(serverUrl));

            if (coroutineRunner == null)
                throw new ArgumentNullException(nameof(coroutineRunner), "SeqSink requires a MonoBehaviour to run coroutines");

            _serverUrl = serverUrl.TrimEnd('/');
            _apiKey = apiKey;
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
                _coroutineRunner.StartCoroutine(SendBatchAsync(toSend));
            }
        }

        private IEnumerator SendBatchAsync(List<LogMessage> messages)
        {
            // Seq uses CLEF format (one JSON object per line)
            string clef = SerializeAsCLEF(messages);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(clef);

            string endpoint = $"{_serverUrl}/api/events/raw";
            if (!string.IsNullOrEmpty(_apiKey))
            {
                endpoint += $"?apiKey={_apiKey}";
            }

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/vnd.serilog.clef");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[LogSmith.SeqSink] Failed to send logs: {request.error}");
                }
            }
        }

        private string SerializeAsCLEF(List<LogMessage> messages)
        {
            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                sb.Append("{");

                // Timestamp (ISO 8601 format)
                sb.Append($"\"@t\":\"{msg.Timestamp:O}\",");

                // Message template
                sb.Append($"\"@mt\":\"{EscapeJson(msg.Message)}\",");

                // Level
                sb.Append($"\"@l\":\"{MapLogLevel(msg.Level)}\",");

                // Source context (category)
                if (!string.IsNullOrEmpty(msg.Category))
                {
                    sb.Append($"\"SourceContext\":\"{EscapeJson(msg.Category)}\",");
                }

                // Exception (if present via stack trace)
                if (!string.IsNullOrEmpty(msg.StackTrace))
                {
                    sb.Append($"\"@x\":\"{EscapeJson(msg.StackTrace)}\",");
                }

                // Properties
                sb.Append($"\"Frame\":{msg.Frame},");
                sb.Append($"\"ThreadId\":{msg.ThreadId}");

                if (!string.IsNullOrEmpty(msg.ThreadName))
                {
                    sb.Append($",\"ThreadName\":\"{EscapeJson(msg.ThreadName)}\"");
                }

                if (!string.IsNullOrEmpty(msg.CallerFilePath))
                {
                    sb.Append($",\"CallerFilePath\":\"{EscapeJson(msg.CallerFilePath)}\"");
                }

                if (!string.IsNullOrEmpty(msg.CallerMemberName))
                {
                    sb.Append($",\"CallerMemberName\":\"{EscapeJson(msg.CallerMemberName)}\"");
                }

                if (msg.CallerLineNumber > 0)
                {
                    sb.Append($",\"CallerLineNumber\":{msg.CallerLineNumber}");
                }

                // Application metadata
                sb.Append($",\"Application\":\"Unity\"");
                sb.Append($",\"UnityVersion\":\"{EscapeJson(Application.unityVersion)}\"");
                sb.Append($",\"Platform\":\"{Application.platform}\"");

                // Custom context properties
                if (msg.Context != null && msg.Context.Count > 0)
                {
                    foreach (var kvp in msg.Context)
                    {
                        sb.Append($",\"{EscapeJson(kvp.Key)}\":");

                        // Try to preserve type information for primitives
                        if (kvp.Value is int || kvp.Value is long || kvp.Value is float || kvp.Value is double || kvp.Value is bool)
                        {
                            sb.Append($"{kvp.Value.ToString().ToLowerInvariant()}");
                        }
                        else
                        {
                            sb.Append($"\"{EscapeJson(kvp.Value?.ToString() ?? "null")}\"");
                        }
                    }
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
                    return "Verbose";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Info:
                    return "Information";
                case LogLevel.Warn:
                    return "Warning";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Critical:
                    return "Fatal";
                default:
                    return "Information";
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
