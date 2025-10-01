using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using IrsikSoftware.LogSmith.BuiltIn;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for Built-in Render Pipeline adapter.
    /// </summary>
    public class BuiltInRenderPipelineAdapterTests
    {
        private GameObject _cameraObject;
        private Camera _camera;
        private BuiltInRenderPipelineAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("Test Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _adapter = new BuiltInRenderPipelineAdapter();
        }

        [TearDown]
        public void TearDown()
        {
            _adapter?.Cleanup();
            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
            }
        }

        [Test]
        public void Initialize_CreatesVisualDebugRenderer()
        {
            _adapter.Initialize(_camera, enabled: false);

            Assert.IsNotNull(_adapter.VisualDebugRenderer);
        }

        [Test]
        public void Initialize_EnablesRendererWhenRequested()
        {
            _adapter.Initialize(_camera, enabled: true);

            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);
        }

        [Test]
        public void Initialize_DisablesRendererByDefault()
        {
            _adapter.Initialize(_camera, enabled: false);

            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
        }

        [Test]
        public void VisualDebugRenderer_CanDrawLine()
        {
            _adapter.Initialize(_camera, enabled: true);

            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.DrawLine(
                    Vector3.zero,
                    Vector3.one,
                    Color.red,
                    duration: 0f
                );
            });
        }

        [Test]
        public void VisualDebugRenderer_CanDrawQuad()
        {
            _adapter.Initialize(_camera, enabled: true);

            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.DrawQuad(
                    Vector3.zero,
                    new Vector2(1f, 1f),
                    Color.blue,
                    duration: 0f
                );
            });
        }

        [Test]
        public void VisualDebugRenderer_CanClear()
        {
            _adapter.Initialize(_camera, enabled: true);

            _adapter.VisualDebugRenderer.DrawLine(Vector3.zero, Vector3.one, Color.red);
            _adapter.VisualDebugRenderer.DrawQuad(Vector3.zero, Vector2.one, Color.blue);

            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.Clear();
            });
        }

        [Test]
        public void VisualDebugRenderer_CanToggleEnabled()
        {
            _adapter.Initialize(_camera, enabled: false);

            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = true;
            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = false;
            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
        }

        [UnityTest]
        public IEnumerator VisualDebugRenderer_ExpiredShapesAreRemoved()
        {
            _adapter.Initialize(_camera, enabled: true);

            // Draw a shape with 0.1 second duration
            _adapter.VisualDebugRenderer.DrawLine(
                Vector3.zero,
                Vector3.one,
                Color.red,
                duration: 0.1f
            );

            // Wait for shape to expire
            yield return new WaitForSeconds(0.15f);

            // Shape should be expired and removed on next frame
            // We can't directly test this without exposing internals,
            // but we verify no errors occur
            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.DrawLine(Vector3.zero, Vector3.up, Color.blue);
            });
        }

        [Test]
        public void Cleanup_RemovesRenderer()
        {
            _adapter.Initialize(_camera, enabled: true);
            _adapter.Cleanup();

            Assert.IsNull(_adapter.VisualDebugRenderer);
        }

        [Test]
        public void IsActive_ReturnsTrueWhenNoOtherPipelinesAvailable()
        {
            // This depends on compile-time defines
            // In a clean Built-in RP project, should be true
#if !LOGSMITH_URP_AVAILABLE && !LOGSMITH_HDRP_AVAILABLE
            Assert.IsTrue(BuiltInRenderPipelineAdapter.IsActive);
#else
            Assert.IsFalse(BuiltInRenderPipelineAdapter.IsActive);
#endif
        }
    }
}
