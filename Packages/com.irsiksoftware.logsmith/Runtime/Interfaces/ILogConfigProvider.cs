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
        /// Default message template.
        /// </summary>
        public string DefaultTemplate { get; set; } = "[{Timestamp:HH:mm:ss}] [{Level}] [{Category}] {Message}";

        /// <summary>
        /// Whether file sink is enabled.
        /// </summary>
        public bool EnableFileSink { get; set; } = true;

        /// <summary>
        /// Whether console sink is enabled.
        /// </summary>
        public bool EnableConsoleSink { get; set; } = true;

        /// <summary>
        /// File path for file sink output.
        /// </summary>
        public string LogFilePath { get; set; } = "Logs/logsmith.log";
    }
}