using UnityEditor;
using UnityEngine;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Sinks tab for the LogSmith Editor Window.
    /// Allows configuring console and file sinks.
    /// </summary>
    public class SinksTab : IEditorTab
    {
        public string TabName => "Sinks";

        private IPlatformCapabilities _platformCapabilities;

        public SinksTab()
        {
            _platformCapabilities = new PlatformCapabilities();
        }

        public void Draw(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Sink Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Sinks determine where log messages are output. Configure console and file sinks below.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            // Console Sink
            DrawConsoleSink(serializedSettings);

            EditorGUILayout.Space(15);

            // File Sink
            DrawFileSink(serializedSettings, settings);
        }

        private void DrawConsoleSink(SerializedObject serializedSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Console Sink", EditorStyles.boldLabel);

            var enableConsoleProp = serializedSettings.FindProperty("enableConsoleSink");

            EditorGUILayout.BeginHorizontal();
            enableConsoleProp.boolValue = EditorGUILayout.Toggle("Enabled", enableConsoleProp.boolValue);
            EditorGUILayout.EndHorizontal();

            if (enableConsoleProp.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "✓ Console sink active - logs will appear in Unity Console and Editor.log",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Console sink disabled - no output to Unity Console",
                    MessageType.Warning
                );
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFileSink(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("File Sink", EditorStyles.boldLabel);

            var enableFileProp = serializedSettings.FindProperty("enableFileSink");
            var logFilePathProp = serializedSettings.FindProperty("logFilePath");
            var enableRotationProp = serializedSettings.FindProperty("enableLogRotation");
            var maxFileSizeProp = serializedSettings.FindProperty("maxFileSizeMB");
            var retentionCountProp = serializedSettings.FindProperty("retentionCount");
            var formatModeProp = serializedSettings.FindProperty("defaultFormatMode");

            // Enable/Disable
            enableFileProp.boolValue = EditorGUILayout.Toggle("Enabled", enableFileProp.boolValue);

            EditorGUI.BeginDisabledGroup(!enableFileProp.boolValue);

            // File Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Log File Path:", GUILayout.Width(120));
            logFilePathProp.stringValue = EditorGUILayout.TextField(logFilePathProp.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                $"Relative to: {Application.persistentDataPath}",
                MessageType.None
            );

            EditorGUILayout.Space(10);

            // Format Mode
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Format:", GUILayout.Width(120));
            formatModeProp.enumValueIndex = (int)(MessageFormatMode)EditorGUILayout.EnumPopup(
                (MessageFormatMode)formatModeProp.enumValueIndex
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Rotation Settings
            EditorGUILayout.LabelField("Rotation Settings", EditorStyles.boldLabel);
            enableRotationProp.boolValue = EditorGUILayout.Toggle("Enable Rotation", enableRotationProp.boolValue);

            EditorGUI.BeginDisabledGroup(!enableRotationProp.boolValue);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max File Size (MB):", GUILayout.Width(150));
            maxFileSizeProp.intValue = EditorGUILayout.IntSlider(maxFileSizeProp.intValue, 1, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Retention Count:", GUILayout.Width(150));
            retentionCountProp.intValue = EditorGUILayout.IntSlider(retentionCountProp.intValue, 0, 20);
            EditorGUILayout.EndHorizontal();

            if (retentionCountProp.intValue == 0)
            {
                EditorGUILayout.HelpBox("Retention Count = 0: All archived logs will be kept", MessageType.Info);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.EndDisabledGroup();

            // Platform Compatibility Warning
            if (enableFileProp.boolValue)
            {
                DrawPlatformWarnings();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPlatformWarnings()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Platform Compatibility", EditorStyles.boldLabel);

            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            var isCurrentTargetCompatible = IsFileSinkCompatible(currentTarget);

            if (!isCurrentTargetCompatible)
            {
                EditorGUILayout.HelpBox(
                    $"⚠ File sink is NOT supported on {currentTarget}.\n\n" +
                    "File logging will be automatically disabled at runtime on this platform.",
                    MessageType.Warning
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"✓ File sink is supported on {currentTarget}.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Unsupported Platforms:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• WebGL (no file system access)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Nintendo Switch (restricted file I/O)", EditorStyles.miniLabel);
        }

        private bool IsFileSinkCompatible(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.WebGL:
                case BuildTarget.Switch:
                    return false;
                default:
                    return true;
            }
        }
    }
}
