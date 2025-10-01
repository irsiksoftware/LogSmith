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
    /// Represents a structured log message.
    /// </summary>
    public struct LogMessage
    {
        public LogLevel Level;
        public string Category;
        public string Message;
        public DateTime Timestamp;
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