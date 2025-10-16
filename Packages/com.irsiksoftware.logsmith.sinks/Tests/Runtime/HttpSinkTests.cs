using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Sinks;
using System;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Sinks.Tests
{
    [TestFixture]
    public class HttpSinkTests
    {
        private class TestCoroutineRunner : MonoBehaviour { }

        [Test]
        public void HttpSink_Constructor_ThrowsOnNullEndpoint()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            Assert.Throws<ArgumentNullException>(() => new HttpSink(null, coroutineRunner: runner));
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }

        [Test]
        public void HttpSink_Constructor_ThrowsOnNullCoroutineRunner()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpSink("http://localhost:8080", coroutineRunner: null));
        }

        [Test]
        public void HttpSink_HasCorrectName()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new HttpSink("http://localhost:8080", coroutineRunner: runner);
            Assert.AreEqual("HTTP", sink.Name);
            sink.Dispose();
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }

        [Test]
        public void HttpSink_Write_DoesNotThrow()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new HttpSink("http://localhost:8080", batchSize: 100, coroutineRunner: runner);

            var message = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Test message",
                Timestamp = DateTime.UtcNow,
                Frame = 0,
                ThreadId = 1
            };

            Assert.DoesNotThrow(() => sink.Write(message));
            sink.Dispose();
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }

        [Test]
        public void HttpSink_Dispose_CanBeCalledMultipleTimes()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new HttpSink("http://localhost:8080", coroutineRunner: runner);

            sink.Dispose();
            Assert.DoesNotThrow(() => sink.Dispose());
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }
    }
}
