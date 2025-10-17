using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Configuration settings for the LogSmith logging system.
    /// Can be used with VContainer for dependency injection or with static initialization.
    /// </summary>
    [CreateAssetMenu(fileName = "LoggingSettings", menuName = "LogSmith/Logging Settings", order = 1)]
    public class LoggingSettings : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Minimum log level for all categories (unless overridden per-category)")]
        public LogLevel minimumLogLevel = LogLevel.Debug;

        [Tooltip("Enable console output via ConsoleSink")]
        public bool enableConsoleSink = true;

        [Header("Categories")]
        [Tooltip("List of log categories with their individual settings")]
        public List<CategoryDefinition> categories = new List<CategoryDefinition>();

        [Header("File Sink Settings")]
        [Tooltip("Enable file output via FileSink")]
        public bool enableFileSink = false;

        [Tooltip("File path for log output (relative to Application.persistentDataPath)")]
        public string logFilePath = "Logs/logsmith.log";

        [Tooltip("Output format for file sink")]
        public MessageFormatMode fileOutputFormat = MessageFormatMode.Text;

        [Tooltip("Maximum log file size in MB before rotation (0 = no rotation)")]
        public int maxFileSizeMB = 10;

        [Tooltip("Number of archived log files to keep")]
        public int retainedFileCount = 5;

        [Header("Message Formatting")]
        [Tooltip("Default message format mode")]
        public MessageFormatMode defaultFormatMode = MessageFormatMode.Text;

        [Tooltip("Default text template for message formatting")]
        [TextArea(3, 5)]
        public string defaultTextTemplate = "{timestamp} | {level} | [{category}] {message}";

        [Tooltip("Default JSON template for message formatting")]
        [TextArea(3, 5)]
        public string defaultJsonTemplate = "{\"timestamp\":\"{timestamp}\",\"level\":\"{level}\",\"category\":\"{category}\",\"message\":\"{message}\"}";

        [Tooltip("Per-category template overrides")]
        public List<CategoryTemplateOverride> templateOverrides = new List<CategoryTemplateOverride>();

        [Header("Performance")]
        [Tooltip("Buffer size for file writes (0 = unbuffered)")]
        public int fileBufferSize = 4096;

        /// <summary>
        /// Creates default settings for quick initialization.
        /// </summary>
        public static LoggingSettings CreateDefault()
        {
            var settings = CreateInstance<LoggingSettings>();
            settings.minimumLogLevel = LogLevel.Debug;
            settings.enableConsoleSink = true;
            settings.enableFileSink = false;
            settings.logFilePath = "Logs/logsmith.log";
            settings.defaultFormatMode = MessageFormatMode.Text;
            settings.defaultTextTemplate = "{timestamp} | {level} | [{category}] {message}";
            settings.defaultJsonTemplate = "{\"timestamp\":\"{timestamp}\",\"level\":\"{level}\",\"category\":\"{category}\",\"message\":\"{message}\"}";
            settings.fileBufferSize = 4096;
            settings.fileOutputFormat = MessageFormatMode.Text;
            settings.maxFileSizeMB = 10;
            settings.retainedFileCount = 5;

            // Add default category
            settings.categories.Add(new CategoryDefinition("Default", Color.white, LogLevel.Debug, true));

            return settings;
        }

        /// <summary>
        /// Gets predefined pleasant colors for categories.
        /// </summary>
        public static Color[] GetPredefinedColors()
        {
            return new Color[]
            {
                new Color(0.4f, 0.8f, 1.0f),    // Light Blue
                new Color(0.4f, 1.0f, 0.6f),    // Light Green
                new Color(1.0f, 0.8f, 0.4f),    // Light Orange
                new Color(1.0f, 0.6f, 0.8f),    // Light Pink
                new Color(0.8f, 0.6f, 1.0f),    // Light Purple
                new Color(0.6f, 1.0f, 1.0f),    // Cyan
                new Color(1.0f, 1.0f, 0.6f),    // Light Yellow
                new Color(0.8f, 0.8f, 0.8f)     // Light Gray
            };
        }
    }

    /// <summary>
    /// Message format mode for template engine.
    /// </summary>
    public enum MessageFormatMode
    {
        Text,
        Json
    }
}