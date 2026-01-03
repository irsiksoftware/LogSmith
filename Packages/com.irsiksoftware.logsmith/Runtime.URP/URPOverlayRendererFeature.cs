#if LOGSMITH_URP_PRESENT
using System.Collections.Generic;
using IrsikSoftware.LogSmith.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IrsikSoftware.LogSmith.URP
{
    /// <summary>
    /// URP ScriptableRendererFeature for rendering visual debug shapes.
    /// Add this to your URP Renderer asset to enable visual debug rendering.
    /// </summary>
    public class URPOverlayRendererFeature : ScriptableRendererFeature
    {
        private URPOverlayRenderPass _renderPass;

        [SerializeField]
        private bool _enabled = true;

        /// <summary>
        /// Called when the feature is created or re-initialized.
        /// </summary>
        public override void Create()
        {
            _renderPass = new URPOverlayRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }

        /// <summary>
        /// Injects the render pass into the renderer.
        /// </summary>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!_enabled || _renderPass == null)
                return;

            renderer.EnqueuePass(_renderPass);
        }

        /// <summary>
        /// Sets whether this renderer feature is enabled.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        protected override void Dispose(bool disposing)
        {
            _renderPass?.Cleanup();
        }
    }

    /// <summary>
    /// URP ScriptableRenderPass for rendering debug shapes.
    /// </summary>
    public class URPOverlayRenderPass : ScriptableRenderPass
    {
        private Material _shapeMaterial;
        private readonly List<DebugShape> _shapes = new List<DebugShape>();
        private URPAdapter _adapter;

        public URPOverlayRenderPass()
        {
            CreateMaterial();
        }

        /// <summary>
        /// Registers the adapter that owns this pass.
        /// </summary>
        public void RegisterAdapter(URPAdapter adapter)
        {
            _adapter = adapter;
        }

        /// <summary>
        /// Adds a debug shape to be rendered.
        /// </summary>
        public void AddShape(DebugShape shape)
        {
            _shapes.Add(shape);
        }

        /// <summary>
        /// Clears all debug shapes.
        /// </summary>
        public void ClearShapes()
        {
            _shapes.Clear();
        }

        /// <summary>
        /// Executes the render pass.
        /// </summary>
        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_shapeMaterial == null)
                return;

            CommandBuffer cmd = new CommandBuffer { name = "LogSmith URP Visual Debug" };

            // Remove expired shapes
            var currentTime = Time.realtimeSinceStartup;
            _shapes.RemoveAll(s => s.IsExpired(currentTime));

            // Render all shapes
            foreach (var shape in _shapes)
            {
                switch (shape.Type)
                {
                    case DebugShapeType.Line:
                        RenderLine(cmd, shape, ref renderingData);
                        break;
                    case DebugShapeType.Quad:
                        RenderQuad(cmd, shape, ref renderingData);
                        break;
                }
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Release();
        }

        public void Cleanup()
        {
            if (_shapeMaterial != null)
            {
                Object.Destroy(_shapeMaterial);
                _shapeMaterial = null;
            }
            _shapes.Clear();
        }

        private void CreateMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[URPOverlayRenderPass] Could not find Hidden/Internal-Colored shader");
                return;
            }

            _shapeMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _shapeMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _shapeMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _shapeMaterial.SetInt("_Cull", (int)CullMode.Off);
            _shapeMaterial.SetInt("_ZWrite", 0);
            _shapeMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        private void RenderLine(CommandBuffer cmd, DebugShape shape, ref RenderingData renderingData)
        {
            cmd.BeginSample("Draw URP Debug Line");

            var mesh = new Mesh
            {
                vertices = new[] { shape.Start, shape.End },
                colors = new[] { shape.Color, shape.Color }
            };
            mesh.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);

            cmd.DrawMesh(mesh, Matrix4x4.identity, _shapeMaterial, 0, 0);

            cmd.EndSample("Draw URP Debug Line");
        }

        private void RenderQuad(CommandBuffer cmd, DebugShape shape, ref RenderingData renderingData)
        {
            cmd.BeginSample("Draw URP Debug Quad");

            Vector3 center = shape.Start;
            var halfWidth = shape.Size.x * 0.5f;
            var halfHeight = shape.Size.y * 0.5f;

            Camera camera = renderingData.cameraData.camera;
            Vector3 right = camera.transform.right;
            Vector3 up = camera.transform.up;

            Vector3 v0 = center - right * halfWidth - up * halfHeight;
            Vector3 v1 = center + right * halfWidth - up * halfHeight;
            Vector3 v2 = center + right * halfWidth + up * halfHeight;
            Vector3 v3 = center - right * halfWidth + up * halfHeight;

            var mesh = new Mesh
            {
                vertices = new[] { v0, v1, v2, v3 },
                colors = new[] { shape.Color, shape.Color, shape.Color, shape.Color },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };

            cmd.DrawMesh(mesh, Matrix4x4.identity, _shapeMaterial, 0, 0);

            cmd.EndSample("Draw URP Debug Quad");
        }
    }
}
#endif
