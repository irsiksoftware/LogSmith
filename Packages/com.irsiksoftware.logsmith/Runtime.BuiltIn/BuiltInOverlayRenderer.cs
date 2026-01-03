using System.Collections.Generic;
using IrsikSoftware.LogSmith.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace IrsikSoftware.LogSmith.BuiltIn
{
    /// <summary>
    /// Visual debug renderer for Built-in Render Pipeline.
    /// Uses Camera.AddCommandBuffer to inject rendering commands after everything else.
    /// </summary>
    public class BuiltInOverlayRenderer : IVisualDebugRenderer
    {
        private Camera _targetCamera;
        private CommandBuffer _commandBuffer;
        private Material _lineMaterial;
        private Material _quadMaterial;
        private readonly List<DebugShape> _shapes = new List<DebugShape>();
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;

                if (_isEnabled && _targetCamera != null)
                {
                    AttachToCamera();
                }
                else if (!_isEnabled && _targetCamera != null)
                {
                    DetachFromCamera();
                }
            }
        }

        public void Initialize(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("[BuiltInOverlayRenderer] Cannot initialize with null camera");
                return;
            }

            _targetCamera = camera;
            _commandBuffer = new CommandBuffer { name = "LogSmith Built-in Visual Debug" };

            CreateMaterials();

            if (_isEnabled)
            {
                AttachToCamera();
            }
        }

        public void Cleanup()
        {
            DetachFromCamera();

            if (_commandBuffer != null)
            {
                _commandBuffer.Release();
                _commandBuffer = null;
            }

            if (_lineMaterial != null)
            {
                Object.Destroy(_lineMaterial);
                _lineMaterial = null;
            }

            if (_quadMaterial != null)
            {
                Object.Destroy(_quadMaterial);
                _quadMaterial = null;
            }

            _shapes.Clear();
            _targetCamera = null;
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f)
        {
            var shape = new DebugShape
            {
                Type = DebugShapeType.Line,
                Start = start,
                End = end,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _shapes.Add(shape);
            RebuildCommandBuffer();
        }

        public void DrawQuad(Vector3 center, Vector2 size, Color color, float duration = 0f)
        {
            var shape = new DebugShape
            {
                Type = DebugShapeType.Quad,
                Start = center,
                Size = size,
                Color = color,
                ExpiryTime = duration > 0f ? Time.realtimeSinceStartup + duration : 0f
            };

            _shapes.Add(shape);
            RebuildCommandBuffer();
        }

        public void Clear()
        {
            _shapes.Clear();
            RebuildCommandBuffer();
        }

        private void CreateMaterials()
        {
            // Create unlit material for lines and quads
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogError("[BuiltInOverlayRenderer] Could not find Hidden/Internal-Colored shader");
                return;
            }

            _lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            _lineMaterial.SetInt("_ZWrite", 0);
            _lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);

            _quadMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _quadMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _quadMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _quadMaterial.SetInt("_Cull", (int)CullMode.Off);
            _quadMaterial.SetInt("_ZWrite", 0);
            _quadMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        private void AttachToCamera()
        {
            if (_targetCamera == null || _commandBuffer == null)
                return;

            // Remove existing to avoid duplicates
            DetachFromCamera();

            _targetCamera.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
            RebuildCommandBuffer();
        }

        private void DetachFromCamera()
        {
            if (_targetCamera == null || _commandBuffer == null)
                return;

            _targetCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
        }

        private void RebuildCommandBuffer()
        {
            if (_commandBuffer == null)
                return;

            _commandBuffer.Clear();

            // Remove expired shapes
            var currentTime = Time.realtimeSinceStartup;
            _shapes.RemoveAll(s => s.IsExpired(currentTime));

            // Render all shapes
            foreach (var shape in _shapes)
            {
                switch (shape.Type)
                {
                    case DebugShapeType.Line:
                        RenderLine(shape);
                        break;
                    case DebugShapeType.Quad:
                        RenderQuad(shape);
                        break;
                }
            }
        }

        private void RenderLine(DebugShape shape)
        {
            if (_lineMaterial == null)
                return;

            // Use GL.LINES to draw a simple line
            _commandBuffer.BeginSample("Draw Debug Line");

            var mesh = new Mesh
            {
                vertices = new[] { shape.Start, shape.End },
                colors = new[] { shape.Color, shape.Color }
            };
            mesh.SetIndices(new[] { 0, 1 }, MeshTopology.Lines, 0);

            _commandBuffer.DrawMesh(mesh, Matrix4x4.identity, _lineMaterial, 0, 0);

            _commandBuffer.EndSample("Draw Debug Line");
        }

        private void RenderQuad(DebugShape shape)
        {
            if (_quadMaterial == null)
                return;

            _commandBuffer.BeginSample("Draw Debug Quad");

            // Create a quad mesh centered at shape.Start
            Vector3 center = shape.Start;
            var halfWidth = shape.Size.x * 0.5f;
            var halfHeight = shape.Size.y * 0.5f;

            // Build quad facing camera
            Vector3 right = _targetCamera != null ? _targetCamera.transform.right : Vector3.right;
            Vector3 up = _targetCamera != null ? _targetCamera.transform.up : Vector3.up;

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

            _commandBuffer.DrawMesh(mesh, Matrix4x4.identity, _quadMaterial, 0, 0);

            _commandBuffer.EndSample("Draw Debug Quad");
        }
    }
}
