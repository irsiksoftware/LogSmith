using UnityEngine;

namespace IrsikSoftware.LogSmith.URP
{
    /// <summary>
    /// Adapter for Universal Render Pipeline (URP).
    /// Provides visual debug rendering capabilities for URP.
    /// </summary>
    public class URPAdapter
    {
#if LOGSMITH_URP_PRESENT
        private URPOverlayRenderer _overlayRenderer;
#endif

        /// <summary>
        /// Gets whether the Universal Render Pipeline is available.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
#if LOGSMITH_URP_PRESENT
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the visual debug renderer for this pipeline.
        /// </summary>
        public IVisualDebugRenderer VisualDebugRenderer
        {
            get
            {
#if LOGSMITH_URP_PRESENT
                return _overlayRenderer;
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// Initializes the URP adapter.
        /// Note: URP requires URPOverlayRendererFeature to be added to the URP Renderer asset.
        /// </summary>
        /// <param name="camera">The camera to attach visual debug rendering to (not used in URP - feature-based).</param>
        /// <param name="enabled">Whether visual debug rendering should be enabled.</param>
        public void Initialize(Camera camera, bool enabled = false)
        {
#if LOGSMITH_URP_PRESENT
            if (_overlayRenderer != null)
            {
                Debug.LogWarning("[URPAdapter] Already initialized");
                return;
            }

            _overlayRenderer = new URPOverlayRenderer();
            _overlayRenderer.Initialize(camera);
            _overlayRenderer.IsEnabled = enabled;
#else
            Debug.LogWarning("[URPAdapter] URP is not available in this project");
#endif
        }

        /// <summary>
        /// Cleans up resources used by the adapter.
        /// </summary>
        public void Cleanup()
        {
#if LOGSMITH_URP_PRESENT
            if (_overlayRenderer != null)
            {
                _overlayRenderer.Cleanup();
                _overlayRenderer = null;
            }
#endif
        }
    }
}
