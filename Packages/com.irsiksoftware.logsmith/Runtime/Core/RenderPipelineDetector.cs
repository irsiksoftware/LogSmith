using UnityEngine;
using UnityEngine.Rendering;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Detects which render pipeline is active at runtime.
    /// </summary>
    public static class RenderPipelineDetector
    {
        /// <summary>
        /// The type of render pipeline detected.
        /// </summary>
        public enum PipelineType
        {
            /// <summary>Built-in Render Pipeline</summary>
            BuiltIn,
            /// <summary>Universal Render Pipeline</summary>
            URP,
            /// <summary>High Definition Render Pipeline</summary>
            HDRP,
            /// <summary>Unknown or custom pipeline</summary>
            Unknown
        }

        /// <summary>
        /// Detects the active render pipeline at runtime.
        /// </summary>
        /// <returns>The type of render pipeline currently active.</returns>
        public static PipelineType DetectPipeline()
        {
            var currentPipeline = GraphicsSettings.currentRenderPipeline;

            if (currentPipeline == null)
            {
                // No SRP asset means Built-in RP
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[RenderPipelineDetector] Unknown render pipeline: {pipelineName}");
#endif
            return PipelineType.Unknown;
        }

        /// <summary>
        /// Gets a human-readable name for the detected pipeline.
        /// </summary>
        public static string GetPipelineName(PipelineType pipeline)
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
    }
}
