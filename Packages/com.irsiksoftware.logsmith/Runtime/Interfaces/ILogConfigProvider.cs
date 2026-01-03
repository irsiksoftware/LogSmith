using System;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Provides logging configuration with runtime reload support.
    /// </summary>
    public interface ILogConfigProvider
    {
        /// <summary>
        /// Gets the current logging configuration.
        /// </summary>
        LogConfig GetConfig();

        /// <summary>
        /// Reloads the configuration from its source.
        /// </summary>
        void ReloadConfig();

        /// <summary>
        /// Subscribes to configuration change notifications.
        /// </summary>
        IDisposable Subscribe(Action<LogConfig> onConfigChanged);
    }

    /// <summary>
    /// Logging configuration data.
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// Default minimum log level for all categories.
        /// </summary>
        public LogLevel DefaultMinimumLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Per-category minimum level overrides.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, LogLevel> CategoryMinLevels { get; set; } = new System.Collections.Generic.Dictionary<string, LogLevel>();

        /// <summary>
        /// Whether console sink is enabled.
        /// </summary>
        public bool EnableConsoleSink { get; set; } = true;

        /// <summary>
        /// Whether file sink is enabled.
        /// </summary>
        public bool EnableFileSink { get; set; } = true;

        /// <summary>
        /// File path for file sink output (relative to Application.persistentDataPath).
        /// </summary>
        public string LogFilePath { get; set; } = "Logs/logsmith.log";

        /// <summary>
        /// Enable log file rotation.
        /// </summary>
        public bool EnableLogRotation { get; set; } = true;

        /// <summary>
        /// Maximum file size in MB before rotation (0 = no limit).
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 10;

        /// <summary>
        /// Number of archived log files to retain (0 = keep all).
        /// </summary>
        public int RetentionCount { get; set; } = 5;

        /// <summary>
        /// Default message format mode (Text or JSON).
        /// </summary>
        public MessageFormatMode DefaultFormatMode { get; set; } = MessageFormatMode.Text;

        /// <summary>
        /// Default text template for message formatting.
        /// </summary>
        public string DefaultTemplate { get; set; } = "{timestamp} [{level}] {category}: {message}";

        /// <summary>
        /// Buffer size for file writes (0 = unbuffered).
        /// </summary>
        public int FileBufferSize { get; set; } = 4096;

        /// <summary>
        /// Enable automatic reload of settings changes at runtime.
        /// </summary>
        public bool EnableLiveReload { get; set; } = true;
    }
}
