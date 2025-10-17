using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// PlayMode performance tests for LogRouter to validate real-world runtime performance.
    /// Part of issue #21: Performance Budget & Benchmarks
    /// Target: < 0.2ms/frame at 1k msgs/sec (< 200µs per message)
    /// </summary>
    [TestFixture]
    public class LogRouterPlayModePerformanceTests
    {
        private LogRouter _router;
        private TestSink _sink;
        private LogMessage _testMessage;

        [SetUp]
        public void Setup()
        {
            _router = new LogRouter();
            _sink = new TestSink();
            _router.RegisterSink(_sink);
            _testMessage = CreateTestMessage();
        }

        [TearDown]
        public void TearDown()
        {
            _router = null;
            _sink = null;
        }

        private LogMessage CreateTestMessage()
        {
            // Get frame count safely - Time.frameCount can only be called from main thread
            int frameCount = -1;
            try
            {
                frameCount = Time.frameCount;
            }
            catch (UnityEngine.UnityException)
            {
                // Called from background thread - frameCount unavailable
            }

            return new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Performance",
                Message = "PlayMode performance test message",
                Timestamp = DateTime.UtcNow,
                Frame = frameCount,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                ThreadName = "Main"
            };
        }

        [UnityTest]
        public IEnumerator RuntimeRouting_SingleSink_CompletesUnder200Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            yield return null; // Wait one frame to ensure Unity is ready

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 200.0,
                $"PlayMode routing took {averageMicroseconds:F2}µs average, expected < 200µs");

            UnityEngine.Debug.Log($"[PERF][PlayMode] Single sink routing: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [UnityTest]
        public IEnumerator RuntimeRouting_HighThroughput_MaintainsFrameBudget()
        {
            // Arrange - Simulate 1000 messages/second
            const int messagesPerFrame = 1000;
            yield return null; // Wait one frame

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < messagesPerFrame; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Must stay under 0.25ms/frame budget (with buffer for system variance/debug builds)
            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            Assert.Less(totalMilliseconds, 0.25,
                $"PlayMode high throughput took {totalMilliseconds:F3}ms, expected < 0.25ms/frame");

            UnityEngine.Debug.Log($"[PERF][PlayMode] High throughput: {totalMilliseconds:F3}ms for {messagesPerFrame} messages");
        }

        [UnityTest]
        public IEnumerator RuntimeRouting_MultipleSinks_MeetsPerformanceTarget()
        {
            // Arrange
            const int iterations = 1000;
            var sink2 = new TestSink();
            var sink3 = new TestSink();
            _router.RegisterSink(sink2);
            _router.RegisterSink(sink3);
            yield return null;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 500.0,
                $"PlayMode multi-sink routing took {averageMicroseconds:F2}µs average, expected < 500µs");

            UnityEngine.Debug.Log($"[PERF][PlayMode] Multi sink routing (3 sinks): {averageMicroseconds:F2}µs average");
        }

        [UnityTest]
        public IEnumerator RuntimeRouting_WithFiltering_FastRejection()
        {
            // Arrange
            const int iterations = 1000;
            _router.SetGlobalMinimumLevel(LogLevel.Error); // Filter out Info messages
            yield return null;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage); // Should be filtered out
            }
            stopwatch.Stop();

            // Assert - Filtered messages should be very fast
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 100.0,
                $"PlayMode filtered routing took {averageMicroseconds:F2}µs average, expected < 100µs");

            UnityEngine.Debug.Log($"[PERF][PlayMode] Filtered routing: {averageMicroseconds:F2}µs average");
        }

        [UnityTest]
        public IEnumerator RuntimeRouting_AcrossMultipleFrames_SustainedPerformance()
        {
            // Arrange - Test sustained performance over 10 frames
            const int framesToTest = 10;
            const int messagesPerFrame = 100;
            var frameTimes = new float[framesToTest];

            // Act
            for (int frame = 0; frame < framesToTest; frame++)
            {
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < messagesPerFrame; i++)
                {
                    _testMessage.Frame = Time.frameCount;
                    _router.Route(_testMessage);
                }
                stopwatch.Stop();
                frameTimes[frame] = (float)stopwatch.Elapsed.TotalMilliseconds;

                yield return null; // Next frame
            }

            // Assert - Each frame should be under budget
            for (int i = 0; i < framesToTest; i++)
            {
                Assert.Less(frameTimes[i], 0.2f,
                    $"Frame {i} took {frameTimes[i]:F3}ms for {messagesPerFrame} messages, expected < 0.2ms");
            }

            var avgFrameTime = 0f;
            foreach (var time in frameTimes) avgFrameTime += time;
            avgFrameTime /= framesToTest;

            UnityEngine.Debug.Log($"[PERF][PlayMode] Sustained performance: {avgFrameTime:F3}ms average over {framesToTest} frames");
        }

        /// <summary>
        /// Minimal test sink for performance testing.
        /// </summary>
        private class TestSink : ILogSink
        {
            public string Name => "PlayModeTestSink";
            public int WriteCount { get; private set; }

            public void Write(LogMessage message)
            {
                WriteCount++;
            }

            public void Flush() { }
        }
    }
}
