using UnityEngine;

namespace IrsikSoftware.LogSmith.HDRP
{
    /// <summary>
    /// Adapter for High Definition Render Pipeline (HDRP).
    /// Provides visual debug rendering capabilities for HDRP.
    /// </summary>
    public class HDRPAdapter
    {
#if LOGSMITH_HDRP_PRESENT
        private HDRPOverlayRenderer _overlayRenderer;
#endif

        /// <summary>
        /// Gets whether the High Definition Render Pipeline is available.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
#if LOGSMITH_HDRP_PRESENT
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
#if LOGSMITH_HDRP_PRESENT
                return _overlayRenderer;
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// Initializes the HDRP adapter.
        /// Note: HDRP requires HDRPOverlayCustomPass to be added to a Custom Pass Volume.
        /// </summary>
        /// <param name="camera">The camera to attach visual debug rendering to (not used in HDRP - volume-based).</param>
        /// <param name="enabled">Whether visual debug rendering should be enabled.</param>
        public void Initialize(Camera camera, bool enabled = false)
        {
#if LOGSMITH_HDRP_PRESENT
            if (_overlayRenderer != null)
            {
                Debug.LogWarning("[HDRPAdapter] Already initialized");
                return;
            }

            _overlayRenderer = new HDRPOverlayRenderer();
            _overlayRenderer.Initialize(camera);
            _overlayRenderer.IsEnabled = enabled;
#else
            Debug.LogWarning("[HDRPAdapter] HDRP is not available in this project");
#endif
        }

        /// <summary>
        /// Cleans up resources used by the adapter.
        /// </summary>
        public void Cleanup()
        {
#if LOGSMITH_HDRP_PRESENT
            if (_overlayRenderer != null)
            {
                _overlayRenderer.Cleanup();
                _overlayRenderer = null;
            }
#endif
        }
    }
}
