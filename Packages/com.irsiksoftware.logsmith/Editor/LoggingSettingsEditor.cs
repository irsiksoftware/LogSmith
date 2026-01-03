using IrsikSoftware.LogSmith.Core;
using UnityEditor;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Custom inspector for LoggingSettings with platform compatibility warnings.
    /// </summary>
    [CustomEditor(typeof(LoggingSettings))]
    public class LoggingSettingsEditor : UnityEditor.Editor
    {
        private IPlatformCapabilities _platformCapabilities;

        private void OnEnable()
        {
            _platformCapabilities = new PlatformCapabilities();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default inspector
            DrawDefaultInspector();

            // Show platform compatibility warnings
            var settings = (LoggingSettings)target;

            if (settings.enableFileSink)
            {
                ShowFileSinkPlatformWarnings();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowFileSinkPlatformWarnings()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Platform Compatibility", EditorStyles.boldLabel);

            // Check current build target compatibility
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            var isCurrentTargetCompatible = IsFileSinkCompatible(currentTarget);

            if (!isCurrentTargetCompatible)
            {
                EditorGUILayout.HelpBox(
                    $"⚠ File sink is NOT supported on {currentTarget}.\n\n" +
                    "File logging will be automatically disabled at runtime on this platform. " +
                    "Consider disabling the file sink in settings or creating platform-specific " +
                    "settings assets.\n\n" +
                    "Supported platforms: Windows, macOS, Linux, iOS, Android, PlayStation, Xbox.",
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

            // Show list of incompatible platforms
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
