using System;
using System.Text;
using IrsikSoftware.LogSmith;
using UnityEngine.Networking;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Example custom sink that sends log messages to an HTTP endpoint.
    /// This is a stub implementation demonstrating how to create third-party sinks.
    ///
    /// Usage:
    /// 1. Copy this file to your project
    /// 2. Customize the endpoint URL and formatting logic
    /// 3. Register with LogRouter:
    ///    var httpSink = new HttpSink("https://your-endpoint.com/logs");
    ///    LogSmith.Router.RegisterSink(httpSink);
    /// </summary>
    public class HttpSink : ILogSink
    {
        private readonly string _endpoint;
        private readonly IMessageTemplateEngine _templateEngine;
        private bool _disposed;
        private MessageFormat _currentFormat = MessageFormat.Json;

        public string Name => "HTTPSink";

        public MessageFormat CurrentFormat
        {
            get => _currentFormat;
            set => _currentFormat = value;
        }

        /// <summary>
        /// Creates an HTTP sink that posts log messages to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint URL</param>
        /// <param name="templateEngine">Optional template engine (will use default if null)</param>
        public HttpSink(string endpoint, IMessageTemplateEngine templateEngine = null)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("Endpoint URL cannot be null or empty", nameof(endpoint));
            }

            _endpoint = endpoint;
            _templateEngine = templateEngine ?? new Core.MessageTemplateEngine();
        }

        /// <summary>
        /// Writes a log message by POSTing it to the HTTP endpoint.
        /// Note: In production, you might want to batch messages or use a background queue.
        /// </summary>
        public void Write(LogMessage message)
        {
            if (_disposed) return;

            try
            {
                // Format the message
                string formattedMessage = _templateEngine.Format(message, _currentFormat);

                // In a real implementation, you would:
                // 1. Queue messages for batch sending
                // 2. Handle retries and failures
                // 3. Respect rate limits
                // 4. Use a background thread/job

                // For this stub, we just prepare the request
                SendToEndpoint(formattedMessage);
            }
            catch (Exception ex)
            {
                // In production, consider:
                // - Logging to fallback sink
                // - Metrics/alerting
                // - Circuit breaker pattern
                UnityEngine.Debug.LogWarning($"[HttpSink] Failed to send log: {ex.Message}");
            }
        }

        /// <summary>
        /// Stub method showing how to send data to HTTP endpoint.
        /// In production, implement proper async handling and error recovery.
        /// </summary>
        private void SendToEndpoint(string payload)
        {
            // STUB IMPLEMENTATION - Not production ready!
            // This demonstrates the structure, but doesn't actually send requests.

            // In a real implementation:
            // using (var request = new UnityWebRequest(_endpoint, "POST"))
            // {
            //     byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            //     request.downloadHandler = new DownloadHandlerBuffer();
            //     request.SetRequestHeader("Content-Type", "application/json");
            //
            //     // Send async (ideally on background thread or using coroutine)
            //     var operation = request.SendWebRequest();
            //     // Handle response...
            // }

            // For now, just log that we would send it
            UnityEngine.Debug.Log($"[HttpSink] Would POST to {_endpoint}: {payload.Substring(0, Math.Min(100, payload.Length))}...");
        }

        /// <summary>
        /// Flush any pending messages.
        /// In production, this would force-send any queued messages.
        /// </summary>
        public void Flush()
        {
            if (_disposed) return;

            // In production: flush any queued messages
            // For stub: nothing to do
        }

        /// <summary>
        /// Disposes the sink and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            // In production:
            // - Flush remaining messages
            // - Cancel pending requests
            // - Dispose HTTP client
        }
    }
}
