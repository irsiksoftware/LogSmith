using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Tests
{
    [TestFixture]
    public class CategoryRegistryTests
    {
        private CategoryRegistry _registry;

        [SetUp]
        public void Setup()
        {
            _registry = new CategoryRegistry();
        }

        [Test]
        public void RegisterCategory_SimpleOverload_RegistersWithDefaultSettings()
        {
            // Arrange
            const string category = "TestCategory";
            const LogLevel minLevel = LogLevel.Debug;

            // Act
            _registry.RegisterCategory(category, minLevel);

            // Assert
            Assert.IsTrue(_registry.HasCategory(category));
            Assert.AreEqual(minLevel, _registry.GetMinimumLevel(category));
            Assert.IsTrue(_registry.IsEnabled(category));
        }

        [Test]
        public void RegisterCategory_WithMetadata_RegistersAllProperties()
        {
            // Arrange
            const string category = "TestCategory";
            var metadata = new CategoryMetadata
            {
                Name = category,
                MinimumLevel = LogLevel.Warn,
                Enabled = false,
                Color = Color.red
            };

            // Act
            _registry.RegisterCategory(category, metadata);

            // Assert
            Assert.IsTrue(_registry.HasCategory(category));
            Assert.AreEqual(LogLevel.Warn, _registry.GetMinimumLevel(category));
            Assert.IsFalse(_registry.IsEnabled(category));
            Assert.AreEqual(Color.red, _registry.GetColor(category));
        }

        [Test]
        public void UnregisterCategory_RemovesCategory()
        {
            // Arrange
            const string category = "TestCategory";
            _registry.RegisterCategory(category, LogLevel.Info);

            // Act
            _registry.UnregisterCategory(category);

            // Assert
            Assert.IsFalse(_registry.HasCategory(category));
        }

        [Test]
        public void RenameCategory_PreservesMetadata()
        {
            // Arrange
            const string oldName = "OldCategory";
            const string newName = "NewCategory";
            var metadata = new CategoryMetadata
            {
                Name = oldName,
                MinimumLevel = LogLevel.Error,
                Enabled = false,
                Color = Color.blue
            };
            _registry.RegisterCategory(oldName, metadata);

            // Act
            _registry.RenameCategory(oldName, newName);

            // Assert
            Assert.IsFalse(_registry.HasCategory(oldName));
            Assert.IsTrue(_registry.HasCategory(newName));
            Assert.AreEqual(LogLevel.Error, _registry.GetMinimumLevel(newName));
            Assert.IsFalse(_registry.IsEnabled(newName));
            Assert.AreEqual(Color.blue, _registry.GetColor(newName));
        }

        [Test]
        public void SetMinimumLevel_UpdatesExistingCategory()
        {
            // Arrange
            const string category = "TestCategory";
            _registry.RegisterCategory(category, LogLevel.Info);

            // Act
            _registry.SetMinimumLevel(category, LogLevel.Critical);

            // Assert
            Assert.AreEqual(LogLevel.Critical, _registry.GetMinimumLevel(category));
        }

        [Test]
        public void SetMinimumLevel_AutoRegistersNewCategory()
        {
            // Arrange
            const string category = "NewCategory";

            // Act
            _registry.SetMinimumLevel(category, LogLevel.Warn);

            // Assert
            Assert.IsTrue(_registry.HasCategory(category));
            Assert.AreEqual(LogLevel.Warn, _registry.GetMinimumLevel(category));
        }

        [Test]
        public void GetMinimumLevel_ReturnsDefaultForUnregisteredCategory()
        {
            // Act
            var level = _registry.GetMinimumLevel("UnregisteredCategory");

            // Assert
            Assert.AreEqual(LogLevel.Info, level); // Default is Info
        }

        [Test]
        public void SetEnabled_UpdatesEnabledState()
        {
            // Arrange
            const string category = "TestCategory";
            _registry.RegisterCategory(category, LogLevel.Info);

            // Act
            _registry.SetEnabled(category, false);

            // Assert
            Assert.IsFalse(_registry.IsEnabled(category));

            // Act again
            _registry.SetEnabled(category, true);

            // Assert again
            Assert.IsTrue(_registry.IsEnabled(category));
        }

        [Test]
        public void IsEnabled_ReturnsTrueForUnregisteredCategory()
        {
            // Act
            var enabled = _registry.IsEnabled("UnregisteredCategory");

            // Assert
            Assert.IsTrue(enabled);
        }

        [Test]
        public void SetColor_UpdatesColor()
        {
            // Arrange
            const string category = "TestCategory";
            _registry.RegisterCategory(category, LogLevel.Info);

            // Act
            _registry.SetColor(category, Color.green);

            // Assert
            Assert.AreEqual(Color.green, _registry.GetColor(category));
        }

        [Test]
        public void GetColor_ReturnsDefaultForUnregisteredCategory()
        {
            // Act
            var color = _registry.GetColor("UnregisteredCategory");

            // Assert
            Assert.AreEqual(Color.white, color); // Default is white
        }

        [Test]
        public void GetMetadata_ReturnsCompleteMetadata()
        {
            // Arrange
            const string category = "TestCategory";
            var originalMetadata = new CategoryMetadata
            {
                Name = category,
                MinimumLevel = LogLevel.Debug,
                Enabled = false,
                Color = Color.yellow
            };
            _registry.RegisterCategory(category, originalMetadata);

            // Act
            var retrieved = _registry.GetMetadata(category);

            // Assert
            Assert.AreEqual(category, retrieved.Name);
            Assert.AreEqual(LogLevel.Debug, retrieved.MinimumLevel);
            Assert.IsFalse(retrieved.Enabled);
            Assert.AreEqual(Color.yellow, retrieved.Color);
        }

        [Test]
        public void GetMetadata_ReturnsDefaultForUnregisteredCategory()
        {
            // Act
            var metadata = _registry.GetMetadata("UnregisteredCategory");

            // Assert
            Assert.AreEqual("UnregisteredCategory", metadata.Name);
            Assert.AreEqual(LogLevel.Info, metadata.MinimumLevel);
            Assert.IsTrue(metadata.Enabled);
            Assert.AreEqual(Color.white, metadata.Color);
        }

        [Test]
        public void GetCategories_ReturnsAllRegisteredCategories()
        {
            // Arrange
            _registry.RegisterCategory("Category1", LogLevel.Info);
            _registry.RegisterCategory("Category2", LogLevel.Debug);
            _registry.RegisterCategory("Category3", LogLevel.Warn);

            // Act
            var categories = _registry.GetCategories();

            // Assert
            Assert.AreEqual(3, categories.Count);
            Assert.Contains("Category1", categories as System.Collections.IList);
            Assert.Contains("Category2", categories as System.Collections.IList);
            Assert.Contains("Category3", categories as System.Collections.IList);
        }

        [Test]
        public void HasCategory_ReturnsFalseForNullOrEmpty()
        {
            // Act & Assert
            Assert.IsFalse(_registry.HasCategory(null));
            Assert.IsFalse(_registry.HasCategory(string.Empty));
        }

        [Test]
        public void RegisterCategory_ThrowsForNullOrEmpty()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => _registry.RegisterCategory(null, LogLevel.Info));
            Assert.Throws<System.ArgumentNullException>(() => _registry.RegisterCategory(string.Empty, LogLevel.Info));
        }

        [Test]
        public void ThreadSafety_ConcurrentAccess_DoesNotThrow()
        {
            // Arrange
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var threads = new System.Threading.Thread[threadCount];

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var category = $"Thread{threadIndex}_Category{j}";
                        _registry.RegisterCategory(category, LogLevel.Info);
                        _registry.SetMinimumLevel(category, LogLevel.Debug);
                        _registry.SetEnabled(category, j % 2 == 0);
                        _registry.SetColor(category, Color.red);
                        _registry.HasCategory(category);
                        _registry.GetMetadata(category);
                    }
                });
                threads[i].Start();
            }

            // Wait for all threads
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            // Assert - no exceptions means thread-safe
            var categories = _registry.GetCategories();
            Assert.GreaterOrEqual(categories.Count, 1);
        }
    }
}
