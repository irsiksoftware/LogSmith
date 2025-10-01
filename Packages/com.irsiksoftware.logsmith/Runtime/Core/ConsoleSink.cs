using IrsikSoftware.LogSmith.Adapters;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Console sink implementation using Unity's native logging backend.
    /// </summary>
    public class ConsoleSink : ILogSink
    {
        public string Name => "Console";

        public void Write(LogMessage message)
        {
            NativeUnityLoggerAdapter.Write(message.Level, message.Category, message.Message);
        }

        public void Flush()
        {
            // Unity.Logging handles flushing internally
        }
    }
}