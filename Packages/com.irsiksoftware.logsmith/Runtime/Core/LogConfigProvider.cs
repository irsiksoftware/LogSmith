using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Minimal configuration provider with hot-reload support.
    /// </summary>
    public class LogConfigProvider : ILogConfigProvider
    {
        private readonly List<Action<LogConfig>> _subscribers = new List<Action<LogConfig>>();
        private readonly object _lock = new object();
        private LogConfig _currentConfig;

        public LogConfigProvider()
        {
            _currentConfig = new LogConfig();
        }

        public LogConfig GetConfig()
        {
            lock (_lock)
            {
                return _currentConfig;
            }
        }

        public void ReloadConfig()
        {
            lock (_lock)
            {
                // In a full implementation, this would load from ScriptableObject
                // For now, just notify subscribers with current config
                var config = _currentConfig;

                foreach (var subscriber in _subscribers)
                {
                    try
                    {
                        subscriber(config);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[LogSmith] Config subscriber failed: {ex.Message}");
                    }
                }
            }
        }

        public IDisposable Subscribe(Action<LogConfig> onConfigChanged)
        {
            if (onConfigChanged == null)
                throw new ArgumentNullException(nameof(onConfigChanged));

            lock (_lock)
            {
                _subscribers.Add(onConfigChanged);

                // Immediately notify with current config
                try
                {
                    onConfigChanged(_currentConfig);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError($"[LogSmith] Config subscriber failed: {ex.Message}");
#endif
                }
            }

            return new Subscription(this, onConfigChanged);
        }

        /// <summary>
        /// Updates the configuration (for testing or runtime changes).
        /// </summary>
        public void UpdateConfig(LogConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            lock (_lock)
            {
                _currentConfig = config;
                ReloadConfig();
            }
        }

        private class Subscription : IDisposable
        {
            private readonly LogConfigProvider _provider;
            private readonly Action<LogConfig> _handler;
            private bool _disposed;

            public Subscription(LogConfigProvider provider, Action<LogConfig> handler)
            {
                _provider = provider;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;

                lock (_provider._lock)
                {
                    _provider._subscribers.Remove(_handler);
                }
            }
        }
    }
}
