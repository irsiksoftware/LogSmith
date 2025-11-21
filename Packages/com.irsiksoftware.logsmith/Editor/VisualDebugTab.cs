using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Visual Debug tab for the LogSmith Editor Window.
    /// Allows configuring visual debug rendering with render pipeline adapter warnings.
    /// </summary>
    public class VisualDebugTab : IEditorTab
    {
        public string TabName => "Visual Debug";

        public void Draw(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Visual Debug Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Visual debug rendering allows drawing shapes (lines, quads) in the game view. " +
                "Requires the appropriate render pipeline adapter to be installed and configured.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            // Enable Visual Debug
            var enableVisualDebugProp = serializedSettings.FindProperty("enableVisualDebug");
            enableVisualDebugProp.boolValue = EditorGUILayout.Toggle("Enable Visual Debug", enableVisualDebugProp.boolValue);

            EditorGUILayout.Space(15);

            // Render Pipeline Detection
            DrawPipelineStatus(enableVisualDebugProp.boolValue);
        }

        private void DrawPipelineStatus(bool visualDebugEnabled)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Render Pipeline Status", EditorStyles.boldLabel);

            // Detect active pipeline
            var detectedPipeline = DetectActivePipeline();
            string pipelineName = GetPipelineDisplayName(detectedPipeline);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Detected Pipeline: {pipelineName}", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            // Show warnings based on pipeline and adapter availability
            switch (detectedPipeline)
            {
                case PipelineType.BuiltIn:
                    DrawBuiltInStatus(visualDebugEnabled);
                    break;

                case PipelineType.URP:
                    DrawURPStatus(visualDebugEnabled);
                    break;

                case PipelineType.HDRP:
                    DrawHDRPStatus(visualDebugEnabled);
                    break;

                case PipelineType.Unknown:
                    DrawUnknownPipelineStatus(visualDebugEnabled);
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuiltInStatus(bool visualDebugEnabled)
        {
            if (visualDebugEnabled)
            {
                EditorGUILayout.HelpBox(
                    "✓ Built-in Render Pipeline adapter is available.\n" +
                    "Visual debug rendering will work automatically.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Visual debug rendering is disabled. Enable it above to use shape drawing.",
                    MessageType.None
                );
            }
        }

        private void DrawURPStatus(bool visualDebugEnabled)
        {
            bool isURPPackageInstalled = IsURPPackageInstalled();

            if (!isURPPackageInstalled)
            {
                EditorGUILayout.HelpBox(
                    "⚠ Universal Render Pipeline (URP) package is NOT installed.\n\n" +
                    "Visual debug rendering will NOT work until you install the URP package.\n\n" +
                    "To install: Window → Package Manager → Unity Registry → Universal RP → Install",
                    MessageType.Warning
                );

                if (GUILayout.Button("Open Package Manager", GUILayout.Height(25)))
                {
                    UnityEditor.PackageManager.UI.Window.Open("com.unity.render-pipelines.universal");
                }
            }
            else if (visualDebugEnabled)
            {
                EditorGUILayout.HelpBox(
                    "✓ URP package is installed.\n\n" +
                    "Additional Setup Required:\n" +
                    "1. Open your URP Renderer asset\n" +
                    "2. Add 'LogSmith Overlay Renderer Feature' to the renderer features list\n" +
                    "3. Visual debug shapes will then render correctly",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "✓ URP package is installed.\n" +
                    "Visual debug rendering is disabled. Enable it above to use shape drawing.",
                    MessageType.None
                );
            }
        }

        private void DrawHDRPStatus(bool visualDebugEnabled)
        {
            bool isHDRPPackageInstalled = IsHDRPPackageInstalled();

            if (!isHDRPPackageInstalled)
            {
                EditorGUILayout.HelpBox(
                    "⚠ High Definition Render Pipeline (HDRP) package is NOT installed.\n\n" +
                    "Visual debug rendering will NOT work until you install the HDRP package.\n\n" +
                    "To install: Window → Package Manager → Unity Registry → High Definition RP → Install",
                    MessageType.Warning
                );

                if (GUILayout.Button("Open Package Manager", GUILayout.Height(25)))
                {
                    UnityEditor.PackageManager.UI.Window.Open("com.unity.render-pipelines.high-definition");
                }
            }
            else if (visualDebugEnabled)
            {
                EditorGUILayout.HelpBox(
                    "✓ HDRP package is installed.\n\n" +
                    "Additional Setup Required:\n" +
                    "1. Add a 'Custom Pass Volume' to your scene\n" +
                    "2. Add 'LogSmith Overlay Custom Pass' to the volume\n" +
                    "3. Visual debug shapes will then render correctly",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "✓ HDRP package is installed.\n" +
                    "Visual debug rendering is disabled. Enable it above to use shape drawing.",
                    MessageType.None
                );
            }
        }

        private void DrawUnknownPipelineStatus(bool visualDebugEnabled)
        {
            EditorGUILayout.HelpBox(
                "⚠ Unknown or custom render pipeline detected.\n\n" +
                "Visual debug rendering is only supported for:\n" +
                "• Built-in Render Pipeline\n" +
                "• Universal Render Pipeline (URP)\n" +
                "• High Definition Render Pipeline (HDRP)",
                MessageType.Warning
            );
        }

        private enum PipelineType
        {
            BuiltIn,
            URP,
            HDRP,
            Unknown
        }

        private PipelineType DetectActivePipeline()
        {
            var currentPipeline = GraphicsSettings.currentRenderPipeline;

            if (currentPipeline == null)
            {
                return PipelineType.BuiltIn;
            }

            string pipelineName = currentPipeline.GetType().Name;

            if (pipelineName.Contains("Universal") || pipelineName.Contains("URP"))
            {
                return PipelineType.URP;
            }

            if (pipelineName.Contains("HDRenderPipeline") || pipelineName.Contains("HDRP"))
            {
                return PipelineType.HDRP;
            }

            return PipelineType.Unknown;
        }

        private string GetPipelineDisplayName(PipelineType pipeline)
        {
            switch (pipeline)
            {
                case PipelineType.BuiltIn:
                    return "Built-in Render Pipeline";
                case PipelineType.URP:
                    return "Universal Render Pipeline (URP)";
                case PipelineType.HDRP:
                    return "High Definition Render Pipeline (HDRP)";
                case PipelineType.Unknown:
                    return "Unknown Pipeline";
                default:
                    return "Unknown";
            }
        }

        private bool IsURPPackageInstalled()
        {
#if LOGSMITH_URP_PRESENT
            return true;
#else
            return false;
#endif
        }

        private bool IsHDRPPackageInstalled()
        {
#if LOGSMITH_HDRP_PRESENT
            return true;
#else
            return false;
#endif
        }
    }
}
