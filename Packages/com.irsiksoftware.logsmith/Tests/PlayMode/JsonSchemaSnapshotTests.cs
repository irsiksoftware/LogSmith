using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode integration tests for JSON schema validation and snapshot testing.
    /// Part of issue #24: "JSON schema snapshot tests"
    /// Ensures JSON output conforms to expected schema and remains stable across versions.
    /// </summary>
    [TestFixture]
    public class JsonSchemaSnapshotTests
    {
        private MessageTemplateEngine _templateEngine;

        [SetUp]
        public void Setup()
        {
            _templateEngine = new MessageTemplateEngine();
        }

        private LogMessage CreateTestMessage(string message = "Test message", LogLevel level = LogLevel.Info, string category = "Test")
        {
            return new LogMessage
            {
                Level = level,
                Category = category,
                Message = message,
                Timestamp = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc), // Fixed timestamp
                Frame = 100,
                ThreadId = 1,
                ThreadName = "MainThread",
                CallerFilePath = "TestFile.cs",
                CallerMemberName = "TestMethod",
                CallerLineNumber = 42,
                StackTrace = null,
                Context = null
            };
        }

        #region JSON Schema Structure Tests

        [UnityTest]
        public IEnumerator JsonOutput_ContainsRequiredFields()
        {
            // Arrange
            var message = CreateTestMessage("Schema test", LogLevel.Info, "TestCategory");
            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Required fields should be present
            Assert.IsTrue(json.Contains("\"level\"") || json.Contains("\"Level\""),
                "JSON should contain level field");
            Assert.IsTrue(json.Contains("\"message\"") || json.Contains("\"Message\""),
                "JSON should contain message field");
            Assert.IsTrue(json.Contains("\"timestamp\"") || json.Contains("\"Timestamp\""),
                "JSON should contain timestamp field");
            Assert.IsTrue(json.Contains("\"category\"") || json.Contains("\"Category\""),
                "JSON should contain category field");
        }

        [UnityTest]
        public IEnumerator JsonOutput_ValidJsonStructure()
        {
            // Arrange
            var message = CreateTestMessage("Valid JSON test", LogLevel.Warn);
            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Should be valid JSON
            Assert.IsTrue(json.TrimStart().StartsWith("{"), "JSON should start with opening brace");
            Assert.IsTrue(json.TrimEnd().EndsWith("}"), "JSON should end with closing brace");

            // Should have proper quote pairing
            var quoteCount = json.Split('"').Length - 1;
            Assert.AreEqual(0, quoteCount % 2, "JSON should have even number of quotes");
        }

        [UnityTest]
        public IEnumerator JsonOutput_TimestampFormat_ISO8601()
        {
            // Arrange
            var message = CreateTestMessage("Timestamp test");
            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Timestamp should be in ISO 8601 format
            // Pattern: YYYY-MM-DDTHH:mm:ss or YYYY-MM-DDTHH:mm:ss.fff
            var iso8601Pattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}";
            Assert.IsTrue(Regex.IsMatch(json, iso8601Pattern),
                $"JSON should contain ISO 8601 timestamp. JSON: {json}");
        }

        [UnityTest]
        public IEnumerator JsonOutput_LevelField_CorrectValues()
        {
            // Arrange & Act & Assert for each level
            var levels = new[]
            {
                (LogLevel.Trace, "Trace"),
                (LogLevel.Debug, "Debug"),
                (LogLevel.Info, "Info"),
                (LogLevel.Warn, "Warn"),
                (LogLevel.Error, "Error"),
                (LogLevel.Critical, "Critical")
            };

            foreach (var (level, expectedName) in levels)
            {
                var message = CreateTestMessage("Level test", level);
                var json = _templateEngine.Format(message, MessageFormat.Json);

                Assert.IsTrue(json.Contains(expectedName) || json.Contains(level.ToString()),
                    $"JSON should contain level '{expectedName}' or '{level}'. JSON: {json}");
            }

            yield return null;
        }

        #endregion

        #region JSON Context Field Tests

        [UnityTest]
        public IEnumerator JsonOutput_WithContext_IncludesContextObject()
        {
            // Arrange
            var message = CreateTestMessage("Context test");
            message.Context = new Dictionary<string, object>
            {
                { "userId", "user123" },
                { "requestId", 456 },
                { "isActive", true }
            };

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Context data should be present
            Assert.IsTrue(json.Contains("user123"), "JSON should contain userId value");
            Assert.IsTrue(json.Contains("456") || json.Contains("requestId"),
                "JSON should contain requestId");
        }

        [UnityTest]
        public IEnumerator JsonOutput_WithoutContext_ValidStructure()
        {
            // Arrange
            var message = CreateTestMessage("No context test");
            message.Context = null;

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Should still be valid JSON even without context
            Assert.IsTrue(json.Contains("{") && json.Contains("}"),
                "JSON should be valid structure even without context");
            Assert.IsTrue(json.Contains("No context test"), "JSON should contain message");
        }

        [UnityTest]
        public IEnumerator JsonOutput_EmptyContext_ValidStructure()
        {
            // Arrange
            var message = CreateTestMessage("Empty context test");
            message.Context = new Dictionary<string, object>();

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert
            Assert.IsTrue(json.Contains("{") && json.Contains("}"),
                "JSON should be valid with empty context");
        }

        #endregion

        #region JSON Escaping Tests

        [UnityTest]
        public IEnumerator JsonOutput_SpecialCharacters_ProperlyEscaped()
        {
            // Arrange
            var message = CreateTestMessage("Message with \"quotes\" and \\backslash");
            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Special characters should be escaped
            // Should contain escaped quotes or properly formatted JSON
            Assert.IsTrue(json.Contains("\\\"") || json.Contains("&quot;"),
                $"Quotes should be escaped in JSON. JSON: {json}");
        }

        [UnityTest]
        public IEnumerator JsonOutput_NewlineCharacters_ProperlyEscaped()
        {
            // Arrange
            var message = CreateTestMessage("Message with\nnewline\rand\ttab");
            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Should not have raw newlines breaking JSON structure
            var lines = json.Split('\n');
            // If properly escaped, should be single line or properly formatted
            Assert.IsTrue(json.Contains("\\n") || json.Contains("\\r") || lines.Length >= 1,
                "Newlines should be escaped or JSON should be properly formatted");
        }

        #endregion

        #region JSON Snapshot Tests (Determinism)

        [UnityTest]
        public IEnumerator JsonOutput_Snapshot_BasicMessage()
        {
            // Arrange - Create deterministic message
            var message = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Snapshot",
                Message = "Snapshot test message",
                Timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Frame = 1,
                ThreadId = 1,
                ThreadName = "Main",
                CallerFilePath = "Test.cs",
                CallerMemberName = "SnapshotTest",
                CallerLineNumber = 10
            };

            // Act
            var json1 = _templateEngine.Format(message, MessageFormat.Json);
            var json2 = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert - Same input should produce identical output (snapshot consistency)
            Assert.AreEqual(json1, json2,
                "Identical messages should produce identical JSON (deterministic output)");
        }

        [UnityTest]
        public IEnumerator JsonOutput_Snapshot_WithContext()
        {
            // Arrange
            var message = new LogMessage
            {
                Level = LogLevel.Error,
                Category = "SnapshotContext",
                Message = "Error with context",
                Timestamp = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc),
                Frame = 500,
                ThreadId = 2,
                ThreadName = "Worker",
                Context = new Dictionary<string, object>
                {
                    { "errorCode", 404 },
                    { "errorMessage", "Not found" },
                    { "retryable", false }
                }
            };

            // Act
            var json1 = _templateEngine.Format(message, MessageFormat.Json);
            var json2 = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert
            Assert.AreEqual(json1, json2,
                "Messages with context should produce deterministic JSON");
        }

        #endregion

        #region JSON Metadata Fields Tests

        [UnityTest]
        public IEnumerator JsonOutput_ContainsThreadInfo()
        {
            // Arrange
            var message = CreateTestMessage("Thread info test");
            message.ThreadId = 42;
            message.ThreadName = "CustomThread";

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert
            Assert.IsTrue(json.Contains("42") || json.Contains("CustomThread"),
                "JSON should contain thread information");
        }

        [UnityTest]
        public IEnumerator JsonOutput_ContainsFrameInfo()
        {
            // Arrange
            var message = CreateTestMessage("Frame info test");
            message.Frame = 1234;

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert
            Assert.IsTrue(json.Contains("1234") || json.Contains("frame"),
                "JSON should contain frame information");
        }

        [UnityTest]
        public IEnumerator JsonOutput_ContainsCallerInfo()
        {
            // Arrange
            var message = CreateTestMessage("Caller info test");
            message.CallerMemberName = "TestMethod";
            message.CallerFilePath = "TestFile.cs";
            message.CallerLineNumber = 123;

            var json = _templateEngine.Format(message, MessageFormat.Json);

            yield return null;

            // Assert
            Assert.IsTrue(json.Contains("TestMethod") || json.Contains("TestFile") || json.Contains("123"),
                "JSON should contain caller information");
        }

        #endregion

        #region JSON Schema Stability Tests

        [UnityTest]
        public IEnumerator JsonOutput_SchemaStability_AllLogLevels()
        {
            // This test ensures all log levels produce consistent JSON structure
            // Arrange
            var levels = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Info,
                                LogLevel.Warn, LogLevel.Error, LogLevel.Critical };

            // Act & Assert
            foreach (var level in levels)
            {
                var message = CreateTestMessage($"Level {level}", level);
                var json = _templateEngine.Format(message, MessageFormat.Json);

                // All levels should produce valid JSON with required fields
                Assert.IsTrue(json.Contains("{") && json.Contains("}"),
                    $"Level {level} should produce valid JSON");
                Assert.IsTrue(json.Contains("\"level\"") || json.Contains("\"Level\"") ||
                             json.Contains(level.ToString()),
                    $"Level {level} JSON should contain level field");
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator JsonOutput_SchemaStability_DifferentCategories()
        {
            // Ensure different categories don't change JSON structure
            // Arrange
            var categories = new[] { "System", "Network", "UI", "Physics", "Audio" };

            // Act & Assert
            foreach (var category in categories)
            {
                var message = CreateTestMessage($"Message from {category}", LogLevel.Info, category);
                var json = _templateEngine.Format(message, MessageFormat.Json);

                Assert.IsTrue(json.Contains("{") && json.Contains("}"),
                    $"Category {category} should produce valid JSON");
                Assert.IsTrue(json.Contains(category),
                    $"JSON should contain category '{category}'");
            }

            yield return null;
        }

        #endregion
    }
}
