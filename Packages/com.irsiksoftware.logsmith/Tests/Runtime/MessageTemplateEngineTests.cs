using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Tests
{
    [TestFixture]
    public class MessageTemplateEngineTests
    {
        private MessageTemplateEngine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new MessageTemplateEngine();
        }

        private LogMessage CreateTestMessage()
        {
            return new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Test message",
                Timestamp = new DateTime(2025, 1, 15, 14, 30, 45, DateTimeKind.Utc),
                Frame = 1000,
                ThreadId = 42,
                ThreadName = "MainThread",
                CallerFilePath = "TestFile.cs",
                CallerMemberName = "TestMethod",
                StackTrace = "at TestMethod()",
                Context = new Dictionary<string, object>
                {
                    { "userId", "user123" },
                    { "sessionId", 456 }
                }
            };
        }

        [Test]
        public void Format_AllStandardTokens_ReplacedCorrectly()
        {
            // Arrange
            var template = "{level} {category} {message} {frame} {threadId} {file} {method}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("Info Test Test message 1000 42 TestFile.cs TestMethod", result);
        }

        [Test]
        public void Format_TimestampWithCustomFormat_FormatsCorrectly()
        {
            // Arrange
            var template = "{timestamp:yyyy-MM-dd} {timestamp:HH:mm:ss}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("2025-01-15 14:30:45", result);
        }

        [Test]
        public void Format_TimestampWithoutFormat_UsesISO8601()
        {
            // Arrange
            var template = "{timestamp}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            StringAssert.StartsWith("2025-01-15T14:30:45", result);
        }

        [Test]
        public void Format_ThreadToken_CombinesNameAndId()
        {
            // Arrange
            var template = "{thread}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("MainThread (42)", result);
        }

        [Test]
        public void Format_ThreadTokenNoName_ShowsIdOnly()
        {
            // Arrange
            var template = "{thread}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();
            message.ThreadName = null;

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("42", result);
        }

        [Test]
        public void Format_ContextToken_FormatsAllKeyValuePairs()
        {
            // Arrange
            var template = "{context}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            StringAssert.Contains("userId=user123", result);
            StringAssert.Contains("sessionId=456", result);
        }

        [Test]
        public void Format_ContextKeyAsToken_ResolvesFromDictionary()
        {
            // Arrange
            var template = "User: {userId}, Session: {sessionId}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("User: user123, Session: 456", result);
        }

        [Test]
        public void Format_MalformedToken_ReturnsTokenAsIs()
        {
            // Arrange
            var template = "{unknownToken} {message}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("{unknownToken} Test message", result);
        }

        [Test]
        public void Format_EmptyTemplate_ReturnsEmptyString()
        {
            // Arrange
            _engine.SetCategoryTemplate("Test", "");
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void Format_NoTemplate_UsesDefault()
        {
            // Arrange
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            StringAssert.Contains("[Info]", result);
            StringAssert.Contains("[Test]", result);
            StringAssert.Contains("Test message", result);
        }

        [Test]
        public void Format_CaseSensitiveTokens_HandledCorrectly()
        {
            // Arrange
            var template = "{LEVEL} {Level} {level}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("Info Info Info", result);
        }

        [Test]
        public void Format_JsonFormat_ProducesValidJson()
        {
            // Arrange
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Json);

            // Assert
            StringAssert.Contains("\"level\":\"Info\"", result);
            StringAssert.Contains("\"category\":\"Test\"", result);
            StringAssert.Contains("\"message\":\"Test message\"", result);
            StringAssert.Contains("\"frame\":1000", result);
            StringAssert.Contains("\"threadId\":42", result);
        }

        [Test]
        public void Format_JsonWithContext_IncludesContextObject()
        {
            // Arrange
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Json);

            // Assert
            StringAssert.Contains("\"context\":{", result);
            StringAssert.Contains("\"userId\":\"user123\"", result);
            StringAssert.Contains("\"sessionId\":\"456\"", result);
        }

        [Test]
        public void Format_JsonEscaping_HandlesSpecialCharacters()
        {
            // Arrange
            var message = CreateTestMessage();
            message.Message = "Test \"quoted\" message\nwith newline\ttab\\backslash";

            // Act
            var result = _engine.Format(message, MessageFormat.Json);

            // Assert
            StringAssert.Contains("\\\"quoted\\\"", result);
            StringAssert.Contains("\\n", result);
            StringAssert.Contains("\\t", result);
            StringAssert.Contains("\\\\", result);
        }

        [Test]
        public void Format_OptionalFields_OmittedWhenEmpty()
        {
            // Arrange
            var message = new LogMessage
            {
                Level = LogLevel.Warn,
                Category = "Test",
                Message = "Simple message",
                Timestamp = DateTime.UtcNow
            };

            // Act
            var result = _engine.Format(message, MessageFormat.Json);

            // Assert
            StringAssert.DoesNotContain("stackTrace", result);
            StringAssert.DoesNotContain("context", result);
            StringAssert.DoesNotContain("\"file\"", result);
        }

        [Test]
        public void SetCategoryTemplate_ThrowsOnNullCategory()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _engine.SetCategoryTemplate(null, "{message}"));
        }

        [Test]
        public void SetCategoryTemplate_ThrowsOnNullTemplate()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _engine.SetCategoryTemplate("Test", null));
        }

        [Test]
        public void GetCategoryTemplate_ReturnsDefaultForUnsetCategory()
        {
            // Act
            var template = _engine.GetCategoryTemplate("UnsetCategory");

            // Assert
            StringAssert.Contains("{timestamp", template);
            StringAssert.Contains("{level}", template);
            StringAssert.Contains("{category}", template);
            StringAssert.Contains("{message}", template);
        }

        [Test]
        public void Format_MemoryToken_ReturnsNumericValue()
        {
            // Arrange
            var template = "{memoryMB}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert - Should be a number
            Assert.IsTrue(float.TryParse(result, out _), $"Memory value should be numeric, got: {result}");
        }

        [Test]
        public void Format_StackToken_ReturnsStackTrace()
        {
            // Arrange
            var template = "{stack}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("at TestMethod()", result);
        }

        [Test]
        public void Format_ComplexTemplate_AllTokensReplaced()
        {
            // Arrange
            var template = "[{timestamp:HH:mm:ss}] [{level}] {category} | Frame:{frame} Thread:{thread} | {message} | File:{file}:{method}";
            _engine.SetCategoryTemplate("Test", template);
            var message = CreateTestMessage();

            // Act
            var result = _engine.Format(message, MessageFormat.Text);

            // Assert
            Assert.AreEqual("[14:30:45] [Info] Test | Frame:1000 Thread:MainThread (42) | Test message | File:TestFile.cs:TestMethod", result);
        }

        [Test]
        public void Format_MultipleCategories_MaintainsSeparateTemplates()
        {
            // Arrange
            _engine.SetCategoryTemplate("Cat1", "{level}: {message}");
            _engine.SetCategoryTemplate("Cat2", "[{category}] {message}");
            var msg1 = CreateTestMessage();
            msg1.Category = "Cat1";
            var msg2 = CreateTestMessage();
            msg2.Category = "Cat2";

            // Act
            var result1 = _engine.Format(msg1, MessageFormat.Text);
            var result2 = _engine.Format(msg2, MessageFormat.Text);

            // Assert
            Assert.AreEqual("Info: Test message", result1);
            Assert.AreEqual("[Cat2] Test message", result2);
        }
    }
}
