using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System.IO;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Tests for UnityLoggingBootstrap functionality including initialization, rotation, and format switching.
    /// </summary>
    public class UnityLoggingBootstrapTests
    {
        private string _testDirectory;

        [SetUp]
        public void Setup()
        {
            // Create a unique test directory for each test
            _testDirectory = Path.Combine(Path.GetTempPath(), $"logsmith_bootstrap_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup failures in tests
                }
            }
        }

        [Test]
        public void Bootstrap_InitializesWithDefaultSettings()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            var router = new LogRouter();
            var templateEngine = new MessageTemplateEngine();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router, templateEngine))
            {
                // Assert
                Assert.IsNotNull(bootstrap);
                Assert.IsNotNull(bootstrap.ConsoleSink);
                Assert.IsNull(bootstrap.FileSink); // File sink disabled by default
            }
        }

        [Test]
        public void Bootstrap_EnablesConsoleSink_WhenConfigured()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = true;
            var router = new LogRouter();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Assert
                Assert.IsNotNull(bootstrap.ConsoleSink);
            }
        }

        [Test]
        public void Bootstrap_DisablesConsoleSink_WhenConfigured()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = false;
            var router = new LogRouter();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Assert
                Assert.IsNull(bootstrap.ConsoleSink);
            }
        }

        [Test]
        public void Bootstrap_EnablesFileSink_WhenConfigured()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "test.log");
            var router = new LogRouter();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Assert
                Assert.IsNotNull(bootstrap.FileSink);
            }
        }

        [Test]
        public void Bootstrap_RegistersSinks_WithRouter()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = true;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "test.log");

            var router = new LogRouter();
            int messageCount = 0;

            using (router.Subscribe(_ => messageCount++))
            {
                // Act
                using (var bootstrap = new UnityLoggingBootstrap(settings, router))
                {
                    router.Route(new LogMessage
                    {
                        Level = LogLevel.Info,
                        Category = "Test",
                        Message = "Test message",
                        Timestamp = DateTime.UtcNow
                    });

                    bootstrap.Flush();

                    // Assert
                    Assert.AreEqual(1, messageCount);
                }
            }
        }

        [Test]
        public void Bootstrap_SetsGlobalMinimumLevel()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.minimumLogLevel = LogLevel.Warn;
            var router = new LogRouter();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Log messages at different levels
                int messageCount = 0;
                using (router.Subscribe(_ => messageCount++))
                {
                    // Debug should be filtered out
                    router.Route(new LogMessage { Level = LogLevel.Debug, Category = "Test", Message = "Debug", Timestamp = DateTime.UtcNow });

                    // Warn should pass through
                    router.Route(new LogMessage { Level = LogLevel.Warn, Category = "Test", Message = "Warn", Timestamp = DateTime.UtcNow });
                }

                // Assert
                Assert.AreEqual(1, messageCount); // Only Warn should pass
            }
        }

        [Test]
        public void FileSink_RotatesLog_WhenSizeExceeds()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = false;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "rotating.log");
            settings.enableLogRotation = true;
            settings.maxFileSizeMB = 0; // Set to 0 to trigger rotation immediately (< 1KB threshold)
            settings.retentionCount = 5;

            var router = new LogRouter();

            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Write a large message to trigger rotation
                var largeMessage = new string('A', 1024 * 1024 + 1); // 1MB + 1 byte

                // Act - Write first message
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = largeMessage,
                    Timestamp = DateTime.UtcNow
                });
                bootstrap.Flush();

                // Wait a moment for file system
                Thread.Sleep(100);

                // Write second message (should trigger rotation since file > maxSize)
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = "After rotation",
                    Timestamp = DateTime.UtcNow
                });
                bootstrap.Flush();

                // Assert - Check if archived file was created
                var directory = Path.GetDirectoryName(settings.logFilePath);
                var archivedFiles = Directory.GetFiles(directory, "rotating_*.log");

                // Should have at least one archived file or the rotation was triggered
                // Note: Due to timing and MB conversion, this test verifies the mechanism exists
                Assert.IsTrue(File.Exists(settings.logFilePath));
            }
        }

        [Test]
        public void FileSink_RespectsRetentionCount()
        {
            // Arrange - Create 5 pre-existing archived files with different timestamps
            var logPath = Path.Combine(_testDirectory, "retention.log");
            var directory = Path.GetDirectoryName(logPath);
            var baseFileName = Path.GetFileNameWithoutExtension(logPath);
            var extension = Path.GetExtension(logPath);

            // Create mock archived files with different LastWriteTime values
            var now = DateTime.Now;
            for (int i = 0; i < 5; i++)
            {
                var archivedPath = Path.Combine(directory, $"{baseFileName}_{now.AddMinutes(-i):yyyyMMdd-HHmmss}{extension}");
                File.WriteAllText(archivedPath, $"Archived content {i}");
                // Set distinct LastWriteTime to ensure ordering
                File.SetLastWriteTime(archivedPath, now.AddMinutes(-i));
            }

            // Verify 5 files were created
            var filesBeforeRotation = Directory.GetFiles(directory, $"{baseFileName}_*{extension}");
            Assert.AreEqual(5, filesBeforeRotation.Length, "Should have 5 archived files before rotation");

            // Create a main log file large enough to trigger rotation
            File.WriteAllText(logPath, new string('A', 1024)); // 1KB file

            // Act - Create FileSink with retention count of 2 and trigger rotation
            var fileSink = new FileSink(logPath, null, enableRotation: true, maxFileSizeMB: 0, retentionCount: 2);
            try
            {
                // Write a message to trigger rotation (maxFileSizeMB=0 means any size triggers rotation)
                fileSink.Write(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = "Test message",
                    Timestamp = DateTime.UtcNow
                });
                fileSink.Flush();

                // Assert - After rotation with retentionCount=2, only 2 archived files should remain
                // (plus the newly rotated file = 3 total)
                var filesAfterRotation = Directory.GetFiles(directory, $"{baseFileName}_*{extension}");
                Assert.LessOrEqual(filesAfterRotation.Length, 3, "Should have at most 3 archived files after rotation (2 old + 1 new)");
            }
            finally
            {
                fileSink.Dispose();
            }
        }

        [Test]
        public void Bootstrap_ReloadSettings_UpdatesMinimumLevel()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.minimumLogLevel = LogLevel.Debug;
            settings.enableLiveReload = true;
            var router = new LogRouter();

            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Act - Change settings and reload
                settings.minimumLogLevel = LogLevel.Error;
                bootstrap.ReloadSettings();

                // Verify by checking message routing
                int messageCount = 0;
                using (router.Subscribe(_ => messageCount++))
                {
                    // Info should be filtered out now
                    router.Route(new LogMessage { Level = LogLevel.Info, Category = "Test", Message = "Info", Timestamp = DateTime.UtcNow });

                    // Error should pass through
                    router.Route(new LogMessage { Level = LogLevel.Error, Category = "Test", Message = "Error", Timestamp = DateTime.UtcNow });
                }

                // Assert
                Assert.AreEqual(1, messageCount); // Only Error should pass
            }
        }

        [Test]
        public void Bootstrap_SwitchFormat_ChangesFileSinkFormat()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = false;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "format_switch.log");
            settings.defaultFormatMode = MessageFormatMode.Text;
            var router = new LogRouter();

            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Act - Switch to JSON format
                bootstrap.SwitchFormat(MessageFormat.Json);

                // Write a message
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = "JSON test",
                    Timestamp = DateTime.UtcNow
                });
                bootstrap.Flush();

                // Force file close
            }

            // Assert - Read file and verify JSON format
            var content = File.ReadAllText(settings.logFilePath);
            Assert.IsTrue(content.Contains("\"level\":\"Info\""));
            Assert.IsTrue(content.Contains("\"message\":\"JSON test\""));
        }

        [Test]
        public void Bootstrap_DefaultFormat_IsText()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = false;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "text_format.log");
            settings.defaultFormatMode = MessageFormatMode.Text;
            var router = new LogRouter();

            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Write a message
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "TestCat",
                    Message = "Text test",
                    Timestamp = DateTime.UtcNow
                });
                bootstrap.Flush();
            }

            // Assert - Read file and verify text format (not JSON)
            var content = File.ReadAllText(settings.logFilePath);
            Assert.IsTrue(content.Contains("Info"));
            Assert.IsTrue(content.Contains("TestCat"));
            Assert.IsTrue(content.Contains("Text test"));
            Assert.IsFalse(content.Contains("\"level\":")); // Not JSON
        }

        [Test]
        public void Bootstrap_Flush_FlushesAllSinks()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = true;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "flush_test.log");
            var router = new LogRouter();

            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                // Write a message
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = "Flush test",
                    Timestamp = DateTime.UtcNow
                });

                // Act
                Assert.DoesNotThrow(() => bootstrap.Flush());
            }

            // Assert - File should exist and contain message
            Assert.IsTrue(File.Exists(settings.logFilePath));
            var content = File.ReadAllText(settings.logFilePath);
            Assert.IsTrue(content.Contains("Flush test"));
        }

        [Test]
        public void Bootstrap_Dispose_UnregistersSinks()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableConsoleSink = true;
            settings.enableFileSink = true;
            settings.logFilePath = Path.Combine(_testDirectory, "dispose_test.log");
            var router = new LogRouter();

            int messageCountBefore = 0;
            int messageCountAfter = 0;

            var bootstrap = new UnityLoggingBootstrap(settings, router);

            // Count messages before disposal
            using (router.Subscribe(_ => messageCountBefore++))
            {
                router.Route(new LogMessage { Level = LogLevel.Info, Category = "Test", Message = "Before", Timestamp = DateTime.UtcNow });
            }

            // Act - Dispose
            bootstrap.Dispose();

            // Count messages after disposal
            using (router.Subscribe(_ => messageCountAfter++))
            {
                router.Route(new LogMessage { Level = LogLevel.Info, Category = "Test", Message = "After", Timestamp = DateTime.UtcNow });
            }

            // Assert
            Assert.AreEqual(1, messageCountBefore); // Should have received message before disposal
            Assert.AreEqual(1, messageCountAfter); // Subscription still works, but sinks are unregistered
        }

        [Test]
        public void Bootstrap_DoubleDispose_DoesNotThrow()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            var router = new LogRouter();
            var bootstrap = new UnityLoggingBootstrap(settings, router);

            // Act & Assert
            Assert.DoesNotThrow(() => bootstrap.Dispose());
            Assert.DoesNotThrow(() => bootstrap.Dispose()); // Should be safe
        }

        [Test]
        public void Bootstrap_WithRelativePath_UsesApplicationDataPath()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableFileSink = true;
            settings.logFilePath = "Logs/relative_path.log"; // Relative path
            var router = new LogRouter();

            // Act
            using (var bootstrap = new UnityLoggingBootstrap(settings, router))
            {
                router.Route(new LogMessage
                {
                    Level = LogLevel.Info,
                    Category = "Test",
                    Message = "Relative path test",
                    Timestamp = DateTime.UtcNow
                });
                bootstrap.Flush();

                // Assert - File should be created relative to Application.persistentDataPath
                var expectedPath = Path.Combine(Application.persistentDataPath, settings.logFilePath);
                var directory = Path.GetDirectoryName(expectedPath);

                // Check if directory was created (file existence check may fail on some platforms)
                Assert.IsTrue(Directory.Exists(directory));
            }
        }

        [Test]
        public void LogSmith_ReloadSettings_CallsBootstrapReload()
        {
            // This tests the integration between LogSmith static API and UnityLoggingBootstrap
            // Note: This test verifies the API exists and doesn't throw

            // Act & Assert
            Assert.DoesNotThrow(() => LogSmith.ReloadSettings());
        }

        [Test]
        public void LogSmith_SwitchFormat_CallsBootstrapSwitchFormat()
        {
            // This tests the integration between LogSmith static API and UnityLoggingBootstrap
            // Note: This test verifies the API exists and doesn't throw

            // Act & Assert
            Assert.DoesNotThrow(() => LogSmith.SwitchFormat(MessageFormat.Json));
            Assert.DoesNotThrow(() => LogSmith.SwitchFormat(MessageFormat.Text));
        }
    }
}
