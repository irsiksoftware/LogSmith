using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Routes log messages to registered sinks with filtering and event stream support.
    /// </summary>
    internal class LogRouter : ILogRouter
    {
        private readonly List<ILogSink> _sinks = new List<ILogSink>();
        private readonly List<Action<LogMessage>> _subscribers = new List<Action<LogMessage>>();
        private readonly Dictionary<string, LogLevel> _categoryFilters = new Dictionary<string, LogLevel>();
        private readonly object _lock = new object();
        private LogLevel _globalMinimumLevel = LogLevel.Trace;

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
                // Apply filters
                if (!ShouldRoute(message))
                {
                    return;
                }

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

        /// <summary>
        /// Sets the global minimum log level for all categories.
        /// </summary>
        public void SetGlobalMinimumLevel(LogLevel level)
        {
            lock (_lock)
            {
                _globalMinimumLevel = level;
            }
        }

        /// <summary>
        /// Sets a minimum log level filter for a specific category.
        /// </summary>
        public void SetCategoryFilter(string category, LogLevel minimumLevel)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                _categoryFilters[category] = minimumLevel;
            }
        }

        /// <summary>
        /// Removes the filter for a specific category.
        /// </summary>
        public void ClearCategoryFilter(string category)
        {
            if (string.IsNullOrEmpty(category)) return;

            lock (_lock)
            {
                _categoryFilters.Remove(category);
            }
        }

        private bool ShouldRoute(LogMessage message)
        {
            // Check category-specific filter first
            if (_categoryFilters.TryGetValue(message.Category, out var categoryMinLevel))
            {
                return message.Level >= categoryMinLevel;
            }

            // Fall back to global minimum level
            return message.Level >= _globalMinimumLevel;
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