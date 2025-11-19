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
        private readonly IPlatformCapabilities _platformCapabilities;

        private ConsoleSink _consoleSink;
        private FileSink _fileSink;
        private DebugOverlayController _debugOverlay;
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
        /// <param name="platformCapabilities">Optional platform capabilities service for feature detection.</param>
        public UnityLoggingBootstrap(LoggingSettings settings, ILogRouter router,
            IMessageTemplateEngine templateEngine = null, IPlatformCapabilities platformCapabilities = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _templateEngine = templateEngine ?? new MessageTemplateEngine();
            _platformCapabilities = platformCapabilities ?? new PlatformCapabilities();

            Initialize();
        }

        /// <summary>
        /// Initializes the logging system by configuring and registering sinks.
        /// </summary>
        private void Initialize()
        {
            // Configure and register console sink
            if (_settings.enableConsoleSink)
            {
                _consoleSink = new ConsoleSink(_templateEngine);
                _router.RegisterSink(_consoleSink);
            }

            // Configure and register file sink with rotation support
            if (_settings.enableFileSink)
            {
                // Check if platform supports file I/O
                if (!_platformCapabilities.HasWritablePersistentDataPath)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning($"[LogSmith] File sink is not supported on {_platformCapabilities.PlatformName}. " +
                                   "File logging will be disabled. Supported platforms: Windows, macOS, Linux, iOS, Android, PlayStation, Xbox.");
#endif
                }
                else
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
            }

            // Set global minimum log level
            _router.SetGlobalMinimumLevel(_settings.minimumLogLevel);

            // Apply per-category minimum level overrides
            ApplyCategoryMinLevelOverrides();

            // Enable live reload if configured
            _liveReloadEnabled = _settings.enableLiveReload;

            // Initialize debug overlay if enabled
            if (_settings.enableDebugOverlay)
            {
                InitializeDebugOverlay();
            }

            var fileStatus = _settings.enableFileSink
                ? (_fileSink != null ? "Enabled" : $"Disabled (unsupported on {_platformCapabilities.PlatformName})")
                : "Disabled";
            var overlayStatus = _settings.enableDebugOverlay ? "Enabled (F1 to toggle)" : "Disabled";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[LogSmith] UnityLoggingBootstrap initialized - Console: {_settings.enableConsoleSink}, File: {fileStatus}, Overlay: {overlayStatus}, Rotation: {_settings.enableLogRotation}, Format: {_settings.defaultFormatMode}");
#endif
        }

        /// <summary>
        /// Reloads the logging configuration from settings.
        /// This allows runtime changes to take effect without restarting.
        /// </summary>
        public void ReloadSettings()
        {
            if (!_liveReloadEnabled)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[LogSmith] Live reload is disabled in settings");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[LogSmith] Reloading logging settings...");
#endif

            // Update global minimum log level
            _router.SetGlobalMinimumLevel(_settings.minimumLogLevel);

            // Apply per-category minimum level overrides
            ApplyCategoryMinLevelOverrides();

            // Update file sink format if enabled
            if (_fileSink != null)
            {
                _fileSink.CurrentFormat = _settings.defaultFormatMode == MessageFormatMode.Text
                    ? MessageFormat.Text
                    : MessageFormat.Json;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[LogSmith] Settings reloaded - MinLevel: {_settings.minimumLogLevel}, Format: {_settings.defaultFormatMode}");
#endif
        }

        /// <summary>
        /// Switches the file sink output format at runtime.
        /// </summary>
        /// <param name="format">The desired message format (Text or JSON).</param>
        public void SwitchFormat(MessageFormat format)
        {
            if (_fileSink == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[LogSmith] Cannot switch format: File sink is not enabled");
#endif
                return;
            }

            _fileSink.CurrentFormat = format;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[LogSmith] File sink format switched to: {format}");
#endif
        }

        /// <summary>
        /// Applies per-category minimum level overrides from settings to the router.
        /// </summary>
        private void ApplyCategoryMinLevelOverrides()
        {
            if (_settings.categoryMinLevelOverrides == null || _settings.categoryMinLevelOverrides.Count == 0)
            {
                return;
            }

            foreach (var categoryOverride in _settings.categoryMinLevelOverrides)
            {
                if (!string.IsNullOrEmpty(categoryOverride.categoryName))
                {
                    _router.SetCategoryFilter(categoryOverride.categoryName, categoryOverride.minimumLevel);
                }
            }
        }

        /// <summary>
        /// Initializes the debug overlay for in-game log viewing.
        /// </summary>
        private void InitializeDebugOverlay()
        {
            var overlayObject = new GameObject("[LogSmith] DebugOverlay");
            _debugOverlay = overlayObject.AddComponent<DebugOverlayController>();
            _debugOverlay.Initialize(_router);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[LogSmith] Debug Overlay initialized - Press F1 to toggle");
#endif
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

            if (_debugOverlay != null)
            {
                UnityEngine.Object.Destroy(_debugOverlay.gameObject);
                _debugOverlay = null;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[LogSmith] UnityLoggingBootstrap disposed");
#endif
        }
    }
}
