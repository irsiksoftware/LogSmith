using UnityEngine;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Samples.URP
{
    /// <summary>
    /// Demonstrates LogSmith visual debug rendering with URP.
    /// </summary>
    public class URPDemo : MonoBehaviour
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
                    Debug.LogError("[URPDemo] No camera found! Please assign a camera or add one to the scene.");
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
                Debug.LogWarning("[URPDemo] Visual debug renderer not available. Make sure URP is active and the Renderer Feature is added.");
            }
            else
            {
                Debug.Log("[URPDemo] URP adapter initialized successfully!");
            }
        }

        private void Update()
        {
            if (_renderer == null || !enableShapes) return;

            // Draw a rotating circle with lines
            float angle = Time.time;
            Vector3 center = new Vector3(0, 1, 0);
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float a1 = angle + (i / (float)segments) * Mathf.PI * 2f;
                float a2 = angle + ((i + 1) / (float)segments) * Mathf.PI * 2f;
                Vector3 p1 = center + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * 2f;
                Vector3 p2 = center + new Vector3(Mathf.Cos(a2), 0, Mathf.Sin(a2)) * 2f;

                _renderer.DrawLine(p1, p2, Color.cyan);
            }

            // Draw a static quad
            Vector3 quadCenter = new Vector3(3, 1, 0);
            _renderer.DrawQuad(quadCenter, Quaternion.identity, new Vector2(1, 1), new Color(0, 1, 0, 0.5f));

            // Log demo message
            if (Time.frameCount % 120 == 0)
            {
                Log.Info("URP Demo", $"Frame {Time.frameCount}: Rendering shapes with URP");
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
