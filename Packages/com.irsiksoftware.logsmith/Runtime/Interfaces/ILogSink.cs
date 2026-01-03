namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Output target for log messages (e.g., Console, File).
    /// </summary>
    public interface ILogSink
    {
        /// <summary>
        /// Writes a log message to this sink.
        /// </summary>
        void Write(LogMessage message);

        /// <summary>
        /// Flushes any buffered messages to the underlying output.
        /// </summary>
        void Flush();

        /// <summary>
        /// Gets the name of this sink.
        /// </summary>
        string Name { get; }
    }
}
