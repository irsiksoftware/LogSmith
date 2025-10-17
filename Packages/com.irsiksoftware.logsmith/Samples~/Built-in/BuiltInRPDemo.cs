using UnityEngine;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Samples.BuiltIn
{
    /// <summary>
    /// Demonstrates LogSmith visual debug rendering with Built-in RP.
    /// </summary>
    public class BuiltInRPDemo : MonoBehaviour
    {
        [Header("Visual Debug Configuration")]
        [Tooltip("Enable visual debug shapes")]
        public bool enableShapes = true;

        [Tooltip("Camera for visual debug rendering")]
        public Camera targetCamera;

        private IRenderPipelineAdapterService _adapterService;
        private IVisualDebugRenderer _renderer;

        private void Start()
        {
            // Find camera if not assigned
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogError("[BuiltInRPDemo] No camera found! Please assign a camera or add one to the scene.");
                    enabled = false;
                    return;
                }
            }

            // Initialize LogSmith if not already initialized
            if (!LogSmith.IsInitialized)
            {
                LogSmith.Initialize();
            }

            // Initialize render pipeline adapter
            _adapterService = new RenderPipelineAdapterService();
            _adapterService.Initialize(targetCamera, enableShapes);
            _renderer = _adapterService.ActiveRenderer;

            if (_renderer == null)
            {
                Debug.LogWarning("[BuiltInRPDemo] Visual debug renderer not available. Make sure Built-in RP is active.");
            }
            else
            {
                Debug.Log("[BuiltInRPDemo] Built-in RP adapter initialized successfully!");
            }
        }

        private void Update()
        {
            if (_renderer == null || !enableShapes) return;

            // Draw a rotating line
            float angle = Time.time;
            Vector3 center = new Vector3(0, 1, 0);
            Vector3 start = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2f;
            Vector3 end = center + new Vector3(Mathf.Cos(angle + Mathf.PI), 0, Mathf.Sin(angle + Mathf.PI)) * 2f;

            _renderer.DrawLine(start, end, Color.green);

            // Draw a static quad
            Vector3 quadCenter = new Vector3(3, 1, 0);
            _renderer.DrawQuad(quadCenter, Quaternion.identity, new Vector2(1, 1), new Color(1, 0, 0, 0.5f));

            // Log demo message
            if (Time.frameCount % 120 == 0)
            {
                Log.Info("Built-in RP Demo", $"Frame {Time.frameCount}: Rendering shapes");
            }
        }

        private void OnDestroy()
        {
            if (_adapterService != null)
            {
                _adapterService.Cleanup();
            }
        }
    }
}
