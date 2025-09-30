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

        [Header("File Sink Settings")]
        [Tooltip("Enable file output via FileSink")]
        public bool enableFileSink = false;

        [Tooltip("File path for log output (relative to Application.persistentDataPath)")]
        public string logFilePath = "Logs/logsmith.log";

        [Header("Message Formatting")]
        [Tooltip("Default message format mode")]
        public MessageFormatMode defaultFormatMode = MessageFormatMode.Text;

        [Tooltip("Default text template for message formatting")]
        [TextArea(3, 5)]
        public string defaultTextTemplate = "{timestamp} [{level}] {category}: {message}";

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
            settings.defaultTextTemplate = "{timestamp} [{level}] {category}: {message}";
            settings.fileBufferSize = 4096;
            return settings;
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