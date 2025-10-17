using UnityEngine;

namespace IrsikSoftware.LogSmith.BuiltIn
{
    /// <summary>
    /// Adapter for Built-in Render Pipeline.
    /// Provides visual debug rendering capabilities for the Built-in RP.
    /// </summary>
    public class BuiltInRenderPipelineAdapter
    {
        private BuiltInOverlayRenderer _overlayRenderer;

        /// <summary>
        /// Gets whether the Built-in Render Pipeline is active.
        /// Returns true only when URP and HDRP are not available.
        /// </summary>
        public static bool IsActive
        {
            get
            {
#if !LOGSMITH_URP_AVAILABLE && !LOGSMITH_HDRP_AVAILABLE
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the visual debug renderer for this pipeline.
        /// </summary>
        public IVisualDebugRenderer VisualDebugRenderer => _overlayRenderer;

        /// <summary>
        /// Initializes the Built-in Render Pipeline adapter with the specified camera.
        /// </summary>
        /// <param name="camera">The camera to attach visual debug rendering to.</param>
        /// <param name="enabled">Whether visual debug rendering should be enabled.</param>
        public void Initialize(Camera camera, bool enabled = false)
        {
            if (_overlayRenderer != null)
            {
                Debug.LogWarning("[BuiltInRPAdapter] Already initialized");
                return;
            }

            _overlayRenderer = new BuiltInOverlayRenderer();
            _overlayRenderer.Initialize(camera);
            _overlayRenderer.IsEnabled = enabled;
        }

        /// <summary>
        /// Cleans up resources used by the adapter.
        /// </summary>
        public void Cleanup()
        {
            if (_overlayRenderer != null)
            {
                _overlayRenderer.Cleanup();
                _overlayRenderer = null;
            }
        }
    }
}
