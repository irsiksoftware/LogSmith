#if LOGSMITH_HDRP_PRESENT
using UnityEngine;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.HDRP
{
    /// <summary>
    /// Visual debug renderer for High Definition Render Pipeline.
    /// Works in conjunction with HDRPOverlayCustomPass.
    /// </summary>
    public class HDRPOverlayRenderer : IVisualDebugRenderer
    {
        private HDRPOverlayCustomPass _customPass;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void Initialize(Camera camera)
        {
            // HDRP rendering is handled by HDRPOverlayCustomPass
            // This class acts as a bridge to the custom pass
            Debug.Log("[HDRPOverlayRenderer] Initialized. Make sure HDRPOverlayCustomPass is added to your Custom Pass Volume.");
        }

        public void Cleanup()
        {
            _customPass?.ClearShapes();
            _customPass = null;
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            if (!_isEnabled || _customPass == null)
                return;

            var shape = new DebugShape
            {
                Type = DebugShapeType.Line,
                Start = start,
                End = end,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _customPass.AddShape(shape);
        }

        public void DrawQuad(Vector3 center, Vector2 size, Color color, float duration = 0f)
        {
            if (!_isEnabled || _customPass == null)
                return;

            var shape = new DebugShape
            {
                Type = DebugShapeType.Quad,
                Start = center,
                Size = size,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _customPass.AddShape(shape);
        }

        public void Clear()
        {
            _customPass?.ClearShapes();
        }

        /// <summary>
        /// Registers the custom pass from HDRPOverlayCustomPass.
        /// This must be called by the custom pass during initialization.
        /// </summary>
        public void RegisterCustomPass(HDRPOverlayCustomPass customPass)
        {
            _customPass = customPass;
        }
    }
}
#endif
