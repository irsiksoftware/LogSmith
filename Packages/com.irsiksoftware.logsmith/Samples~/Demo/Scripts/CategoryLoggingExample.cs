using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Demonstrates category-specific logging.
    /// Shows how different systems can have their own log categories.
    /// </summary>
    public class CategoryLoggingExample : MonoBehaviour
    {
        [Header("Category Configuration")]
        [Tooltip("Name of the log category (matches category in LogSmith Settings)")]
        [SerializeField] private string categoryName = "Gameplay";

        [Tooltip("Color to display in logs (visual reference only)")]
        [SerializeField] private Color categoryColor = Color.cyan;

        [Header("Auto Logging")]
        [Tooltip("Automatically log on Start()")]
        [SerializeField] private bool logOnStart = true;

        [Tooltip("Log when GameObject is enabled/disabled")]
        [SerializeField] private bool logLifecycleEvents = true;

        [Tooltip("Periodically log status updates")]
        [SerializeField] private bool autoLogUpdates = false;

        [Tooltip("Interval between auto-log updates (seconds)")]
        [SerializeField] [Range(1f, 30f)] private float updateInterval = 5f;

        [Header("Message Customization")]
        [Tooltip("Prefix to add to all messages from this category")]
        [SerializeField] private string messagePrefix = "";

        [Tooltip("Include timestamp in messages")]
        [SerializeField] private bool includeTimestamp = false;

        [Tooltip("Include GameObject name in messages")]
        [SerializeField] private bool includeGameObjectName = true;

        [Header("Statistics")]
        [Tooltip("Track and display message counts")]
        [SerializeField] private bool trackStatistics = true;

        [SerializeField] [HideInInspector] private int messagesLogged = 0;

        private ILog _categoryLogger;
        private float _lastUpdateTime;

        void OnEnable()
        {
            if (logLifecycleEvents && _categoryLogger != null)
            {
                _categoryLogger.Debug(FormatMessage("Category enabled"));
            }
        }

        void Start()
        {
            // Create a category-specific logger
            _categoryLogger = LogSmith.CreateLogger(categoryName);

            if (logOnStart)
            {
                _categoryLogger.Info(FormatMessage("Category system initialized"));
                _categoryLogger.Debug(FormatMessage($"Using category: '{categoryName}'"));

                if (trackStatistics)
                {
                    _categoryLogger.Debug(FormatMessage($"Statistics tracking enabled"));
                }
            }
        }

        void Update()
        {
            if (autoLogUpdates && Time.time - _lastUpdateTime >= updateInterval)
            {
                _categoryLogger.Info(FormatMessage($"Periodic update (interval: {updateInterval}s)"));
                _lastUpdateTime = Time.time;
            }
        }

        private string FormatMessage(string message)
        {
            string formatted = message;

            if (!string.IsNullOrEmpty(messagePrefix))
                formatted = $"{messagePrefix} {formatted}";

            if (includeGameObjectName)
                formatted = $"[{gameObject.name}] {formatted}";

            if (includeTimestamp)
                formatted = $"[{System.DateTime.Now:HH:mm:ss}] {formatted}";

            if (trackStatistics)
                messagesLogged++;

            return formatted;
        }

        [ContextMenu("Log Info")]
        public void LogInfoMessage()
        {
            _categoryLogger.Info(FormatMessage("Manual Info message triggered"));
        }

        [ContextMenu("Log Debug")]
        public void LogDebugMessage()
        {
            _categoryLogger.Debug(FormatMessage("Manual Debug message triggered"));
        }

        [ContextMenu("Log Warning")]
        public void LogWarningMessage()
        {
            _categoryLogger.Warn(FormatMessage("Manual Warning message triggered"));
        }

        [ContextMenu("Log Error")]
        public void LogErrorMessage()
        {
            _categoryLogger.Error(FormatMessage("Manual Error message triggered"));
        }

        [ContextMenu("Log All Levels")]
        public void LogAllLevels()
        {
            _categoryLogger.Trace(FormatMessage("Trace level message"));
            _categoryLogger.Debug(FormatMessage("Debug level message"));
            _categoryLogger.Info(FormatMessage("Info level message"));
            _categoryLogger.Warn(FormatMessage("Warning level message"));
            _categoryLogger.Error(FormatMessage("Error level message"));
            _categoryLogger.Critical(FormatMessage("Critical level message"));
        }

        [ContextMenu("Show Statistics")]
        public void ShowStatistics()
        {
            if (trackStatistics)
            {
                _categoryLogger.Info(FormatMessage($"Total messages logged: {messagesLogged}"));
                _categoryLogger.Info(FormatMessage($"Category: {categoryName}, GameObject: {gameObject.name}"));
            }
            else
            {
                Debug.Log("Statistics tracking is disabled");
            }
        }

        [ContextMenu("Reset Statistics")]
        public void ResetStatistics()
        {
            messagesLogged = 0;
            _categoryLogger?.Info(FormatMessage("Statistics reset"));
        }

        void OnDisable()
        {
            if (logLifecycleEvents && _categoryLogger != null)
            {
                _categoryLogger.Debug(FormatMessage("Category disabled"));
            }
        }

        void OnDestroy()
        {
            if (_categoryLogger != null)
            {
                if (trackStatistics)
                {
                    _categoryLogger.Info(FormatMessage($"Shutting down (Total messages: {messagesLogged})"));
                }
                else
                {
                    _categoryLogger.Info(FormatMessage("Shutting down"));
                }
            }
        }

        // Inspector visualization
        void OnDrawGizmosSelected()
        {
            Gizmos.color = categoryColor;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
