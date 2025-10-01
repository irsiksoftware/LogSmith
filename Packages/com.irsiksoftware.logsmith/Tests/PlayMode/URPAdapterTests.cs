using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.URP;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for URP adapter.
    /// </summary>
    public class URPAdapterTests
    {
        private GameObject _cameraObject;
        private Camera _camera;
        private URPAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("Test Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _adapter = new URPAdapter();
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
#if LOGSMITH_URP_PRESENT
            Assert.IsTrue(URPAdapter.IsAvailable);
#else
            Assert.IsFalse(URPAdapter.IsAvailable);
#endif
        }

        [Test]
        public void Initialize_CreatesVisualDebugRenderer_WhenURPAvailable()
        {
#if LOGSMITH_URP_PRESENT
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
#if LOGSMITH_URP_PRESENT
            _adapter.Initialize(_camera, enabled: true);
            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void Initialize_DisablesRendererByDefault()
        {
#if LOGSMITH_URP_PRESENT
            _adapter.Initialize(_camera, enabled: false);
            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanDrawLine_WhenURPAvailable()
        {
#if LOGSMITH_URP_PRESENT
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
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanDrawQuad_WhenURPAvailable()
        {
#if LOGSMITH_URP_PRESENT
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
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanClear_WhenURPAvailable()
        {
#if LOGSMITH_URP_PRESENT
            _adapter.Initialize(_camera, enabled: true);

            _adapter.VisualDebugRenderer.DrawLine(Vector3.zero, Vector3.one, Color.red);
            _adapter.VisualDebugRenderer.DrawQuad(Vector3.zero, Vector2.one, Color.blue);

            Assert.DoesNotThrow(() =>
            {
                _adapter.VisualDebugRenderer.Clear();
            });
#else
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void VisualDebugRenderer_CanToggleEnabled_WhenURPAvailable()
        {
#if LOGSMITH_URP_PRESENT
            _adapter.Initialize(_camera, enabled: false);

            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = true;
            Assert.IsTrue(_adapter.VisualDebugRenderer.IsEnabled);

            _adapter.VisualDebugRenderer.IsEnabled = false;
            Assert.IsFalse(_adapter.VisualDebugRenderer.IsEnabled);
#else
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }

        [Test]
        public void Cleanup_RemovesRenderer()
        {
#if LOGSMITH_URP_PRESENT
            _adapter.Initialize(_camera, enabled: true);
            _adapter.Cleanup();

            Assert.IsNull(_adapter.VisualDebugRenderer);
#else
            // Skip test when URP not available
            Assert.Pass("URP not available, skipping test");
#endif
        }
    }
}
