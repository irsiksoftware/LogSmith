using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Matrix tests for per-category minimum level filtering.
    /// Tests all combinations of global min level, category min level, and message level.
    /// </summary>
    [TestFixture]
    public class PerCategoryMinLevelMatrixTests
    {
        private LogRouter _router;
        private CategoryRegistry _categoryRegistry;
        private TestLogSink _testSink;

        [SetUp]
        public void Setup()
        {
            _categoryRegistry = new CategoryRegistry();
            _router = new LogRouter(_categoryRegistry);
            _testSink = new TestLogSink();
            _router.RegisterSink(_testSink);
        }

        /// <summary>
        /// Tests the matrix of global minimum level vs message level for unregistered categories.
        /// </summary>
        [Test]
        public void GlobalMinLevel_Matrix_FiltersCorrectly(
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel globalMin,
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel messageLevel)
        {
            // Arrange
            _router.SetGlobalMinimumLevel(globalMin);

            var message = new LogMessage
            {
                Category = "UnregisteredCategory",
                Level = messageLevel,
                Message = $"Global:{globalMin}, Message:{messageLevel}",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            bool shouldPass = messageLevel >= globalMin;
            int expectedCount = shouldPass ? 1 : 0;

            Assert.AreEqual(expectedCount, _testSink.Messages.Count,
                $"Global min={globalMin}, Message level={messageLevel} should {(shouldPass ? "PASS" : "SUPPRESS")}");
        }

        /// <summary>
        /// Tests the matrix of category minimum level vs message level.
        /// Category minimum should take precedence over global minimum.
        /// </summary>
        [Test]
        public void CategoryMinLevel_Matrix_FiltersCorrectly(
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel categoryMin,
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel messageLevel)
        {
            // Arrange
            const string category = "TestCategory";
            _router.SetGlobalMinimumLevel(LogLevel.Trace); // Set global to lowest to test category filter
            _categoryRegistry.RegisterCategory(category, categoryMin);

            var message = new LogMessage
            {
                Category = category,
                Level = messageLevel,
                Message = $"Category:{categoryMin}, Message:{messageLevel}",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            bool shouldPass = messageLevel >= categoryMin;
            int expectedCount = shouldPass ? 1 : 0;

            Assert.AreEqual(expectedCount, _testSink.Messages.Count,
                $"Category min={categoryMin}, Message level={messageLevel} should {(shouldPass ? "PASS" : "SUPPRESS")}");
        }

        /// <summary>
        /// Tests the matrix of router category filter vs message level.
        /// Router filter should take precedence over category registry minimum.
        /// </summary>
        [Test]
        public void RouterCategoryFilter_Matrix_FiltersCorrectly(
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel routerFilterMin,
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel messageLevel)
        {
            // Arrange
            const string category = "TestCategory";
            _router.SetGlobalMinimumLevel(LogLevel.Trace);
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace); // Set registry to lowest
            _router.SetCategoryFilter(category, routerFilterMin); // Router filter overrides

            var message = new LogMessage
            {
                Category = category,
                Level = messageLevel,
                Message = $"RouterFilter:{routerFilterMin}, Message:{messageLevel}",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            bool shouldPass = messageLevel >= routerFilterMin;
            int expectedCount = shouldPass ? 1 : 0;

            Assert.AreEqual(expectedCount, _testSink.Messages.Count,
                $"Router filter min={routerFilterMin}, Message level={messageLevel} should {(shouldPass ? "PASS" : "SUPPRESS")}");
        }

        /// <summary>
        /// Tests precedence: disabled category blocks all messages regardless of level.
        /// </summary>
        [Test]
        public void DisabledCategory_Matrix_BlocksAllLevels(
            [Values(LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Critical)] LogLevel messageLevel)
        {
            // Arrange
            const string category = "DisabledCategory";
            _categoryRegistry.RegisterCategory(category, LogLevel.Trace);
            _categoryRegistry.SetEnabled(category, false);

            var message = new LogMessage
            {
                Category = category,
                Level = messageLevel,
                Message = $"Disabled category, Message:{messageLevel}",
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(0, _testSink.Messages.Count,
                $"Disabled category should block all messages, even {messageLevel}");
        }

        /// <summary>
        /// Tests the full precedence chain: Enabled > Router Filter > Category Registry > Global.
        /// </summary>
        [Test]
        public void FullPrecedenceChain_RouterFilterOverridesRegistry()
        {
            // Arrange
            const string category = "TestCategory";
            _router.SetGlobalMinimumLevel(LogLevel.Trace);
            _categoryRegistry.RegisterCategory(category, LogLevel.Debug); // Registry says Debug+
            _router.SetCategoryFilter(category, LogLevel.Error); // Router overrides to Error+

            var debugMessage = new LogMessage { Category = category, Level = LogLevel.Debug, Message = "Debug", Timestamp = System.DateTime.UtcNow };
            var infoMessage = new LogMessage { Category = category, Level = LogLevel.Info, Message = "Info", Timestamp = System.DateTime.UtcNow };
            var warnMessage = new LogMessage { Category = category, Level = LogLevel.Warn, Message = "Warn", Timestamp = System.DateTime.UtcNow };
            var errorMessage = new LogMessage { Category = category, Level = LogLevel.Error, Message = "Error", Timestamp = System.DateTime.UtcNow };

            // Act
            _router.Route(debugMessage);
            _router.Route(infoMessage);
            _router.Route(warnMessage);
            _router.Route(errorMessage);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count, "Only Error should pass (router filter overrides registry)");
            Assert.AreEqual("Error", _testSink.Messages[0].Message);
        }

        /// <summary>
        /// Tests the full precedence chain: When no router filter is set, category registry minimum applies.
        /// </summary>
        [Test]
        public void FullPrecedenceChain_RegistryMinimumWhenNoRouterFilter()
        {
            // Arrange
            const string category = "TestCategory";
            _router.SetGlobalMinimumLevel(LogLevel.Trace);
            _categoryRegistry.RegisterCategory(category, LogLevel.Warn); // Registry says Warn+

            var debugMessage = new LogMessage { Category = category, Level = LogLevel.Debug, Message = "Debug", Timestamp = System.DateTime.UtcNow };
            var infoMessage = new LogMessage { Category = category, Level = LogLevel.Info, Message = "Info", Timestamp = System.DateTime.UtcNow };
            var warnMessage = new LogMessage { Category = category, Level = LogLevel.Warn, Message = "Warn", Timestamp = System.DateTime.UtcNow };
            var errorMessage = new LogMessage { Category = category, Level = LogLevel.Error, Message = "Error", Timestamp = System.DateTime.UtcNow };

            // Act
            _router.Route(debugMessage);
            _router.Route(infoMessage);
            _router.Route(warnMessage);
            _router.Route(errorMessage);

            // Assert
            Assert.AreEqual(2, _testSink.Messages.Count, "Only Warn and Error should pass (registry minimum)");
            Assert.AreEqual("Warn", _testSink.Messages[0].Message);
            Assert.AreEqual("Error", _testSink.Messages[1].Message);
        }

        /// <summary>
        /// Tests the full precedence chain: When no router filter or registry entry, global minimum applies.
        /// </summary>
        [Test]
        public void FullPrecedenceChain_GlobalMinimumWhenNoRegistryOrFilter()
        {
            // Arrange
            _router.SetGlobalMinimumLevel(LogLevel.Info);

            var debugMessage = new LogMessage { Category = "UnknownCategory", Level = LogLevel.Debug, Message = "Debug", Timestamp = System.DateTime.UtcNow };
            var infoMessage = new LogMessage { Category = "UnknownCategory", Level = LogLevel.Info, Message = "Info", Timestamp = System.DateTime.UtcNow };
            var warnMessage = new LogMessage { Category = "UnknownCategory", Level = LogLevel.Warn, Message = "Warn", Timestamp = System.DateTime.UtcNow };

            // Act
            _router.Route(debugMessage);
            _router.Route(infoMessage);
            _router.Route(warnMessage);

            // Assert
            Assert.AreEqual(2, _testSink.Messages.Count, "Only Info and Warn should pass (global minimum)");
            Assert.AreEqual("Info", _testSink.Messages[0].Message);
            Assert.AreEqual("Warn", _testSink.Messages[1].Message);
        }

        /// <summary>
        /// Tests that clearing a category filter falls back to registry minimum.
        /// </summary>
        [Test]
        public void ClearCategoryFilter_FallsBackToRegistryMinimum()
        {
            // Arrange
            const string category = "TestCategory";
            _router.SetGlobalMinimumLevel(LogLevel.Trace);
            _categoryRegistry.RegisterCategory(category, LogLevel.Info); // Registry says Info+
            _router.SetCategoryFilter(category, LogLevel.Error); // Router overrides to Error+

            // Act - First with router filter
            var warnMessage1 = new LogMessage { Category = category, Level = LogLevel.Warn, Message = "Warn1", Timestamp = System.DateTime.UtcNow };
            _router.Route(warnMessage1);

            Assert.AreEqual(0, _testSink.Messages.Count, "Warn should be blocked by Error filter");

            // Clear the filter
            _router.ClearCategoryFilter(category);

            // Route again after clearing filter
            var warnMessage2 = new LogMessage { Category = category, Level = LogLevel.Warn, Message = "Warn2", Timestamp = System.DateTime.UtcNow };
            _router.Route(warnMessage2);

            // Assert
            Assert.AreEqual(1, _testSink.Messages.Count, "Warn should now pass (falls back to registry Info minimum)");
            Assert.AreEqual("Warn2", _testSink.Messages[0].Message);
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
