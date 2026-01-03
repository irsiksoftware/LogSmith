using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Basic example showing LogSmith usage without dependency injection.
    /// Attach this to any GameObject to see logging in action.
    /// </summary>
    public class BasicLoggingExample : MonoBehaviour
    {
        private ILog _log;

        private void Awake()
        {
            // Get a logger for this category
            // Categories are automatically created on first use
            _log = Log.GetLogger("Gameplay");

            _log.Info("BasicLoggingExample initialized");
        }

        private void Start()
        {
            // Demonstrate different log levels
            _log.Trace("Trace: Detailed diagnostic information");
            _log.Debug("Debug: Development-time information");
            _log.Info("Info: General application flow");
            _log.Warn("Warn: Unusual but handled condition");
            _log.Error("Error: Recoverable error occurred");
            _log.Critical("Critical: System-level failure");

            // Structured logging with parameters
            int playerHealth = 75;
            int maxHealth = 100;
            _log.Info("Player health: {Health}/{MaxHealth}", playerHealth, maxHealth);

            // Log from different categories
            var networkLog = Log.GetLogger("Network");
            networkLog.Info("Connection established");

            var aiLog = Log.GetLogger("AI");
            aiLog.Debug("Pathfinding completed in 15ms");
        }

        private void Update()
        {
            // Example: Conditional logging
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _log.Info("Spacebar pressed at time {Time}", Time.time);
            }
        }

        private void OnDestroy()
        {
            _log?.Info("BasicLoggingExample destroyed");
        }
    }
}
