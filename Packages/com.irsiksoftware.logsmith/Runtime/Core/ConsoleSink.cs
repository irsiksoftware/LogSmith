using System;
using IrsikSoftware.LogSmith.Adapters;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Console sink implementation using Unity's native logging backend.
    /// </summary>
    public class ConsoleSink : ILogSink, IDisposable
    {
        private readonly IMessageTemplateEngine _templateEngine;
        private readonly object _lock = new object();
        private bool _disposed;
        private MessageFormat _currentFormat = MessageFormat.Text;

        public string Name => "Console";

        /// <summary>
        /// Gets or sets the current message format (Text or JSON).
        /// </summary>
        public MessageFormat CurrentFormat
        {
            get
            {
                lock (_lock)
                {
                    return _currentFormat;
                }
            }
            set
            {
                lock (_lock)
                {
                    _currentFormat = value;
                }
            }
        }

        public ConsoleSink(IMessageTemplateEngine templateEngine = null)
        {
            _templateEngine = templateEngine ?? new MessageTemplateEngine();
        }

        public void Write(LogMessage message)
        {
            if (_disposed) return;

            lock (_lock)
            {
                try
                {
                    // Format message using template engine if needed
                    // For console, we typically just use the raw message
                    // but template engine allows consistent formatting across sinks
                    NativeUnityLoggerAdapter.Write(message.Level, message.Category, message.Message);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LogSmith] ConsoleSink write failed: {ex.Message}");
                }
            }
        }

        public void Flush()
        {
            // Unity.Logging handles flushing internally
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