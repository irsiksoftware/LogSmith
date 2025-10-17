using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Demonstrates basic logging using the static API.
    /// Toggle this GameObject on/off to see basic log messages.
    /// </summary>
    public class BasicLoggingExample : MonoBehaviour
    {
        [Header("Lifecycle Logging")]
        [Tooltip("Log a message when Start() is called")]
        [SerializeField] private bool logOnStart = true;

        [Tooltip("Log a message when OnEnable() is called")]
        [SerializeField] private bool logOnEnable = true;

        [Tooltip("Log a message when OnDisable() is called")]
        [SerializeField] private bool logOnDisable = true;

        [Tooltip("Log a message when OnDestroy() is called")]
        [SerializeField] private bool logOnDestroy = true;

        [Header("Update Logging")]
        [Tooltip("Continuously log messages in Update()")]
        [SerializeField] private bool logOnUpdate = false;

        [Tooltip("Time in seconds between update logs")]
        [SerializeField] [Range(0.1f, 10f)] private float updateInterval = 1f;

        [Tooltip("Include frame count in update messages")]
        [SerializeField] private bool includeFrameCount = true;

        [Header("Log Levels to Demonstrate")]
        [Tooltip("Log a Trace message on Start")]
        [SerializeField] private bool logTrace = false;

        [Tooltip("Log a Debug message on Start")]
        [SerializeField] private bool logDebug = true;

        [Tooltip("Log an Info message on Start")]
        [SerializeField] private bool logInfo = true;

        [Tooltip("Log a Warning message on Start")]
        [SerializeField] private bool logWarning = true;

        [Tooltip("Log an Error message on Start")]
        [SerializeField] private bool logError = true;

        [Tooltip("Log a Critical message on Start")]
        [SerializeField] private bool logCritical = false;

        [Header("Custom Messages")]
        [Tooltip("Custom message to log (leave empty for default)")]
        [SerializeField] private string customMessage = "";

        private float _lastLogTime;
        private int _updateCount = 0;

        void OnEnable()
        {
            if (logOnEnable)
                LogSmith.Logger.Info("BasicLoggingExample enabled");
        }

        void Start()
        {
            if (!logOnStart) return;

            string msg = string.IsNullOrEmpty(customMessage) ? "BasicLoggingExample started!" : customMessage;

            if (logTrace)
                LogSmith.Logger.Trace($"TRACE: {msg}");

            if (logDebug)
                LogSmith.Logger.Debug($"DEBUG: {msg}");

            if (logInfo)
                LogSmith.Logger.Info($"INFO: {msg}");

            if (logWarning)
                LogSmith.Logger.Warn($"WARNING: {msg}");

            if (logError)
                LogSmith.Logger.Error($"ERROR: {msg} (don't worry, just a demo!)");

            if (logCritical)
                LogSmith.Logger.Critical($"CRITICAL: {msg} (just testing!)");
        }

        void Update()
        {
            if (logOnUpdate && Time.time - _lastLogTime >= updateInterval)
            {
                _updateCount++;
                string msg = includeFrameCount
                    ? $"Update #{_updateCount} at {Time.time:F2}s (Frame {Time.frameCount})"
                    : $"Update tick at {Time.time:F2} seconds";

                LogSmith.Logger.Info(msg);
                _lastLogTime = Time.time;
            }
        }

        void OnDisable()
        {
            if (logOnDisable)
                LogSmith.Logger.Info("BasicLoggingExample disabled");
        }

        void OnDestroy()
        {
            if (logOnDestroy)
                LogSmith.Logger.Info("BasicLoggingExample destroyed");
        }

        [ContextMenu("Log Test Message Now")]
        private void LogTestMessage()
        {
            LogSmith.Logger.Info($"Manual test message triggered at {Time.time:F2}s");
        }

        [ContextMenu("Log All Levels")]
        private void LogAllLevels()
        {
            LogSmith.Logger.Trace("This is TRACE level");
            LogSmith.Logger.Debug("This is DEBUG level");
            LogSmith.Logger.Info("This is INFO level");
            LogSmith.Logger.Warn("This is WARN level");
            LogSmith.Logger.Error("This is ERROR level");
            LogSmith.Logger.Critical("This is CRITICAL level");
        }
    }
}
