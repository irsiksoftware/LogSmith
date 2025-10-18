using UnityEngine;
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

namespace Irsik.LogSmith.Samples
{
    /// <summary>
    /// Example demonstrating how to configure and use the HTTP Sink.
    /// This sample shows how to send logs to a remote HTTP endpoint.
    /// </summary>
    public class HttpSinkExample : MonoBehaviour
    {
        [Header("HTTP Sink Configuration")]
        [Tooltip("The HTTP endpoint to send logs to (e.g., http://localhost:5000/logs)")]
        [SerializeField] private string httpEndpoint = "http://localhost:5000/logs";

        [Tooltip("Minimum log level to send to HTTP endpoint")]
        [SerializeField] private LogEventLevel minimumLevel = LogEventLevel.Information;

        [Tooltip("Batch size for batching logs before sending")]
        [SerializeField] private int batchSizeLimit = 50;

        [Tooltip("Time period in seconds before sending batch")]
        [SerializeField] private float batchPeriodSeconds = 2f;

        private ILogger _logger;

        void Start()
        {
            // Configure LogSmith with HTTP Sink
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Http(
                    requestUri: httpEndpoint,
                    restrictedToMinimumLevel: minimumLevel,
                    batchPostingLimit: batchSizeLimit,
                    period: System.TimeSpan.FromSeconds(batchPeriodSeconds)
                )
                .CreateLogger();

            LogSmith.Initialize(_logger);

            Debug.Log("HTTP Sink Example started. Logs will be sent to: " + httpEndpoint);

            // Log some example messages
            LogExampleMessages();
        }

        void OnDestroy()
        {
            // Ensure all logs are flushed before shutting down
            LogSmith.CloseAndFlush();
        }

        private void LogExampleMessages()
        {
            LogSmith.Verbose("This is a verbose message");
            LogSmith.Debug("This is a debug message");
            LogSmith.Information("HTTP Sink is configured and ready");
            LogSmith.Warning("This is a warning message");

            // Example with structured data
            LogSmith.Information("Player data: {PlayerId} {PlayerName} {Score}",
                12345, "TestPlayer", 9999);
        }

        // UI buttons for testing
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("HTTP Sink Example", GUI.skin.box);
            GUILayout.Label($"Endpoint: {httpEndpoint}");
            GUILayout.Space(10);

            if (GUILayout.Button("Log Information"))
            {
                LogSmith.Information("Test information message at {Time}", System.DateTime.Now);
            }

            if (GUILayout.Button("Log Warning"))
            {
                LogSmith.Warning("Test warning message");
            }

            if (GUILayout.Button("Log Error"))
            {
                LogSmith.Error("Test error message");
            }

            if (GUILayout.Button("Log with Exception"))
            {
                try
                {
                    throw new System.Exception("Test exception for logging");
                }
                catch (System.Exception ex)
                {
                    LogSmith.Error(ex, "An exception occurred during testing");
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("Note: Make sure your HTTP endpoint is running!");

            GUILayout.EndArea();
        }
    }
}
