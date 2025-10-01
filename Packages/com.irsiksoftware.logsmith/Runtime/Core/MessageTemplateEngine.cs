using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Robust template engine for formatting log messages with extensive token support.
    /// Supports custom timestamp formats, context data, performance metrics, and diagnostic information.
    /// </summary>
    public class MessageTemplateEngine : IMessageTemplateEngine
    {
        private readonly Dictionary<string, string> _categoryTemplates = new Dictionary<string, string>();
        private string _defaultTemplate = "[{timestamp:HH:mm:ss}] [{level}] [{category}] {message}";
        private readonly object _lock = new object();
        private static readonly Regex TokenRegex = new Regex(@"\{([^}]+)\}", RegexOptions.Compiled);

        public string Format(LogMessage message, MessageFormat format)
        {
            switch (format)
            {
                case MessageFormat.Text:
                    return FormatText(message);
                case MessageFormat.Json:
                    return FormatJson(message);
                default:
                    return FormatText(message);
            }
        }

        public void SetCategoryTemplate(string category, string template)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrEmpty(template)) throw new ArgumentNullException(nameof(template));

            lock (_lock)
            {
                _categoryTemplates[category] = template;
            }
        }

        public string GetCategoryTemplate(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultTemplate;

            lock (_lock)
            {
                return _categoryTemplates.TryGetValue(category, out var template) ? template : _defaultTemplate;
            }
        }

        /// <summary>
        /// Sets the default template used for categories without specific overrides.
        /// </summary>
        public void SetDefaultTemplate(string template)
        {
            if (string.IsNullOrEmpty(template)) throw new ArgumentNullException(nameof(template));

            lock (_lock)
            {
                _defaultTemplate = template;
            }
        }

        private string FormatText(LogMessage message)
        {
            var template = GetCategoryTemplate(message.Category);
            return ReplaceTokens(template, message);
        }

        private string ReplaceTokens(string template, LogMessage message)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            return TokenRegex.Replace(template, match =>
            {
                var token = match.Groups[1].Value;
                return ResolveToken(token, message);
            });
        }

        private string ResolveToken(string token, LogMessage message)
        {
            try
            {
                // Handle timestamp with custom format
                if (token.StartsWith("timestamp:", StringComparison.OrdinalIgnoreCase))
                {
                    var format = token.Substring(10);
                    return message.Timestamp.ToString(format);
                }

                // Simple token matching (case-insensitive)
                switch (token.ToLowerInvariant())
                {
                    case "timestamp":
                        return message.Timestamp.ToString("o"); // ISO 8601
                    case "level":
                        return message.Level.ToString();
                    case "category":
                        return message.Category ?? string.Empty;
                    case "message":
                        return message.Message ?? string.Empty;
                    case "frame":
                        return message.Frame.ToString();
                    case "file":
                        return message.CallerFilePath ?? string.Empty;
                    case "method":
                        return message.CallerMemberName ?? string.Empty;
                    case "memorymb":
                        return GetMemoryUsageMB().ToString("F2");
                    case "stack":
                        return message.StackTrace ?? string.Empty;
                    case "thread":
                        return !string.IsNullOrEmpty(message.ThreadName)
                            ? $"{message.ThreadName} ({message.ThreadId})"
                            : message.ThreadId.ToString();
                    case "threadid":
                        return message.ThreadId.ToString();
                    case "threadname":
                        return message.ThreadName ?? string.Empty;
                    case "context":
                        return FormatContext(message.Context);
                    default:
                        // Try to resolve from context dictionary
                        if (message.Context != null && message.Context.TryGetValue(token, out var contextValue))
                        {
                            return contextValue?.ToString() ?? string.Empty;
                        }
                        // Return original token if not recognized (graceful handling)
                        return $"{{{token}}}";
                }
            }
            catch (Exception)
            {
                // Graceful error handling - return the malformed token as-is
                return $"{{{token}}}";
            }
        }

        private string FormatContext(Dictionary<string, object> context)
        {
            if (context == null || context.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            bool first = true;
            foreach (var kvp in context)
            {
                if (!first) sb.Append(", ");
                sb.Append($"{kvp.Key}={kvp.Value}");
                first = false;
            }
            return sb.ToString();
        }

        private float GetMemoryUsageMB()
        {
            try
            {
                return UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            }
            catch
            {
                return 0f;
            }
        }

        private string FormatJson(LogMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"timestamp\":\"{message.Timestamp:o}\",");
            sb.Append($"\"level\":\"{message.Level}\",");
            sb.Append($"\"category\":\"{EscapeJson(message.Category)}\",");
            sb.Append($"\"message\":\"{EscapeJson(message.Message)}\"");

            // Optional fields
            if (message.Frame > 0)
            {
                sb.Append($",\"frame\":{message.Frame}");
            }

            if (message.ThreadId != 0)
            {
                sb.Append($",\"threadId\":{message.ThreadId}");
            }

            if (!string.IsNullOrEmpty(message.ThreadName))
            {
                sb.Append($",\"threadName\":\"{EscapeJson(message.ThreadName)}\"");
            }

            if (!string.IsNullOrEmpty(message.CallerFilePath))
            {
                sb.Append($",\"file\":\"{EscapeJson(message.CallerFilePath)}\"");
            }

            if (!string.IsNullOrEmpty(message.CallerMemberName))
            {
                sb.Append($",\"method\":\"{EscapeJson(message.CallerMemberName)}\"");
            }

            if (!string.IsNullOrEmpty(message.StackTrace))
            {
                sb.Append($",\"stackTrace\":\"{EscapeJson(message.StackTrace)}\"");
            }

            if (message.Context != null && message.Context.Count > 0)
            {
                sb.Append(",\"context\":{");
                bool first = true;
                foreach (var kvp in message.Context)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"\"{EscapeJson(kvp.Key)}\":\"{EscapeJson(kvp.Value?.ToString() ?? string.Empty)}\"");
                    first = false;
                }
                sb.Append("}");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
