using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System.IO;
using System;

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

        [Test]
        public void Router_CanSubscribeToLogEvents()
        {
            // Arrange
            LogSmith.Initialize();
            var router = LogSmith.Router;
            var messageReceived = false;
            LogMessage receivedMessage = default;

            // Act
            using (var subscription = router.Subscribe(msg =>
            {
                messageReceived = true;
                receivedMessage = msg;
            }))
            {
                LogSmith.Logger.Info("Test subscription");
            }

            // Assert
            Assert.IsTrue(messageReceived);
            Assert.AreEqual("Test subscription", receivedMessage.Message);
        }

        [Test]
        public void CategoryRegistry_CanRegisterAndUnregisterCategories()
        {
            // Arrange
            var registry = new CategoryRegistry();

            // Act
            registry.RegisterCategory("TestCategory", LogLevel.Debug);

            // Assert
            Assert.IsTrue(registry.HasCategory("TestCategory"));
            Assert.AreEqual(LogLevel.Debug, registry.GetMinimumLevel("TestCategory"));

            // Act - Unregister
            registry.UnregisterCategory("TestCategory");

            // Assert
            Assert.IsFalse(registry.HasCategory("TestCategory"));
        }

        [Test]
        public void CategoryRegistry_CanRenameCategory()
        {
            // Arrange
            var registry = new CategoryRegistry();
            registry.RegisterCategory("OldName", LogLevel.Warn);

            // Act
            registry.RenameCategory("OldName", "NewName");

            // Assert
            Assert.IsFalse(registry.HasCategory("OldName"));
            Assert.IsTrue(registry.HasCategory("NewName"));
            Assert.AreEqual(LogLevel.Warn, registry.GetMinimumLevel("NewName"));
        }

        [Test]
        public void CategoryRegistry_CanSetAndGetMinimumLevel()
        {
            // Arrange
            var registry = new CategoryRegistry();
            registry.RegisterCategory("TestCategory", LogLevel.Info);

            // Act
            registry.SetMinimumLevel("TestCategory", LogLevel.Error);

            // Assert
            Assert.AreEqual(LogLevel.Error, registry.GetMinimumLevel("TestCategory"));
        }

        [Test]
        public void CategoryRegistry_ReturnsAllCategories()
        {
            // Arrange
            var registry = new CategoryRegistry();
            registry.RegisterCategory("Category1", LogLevel.Info);
            registry.RegisterCategory("Category2", LogLevel.Debug);
            registry.RegisterCategory("Category3", LogLevel.Warn);

            // Act
            var categories = registry.GetCategories();

            // Assert
            Assert.AreEqual(3, categories.Count);
            Assert.IsTrue(categories.Contains("Category1"));
            Assert.IsTrue(categories.Contains("Category2"));
            Assert.IsTrue(categories.Contains("Category3"));
        }

        [Test]
        public void FileSink_CanWriteToFile()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), $"logsmith_test_{Guid.NewGuid()}.log");
            var fileSink = new FileSink(testFilePath);

            try
            {
                var message = new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "TestCategory",
                    Message = "Test message",
                    Timestamp = DateTime.UtcNow
                };

                // Act
                fileSink.Write(message);
                fileSink.Flush();

                // Assert
                Assert.IsTrue(File.Exists(testFilePath));
                var content = File.ReadAllText(testFilePath);
                Assert.IsTrue(content.Contains("Test message"));
                Assert.IsTrue(content.Contains("Info"));
                Assert.IsTrue(content.Contains("TestCategory"));
            }
            finally
            {
                // Cleanup
                fileSink.Dispose();
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Test]
        public void FileSink_CanBeDisposed()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), $"logsmith_test_{Guid.NewGuid()}.log");
            var fileSink = new FileSink(testFilePath);

            try
            {
                // Act & Assert (should not throw)
                Assert.DoesNotThrow(() => fileSink.Dispose());
                Assert.DoesNotThrow(() => fileSink.Dispose()); // Double dispose should be safe
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
        }

        [Test]
        public void LogConfigProvider_ReturnsDefaultConfig()
        {
            // Arrange
            var provider = new LogConfigProvider();

            // Act
            var config = provider.GetConfig();

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(LogLevel.Info, config.DefaultMinimumLevel);
            Assert.IsTrue(config.EnableConsoleSink);
            Assert.IsTrue(config.EnableFileSink);
        }

        [Test]
        public void LogConfigProvider_CanSubscribeToConfigChanges()
        {
            // Arrange
            var provider = new LogConfigProvider();
            var notificationReceived = false;
            LogConfig receivedConfig = null;

            // Act
            using (var subscription = provider.Subscribe(config =>
            {
                notificationReceived = true;
                receivedConfig = config;
            }))
            {
                // Subscription should immediately notify with current config
            }

            // Assert
            Assert.IsTrue(notificationReceived);
            Assert.IsNotNull(receivedConfig);
        }

        [Test]
        public void LogConfigProvider_NotifiesSubscribersOnReload()
        {
            // Arrange
            var provider = new LogConfigProvider();
            var notificationCount = 0;

            using (var subscription = provider.Subscribe(config => notificationCount++))
            {
                // First notification on subscribe
                Assert.AreEqual(1, notificationCount);

                // Act
                provider.ReloadConfig();

                // Assert
                Assert.AreEqual(2, notificationCount);
            }
        }

        [Test]
        public void MessageTemplateEngine_FormatsTextCorrectly()
        {
            // Arrange
            var engine = new MessageTemplateEngine();
            var message = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "TestCategory",
                Message = "Test message",
                Timestamp = new DateTime(2025, 9, 29, 12, 30, 45)
            };

            // Act
            var formatted = engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.IsTrue(formatted.Contains("12:30:45"));
            Assert.IsTrue(formatted.Contains("Info"));
            Assert.IsTrue(formatted.Contains("TestCategory"));
            Assert.IsTrue(formatted.Contains("Test message"));
        }

        [Test]
        public void MessageTemplateEngine_FormatsJsonCorrectly()
        {
            // Arrange
            var engine = new MessageTemplateEngine();
            var message = new LogMessage
            {
                Level = LogLevel.Error,
                Category = "ErrorCategory",
                Message = "Error message",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var formatted = engine.Format(message, MessageFormat.Json);

            // Assert
            Assert.IsTrue(formatted.Contains("\"level\":\"Error\""));
            Assert.IsTrue(formatted.Contains("\"category\":\"ErrorCategory\""));
            Assert.IsTrue(formatted.Contains("\"message\":\"Error message\""));
            Assert.IsTrue(formatted.StartsWith("{"));
            Assert.IsTrue(formatted.EndsWith("}"));
        }

        [Test]
        public void MessageTemplateEngine_CanSetCategoryTemplate()
        {
            // Arrange
            var engine = new MessageTemplateEngine();
            var customTemplate = "[{Level}] {Message}";

            // Act
            engine.SetCategoryTemplate("CustomCategory", customTemplate);
            var retrievedTemplate = engine.GetCategoryTemplate("CustomCategory");

            // Assert
            Assert.AreEqual(customTemplate, retrievedTemplate);
        }

        [Test]
        public void MessageTemplateEngine_EscapesJsonSpecialCharacters()
        {
            // Arrange
            var engine = new MessageTemplateEngine();
            var message = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Message with \"quotes\" and \nnewlines",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var formatted = engine.Format(message, MessageFormat.Json);

            // Assert
            Assert.IsTrue(formatted.Contains("\\\"quotes\\\""));
            Assert.IsTrue(formatted.Contains("\\n"));
        }

        private class TestSink : ILogSink
        {
            public string Name => "TestSink";
            public void Write(LogMessage message) { }
            public void Flush() { }
        }
    }
}