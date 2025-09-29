using IrsikSoftware.LogSmith.Core;

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
        /// Initializes the logging system with default configuration.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            // Initialize Unity.Logging backend
            Adapters.NativeUnityLoggerAdapter.Initialize();

            // Create router
            _router = new LogRouter();

            // Register default console sink
            var consoleSink = new ConsoleSink();
            _router.RegisterSink(consoleSink);

            // Create default logger
            _defaultLogger = new LogSmithLogger(_router, "Default");

            _initialized = true;
        }

        /// <summary>
        /// Creates a logger for a specific category.
        /// </summary>
        public static ILog CreateLogger(string category)
        {
            EnsureInitialized();
            return _defaultLogger.WithCategory(category);
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