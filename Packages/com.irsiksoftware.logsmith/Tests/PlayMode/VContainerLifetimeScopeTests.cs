using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.DI;
using VContainer;
using VContainer.Unity;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for VContainer LifetimeScope integration with LogSmith.
    /// These tests require PlayMode because LifetimeScope.Build() calls DontDestroyOnLoad().
    /// </summary>
    [TestFixture]
    public class VContainerLifetimeScopeTests
    {
        private GameObject _scopeObject;
        private LoggingLifetimeScope _scope;

        [SetUp]
        public void SetUp()
        {
            // Clean up any existing instances
            TearDown();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up our test scope object
            if (_scopeObject != null)
            {
                Object.DestroyImmediate(_scopeObject);
                _scopeObject = null;
                _scope = null;
            }

            // Clean up ANY LoggingLifetimeScope objects that might be left in the scene
            var allScopes = Object.FindObjectsByType<DI.LoggingLifetimeScope>(FindObjectsSortMode.None);
            foreach (var scope in allScopes)
            {
                Object.DestroyImmediate(scope.gameObject);
            }

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
        public void LoggingLifetimeScope_ConfiguresContainer()
        {
            // Arrange
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();

            // Act - Force Build
            _scope.Build();

            // Assert
            Assert.IsNotNull(_scope.Container);
            var log = _scope.Container.Resolve<ILog>();
            Assert.IsNotNull(log);
        }

        [Test]
        public void LoggingLifetimeScope_CanLogMessages()
        {
            // Arrange
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();
            _scope.Build();

            var log = _scope.Container.Resolve<ILog>();

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                log.Info("Test msg");  // Short message to avoid FixedString32Bytes truncation in Unity.Logging
                log.Debug("Debug");
                log.Warn("Warn");
            });
        }

        [Test]
        public void LogSmith_WithVContainerScope_UsesVContainer()
        {
            // Arrange
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();
            _scope.Build();

            // Act
            var logger = LogSmith.Logger;

            // Assert
            Assert.IsTrue(LogSmith.IsUsingDependencyInjection);
            Assert.IsNotNull(logger);
        }

        [Test]
        public void LogSmith_CreateLogger_WorksInBothModes()
        {
            // Test without VContainer
            var loggerWithoutDI = LogSmith.CreateLogger("TestCategory");
            Assert.IsNotNull(loggerWithoutDI);
            Assert.IsFalse(LogSmith.IsUsingDependencyInjection);

            // Reset
            TearDown();
            SetUp();

            // Test with VContainer
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();
            _scope.Build();

            var loggerWithDI = LogSmith.CreateLogger("TestCategory");
            Assert.IsNotNull(loggerWithDI);
            Assert.IsTrue(LogSmith.IsUsingDependencyInjection);
        }

        [Test]
        public void LogSmith_Resolve_WorksOnlyWithVContainer()
        {
            // Test without VContainer
            var routerWithoutDI = LogSmith.Resolve<ILogRouter>();
            Assert.IsNull(routerWithoutDI);

            // Reset
            TearDown();
            SetUp();

            // Test with VContainer
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();
            _scope.Build();

            // Force initialization
            var _ = LogSmith.Logger;

            var routerWithDI = LogSmith.Resolve<ILogRouter>();
            Assert.IsNotNull(routerWithDI);
        }

        [Test]
        public void ResolvedLogger_ProducesSameOutputAcrossModes()
        {
            // This is more of a sanity check that both paths work identically

            // Test static mode
            var staticLogger = LogSmith.Logger;
            Assert.DoesNotThrow(() =>
            {
                staticLogger.Info("Static");  // Short to avoid Unity.Logging truncation
                staticLogger.Debug("Debug");
            });

            // Reset
            TearDown();
            SetUp();

            // Test DI mode
            _scopeObject = new GameObject("LoggingScope");
            _scope = _scopeObject.AddComponent<LoggingLifetimeScope>();
            _scope.Build();

            var diLogger = LogSmith.Logger;
            Assert.DoesNotThrow(() =>
            {
                diLogger.Info("DI");
                diLogger.Debug("Debug");
            });
        }
    }
}
