using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Tests
{
    [TestFixture]
    public class LogRouterCategoryIntegrationTests
    {
        private CategoryRegistry _categoryRegistry;
        private LogRouter _router;
        private TestLogSink _testSink;

        [SetUp]
        public void Setup()
        {
            _categoryRegistry = new CategoryRegistry();
            _router = new LogRouter(_categoryRegistry);
            _testSink = new TestLogSink();
            _router.RegisterSink(_testSink);
        }

        [Test]
        public void Route_DisabledCategory_FiltersOutMessage()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);
            _categoryRegistry.SetEnabled(category, false);

            var message = new LogMessage
            {
                Category = category,
                Level = LogLevel.Critical,
                Message = "Test message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(0, _testSink.Messages.Count, "Disabled category should filter out all messages");
        }

        [Test]
        public void Route_EnabledCategory_AllowsMessage()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);
            _categoryRegistry.SetEnabled(category, true);

            var message = new LogMessage
            {
                Category = category,
                Level = LogLevel.Info,
                Message = "Test message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count);
            Assert.AreEqual("Test message", _testSink.Messages[0].Message);
        }

        [Test]
        public void Route_RespectsRegistryMinimumLevel()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Warn);

            var infoMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Info,
                Message = "Info message",
                Timestamp = System.DateTime.UtcNow
            };

            var warnMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Warn,
                Message = "Warn message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(infoMessage);
            _router.Route(warnMessage);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count, "Only warn message should pass");
            Assert.AreEqual("Warn message", _testSink.Messages[0].Message);
        }

        [Test]
        public void Route_CategoryFilterTakesPrecedenceOverRegistry()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Debug);
            _router.SetCategoryFilter(category, LogLevel.Error); // Router filter overrides registry

            var debugMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Debug,
                Message = "Debug message",
                Timestamp = System.DateTime.UtcNow
            };

            var errorMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Error,
                Message = "Error message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(debugMessage);
            _router.Route(errorMessage);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count, "Router filter should take precedence");
            Assert.AreEqual("Error message", _testSink.Messages[0].Message);
        }

        [Test]
        public void Route_UnregisteredCategory_UsesGlobalMinimum()
        {
            // Arrange
            _router.SetGlobalMinimumLevel(LogLevel.Warn);

            var infoMessage = new LogMessage
            {
                Category = "UnregisteredCategory",
                Level = LogLevel.Info,
                Message = "Info message",
                Timestamp = System.DateTime.UtcNow
            };

            var warnMessage = new LogMessage
            {
                Category = "UnregisteredCategory",
                Level = LogLevel.Warn,
                Message = "Warn message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(infoMessage);
            _router.Route(warnMessage);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count);
            Assert.AreEqual("Warn message", _testSink.Messages[0].Message);
        }

        [Test]
        public void Route_DynamicallyDisableCategory_FiltersSubsequentMessages()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);
            _categoryRegistry.SetEnabled(category, true);

            var firstMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Info,
                Message = "First message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act - Route with category enabled
            _router.Route(firstMessage);

            // Disable category
            _categoryRegistry.SetEnabled(category, false);

            var secondMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Info,
                Message = "Second message",
                Timestamp = System.DateTime.UtcNow
            };

            _router.Route(secondMessage);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count, "Only first message should be routed");
            Assert.AreEqual("First message", _testSink.Messages[0].Message);
        }

        [Test]
        public void Route_DynamicallyChangeMinimumLevel_FiltersAccordingly()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);

            var debugMessage1 = new LogMessage
            {
                Category = category,
                Level = LogLevel.Debug,
                Message = "Debug 1",
                Timestamp = System.DateTime.UtcNow
            };

            // Act - Route with Trace minimum
            _router.Route(debugMessage1);

            // Change minimum level to Warn
            _categoryRegistry.SetMinimumLevel(category, LogLevel.Warn);

            var debugMessage2 = new LogMessage
            {
                Category = category,
                Level = LogLevel.Debug,
                Message = "Debug 2",
                Timestamp = System.DateTime.UtcNow
            };

            var warnMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Warn,
                Message = "Warn 1",
                Timestamp = System.DateTime.UtcNow
            };

            _router.Route(debugMessage2);
            _router.Route(warnMessage);

            // Assert
            Assert.AreEqual(2, _testSink.Messages.Count, "Debug 1 and Warn 1 should pass");
            Assert.AreEqual("Debug 1", _testSink.Messages[0].Message);
            Assert.AreEqual("Warn 1", _testSink.Messages[1].Message);
        }

        [Test]
        public void Route_EnabledStateCheckHappensFirmst_BeforeMinimumLevel()
        {
            // Arrange
            const string category = "TestCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);
            _categoryRegistry.SetEnabled(category, false);

            var criticalMessage = new LogMessage
            {
                Category = category,
                Level = LogLevel.Critical, // Highest level
                Message = "Critical message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(criticalMessage);

            // Assert
            Assert.AreEqual(0, _testSink.Messages.Count, "Disabled category should block even Critical messages");
        }

        [Test]
        public void Route_WithoutCategoryRegistry_WorksNormally()
        {
            // Arrange - Create router without category registry
            var routerWithoutRegistry = new LogRouter(null);
            var testSink = new TestLogSink();
            routerWithoutRegistry.RegisterSink(testSink);
            routerWithoutRegistry.SetGlobalMinimumLevel(LogLevel.Info);

            var message = new LogMessage
            {
                Category = "SomeCategory",
                Level = LogLevel.Warn,
                Message = "Test message",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            routerWithoutRegistry.Route(message);

            // Assert
            Assert.AreEqual(1, testSink.Messages.Count);
            Assert.AreEqual("Test message", testSink.Messages[0].Message);
        }

        // Helper test sink for capturing messages
        private class TestLogSink : ILogSink
        {
            public string Name => "TestSink";
            public List<LogMessage> Messages { get; } = new List<LogMessage>();

            public void Write(LogMessage message)
            {
                Messages.Add(message);
            }

            public void Flush()
            {
                // No-op for test
            }
        }
    }
}
