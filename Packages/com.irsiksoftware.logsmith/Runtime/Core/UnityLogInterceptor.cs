using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Intercepts Unity Debug.Log messages and routes those with [Category] prefix
    /// to the LogSmith system for automatic categorization.
    /// </summary>
    public class UnityLogInterceptor : IDisposable
    {
        private const string LogSmithCategoryPrefix = "LogSmith";
        private static readonly Regex CategoryPattern = new Regex(
            @"^\[([^\]]+)\]\s*-\s*(.*)$",
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        private readonly ILogRouter _router;
        private bool _isEnabled;
        private bool _disposed;

        /// <summary>
        /// Creates a new UnityLogInterceptor that routes categorized messages to the specified router.
        /// </summary>
        /// <param name="router">The log router to send intercepted messages to.</param>
        public UnityLogInterceptor(ILogRouter router)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        /// <summary>
        /// Enables interception of Unity log messages via Application.logMessageReceived.
        /// </summary>
        public void Enable()
        {
            if (_disposed) return;
            if (_isEnabled) return;

            Application.logMessageReceived += OnLogMessageReceived;
            _isEnabled = true;
        }

        /// <summary>
        /// Disables interception of Unity log messages.
        /// </summary>
        public void Disable()
        {
            if (!_isEnabled) return;

            Application.logMessageReceived -= OnLogMessageReceived;
            _isEnabled = false;
        }

        /// <summary>
        /// Handles a log message. Can be called directly for testing or via Application.logMessageReceived.
        /// </summary>
        /// <param name="condition">The log message text.</param>
        /// <param name="stackTrace">The stack trace associated with the log.</param>
        /// <param name="type">The Unity log type.</param>
        public void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (!_isEnabled) return;

            var parseResult = ParseCategoryFromMessage(condition);

            // Only route messages with valid category syntax
            if (!parseResult.HasCategory) return;

            // Avoid feedback loop - ignore LogSmith's own internal messages
            if (string.Equals(parseResult.Category, LogSmithCategoryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var logLevel = MapUnityLogType(type);
            var message = new LogMessage
            {
                Level = logLevel,
                Category = parseResult.Category,
                Message = parseResult.Message,
                Timestamp = DateTime.UtcNow,
                Frame = Time.frameCount,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                StackTrace = stackTrace
            };

            _router.Route(message);
        }

        /// <summary>
        /// Parses a message string for [Category] - Message syntax.
        /// </summary>
        /// <param name="message">The raw log message.</param>
        /// <returns>A result containing the parsed category and message, or indicating no category was found.</returns>
        public static ParseResult ParseCategoryFromMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return new ParseResult { HasCategory = false, Message = message };
            }

            var match = CategoryPattern.Match(message);
            if (!match.Success)
            {
                return new ParseResult { HasCategory = false, Message = message };
            }

            var category = match.Groups[1].Value.Trim();
            var parsedMessage = match.Groups[2].Value.Trim();

            // Empty or whitespace-only category is invalid
            if (string.IsNullOrWhiteSpace(category))
            {
                return new ParseResult { HasCategory = false, Message = message };
            }

            return new ParseResult
            {
                HasCategory = true,
                Category = category,
                Message = parsedMessage
            };
        }

        /// <summary>
        /// Maps Unity LogType to LogSmith LogLevel.
        /// </summary>
        private static LogLevel MapUnityLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    return LogLevel.Info;
                case LogType.Warning:
                    return LogLevel.Warn;
                case LogType.Error:
                    return LogLevel.Error;
                case LogType.Assert:
                    return LogLevel.Error;
                case LogType.Exception:
                    return LogLevel.Critical;
                default:
                    return LogLevel.Info;
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            HandleLog(condition, stackTrace, type);
        }

        /// <summary>
        /// Disposes the interceptor and unsubscribes from log events.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Disable();
        }

        /// <summary>
        /// Result of parsing a log message for category syntax.
        /// </summary>
        public struct ParseResult
        {
            /// <summary>
            /// Whether a valid category was found.
            /// </summary>
            public bool HasCategory;

            /// <summary>
            /// The parsed category name (null if no category found).
            /// </summary>
            public string Category;

            /// <summary>
            /// The message text (without category prefix if parsed, original message otherwise).
            /// </summary>
            public string Message;
        }
    }
}
