using UnityEngine;
using VContainer;
using VContainer.Unity;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.DI
{
    /// <summary>
    /// VContainer LifetimeScope for LogSmith logging services.
    /// Attach this to a GameObject (typically in a prefab) to enable dependency injection for logging.
    /// This provides a single scope for the entire application - not per-scene.
    /// </summary>
    public class LoggingLifetimeScope : LifetimeScope
    {
        [Header("Logging Configuration")]
        [Tooltip("Settings for the logging system. If null, default settings will be used.")]
        [SerializeField] private LoggingSettings settings;

        protected override void Configure(IContainerBuilder builder)
        {
            // Use default settings if none provided
            var config = settings != null ? settings : LoggingSettings.CreateDefault();

            // Initialize Unity backend adapter
            Adapters.NativeUnityLoggerAdapter.Initialize();

            // Register core services as singletons
            builder.Register<ILogRouter, LogRouter>(Lifetime.Singleton);
            builder.Register<ICategoryRegistry, CategoryRegistry>(Lifetime.Singleton);
            builder.Register<IMessageTemplateEngine, MessageTemplateEngine>(Lifetime.Singleton);
            builder.Register<ILogConfigProvider, LogConfigProvider>(Lifetime.Singleton);

            // Register sinks based on settings
            if (config.enableConsoleSink)
            {
                builder.Register<ConsoleSink>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            }

            if (config.enableFileSink)
            {
                builder.Register<FileSink>(Lifetime.Singleton)
                    .WithParameter("filePath", config.logFilePath)
                    .AsSelf()
                    .AsImplementedInterfaces();
            }

            // Register ILog factory - creates loggers with the "Default" category
            // Users can call ILog.WithCategory() to create category-specific loggers
            builder.Register<ILog>(container =>
            {
                var router = container.Resolve<ILogRouter>();
                return new LogSmithLogger(router, "Default");
            }, Lifetime.Singleton);

            // Apply settings after container is built
            builder.RegisterBuildCallback(container =>
            {
                var router = container.Resolve<ILogRouter>();
                var categoryRegistry = container.Resolve<ICategoryRegistry>();

                // Set minimum log level
                categoryRegistry.SetMinimumLevel("Default", config.minimumLogLevel);

                // Register sinks with router
                if (config.enableConsoleSink)
                {
                    var consoleSink = container.Resolve<ConsoleSink>();
                    router.RegisterSink(consoleSink);
                }

                if (config.enableFileSink)
                {
                    var fileSink = container.Resolve<FileSink>();
                    router.RegisterSink(fileSink);
                }
            });
        }

        protected override void Awake()
        {
            // Ensure this scope persists across scenes since it's the root logging scope
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            base.Awake();
        }
    }
}