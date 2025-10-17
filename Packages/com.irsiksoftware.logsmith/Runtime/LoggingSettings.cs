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
        /// <summary>
        /// Event raised when settings change in editor (OnValidate).
        /// </summary>
        public event Action SettingsChanged;

        [Header("General Settings")]
        [Tooltip("Minimum log level for all categories (unless overridden per-category)")]
        public LogLevel minimumLogLevel = LogLevel.Debug;

        [Tooltip("Per-category minimum level overrides")]
        public List<CategoryMinLevelOverride> categoryMinLevelOverrides = new List<CategoryMinLevelOverride>();

        [Header("Category Definitions")]
        [Tooltip("All registered categories with metadata (color, enabled state, minimum level)")]
        public List<CategoryDefinition> categories = new List<CategoryDefinition>();

        [Header("Sink Settings")]
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

        [Tooltip("Per-category template overrides")]
        public List<CategoryTemplateOverride> categoryTemplateOverrides = new List<CategoryTemplateOverride>();

        [Header("Performance")]
        [Tooltip("Buffer size for file writes (0 = unbuffered)")]
        public int fileBufferSize = 4096;

        [Header("Debug Overlay")]
        [Tooltip("Enable in-game debug overlay (toggle with F1)")]
        public bool enableDebugOverlay = false;

        [Tooltip("Maximum number of log entries in overlay buffer")]
        public int overlayMaxLogCount = 500;

        [Header("Visual Debug Rendering")]
        [Tooltip("Enable visual debug shape rendering (lines, quads) using render pipeline adapters")]
        public bool enableVisualDebug = false;

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
            settings.enableDebugOverlay = false;
            settings.overlayMaxLogCount = 500;
            settings.enableVisualDebug = false;
            settings.enableLiveReload = true;
            return settings;
        }

        /// <summary>
        /// Manually triggers settings changed event (useful for testing).
        /// </summary>
        public void TriggerSettingsChanged()
        {
            SettingsChanged?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called when values change in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (enableLiveReload)
            {
                TriggerSettingsChanged();
            }
        }
#endif
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

    /// <summary>
    /// Category definition with full metadata for Editor UI.
    /// </summary>
    [Serializable]
    public class CategoryDefinition
    {
        [Tooltip("Category name")]
        public string categoryName;

        [Tooltip("Display color for this category")]
        public Color color = Color.white;

        [Tooltip("Minimum log level for this category")]
        public LogLevel minimumLevel = LogLevel.Debug;

        [Tooltip("Whether this category is enabled")]
        public bool enabled = true;
    }

    /// <summary>
    /// Per-category message template override.
    /// </summary>
    [Serializable]
    public class CategoryTemplateOverride
    {
        [Tooltip("Category name to apply the template to")]
        public string categoryName;

        [Tooltip("Custom template for this category")]
        [TextArea(2, 4)]
        public string template;

        [Tooltip("Whether to use JSON format for this category")]
        public bool useJsonFormat;
    }
}