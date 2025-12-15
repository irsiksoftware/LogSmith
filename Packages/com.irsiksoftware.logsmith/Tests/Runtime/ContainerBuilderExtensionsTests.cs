using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using IrsikSoftware.LogSmith.DI;
using VContainer;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Tests for VContainer IContainerBuilder extension methods.
    /// </summary>
    public class ContainerBuilderExtensionsTests
    {
        [Test]
        public void RegisterLogSmith_ReturnsBuilder_ForMethodChaining()
        {
            // Arrange
            var builder = new ContainerBuilder();

            // Act
            var result = builder.RegisterLogSmith();

            // Assert
            Assert.AreSame(builder, result);
        }

        [Test]
        public void RegisterLogSmith_RegistersILogRouter()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var router = container.Resolve<ILogRouter>();

                // Assert
                Assert.IsNotNull(router);
            }
        }

        [Test]
        public void RegisterLogSmith_RegistersICategoryRegistry()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var registry = container.Resolve<ICategoryRegistry>();

                // Assert
                Assert.IsNotNull(registry);
            }
        }

        [Test]
        public void RegisterLogSmith_RegistersIMessageTemplateEngine()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var engine = container.Resolve<IMessageTemplateEngine>();

                // Assert
                Assert.IsNotNull(engine);
            }
        }

        [Test]
        public void RegisterLogSmith_RegistersILog()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var log = container.Resolve<ILog>();

                // Assert
                Assert.IsNotNull(log);
            }
        }

        [Test]
        public void RegisterLogSmith_RegistersServicesAsSingletons()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var router1 = container.Resolve<ILogRouter>();
                var router2 = container.Resolve<ILogRouter>();

                // Assert
                Assert.AreSame(router1, router2);
            }
        }

        [Test]
        public void RegisterLogSmith_WithSettings_UsesProvidedSettings()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.minimumLogLevel = LogLevel.Warn;
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith(settings);

            // Act
            using (var container = builder.Build())
            {
                var resolvedSettings = container.Resolve<LoggingSettings>();

                // Assert
                Assert.AreSame(settings, resolvedSettings);
                Assert.AreEqual(LogLevel.Warn, resolvedSettings.minimumLogLevel);
            }
        }

        [Test]
        public void RegisterLogSmith_WithoutSettings_UsesDefaultSettings()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var settings = container.Resolve<LoggingSettings>();

                // Assert
                Assert.IsNotNull(settings);
            }
        }

        [Test]
        public void RegisterLogSmith_ILog_CanLogMessages()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var log = container.Resolve<ILog>();

                // Assert - should not throw
                Assert.DoesNotThrow(() => log.Info("Test message"));
            }
        }

        [Test]
        public void RegisterLogSmith_ILog_WithCategory_CreatesNewLogger()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterLogSmith();

            // Act
            using (var container = builder.Build())
            {
                var log = container.Resolve<ILog>();
                var categoryLog = log.WithCategory("TestCategory");

                // Assert
                Assert.IsNotNull(categoryLog);
                Assert.AreNotSame(log, categoryLog);
            }
        }
    }
}
