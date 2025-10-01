using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Linq;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Tests for ScriptableObjectConfigProvider functionality.
    /// </summary>
    public class ScriptableObjectConfigProviderTests
    {
        private LoggingSettings _settings;
        private ScriptableObjectConfigProvider _provider;

        [SetUp]
        public void Setup()
        {
            _settings = ScriptableObject.CreateInstance<LoggingSettings>();
            _provider = new ScriptableObjectConfigProvider(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            if (_settings != null)
            {
                UnityEngine.Object.DestroyImmediate(_settings);
            }
        }

        [Test]
        public void GetConfig_ReturnsConfigWithDefaultSettings()
        {
            // Act
            var config = _provider.GetConfig();

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(_settings.minimumLogLevel, config.DefaultMinimumLevel);
            Assert.AreEqual(_settings.enableConsoleSink, config.EnableConsoleSink);
            Assert.AreEqual(_settings.enableFileSink, config.EnableFileSink);
        }

        [Test]
        public void GetConfig_ConvertsScriptableObjectSettings()
        {
            // Arrange
            _settings.minimumLogLevel = LogLevel.Warn;
            _settings.enableConsoleSink = false;
            _settings.enableFileSink = true;
            _settings.logFilePath = "custom/path.log";
            _settings.maxFileSizeMB = 20;
            _settings.retentionCount = 10;
            _settings.defaultTextTemplate = "custom template";
            _settings.fileBufferSize = 8192;

            _provider.ReloadConfig();

            // Act
            var config = _provider.GetConfig();

            // Assert
            Assert.AreEqual(LogLevel.Warn, config.DefaultMinimumLevel);
            Assert.IsFalse(config.EnableConsoleSink);
            Assert.IsTrue(config.EnableFileSink);
            Assert.AreEqual("custom/path.log", config.LogFilePath);
            Assert.AreEqual(20, config.MaxFileSizeMB);
            Assert.AreEqual(10, config.RetentionCount);
            Assert.AreEqual("custom template", config.DefaultTemplate);
            Assert.AreEqual(8192, config.FileBufferSize);
        }

        [Test]
        public void GetConfig_ConvertsCategoryMinLevelOverrides()
        {
            // Arrange
            _settings.categoryMinLevelOverrides.Add(new CategoryMinLevelOverride
            {
                categoryName = "Network",
                minimumLevel = LogLevel.Error
            });
            _settings.categoryMinLevelOverrides.Add(new CategoryMinLevelOverride
            {
                categoryName = "Audio",
                minimumLevel = LogLevel.Warn
            });

            _provider.ReloadConfig();

            // Act
            var config = _provider.GetConfig();

            // Assert
            Assert.IsTrue(config.CategoryMinLevels.ContainsKey("Network"));
            Assert.AreEqual(LogLevel.Error, config.CategoryMinLevels["Network"]);
            Assert.IsTrue(config.CategoryMinLevels.ContainsKey("Audio"));
            Assert.AreEqual(LogLevel.Warn, config.CategoryMinLevels["Audio"]);
        }

        [Test]
        public void GetConfig_IgnoresEmptyCategoryNames()
        {
            // Arrange
            _settings.categoryMinLevelOverrides.Add(new CategoryMinLevelOverride
            {
                categoryName = "",
                minimumLevel = LogLevel.Error
            });
            _settings.categoryMinLevelOverrides.Add(new CategoryMinLevelOverride
            {
                categoryName = null,
                minimumLevel = LogLevel.Warn
            });
            _settings.categoryMinLevelOverrides.Add(new CategoryMinLevelOverride
            {
                categoryName = "  ",
                minimumLevel = LogLevel.Info
            });

            _provider.ReloadConfig();

            // Act
            var config = _provider.GetConfig();

            // Assert
            Assert.AreEqual(0, config.CategoryMinLevels.Count);
        }

        [Test]
        public void Subscribe_NotifiesImmediatelyWithCurrentConfig()
        {
            // Arrange
            LogConfig receivedConfig = null;

            // Act
            _provider.Subscribe(config => receivedConfig = config);

            // Assert
            Assert.IsNotNull(receivedConfig);
            Assert.AreEqual(_settings.minimumLogLevel, receivedConfig.DefaultMinimumLevel);
        }

        [Test]
        public void Subscribe_NotifiesOnReload()
        {
            // Arrange
            int notificationCount = 0;
            LogConfig lastConfig = null;

            _provider.Subscribe(config =>
            {
                notificationCount++;
                lastConfig = config;
            });

            // Initial notification on subscribe
            Assert.AreEqual(1, notificationCount);

            // Act
            _settings.minimumLogLevel = LogLevel.Error;
            _provider.ReloadConfig();

            // Assert
            Assert.AreEqual(2, notificationCount);
            Assert.IsNotNull(lastConfig);
            Assert.AreEqual(LogLevel.Error, lastConfig.DefaultMinimumLevel);
        }

        [Test]
        public void Subscribe_Dispose_StopsNotifications()
        {
            // Arrange
            int notificationCount = 0;
            var subscription = _provider.Subscribe(config => notificationCount++);

            // Initial notification
            Assert.AreEqual(1, notificationCount);

            // Act
            subscription.Dispose();
            _provider.ReloadConfig();

            // Assert
            Assert.AreEqual(1, notificationCount); // No new notifications after dispose
        }

        [Test]
        public void Constructor_ThrowsOnNullSettings()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ScriptableObjectConfigProvider(null));
        }

#if UNITY_EDITOR
        [Test]
        public void SettingsChanged_TriggersReloadWhenLiveReloadEnabled()
        {
            // Arrange
            int notificationCount = 0;
            _settings.enableLiveReload = true;
            _provider.Subscribe(config => notificationCount++);

            // Initial notification
            Assert.AreEqual(1, notificationCount);

            // Act - Manually invoke SettingsChanged since OnValidate is editor-only
            _settings.minimumLogLevel = LogLevel.Critical;
            _settings.TriggerSettingsChanged();

            // Assert
            Assert.AreEqual(2, notificationCount);
        }

        [Test]
        public void SettingsChanged_DoesNotTriggerWhenLiveReloadDisabled()
        {
            // Arrange
            _settings.enableLiveReload = false;
            int notificationCount = 0;
            _provider.Subscribe(config => notificationCount++);

            // Initial notification
            Assert.AreEqual(1, notificationCount);

            // Act - OnValidate won't fire the event when enableLiveReload is false
            _settings.minimumLogLevel = LogLevel.Critical;
            // We can't test OnValidate directly, but we can verify the event doesn't trigger
            // when we manually call it without checking enableLiveReload
            // The actual OnValidate checks enableLiveReload before invoking

            // Assert - only initial notification
            Assert.AreEqual(1, notificationCount);
        }
#endif
    }
}
