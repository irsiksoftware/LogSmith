using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Automatically adds demo scripts to GameObjects after samples are imported.
    /// Access via: Tools > LogSmith > Auto-Add Demo Scripts
    /// </summary>
    public static class AutoAddDemoScriptsUtility
    {
        [MenuItem("Tools/LogSmith/Auto-Add Demo Scripts")]
        public static void AutoAddDemoScripts()
        {
            // Find the demo parent GameObject
            GameObject demoParent = GameObject.Find("LogSmith Demo Examples");

            if (demoParent == null)
            {
                EditorUtility.DisplayDialog(
                    "Demo Parent Not Found",
                    "Could not find 'LogSmith Demo Examples' GameObject.\n\n" +
                    "Please run 'Tools → LogSmith → Setup Demo Scene' first.",
                    "OK");
                return;
            }

            int scriptsAdded = 0;
            int scriptsFailed = 0;

            // Try to add each script
            scriptsAdded += TryAddScript(demoParent, "1. BasicLogging", "IrsikSoftware.LogSmith.Samples.BasicLoggingExample", ref scriptsFailed);
            scriptsAdded += TryAddScript(demoParent, "2. CategoryLogging", "IrsikSoftware.LogSmith.Samples.CategoryLoggingExample", ref scriptsFailed);
            scriptsAdded += TryAddScript(demoParent, "3. GameSystems", "IrsikSoftware.LogSmith.Samples.GameSystemsExample", ref scriptsFailed);
            scriptsAdded += TryAddScript(demoParent, "4. Performance", "IrsikSoftware.LogSmith.Samples.PerformanceLoggingExample", ref scriptsFailed);
            scriptsAdded += TryAddScript(demoParent, "5. Player (Press 1-5)", "IrsikSoftware.LogSmith.Samples.PlayerActionExample", ref scriptsFailed);
            scriptsAdded += TryAddScript(demoParent, "6. LogLevels (Use Context Menu)", "IrsikSoftware.LogSmith.Samples.LogLevelExample", ref scriptsFailed);

            // Show results
            if (scriptsFailed > 0)
            {
                EditorUtility.DisplayDialog(
                    "Demo Scripts Partially Added",
                    $"Added {scriptsAdded} scripts successfully.\n" +
                    $"Failed to add {scriptsFailed} scripts.\n\n" +
                    "Failed scripts are likely because:\n" +
                    "1. Demo sample not imported yet (Window → Package Manager → LogSmith → Samples → Demo → Import)\n" +
                    "2. Scripts already added to GameObjects\n\n" +
                    "If you haven't imported the Demo sample, do that first!",
                    "OK");
            }
            else if (scriptsAdded > 0)
            {
                EditorUtility.DisplayDialog(
                    "Demo Scripts Added Successfully!",
                    $"Successfully added {scriptsAdded} demo scripts!\n\n" +
                    "Ready to test:\n" +
                    "1. Press Play\n" +
                    "2. Toggle GameObjects on/off in Hierarchy\n" +
                    "3. Press keys 1-5 for Player actions\n" +
                    "4. Right-click LogLevels script → Use context menu\n\n" +
                    "Open Window → LogSmith → Settings to configure categories!",
                    "Let's Go!");

                // Expand the demo parent in hierarchy
                EditorGUIUtility.PingObject(demoParent);
                Selection.activeGameObject = demoParent;
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Scripts Added",
                    "No scripts were added. They may already be present on the GameObjects.\n\n" +
                    "Check that:\n" +
                    "1. Demo sample is imported\n" +
                    "2. GameObjects don't already have the scripts",
                    "OK");
            }
        }

        private static int TryAddScript(GameObject parent, string childName, string scriptTypeName, ref int failCount)
        {
            // Find the child GameObject
            Transform child = parent.transform.Find(childName);
            if (child == null)
            {
                Debug.LogWarning($"Child GameObject '{childName}' not found under '{parent.name}'");
                failCount++;
                return 0;
            }

            GameObject go = child.gameObject;

            // Try to find the script type by name
            Type scriptType = FindTypeByName(scriptTypeName);

            if (scriptType == null)
            {
                Debug.LogWarning($"Script type '{scriptTypeName}' not found. Make sure Demo sample is imported!");
                failCount++;
                return 0;
            }

            // Check if component already exists
            if (go.GetComponent(scriptType) != null)
            {
                Debug.Log($"Script '{scriptType.Name}' already exists on '{go.name}'");
                return 0;
            }

            // Add the component
            Undo.AddComponent(go, scriptType);
            Debug.Log($"✓ Added '{scriptType.Name}' to '{go.name}'");
            return 1;
        }

        private static Type FindTypeByName(string fullTypeName)
        {
            // Search all assemblies for the type
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        [MenuItem("Tools/LogSmith/Remove All Demo Scripts")]
        public static void RemoveAllDemoScripts()
        {
            if (!EditorUtility.DisplayDialog(
                "Remove Demo Scripts",
                "This will remove all LogSmith demo scripts from the scene.\n\nAre you sure?",
                "Remove Scripts",
                "Cancel"))
            {
                return;
            }

            GameObject demoParent = GameObject.Find("LogSmith Demo Examples");

            if (demoParent == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find 'LogSmith Demo Examples' GameObject.", "OK");
                return;
            }

            int removed = 0;

            // Find and remove all sample scripts
            var scripts = demoParent.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null && script.GetType().Namespace == "IrsikSoftware.LogSmith.Samples")
                {
                    Undo.DestroyObjectImmediate(script);
                    removed++;
                }
            }

            EditorUtility.DisplayDialog(
                "Scripts Removed",
                $"Removed {removed} demo scripts from the scene.",
                "OK");
        }
    }
}
