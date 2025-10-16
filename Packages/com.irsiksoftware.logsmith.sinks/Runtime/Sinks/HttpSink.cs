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
    /// HTTP/REST sink for sending log messages to remote endpoints.
    /// Supports batching and JSON serialization.
    /// </summary>
    public class HttpSink : ILogSink, IDisposable
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly int _batchSize;
        private readonly float _flushInterval;
        private readonly List<LogMessage> _batch;
        private readonly object _lock = new object();
        private float _lastFlushTime;
        private bool _disposed;
        private MonoBehaviour _coroutineRunner;

        public string Name => "HTTP";

        /// <summary>
        /// Creates a new HTTP sink.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint URL to send logs to</param>
        /// <param name="apiKey">Optional API key for authentication</param>
        /// <param name="batchSize">Number of messages to batch before sending (default: 10)</param>
        /// <param name="flushInterval">Time in seconds between automatic flushes (default: 5.0)</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines (required)</param>
        public HttpSink(string endpoint, string apiKey = null, int batchSize = 10, float flushInterval = 5.0f, MonoBehaviour coroutineRunner = null)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            if (coroutineRunner == null)
                throw new ArgumentNullException(nameof(coroutineRunner), "HttpSink requires a MonoBehaviour to run coroutines");

            _endpoint = endpoint;
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
            string json = SerializeMessages(messages);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(_endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {_apiKey}");
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[LogSmith.HttpSink] Failed to send logs: {request.error}");
                }
            }
        }

        private string SerializeMessages(List<LogMessage> messages)
        {
            var sb = new StringBuilder();
            sb.Append("{\"logs\":[");

            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");

                var msg = messages[i];
                sb.Append("{");
                sb.Append($"\"timestamp\":\"{msg.Timestamp:O}\",");
                sb.Append($"\"level\":\"{msg.Level}\",");
                sb.Append($"\"category\":\"{EscapeJson(msg.Category)}\",");
                sb.Append($"\"message\":\"{EscapeJson(msg.Message)}\",");
                sb.Append($"\"frame\":{msg.Frame},");
                sb.Append($"\"threadId\":{msg.ThreadId}");

                if (!string.IsNullOrEmpty(msg.ThreadName))
                {
                    sb.Append($",\"threadName\":\"{EscapeJson(msg.ThreadName)}\"");
                }

                if (!string.IsNullOrEmpty(msg.StackTrace))
                {
                    sb.Append($",\"stackTrace\":\"{EscapeJson(msg.StackTrace)}\"");
                }

                if (msg.Context != null && msg.Context.Count > 0)
                {
                    sb.Append(",\"context\":{");
                    bool first = true;
                    foreach (var kvp in msg.Context)
                    {
                        if (!first) sb.Append(",");
                        sb.Append($"\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value?.ToString() ?? "null")}\"");
                        first = false;
                    }
                    sb.Append("}");
                }

                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
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
