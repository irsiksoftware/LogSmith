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
        [Tooltip("The name of the category (e.g., 'Gameplay', 'Network', 'UI')")]
        public string name = "Default";

        [Tooltip("Display color for this category in logs and UI")]
        public Color color = Color.white;

        [Tooltip("Minimum log level for this category")]
        public LogLevel minimumLevel = LogLevel.Debug;

        [Tooltip("Whether this category is enabled")]
        public bool enabled = true;

        public CategoryDefinition()
        {
        }

        public CategoryDefinition(string name, Color color, LogLevel minimumLevel = LogLevel.Debug, bool enabled = true)
        {
            this.name = name;
            this.color = color;
            this.minimumLevel = minimumLevel;
            this.enabled = enabled;
        }
    }
}
