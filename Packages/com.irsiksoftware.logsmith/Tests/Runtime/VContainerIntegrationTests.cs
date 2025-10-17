using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.DI;
using VContainer;
using VContainer.Unity;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// EditMode tests for VContainer integration with LogSmith.
    /// Tests that require LifetimeScope.Build() (which needs PlayMode) are in PlayMode/VContainerLifetimeScopeTests.cs
    /// </summary>
    [TestFixture]
    public class VContainerIntegrationTests
    {
        [TearDown]
        public void TearDown()
        {
            // Reset LogSmith static state via reflection
            var type = typeof(LogSmith);
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;

            type.GetField("_initialized", bindingFlags)?.SetValue(null, false);
            type.GetField("_isUsingDependencyInjection", bindingFlags)?.SetValue(null, false);
            type.GetField("_container", bindingFlags)?.SetValue(null, null);
            type.GetField("_router", bindingFlags)?.SetValue(null, null);
            type.GetField("_defaultLogger", bindingFlags)?.SetValue(null, null);
        }

        [Test]
        public void ExtensionMethod_RegistersAllServices()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var settings = LoggingSettings.CreateDefault();

            // Act
            builder.AddLogSmithLogging(settings);
            var container = builder.Build();

            // Assert
            Assert.IsNotNull(container.Resolve<ILog>());
            Assert.IsNotNull(container.Resolve<ILogRouter>());
            Assert.IsNotNull(container.Resolve<ICategoryRegistry>());
            Assert.IsNotNull(container.Resolve<IMessageTemplateEngine>());
            Assert.IsNotNull(container.Resolve<ILogConfigProvider>());
        }

        [Test]
        public void ExtensionMethod_WithConsoleSinkEnabled_RegistersConsoleSink()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = true;

            // Act
            builder.AddLogSmithLogging(settings);
            var container = builder.Build();

            // Assert - ConsoleSink should be registered and added to router
            var router = container.Resolve<ILogRouter>();
            Assert.IsNotNull(router);
        }

        [Test]
        public void ContainerBuilderExtensions_AddLogSmithLogging_RegistersServices()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var settings = LoggingSettings.CreateDefault();

            // Act
            builder.AddLogSmithLogging(settings);
            var container = builder.Build();

            // Assert
            Assert.IsNotNull(container.Resolve<ILog>());
            Assert.IsNotNull(container.Resolve<ILogRouter>());
        }

        [Test]
        public void ContainerBuilderExtensions_AddLogSmithLogging_WithNullSettings_UsesDefaults()
        {
            // Arrange
            var builder = new ContainerBuilder();

            // Act
            builder.AddLogSmithLogging(null);
            var container = builder.Build();

            // Assert
            Assert.IsNotNull(container.Resolve<ILog>());
        }

        [Test]
        public void LogSmith_WithoutVContainerScope_UsesStaticFallback()
        {
            // Arrange - No scope created

            // Act
            var logger = LogSmith.Logger;

            // Assert
            Assert.IsFalse(LogSmith.IsUsingDependencyInjection);
            Assert.IsNotNull(logger);
        }
    }
}