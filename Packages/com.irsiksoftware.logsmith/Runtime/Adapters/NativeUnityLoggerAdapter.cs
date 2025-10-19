#if UNITY_6000_0_OR_NEWER
using Unity.Logging;
using Unity.Logging.Sinks;
using UnityLog = Unity.Logging.Log;
using UnityLogLevel = Unity.Logging.LogLevel;
#endif

namespace IrsikSoftware.LogSmith.Adapters
{
    /// <summary>
    /// Adapter wrapping Unity's native logging system (com.unity.logging).
    /// All backend-specific calls should be isolated within this class.
    /// Note: Unity.Logging (com.unity.logging) is only available in Unity 6+.
    /// For earlier Unity versions, this adapter uses UnityEngine.Debug directly.
    /// </summary>
    internal static class NativeUnityLoggerAdapter
    {
        private static bool _initialized;

        /// <summary>
        /// Initializes Unity's logging system with default configuration.
        /// Only configures Unity.Logging on Unity 6+. Earlier versions use UnityEngine.Debug directly.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

#if UNITY_6000_0_OR_NEWER
            // Configure Unity's logging system (Unity 6+ only)
            UnityLog.Logger = new Logger(new LoggerConfig()
                .MinimumLevel.Set(UnityLogLevel.Verbose)
                .WriteTo.UnityDebugLog());
#endif

            _initialized = true;
        }

        /// <summary>
        /// Writes a log message at the specified level using Unity's Debug logging.
        /// Note: We use UnityEngine.Debug instead of Unity.Logging to avoid FixedString size limitations.
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