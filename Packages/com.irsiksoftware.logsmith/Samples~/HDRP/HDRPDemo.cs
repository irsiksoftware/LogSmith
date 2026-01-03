using UnityEngine;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Samples.HDRP
{
    /// <summary>
    /// Demonstrates LogSmith visual debug rendering with HDRP.
    /// </summary>
    public class HDRPDemo : MonoBehaviour
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
                    Debug.LogError("[HDRPDemo] No camera found! Please assign a camera or add one to the scene.");
                    enabled = false;
                    return;
                }
            }

            // Initialize LogSmith if not already initialized
            if (!LogSmith.IsInitialized)
            {
                Log.Initialize();
            }

            // Initialize render pipeline adapter
            _adapterService = new RenderPipelineAdapterService();
            _adapterService.Initialize(targetCamera, enableShapes);
            _renderer = _adapterService.ActiveRenderer;

            if (_renderer == null)
            {
                Debug.LogWarning("[HDRPDemo] Visual debug renderer not available. Make sure HDRP is active and the Custom Pass is added.");
            }
            else
            {
                Debug.Log("[HDRPDemo] HDRP adapter initialized successfully!");
            }
        }

        private void Update()
        {
            if (_renderer == null || !enableShapes) return;

            // Draw a rotating cube wireframe
            float angle = Time.time;
            Vector3 center = new Vector3(0, 1, 0);
            Quaternion rotation = Quaternion.Euler(angle * 30f, angle * 45f, 0);

            // Draw cube edges
            float size = 1.5f;
            Vector3[] corners = new Vector3[8]
            {
                new Vector3(-size, -size, -size),
                new Vector3( size, -size, -size),
                new Vector3( size,  size, -size),
                new Vector3(-size,  size, -size),
                new Vector3(-size, -size,  size),
                new Vector3( size, -size,  size),
                new Vector3( size,  size,  size),
                new Vector3(-size,  size,  size)
            };

            // Transform and draw edges
            for (int i = 0; i < 4; i++)
            {
                // Bottom face
                Vector3 p1 = center + rotation * corners[i];
                Vector3 p2 = center + rotation * corners[(i + 1) % 4];
                _renderer.DrawLine(p1, p2, Color.magenta);

                // Top face
                p1 = center + rotation * corners[i + 4];
                p2 = center + rotation * corners[((i + 1) % 4) + 4];
                _renderer.DrawLine(p1, p2, Color.magenta);

                // Vertical edges
                p1 = center + rotation * corners[i];
                p2 = center + rotation * corners[i + 4];
                _renderer.DrawLine(p1, p2, Color.magenta);
            }

            // Draw a static quad
            Vector3 quadCenter = new Vector3(3, 1, 0);
            _renderer.DrawQuad(quadCenter, Quaternion.identity, new Vector2(1, 1), new Color(0, 0, 1, 0.5f));

            // Log demo message
            if (Time.frameCount % 120 == 0)
            {
                Log.Info("HDRP Demo", $"Frame {Time.frameCount}: Rendering shapes with HDRP");
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
