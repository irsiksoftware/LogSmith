using System;
using System.Linq;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Configuration provider backed by LoggingSettings ScriptableObject with live reload support.
    /// </summary>
    public class ScriptableObjectConfigProvider : ILogConfigProvider
    {
        private readonly LoggingSettings _settings;
        private readonly LogConfigProvider _baseProvider;

        public ScriptableObjectConfigProvider(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _baseProvider = new LogConfigProvider();

            // Subscribe to settings changes for live reload
            _settings.SettingsChanged += OnSettingsChanged;

            // Initialize with settings
            ReloadConfig();
        }

        private void OnSettingsChanged()
        {
            ReloadConfig();
        }

        public LogConfig GetConfig()
        {
            return _baseProvider.GetConfig();
        }

        public void ReloadConfig()
        {
            var config = ConvertToLogConfig(_settings);
            _baseProvider.UpdateConfig(config);
        }

        public IDisposable Subscribe(Action<LogConfig> onConfigChanged)
        {
            return _baseProvider.Subscribe(onConfigChanged);
        }

        private LogConfig ConvertToLogConfig(LoggingSettings settings)
        {
            var config = new LogConfig
            {
                DefaultMinimumLevel = settings.minimumLogLevel,
                EnableConsoleSink = settings.enableConsoleSink,
                EnableFileSink = settings.enableFileSink,
                LogFilePath = settings.logFilePath,
                EnableLogRotation = settings.enableLogRotation,
                MaxFileSizeMB = settings.maxFileSizeMB,
                RetentionCount = settings.retentionCount,
                DefaultFormatMode = settings.defaultFormatMode,
                DefaultTemplate = settings.defaultTextTemplate,
                FileBufferSize = settings.fileBufferSize,
                EnableLiveReload = settings.enableLiveReload
            };

            // Convert category overrides
            foreach (var categoryOverride in settings.categoryMinLevelOverrides ?? Enumerable.Empty<CategoryMinLevelOverride>())
            {
                if (!string.IsNullOrWhiteSpace(categoryOverride.categoryName))
                {
                    config.CategoryMinLevels[categoryOverride.categoryName] = categoryOverride.minimumLevel;
                }
            }

            return config;
        }
    }
}
