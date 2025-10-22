using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using IrsikSoftware.LogSmith.Editor;

namespace IrsikSoftware.LogSmith.Tests.Editor
{
    /// <summary>
    /// Tests for LiveLogConsoleWindow functionality.
    /// </summary>
    public class LiveLogConsoleWindowTests
    {
        [Test]
        public void LiveLogConsoleWindow_CanBeInstantiated()
        {
            var window = ScriptableObject.CreateInstance<LiveLogConsoleWindow>();

            Assert.IsNotNull(window);

            Object.DestroyImmediate(window);
        }

        [Test]
        public void LiveLogConsoleWindow_MenuItemExists()
        {
            // Skip in batch mode - ExecuteMenuItem requires a graphics device to show windows
            if (Application.isBatchMode)
            {
                Assert.Ignore("This test requires a graphics device and cannot run in batch mode (CI).");
                return;
            }

            // Verify the menu item is registered
            var menuItemPath = "Window/LogSmith/Live Log Console";

            // This will throw if the menu item doesn't exist
            Assert.DoesNotThrow(() =>
            {
                EditorApplication.ExecuteMenuItem(menuItemPath);
            });
        }
    }
}
