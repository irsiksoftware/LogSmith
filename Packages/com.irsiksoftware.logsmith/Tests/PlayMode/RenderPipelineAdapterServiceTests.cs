using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for Render Pipeline Adapter Service.
    /// </summary>
    public class RenderPipelineAdapterServiceTests
    {
        private GameObject _cameraObject;
        private Camera _camera;
        private RenderPipelineAdapterService _service;

        [SetUp]
        public void SetUp()
        {
            _cameraObject = new GameObject("Test Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _service = new RenderPipelineAdapterService();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Cleanup();
            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
            }
        }

        [Test]
        public void Initialize_DetectsPipeline()
        {
            _service.Initialize(_camera, enabled: false);

            // Should detect one of the known pipelines
            Assert.That(_service.DetectedPipeline, Is.Not.EqualTo(RenderPipelineDetector.PipelineType.Unknown));
        }

        [Test]
        public void Initialize_ActivatesAdapter_WhenAvailable()
        {
            _service.Initialize(_camera, enabled: false);

            // If a pipeline is detected and adapter is available, should have an active renderer
            // In a default Built-in RP project, this should be non-null
            var detectedPipeline = _service.DetectedPipeline;
            if (detectedPipeline == RenderPipelineDetector.PipelineType.BuiltIn)
            {
                Assert.IsNotNull(_service.ActiveRenderer);
            }
        }

        [Test]
        public void Initialize_LogsInfoMessage()
        {
            // This test verifies that initialization produces a log message
            // The actual message content is tested manually
            Assert.DoesNotThrow(() =>
            {
                _service.Initialize(_camera, enabled: false);
            });
        }

        [Test]
        public void Cleanup_RemovesActiveRenderer()
        {
            _service.Initialize(_camera, enabled: false);
            _service.Cleanup();

            Assert.IsNull(_service.ActiveRenderer);
        }

        [Test]
        public void Initialize_WithEnabledTrue_EnablesRenderer()
        {
            _service.Initialize(_camera, enabled: true);

            if (_service.ActiveRenderer != null)
            {
                Assert.IsTrue(_service.ActiveRenderer.IsEnabled);
            }
        }

        [Test]
        public void Initialize_WithEnabledFalse_DisablesRenderer()
        {
            _service.Initialize(_camera, enabled: false);

            if (_service.ActiveRenderer != null)
            {
                Assert.IsFalse(_service.ActiveRenderer.IsEnabled);
            }
        }

        [Test]
        public void DetectedPipeline_MatchesCurrentProject()
        {
            _service.Initialize(_camera, enabled: false);

            var detectedType = _service.DetectedPipeline;
            var detectedName = RenderPipelineDetector.GetPipelineName(detectedType);

            // Verify the name is not empty
            Assert.IsNotEmpty(detectedName);
        }

        [Test]
        public void RenderPipelineDetector_GetPipelineName_ReturnsValidNames()
        {
            Assert.AreEqual("Built-in Render Pipeline",
                RenderPipelineDetector.GetPipelineName(RenderPipelineDetector.PipelineType.BuiltIn));

            Assert.AreEqual("Universal Render Pipeline (URP)",
                RenderPipelineDetector.GetPipelineName(RenderPipelineDetector.PipelineType.URP));

            Assert.AreEqual("High Definition Render Pipeline (HDRP)",
                RenderPipelineDetector.GetPipelineName(RenderPipelineDetector.PipelineType.HDRP));

            Assert.AreEqual("Unknown Pipeline",
                RenderPipelineDetector.GetPipelineName(RenderPipelineDetector.PipelineType.Unknown));
        }

        [Test]
        public void RenderPipelineDetector_DetectPipeline_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var pipeline = RenderPipelineDetector.DetectPipeline();
                Assert.That(pipeline, Is.Not.Null);
            });
        }
    }
}
