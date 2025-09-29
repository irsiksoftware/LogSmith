using System.Collections.Generic;

namespace IrsikSoftware.LogSmith
{
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
        /// Gets all registered categories.
        /// </summary>
        IReadOnlyList<string> GetCategories();

        /// <summary>
        /// Checks if a category is registered.
        /// </summary>
        bool HasCategory(string category);
    }
}