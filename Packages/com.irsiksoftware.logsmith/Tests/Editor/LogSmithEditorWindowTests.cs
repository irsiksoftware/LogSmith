using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using IrsikSoftware.LogSmith.Editor;

namespace IrsikSoftware.LogSmith.Tests.Editor
{
    /// <summary>
    /// Tests for LogSmithEditorWindow functionality.
    /// </summary>
    public class LogSmithEditorWindowTests
    {
        private LoggingSettings _testSettings;

        [SetUp]
        public void SetUp()
        {
            _testSettings = ScriptableObject.CreateInstance<LoggingSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testSettings != null)
            {
                Object.DestroyImmediate(_testSettings);
            }
        }

        [Test]
        public void CategoryDefinition_SerializesCorrectly()
        {
            var category = new CategoryDefinition
            {
                categoryName = "TestCategory",
                color = Color.red,
                minimumLevel = LogLevel.Warn,
                enabled = true
            };

            Assert.AreEqual("TestCategory", category.categoryName);
            Assert.AreEqual(Color.red, category.color);
            Assert.AreEqual(LogLevel.Warn, category.minimumLevel);
            Assert.IsTrue(category.enabled);
        }

        [Test]
        public void CategoryTemplateOverride_SerializesCorrectly()
        {
            var templateOverride = new CategoryTemplateOverride
            {
                categoryName = "TestCategory",
                template = "{timestamp} {level} {message}",
                useJsonFormat = true
            };

            Assert.AreEqual("TestCategory", templateOverride.categoryName);
            Assert.AreEqual("{timestamp} {level} {message}", templateOverride.template);
            Assert.IsTrue(templateOverride.useJsonFormat);
        }

        [Test]
        public void LoggingSettings_CanAddCategories()
        {
            _testSettings.categories.Add(new CategoryDefinition
            {
                categoryName = "Category1",
                color = Color.blue,
                minimumLevel = LogLevel.Debug,
                enabled = true
            });

            _testSettings.categories.Add(new CategoryDefinition
            {
                categoryName = "Category2",
                color = Color.green,
                minimumLevel = LogLevel.Info,
                enabled = false
            });

            Assert.AreEqual(2, _testSettings.categories.Count);
            Assert.AreEqual("Category1", _testSettings.categories[0].categoryName);
            Assert.AreEqual("Category2", _testSettings.categories[1].categoryName);
        }

        [Test]
        public void LoggingSettings_CanAddTemplateOverrides()
        {
            _testSettings.categoryTemplateOverrides.Add(new CategoryTemplateOverride
            {
                categoryName = "Category1",
                template = "{level}: {message}",
                useJsonFormat = false
            });

            Assert.AreEqual(1, _testSettings.categoryTemplateOverrides.Count);
            Assert.AreEqual("Category1", _testSettings.categoryTemplateOverrides[0].categoryName);
            Assert.AreEqual("{level}: {message}", _testSettings.categoryTemplateOverrides[0].template);
        }

        [Test]
        public void LoggingSettings_DefaultsAreCorrect()
        {
            var settings = LoggingSettings.CreateDefault();

            Assert.AreEqual(LogLevel.Debug, settings.minimumLogLevel);
            Assert.IsTrue(settings.enableConsoleSink);
            Assert.IsFalse(settings.enableFileSink);
            Assert.AreEqual("Logs/logsmith.log", settings.logFilePath);
            Assert.IsTrue(settings.enableLogRotation);
            Assert.AreEqual(10, settings.maxFileSizeMB);
            Assert.AreEqual(5, settings.retentionCount);
            Assert.AreEqual(MessageFormatMode.Text, settings.defaultFormatMode);
            Assert.AreEqual("{timestamp} [{level}] {category}: {message}", settings.defaultTextTemplate);
            Assert.AreEqual(4096, settings.fileBufferSize);
            Assert.IsTrue(settings.enableLiveReload);

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void LoggingSettings_SettingsChangedEventFires()
        {
            bool eventFired = false;
            _testSettings.SettingsChanged += () => eventFired = true;

            _testSettings.TriggerSettingsChanged();

            Assert.IsTrue(eventFired);
        }

        [Test]
        public void CategoryDefinition_DefaultValuesAreReasonable()
        {
            var category = new CategoryDefinition();

            Assert.AreEqual(Color.white, category.color);
            Assert.AreEqual(LogLevel.Debug, category.minimumLevel);
            Assert.IsTrue(category.enabled);
        }

        [Test]
        public void LoggingSettings_CanStoreMixedConfiguration()
        {
            // Add categories
            _testSettings.categories.Add(new CategoryDefinition
            {
                categoryName = "Gameplay",
                color = Color.cyan,
                minimumLevel = LogLevel.Info,
                enabled = true
            });

            _testSettings.categories.Add(new CategoryDefinition
            {
                categoryName = "Network",
                color = Color.magenta,
                minimumLevel = LogLevel.Debug,
                enabled = true
            });

            // Add template overrides
            _testSettings.categoryTemplateOverrides.Add(new CategoryTemplateOverride
            {
                categoryName = "Network",
                template = "[NET] {timestamp:HH:mm:ss.fff} {message}",
                useJsonFormat = false
            });

            // Verify
            Assert.AreEqual(2, _testSettings.categories.Count);
            Assert.AreEqual(1, _testSettings.categoryTemplateOverrides.Count);
            Assert.AreEqual("Gameplay", _testSettings.categories[0].categoryName);
            Assert.AreEqual("Network", _testSettings.categories[1].categoryName);
            Assert.AreEqual("Network", _testSettings.categoryTemplateOverrides[0].categoryName);
        }

        [Test]
        public void LoggingSettings_VisualDebugDefaultsCorrectly()
        {
            var settings = LoggingSettings.CreateDefault();

            Assert.IsFalse(settings.enableVisualDebug, "Visual debug should be disabled by default");

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void LoggingSettings_CanEnableVisualDebug()
        {
            _testSettings.enableVisualDebug = true;

            Assert.IsTrue(_testSettings.enableVisualDebug);
        }

        [Test]
        public void VisualDebugTab_CanBeInstantiated()
        {
            var tab = new VisualDebugTab();

            Assert.IsNotNull(tab);
        }

        [Test]
        [Ignore("GUI drawing tests require Unity Editor Window OnGUI context which is not available in unit tests")]
        public void VisualDebugTab_CanDrawWithoutErrors()
        {
            var tab = new VisualDebugTab();
            var serializedSettings = new SerializedObject(_testSettings);

            // This should not throw
            Assert.DoesNotThrow(() => tab.Draw(serializedSettings, _testSettings));
        }

        [Test]
        public void SinksTab_CanBeInstantiated()
        {
            var tab = new SinksTab();

            Assert.IsNotNull(tab);
        }

        [Test]
        [Ignore("GUI drawing tests require Unity Editor Window OnGUI context which is not available in unit tests")]
        public void SinksTab_CanDrawWithoutErrors()
        {
            var tab = new SinksTab();
            var serializedSettings = new SerializedObject(_testSettings);

            // This should not throw
            Assert.DoesNotThrow(() => tab.Draw(serializedSettings, _testSettings));
        }

        [Test]
        [Ignore("GUI drawing tests require Unity Editor Window OnGUI context which is not available in unit tests")]
        public void SinksTab_ShowsFileSinkPlatformWarnings()
        {
            var tab = new SinksTab();
            _testSettings.enableFileSink = true;
            var serializedSettings = new SerializedObject(_testSettings);

            // Draw should work regardless of build target - warnings are platform-specific
            Assert.DoesNotThrow(() => tab.Draw(serializedSettings, _testSettings));
        }
    }
}
