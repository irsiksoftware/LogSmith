using NUnit.Framework;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Tests for DebugOverlay CircularBuffer implementation.
    /// </summary>
    public class DebugOverlayTests
    {
        [Test]
        public void CircularBuffer_AddsItemsCorrectly()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            var items = buffer.GetAll();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual(2, items[1]);
            Assert.AreEqual(3, items[2]);
        }

        [Test]
        public void CircularBuffer_OverwritesOldestWhenFull()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4); // Should overwrite 1
            buffer.Add(5); // Should overwrite 2

            var items = buffer.GetAll();
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, items[0]);
            Assert.AreEqual(4, items[1]);
            Assert.AreEqual(5, items[2]);
        }

        [Test]
        public void CircularBuffer_ClearsCorrectly()
        {
            var buffer = new CircularBuffer<int>(3);

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            Assert.AreEqual(3, buffer.Count);

            buffer.Clear();

            Assert.AreEqual(0, buffer.Count);
            Assert.AreEqual(0, buffer.GetAll().Count);
        }

        [Test]
        public void CircularBuffer_HandlesEmptyBuffer()
        {
            var buffer = new CircularBuffer<int>(5);

            Assert.AreEqual(0, buffer.Count);
            var items = buffer.GetAll();
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
        }

        [Test]
        public void CircularBuffer_HandlesLogMessages()
        {
            var buffer = new CircularBuffer<LogMessage>(2);

            var msg1 = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "Test",
                Message = "Message 1",
                Timestamp = System.DateTime.Now
            };

            var msg2 = new LogMessage
            {
                Level = LogLevel.Error,
                Category = "Test",
                Message = "Message 2",
                Timestamp = System.DateTime.Now
            };

            buffer.Add(msg1);
            buffer.Add(msg2);

            var items = buffer.GetAll();
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Message 1", items[0].Message);
            Assert.AreEqual("Message 2", items[1].Message);
        }

        [Test]
        public void CircularBuffer_MaintainsOrderUnderLoad()
        {
            var buffer = new CircularBuffer<int>(100);

            // Add 250 items (more than capacity)
            for (int i = 0; i < 250; i++)
            {
                buffer.Add(i);
            }

            var items = buffer.GetAll();

            // Should have exactly 100 items (capacity)
            Assert.AreEqual(100, items.Count);

            // Should be the last 100 items (150-249)
            Assert.AreEqual(150, items[0]);
            Assert.AreEqual(249, items[99]);

            // Verify sequential order
            for (int i = 0; i < 99; i++)
            {
                Assert.AreEqual(items[i] + 1, items[i + 1]);
            }
        }

        [Test]
        public void LoggingSettings_OverlayDefaultsAreCorrect()
        {
            var settings = LoggingSettings.CreateDefault();

            Assert.IsFalse(settings.enableDebugOverlay);
            Assert.AreEqual(500, settings.overlayMaxLogCount);

            UnityEngine.Object.DestroyImmediate(settings);
        }
    }
}
