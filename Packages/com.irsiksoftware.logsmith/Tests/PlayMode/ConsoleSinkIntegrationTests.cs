using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode integration tests for ConsoleSink covering Unity console output capture.
    /// Part of issue #24: "PlayMode tests: console sink capture"
    /// </summary>
    [TestFixture]
    public class ConsoleSinkIntegrationTests
    {
        private MessageTemplateEngine _templateEngine;
        private List<string> _capturedLogs;

        [SetUp]
        public void Setup()
        {
            _templateEngine = new MessageTemplateEngine();
            _capturedLogs = new List<string>();
            Application.logMessageReceived += CaptureLog;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= CaptureLog;
            _capturedLogs.Clear();
        }

        private void CaptureLog(string logString, string stackTrace, LogType type)
        {
            _capturedLogs.Add($"[{type}] {logString}");
        }

        private LogMessage CreateTestMessage(string message = "Test message", LogLevel level = LogLevel.Info, string category = "Test")
        {
            // Get frame count safely - Time.frameCount can only be called from main thread
            int frameCount = -1;
            try
            {
                frameCount = Time.frameCount;
            }
            catch (UnityEngine.UnityException)
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

        #region Basic Console Output Tests

        [UnityTest]
        public IEnumerator ConsoleSink_Write_OutputsToUnityConsole()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Console test message", LogLevel.Info);
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null; // Wait one frame for Unity console processing

            // Assert
            Assert.Greater(_capturedLogs.Count, 0, "Should have captured at least one log");
            var captured = string.Join("\n", _capturedLogs);
            Assert.IsTrue(captured.Contains("Console test message"),
                $"Console should contain the message. Captured: {captured}");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_Write_InfoLevel_UsesLogType()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Info message", LogLevel.Info);
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = _capturedLogs[0];
            Assert.IsTrue(captured.StartsWith("[Log]"),
                $"Info level should use Unity LogType.Log. Captured: {captured}");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_Write_WarnLevel_UsesWarningType()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Warning message", LogLevel.Warn);
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = _capturedLogs[0];
            Assert.IsTrue(captured.StartsWith("[Warning]"),
                $"Warn level should use Unity LogType.Warning. Captured: {captured}");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_Write_ErrorLevel_UsesErrorType()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Error message", LogLevel.Error);
            _capturedLogs.Clear();

            // Expect the error log
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@".*\[Test\] Error message.*"));

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = _capturedLogs[0];
            Assert.IsTrue(captured.StartsWith("[Error]"),
                $"Error level should use Unity LogType.Error. Captured: {captured}");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_Write_CriticalLevel_UsesErrorType()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Critical message", LogLevel.Critical);
            _capturedLogs.Clear();

            // Expect the error log
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@".*\[Test\] Critical message.*"));

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = _capturedLogs[0];
            Assert.IsTrue(captured.StartsWith("[Error]"),
                $"Critical level should use Unity LogType.Error. Captured: {captured}");
        }

        #endregion

        #region Multiple Messages

        [UnityTest]
        public IEnumerator ConsoleSink_Write_MultipleMessages_AllCaptured()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            _capturedLogs.Clear();

            // Act
            for (int i = 0; i < 5; i++)
            {
                sink.Write(CreateTestMessage($"Message {i}", LogLevel.Info));
            }
            yield return null;

            // Assert
            Assert.GreaterOrEqual(_capturedLogs.Count, 5, "Should have captured at least 5 logs");

            for (int i = 0; i < 5; i++)
            {
                var captured = string.Join("\n", _capturedLogs);
                Assert.IsTrue(captured.Contains($"Message {i}"),
                    $"Should contain 'Message {i}'. Captured: {captured}");
            }
        }

        #endregion

        #region Format Tests

        [UnityTest]
        public IEnumerator ConsoleSink_TextFormat_ContainsMetadata()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            sink.CurrentFormat = MessageFormat.Text;
            var message = CreateTestMessage("Metadata test", LogLevel.Info, "TestCategory");
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = string.Join("\n", _capturedLogs);
            Assert.IsTrue(captured.Contains("Metadata test"), "Should contain message");
            Assert.IsTrue(captured.Contains("TestCategory") || captured.Contains("[Test]"),
                "Should contain category information");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_JsonFormat_OutputsValidStructure()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            sink.CurrentFormat = MessageFormat.Json;
            var message = CreateTestMessage("JSON test", LogLevel.Info, "JsonCategory");
            message.Context = new Dictionary<string, object> { { "key", "value" } };
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = string.Join("\n", _capturedLogs);

            // Should have JSON structure indicators
            Assert.IsTrue(captured.Contains("{") && captured.Contains("}"),
                "JSON format should have braces");
            Assert.IsTrue(captured.Contains("JSON test"), "Should contain message");
        }

        #endregion

        #region Context Data Tests

        [UnityTest]
        public IEnumerator ConsoleSink_WithContext_IncludesContextData()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            var message = CreateTestMessage("Context message", LogLevel.Info);
            message.Context = new Dictionary<string, object>
            {
                { "userId", "user123" },
                { "requestId", 456 }
            };
            _capturedLogs.Clear();

            // Act
            sink.Write(message);
            yield return null;

            // Assert
            Assert.Greater(_capturedLogs.Count, 0);
            var captured = string.Join("\n", _capturedLogs);

            // Context should be present in output (format may vary)
            Assert.IsTrue(captured.Contains("Context message"), "Should contain message");
        }

        #endregion

        #region Edge Cases

        [UnityTest]
        public IEnumerator ConsoleSink_Dispose_DoesNotThrow()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);

            // Act & Assert
            Assert.DoesNotThrow(() => sink.Dispose());
            yield return null;
        }

        [UnityTest]
        public IEnumerator ConsoleSink_DisposedSink_DoesNotWriteOrThrow()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);
            sink.Dispose();
            _capturedLogs.Clear();

            // Act & Assert
            Assert.DoesNotThrow(() => sink.Write(CreateTestMessage("After dispose")));
            yield return null;

            // Should not have written to console
            var captured = string.Join("\n", _capturedLogs);
            Assert.IsFalse(captured.Contains("After dispose"),
                "Disposed sink should not write to console");
        }

        [UnityTest]
        public IEnumerator ConsoleSink_Flush_DoesNotThrow()
        {
            // Arrange
            var sink = new ConsoleSink(_templateEngine);

            // Act & Assert
            Assert.DoesNotThrow(() => sink.Flush());
            yield return null;
        }

        #endregion
    }
}
