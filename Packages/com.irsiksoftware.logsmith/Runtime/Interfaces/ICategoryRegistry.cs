using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Metadata for a log category.
    /// </summary>
    public class CategoryMetadata
    {
        /// <summary>
        /// The minimum log level for this category.
        /// </summary>
        public LogLevel MinimumLevel { get; set; }

        /// <summary>
        /// The color used to display this category in the UI.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Whether this category is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        public CategoryMetadata()
        {
            MinimumLevel = LogLevel.Info;
            Color = Color.white;
            Enabled = true;
        }

        public CategoryMetadata(LogLevel minimumLevel, Color color, bool enabled)
        {
            MinimumLevel = minimumLevel;
            Color = color;
            Enabled = enabled;
        }
    }

    /// <summary>
    /// Manages runtime log categories and their metadata.
    /// </summary>
    public interface ICategoryRegistry
    {
        /// <summary>
        /// Registers a new category with the specified minimum log level.
        /// </summary>
        void RegisterCategory(string category, LogLevel minimumLevel);

        /// <summary>
        /// Registers a new category with full metadata.
        /// </summary>
        void RegisterCategory(string category, CategoryMetadata metadata);

        /// <summary>
        /// Unregisters a category.
        /// </summary>
        void UnregisterCategory(string category);

        /// <summary>
        /// Renames an existing category.
        /// </summary>
        void RenameCategory(string oldName, string newName);

        /// <summary>
        /// Sets the minimum log level for a category.
        /// </summary>
        void SetMinimumLevel(string category, LogLevel level);

        /// <summary>
        /// Gets the minimum log level for a category.
        /// </summary>
        LogLevel GetMinimumLevel(string category);

        /// <summary>
        /// Sets the color for a category.
        /// </summary>
        void SetColor(string category, Color color);

        /// <summary>
        /// Gets the color for a category.
        /// </summary>
        Color GetColor(string category);

        /// <summary>
        /// Sets whether a category is enabled.
        /// </summary>
        void SetEnabled(string category, bool enabled);

        /// <summary>
        /// Gets whether a category is enabled.
        /// </summary>
        bool IsEnabled(string category);

        /// <summary>
        /// Gets the full metadata for a category.
        /// </summary>
        CategoryMetadata GetMetadata(string category);

        /// <summary>
        /// Gets all registered categories.
        /// </summary>
        IReadOnlyList<string> GetCategories();

        /// <summary>
        /// Checks if a category is registered.
        /// </summary>
        bool HasCategory(string category);
    }
}