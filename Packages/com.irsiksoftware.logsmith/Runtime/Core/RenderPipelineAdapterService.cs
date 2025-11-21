using System;
using System.Reflection;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Service that automatically detects and activates the appropriate render pipeline adapter.
    /// </summary>
    public class RenderPipelineAdapterService
    {
        private IVisualDebugRenderer _activeRenderer;
        private object _activeAdapter;
        private RenderPipelineDetector.PipelineType _detectedPipeline;

        /// <summary>
        /// Gets the currently active visual debug renderer, or null if none is active.
        /// </summary>
        public IVisualDebugRenderer ActiveRenderer => _activeRenderer;

        /// <summary>
        /// Gets the type of pipeline that was detected.
        /// </summary>
        public RenderPipelineDetector.PipelineType DetectedPipeline => _detectedPipeline;

        /// <summary>
        /// Initializes the service by detecting the active pipeline and activating the appropriate adapter.
        /// </summary>
        /// <param name="camera">The camera to attach visual debug rendering to.</param>
        /// <param name="enabled">Whether visual debug rendering should be enabled.</param>
        public void Initialize(Camera camera, bool enabled = false)
        {
            _detectedPipeline = RenderPipelineDetector.DetectPipeline();
            string pipelineName = RenderPipelineDetector.GetPipelineName(_detectedPipeline);

            switch (_detectedPipeline)
            {
                case RenderPipelineDetector.PipelineType.BuiltIn:
                    TryActivateAdapter("IrsikSoftware.LogSmith.BuiltIn.BuiltInRenderPipelineAdapter, IrsikSoftware.LogSmith.BuiltIn", camera, enabled, pipelineName);
                    break;

                case RenderPipelineDetector.PipelineType.URP:
                    TryActivateAdapter("IrsikSoftware.LogSmith.URP.URPAdapter, IrsikSoftware.LogSmith.URP", camera, enabled, pipelineName);
                    break;

                case RenderPipelineDetector.PipelineType.HDRP:
                    TryActivateAdapter("IrsikSoftware.LogSmith.HDRP.HDRPAdapter, IrsikSoftware.LogSmith.HDRP", camera, enabled, pipelineName);
                    break;

                case RenderPipelineDetector.PipelineType.Unknown:
                default:
                    FallbackToNoOp(pipelineName);
                    break;
            }
        }

        private void TryActivateAdapter(string typeName, Camera camera, bool enabled, string pipelineName)
        {
            Type adapterType = Type.GetType(typeName);
            if (adapterType == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[RenderPipelineAdapterService] {pipelineName} detected but adapter type not found: {typeName}");
#endif
                FallbackToNoOp(pipelineName);
                return;
            }

            // Check IsAvailable or IsActive static property
            PropertyInfo availableProp = adapterType.GetProperty("IsAvailable", BindingFlags.Public | BindingFlags.Static);
            if (availableProp == null)
            {
                availableProp = adapterType.GetProperty("IsActive", BindingFlags.Public | BindingFlags.Static);
            }

            bool isAvailable = true;
            if (availableProp != null && availableProp.PropertyType == typeof(bool))
            {
                isAvailable = (bool)availableProp.GetValue(null);
            }

            if (!isAvailable)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[RenderPipelineAdapterService] {pipelineName} detected but adapter not available");
#endif
                FallbackToNoOp(pipelineName);
                return;
            }

            // Create instance and initialize
            try
            {
                _activeAdapter = Activator.CreateInstance(adapterType);
                MethodInfo initMethod = adapterType.GetMethod("Initialize", new[] { typeof(Camera), typeof(bool) });
                if (initMethod != null)
                {
                    initMethod.Invoke(_activeAdapter, new object[] { camera, enabled });
                }

                // Get VisualDebugRenderer property
                PropertyInfo rendererProp = adapterType.GetProperty("VisualDebugRenderer");
                if (rendererProp != null)
                {
                    _activeRenderer = rendererProp.GetValue(_activeAdapter) as IVisualDebugRenderer;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[RenderPipelineAdapterService] Activated adapter for {pipelineName}");
#endif
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"[RenderPipelineAdapterService] Failed to activate adapter for {pipelineName}: {ex.Message}");
#endif
                FallbackToNoOp(pipelineName);
            }
        }

        /// <summary>
        /// Cleans up the active adapter.
        /// </summary>
        public void Cleanup()
        {
            if (_activeAdapter != null)
            {
                // Call Cleanup method via reflection
                MethodInfo cleanupMethod = _activeAdapter.GetType().GetMethod("Cleanup");
                if (cleanupMethod != null)
                {
                    try
                    {
                        cleanupMethod.Invoke(_activeAdapter, null);
                    }
                    catch (Exception ex)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogError($"[RenderPipelineAdapterService] Error during cleanup: {ex.Message}");
#endif
                    }
                }

                _activeAdapter = null;
                _activeRenderer = null;
            }
        }

        private void FallbackToNoOp(string pipelineName)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[RenderPipelineAdapterService] No adapter available for {pipelineName}, using No-Op fallback");
#endif
            _activeRenderer = null;
            _activeAdapter = null;
        }
    }
}
