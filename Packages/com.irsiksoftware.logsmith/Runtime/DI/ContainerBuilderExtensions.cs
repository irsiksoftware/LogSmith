using VContainer;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.DI
{
    /// <summary>
    /// Extension methods for IContainerBuilder to simplify LogSmith registration.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers LogSmith logging services with the container using the specified settings.
        /// Use this if you want to manually register LogSmith in your own LifetimeScope instead of using LoggingLifetimeScope.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="settings">The logging settings. If null, default settings will be used.</param>
        /// <returns>The container builder for method chaining.</returns>
        public static IContainerBuilder AddLogSmithLogging(this IContainerBuilder builder, LoggingSettings settings = null)
        {
            // Use default settings if none provided
            var config = settings ?? LoggingSettings.CreateDefault();

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

            // Register ILog factory
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

                // Wire up category registry to router for enabled state checking
                if (router is LogRouter logRouter)
                {
                    logRouter.SetCategoryRegistry(categoryRegistry);
                }

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

            return builder;
        }
    }
}