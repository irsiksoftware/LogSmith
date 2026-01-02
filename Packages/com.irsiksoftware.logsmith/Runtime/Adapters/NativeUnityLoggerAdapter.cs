using UnityEngine;

namespace IrsikSoftware.LogSmith.Adapters
{
    /// <summary>
    /// Adapter for Unity's native logging system.
    /// All backend-specific calls should be isolated within this class.
    /// </summary>
    internal static class NativeUnityLoggerAdapter
    {
        /// <summary>
        /// Writes a log message at the specified level using UnityEngine.Debug.
        /// </summary>
        [HideInCallstack]
        public static void Write(LogLevel level, string category, string message)
        {
            var formattedMessage = $"[{category}] {message}";

            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Info:
                    Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warn:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
                case LogLevel.Critical:
                    Debug.LogError($"[CRITICAL] {formattedMessage}");
                    break;
            }
        }
    }
}
