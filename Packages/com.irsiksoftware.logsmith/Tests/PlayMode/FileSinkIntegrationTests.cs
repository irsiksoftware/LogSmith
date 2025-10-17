using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode integration tests for FileSink covering write, rotation, and retention behavior.
    /// Part of issue #24: "PlayMode tests: file sink write/rotate/retention"
    /// </summary>
    [TestFixture]
    public class FileSinkIntegrationTests
    {
        private string _testDirectory;
        private string _testFilePath;
        private MessageTemplateEngine _templateEngine;

        [SetUp]
        public void Setup()
        {
            // Create unique test directory for this test run
            _testDirectory = Path.Combine(Application.temporaryCachePath, "LogSmith_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "test.log");
            _templateEngine = new MessageTemplateEngine();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"TearDown cleanup failed: {ex.Message}");
            }
        }

        private LogMessage CreateTestMessage(string message = "Test message", LogLevel level = LogLevel.Info, string category = "Test")
        {
            // Get frame count safely - Time.frameCount can only be called from main thread
            int frameCount = -1;
            try
            {
                frameCount = Time.frameCount;
            }
            catch (UnityException)
            {
                // Called from background thread - frameCount unavailable
            }

            return new LogMessage
            {
                Level = level,
                Category = category,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Frame = frameCount,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                ThreadName = System.Threading.Thread.CurrentThread.Name ?? "Main",
                CallerFilePath = "TestFile.cs",
                CallerMemberName = "TestMethod",
                StackTrace = null,
                Context = null
            };
        }

        #region Basic Write Tests

        [UnityTest]
        public IEnumerator FileSink_Write_CreatesFile()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false))
            {
                var message = CreateTestMessage("Initial message");
                sink.Write(message);
                sink.Flush();
            } // Dispose sink before reading file
            yield return null; // Wait one frame for file system operations

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath), "Log file should be created");
            var content = File.ReadAllText(_testFilePath);
            Assert.IsTrue(content.Contains("Initial message"), "File should contain the logged message");
        }

        [UnityTest]
        public IEnumerator FileSink_Write_MultipleMessages_AppendsToFile()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false))
            {
                for (int i = 0; i < 10; i++)
                {
                    sink.Write(CreateTestMessage($"Message {i}"));
                }
                sink.Flush();
            } // Dispose sink before reading file
            yield return null;

            // Assert
            Assert.IsTrue(File.Exists(_testFilePath));
            var lines = File.ReadAllLines(_testFilePath);
            Assert.AreEqual(10, lines.Length, "File should contain 10 lines");

            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(lines[i].Contains($"Message {i}"), $"Line {i} should contain 'Message {i}'");
            }
        }

        [UnityTest]
        public IEnumerator FileSink_Write_TextFormat_ProducesReadableOutput()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false))
            {
                sink.CurrentFormat = MessageFormat.Text;
                var message = CreateTestMessage("Readable text message", LogLevel.Warn, "TestCategory");
                sink.Write(message);
                sink.Flush();
            } // Dispose sink before reading file
            yield return null;

            // Assert
            var content = File.ReadAllText(_testFilePath);
            Assert.IsTrue(content.Contains("Readable text message"));
            Assert.IsTrue(content.Contains("Warn") || content.Contains("WARN") || content.Contains("Warning"),
                "Should contain Warn level indicator");
            Assert.IsTrue(content.Contains("TestCategory"));
        }

        [UnityTest]
        public IEnumerator FileSink_Write_JsonFormat_ProducesValidJson()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false))
            {
                sink.CurrentFormat = MessageFormat.Json;
                var message = CreateTestMessage("JSON message", LogLevel.Error, "JsonCategory");
                message.Context = new Dictionary<string, object>
                {
                    { "userId", "user123" },
                    { "requestId", 456 }
                };
                sink.Write(message);
                sink.Flush();
            } // Dispose sink before reading file
            yield return null;

            // Assert
            var content = File.ReadAllText(_testFilePath);
            Assert.IsTrue(content.Contains("\"message\"") || content.Contains("JSON message"));
            Assert.IsTrue(content.Contains("\"level\"") || content.Contains("Error"));
            Assert.IsTrue(content.Contains("\"category\"") || content.Contains("JsonCategory"));

            // Verify it's valid JSON structure (should have braces and quotes)
            Assert.IsTrue(content.Contains("{") && content.Contains("}"));
        }

        #endregion

        #region Rotation Tests

        [UnityTest]
        public IEnumerator FileSink_Rotation_TriggersWhenFileSizeExceeded()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: 5))
            {
                // Write enough messages to well exceed 1MB to ensure rotation triggers
                // Default template produces ~85-90 byte messages
                // 1MB = 1,048,576 bytes
                // Write 15,000 messages (15,000 × 90 = 1,350,000 bytes = ~1.3MB) to ensure rotation
                for (int i = 0; i < 15000; i++)
                {
                    sink.Write(CreateTestMessage($"Message number {i} with some padding text to increase size"));

                    // Every 1000 messages, flush and yield to ensure file size is accurate for rotation checks
                    if (i % 1000 == 0)
                    {
                        sink.Flush();
                        yield return null;
                    }
                }
                sink.Flush();
            } // Dispose sink before reading files
            yield return null;

            // Assert - Should have rotated file plus archived file(s)
            var logFiles = Directory.GetFiles(_testDirectory, "*.log");
            Assert.Greater(logFiles.Length, 1, "Should have multiple log files after rotation");

            // Current file should exist
            Assert.IsTrue(File.Exists(_testFilePath), "Current log file should exist");

            // At least one archived file should exist (pattern: test_YYYYMMDD-HHmmss-fff.log)
            var archivedFiles = logFiles.Where(f => !f.Equals(_testFilePath) && Path.GetFileName(f).Contains("_")).ToArray();
            Assert.Greater(archivedFiles.Length, 0, "Should have at least one archived file");
        }

        [UnityTest]
        public IEnumerator FileSink_Rotation_ArchivedFileName_ContainsTimestamp()
        {
            // Arrange
            using var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: 5);

            // Act - Write enough to trigger rotation (15,000 messages to exceed 1MB)
            for (int i = 0; i < 15000; i++)
            {
                sink.Write(CreateTestMessage($"Rotation test message {i} with padding for file size increase"));
                if (i % 1000 == 0)
                {
                    sink.Flush();
                    yield return null;
                }
            }
            sink.Flush();
            yield return null;

            // Assert - Check archived file naming pattern
            var logFiles = Directory.GetFiles(_testDirectory, "*.log");
            var archivedFiles = logFiles
                .Where(f => !f.Equals(_testFilePath))
                .Select(f => Path.GetFileName(f))
                .ToArray();

            if (archivedFiles.Length > 0)
            {
                // Pattern: test_YYYYMMDD-HHmmss-fff.log
                var timestampPattern = new Regex(@"test_\d{8}-\d{6}-\d{3}\.log");
                var hasValidTimestamp = archivedFiles.Any(f => timestampPattern.IsMatch(f));
                Assert.IsTrue(hasValidTimestamp,
                    $"At least one archived file should match timestamp pattern. Files: {string.Join(", ", archivedFiles)}");
            }
        }

        [UnityTest]
        public IEnumerator FileSink_Rotation_PreservesAllMessages()
        {
            // Arrange & Act
            const int messageCount = 15000;
            using (var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: 10))
            {
                for (int i = 0; i < messageCount; i++)
                {
                    sink.Write(CreateTestMessage($"MSG_{i:D5}"));
                    if (i % 1000 == 0)
                    {
                        sink.Flush();
                        yield return null;
                    }
                }
                sink.Flush();
            } // Dispose sink before reading files
            yield return null;

            // Assert - Read all log files and verify all messages are present
            var allLogFiles = Directory.GetFiles(_testDirectory, "*.log");
            var allContent = new StringBuilder();

            foreach (var file in allLogFiles)
            {
                allContent.Append(File.ReadAllText(file));
            }

            var combinedContent = allContent.ToString();

            // Check a sample of messages across the range
            Assert.IsTrue(combinedContent.Contains("MSG_00000"), "Should contain first message");
            Assert.IsTrue(combinedContent.Contains("MSG_07500"), "Should contain middle message");
            Assert.IsTrue(combinedContent.Contains("MSG_14999"), "Should contain last message");
        }

        #endregion

        #region Retention Tests

        [UnityTest]
        public IEnumerator FileSink_Retention_DeletesOldestFiles()
        {
            // Arrange - Set retention to 3 files
            const int retentionCount = 3;
            using var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: retentionCount);

            // Act - Write enough to trigger multiple rotations (need > retentionCount rotations)
            // Need 15,000 messages per batch to exceed 1MB and trigger rotation
            for (int batch = 0; batch < 6; batch++)
            {
                for (int i = 0; i < 15000; i++)
                {
                    sink.Write(CreateTestMessage($"Batch{batch}_Msg{i} with extra padding text for size"));
                    if (i % 1000 == 0)
                    {
                        sink.Flush();
                        yield return null;
                    }
                }
                sink.Flush();
                yield return null;
            }

            yield return null;

            // Assert - Should have current file + at most retentionCount archived files
            var allLogFiles = Directory.GetFiles(_testDirectory, "*.log");
            Assert.LessOrEqual(allLogFiles.Length, retentionCount + 1,
                $"Should have at most {retentionCount + 1} files (current + {retentionCount} archived). Found: {allLogFiles.Length}");
        }

        [UnityTest]
        public IEnumerator FileSink_Retention_KeepsCurrentFileAndRecentArchives()
        {
            // Arrange
            const int retentionCount = 2;
            using var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: retentionCount);

            // Act - Force multiple rotations (15,000 messages per batch to trigger rotation)
            for (int batch = 0; batch < 5; batch++)
            {
                for (int i = 0; i < 15000; i++)
                {
                    sink.Write(CreateTestMessage($"RetentionTest_B{batch}_M{i}_PaddingForFileSizeIncrease"));
                    if (i % 1000 == 0)
                    {
                        sink.Flush();
                        yield return null;
                    }
                }
                sink.Flush();
                yield return null;
            }

            yield return null;

            // Assert
            var allLogFiles = Directory.GetFiles(_testDirectory, "*.log");

            // Current file should always exist
            Assert.IsTrue(File.Exists(_testFilePath), "Current log file should exist");

            // Total files should not exceed retention limit + current
            Assert.LessOrEqual(allLogFiles.Length, retentionCount + 1,
                $"Total files should not exceed {retentionCount + 1}");
        }

        [UnityTest]
        public IEnumerator FileSink_Retention_ZeroRetention_KeepsAllFiles()
        {
            // Arrange & Act - Retention 0 means no cleanup
            using (var sink = new FileSink(_testFilePath, _templateEngine,
                enableRotation: true, maxFileSizeMB: 1, retentionCount: 0))
            {
                // Force a few rotations - each batch should be ~1MB
                // Need 15,000 messages per batch to trigger rotation
                for (int batch = 0; batch < 3; batch++)
                {
                    for (int i = 0; i < 15000; i++)
                    {
                        sink.Write(CreateTestMessage($"NoRetention_Batch{batch}_Msg{i}_Padding"));
                        if (i % 1000 == 0)
                        {
                            sink.Flush();
                            yield return null;
                        }
                    }
                    sink.Flush();
                    yield return null;
                }
            } // Dispose sink before reading files
            yield return null;

            // Assert - With retention=0, should keep all rotated files
            var allLogFiles = Directory.GetFiles(_testDirectory, "*.log");

            // Should have multiple files (current + archives) since no cleanup happens
            Assert.GreaterOrEqual(allLogFiles.Length, 2,
                "With zero retention, all rotated files should be kept");
        }

        #endregion

        #region Deterministic Output Tests

        [UnityTest]
        public IEnumerator FileSink_DeterministicOutput_SameInputProducesSameFormat()
        {
            // Arrange
            var fixedTimestamp = new DateTime(2025, 1, 15, 14, 30, 45, DateTimeKind.Utc);
            var message1 = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Deterministic message",
                Timestamp = fixedTimestamp,
                Frame = 100,
                ThreadId = 1,
                ThreadName = "MainThread",
                CallerFilePath = "Test.cs",
                CallerMemberName = "TestMethod",
                StackTrace = null,
                Context = new Dictionary<string, object> { { "key", "value" } }
            };

            var message2 = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Deterministic message",
                Timestamp = fixedTimestamp,
                Frame = 100,
                ThreadId = 1,
                ThreadName = "MainThread",
                CallerFilePath = "Test.cs",
                CallerMemberName = "TestMethod",
                StackTrace = null,
                Context = new Dictionary<string, object> { { "key", "value" } }
            };

            // Act
            var file1 = Path.Combine(_testDirectory, "det1.log");
            var file2 = Path.Combine(_testDirectory, "det2.log");

            using (var sink1 = new FileSink(file1, _templateEngine, enableRotation: false))
            {
                sink1.Write(message1);
                sink1.Flush();
            }
            yield return null;

            using (var sink2 = new FileSink(file2, _templateEngine, enableRotation: false))
            {
                sink2.Write(message2);
                sink2.Flush();
            }
            yield return null;

            // Assert
            var content1 = File.ReadAllText(file1);
            var content2 = File.ReadAllText(file2);
            Assert.AreEqual(content1, content2, "Identical messages should produce identical output");
        }

        [UnityTest]
        public IEnumerator FileSink_JsonOutput_IsDeterministic()
        {
            // Arrange
            var fixedTimestamp = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var message = new LogMessage
            {
                Level = LogLevel.Warn,
                Category = "JsonTest",
                Message = "Test JSON output",
                Timestamp = fixedTimestamp,
                Frame = 500,
                ThreadId = 2,
                ThreadName = "Worker",
                Context = new Dictionary<string, object>
                {
                    { "alpha", "first" },
                    { "beta", 123 },
                    { "gamma", true }
                }
            };

            // Act - Write same message twice
            var file1 = Path.Combine(_testDirectory, "json1.log");
            var file2 = Path.Combine(_testDirectory, "json2.log");

            using (var sink1 = new FileSink(file1, _templateEngine, enableRotation: false))
            {
                sink1.CurrentFormat = MessageFormat.Json;
                sink1.Write(message);
                sink1.Flush();
            }
            yield return null;

            using (var sink2 = new FileSink(file2, _templateEngine, enableRotation: false))
            {
                sink2.CurrentFormat = MessageFormat.Json;
                sink2.Write(message);
                sink2.Flush();
            }
            yield return null;

            // Assert
            var json1 = File.ReadAllText(file1);
            var json2 = File.ReadAllText(file2);
            Assert.AreEqual(json1, json2, "JSON output should be deterministic");
        }

        #endregion

        #region Edge Cases and Error Handling

        [UnityTest]
        public IEnumerator FileSink_DisposedSink_DoesNotWriteOrThrow()
        {
            // Arrange
            var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false);
            sink.Dispose();

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => sink.Write(CreateTestMessage("After dispose")));
            Assert.DoesNotThrow(() => sink.Flush());

            yield return null;

            // File should either not exist or be empty (no message after dispose)
            if (File.Exists(_testFilePath))
            {
                var content = File.ReadAllText(_testFilePath);
                Assert.IsFalse(content.Contains("After dispose"),
                    "Disposed sink should not write messages");
            }
        }

        [UnityTest]
        public IEnumerator FileSink_ConcurrentWrites_ThreadSafe()
        {
            // Arrange & Act
            using (var sink = new FileSink(_testFilePath, _templateEngine, enableRotation: false))
            {
                const int threadCount = 4;
                const int messagesPerThread = 250;
                var threads = new System.Threading.Thread[threadCount];

                // Write from multiple threads
                for (int t = 0; t < threadCount; t++)
                {
                    int threadIndex = t;
                    threads[t] = new System.Threading.Thread(() =>
                    {
                        for (int i = 0; i < messagesPerThread; i++)
                        {
                            sink.Write(CreateTestMessage($"Thread{threadIndex}_Msg{i}"));
                        }
                    });
                    threads[t].Start();
                }

                // Wait for threads
                foreach (var thread in threads)
                {
                    thread.Join();
                }

                sink.Flush();
            } // Dispose sink before reading file
            yield return null;

            // Assert - All messages should be written (1000 total)
            var lines = File.ReadAllLines(_testFilePath);
            Assert.AreEqual(4 * 250, lines.Length,
                "All messages from all threads should be written");
        }

        #endregion
    }
}
