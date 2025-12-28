using NUnit.Framework;
using IrsikSoftware.LogSmith.Core;
using System;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Unit tests for CircularBuffer<T> to verify core functionality and edge cases.
    /// </summary>
    public class CircularBufferTests
    {
        [Test]
        public void Constructor_WithValidCapacity_CreatesBuffer()
        {
            // Arrange & Act
            var buffer = new CircularBuffer<int>(10);

            // Assert
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Constructor_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(0));
        }

        [Test]
        public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(-1));
        }

        [Test]
        public void Add_SingleItem_IncreasesCount()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);

            // Act
            buffer.Add(42);

            // Assert
            Assert.AreEqual(1, buffer.Count);
        }

        [Test]
        public void Add_MultipleItems_IncreasesCount()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);

            // Act
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Assert
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
        public void Add_ItemsUpToCapacity_CountEqualsCapacity()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);

            // Act
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Assert
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
        public void Add_ItemsBeyondCapacity_CountRemainsAtCapacity()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);

            // Act
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);
            buffer.Add(5);

            // Assert
            Assert.AreEqual(3, buffer.Count);
        }

        [Test]
        public void GetAll_EmptyBuffer_ReturnsEmptyList()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);

            // Act
            var result = buffer.GetAll();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAll_WithItems_ReturnsItemsInOrder()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act
            var result = buffer.GetAll();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3, result[2]);
        }

        [Test]
        public void GetAll_AfterWrapping_ReturnsItemsInCorrectOrder()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4); // Overwrites 1
            buffer.Add(5); // Overwrites 2

            // Act
            var result = buffer.GetAll();

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(5, result[2]);
        }

        [Test]
        public void GetAll_MultipleWraps_ReturnsCorrectItems()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(2);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3); // Overwrites 1
            buffer.Add(4); // Overwrites 2
            buffer.Add(5); // Overwrites 3
            buffer.Add(6); // Overwrites 4

            // Act
            var result = buffer.GetAll();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(5, result[0]);
            Assert.AreEqual(6, result[1]);
        }

        [Test]
        public void Clear_EmptyBuffer_RemainsEmpty()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);

            // Act
            buffer.Clear();

            // Assert
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Clear_WithItems_ResetsCount()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(5);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act
            buffer.Clear();

            // Assert
            Assert.AreEqual(0, buffer.Count);
        }

        [Test]
        public void Clear_AfterWrapping_ResetsBuffer()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Add(4);
            buffer.Add(5);

            // Act
            buffer.Clear();

            // Assert
            Assert.AreEqual(0, buffer.Count);
            var result = buffer.GetAll();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Add_AfterClear_WorksCorrectly()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);
            buffer.Clear();

            // Act
            buffer.Add(10);
            buffer.Add(20);

            // Assert
            Assert.AreEqual(2, buffer.Count);
            var result = buffer.GetAll();
            Assert.AreEqual(10, result[0]);
            Assert.AreEqual(20, result[1]);
        }

        [Test]
        public void CircularBuffer_WithReferenceType_HandlesNullValues()
        {
            // Arrange
            var buffer = new CircularBuffer<string>(3);

            // Act
            buffer.Add("first");
            buffer.Add(null);
            buffer.Add("third");

            // Assert
            var result = buffer.GetAll();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("first", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("third", result[2]);
        }

        [Test]
        public void CircularBuffer_CapacityOfOne_OverwritesOnEachAdd()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(1);

            // Act
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Assert
            Assert.AreEqual(1, buffer.Count);
            var result = buffer.GetAll();
            Assert.AreEqual(3, result[0]);
        }

        [Test]
        public void GetAll_ReturnsNewListEachTime()
        {
            // Arrange
            var buffer = new CircularBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);

            // Act
            var result1 = buffer.GetAll();
            var result2 = buffer.GetAll();

            // Assert
            Assert.AreNotSame(result1, result2);
            CollectionAssert.AreEqual(result1, result2);
        }
    }
}
