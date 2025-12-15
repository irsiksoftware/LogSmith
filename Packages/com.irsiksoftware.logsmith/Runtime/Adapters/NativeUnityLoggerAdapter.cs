using Unity.Logging;

namespace IrsikSoftware.LogSmith.Adapters
{
    /// <summary>
    /// Adapter for Unity's native logging system (com.unity.logging).
    /// All backend-specific calls should be isolated within this class.
    /// </summary>
    internal static class NativeUnityLoggerAdapter
    {
        /// <summary>
        /// Writes a log message at the specified level using Unity.Logging.
        /// </summary>
        public static void Write(LogLevel level, string category, string message)
        {
            var formattedMessage = $"[{category}] {message}";

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Log.Debug(formattedMessage);
                    break;
                case LogLevel.Info:
                    Log.Info(formattedMessage);
                    break;
                case LogLevel.Warn:
                    Log.Warning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Log.Error(formattedMessage);
                    break;
                case LogLevel.Critical:
                    Log.Error($"[CRITICAL] {formattedMessage}");
                    break;
            }
        }
    }
}
