using UnityEngine;
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

namespace Irsik.LogSmith.Samples
{
    /// <summary>
    /// Example demonstrating how to configure and use the Sentry Sink.
    /// This sample shows how to track errors and exceptions using Sentry.
    /// </summary>
    public class SentrySinkExample : MonoBehaviour
    {
        [Header("Sentry Configuration")]
        [Tooltip("Your Sentry DSN (Data Source Name)")]
        [SerializeField] private string sentryDsn = "https://your-key@o123456.ingest.sentry.io/123456";

        [Tooltip("Minimum log level to send to Sentry")]
        [SerializeField] private LogEventLevel minimumLevel = LogEventLevel.Warning;

        [Tooltip("Environment name (e.g., Development, Staging, Production)")]
        [SerializeField] private string environment = "Development";

        [Tooltip("Release version identifier")]
        [SerializeField] private string release = "1.0.0";

        private ILogger _logger;

        void Start()
        {
            // Configure LogSmith with Sentry Sink
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Sentry(o =>
                {
                    o.Dsn = sentryDsn;
                    o.Environment = environment;
                    o.Release = release;
                    o.MinimumEventLevel = minimumLevel;
                    o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                    o.Debug = true;
                    o.AttachStacktrace = true;
                })
                .CreateLogger();

            LogSmith.Initialize(_logger);

            Debug.Log("Sentry Sink Example started. Errors will be sent to Sentry.");

            // Add breadcrumbs for context
            LogSmith.Information("Application started");
            LogSmith.Debug("Scene loaded: {SceneName}", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            // Ensure all logs are flushed before shutting down
            LogSmith.CloseAndFlush();
        }

        // UI buttons for testing different error scenarios
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.Label("Sentry Sink Example", GUI.skin.box);
            GUILayout.Label($"Environment: {environment}");
            GUILayout.Label($"Release: {release}");
            GUILayout.Space(10);

            GUILayout.Label("Error Tracking Examples:", GUI.skin.box);

            if (GUILayout.Button("Log Warning"))
            {
                LogSmith.Warning("User attempted invalid action at {Time}", System.DateTime.Now);
            }

            if (GUILayout.Button("Log Error"))
            {
                LogSmith.Error("Failed to load resource: {ResourceName}", "PlayerTexture.png");
            }

            if (GUILayout.Button("Log Exception - Null Reference"))
            {
                try
                {
                    SimulateNullReferenceException();
                }
                catch (System.Exception ex)
                {
                    LogSmith.Error(ex, "Null reference exception occurred in gameplay");
                }
            }

            if (GUILayout.Button("Log Exception - Index Out of Range"))
            {
                try
                {
                    SimulateIndexOutOfRangeException();
                }
                catch (System.Exception ex)
                {
                    LogSmith.Error(ex, "Array index error in inventory system");
                }
            }

            if (GUILayout.Button("Log Fatal Error"))
            {
                LogSmith.Fatal("Critical system failure detected");
            }

            GUILayout.Space(10);
            GUILayout.Label("Structured Data Examples:", GUI.skin.box);

            if (GUILayout.Button("Log Error with User Context"))
            {
                LogSmith.Error(
                    "Player save failed for user {UserId} {UserName}",
                    12345,
                    "TestPlayer"
                );
            }

            if (GUILayout.Button("Log Error with Game State"))
            {
                LogSmith.Error(
                    "Game state corruption detected: Level={Level} Score={Score} Health={Health}",
                    5,
                    9999,
                    0
                );
            }

            GUILayout.Space(10);
            if (!sentryDsn.Contains("your-key"))
            {
                GUILayout.Label("Sentry is configured!", GUI.skin.box);
            }
            else
            {
                GUILayout.Label("WARNING: Update Sentry DSN!", GUI.skin.box);
            }

            GUILayout.EndArea();
        }

        private void SimulateNullReferenceException()
        {
            GameObject obj = null;
            obj.transform.position = Vector3.zero; // This will throw NullReferenceException
        }

        private void SimulateIndexOutOfRangeException()
        {
            int[] array = new int[5];
            int value = array[10]; // This will throw IndexOutOfRangeException
        }
    }
}
