using NUnit.Framework;
using UnityEngine;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Tests
{
    /// <summary>
    /// Unit tests for CategoryRegistry.
    /// </summary>
    [TestFixture]
    public class CategoryRegistryTests
    {
        private CategoryRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _registry = new CategoryRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            _registry = null;
        }

        #region Basic Registration

        [Test]
        public void RegisterCategory_AddsCategory()
        {
            // Act
            _registry.RegisterCategory("TestCategory", LogLevel.Debug);

            // Assert
            Assert.IsTrue(_registry.HasCategory("TestCategory"));
            Assert.AreEqual(LogLevel.Debug, _registry.GetMinimumLevel("TestCategory"));
        }

        [Test]
        public void RegisterCategory_WithMetadata_AddsCategory()
        {
            // Arrange
            var metadata = new CategoryMetadata(LogLevel.Warning, Color.red, true);

            // Act
            _registry.RegisterCategory("TestCategory", metadata);

            // Assert
            Assert.IsTrue(_registry.HasCategory("TestCategory"));
            Assert.AreEqual(LogLevel.Warning, _registry.GetMinimumLevel("TestCategory"));
            Assert.AreEqual(Color.red, _registry.GetColor("TestCategory"));
            Assert.IsTrue(_registry.IsEnabled("TestCategory"));
        }

        [Test]
        public void RegisterCategory_WithNullName_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                _registry.RegisterCategory(null, LogLevel.Info));
        }

        [Test]
        public void RegisterCategory_WithEmptyName_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                _registry.RegisterCategory("", LogLevel.Info));
        }

        [Test]
        public void RegisterCategory_WithNullMetadata_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                _registry.RegisterCategory("TestCategory", null));
        }

        #endregion

        #region Unregistration

        [Test]
        public void UnregisterCategory_RemovesCategory()
        {
            // Arrange
            _registry.RegisterCategory("TestCategory", LogLevel.Debug);
            Assert.IsTrue(_registry.HasCategory("TestCategory"));

            // Act
            _registry.UnregisterCategory("TestCategory");

            // Assert
            Assert.IsFalse(_registry.HasCategory("TestCategory"));
        }

        [Test]
        public void UnregisterCategory_NonExistentCategory_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _registry.UnregisterCategory("NonExistent"));
        }

        #endregion

        #region Renaming

        [Test]
        public void RenameCategory_UpdatesCategoryName()
        {
            // Arrange
            _registry.RegisterCategory("OldName", LogLevel.Debug);
            _registry.SetColor("OldName", Color.blue);
            _registry.SetEnabled("OldName", false);

            // Act
            _registry.RenameCategory("OldName", "NewName");

            // Assert
            Assert.IsFalse(_registry.HasCategory("OldName"));
            Assert.IsTrue(_registry.HasCategory("NewName"));
            Assert.AreEqual(LogLevel.Debug, _registry.GetMinimumLevel("NewName"));
            Assert.AreEqual(Color.blue, _registry.GetColor("NewName"));
            Assert.IsFalse(_registry.IsEnabled("NewName"));
        }

        [Test]
        public void RenameCategory_NonExistentCategory_DoesNothing()
        {
            // Act
            _registry.RenameCategory("NonExistent", "NewName");

            // Assert
            Assert.IsFalse(_registry.HasCategory("NewName"));
        }

        #endregion

        #region Minimum Level

        [Test]
        public void SetMinimumLevel_UpdatesLevel()
        {
            // Arrange
            _registry.RegisterCategory("TestCategory", LogLevel.Debug);

            // Act
            _registry.SetMinimumLevel("TestCategory", LogLevel.Error);

            // Assert
            Assert.AreEqual(LogLevel.Error, _registry.GetMinimumLevel("TestCategory"));
        }

        [Test]
        public void SetMinimumLevel_NonExistentCategory_CreatesCategory()
        {
            // Act
            _registry.SetMinimumLevel("NewCategory", LogLevel.Warning);

            // Assert
            Assert.IsTrue(_registry.HasCategory("NewCategory"));
            Assert.AreEqual(LogLevel.Warning, _registry.GetMinimumLevel("NewCategory"));
        }

        [Test]
        public void GetMinimumLevel_NonExistentCategory_ReturnsDefault()
        {
            // Act
            var level = _registry.GetMinimumLevel("NonExistent");

            // Assert
            Assert.AreEqual(LogLevel.Info, level);
        }

        #endregion

        #region Color

        [Test]
        public void SetColor_UpdatesColor()
        {
            // Arrange
            _registry.RegisterCategory("TestCategory", LogLevel.Debug);

            // Act
            _registry.SetColor("TestCategory", Color.green);

            // Assert
            Assert.AreEqual(Color.green, _registry.GetColor("TestCategory"));
        }

        [Test]
        public void SetColor_NonExistentCategory_CreatesCategory()
        {
            // Act
            _registry.SetColor("NewCategory", Color.yellow);

            // Assert
            Assert.IsTrue(_registry.HasCategory("NewCategory"));
            Assert.AreEqual(Color.yellow, _registry.GetColor("NewCategory"));
        }

        [Test]
        public void GetColor_NonExistentCategory_ReturnsDefault()
        {
            // Act
            var color = _registry.GetColor("NonExistent");

            // Assert
            Assert.AreEqual(Color.white, color);
        }

        #endregion

        #region Enabled State

        [Test]
        public void SetEnabled_UpdatesEnabledState()
        {
            // Arrange
            _registry.RegisterCategory("TestCategory", LogLevel.Debug);
            Assert.IsTrue(_registry.IsEnabled("TestCategory"));

            // Act
            _registry.SetEnabled("TestCategory", false);

            // Assert
            Assert.IsFalse(_registry.IsEnabled("TestCategory"));
        }

        [Test]
        public void SetEnabled_NonExistentCategory_CreatesCategory()
        {
            // Act
            _registry.SetEnabled("NewCategory", false);

            // Assert
            Assert.IsTrue(_registry.HasCategory("NewCategory"));
            Assert.IsFalse(_registry.IsEnabled("NewCategory"));
        }

        [Test]
        public void IsEnabled_NonExistentCategory_ReturnsTrue()
        {
            // Act
            var enabled = _registry.IsEnabled("NonExistent");

            // Assert
            Assert.IsTrue(enabled);
        }

        #endregion

        #region Metadata

        [Test]
        public void GetMetadata_ReturnsFullMetadata()
        {
            // Arrange
            _registry.RegisterCategory("TestCategory", LogLevel.Warning);
            _registry.SetColor("TestCategory", Color.magenta);
            _registry.SetEnabled("TestCategory", false);

            // Act
            var metadata = _registry.GetMetadata("TestCategory");

            // Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual(LogLevel.Warning, metadata.MinimumLevel);
            Assert.AreEqual(Color.magenta, metadata.Color);
            Assert.IsFalse(metadata.Enabled);
        }

        [Test]
        public void GetMetadata_NonExistentCategory_ReturnsNull()
        {
            // Act
            var metadata = _registry.GetMetadata("NonExistent");

            // Assert
            Assert.IsNull(metadata);
        }

        #endregion

        #region Category Listing

        [Test]
        public void GetCategories_ReturnsAllCategories()
        {
            // Arrange
            _registry.RegisterCategory("Category1", LogLevel.Debug);
            _registry.RegisterCategory("Category2", LogLevel.Info);
            _registry.RegisterCategory("Category3", LogLevel.Warning);

            // Act
            var categories = _registry.GetCategories();

            // Assert
            Assert.AreEqual(3, categories.Count);
            Assert.Contains("Category1", (System.Collections.ICollection)categories);
            Assert.Contains("Category2", (System.Collections.ICollection)categories);
            Assert.Contains("Category3", (System.Collections.ICollection)categories);
        }

        [Test]
        public void GetCategories_EmptyRegistry_ReturnsEmptyList()
        {
            // Act
            var categories = _registry.GetCategories();

            // Assert
            Assert.IsNotNull(categories);
            Assert.AreEqual(0, categories.Count);
        }

        #endregion

        #region Thread Safety

        [Test]
        public void MultipleOperations_AreThreadSafe()
        {
            // This is a basic smoke test for thread safety
            // More comprehensive thread safety tests would require additional tooling

            // Arrange & Act
            System.Threading.Tasks.Parallel.For(0, 100, i =>
            {
                var categoryName = $"Category{i % 10}";
                _registry.RegisterCategory(categoryName, LogLevel.Debug);
                _registry.SetColor(categoryName, Color.red);
                _registry.SetEnabled(categoryName, i % 2 == 0);
                _registry.GetMinimumLevel(categoryName);
                _registry.GetColor(categoryName);
                _registry.IsEnabled(categoryName);
            });

            // Assert - Should not crash
            var categories = _registry.GetCategories();
            Assert.LessOrEqual(categories.Count, 10);
        }

        #endregion
    }
}
