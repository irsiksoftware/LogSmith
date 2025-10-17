using UnityEditor;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Utility to automatically set up the LogSmith demo scene with hierarchical structure.
    /// Access via: Tools > LogSmith > Demo Scene Setup > (various options)
    /// </summary>
    public static class SetupDemoSceneUtility
    {
        [MenuItem("Tools/LogSmith/Demo Scene Setup/Setup All Demo GameObjects", priority = 1)]
        public static void SetupAllDemoGameObjects()
        {
            if (!EditorUtility.DisplayDialog(
                "Setup Complete LogSmith Demo Scene",
                "This will add ALL LogSmith demo GameObjects to the current scene organized by sample type.\n\n" +
                "This includes:\n" +
                "- Basic Usage sample GameObjects\n" +
                "- Comprehensive Demo sample GameObjects\n" +
                "- VContainer Integration sample GameObjects\n" +
                "- Render Pipeline demo GameObjects\n\n" +
                "Scripts must be added after importing the respective samples.",
                "Setup All",
                "Cancel"))
            {
                return;
            }

            GameObject root = CreateOrFindRoot();

            SetupBasicUsageSample(root);
            SetupComprehensiveDemoSample(root);
            SetupVContainerSample(root);
            SetupRenderPipelineDemos(root);

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);

            EditorUtility.DisplayDialog(
                "Complete Demo Scene Setup!",
                "All LogSmith demo GameObjects have been added to your scene!\n\n" +
                "Next steps:\n" +
                "1. Import samples from Package Manager (Window → Package Manager → LogSmith → Samples)\n" +
                "2. Use 'Tools → LogSmith → Demo Scene Setup → Auto-Add Scripts' to add scripts\n" +
                "3. Press Play and explore!\n\n" +
                "Use the submenu options to set up individual sample categories if needed.",
                "Got It!");
        }

        [MenuItem("Tools/LogSmith/Demo Scene Setup/Basic Usage Sample", priority = 10)]
        public static void SetupBasicUsageSampleOnly()
        {
            GameObject root = CreateOrFindRoot();
            SetupBasicUsageSample(root);
            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "Basic Usage Sample Setup",
                "Basic Usage sample GameObject created!\n\n" +
                "Next steps:\n" +
                "1. Import 'Basic Usage' sample from Package Manager\n" +
                "2. Add BasicLoggingExample script to the GameObject\n" +
                "3. Press Play!",
                "OK");
        }

        [MenuItem("Tools/LogSmith/Demo Scene Setup/Comprehensive Demo Sample", priority = 11)]
        public static void SetupComprehensiveDemoSampleOnly()
        {
            GameObject root = CreateOrFindRoot();
            SetupComprehensiveDemoSample(root);
            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "Comprehensive Demo Sample Setup",
                "Comprehensive Demo sample GameObjects created!\n\n" +
                "Next steps:\n" +
                "1. Import 'Comprehensive Demo' sample from Package Manager\n" +
                "2. Use 'Tools → LogSmith → Demo Scene Setup → Auto-Add Scripts'\n" +
                "3. Press Play and toggle GameObjects on/off!",
                "OK");
        }

        [MenuItem("Tools/LogSmith/Demo Scene Setup/VContainer Integration Sample", priority = 12)]
        public static void SetupVContainerSampleOnly()
        {
            GameObject root = CreateOrFindRoot();
            SetupVContainerSample(root);
            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "VContainer Integration Sample Setup",
                "VContainer sample GameObjects created!\n\n" +
                "Next steps:\n" +
                "1. Install VContainer package\n" +
                "2. Import 'VContainer Integration' sample from Package Manager\n" +
                "3. Configure the LoggingLifetimeScope component\n" +
                "4. Press Play!",
                "OK");
        }

        [MenuItem("Tools/LogSmith/Demo Scene Setup/Render Pipeline Demos", priority = 13)]
        public static void SetupRenderPipelineDemosOnly()
        {
            GameObject root = CreateOrFindRoot();
            SetupRenderPipelineDemos(root);
            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "Render Pipeline Demos Setup",
                "Render Pipeline demo GameObjects created!\n\n" +
                "Next steps:\n" +
                "1. Import the sample matching your render pipeline:\n" +
                "   - Built-in RP Demo\n" +
                "   - URP Demo (requires URP package)\n" +
                "   - HDRP Demo (requires HDRP package)\n" +
                "2. Add the corresponding demo script\n" +
                "3. Press Play to see the visual debug overlay!",
                "OK");
        }

        private static GameObject CreateOrFindRoot()
        {
            GameObject root = GameObject.Find("LogSmith Demo Examples");
            if (root == null)
            {
                root = new GameObject("LogSmith Demo Examples");
                Undo.RegisterCreatedObjectUndo(root, "Create LogSmith Demo Root");
            }
            return root;
        }

        private static void SetupBasicUsageSample(GameObject root)
        {
            GameObject basicUsageParent = FindOrCreateChild(root, "Basic Usage Sample");
            FindOrCreateChild(basicUsageParent, "BasicLoggingExample");
        }

        private static void SetupComprehensiveDemoSample(GameObject root)
        {
            GameObject demoParent = FindOrCreateChild(root, "Comprehensive Demo Sample");

            FindOrCreateChild(demoParent, "1. BasicLogging");
            FindOrCreateChild(demoParent, "2. CategoryLogging");
            FindOrCreateChild(demoParent, "3. GameSystems");
            FindOrCreateChild(demoParent, "4. Performance");
            FindOrCreateChild(demoParent, "5. Player (Press 1-5)");
            FindOrCreateChild(demoParent, "6. LogLevels (Use Context Menu)");
            FindOrCreateChild(demoParent, "7. VContainerLogging");
        }

        private static void SetupVContainerSample(GameObject root)
        {
            GameObject vcontainerParent = FindOrCreateChild(root, "VContainer Integration Sample");

            GameObject lifetimeScope = FindOrCreateChild(vcontainerParent, "GameLifetimeScope");

            // Try to add LoggingLifetimeScope if VContainer is available
            var type = System.Type.GetType("IrsikSoftware.LogSmith.DI.LoggingLifetimeScope, IrsikSoftware.LogSmith.VContainerIntegration");
            if (type != null && lifetimeScope.GetComponent(type) == null)
            {
                Undo.AddComponent(lifetimeScope, type);
            }
        }

        private static void SetupRenderPipelineDemos(GameObject root)
        {
            GameObject rpParent = FindOrCreateChild(root, "Render Pipeline Demos");

            FindOrCreateChild(rpParent, "Built-in RP Demo");
            FindOrCreateChild(rpParent, "URP Demo");
            FindOrCreateChild(rpParent, "HDRP Demo");
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            Transform existing = parent.transform.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(child, $"Create {childName}");
            return child;
        }

        [MenuItem("Tools/LogSmith/Create LoggingSettings Asset", priority = 100)]
        public static void CreateLoggingSettings()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create LoggingSettings Asset",
                "LoggingSettings",
                "asset",
                "Choose where to save the LoggingSettings asset",
                "Assets");

            if (!string.IsNullOrEmpty(path))
            {
                var settings = ScriptableObject.CreateInstance<LoggingSettings>();
                settings.categories.Add(new CategoryDefinition("Default", Color.white, LogLevel.Debug, true));
                settings.categories.Add(new CategoryDefinition("Gameplay", new Color(0.4f, 0.8f, 1.0f), LogLevel.Debug, true));
                settings.categories.Add(new CategoryDefinition("Network", new Color(0.4f, 1.0f, 0.6f), LogLevel.Info, true));
                settings.categories.Add(new CategoryDefinition("Physics", new Color(1.0f, 0.8f, 0.4f), LogLevel.Warn, true));
                settings.categories.Add(new CategoryDefinition("AI", new Color(1.0f, 0.6f, 0.8f), LogLevel.Debug, true));
                settings.categories.Add(new CategoryDefinition("Audio", new Color(0.8f, 0.6f, 1.0f), LogLevel.Info, true));
                settings.categories.Add(new CategoryDefinition("Performance", new Color(1.0f, 1.0f, 0.6f), LogLevel.Warn, true));
                settings.categories.Add(new CategoryDefinition("Player", new Color(0.6f, 1.0f, 1.0f), LogLevel.Info, true));

                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog(
                    "LoggingSettings Created",
                    $"LoggingSettings asset created at:\n{path}\n\n" +
                    "Preconfigured categories:\n" +
                    "- Default, Gameplay, Network, Physics\n" +
                    "- AI, Audio, Performance, Player\n\n" +
                    "Open Window → LogSmith → Settings to customize!",
                    "OK");

                EditorGUIUtility.PingObject(settings);
                Selection.activeObject = settings;
            }
        }
    }
}
