using UnityEngine;
using IrsikSoftware.LogSmith.BuiltIn;
using IrsikSoftware.LogSmith.URP;
using IrsikSoftware.LogSmith.HDRP;

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
                    if (BuiltInRenderPipelineAdapter.IsActive)
                    {
                        var adapter = new BuiltInRenderPipelineAdapter();
                        adapter.Initialize(camera, enabled);
                        _activeAdapter = adapter;
                        _activeRenderer = adapter.VisualDebugRenderer;
                        Debug.Log($"[RenderPipelineAdapterService] Activated adapter for {pipelineName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[RenderPipelineAdapterService] {pipelineName} detected but adapter not available");
                    }
                    break;

                case RenderPipelineDetector.PipelineType.URP:
                    if (URPAdapter.IsAvailable)
                    {
                        var adapter = new URPAdapter();
                        adapter.Initialize(camera, enabled);
                        _activeAdapter = adapter;
                        _activeRenderer = adapter.VisualDebugRenderer;
                        Debug.Log($"[RenderPipelineAdapterService] Activated adapter for {pipelineName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[RenderPipelineAdapterService] {pipelineName} detected but adapter not available");
                        FallbackToNoOp(pipelineName);
                    }
                    break;

                case RenderPipelineDetector.PipelineType.HDRP:
                    if (HDRPAdapter.IsAvailable)
                    {
                        var adapter = new HDRPAdapter();
                        adapter.Initialize(camera, enabled);
                        _activeAdapter = adapter;
                        _activeRenderer = adapter.VisualDebugRenderer;
                        Debug.Log($"[RenderPipelineAdapterService] Activated adapter for {pipelineName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[RenderPipelineAdapterService] {pipelineName} detected but adapter not available");
                        FallbackToNoOp(pipelineName);
                    }
                    break;

                case RenderPipelineDetector.PipelineType.Unknown:
                default:
                    FallbackToNoOp(pipelineName);
                    break;
            }
        }

        /// <summary>
        /// Cleans up the active adapter.
        /// </summary>
        public void Cleanup()
        {
            if (_activeAdapter != null)
            {
                // Call Cleanup on whichever adapter type is active
                if (_activeAdapter is BuiltInRenderPipelineAdapter builtInAdapter)
                {
                    builtInAdapter.Cleanup();
                }
                else if (_activeAdapter is URPAdapter urpAdapter)
                {
                    urpAdapter.Cleanup();
                }
                else if (_activeAdapter is HDRPAdapter hdrpAdapter)
                {
                    hdrpAdapter.Cleanup();
                }

                _activeAdapter = null;
                _activeRenderer = null;
            }
        }

        private void FallbackToNoOp(string pipelineName)
        {
            Debug.Log($"[RenderPipelineAdapterService] No adapter available for {pipelineName}, using No-Op fallback");
            _activeRenderer = null;
            _activeAdapter = null;
        }
    }
}
