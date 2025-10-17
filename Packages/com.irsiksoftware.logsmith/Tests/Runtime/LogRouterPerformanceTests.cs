using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Performance tests for LogRouter to ensure routing and dispatch operations meet performance budgets.
    /// Part of issue #21: Performance Budget & Benchmarks
    /// Target: < 0.2ms/frame at 1k msgs/sec (< 200µs per message)
    /// </summary>
    [TestFixture]
    public class LogRouterPerformanceTests
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

        private LogMessage CreateTestMessage()
        {
            return new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Performance",
                Message = "Performance test message",
                Timestamp = DateTime.UtcNow,
                Frame = 100,
                ThreadId = 1,
                ThreadName = "Main"
            };
        }

        [Test]
        public void Route_SingleSink_CompletesUnder200Microseconds()
        {
            // Arrange
            const int iterations = 1000;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Must meet < 200µs per message target (< 0.2ms/frame at 1k msg/sec)
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 200.0,
                $"Routing to single sink took {averageMicroseconds:F2}µs average, expected < 200µs (0.2ms target)");

            UnityEngine.Debug.Log($"[PERF] Single sink routing: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void Route_MultipleSinks_CompletesUnder500Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            var sink2 = new TestSink();
            var sink3 = new TestSink();
            _router.RegisterSink(sink2);
            _router.RegisterSink(sink3);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Three sinks should still be reasonably fast
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 500.0,
                $"Routing to 3 sinks took {averageMicroseconds:F2}µs average, expected < 500µs");

            UnityEngine.Debug.Log($"[PERF] Multi sink routing (3 sinks): {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void Route_WithCategoryFilter_CompletesUnder250Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            _router.SetCategoryFilter("Performance", LogLevel.Info);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Filtering should have minimal overhead
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 250.0,
                $"Routing with category filter took {averageMicroseconds:F2}µs average, expected < 250µs");

            UnityEngine.Debug.Log($"[PERF] Filtered routing: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void Route_WithGlobalMinimumLevel_CompletesUnder200Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            _router.SetGlobalMinimumLevel(LogLevel.Info);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Global minimum level check should be fast
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 200.0,
                $"Routing with global minimum level took {averageMicroseconds:F2}µs average, expected < 200µs");

            UnityEngine.Debug.Log($"[PERF] Global level routing: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void Route_FilteredOut_CompletesUnder100Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            _router.SetGlobalMinimumLevel(LogLevel.Error); // Filter out Info messages

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage); // This should be filtered out
            }
            stopwatch.Stop();

            // Assert - Filtered messages should be even faster (early exit)
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 100.0,
                $"Routing filtered messages took {averageMicroseconds:F2}µs average, expected < 100µs (fast rejection)");

            UnityEngine.Debug.Log($"[PERF] Filtered-out routing: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void RegisterSink_CompletesUnder50Microseconds()
        {
            // Arrange
            const int iterations = 1000;
            var sinks = new List<TestSink>();
            for (int i = 0; i < iterations; i++)
            {
                sinks.Add(new TestSink());
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            foreach (var sink in sinks)
            {
                _router.RegisterSink(sink);
            }
            stopwatch.Stop();

            // Assert - Sink registration should be very fast
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 50.0,
                $"Sink registration took {averageMicroseconds:F2}µs average, expected < 50µs");

            UnityEngine.Debug.Log($"[PERF] Sink registration: {averageMicroseconds:F2}µs average ({iterations} iterations)");
        }

        [Test]
        public void Subscribe_WithMainThreadDispatch_MeetsPerformanceTarget()
        {
            // Arrange
            const int iterations = 1000;
            int receivedCount = 0;
            var subscription = _router.Subscribe(msg => { receivedCount++; });

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Process queued subscriber notifications
            MainThreadDispatcher.Instance.ProcessQueue();

            // Assert - Subscription dispatch should not significantly impact routing performance
            var averageMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 300.0,
                $"Routing with subscription took {averageMicroseconds:F2}µs average, expected < 300µs");

            // Verify subscribers actually received messages
            Assert.AreEqual(iterations, receivedCount, "All messages should be received by subscriber");

            UnityEngine.Debug.Log($"[PERF] Routing with subscription: {averageMicroseconds:F2}µs average ({iterations} iterations)");

            subscription.Dispose();
        }

        [Test]
        public void Route_HighThroughput_MaintainsPerformance()
        {
            // Arrange - Simulate 1000 messages/second for 1 second
            const int messagesPerSecond = 1000;
            const int durationSeconds = 1;
            const int totalMessages = messagesPerSecond * durationSeconds;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < totalMessages; i++)
            {
                _router.Route(_testMessage);
            }
            stopwatch.Stop();

            // Assert - Must process 1000 msg/sec with < 0.2ms/frame budget
            var totalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var averageMicroseconds = (totalMilliseconds * 1000.0) / totalMessages;

            Assert.Less(averageMicroseconds, 200.0,
                $"High throughput (1k msg/sec) took {averageMicroseconds:F2}µs average, expected < 200µs");

            Assert.Less(totalMilliseconds, durationSeconds * 1000 * 0.2,
                $"Total time {totalMilliseconds:F2}ms exceeds 0.2ms/frame budget for {totalMessages} messages");

            UnityEngine.Debug.Log($"[PERF] High throughput: {averageMicroseconds:F2}µs/msg, {totalMilliseconds:F2}ms total for {totalMessages} messages");
        }

        [Test]
        public void Route_ConcurrentAccess_MaintainsThreadSafety()
        {
            // Arrange
            const int threadCount = 4;
            const int messagesPerThread = 250;
            var threads = new System.Threading.Thread[threadCount];
            var results = new double[threadCount];

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadIndex = t;
                threads[t] = new System.Threading.Thread(() =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    for (int i = 0; i < messagesPerThread; i++)
                    {
                        _router.Route(_testMessage);
                    }
                    stopwatch.Stop();
                    results[threadIndex] = (stopwatch.Elapsed.TotalMilliseconds * 1000.0) / messagesPerThread;
                });
                threads[t].Start();
            }

            for (int t = 0; t < threadCount; t++)
            {
                threads[t].Join();
            }

            // Assert - Concurrent routing should maintain performance
            foreach (var avgMicroseconds in results)
            {
                Assert.Less(avgMicroseconds, 400.0,
                    $"Concurrent routing took {avgMicroseconds:F2}µs average, expected < 400µs (accounts for lock contention)");
            }

            var overallAvg = 0.0;
            foreach (var result in results) overallAvg += result;
            overallAvg /= threadCount;

            UnityEngine.Debug.Log($"[PERF] Concurrent routing ({threadCount} threads): {overallAvg:F2}µs average");
        }

        /// <summary>
        /// Test sink that does minimal work (just counts messages).
        /// </summary>
        private class TestSink : ILogSink
        {
            public string Name => "TestSink";
            public int WriteCount { get; private set; }

            public void Write(LogMessage message)
            {
                WriteCount++;
            }

            public void Flush() { }
        }
    }
}
