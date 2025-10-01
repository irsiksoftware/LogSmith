using System;
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

        [Tooltip("Per-category minimum level overrides")]
        public List<CategoryMinLevelOverride> categoryMinLevelOverrides = new List<CategoryMinLevelOverride>();

        [Tooltip("Enable console output via ConsoleSink")]
        public bool enableConsoleSink = true;

        [Header("File Sink Settings")]
        [Tooltip("Enable file output via FileSink")]
        public bool enableFileSink = false;

        [Tooltip("File path for log output (relative to Application.persistentDataPath)")]
        public string logFilePath = "Logs/logsmith.log";

        [Tooltip("Enable log file rotation")]
        public bool enableLogRotation = true;

        [Tooltip("Maximum file size in MB before rotation (0 = no limit)")]
        public int maxFileSizeMB = 10;

        [Tooltip("Number of archived log files to retain (0 = keep all)")]
        public int retentionCount = 5;

        [Header("Message Formatting")]
        [Tooltip("Default message format mode")]
        public MessageFormatMode defaultFormatMode = MessageFormatMode.Text;

        [Tooltip("Default text template for message formatting")]
        [TextArea(3, 5)]
        public string defaultTextTemplate = "{timestamp} [{level}] {category}: {message}";

        [Header("Performance")]
        [Tooltip("Buffer size for file writes (0 = unbuffered)")]
        public int fileBufferSize = 4096;

        [Header("Live Reload")]
        [Tooltip("Enable automatic reload of settings changes at runtime")]
        public bool enableLiveReload = true;

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
            settings.enableLogRotation = true;
            settings.maxFileSizeMB = 10;
            settings.retentionCount = 5;
            settings.defaultFormatMode = MessageFormatMode.Text;
            settings.defaultTextTemplate = "{timestamp} [{level}] {category}: {message}";
            settings.fileBufferSize = 4096;
            settings.enableLiveReload = true;
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

    /// <summary>
    /// Per-category minimum level override configuration.
    /// </summary>
    [Serializable]
    public class CategoryMinLevelOverride
    {
        [Tooltip("Category name to apply the minimum level to")]
        public string categoryName;

        [Tooltip("Minimum log level for this category")]
        public LogLevel minimumLevel = LogLevel.Info;
    }
}