using UnityEditor;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Interface for LogSmith Editor Window tabs.
    /// Defines the contract that all editor tabs must implement.
    /// </summary>
    public interface IEditorTab
    {
        /// <summary>
        /// Display name shown in tab selector.
        /// </summary>
        string TabName { get; }

        /// <summary>
        /// Draw the tab's UI content.
        /// </summary>
        /// <param name="serializedSettings">Serialized settings object for property binding.</param>
        /// <param name="settings">LoggingSettings instance for direct access.</param>
        void Draw(SerializedObject serializedSettings, LoggingSettings settings);

        /// <summary>
        /// Called when tab becomes active (optional lifecycle).
        /// Default implementation does nothing.
        /// </summary>
        void OnTabEnabled() { }

        /// <summary>
        /// Called when tab becomes inactive (optional lifecycle).
        /// Default implementation does nothing.
        /// </summary>
        void OnTabDisabled() { }
    }
}
