using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Sinks;
using System;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Sinks.Tests
{
    [TestFixture]
    public class SeqSinkTests
    {
        private class TestCoroutineRunner : MonoBehaviour { }

        [Test]
        public void SeqSink_Constructor_ThrowsOnNullServerUrl()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            Assert.Throws<ArgumentNullException>(() => new SeqSink(null, coroutineRunner: runner));
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }

        [Test]
        public void SeqSink_Constructor_ThrowsOnNullCoroutineRunner()
        {
            Assert.Throws<ArgumentNullException>(() => new SeqSink("http://localhost:5341", coroutineRunner: null));
        }

        [Test]
        public void SeqSink_HasCorrectName()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new SeqSink("http://localhost:5341", coroutineRunner: runner);
            Assert.AreEqual("Seq", sink.Name);
            sink.Dispose();
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }

        [Test]
        public void SeqSink_Write_DoesNotThrow()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new SeqSink("http://localhost:5341", batchSize: 100, coroutineRunner: runner);

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
        public void SeqSink_Dispose_CanBeCalledMultipleTimes()
        {
            var runner = new GameObject().AddComponent<TestCoroutineRunner>();
            var sink = new SeqSink("http://localhost:5341", coroutineRunner: runner);

            sink.Dispose();
            Assert.DoesNotThrow(() => sink.Dispose());
            UnityEngine.Object.DestroyImmediate(runner.gameObject);
        }
    }
}
