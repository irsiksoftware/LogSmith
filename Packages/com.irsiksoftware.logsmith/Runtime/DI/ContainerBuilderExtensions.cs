using VContainer;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.DI
{
    /// <summary>
    /// Extension methods for IContainerBuilder to simplify LogSmith registration with VContainer.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers all LogSmith logging services with the container.
        /// This is the primary method for VContainer integration.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="settings">Optional logging settings. If null, default settings will be used.</param>
        /// <returns>The container builder for method chaining.</returns>
        /// <example>
        /// <code>
        /// public class GameLifetimeScope : LifetimeScope
        /// {
        ///     protected override void Configure(IContainerBuilder builder)
        ///     {
        ///         builder.RegisterLogSmith();
        ///         // ... other registrations
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IContainerBuilder RegisterLogSmith(this IContainerBuilder builder, LoggingSettings settings = null)
        {
            // Use default settings if none provided
            var config = settings ?? LoggingSettings.CreateDefault();

            // Register settings instance
            builder.RegisterInstance(config);

            // Register core services as singletons
            builder.Register<ICategoryRegistry, CategoryRegistry>(Lifetime.Singleton);
            builder.Register<IMessageTemplateEngine, MessageTemplateEngine>(Lifetime.Singleton);
            builder.Register<IPlatformCapabilities, PlatformCapabilities>(Lifetime.Singleton);

            // Register LogRouter with dependency on ICategoryRegistry
            builder.Register<ILogRouter>(container =>
            {
                var categoryRegistry = container.Resolve<ICategoryRegistry>();
                return new LogRouter(categoryRegistry);
            }, Lifetime.Singleton);

            // Register UnityLoggingBootstrap to handle initialization
            builder.Register<UnityLoggingBootstrap>(container =>
            {
                var resolvedSettings = container.Resolve<LoggingSettings>();
                var router = container.Resolve<ILogRouter>();
                var templateEngine = container.Resolve<IMessageTemplateEngine>();
                var platformCapabilities = container.Resolve<IPlatformCapabilities>();
                return new UnityLoggingBootstrap(resolvedSettings, router, templateEngine, platformCapabilities);
            }, Lifetime.Singleton);

            // Register ILog factory - creates a default logger
            builder.Register<ILog>(container =>
            {
                var router = container.Resolve<ILogRouter>();
                return new LogSmithLogger(router, "Default");
            }, Lifetime.Singleton);

            // Apply settings after container is built using UnityLoggingBootstrap
            builder.RegisterBuildCallback(container =>
            {
                // Resolve UnityLoggingBootstrap to trigger initialization
                container.Resolve<UnityLoggingBootstrap>();

                // Category registry setup
                var categoryRegistry = container.Resolve<ICategoryRegistry>();
                categoryRegistry.SetMinimumLevel("Default", config.minimumLogLevel);
            });

            return builder;
        }
    }
}
