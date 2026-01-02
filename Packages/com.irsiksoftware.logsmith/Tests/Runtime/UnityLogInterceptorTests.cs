using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Tests for UnityLogInterceptor - automatic routing of Debug.Log messages
    /// with [Category] prefix parsing to the LogSmith system.
    /// </summary>
    [TestFixture]
    public class UnityLogInterceptorTests
    {
        private LogRouter _router;
        private TestLogSink _sink;
        private UnityLogInterceptor _interceptor;

        [SetUp]
        public void Setup()
        {
            _router = new LogRouter();
            _sink = new TestLogSink("TestSink");
            _router.RegisterSink(_sink);
        }

        [TearDown]
        public void TearDown()
        {
            _interceptor?.Dispose();
            _interceptor = null;
        }

        #region Category Parsing Tests

        [Test]
        public void ParseCategory_ValidBracketSyntax_ReturnsCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[AI] - Player moved to position");

            // Assert
            Assert.IsTrue(result.HasCategory);
            Assert.AreEqual("AI", result.Category);
            Assert.AreEqual("Player moved to position", result.Message);
        }

        [Test]
        public void ParseCategory_CategoryWithSpaces_ReturnsCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[Game Systems] - Initializing subsystems");

            // Assert
            Assert.IsTrue(result.HasCategory);
            Assert.AreEqual("Game Systems", result.Category);
            Assert.AreEqual("Initializing subsystems", result.Message);
        }

        [Test]
        public void ParseCategory_NoBrackets_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("Just a regular log message");

            // Assert
            Assert.IsFalse(result.HasCategory);
            Assert.IsNull(result.Category);
            Assert.AreEqual("Just a regular log message", result.Message);
        }

        [Test]
        public void ParseCategory_MissingDash_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[AI] Player moved");

            // Assert
            Assert.IsFalse(result.HasCategory);
        }

        [Test]
        public void ParseCategory_EmptyBrackets_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[] - Empty category");

            // Assert
            Assert.IsFalse(result.HasCategory);
        }

        [Test]
        public void ParseCategory_WhitespaceOnlyCategory_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[   ] - Whitespace category");

            // Assert
            Assert.IsFalse(result.HasCategory);
        }

        [Test]
        public void ParseCategory_NullMessage_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage(null);

            // Assert
            Assert.IsFalse(result.HasCategory);
            Assert.IsNull(result.Message);
        }

        [Test]
        public void ParseCategory_EmptyMessage_ReturnsNoCategory()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage(string.Empty);

            // Assert
            Assert.IsFalse(result.HasCategory);
            Assert.AreEqual(string.Empty, result.Message);
        }

        [Test]
        public void ParseCategory_NestedBrackets_UsesFirstBracket()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[Outer[Inner]] - Message");

            // Assert - Should extract up to first closing bracket
            Assert.IsTrue(result.HasCategory);
            Assert.AreEqual("Outer[Inner]", result.Category);
        }

        [Test]
        public void ParseCategory_MessageAfterDash_PreservesMessage()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[Network] - Connection established - retrying 3 times");

            // Assert
            Assert.IsTrue(result.HasCategory);
            Assert.AreEqual("Network", result.Category);
            Assert.AreEqual("Connection established - retrying 3 times", result.Message);
        }

        [Test]
        public void ParseCategory_TrimsWhitespace()
        {
            // Arrange & Act
            var result = UnityLogInterceptor.ParseCategoryFromMessage("[  Combat  ] -   Attack dealt 50 damage  ");

            // Assert
            Assert.IsTrue(result.HasCategory);
            Assert.AreEqual("Combat", result.Category);
            Assert.AreEqual("Attack dealt 50 damage", result.Message);
        }

        #endregion

        #region Log Interception Tests

        [Test]
        public void Interceptor_CategorizedLog_RoutesToLogSmith()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act - Simulate what would come from Application.logMessageReceived
            // Note: HandleLog receives messages that already appeared in console, it just routes them to LogSmith
            _interceptor.HandleLog("[AI] - Test AI message", string.Empty, LogType.Log);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual("AI", _sink.Messages[0].Category);
            Assert.AreEqual("Test AI message", _sink.Messages[0].Message);
        }

        [Test]
        public void Interceptor_UncategorizedLog_NotRouted()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.HandleLog("Regular log message", string.Empty, LogType.Log);

            // Assert - Should not route uncategorized messages
            Assert.AreEqual(0, _sink.Messages.Count);
        }

        [Test]
        public void Interceptor_WarningLog_MapsToWarnLevel()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.HandleLog("[Combat] - Low health warning", string.Empty, LogType.Warning);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual(LogLevel.Warn, _sink.Messages[0].Level);
        }

        [Test]
        public void Interceptor_ErrorLog_MapsToErrorLevel()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.HandleLog("[Network] - Connection failed", string.Empty, LogType.Error);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual(LogLevel.Error, _sink.Messages[0].Level);
        }

        [Test]
        public void Interceptor_ExceptionLog_MapsToCriticalLevel()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.HandleLog("[System] - Critical failure", "NullReferenceException: ...", LogType.Exception);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual(LogLevel.Critical, _sink.Messages[0].Level);
        }

        [Test]
        public void Interceptor_AssertLog_MapsToErrorLevel()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.HandleLog("[Debug] - Assertion failed", string.Empty, LogType.Assert);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual(LogLevel.Error, _sink.Messages[0].Level);
        }

        [Test]
        public void Interceptor_Disabled_DoesNotIntercept()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            // Not calling Enable()

            // Act
            _interceptor.HandleLog("[AI] - Should not be intercepted", string.Empty, LogType.Log);

            // Assert
            Assert.AreEqual(0, _sink.Messages.Count);
        }

        [Test]
        public void Interceptor_EnableDisable_TogglesInterception()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);

            // Act - Enable and send message
            _interceptor.Enable();
            _interceptor.HandleLog("[AI] - First message", string.Empty, LogType.Log);

            // Disable and send another message
            _interceptor.Disable();
            _interceptor.HandleLog("[AI] - Second message", string.Empty, LogType.Log);

            // Assert - Only first message should be routed
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual("First message", _sink.Messages[0].Message);
        }

        [Test]
        public void Interceptor_AvoidsFeedbackLoop_IgnoresLogSmithMessages()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act - Simulate a LogSmith internal message (prefixed with [LogSmith])
            _interceptor.HandleLog("[LogSmith] Internal message", string.Empty, LogType.Log);

            // Assert - Should not route to avoid feedback loop
            Assert.AreEqual(0, _sink.Messages.Count);
        }

        [Test]
        public void Interceptor_Dispose_DisablesInterception()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();

            // Act
            _interceptor.Dispose();
            _interceptor.HandleLog("[AI] - Should not be intercepted", string.Empty, LogType.Log);

            // Assert
            Assert.AreEqual(0, _sink.Messages.Count);

            // Prevent double-dispose in TearDown
            _interceptor = null;
        }

        [Test]
        public void Interceptor_PreservesStackTrace()
        {
            // Arrange
            _interceptor = new UnityLogInterceptor(_router);
            _interceptor.Enable();
            const string stackTrace = "at SomeClass.SomeMethod() in SomeFile.cs:123";

            // Act
            _interceptor.HandleLog("[Debug] - Test message", stackTrace, LogType.Error);

            // Assert
            Assert.AreEqual(1, _sink.Messages.Count);
            Assert.AreEqual(stackTrace, _sink.Messages[0].StackTrace);
        }

        #endregion

        #region Helper Classes

        private class TestLogSink : ILogSink
        {
            public string Name { get; }
            public List<LogMessage> Messages { get; } = new List<LogMessage>();

            public TestLogSink(string name)
            {
                Name = name;
            }

            public void Write(LogMessage message)
            {
                Messages.Add(message);
            }

            public void Flush()
            {
            }
        }

        #endregion
    }
}
