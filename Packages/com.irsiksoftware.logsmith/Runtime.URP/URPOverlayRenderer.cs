#if LOGSMITH_URP_PRESENT
using IrsikSoftware.LogSmith.Core;
using UnityEngine;

namespace IrsikSoftware.LogSmith.URP
{
    /// <summary>
    /// Visual debug renderer for Universal Render Pipeline.
    /// Works in conjunction with URPOverlayRendererFeature.
    /// </summary>
    public class URPOverlayRenderer : IVisualDebugRenderer
    {
        private URPOverlayRenderPass _renderPass;

        public bool IsEnabled { get; set; }

        public void Initialize(Camera camera)
        {
            // URP rendering is handled by URPOverlayRendererFeature
            // This class acts as a bridge to the feature
            Debug.Log("[URPOverlayRenderer] Initialized. Make sure URPOverlayRendererFeature is added to your URP Renderer asset.");
        }

        public void Cleanup()
        {
            _renderPass?.ClearShapes();
            _renderPass = null;
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            if (!IsEnabled || _renderPass == null)
                return;

            var shape = new DebugShape
            {
                Type = DebugShapeType.Line,
                Start = start,
                End = end,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _renderPass.AddShape(shape);
        }

        public void DrawQuad(Vector3 center, Vector2 size, Color color, float duration = 0f)
        {
            if (!IsEnabled || _renderPass == null)
                return;

            var shape = new DebugShape
            {
                Type = DebugShapeType.Quad,
                Start = center,
                Size = size,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _renderPass.AddShape(shape);
        }

        public void Clear()
        {
            _renderPass?.ClearShapes();
        }

        /// <summary>
        /// Registers the render pass from the URPOverlayRendererFeature.
        /// This must be called by the feature during initialization.
        /// </summary>
        public void RegisterRenderPass(URPOverlayRenderPass renderPass)
        {
            _renderPass = renderPass;
            _renderPass.RegisterAdapter(null); // Adapter reference not needed for now
        }
    }
}
#endif
