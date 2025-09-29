using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Routes log messages to registered sinks with event stream support.
    /// </summary>
    internal class LogRouter : ILogRouter
    {
        private readonly List<ILogSink> _sinks = new List<ILogSink>();
        private readonly List<Action<LogMessage>> _subscribers = new List<Action<LogMessage>>();
        private readonly object _lock = new object();

        public void RegisterSink(ILogSink sink)
        {
            if (sink == null) throw new ArgumentNullException(nameof(sink));

            lock (_lock)
            {
                if (!_sinks.Contains(sink))
                {
                    _sinks.Add(sink);
                }
            }
        }

        public void UnregisterSink(ILogSink sink)
        {
            if (sink == null) throw new ArgumentNullException(nameof(sink));

            lock (_lock)
            {
                _sinks.Remove(sink);
            }
        }

        public void Route(LogMessage message)
        {
            lock (_lock)
            {
                // Route to all registered sinks
                foreach (var sink in _sinks)
                {
                    try
                    {
                        sink.Write(message);
                    }
                    catch (Exception ex)
                    {
                        // Prevent sink failures from breaking other sinks
                        UnityEngine.Debug.LogError($"[LogSmith] Sink '{sink.Name}' failed: {ex.Message}");
                    }
                }

                // Notify subscribers
                foreach (var subscriber in _subscribers)
                {
                    try
                    {
                        subscriber(message);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[LogSmith] Subscriber failed: {ex.Message}");
                    }
                }
            }
        }

        public IDisposable Subscribe(Action<LogMessage> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                _subscribers.Add(handler);
            }

            return new Subscription(this, handler);
        }

        private class Subscription : IDisposable
        {
            private readonly LogRouter _router;
            private readonly Action<LogMessage> _handler;
            private bool _disposed;

            public Subscription(LogRouter router, Action<LogMessage> handler)
            {
                _router = router;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                lock (_router._lock)
                {
                    _router._subscribers.Remove(_handler);
                }
            }
        }
    }
}