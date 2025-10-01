using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.HDRP;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for HDRP adapter.
    /// </summary>
    public class HDRPAdapterTests
    {
        private GameObject _cameraObject;
        private Camera _camera;
        private HDRPAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("Test Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _adapter = new HDRPAdapter();
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
        public void IsAvailable_ReturnsExpectedValue()
        {
            // This depends on compile-time defines
#if LOGSMITH_HDRP_PRESENT
            Assert.IsTrue(HDRPAdapter.IsAvailable);
#else
            Assert.IsFalse(HDRPAdapter.IsAvailable);
#endif
        }

        [Test]
        public void Initialize_CreatesVisualDebugRenderer_WhenHDRPAvailable()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: false);
            Assert.IsNotNull(_adapter.VisualDebugRenderer);
#else
            _adapter.Initialize(_camera, enabled: false);
            Assert.IsNull(_adapter.VisualDebugRenderer);
#endif
        }

        [Test]
        public void Initialize_EnablesRendererWhenRequested()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: true);
            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void Initialize_DisablesRendererByDefault()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: false);
            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanDrawLine_WhenHDRPAvailable()
        {
#if LOGSMITH_HDRP_PRESENT
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
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanDrawQuad_WhenHDRPAvailable()
        {
#if LOGSMITH_HDRP_PRESENT
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
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanClear_WhenHDRPAvailable()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: true);

            _adapter.VisualDebugRenderer.DrawLine(Vector3.zero, Vector3.one, Color.red);
            _adapter.VisualDebugRenderer.DrawQuad(Vector3.zero, Vector2.one, Color.blue);

            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.Clear();
            });
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanToggleEnabled_WhenHDRPAvailable()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: false);

            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = true;
            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = false;
            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }

        [Test]
        public void Cleanup_RemovesRenderer()
        {
#if LOGSMITH_HDRP_PRESENT
            _adapter.Initialize(_camera, enabled: true);
            _adapter.Cleanup();

            Assert.IsNull(_adapter.VisualDebugRenderer);
#else
            // Skip test when HDRP not available
            Assert.Pass("HDRP not available, skipping test");
#endif
        }
    }
}
