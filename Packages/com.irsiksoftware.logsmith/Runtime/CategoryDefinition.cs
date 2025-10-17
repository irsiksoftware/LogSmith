using System;
using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Defines a log category with its configuration settings.
    /// </summary>
    [Serializable]
    public class CategoryDefinition
    {
        [Tooltip("Category name")]
        public string categoryName;

        [Tooltip("Display color for this category")]
        public Color color = Color.white;

        [Tooltip("Minimum log level for this category")]
        public LogLevel minimumLevel = LogLevel.Debug;

        [Tooltip("Whether this category is enabled")]
        public bool enabled = true;

        public CategoryDefinition()
        {
        }

        public CategoryDefinition(string categoryName, Color color, LogLevel minimumLevel = LogLevel.Debug, bool enabled = true)
        {
            this.categoryName = categoryName;
            this.color = color;
            this.minimumLevel = minimumLevel;
            this.enabled = enabled;
        }
    }
}
