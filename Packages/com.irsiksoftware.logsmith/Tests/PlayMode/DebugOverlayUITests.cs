using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Profiling;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode UI tests for DebugOverlayController.
    /// Part of issue #25: Overlay UI Tests
    /// Validates filter toggles, category switching, throttled refresh, and no GC spikes.
    /// </summary>
    [TestFixture]
    public class DebugOverlayUITests
    {
        private GameObject _overlayGameObject;
        private DebugOverlayController _overlay;
        private LogRouter _router;
        private TestLogSink _sink;

        [SetUp]
        public void Setup()
        {
            _router = new LogRouter();
            _sink = new TestLogSink("TestSink");
            _router.RegisterSink(_sink);

            _overlayGameObject = new GameObject("DebugOverlay");
            _overlay = _overlayGameObject.AddComponent<DebugOverlayController>();
            _overlay.Initialize(_router);
        }

        [TearDown]
        public void TearDown()
        {
            if (_overlayGameObject != null)
            {
                UnityEngine.Object.Destroy(_overlayGameObject);
            }
            _router = null;
            _sink = null;
        }

        [UnityTest]
        public IEnumerator FilterToggle_ChangingMinLevel_FiltersCorrectly()
        {
            // Arrange - Log messages at different levels
            var logger = new LogSmithLogger(_router, "FilterTest");

            logger.Trace("Trace message");
            logger.Debug("Debug message");
            logger.Info("Info message");
            logger.Warn("Warn message");
            logger.Error("Error message");

            yield return null; // Wait one frame for overlay to process
            yield return new WaitForSeconds(0.05f); // Wait for throttled update

            // Act - Access overlay internals via reflection to verify filtering
            var overlayType = typeof(DebugOverlayController);
            var filteredLogsField = overlayType.GetField("_filteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var minDisplayLevelField = overlayType.GetField("_minDisplayLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateMethod = overlayType.GetMethod("UpdateFilteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test filtering at Warn level
            minDisplayLevelField.SetValue(_overlay, LogLevel.Warn);
            updateMethod.Invoke(_overlay, null);

            var filteredLogs = (List<LogMessage>)filteredLogsField.GetValue(_overlay);

            // Assert - Should only show Warn and Error
            Assert.AreEqual(2, filteredLogs.Count, "Should show 2 messages (Warn, Error) when min level is Warn");
            Assert.IsTrue(filteredLogs[0].Message.Contains("Warn"), "First message should be Warn");
            Assert.IsTrue(filteredLogs[1].Message.Contains("Error"), "Second message should be Error");
        }

        [UnityTest]
        public IEnumerator CategorySwitching_SelectingCategory_ShowsOnlyThatCategory()
        {
            // Arrange - Log to multiple categories
            var logger1 = new LogSmithLogger(_router, "CategoryA");
            var logger2 = new LogSmithLogger(_router, "CategoryB");
            var logger3 = new LogSmithLogger(_router, "CategoryC");

            logger1.Info("Message from A");
            logger2.Info("Message from B");
            logger3.Info("Message from C");
            logger1.Info("Another from A");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Filter by CategoryA
            var overlayType = typeof(DebugOverlayController);
            var selectedCategoryField = overlayType.GetField("_selectedCategory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var filteredLogsField = overlayType.GetField("_filteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateMethod = overlayType.GetMethod("UpdateFilteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            selectedCategoryField.SetValue(_overlay, "CategoryA");
            updateMethod.Invoke(_overlay, null);

            var filteredLogs = (List<LogMessage>)filteredLogsField.GetValue(_overlay);

            // Assert - Should only show CategoryA messages
            Assert.AreEqual(2, filteredLogs.Count, "Should show 2 messages from CategoryA");
            Assert.AreEqual("CategoryA", filteredLogs[0].Category);
            Assert.AreEqual("CategoryA", filteredLogs[1].Category);
        }

        [UnityTest]
        public IEnumerator SearchFilter_EnteringSearchText_FiltersMessages()
        {
            // Arrange - Log messages with different content
            var logger = new LogSmithLogger(_router, "Search");

            logger.Info("The quick brown fox");
            logger.Info("jumps over the lazy dog");
            logger.Info("The quick brown cat");
            logger.Info("runs through the forest");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Search for "quick"
            var overlayType = typeof(DebugOverlayController);
            var searchTextField = overlayType.GetField("_searchText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var filteredLogsField = overlayType.GetField("_filteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateMethod = overlayType.GetMethod("UpdateFilteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            searchTextField.SetValue(_overlay, "quick");
            updateMethod.Invoke(_overlay, null);

            var filteredLogs = (List<LogMessage>)filteredLogsField.GetValue(_overlay);

            // Assert - Should show 2 messages containing "quick"
            Assert.AreEqual(2, filteredLogs.Count, "Should show 2 messages containing 'quick'");
            Assert.IsTrue(filteredLogs[0].Message.Contains("quick"));
            Assert.IsTrue(filteredLogs[1].Message.Contains("quick"));
        }

        [UnityTest]
        public IEnumerator ThrottledRefresh_HighFrequencyLogs_ThrottlesUpdates()
        {
            // Arrange
            var logger = new LogSmithLogger(_router, "Throttle");

            // Monitor updates by tracking needsUpdate flag changes
            var overlayType = typeof(DebugOverlayController);
            var needsUpdateField = overlayType.GetField("_needsUpdate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var lastUpdateTimeField = overlayType.GetField("_lastUpdateTime",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Reset last update time to ensure throttling kicks in
            lastUpdateTimeField.SetValue(_overlay, Time.realtimeSinceStartup);

            // Act - Log 100 messages rapidly
            for (int i = 0; i < 100; i++)
            {
                logger.Info($"Rapid message {i}");
            }

            yield return null;

            // Check that update is needed but throttled
            bool needsUpdate = (bool)needsUpdateField.GetValue(_overlay);
            Assert.IsTrue(needsUpdate, "Should need update after rapid logging");

            // Wait for throttle period (1/30 second = ~33ms)
            yield return new WaitForSeconds(0.05f);

            // After throttle period, update should have occurred
            needsUpdate = (bool)needsUpdateField.GetValue(_overlay);
            Assert.IsFalse(needsUpdate, "Update should have occurred after throttle period");
        }

        [UnityTest]
        public IEnumerator CircularBuffer_ExceedingCapacity_MaintainsLimit()
        {
            // Arrange - Log more than buffer capacity (500)
            var logger = new LogSmithLogger(_router, "Buffer");

            // Act - Log 600 messages
            for (int i = 0; i < 600; i++)
            {
                logger.Info($"Message {i}");
            }

            yield return null;
            yield return new WaitForSeconds(0.1f); // Wait for processing

            // Assert - Buffer should contain only 500 messages
            var overlayType = typeof(DebugOverlayController);
            var logBufferField = overlayType.GetField("_logBuffer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var buffer = logBufferField.GetValue(_overlay) as CircularBuffer<LogMessage>;

            Assert.AreEqual(500, buffer.Count, "Buffer should be capped at 500 messages");

            // Verify oldest messages were discarded (buffer should contain 100-599)
            var allLogs = buffer.GetAll();
            Assert.IsTrue(allLogs[0].Message.Contains("Message 100"), "Oldest message should be Message 100");
            Assert.IsTrue(allLogs[499].Message.Contains("Message 599"), "Newest message should be Message 599");
        }

        [UnityTest]
        public IEnumerator CombinedFilters_LevelCategoryAndSearch_AllApplied()
        {
            // Arrange - Mixed logs
            var logger1 = new LogSmithLogger(_router, "CategoryX");
            var logger2 = new LogSmithLogger(_router, "CategoryY");

            logger1.Trace("Important trace in X");
            logger1.Info("Important info in X");
            logger1.Warn("Important warning in X");
            logger2.Info("Info in Y");
            logger2.Warn("Important warning in Y");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Apply all filters: Level=Info, Category=CategoryX, Search=Important
            var overlayType = typeof(DebugOverlayController);
            var minDisplayLevelField = overlayType.GetField("_minDisplayLevel",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var selectedCategoryField = overlayType.GetField("_selectedCategory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var searchTextField = overlayType.GetField("_searchText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var filteredLogsField = overlayType.GetField("_filteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateMethod = overlayType.GetMethod("UpdateFilteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            minDisplayLevelField.SetValue(_overlay, LogLevel.Info);
            selectedCategoryField.SetValue(_overlay, "CategoryX");
            searchTextField.SetValue(_overlay, "Important");
            updateMethod.Invoke(_overlay, null);

            var filteredLogs = (List<LogMessage>)filteredLogsField.GetValue(_overlay);

            // Assert - Should show 2 messages: Info and Warn from CategoryX containing "Important"
            Assert.AreEqual(2, filteredLogs.Count, "Should show 2 messages matching all filters");
            Assert.AreEqual("CategoryX", filteredLogs[0].Category);
            Assert.AreEqual("CategoryX", filteredLogs[1].Category);
            Assert.IsTrue(filteredLogs[0].Message.Contains("Important"));
            Assert.IsTrue(filteredLogs[1].Message.Contains("Important"));
        }

        [UnityTest]
        public IEnumerator GCAllocation_RepeatedUpdates_NoSignificantGCSpikes()
        {
            // Arrange
            var logger = new LogSmithLogger(_router, "GC");

            // Pre-warm to avoid first-time allocations
            for (int i = 0; i < 10; i++)
            {
                logger.Info($"Warmup {i}");
            }
            yield return null;
            yield return new WaitForSeconds(0.1f);

            // Force GC to get clean baseline
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            yield return null;

            // Act - Measure allocations during logging and filtering
            var startMem = Profiler.GetTotalAllocatedMemoryLong();

            // Simulate realistic workload: 100 logs with periodic filter updates
            var overlayType = typeof(DebugOverlayController);
            var updateMethod = overlayType.GetMethod("UpdateFilteredLogs",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            for (int i = 0; i < 100; i++)
            {
                logger.Info($"Test message {i}");

                // Update filters every 10 messages
                if (i % 10 == 0)
                {
                    updateMethod.Invoke(_overlay, null);
                }
            }

            yield return null;
            yield return new WaitForSeconds(0.1f);

            var endMem = Profiler.GetTotalAllocatedMemoryLong();
            var allocatedKB = (endMem - startMem) / 1024f;

            // Assert - Should not allocate excessively
            // Reasonable threshold: < 100KB for 100 messages + 10 filter updates
            Assert.Less(allocatedKB, 100f,
                $"GC allocation too high: {allocatedKB:F2} KB allocated (expected < 100 KB)");

            UnityEngine.Debug.Log($"[GC Test] Allocated {allocatedKB:F2} KB for 100 messages + 10 filter updates");
        }

        [UnityTest]
        public IEnumerator ClearBuffer_AfterLogging_RemovesAllMessages()
        {
            // Arrange
            var logger = new LogSmithLogger(_router, "Clear");

            for (int i = 0; i < 50; i++)
            {
                logger.Info($"Message {i}");
            }

            yield return null;
            yield return new WaitForSeconds(0.05f);

            var overlayType = typeof(DebugOverlayController);
            var logBufferField = overlayType.GetField("_logBuffer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var buffer = logBufferField.GetValue(_overlay) as CircularBuffer<LogMessage>;

            Assert.AreEqual(50, buffer.Count, "Should have 50 messages before clear");

            // Act - Clear buffer
            buffer.Clear();
            yield return null;

            // Assert
            Assert.AreEqual(0, buffer.Count, "Buffer should be empty after clear");
        }

        [UnityTest]
        public IEnumerator VisibilityToggle_F1Key_TogglesOverlay()
        {
            // This test validates that the overlay can be toggled (structure test)
            // Actual key input simulation would require UnityEngine.TestTools.Input package

            // Arrange - Verify overlay is initialized
            Assert.IsNotNull(_overlay, "Overlay should be initialized");

            var overlayType = typeof(DebugOverlayController);
            var isVisibleField = overlayType.GetField("_isVisible",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            bool initialVisibility = (bool)isVisibleField.GetValue(_overlay);
            Assert.IsTrue(initialVisibility, "Overlay should start visible");

            // Act - Manually toggle visibility (simulates F1 press behavior)
            isVisibleField.SetValue(_overlay, false);
            yield return null;

            // Assert
            bool newVisibility = (bool)isVisibleField.GetValue(_overlay);
            Assert.IsFalse(newVisibility, "Overlay should be hidden after toggle");

            // Toggle back
            isVisibleField.SetValue(_overlay, true);
            yield return null;

            newVisibility = (bool)isVisibleField.GetValue(_overlay);
            Assert.IsTrue(newVisibility, "Overlay should be visible after second toggle");
        }

        #region Runtime Category Management Tests (Issue #148)

        [UnityTest]
        public IEnumerator CategoryManagement_SetMinimumLevel_UpdatesRegistryLevel()
        {
            // Arrange - Initialize with CategoryRegistry
            var registry = new CategoryRegistry();
            _overlay.InitializeCategoryRegistry(registry);

            var logger = new LogSmithLogger(_router, "NetworkCategory");
            logger.Info("Test message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Change minimum level for NetworkCategory via overlay
            _overlay.SetCategoryMinimumLevel("NetworkCategory", LogLevel.Warn);

            // Assert - Registry should reflect the change
            Assert.AreEqual(LogLevel.Warn, registry.GetMinimumLevel("NetworkCategory"),
                "CategoryRegistry should have updated minimum level");
        }

        [UnityTest]
        public IEnumerator CategoryManagement_DisableCategory_UpdatesRegistryEnabled()
        {
            // Arrange - Initialize with CategoryRegistry
            var registry = new CategoryRegistry();
            _overlay.InitializeCategoryRegistry(registry);

            var logger = new LogSmithLogger(_router, "AudioCategory");
            logger.Info("Test message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Verify category is initially enabled
            Assert.IsTrue(registry.IsEnabled("AudioCategory"), "Category should start enabled");

            // Act - Disable category via overlay
            _overlay.SetCategoryEnabled("AudioCategory", false);

            // Assert - Registry should reflect the change
            Assert.IsFalse(registry.IsEnabled("AudioCategory"),
                "CategoryRegistry should have disabled the category");
        }

        [UnityTest]
        public IEnumerator CategoryManagement_EnableCategory_UpdatesRegistryEnabled()
        {
            // Arrange - Initialize with CategoryRegistry and pre-disable a category
            var registry = new CategoryRegistry();
            registry.RegisterCategory("PhysicsCategory", LogLevel.Info);
            registry.SetEnabled("PhysicsCategory", false);
            _overlay.InitializeCategoryRegistry(registry);

            var logger = new LogSmithLogger(_router, "PhysicsCategory");
            logger.Info("Test message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Verify category is initially disabled
            Assert.IsFalse(registry.IsEnabled("PhysicsCategory"), "Category should start disabled");

            // Act - Enable category via overlay
            _overlay.SetCategoryEnabled("PhysicsCategory", true);

            // Assert - Registry should reflect the change
            Assert.IsTrue(registry.IsEnabled("PhysicsCategory"),
                "CategoryRegistry should have enabled the category");
        }

        [UnityTest]
        public IEnumerator CategoryManagement_GetCategoryMetadata_ReturnsCorrectMetadata()
        {
            // Arrange
            var registry = new CategoryRegistry();
            registry.RegisterCategory("UICategory", LogLevel.Debug);
            registry.SetEnabled("UICategory", true);
            _overlay.InitializeCategoryRegistry(registry);

            var logger = new LogSmithLogger(_router, "UICategory");
            logger.Info("Test message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Get metadata from overlay
            var metadata = _overlay.GetCategoryMetadata("UICategory");

            // Assert
            Assert.AreEqual("UICategory", metadata.Name);
            Assert.AreEqual(LogLevel.Debug, metadata.MinimumLevel);
            Assert.IsTrue(metadata.Enabled);
        }

        [UnityTest]
        public IEnumerator CategoryManagement_GetManagedCategories_ReturnsVisibleCategories()
        {
            // Arrange
            var registry = new CategoryRegistry();
            _overlay.InitializeCategoryRegistry(registry);

            var logger1 = new LogSmithLogger(_router, "CategoryAlpha");
            var logger2 = new LogSmithLogger(_router, "CategoryBeta");
            var logger3 = new LogSmithLogger(_router, "CategoryGamma");

            logger1.Info("Alpha message");
            logger2.Info("Beta message");
            logger3.Info("Gamma message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Get managed categories
            var categories = _overlay.GetManagedCategories();

            // Assert - Should return all categories that have been seen
            Assert.GreaterOrEqual(categories.Count, 3, "Should have at least 3 categories");
            Assert.Contains("CategoryAlpha", (System.Collections.ICollection)categories);
            Assert.Contains("CategoryBeta", (System.Collections.ICollection)categories);
            Assert.Contains("CategoryGamma", (System.Collections.ICollection)categories);
        }

        [UnityTest]
        public IEnumerator CategoryManagement_ToggleManagementPanel_ShowsHidesCategoryControls()
        {
            // Arrange
            var registry = new CategoryRegistry();
            _overlay.InitializeCategoryRegistry(registry);

            yield return null;

            // Act & Assert - Management panel starts hidden
            Assert.IsFalse(_overlay.IsCategoryManagementPanelVisible,
                "Category management panel should start hidden");

            // Toggle on
            _overlay.ToggleCategoryManagementPanel();
            Assert.IsTrue(_overlay.IsCategoryManagementPanelVisible,
                "Category management panel should be visible after toggle");

            // Toggle off
            _overlay.ToggleCategoryManagementPanel();
            Assert.IsFalse(_overlay.IsCategoryManagementPanelVisible,
                "Category management panel should be hidden after second toggle");
        }

        [UnityTest]
        public IEnumerator CategoryManagement_ChangeLevelMultipleCategories_AllUpdateCorrectly()
        {
            // Arrange
            var registry = new CategoryRegistry();
            _overlay.InitializeCategoryRegistry(registry);

            var loggerA = new LogSmithLogger(_router, "SystemA");
            var loggerB = new LogSmithLogger(_router, "SystemB");
            var loggerC = new LogSmithLogger(_router, "SystemC");

            loggerA.Info("A message");
            loggerB.Info("B message");
            loggerC.Info("C message");

            yield return null;
            yield return new WaitForSeconds(0.05f);

            // Act - Change levels for multiple categories
            _overlay.SetCategoryMinimumLevel("SystemA", LogLevel.Trace);
            _overlay.SetCategoryMinimumLevel("SystemB", LogLevel.Error);
            _overlay.SetCategoryMinimumLevel("SystemC", LogLevel.Critical);

            // Assert - All categories should have correct levels
            Assert.AreEqual(LogLevel.Trace, registry.GetMinimumLevel("SystemA"));
            Assert.AreEqual(LogLevel.Error, registry.GetMinimumLevel("SystemB"));
            Assert.AreEqual(LogLevel.Critical, registry.GetMinimumLevel("SystemC"));
        }

        #endregion

        /// <summary>
        /// Minimal test sink for overlay testing.
        /// </summary>
        private class TestLogSink : ILogSink
        {
            public string Name { get; }
            public int WriteCount { get; private set; }

            public TestLogSink(string name)
            {
                Name = name;
            }

            public void Write(LogMessage message)
            {
                WriteCount++;
            }

            public void Flush() { }
        }
    }
}
