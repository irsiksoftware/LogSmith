using System;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Routes log messages to registered sinks with filtering capabilities.
    /// </summary>
    public interface ILogRouter
    {
        /// <summary>
        /// Registers a sink to receive log messages.
        /// </summary>
        void RegisterSink(ILogSink sink);

        /// <summary>
        /// Unregisters a sink from receiving log messages.
        /// </summary>
        void UnregisterSink(ILogSink sink);

        /// <summary>
        /// Routes a log message to all registered sinks.
        /// </summary>
        void Route(LogMessage message);

        /// <summary>
        /// Subscribes to log message events for custom processing.
        /// </summary>
        IDisposable Subscribe(Action<LogMessage> handler);

        /// <summary>
        /// Sets the global minimum log level for all categories.
        /// </summary>
        void SetGlobalMinimumLevel(LogLevel level);

        /// <summary>
        /// Sets a minimum log level filter for a specific category.
        /// </summary>
        void SetCategoryFilter(string category, LogLevel minimumLevel);

        /// <summary>
        /// Removes the filter for a specific category.
        /// </summary>
        void ClearCategoryFilter(string category);
    }

    /// <summary>
    /// Represents a structured log message with rich context information.
    /// </summary>
    public struct LogMessage
    {
        /// <summary>
        /// Log severity level.
        /// </summary>
        public LogLevel Level;

        /// <summary>
        /// Category name for this log message.
        /// </summary>
        public string Category;

        /// <summary>
        /// The log message text.
        /// </summary>
        public string Message;

        /// <summary>
        /// Timestamp when the log was created.
        /// </summary>
        public DateTime Timestamp;

        /// <summary>
        /// Unity frame number when the log was created (optional).
        /// </summary>
        public int Frame;

        /// <summary>
        /// Thread ID where the log originated (optional).
        /// </summary>
        public int ThreadId;

        /// <summary>
        /// Thread name if available (optional).
        /// </summary>
        public string ThreadName;

        /// <summary>
        /// Stack trace at the time of logging (optional, expensive to capture).
        /// </summary>
        public string StackTrace;

        /// <summary>
        /// Source file path where the log was called from (optional).
        /// </summary>
        public string CallerFilePath;

        /// <summary>
        /// Method/member name where the log was called from (optional).
        /// </summary>
        public string CallerMemberName;

        /// <summary>
        /// Line number where the log was called from (optional).
        /// </summary>
        public int CallerLineNumber;

        /// <summary>
        /// Additional context key-value pairs (optional).
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> Context;
    }

    /// <summary>
    /// Log severity levels.
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Critical = 5
    }
}