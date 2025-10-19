namespace IrsikSoftware.LogSmith.Adapters
{
    /// <summary>
    /// Adapter for Unity's Debug logging system (UnityEngine.Debug).
    /// All backend-specific calls should be isolated within this class.
    /// </summary>
    internal static class NativeUnityLoggerAdapter
    {
        /// <summary>
        /// Writes a log message at the specified level using Unity's Debug logging.
        /// </summary>
        public static void Write(LogLevel level, string category, string message)
        {
            var formattedMessage = $"[{category}] {message}";

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError($"[CRITICAL] {formattedMessage}");
                    break;
            }
        }
    }
}