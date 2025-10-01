using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Performance tests for MessageTemplateEngine to ensure formatting operations meet performance budgets.
    /// Part of issue #23 acceptance criteria: "IMessageTemplateEngine: tokens, formatting, malformed tokens, perf"
    /// </summary>
    [TestFixture]
    public class MessageTemplateEnginePerformanceTests
    {
        private MessageTemplateEngine _engine;
        private LogMessage _testMessage;

        [SetUp]
        public void Setup()
        {
            _engine = new MessageTemplateEngine();
            _testMessage = CreateTestMessage();
        }

        private LogMessage CreateTestMessage()
        {
            return new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Performance",
                Message = "Performance test message with reasonable length",
                Timestamp = new DateTime(2025, 1, 15, 14, 30, 45, DateTimeKind.Utc),
                Frame = 1000,
                ThreadId = 42,
                ThreadName = "MainThread",
                CallerFilePath = "TestFile.cs",
                CallerMemberName = "TestMethod",
                StackTrace = "at TestMethod() in TestFile.cs:line 100",
                Context = new Dictionary<string, object>
                {
                    { "userId", "user123" },
                    { "sessionId", 456 },
                    { "requestId", "req-789" }
                }
            };
        }

        [Test]
        public void Format_SimpleTemplate_CompletesUnder1Millisecond()
        {
            // Arrange
            const int iterations = 1000;
            var template = "{timestamp} [{level}] {category}: {message}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, MessageFormat.Text);
            }
            stopwatch.Stop();

            // Assert - Should average well under 1ms per format operation
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 1.0, $"Simple template formatting took {averageMs:F4}ms average, expected < 1ms");
        }

        [Test]
        public void Format_ComplexTemplate_CompletesUnder2Milliseconds()
        {
            // Arrange
            const int iterations = 1000;
            var template = "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {category} | Frame:{frame} Thread:{thread} Memory:{memoryMB}MB | {message} | {file}:{method} | Context: {context}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, MessageFormat.Text);
            }
            stopwatch.Stop();

            // Assert - Complex templates should still be fast
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 2.0, $"Complex template formatting took {averageMs:F4}ms average, expected < 2ms");
        }

        [Test]
        public void Format_JsonFormat_CompletesUnder2Milliseconds()
        {
            // Arrange
            const int iterations = 1000;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, MessageFormat.Json);
            }
            stopwatch.Stop();

            // Assert - JSON formatting should be efficient
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 2.0, $"JSON formatting took {averageMs:F4}ms average, expected < 2ms");
        }

        [Test]
        public void Format_WithContextTokens_CompletesUnder1Millisecond()
        {
            // Arrange
            const int iterations = 1000;
            var template = "User: {userId}, Session: {sessionId}, Request: {requestId} - {message}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, MessageFormat.Text);
            }
            stopwatch.Stop();

            // Assert - Context token resolution should be fast
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 1.0, $"Context token formatting took {averageMs:F4}ms average, expected < 1ms");
        }

        [Test]
        public void Format_MalformedTokens_DoesNotDegradePerformance()
        {
            // Arrange
            const int iterations = 1000;
            var template = "{unknownToken1} {unknownToken2} {message} {unknownToken3}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, MessageFormat.Text);
            }
            stopwatch.Stop();

            // Assert - Malformed tokens should not cause performance degradation
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 1.0, $"Malformed token handling took {averageMs:F4}ms average, expected < 1ms");
        }

        [Test]
        public void SetCategoryTemplate_UpdateTemplate_CompletesUnder100Microseconds()
        {
            // Arrange
            const int iterations = 10000;
            var template = "{timestamp} [{level}] {message}";

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.SetCategoryTemplate("Performance", template);
            }
            stopwatch.Stop();

            // Assert - Template updates should be very fast
            var averageMicroseconds = (stopwatch.ElapsedMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 100.0, $"Template updates took {averageMicroseconds:F2}µs average, expected < 100µs");
        }

        [Test]
        public void Format_ConcurrentAccess_MaintainsPerformance()
        {
            // Arrange
            const int threadCount = 4;
            const int iterationsPerThread = 250; // 1000 total iterations
            var threads = new System.Threading.Thread[threadCount];
            var results = new double[threadCount];

            var template = "[{timestamp:HH:mm:ss}] [{level}] {category}: {message}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            for (int t = 0; t < threadCount; t++)
            {
                int threadIndex = t;
                threads[t] = new System.Threading.Thread(() =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    for (int i = 0; i < iterationsPerThread; i++)
                    {
                        _engine.Format(_testMessage, MessageFormat.Text);
                    }
                    stopwatch.Stop();
                    results[threadIndex] = stopwatch.ElapsedMilliseconds / (double)iterationsPerThread;
                });
                threads[t].Start();
            }

            // Wait for all threads
            for (int t = 0; t < threadCount; t++)
            {
                threads[t].Join();
            }

            // Assert - Concurrent access should not significantly degrade performance
            foreach (var avgMs in results)
            {
                Assert.Less(avgMs, 2.0, $"Concurrent formatting took {avgMs:F4}ms average, expected < 2ms");
            }
        }

        [Test]
        public void Format_LargeContext_CompletesUnder2Milliseconds()
        {
            // Arrange
            const int iterations = 500;
            var largeContextMessage = CreateTestMessage();

            // Add large context with many entries
            for (int i = 0; i < 20; i++)
            {
                largeContextMessage.Context[$"key{i}"] = $"value{i}";
            }

            var template = "{message} | {context}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(largeContextMessage, MessageFormat.Text);
            }
            stopwatch.Stop();

            // Assert - Large context should not cause excessive slowdown
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 2.0, $"Large context formatting took {averageMs:F4}ms average, expected < 2ms");
        }

        [Test]
        public void Format_JsonWithLargeContext_CompletesUnder3Milliseconds()
        {
            // Arrange
            const int iterations = 500;
            var largeContextMessage = CreateTestMessage();

            // Add large context with many entries and nested structures
            for (int i = 0; i < 20; i++)
            {
                largeContextMessage.Context[$"key{i}"] = $"value with special chars: \"quotes\", \nnewlines\t, and \\backslashes";
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(largeContextMessage, MessageFormat.Json);
            }
            stopwatch.Stop();

            // Assert - JSON formatting with large context and escaping
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 3.0, $"JSON with large context took {averageMs:F4}ms average, expected < 3ms");
        }

        [Test]
        public void GetCategoryTemplate_RetrieveTemplate_CompletesUnder50Microseconds()
        {
            // Arrange
            const int iterations = 10000;
            _engine.SetCategoryTemplate("Performance", "{message}");

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.GetCategoryTemplate("Performance");
            }
            stopwatch.Stop();

            // Assert - Template retrieval should be very fast
            var averageMicroseconds = (stopwatch.ElapsedMilliseconds * 1000.0) / iterations;
            Assert.Less(averageMicroseconds, 50.0, $"Template retrieval took {averageMicroseconds:F2}µs average, expected < 50µs");
        }

        [Test]
        public void Format_AlternatingFormats_CompletesUnder2Milliseconds()
        {
            // Arrange
            const int iterations = 500;
            var template = "{timestamp} [{level}] {message}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act - Alternate between Text and JSON formats
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(_testMessage, i % 2 == 0 ? MessageFormat.Text : MessageFormat.Json);
            }
            stopwatch.Stop();

            // Assert - Format switching should not cause excessive overhead
            var averageMs = stopwatch.ElapsedMilliseconds / (double)iterations;
            Assert.Less(averageMs, 2.0, $"Alternating format operations took {averageMs:F4}ms average, expected < 2ms");
        }

        [Test]
        public void Format_NoContext_FasterThanWithContext()
        {
            // Arrange
            const int iterations = 1000;
            var noContextMessage = CreateTestMessage();
            noContextMessage.Context = null;

            var withContextMessage = CreateTestMessage();

            var template = "{timestamp} [{level}] {message} {context}";
            _engine.SetCategoryTemplate("Performance", template);

            // Act - Format without context
            var stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(noContextMessage, MessageFormat.Text);
            }
            stopwatch1.Stop();
            var noContextAvg = stopwatch1.ElapsedMilliseconds / (double)iterations;

            // Act - Format with context
            var stopwatch2 = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                _engine.Format(withContextMessage, MessageFormat.Text);
            }
            stopwatch2.Stop();
            var withContextAvg = stopwatch2.ElapsedMilliseconds / (double)iterations;

            // Assert - No context should be faster (or at least not slower)
            Assert.LessOrEqual(noContextAvg, withContextAvg * 1.5,
                $"No-context formatting ({noContextAvg:F4}ms) should not be significantly slower than with-context ({withContextAvg:F4}ms)");
        }
    }
}
