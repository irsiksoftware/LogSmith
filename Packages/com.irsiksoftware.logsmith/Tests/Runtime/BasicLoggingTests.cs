using NUnit.Framework;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Basic tests to verify core logging functionality.
    /// </summary>
    public class BasicLoggingTests
    {
        [Test]
        public void LogSmith_Initializes_Successfully()
        {
            // Act
            LogSmith.Initialize();
            var logger = LogSmith.Logger;

            // Assert
            Assert.IsNotNull(logger);
        }

        [Test]
        public void Logger_CanLog_AllLevels()
        {
            // Arrange
            LogSmith.Initialize();
            var logger = LogSmith.Logger;

            // Act & Assert (should not throw)
            Assert.DoesNotThrow(() => logger.Trace("Trace message"));
            Assert.DoesNotThrow(() => logger.Debug("Debug message"));
            Assert.DoesNotThrow(() => logger.Info("Info message"));
            Assert.DoesNotThrow(() => logger.Warn("Warning message"));
            Assert.DoesNotThrow(() => logger.Error("Error message"));
            Assert.DoesNotThrow(() => logger.Critical("Critical message"));
        }

        [Test]
        public void Logger_CanCreateCategoryLogger()
        {
            // Arrange
            LogSmith.Initialize();
            var logger = LogSmith.CreateLogger("TestCategory");

            // Assert
            Assert.IsNotNull(logger);
        }

        [Test]
        public void Logger_WithCategory_CreatesNewLogger()
        {
            // Arrange
            LogSmith.Initialize();
            var logger = LogSmith.Logger;

            // Act
            var categoryLogger = logger.WithCategory("CustomCategory");

            // Assert
            Assert.IsNotNull(categoryLogger);
            Assert.AreNotSame(logger, categoryLogger);
        }

        [Test]
        public void Router_CanRegisterAndUnregisterSinks()
        {
            // Arrange
            LogSmith.Initialize();
            var router = LogSmith.Router;
            var testSink = new TestSink();

            // Act
            Assert.DoesNotThrow(() => router.RegisterSink(testSink));
            Assert.DoesNotThrow(() => router.UnregisterSink(testSink));
        }

        private class TestSink : ILogSink
        {
            public string Name => "TestSink";
            public void Write(LogMessage message) { }
            public void Flush() { }
        }
    }
}