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
    /// Sentry sink for error tracking and monitoring.
    /// Sends error and critical logs to Sentry for centralized tracking.
    /// </summary>
    public class SentrySink : ILogSink, IDisposable
    {
        private readonly string _dsn;
        private readonly string _environment;
        private readonly string _release;
        private readonly LogLevel _minimumLevel;
        private readonly MonoBehaviour _coroutineRunner;
        private readonly object _lock = new object();
        private bool _disposed;

        public string Name => "Sentry";

        /// <summary>
        /// Creates a new Sentry sink.
        /// </summary>
        /// <param name="dsn">Sentry Data Source Name (DSN)</param>
        /// <param name="environment">Environment name (e.g., "production", "staging")</param>
        /// <param name="release">Release/version identifier</param>
        /// <param name="minimumLevel">Minimum log level to send (default: Error)</param>
        /// <param name="coroutineRunner">MonoBehaviour to run coroutines (required)</param>
        public SentrySink(string dsn, string environment = "production", string release = null,
            LogLevel minimumLevel = LogLevel.Error, MonoBehaviour coroutineRunner = null)
        {
            if (string.IsNullOrEmpty(dsn))
                throw new ArgumentNullException(nameof(dsn));

            if (coroutineRunner == null)
                throw new ArgumentNullException(nameof(coroutineRunner), "SentrySink requires a MonoBehaviour to run coroutines");

            _dsn = dsn;
            _environment = environment;
            _release = release ?? Application.version;
            _minimumLevel = minimumLevel;
            _coroutineRunner = coroutineRunner;
        }

        public void Write(LogMessage message)
        {
            if (_disposed) return;

            // Filter by minimum level
            if (message.Level < _minimumLevel) return;

            lock (_lock)
            {
                if (_coroutineRunner != null)
                {
                    _coroutineRunner.StartCoroutine(SendEventAsync(message));
                }
            }
        }

        public void Flush()
        {
            // Sentry events are sent immediately, no buffering
        }

        private IEnumerator SendEventAsync(LogMessage message)
        {
            string json = SerializeSentryEvent(message);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            // Parse DSN to get endpoint
            string endpoint = GetSentryEndpoint(_dsn);

            using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Sentry-Auth", BuildAuthHeader());

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[LogSmith.SentrySink] Failed to send event: {request.error}");
                }
            }
        }

        private string SerializeSentryEvent(LogMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            // Event ID
            sb.Append($"\"event_id\":\"{Guid.NewGuid():N}\",");

            // Timestamp
            sb.Append($"\"timestamp\":\"{message.Timestamp:O}\",");

            // Platform
            sb.Append("\"platform\":\"csharp\",");

            // Level mapping
            string sentryLevel = MapLogLevel(message.Level);
            sb.Append($"\"level\":\"{sentryLevel}\",");

            // Logger
            sb.Append($"\"logger\":\"{EscapeJson(message.Category)}\",");

            // Message
            sb.Append($"\"message\":\"{EscapeJson(message.Message)}\",");

            // Environment and release
            sb.Append($"\"environment\":\"{EscapeJson(_environment)}\",");
            sb.Append($"\"release\":\"{EscapeJson(_release)}\",");

            // Tags
            sb.Append("\"tags\":{");
            sb.Append($"\"unity.version\":\"{EscapeJson(Application.unityVersion)}\",");
            sb.Append($"\"platform\":\"{Application.platform}\",");
            sb.Append($"\"frame\":\"{message.Frame}\"");
            sb.Append("},");

            // Extra context
            sb.Append("\"extra\":{");
            sb.Append($"\"thread_id\":{message.ThreadId}");

            if (!string.IsNullOrEmpty(message.ThreadName))
            {
                sb.Append($",\"thread_name\":\"{EscapeJson(message.ThreadName)}\"");
            }

            if (!string.IsNullOrEmpty(message.CallerFilePath))
            {
                sb.Append($",\"caller_file\":\"{EscapeJson(message.CallerFilePath)}\"");
            }

            if (!string.IsNullOrEmpty(message.CallerMemberName))
            {
                sb.Append($",\"caller_member\":\"{EscapeJson(message.CallerMemberName)}\"");
            }

            if (message.CallerLineNumber > 0)
            {
                sb.Append($",\"caller_line\":{message.CallerLineNumber}");
            }

            if (message.Context != null && message.Context.Count > 0)
            {
                foreach (var kvp in message.Context)
                {
                    sb.Append($",\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value?.ToString() ?? "null")}\"");
                }
            }

            sb.Append("}");

            // Stack trace
            if (!string.IsNullOrEmpty(message.StackTrace))
            {
                sb.Append(",\"stacktrace\":{\"frames\":[");
                sb.Append("{\"filename\":\"Unity\",");
                sb.Append($"\"function\":\"{EscapeJson(message.CallerMemberName ?? "Unknown")}\",");
                sb.Append($"\"lineno\":{message.CallerLineNumber}");
                sb.Append("}");
                sb.Append("]}");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string MapLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return "debug";
                case LogLevel.Info:
                    return "info";
                case LogLevel.Warn:
                    return "warning";
                case LogLevel.Error:
                    return "error";
                case LogLevel.Critical:
                    return "fatal";
                default:
                    return "error";
            }
        }

        private string GetSentryEndpoint(string dsn)
        {
            // Parse DSN: https://<key>@<host>/<project-id>
            // Returns: https://<host>/api/<project-id>/store/
            try
            {
                var uri = new Uri(dsn);
                var projectId = uri.AbsolutePath.TrimStart('/');
                return $"{uri.Scheme}://{uri.Host}/api/{projectId}/store/";
            }
            catch
            {
                throw new ArgumentException("Invalid Sentry DSN format", nameof(dsn));
            }
        }

        private string BuildAuthHeader()
        {
            // Extract key from DSN
            var uri = new Uri(_dsn);
            string key = uri.UserInfo;

            return $"Sentry sentry_version=7, sentry_client=LogSmith/1.0, sentry_key={key}";
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
                _disposed = true;
            }
        }
    }
}
