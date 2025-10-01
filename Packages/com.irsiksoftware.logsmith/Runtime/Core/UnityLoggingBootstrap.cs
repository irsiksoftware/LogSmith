using System;
using System.IO;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Bootstraps the Unity Logging backend with configured console and file sinks.
    /// Supports live reload of settings and runtime format switching.
    /// </summary>
    public class UnityLoggingBootstrap : IDisposable
    {
        private readonly LoggingSettings _settings;
        private readonly ILogRouter _router;
        private readonly IMessageTemplateEngine _templateEngine;

        private ConsoleSink _consoleSink;
        private FileSink _fileSink;
        private bool _disposed;
        private bool _liveReloadEnabled;

        /// <summary>
        /// Gets the console sink instance if enabled.
        /// </summary>
        public ConsoleSink ConsoleSink => _consoleSink;

        /// <summary>
        /// Gets the file sink instance if enabled.
        /// </summary>
        public FileSink FileSink => _fileSink;

        /// <summary>
        /// Initializes a new instance of UnityLoggingBootstrap with the specified settings.
        /// </summary>
        /// <param name="settings">The logging settings to use.</param>
        /// <param name="router">The log router to register sinks with.</param>
        /// <param name="templateEngine">Optional template engine for message formatting.</param>
        public UnityLoggingBootstrap(LoggingSettings settings, ILogRouter router, IMessageTemplateEngine templateEngine = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _templateEngine = templateEngine ?? new MessageTemplateEngine();

            Initialize();
        }

        /// <summary>
        /// Initializes the logging system by configuring and registering sinks.
        /// </summary>
        private void Initialize()
        {
            // Initialize Unity.Logging backend adapter
            Adapters.NativeUnityLoggerAdapter.Initialize();

            // Configure and register console sink
            if (_settings.enableConsoleSink)
            {
                _consoleSink = new ConsoleSink();
                _router.RegisterSink(_consoleSink);
            }

            // Configure and register file sink with rotation support
            if (_settings.enableFileSink)
            {
                var fullLogPath = GetFullLogPath(_settings.logFilePath);
                _fileSink = new FileSink(
                    fullLogPath,
                    _templateEngine,
                    _settings.enableLogRotation,
                    _settings.maxFileSizeMB,
                    _settings.retentionCount
                );

                // Set initial format mode
                _fileSink.CurrentFormat = _settings.defaultFormatMode == MessageFormatMode.Text
                    ? MessageFormat.Text
                    : MessageFormat.Json;

                _router.RegisterSink(_fileSink);
            }

            // Set global minimum log level
            _router.SetGlobalMinimumLevel(_settings.minimumLogLevel);

            // Enable live reload if configured
            _liveReloadEnabled = _settings.enableLiveReload;

            Debug.Log($"[LogSmith] UnityLoggingBootstrap initialized - Console: {_settings.enableConsoleSink}, File: {_settings.enableFileSink}, Rotation: {_settings.enableLogRotation}, Format: {_settings.defaultFormatMode}");
        }

        /// <summary>
        /// Reloads the logging configuration from settings.
        /// This allows runtime changes to take effect without restarting.
        /// </summary>
        public void ReloadSettings()
        {
            if (!_liveReloadEnabled)
            {
                Debug.LogWarning("[LogSmith] Live reload is disabled in settings");
                return;
            }

            Debug.Log("[LogSmith] Reloading logging settings...");

            // Update global minimum log level
            _router.SetGlobalMinimumLevel(_settings.minimumLogLevel);

            // Update file sink format if enabled
            if (_fileSink != null)
            {
                _fileSink.CurrentFormat = _settings.defaultFormatMode == MessageFormatMode.Text
                    ? MessageFormat.Text
                    : MessageFormat.Json;
            }

            Debug.Log($"[LogSmith] Settings reloaded - MinLevel: {_settings.minimumLogLevel}, Format: {_settings.defaultFormatMode}");
        }

        /// <summary>
        /// Switches the file sink output format at runtime.
        /// </summary>
        /// <param name="format">The desired message format (Text or JSON).</param>
        public void SwitchFormat(MessageFormat format)
        {
            if (_fileSink == null)
            {
                Debug.LogWarning("[LogSmith] Cannot switch format: File sink is not enabled");
                return;
            }

            _fileSink.CurrentFormat = format;
            Debug.Log($"[LogSmith] File sink format switched to: {format}");
        }

        /// <summary>
        /// Converts a relative log file path to an absolute path using Application.persistentDataPath.
        /// </summary>
        private string GetFullLogPath(string relativePath)
        {
            // If path is already absolute, use it as-is
            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            // Otherwise, make it relative to persistent data path
            var dataPath = Application.persistentDataPath;
            return Path.Combine(dataPath, relativePath);
        }

        /// <summary>
        /// Flushes all sinks to ensure pending writes are committed.
        /// </summary>
        public void Flush()
        {
            _consoleSink?.Flush();
            _fileSink?.Flush();
        }

        /// <summary>
        /// Disposes of the bootstrap and all registered sinks.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Flush();

            // Unregister and dispose sinks
            if (_consoleSink != null)
            {
                _router.UnregisterSink(_consoleSink);
                _consoleSink = null;
            }

            if (_fileSink != null)
            {
                _router.UnregisterSink(_fileSink);
                _fileSink.Dispose();
                _fileSink = null;
            }

            Debug.Log("[LogSmith] UnityLoggingBootstrap disposed");
        }
    }
}
