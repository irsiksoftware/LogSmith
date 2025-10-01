using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace IrsikSoftware.LogSmith.Tests.PlayMode
{
    /// <summary>
    /// Stress tests for thread safety with multi-threaded log producers.
    /// Validates issue #20: "ILog callable from any thread" and "UI subscribers receive events on main thread."
    /// </summary>
    [TestFixture]
    public class ThreadSafetyStressTests
    {
        private LogRouter _router;
        private LogSmithLogger _logger;
        private TestLogSink _sink;
        private List<LogMessage> _subscriberMessages;
        private List<int> _subscriberThreadIds;
        private int _mainThreadId;

        [SetUp]
        public void Setup()
        {
            _router = new LogRouter();
            _logger = new LogSmithLogger(_router, "StressTest");
            _sink = new TestLogSink("TestSink");
            _router.RegisterSink(_sink);
            _subscriberMessages = new List<LogMessage>();
            _subscriberThreadIds = new List<int>();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        [TearDown]
        public void TearDown()
        {
            _router = null;
            _logger = null;
            _sink = null;
            _subscriberMessages = null;
            _subscriberThreadIds = null;
        }

        [UnityTest]
        public IEnumerator MultiThreadedLogging_NoDataRaces()
        {
            // Arrange: Launch multiple threads producing logs simultaneously
            const int threadCount = 10;
            const int messagesPerThread = 100;
            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            // Act: Spawn producer threads
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                var thread = new Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < messagesPerThread; j++)
                        {
                            _logger.Info($"Thread {threadIndex} - Message {j}");
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Wait a frame for any pending logs to flush
            yield return null;

            // Assert: No exceptions and all messages logged
            Assert.AreEqual(0, exceptions.Count, $"Expected no exceptions, but got: {string.Join(", ", exceptions)}");
            Assert.AreEqual(threadCount * messagesPerThread, _sink.Messages.Count,
                "All messages should be logged to sink");
        }

        [UnityTest]
        public IEnumerator SubscribersReceiveOnMainThread()
        {
            // Arrange: Subscribe to log events and capture thread IDs
            _router.Subscribe(msg =>
            {
                _subscriberMessages.Add(msg);
                _subscriberThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
            });

            // Act: Log from background threads
            const int threadCount = 5;
            const int messagesPerThread = 20;
            var threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                var thread = new Thread(() =>
                {
                    for (int j = 0; j < messagesPerThread; j++)
                    {
                        _logger.Info($"BG Thread {threadIndex} - Msg {j}");
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Wait for main thread dispatcher to process queued events
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            // Assert: All subscriber callbacks executed on main thread
            Assert.Greater(_subscriberMessages.Count, 0, "Subscriber should receive messages");
            foreach (var threadId in _subscriberThreadIds)
            {
                Assert.AreEqual(_mainThreadId, threadId,
                    $"Subscriber callback should execute on main thread (expected {_mainThreadId}, got {threadId})");
            }

            Assert.AreEqual(threadCount * messagesPerThread, _subscriberMessages.Count,
                "All messages should reach subscribers");
        }

        [UnityTest]
        public IEnumerator BoundedQueue_DropsMessagesWhenFull()
        {
            // Arrange: Subscribe and flood with messages to exceed queue limit
            int subscriberCallCount = 0;
            _router.Subscribe(msg =>
            {
                // Simulate slow subscriber processing
                Thread.Sleep(10);
                subscriberCallCount++;
            });

            // Act: Flood with messages from background thread
            const int floodMessageCount = 2000; // Exceeds MainThreadDispatcher.MaxQueueSize (1000)
            var floodThread = new Thread(() =>
            {
                for (int i = 0; i < floodMessageCount; i++)
                {
                    _logger.Info($"Flood message {i}");
                }
            });

            floodThread.Start();
            floodThread.Join();

            // Wait for dispatcher to process what it can
            for (int i = 0; i < 20; i++)
            {
                yield return null;
            }

            // Assert: Some messages were dropped (subscriber received fewer than sent)
            Assert.Less(subscriberCallCount, floodMessageCount,
                "Bounded queue should drop messages when overwhelmed");

            // But sink should still receive all messages (sinks are synchronous)
            Assert.AreEqual(floodMessageCount, _sink.Messages.Count,
                "Sinks should receive all messages synchronously");
        }

        [UnityTest]
        public IEnumerator ConcurrentSubscribeAndUnsubscribe_NoDataRaces()
        {
            // Arrange: Perform concurrent subscribe/unsubscribe operations
            const int iterationCount = 50;
            var subscriptions = new List<IDisposable>();
            var exceptions = new List<Exception>();
            var subscribeLock = new object();

            // Act: Concurrent subscribe/unsubscribe from background thread
            var subThread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < iterationCount; i++)
                    {
                        var sub = _router.Subscribe(msg => { /* no-op */ });
                        lock (subscribeLock)
                        {
                            subscriptions.Add(sub);
                        }
                        Thread.Sleep(1); // Brief delay
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });

            var unsubThread = new Thread(() =>
            {
                try
                {
                    for (int i = 0; i < iterationCount; i++)
                    {
                        Thread.Sleep(1);
                        lock (subscribeLock)
                        {
                            if (subscriptions.Count > 0)
                            {
                                var sub = subscriptions[0];
                                subscriptions.RemoveAt(0);
                                sub.Dispose();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            });

            subThread.Start();
            unsubThread.Start();

            // Also log from main thread while sub/unsub happening
            for (int i = 0; i < iterationCount; i++)
            {
                _logger.Info($"Main thread log {i}");
                yield return null;
            }

            subThread.Join();
            unsubThread.Join();

            // Assert: No exceptions from concurrent operations
            Assert.AreEqual(0, exceptions.Count,
                $"Concurrent subscribe/unsubscribe should not cause exceptions: {string.Join(", ", exceptions)}");
        }

        /// <summary>
        /// Simple test sink that captures messages in a thread-safe list.
        /// </summary>
        private class TestLogSink : ILogSink
        {
            private readonly List<LogMessage> _messages = new List<LogMessage>();
            private readonly object _lock = new object();

            public string Name { get; }

            public TestLogSink(string name)
            {
                Name = name;
            }

            public List<LogMessage> Messages
            {
                get
                {
                    lock (_lock)
                    {
                        return new List<LogMessage>(_messages);
                    }
                }
            }

            public void Write(LogMessage message)
            {
                lock (_lock)
                {
                    _messages.Add(message);
                }
            }

            public void Flush()
            {
                // No-op for test sink
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    _messages.Clear();
                }
            }
        }
    }
}
