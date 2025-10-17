using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Demonstrates all log levels with examples.
    /// Use the context menu to trigger different log levels.
    /// </summary>
    public class LogLevelExample : MonoBehaviour
    {
        [Header("Log Level Demonstration")]
        [SerializeField] private string categoryName = "LogLevels";

        private ILog _logger;

        void Start()
        {
            _logger = LogSmith.CreateLogger(categoryName);
            _logger.Info("LogLevelExample initialized - use context menu to test different log levels");
        }

        [ContextMenu("1. Trace - Verbose Debugging")]
        public void LogTrace()
        {
            _logger.Trace("TRACE: Very detailed debugging information");
            _logger.Trace("TRACE: Function entered with parameters: x=10, y=20");
            _logger.Trace("TRACE: Loop iteration 5 of 100");
        }

        [ContextMenu("2. Debug - General Debugging")]
        public void LogDebug()
        {
            _logger.Debug("DEBUG: General debugging information");
            _logger.Debug("DEBUG: Variable state - position: (5.2, 3.1, 0.0)");
            _logger.Debug("DEBUG: Cache hit rate: 85%");
        }

        [ContextMenu("3. Info - Informational")]
        public void LogInfo()
        {
            _logger.Info("INFO: Normal application flow");
            _logger.Info("INFO: User logged in successfully");
            _logger.Info("INFO: Level loaded: MainMenu");
            _logger.Info("INFO: Configuration applied");
        }

        [ContextMenu("4. Warn - Warning Conditions")]
        public void LogWarn()
        {
            _logger.Warn("WARN: Potential issue detected");
            _logger.Warn("WARN: Deprecated API usage detected");
            _logger.Warn("WARN: Memory usage at 80%");
            _logger.Warn("WARN: Connection retry attempt 3 of 5");
        }

        [ContextMenu("5. Error - Error Conditions")]
        public void LogError()
        {
            _logger.Error("ERROR: Operation failed");
            _logger.Error("ERROR: File not found: config.json");
            _logger.Error("ERROR: Network request timed out");
            _logger.Error("ERROR: Failed to parse JSON data");
        }

        [ContextMenu("6. Critical - Critical Failures")]
        public void LogCritical()
        {
            _logger.Critical("CRITICAL: System failure!");
            _logger.Critical("CRITICAL: Database connection lost!");
            _logger.Critical("CRITICAL: Unhandled exception in main loop!");
        }

        [ContextMenu("7. Log All Levels")]
        public void LogAllLevels()
        {
            _logger.Trace("This is a TRACE message");
            _logger.Debug("This is a DEBUG message");
            _logger.Info("This is an INFO message");
            _logger.Warn("This is a WARN message");
            _logger.Error("This is an ERROR message");
            _logger.Critical("This is a CRITICAL message");

            _logger.Info("All log levels demonstrated!");
        }

        [ContextMenu("8. Realistic Scenario")]
        public void LogRealisticScenario()
        {
            _logger.Info("Starting game initialization...");
            _logger.Debug("Loading configuration from disk");
            _logger.Trace("Reading config file: settings.json");

            _logger.Info("Connecting to server...");
            _logger.Debug("Server address: game.example.com:7777");
            _logger.Warn("Connection attempt 1 failed, retrying...");
            _logger.Info("Successfully connected to server");

            _logger.Debug("Downloading player data");
            _logger.Info("Player data loaded successfully");

            _logger.Info("Game initialization complete!");
        }
    }
}
