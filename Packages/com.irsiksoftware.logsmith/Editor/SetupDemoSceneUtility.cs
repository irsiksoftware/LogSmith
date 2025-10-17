using UnityEditor;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Utility to automatically set up the LogSmith demo scene.
    /// Access via: Tools > LogSmith > Setup Demo Scene
    /// </summary>
    public static class SetupDemoSceneUtility
    {
        [MenuItem("Tools/LogSmith/Setup Demo Scene")]
        public static void SetupDemoScene()
        {
            if (!EditorUtility.DisplayDialog(
                "Setup LogSmith Demo Scene",
                "This will add LogSmith demo GameObjects to the current scene.\n\n" +
                "Make sure you have the SampleScene open before continuing.",
                "Setup Demo",
                "Cancel"))
            {
                return;
            }

            // Create parent GameObject
            GameObject demoParent = new GameObject("LogSmith Demo Examples");
            Undo.RegisterCreatedObjectUndo(demoParent, "Create LogSmith Demo");

            // Create child GameObjects (scripts must be added manually after importing samples)
            // Note: Sample scripts are in Samples~ folder and only available after import

            GameObject basicLogging = new GameObject("1. BasicLogging");
            basicLogging.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(basicLogging, "Create BasicLogging");

            GameObject categoryLogging = new GameObject("2. CategoryLogging");
            categoryLogging.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(categoryLogging, "Create CategoryLogging");

            GameObject gameSystems = new GameObject("3. GameSystems");
            gameSystems.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(gameSystems, "Create GameSystems");

            GameObject performance = new GameObject("4. Performance");
            performance.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(performance, "Create Performance");

            GameObject player = new GameObject("5. Player (Press 1-5)");
            player.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(player, "Create Player");

            GameObject logLevels = new GameObject("6. LogLevels (Use Context Menu)");
            logLevels.transform.SetParent(demoParent.transform);
            Undo.RegisterCreatedObjectUndo(logLevels, "Create LogLevels");

            // Position parent for better visibility
            demoParent.transform.position = Vector3.zero;

            // Select the parent in the hierarchy
            Selection.activeGameObject = demoParent;

            EditorUtility.DisplayDialog(
                "Demo Scene Setup Complete!",
                "LogSmith demo GameObjects have been added to your scene.\n\n" +
                "Next steps:\n" +
                "1. Import the Demo sample from Package Manager\n" +
                "2. Add the sample scripts to each numbered GameObject\n" +
                "3. Press Play and toggle GameObjects on/off to test!\n\n" +
                "Scripts to add:\n" +
                "- 1. BasicLogging → BasicLoggingExample\n" +
                "- 2. CategoryLogging → CategoryLoggingExample\n" +
                "- 3. GameSystems → GameSystemsExample\n" +
                "- 4. Performance → PerformanceLoggingExample\n" +
                "- 5. Player → PlayerActionExample\n" +
                "- 6. LogLevels → LogLevelExample",
                "OK");
        }

        [MenuItem("Tools/LogSmith/Create LoggingSettings Asset")]
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
