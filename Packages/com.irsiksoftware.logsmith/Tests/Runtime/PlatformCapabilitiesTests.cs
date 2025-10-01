using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Tests for PlatformCapabilities service to verify platform capability detection.
    /// </summary>
    public class PlatformCapabilitiesTests
    {
        private IPlatformCapabilities _capabilities;

        [SetUp]
        public void Setup()
        {
            _capabilities = new PlatformCapabilities();
        }

        [Test]
        public void PlatformName_ReturnsNonEmptyString()
        {
            // Act
            var platformName = _capabilities.PlatformName;

            // Assert
            Assert.IsNotNull(platformName);
            Assert.IsNotEmpty(platformName);
        }

        [Test]
        public void HasWritablePersistentDataPath_ReturnsValidValue()
        {
            // Act
            var hasWritablePath = _capabilities.HasWritablePersistentDataPath;

            // Assert - Should return a boolean value
            // The actual value depends on the compile-time platform defines
            Assert.That(hasWritablePath, Is.True | Is.False);
        }

        [Test]
        public void HasWritablePersistentDataPath_IsConsistentAcrossCalls()
        {
            // Act
            var firstCall = _capabilities.HasWritablePersistentDataPath;
            var secondCall = _capabilities.HasWritablePersistentDataPath;

            // Assert - Should return the same value consistently
            Assert.AreEqual(firstCall, secondCall);
        }

        [Test]
        public void PlatformName_IsConsistentAcrossCalls()
        {
            // Act
            var firstCall = _capabilities.PlatformName;
            var secondCall = _capabilities.PlatformName;

            // Assert - Should return the same value consistently
            Assert.AreEqual(firstCall, secondCall);
        }

#if UNITY_WEBGL
        [Test]
        public void HasWritablePersistentDataPath_ReturnsFalse_OnWebGL()
        {
            // Act
            var hasWritablePath = _capabilities.HasWritablePersistentDataPath;

            // Assert
            Assert.IsFalse(hasWritablePath, "WebGL should not have writable persistent data path");
        }

        [Test]
        public void PlatformName_ReturnsWebGL_OnWebGL()
        {
            // Act
            var platformName = _capabilities.PlatformName;

            // Assert
            Assert.AreEqual("WebGL", platformName);
        }
#endif

#if UNITY_SWITCH
        [Test]
        public void HasWritablePersistentDataPath_ReturnsFalse_OnSwitch()
        {
            // Act
            var hasWritablePath = _capabilities.HasWritablePersistentDataPath;

            // Assert
            Assert.IsFalse(hasWritablePath, "Switch should not have writable persistent data path");
        }

        [Test]
        public void PlatformName_ReturnsSwitch_OnSwitch()
        {
            // Act
            var platformName = _capabilities.PlatformName;

            // Assert
            Assert.AreEqual("Switch", platformName);
        }
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || UNITY_IOS || UNITY_ANDROID
        [Test]
        public void HasWritablePersistentDataPath_ReturnsTrue_OnSupportedPlatforms()
        {
            // Act
            var hasWritablePath = _capabilities.HasWritablePersistentDataPath;

            // Assert
            Assert.IsTrue(hasWritablePath, $"Platform {_capabilities.PlatformName} should have writable persistent data path");
        }
#endif
    }

    /// <summary>
    /// Integration tests for UnityLoggingBootstrap platform compatibility.
    /// </summary>
    public class UnityLoggingBootstrapPlatformTests
    {
        private class MockPlatformCapabilities : IPlatformCapabilities
        {
            public bool HasWritablePersistentDataPath { get; set; }
            public string PlatformName { get; set; }
        }

        [Test]
        public void Bootstrap_DisablesFileSink_WhenPlatformDoesNotSupportFileIO()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableFileSink = true;
            settings.enableConsoleSink = false;

            var mockPlatform = new MockPlatformCapabilities
            {
                HasWritablePersistentDataPath = false,
                PlatformName = "TestUnsupportedPlatform"
            };

            var router = new LogRouter();
            var templateEngine = new MessageTemplateEngine();

            // Act
            var bootstrap = new UnityLoggingBootstrap(settings, router, templateEngine, mockPlatform);

            // Assert
            Assert.IsNull(bootstrap.FileSink, "FileSink should be null on unsupported platform");
        }

        [Test]
        public void Bootstrap_EnablesFileSink_WhenPlatformSupportsFileIO()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableFileSink = true;
            settings.enableConsoleSink = false;
            settings.logFilePath = "test.log";

            var mockPlatform = new MockPlatformCapabilities
            {
                HasWritablePersistentDataPath = true,
                PlatformName = "TestSupportedPlatform"
            };

            var router = new LogRouter();
            var templateEngine = new MessageTemplateEngine();

            // Act
            var bootstrap = new UnityLoggingBootstrap(settings, router, templateEngine, mockPlatform);

            // Assert
            Assert.IsNotNull(bootstrap.FileSink, "FileSink should be created on supported platform");

            // Cleanup
            bootstrap.Dispose();
        }

        [Test]
        public void Bootstrap_WarnsUser_WhenFileSinkRequestedOnUnsupportedPlatform()
        {
            // Arrange
            var settings = LoggingSettings.CreateDefault();
            settings.enableFileSink = true;

            var mockPlatform = new MockPlatformCapabilities
            {
                HasWritablePersistentDataPath = false,
                PlatformName = "UnsupportedTestPlatform"
            };

            var router = new LogRouter();
            var templateEngine = new MessageTemplateEngine();

            // Act & Assert
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*File sink is not supported on.*UnsupportedTestPlatform.*"));

            var bootstrap = new UnityLoggingBootstrap(settings, router, templateEngine, mockPlatform);

            // Cleanup
            bootstrap.Dispose();
        }
    }
}
