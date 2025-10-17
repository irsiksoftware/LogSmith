using IrsikSoftware.LogSmith.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Entry point and composition root for LogSmith.
    /// Provides static access to the logging system with support for both VContainer DI and standalone usage.
    /// </summary>
    public static class LogSmith
    {
        private static ILogRouter _router;
        private static ILog _defaultLogger;
        private static bool _initialized;
        private static bool _isUsingDependencyInjection;
        private static IObjectResolver _container;
        private static Core.UnityLoggingBootstrap _bootstrap;

        /// <summary>
        /// Returns true if LogSmith is using VContainer for dependency injection.
        /// </summary>
        public static bool IsUsingDependencyInjection
        {
            get
            {
                EnsureInitialized();
                return _isUsingDependencyInjection;
            }
        }

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
        /// This will use VContainer if a LoggingLifetimeScope is present, otherwise falls back to static initialization.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            // Try to find VContainer scope
            if (TryInitializeWithVContainer())
            {
                _isUsingDependencyInjection = true;
                _initialized = true;
                return;
            }

            // Fallback to static initialization
            InitializeStatic();
            _isUsingDependencyInjection = false;
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
        /// Attempts to initialize using VContainer.
        /// </summary>
        private static bool TryInitializeWithVContainer()
        {
            try
            {
                // Try to find LoggingLifetimeScope in the scene
                var scope = Object.FindFirstObjectByType<DI.LoggingLifetimeScope>();
                if (scope == null)
                {
                    return false;
                }

                // Get the container from the scope
                _container = scope.Container;
                if (_container == null)
                {
                    return false;
                }

                // Resolve services from container
                _defaultLogger = _container.Resolve<ILog>();
                _router = _container.Resolve<ILogRouter>();

                return true;
            }
            catch
            {
                // If VContainer resolution fails, return false to fall back to static
                return false;
            }
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
        /// Reloads logging settings from the current LoggingSettings configuration.
        /// Only available when using static initialization.
        /// </summary>
        public static void ReloadSettings()
        {
            EnsureInitialized();

            if (_isUsingDependencyInjection)
            {
                // When using DI, resolve bootstrap from container
                var bootstrap = Resolve<Core.UnityLoggingBootstrap>();
                bootstrap?.ReloadSettings();
            }
            else
            {
                // Static mode
                _bootstrap?.ReloadSettings();
            }
        }

        /// <summary>
        /// Switches the file sink output format at runtime.
        /// </summary>
        /// <param name="format">The desired message format (Text or JSON).</param>
        public static void SwitchFormat(MessageFormat format)
        {
            EnsureInitialized();

            if (_isUsingDependencyInjection)
            {
                // When using DI, resolve bootstrap from container
                var bootstrap = Resolve<Core.UnityLoggingBootstrap>();
                bootstrap?.SwitchFormat(format);
            }
            else
            {
                // Static mode
                _bootstrap?.SwitchFormat(format);
            }
        }

        /// <summary>
        /// Resolves a service from the VContainer container if DI is being used.
        /// Returns null if not using DI or if the service cannot be resolved.
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            EnsureInitialized();

            if (!_isUsingDependencyInjection || _container == null)
            {
                return null;
            }

            try
            {
                return _container.Resolve<T>();
            }
            catch
            {
                return null;
            }
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