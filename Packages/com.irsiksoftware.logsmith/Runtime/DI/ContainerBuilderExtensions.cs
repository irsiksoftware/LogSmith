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

            // Register settings instance
            builder.RegisterInstance(config);

            // Register core services as singletons
            builder.Register<ILogRouter, LogRouter>(Lifetime.Singleton);
            builder.Register<ICategoryRegistry, CategoryRegistry>(Lifetime.Singleton);
            builder.Register<IMessageTemplateEngine, MessageTemplateEngine>(Lifetime.Singleton);
            builder.Register<ILogConfigProvider, LogConfigProvider>(Lifetime.Singleton);

            // Register UnityLoggingBootstrap to handle initialization
            builder.Register<UnityLoggingBootstrap>(Lifetime.Singleton);

            // Register ILog factory
            builder.Register<ILog>(container =>
            {
                var router = container.Resolve<ILogRouter>();
                return new LogSmithLogger(router, "Default");
            }, Lifetime.Singleton);

            // Apply settings after container is built using UnityLoggingBootstrap
            builder.RegisterBuildCallback(container =>
            {
                // UnityLoggingBootstrap will be created here and initialize the system
                var bootstrap = container.Resolve<UnityLoggingBootstrap>();

                // Category registry setup
                var categoryRegistry = container.Resolve<ICategoryRegistry>();
                categoryRegistry.SetMinimumLevel("Default", config.minimumLogLevel);
            });

            return builder;
        }
    }
}