using System;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Primary logging interface providing level-based logging methods.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Logs a trace-level message (most verbose).
        /// </summary>
        void Trace(string message);

        /// <summary>
        /// Logs a debug-level message.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// Logs an info-level message.
        /// </summary>
        void Info(string message);

        /// <summary>
        /// Logs a warning-level message.
        /// </summary>
        void Warn(string message);

        /// <summary>
        /// Logs an error-level message.
        /// </summary>
        void Error(string message);

        /// <summary>
        /// Logs a critical-level message (highest severity).
        /// </summary>
        void Critical(string message);

        /// <summary>
        /// Creates a new logger scoped to a specific category.
        /// </summary>
        ILog WithCategory(string category);
    }
}
