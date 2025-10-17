using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Automatically adds demo scripts to GameObjects after samples are imported.
    /// Access via: Tools > LogSmith > Demo Scene Setup > Auto-Add Scripts
    /// </summary>
    public static class AutoAddDemoScriptsUtility
    {
        [MenuItem("Tools/LogSmith/Demo Scene Setup/Auto-Add Scripts", priority = 20)]
        public static void AutoAddDemoScripts()
        {
            // Find the demo parent GameObject
            GameObject demoParent = GameObject.Find("LogSmith Demo Examples");

            if (demoParent == null)
            {
                EditorUtility.DisplayDialog(
                    "Demo Parent Not Found",
                    "Could not find 'LogSmith Demo Examples' GameObject.\n\n" +
                    "Please run 'Tools → LogSmith → Demo Scene Setup → Setup All Demo GameObjects' first.",
                    "OK");
                return;
            }

            int scriptsAdded = 0;
            var missingSamples = new HashSet<string>();
            var failedScripts = new List<string>();

            // Try to add scripts for each sample category
            TryAddBasicUsageScripts(demoParent, ref scriptsAdded, missingSamples, failedScripts);
            TryAddComprehensiveDemoScripts(demoParent, ref scriptsAdded, missingSamples, failedScripts);
            TryAddVContainerScripts(demoParent, ref scriptsAdded, missingSamples, failedScripts);
            TryAddRenderPipelineScripts(demoParent, ref scriptsAdded, missingSamples, failedScripts);

            // Show results
            ShowResultsDialog(scriptsAdded, missingSamples, failedScripts, demoParent);
        }

        private static void TryAddBasicUsageScripts(GameObject root, ref int added, HashSet<string> missingSamples, List<string> failedScripts)
        {
            Transform basicUsage = root.transform.Find("Basic Usage Sample");
            if (basicUsage != null)
            {
                if (TryAddScriptSafe(basicUsage.gameObject, "BasicLoggingExample", "IrsikSoftware.LogSmith.Samples.BasicLoggingExample"))
                {
                    added++;
                }
                else
                {
                    missingSamples.Add("Basic Usage");
                    failedScripts.Add("BasicLoggingExample");
                }
            }
        }

        private static void TryAddComprehensiveDemoScripts(GameObject root, ref int added, HashSet<string> missingSamples, List<string> failedScripts)
        {
            Transform demoParent = root.transform.Find("Comprehensive Demo Sample");
            if (demoParent != null)
            {
                var scripts = new Dictionary<string, string>
                {
                    { "1. BasicLogging", "IrsikSoftware.LogSmith.Samples.BasicLoggingExample" },
                    { "2. CategoryLogging", "IrsikSoftware.LogSmith.Samples.CategoryLoggingExample" },
                    { "3. GameSystems", "IrsikSoftware.LogSmith.Samples.GameSystemsExample" },
                    { "4. Performance", "IrsikSoftware.LogSmith.Samples.PerformanceLoggingExample" },
                    { "5. Player (Press 1-5)", "IrsikSoftware.LogSmith.Samples.PlayerActionExample" },
                    { "6. LogLevels (Use Context Menu)", "IrsikSoftware.LogSmith.Samples.LogLevelExample" },
                    { "7. VContainerLogging", "IrsikSoftware.LogSmith.Samples.VContainerLoggingExample" }
                };

                bool anyFailed = false;
                foreach (var kvp in scripts)
                {
                    Transform child = demoParent.Find(kvp.Key);
                    if (child != null)
                    {
                        if (TryAddScriptSafe(child.gameObject, kvp.Key, kvp.Value))
                        {
                            added++;
                        }
                        else
                        {
                            anyFailed = true;
                            failedScripts.Add(kvp.Key);
                        }
                    }
                }

                if (anyFailed)
                {
                    missingSamples.Add("Comprehensive Demo");
                }
            }
        }

        private static void TryAddVContainerScripts(GameObject root, ref int added, HashSet<string> missingSamples, List<string> failedScripts)
        {
            Transform vcontainerParent = root.transform.Find("VContainer Integration Sample");
            if (vcontainerParent != null)
            {
                Transform lifetimeScope = vcontainerParent.Find("GameLifetimeScope");
                if (lifetimeScope != null)
                {
                    // Try to add GameManager script
                    if (TryAddScriptSafe(lifetimeScope.gameObject, "GameManager", "IrsikSoftware.LogSmith.Samples.GameManager"))
                    {
                        added++;
                    }
                    else
                    {
                        missingSamples.Add("VContainer Integration");
                        failedScripts.Add("GameManager");
                    }
                }
            }
        }

        private static void TryAddRenderPipelineScripts(GameObject root, ref int added, HashSet<string> missingSamples, List<string> failedScripts)
        {
            Transform rpParent = root.transform.Find("Render Pipeline Demos");
            if (rpParent != null)
            {
                // Built-in RP
                Transform builtIn = rpParent.Find("Built-in RP Demo");
                if (builtIn != null)
                {
                    if (TryAddScriptSafe(builtIn.gameObject, "BuiltInRPDemo", "IrsikSoftware.LogSmith.Samples.BuiltInRPDemo"))
                    {
                        added++;
                    }
                    else
                    {
                        missingSamples.Add("Built-in RP Demo");
                        failedScripts.Add("BuiltInRPDemo");
                    }
                }

                // URP
                Transform urp = rpParent.Find("URP Demo");
                if (urp != null)
                {
                    if (TryAddScriptSafe(urp.gameObject, "URPDemo", "IrsikSoftware.LogSmith.Samples.URPDemo"))
                    {
                        added++;
                    }
                    else
                    {
                        missingSamples.Add("URP Demo");
                        failedScripts.Add("URPDemo");
                    }
                }

                // HDRP
                Transform hdrp = rpParent.Find("HDRP Demo");
                if (hdrp != null)
                {
                    if (TryAddScriptSafe(hdrp.gameObject, "HDRPDemo", "IrsikSoftware.LogSmith.Samples.HDRPDemo"))
                    {
                        added++;
                    }
                    else
                    {
                        missingSamples.Add("HDRP Demo");
                        failedScripts.Add("HDRPDemo");
                    }
                }
            }
        }

        private static bool TryAddScriptSafe(GameObject target, string childName, string scriptTypeName)
        {
            GameObject go = target;

            // If childName is provided and different from target name, find child
            if (childName != target.name)
            {
                Transform child = target.transform.Find(childName);
                if (child == null) return false;
                go = child.gameObject;
            }

            // Try to find the script type
            Type scriptType = FindTypeByName(scriptTypeName);
            if (scriptType == null) return false;

            // Check if component already exists
            if (go.GetComponent(scriptType) != null)
            {
                Debug.Log($"Script '{scriptType.Name}' already exists on '{go.name}'");
                return true; // Not a failure - already added
            }

            // Add the component
            Undo.AddComponent(go, scriptType);
            Debug.Log($"✓ Added '{scriptType.Name}' to '{go.name}'");
            return true;
        }

        private static void ShowResultsDialog(int scriptsAdded, HashSet<string> missingSamples, List<string> failedScripts, GameObject demoParent)
        {
            if (missingSamples.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Added {scriptsAdded} scripts successfully.");
                sb.AppendLine($"Failed to add {failedScripts.Count} scripts.\n");
                sb.AppendLine("Missing samples detected:");

                foreach (var sampleName in missingSamples.OrderBy(s => s))
                {
                    sb.AppendLine($"  • {sampleName}");
                }

                sb.AppendLine("\nImport missing samples from:");
                sb.AppendLine("Window → Package Manager → LogSmith → Samples tab");

                EditorUtility.DisplayDialog(
                    "Demo Scripts Partially Added",
                    sb.ToString(),
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
                    "3. Press F1 for debug overlay\n" +
                    "4. Press keys 1-5 for Player actions\n" +
                    "5. Right-click LogLevels script → Use context menu\n\n" +
                    "Open Window → LogSmith → Settings to configure categories!",
                    "Let's Go!");

                EditorGUIUtility.PingObject(demoParent);
                Selection.activeGameObject = demoParent;
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No Scripts Added",
                    "No scripts were added. They may already be present on the GameObjects.\n\n" +
                    "Check that:\n" +
                    "1. Required samples are imported from Package Manager\n" +
                    "2. GameObjects don't already have the scripts\n" +
                    "3. GameObjects were created using the Demo Scene Setup menu",
                    "OK");
            }
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

        [MenuItem("Tools/LogSmith/Demo Scene Setup/Remove All Demo Scripts", priority = 30)]
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
