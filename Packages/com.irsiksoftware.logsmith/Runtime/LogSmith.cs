using IrsikSoftware.LogSmith.Core;
using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Entry point and composition root for LogSmith.
    /// Provides static access to the logging system.
    /// </summary>
    public static class LogSmith
    {
        private static ILogRouter _router;
        private static ILog _defaultLogger;
        private static bool _initialized;
        private static Core.UnityLoggingBootstrap _bootstrap;

        /// <summary>
        /// Gets the default logger instance.
        /// </summary>
        public static ILog Logger
        {
            get
            {
                EnsureInitialized();
                return _defaultLogger;
            }
        }

        /// <summary>
        /// Gets the log router for advanced configuration.
        /// </summary>
        public static ILogRouter Router
        {
            get
            {
                EnsureInitialized();
                return _router;
            }
        }

        /// <summary>
        /// Gets the log router for editor tools and diagnostics.
        /// Returns null if not initialized.
        /// </summary>
        public static ILogRouter GetRouter()
        {
            return _initialized ? _router : null;
        }

        /// <summary>
        /// Initializes the logging system with default configuration.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            InitializeStatic();
            _initialized = true;
        }

        /// <summary>
        /// Initializes the logging system using static composition (no DI).
        /// </summary>
        private static void InitializeStatic()
        {
            // Create category registry
            var categoryRegistry = new Core.CategoryRegistry();

            // Create router with category registry
            _router = new LogRouter(categoryRegistry);
            var templateEngine = new Core.MessageTemplateEngine();

            // Use default settings
            var settings = LoggingSettings.CreateDefault();

            // Bootstrap the system using UnityLoggingBootstrap
            _bootstrap = new Core.UnityLoggingBootstrap(settings, _router, templateEngine);

            // Create default logger
            _defaultLogger = new LogSmithLogger(_router, "Default");
        }

        /// <summary>
        /// Creates a logger for a specific category.
        /// </summary>
        public static ILog CreateLogger(string category)
        {
            EnsureInitialized();
            return _defaultLogger.WithCategory(category);
        }

        /// <summary>
        /// Gets a logger for a specific category.
        /// </summary>
        public static ILog GetLogger(string category)
        {
            return CreateLogger(category);
        }

        /// <summary>
        /// Reloads logging settings from the current LoggingSettings configuration.
        /// </summary>
        public static void ReloadSettings()
        {
            EnsureInitialized();
            _bootstrap?.ReloadSettings();
        }

        /// <summary>
        /// Switches the file sink output format at runtime.
        /// </summary>
        /// <param name="format">The desired message format (Text or JSON).</param>
        public static void SwitchFormat(MessageFormat format)
        {
            EnsureInitialized();
            _bootstrap?.SwitchFormat(format);
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                Initialize();
            }
        }
    }
}
