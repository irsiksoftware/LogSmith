using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Minimal template engine for formatting log messages.
    /// </summary>
    public class MessageTemplateEngine : IMessageTemplateEngine
    {
        private readonly Dictionary<string, string> _categoryTemplates = new Dictionary<string, string>();
        private readonly string _defaultTemplate = "[{Timestamp:HH:mm:ss}] [{Level}] [{Category}] {Message}";
        private readonly object _lock = new object();

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

        private string FormatText(LogMessage message)
        {
            var template = GetCategoryTemplate(message.Category);

            // Simple token replacement
            return template
                .Replace("{Timestamp:HH:mm:ss}", message.Timestamp.ToString("HH:mm:ss"))
                .Replace("{Timestamp}", message.Timestamp.ToString("o"))
                .Replace("{Level}", message.Level.ToString())
                .Replace("{Category}", message.Category)
                .Replace("{Message}", message.Message);
        }

        private string FormatJson(LogMessage message)
        {
            // Minimal JSON formatting without dependencies
            return $"{{\"timestamp\":\"{message.Timestamp:o}\",\"level\":\"{message.Level}\",\"category\":\"{EscapeJson(message.Category)}\",\"message\":\"{EscapeJson(message.Message)}\"}}";
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