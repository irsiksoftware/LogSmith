using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Unit tests for LogRouter focusing on sink management, ordering, and subscriber patterns.
    /// Complements LogRouterCategoryIntegrationTests which focuses on category filtering integration.
    /// </summary>
    [TestFixture]
    public class LogRouterTests
    {
        private LogRouter _router;
        private TestLogSink _sink1;
        private TestLogSink _sink2;
        private TestLogSink _sink3;

        [SetUp]
        public void Setup()
        {
            _router = new LogRouter();
            _sink1 = new TestLogSink("Sink1");
            _sink2 = new TestLogSink("Sink2");
            _sink3 = new TestLogSink("Sink3");
        }

        #region Sink Registration Tests

        [Test]
        public void RegisterSink_AddsSinkToRouter()
        {
            // Act
            _router.RegisterSink(_sink1);
            var message = CreateTestMessage(LogLevel.Info, "Test");

            _router.Route(message);

            // Assert
            Assert.AreEqual(1, _sink1.Messages.Count);
        }

        [Test]
        public void RegisterSink_ThrowsOnNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _router.RegisterSink(null));
        }

        [Test]
        public void RegisterSink_SameSinkTwice_OnlyAddsOnce()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.RegisterSink(_sink1);

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act
            _router.Route(message);

            // Assert - Should only receive one message (not duplicated)
            Assert.AreEqual(1, _sink1.Messages.Count);
        }

        [Test]
        public void UnregisterSink_RemovesSink()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.UnregisterSink(_sink1);

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(0, _sink1.Messages.Count);
        }

        [Test]
        public void UnregisterSink_ThrowsOnNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _router.UnregisterSink(null));
        }

        [Test]
        public void UnregisterSink_NonExistentSink_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _router.UnregisterSink(_sink1));
        }

        #endregion

        #region Multiple Sink Routing Tests

        [Test]
        public void Route_MultipleSinks_RoutesToAll()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.RegisterSink(_sink2);
            _router.RegisterSink(_sink3);

            var message = CreateTestMessage(LogLevel.Info, "Test message");

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(1, _sink1.Messages.Count);
            Assert.AreEqual(1, _sink2.Messages.Count);
            Assert.AreEqual(1, _sink3.Messages.Count);
            Assert.AreEqual("Test message", _sink1.Messages[0].Message);
            Assert.AreEqual("Test message", _sink2.Messages[0].Message);
            Assert.AreEqual("Test message", _sink3.Messages[0].Message);
        }

        [Test]
        public void Route_MultipleSinks_PreservesRegistrationOrder()
        {
            // Arrange
            var orderTracker = new List<string>();
            var orderedSink1 = new OrderTrackingSink("First", orderTracker);
            var orderedSink2 = new OrderTrackingSink("Second", orderTracker);
            var orderedSink3 = new OrderTrackingSink("Third", orderTracker);

            _router.RegisterSink(orderedSink1);
            _router.RegisterSink(orderedSink2);
            _router.RegisterSink(orderedSink3);

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act
            _router.Route(message);

            // Assert
            Assert.AreEqual(3, orderTracker.Count);
            Assert.AreEqual("First", orderTracker[0]);
            Assert.AreEqual("Second", orderTracker[1]);
            Assert.AreEqual("Third", orderTracker[2]);
        }

        [Test]
        public void Route_OneSinkFails_OtherSinksStillReceiveMessages()
        {
            // Arrange
            var failingSink = new FailingLogSink("FailingSink");
            _router.RegisterSink(_sink1);
            _router.RegisterSink(failingSink);
            _router.RegisterSink(_sink2);

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Expect the error log from the failing sink
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, "[LogSmith] Sink 'FailingSink' failed: FailingSink intentionally failed");

            // Act - Should not throw despite failing sink
            Assert.DoesNotThrow(() => _router.Route(message));

            // Assert - Other sinks should still receive the message
            Assert.AreEqual(1, _sink1.Messages.Count);
            Assert.AreEqual(1, _sink2.Messages.Count);
        }

        #endregion

        #region Global Minimum Level Tests

        [Test]
        public void SetGlobalMinimumLevel_FiltersLowerLevels()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.SetGlobalMinimumLevel(LogLevel.Warn);

            var debugMessage = CreateTestMessage(LogLevel.Debug, "Debug");
            var infoMessage = CreateTestMessage(LogLevel.Info, "Info");
            var warnMessage = CreateTestMessage(LogLevel.Warn, "Warn");
            var errorMessage = CreateTestMessage(LogLevel.Error, "Error");

            // Act
            _router.Route(debugMessage);
            _router.Route(infoMessage);
            _router.Route(warnMessage);
            _router.Route(errorMessage);

            // Assert - Only Warn and Error should pass
            Assert.AreEqual(2, _sink1.Messages.Count);
            Assert.AreEqual("Warn", _sink1.Messages[0].Message);
            Assert.AreEqual("Error", _sink1.Messages[1].Message);
        }

        [Test]
        public void SetGlobalMinimumLevel_DefaultIsTrace_AllowsAllLevels()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            // Default is Trace (no explicit call to SetGlobalMinimumLevel)

            var traceMessage = CreateTestMessage(LogLevel.Trace, "Trace");
            var debugMessage = CreateTestMessage(LogLevel.Debug, "Debug");
            var criticalMessage = CreateTestMessage(LogLevel.Critical, "Critical");

            // Act
            _router.Route(traceMessage);
            _router.Route(debugMessage);
            _router.Route(criticalMessage);

            // Assert - All should pass
            Assert.AreEqual(3, _sink1.Messages.Count);
        }

        #endregion

        #region Category Filter Tests

        [Test]
        public void SetCategoryFilter_FiltersSpecificCategory()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.SetCategoryFilter("TestCategory", LogLevel.Error);

            var debugMessage = CreateTestMessage(LogLevel.Debug, "Debug", "TestCategory");
            var errorMessage = CreateTestMessage(LogLevel.Error, "Error", "TestCategory");

            // Act
            _router.Route(debugMessage);
            _router.Route(errorMessage);

            // Assert
            Assert.AreEqual(1, _sink1.Messages.Count);
            Assert.AreEqual("Error", _sink1.Messages[0].Message);
        }

        [Test]
        public void SetCategoryFilter_ThrowsOnNullCategory()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _router.SetCategoryFilter(null, LogLevel.Info));
            Assert.Throws<ArgumentNullException>(() => _router.SetCategoryFilter(string.Empty, LogLevel.Info));
        }

        [Test]
        public void SetCategoryFilter_DifferentCategories_IndependentFiltering()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.SetCategoryFilter("Cat1", LogLevel.Error);
            _router.SetCategoryFilter("Cat2", LogLevel.Debug);

            var cat1Debug = CreateTestMessage(LogLevel.Debug, "Cat1 Debug", "Cat1");
            var cat1Error = CreateTestMessage(LogLevel.Error, "Cat1 Error", "Cat1");
            var cat2Debug = CreateTestMessage(LogLevel.Debug, "Cat2 Debug", "Cat2");
            var cat2Info = CreateTestMessage(LogLevel.Info, "Cat2 Info", "Cat2");

            // Act
            _router.Route(cat1Debug);
            _router.Route(cat1Error);
            _router.Route(cat2Debug);
            _router.Route(cat2Info);

            // Assert - Cat1 Error, Cat2 Debug, Cat2 Info should pass
            Assert.AreEqual(3, _sink1.Messages.Count);
            Assert.AreEqual("Cat1 Error", _sink1.Messages[0].Message);
            Assert.AreEqual("Cat2 Debug", _sink1.Messages[1].Message);
            Assert.AreEqual("Cat2 Info", _sink1.Messages[2].Message);
        }

        [Test]
        public void ClearCategoryFilter_RemovesFilter()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            _router.SetGlobalMinimumLevel(LogLevel.Debug);
            _router.SetCategoryFilter("TestCategory", LogLevel.Error);
            _router.ClearCategoryFilter("TestCategory");

            var debugMessage = CreateTestMessage(LogLevel.Debug, "Debug", "TestCategory");

            // Act
            _router.Route(debugMessage);

            // Assert - Should pass with global minimum level
            Assert.AreEqual(1, _sink1.Messages.Count);
        }

        [Test]
        public void ClearCategoryFilter_NullOrEmpty_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _router.ClearCategoryFilter(null));
            Assert.DoesNotThrow(() => _router.ClearCategoryFilter(string.Empty));
        }

        #endregion

        #region Subscriber Pattern Tests

        [Test]
        public void Subscribe_ReceivesMessages()
        {
            // Arrange
            var receivedMessages = new List<LogMessage>();
            var subscription = _router.Subscribe(msg => receivedMessages.Add(msg));

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act
            _router.Route(message);
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications

            // Assert
            Assert.AreEqual(1, receivedMessages.Count);
            Assert.AreEqual("Test", receivedMessages[0].Message);

            // Cleanup
            subscription.Dispose();
        }

        [Test]
        public void Subscribe_ThrowsOnNullHandler()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _router.Subscribe(null));
        }

        [Test]
        public void Subscribe_MultipleSubscribers_AllReceiveMessages()
        {
            // Arrange
            var messages1 = new List<LogMessage>();
            var messages2 = new List<LogMessage>();
            var messages3 = new List<LogMessage>();

            var sub1 = _router.Subscribe(msg => messages1.Add(msg));
            var sub2 = _router.Subscribe(msg => messages2.Add(msg));
            var sub3 = _router.Subscribe(msg => messages3.Add(msg));

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act
            _router.Route(message);
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications

            // Assert
            Assert.AreEqual(1, messages1.Count);
            Assert.AreEqual(1, messages2.Count);
            Assert.AreEqual(1, messages3.Count);

            // Cleanup
            sub1.Dispose();
            sub2.Dispose();
            sub3.Dispose();
        }

        [Test]
        public void Subscribe_Dispose_StopsReceivingMessages()
        {
            // Arrange
            var receivedMessages = new List<LogMessage>();
            var subscription = _router.Subscribe(msg => receivedMessages.Add(msg));

            var message1 = CreateTestMessage(LogLevel.Info, "Before dispose");

            // Act
            _router.Route(message1);
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications
            subscription.Dispose();

            var message2 = CreateTestMessage(LogLevel.Info, "After dispose");
            _router.Route(message2);
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications

            // Assert - Only first message should be received
            Assert.AreEqual(1, receivedMessages.Count);
            Assert.AreEqual("Before dispose", receivedMessages[0].Message);
        }

        [Test]
        public void Subscribe_DisposeMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var receivedMessages = new List<LogMessage>();
            var subscription = _router.Subscribe(msg => receivedMessages.Add(msg));

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                subscription.Dispose();
                subscription.Dispose();
                subscription.Dispose();
            });
        }

        [Test]
        public void Subscribe_FailingSubscriber_DoesNotBlockOthers()
        {
            // Arrange
            var messages1 = new List<LogMessage>();
            var messages2 = new List<LogMessage>();

            var sub1 = _router.Subscribe(msg => messages1.Add(msg));
            var failingSub = _router.Subscribe(msg => throw new Exception("Subscriber failure"));
            var sub2 = _router.Subscribe(msg => messages2.Add(msg));

            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Expect the error log from the failing subscriber
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, "[LogSmith] Subscriber failed: Subscriber failure");

            // Act - Should not throw
            Assert.DoesNotThrow(() => _router.Route(message));
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications

            // Assert - Other subscribers should still receive messages
            Assert.AreEqual(1, messages1.Count);
            Assert.AreEqual(1, messages2.Count);

            // Cleanup
            sub1.Dispose();
            failingSub.Dispose();
            sub2.Dispose();
        }

        [Test]
        public void Subscribe_FilteredMessage_SubscriberDoesNotReceive()
        {
            // Arrange
            var receivedMessages = new List<LogMessage>();
            var subscription = _router.Subscribe(msg => receivedMessages.Add(msg));
            _router.SetGlobalMinimumLevel(LogLevel.Warn);

            var debugMessage = CreateTestMessage(LogLevel.Debug, "Debug");
            var warnMessage = CreateTestMessage(LogLevel.Warn, "Warn");

            // Act
            _router.Route(debugMessage);
            _router.Route(warnMessage);
            MainThreadDispatcher.Instance.ProcessQueue(); // Process queued subscriber notifications

            // Assert - Only warn message should be received
            Assert.AreEqual(1, receivedMessages.Count);
            Assert.AreEqual("Warn", receivedMessages[0].Message);

            // Cleanup
            subscription.Dispose();
        }

        #endregion

        #region Edge Cases & Thread Safety

        [Test]
        public void Route_NoSinksRegistered_DoesNotThrow()
        {
            // Arrange
            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _router.Route(message));
        }

        [Test]
        public void Route_NoSubscribers_DoesNotThrow()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            var message = CreateTestMessage(LogLevel.Info, "Test");

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => _router.Route(message));

            // Assert - Sink should still receive message
            Assert.AreEqual(1, _sink1.Messages.Count);
        }

        [Test]
        public void ThreadSafety_ConcurrentRouting_AllMessagesProcessed()
        {
            // Arrange
            _router.RegisterSink(_sink1);
            const int threadCount = 10;
            const int messagesPerThread = 100;
            var threads = new System.Threading.Thread[threadCount];

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    for (int j = 0; j < messagesPerThread; j++)
                    {
                        var message = CreateTestMessage(LogLevel.Info, $"Thread{threadIndex}_Msg{j}");
                        _router.Route(message);
                    }
                });
                threads[i].Start();
            }

            // Wait for all threads
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            // Assert
            Assert.AreEqual(threadCount * messagesPerThread, _sink1.Messages.Count);
        }

        [Test]
        public void ThreadSafety_ConcurrentSinkRegistration_DoesNotThrow()
        {
            // Arrange
            const int threadCount = 10;
            var threads = new System.Threading.Thread[threadCount];
            var sinks = new List<TestLogSink>();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                var sink = new TestLogSink($"Sink{threadIndex}");
                sinks.Add(sink);

                threads[i] = new System.Threading.Thread(() =>
                {
                    _router.RegisterSink(sink);
                    _router.UnregisterSink(sink);
                    _router.RegisterSink(sink);
                });
                threads[i].Start();
            }

            // Wait for all threads
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            // Assert - Should complete without exceptions
            Assert.Pass();
        }

        #endregion

        #region Helper Methods & Test Sinks

        private LogMessage CreateTestMessage(LogLevel level, string message, string category = "Default")
        {
            return new LogMessage
            {
                Level = level,
                Category = category,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Frame = 0,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId
            };
        }

        private class TestLogSink : ILogSink
        {
            public string Name { get; }
            public List<LogMessage> Messages { get; } = new List<LogMessage>();

            public TestLogSink(string name = "TestSink")
            {
                Name = name;
            }

            public void Write(LogMessage message)
            {
                Messages.Add(message);
            }

            public void Flush()
            {
                // No-op for test
            }
        }

        private class OrderTrackingSink : ILogSink
        {
            public string Name { get; }
            private readonly List<string> _orderTracker;

            public OrderTrackingSink(string name, List<string> orderTracker)
            {
                Name = name;
                _orderTracker = orderTracker;
            }

            public void Write(LogMessage message)
            {
                _orderTracker.Add(Name);
            }

            public void Flush()
            {
                // No-op
            }
        }

        private class FailingLogSink : ILogSink
        {
            public string Name { get; }

            public FailingLogSink(string name)
            {
                Name = name;
            }

            public void Write(LogMessage message)
            {
                throw new Exception($"{Name} intentionally failed");
            }

            public void Flush()
            {
                // No-op
            }
        }

        #endregion
    }
}
