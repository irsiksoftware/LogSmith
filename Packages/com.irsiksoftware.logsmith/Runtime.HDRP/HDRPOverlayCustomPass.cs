#if LOGSMITH_HDRP_PRESENT
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using IrsikSoftware.LogSmith.Core;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.HDRP
{
    /// <summary>
    /// HDRP Custom Pass for rendering visual debug shapes.
    /// Add this to a Custom Pass Volume to enable visual debug rendering.
    /// Injection point: After Post Process.
    /// </summary>
    public class HDRPOverlayCustomPass : CustomPass
    {
        private Material _shapeMaterial;
        private readonly List<DebugShape> _shapes = new List<DebugShape>();

        [SerializeField]
        private bool _enabled = true;

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            CreateMaterial();
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (!_enabled || _shapeMaterial == null)
                return;

            // Remove expired shapes
            float currentTime = Time.realtimeSinceStartup;
            _shapes.RemoveAll(s => s.IsExpired(currentTime));

            // Render all shapes
            foreach (var shape in _shapes)
            {
                switch (shape.Type)
                {
                    case DebugShapeType.Line:
                        RenderLine(ctx.cmd, shape, ctx);
                        break;
                    case DebugShapeType.Quad:
                        RenderQuad(ctx.cmd, shape, ctx);
                        break;
                }
            }
        }

        protected override void Cleanup()
        {
            if (_shapeMaterial != null)
            {
                Object.Destroy(_shapeMaterial);
                _shapeMaterial = null;
            }
            _shapes.Clear();
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
        /// Sets whether this custom pass is enabled.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        private void CreateMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[HDRPOverlayCustomPass] Could not find Hidden/Internal-Colored shader");
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

        private void RenderLine(CommandBuffer cmd, DebugShape shape, CustomPassContext ctx)
        {
            cmd.BeginSample("Draw HDRP Debug Line");

            var mesh = new Mesh();
            mesh.vertices = new[] { shape.Start, shape.End };
            mesh.colors = new[] { shape.Color, shape.Color };
            mesh.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);

            cmd.DrawMesh(mesh, Matrix4x4.identity, _shapeMaterial, 0, 0);

            cmd.EndSample("Draw HDRP Debug Line");
        }

        private void RenderQuad(CommandBuffer cmd, DebugShape shape, CustomPassContext ctx)
        {
            cmd.BeginSample("Draw HDRP Debug Quad");

            Vector3 center = shape.Start;
            float halfWidth = shape.Size.x * 0.5f;
            float halfHeight = shape.Size.y * 0.5f;

            Camera camera = ctx.hdCamera.camera;
            Vector3 right = camera.transform.right;
            Vector3 up = camera.transform.up;

            Vector3 v0 = center - right * halfWidth - up * halfHeight;
            Vector3 v1 = center + right * halfWidth - up * halfHeight;
            Vector3 v2 = center + right * halfWidth + up * halfHeight;
            Vector3 v3 = center - right * halfWidth + up * halfHeight;

            var mesh = new Mesh();
            mesh.vertices = new[] { v0, v1, v2, v3 };
            mesh.colors = new[] { shape.Color, shape.Color, shape.Color, shape.Color };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };

            cmd.DrawMesh(mesh, Matrix4x4.identity, _shapeMaterial, 0, 0);

            cmd.EndSample("Draw HDRP Debug Quad");
        }
    }
}
#endif
