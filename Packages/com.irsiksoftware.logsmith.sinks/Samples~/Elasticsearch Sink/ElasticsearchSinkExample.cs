using UnityEngine;
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;
using System;

namespace Irsik.LogSmith.Samples
{
    /// <summary>
    /// Example demonstrating how to configure and use the Elasticsearch Sink.
    /// This sample shows how to send logs to Elasticsearch for large-scale log analysis and storage.
    /// </summary>
    public class ElasticsearchSinkExample : MonoBehaviour
    {
        [Header("Elasticsearch Configuration")]
        [Tooltip("Elasticsearch node URIs (comma-separated for multiple nodes)")]
        [SerializeField] private string elasticsearchNodes = "http://localhost:9200";

        [Tooltip("Index name format (supports date formatting, e.g., logsmith-{0:yyyy.MM.dd})")]
        [SerializeField] private string indexFormat = "logsmith-{0:yyyy.MM.dd}";

        [Tooltip("Username for authentication (leave empty if no auth)")]
        [SerializeField] private string username = "";

        [Tooltip("Password for authentication (leave empty if no auth)")]
        [SerializeField] private string password = "";

        [Tooltip("Minimum log level to send to Elasticsearch")]
        [SerializeField] private LogEventLevel minimumLevel = LogEventLevel.Verbose;

        [Tooltip("Batch size for batching logs before sending")]
        [SerializeField] private int batchSizeLimit = 50;

        [Header("Demo Settings")]
        [SerializeField] private bool autoGenerateLogs = false;
        [SerializeField] private float logGenerationInterval = 2f;
        private float _nextLogTime = 0f;

        private ILogger _logger;
        private int _sessionEventCount = 0;

        void Start()
        {
            // Parse node URIs
            var nodeUris = elasticsearchNodes.Split(',');
            var nodes = new Uri[nodeUris.Length];
            for (int i = 0; i < nodeUris.Length; i++)
            {
                nodes[i] = new Uri(nodeUris[i].Trim());
            }

            // Configure LogSmith with Elasticsearch Sink
            var config = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", "LogSmith-ElasticsearchExample")
                .Enrich.WithProperty("Environment", "Development")
                .Enrich.WithProperty("MachineName", System.Environment.MachineName)
                .Enrich.WithProperty("SessionId", Guid.NewGuid().ToString());

            // Configure Elasticsearch sink based on authentication
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                config.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(nodes)
                {
                    IndexFormat = indexFormat,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv7,
                    MinimumLogEventLevel = minimumLevel,
                    BatchPostingLimit = batchSizeLimit,
                    EmitEventFailure = Serilog.Sinks.Elasticsearch.EmitEventFailureHandling.WriteToSelfLog,
                    FailureCallback = e => Debug.LogError($"Elasticsearch error: {e.MessageTemplate}")
                });
            }
            else
            {
                config.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(nodes)
                {
                    IndexFormat = indexFormat,
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv7,
                    ModifyConnectionSettings = conn => conn.BasicAuthentication(username, password),
                    MinimumLogEventLevel = minimumLevel,
                    BatchPostingLimit = batchSizeLimit,
                    EmitEventFailure = Serilog.Sinks.Elasticsearch.EmitEventFailureHandling.WriteToSelfLog,
                    FailureCallback = e => Debug.LogError($"Elasticsearch error: {e.MessageTemplate}")
                });
            }

            _logger = config.CreateLogger();
            LogSmith.Initialize(_logger);

            Debug.Log("Elasticsearch Sink Example started. Logs will be sent to: " + elasticsearchNodes);

            // Log application startup
            LogSmith.Information("Elasticsearch sink initialized for Unity application");
            LogSmith.Debug("Index format: {IndexFormat}", indexFormat);
        }

        void Update()
        {
            if (autoGenerateLogs && Time.time >= _nextLogTime)
            {
                GenerateRandomLog();
                _nextLogTime = Time.time + logGenerationInterval;
            }
        }

        void OnDestroy()
        {
            LogSmith.Information("Application shutdown - Total events logged: {EventCount}", _sessionEventCount);
            LogSmith.CloseAndFlush();
        }

        private void GenerateRandomLog()
        {
            var logTypes = new[] { "PlayerAction", "SystemEvent", "NetworkEvent", "AssetLoad", "PerformanceMetric" };
            var selectedType = logTypes[UnityEngine.Random.Range(0, logTypes.Length)];

            switch (selectedType)
            {
                case "PlayerAction":
                    LogPlayerAction();
                    break;
                case "SystemEvent":
                    LogSystemEvent();
                    break;
                case "NetworkEvent":
                    LogNetworkEvent();
                    break;
                case "AssetLoad":
                    LogAssetLoad();
                    break;
                case "PerformanceMetric":
                    LogPerformanceMetric();
                    break;
            }

            _sessionEventCount++;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 650));
            GUILayout.Label("Elasticsearch Sink Example", GUI.skin.box);
            GUILayout.Label($"Nodes: {elasticsearchNodes}");
            GUILayout.Label($"Index: {indexFormat}");
            GUILayout.Label($"Events logged: {_sessionEventCount}");
            GUILayout.Space(10);

            GUILayout.Label("Manual Logging:", GUI.skin.box);

            if (GUILayout.Button("Log Player Action"))
            {
                LogPlayerAction();
                _sessionEventCount++;
            }

            if (GUILayout.Button("Log System Event"))
            {
                LogSystemEvent();
                _sessionEventCount++;
            }

            if (GUILayout.Button("Log Network Event"))
            {
                LogNetworkEvent();
                _sessionEventCount++;
            }

            if (GUILayout.Button("Log Asset Loading"))
            {
                LogAssetLoad();
                _sessionEventCount++;
            }

            if (GUILayout.Button("Log Performance Metrics"))
            {
                LogPerformanceMetric();
                _sessionEventCount++;
            }

            if (GUILayout.Button("Log Error with Stack Trace"))
            {
                try
                {
                    throw new Exception("Simulated exception for testing");
                }
                catch (Exception ex)
                {
                    LogSmith.Error(ex, "Exception occurred during operation: {Operation}", "TestOperation");
                    _sessionEventCount++;
                }
            }

            if (GUILayout.Button("Log Complex Event"))
            {
                LogComplexEvent();
                _sessionEventCount++;
            }

            GUILayout.Space(10);
            autoGenerateLogs = GUILayout.Toggle(autoGenerateLogs, "Auto-generate logs");
            if (autoGenerateLogs)
            {
                GUILayout.Label($"Next log in: {Mathf.Max(0, _nextLogTime - Time.time):F1}s");
            }

            GUILayout.Space(10);
            GUILayout.Label("Kibana Query Examples:", GUI.skin.box);
            GUILayout.Label("Application:\"LogSmith-ElasticsearchExample\"");
            GUILayout.Label("EventType:PlayerAction AND ActionType:Jump");
            GUILayout.Label("Level:Error");
            GUILayout.Label("Duration:>1000");

            GUILayout.Space(10);
            if (GUILayout.Button("Open Kibana (localhost:5601)"))
            {
                Application.OpenURL("http://localhost:5601");
            }

            GUILayout.EndArea();
        }

        private void LogPlayerAction()
        {
            var actions = new[] { "Jump", "Attack", "Defend", "UseItem", "Move" };
            var action = actions[UnityEngine.Random.Range(0, actions.Length)];

            LogSmith.Information(
                "Player action: {EventType} {ActionType} at {Position} | Duration: {Duration}ms",
                "PlayerAction",
                action,
                new Vector3(UnityEngine.Random.Range(-100f, 100f), UnityEngine.Random.Range(0f, 50f), 0f),
                UnityEngine.Random.Range(50, 500)
            );
        }

        private void LogSystemEvent()
        {
            var events = new[] { "SceneLoaded", "ConfigUpdated", "CacheCleared", "ServiceStarted" };
            var eventType = events[UnityEngine.Random.Range(0, events.Length)];

            LogSmith.Information(
                "System event: {EventType} {SystemEvent} | Memory: {MemoryMB}MB",
                "SystemEvent",
                eventType,
                UnityEngine.Random.Range(100, 2000)
            );
        }

        private void LogNetworkEvent()
        {
            var events = new[] { "Connected", "Disconnected", "MessageSent", "MessageReceived", "Timeout" };
            var eventType = events[UnityEngine.Random.Range(0, events.Length)];
            var latency = UnityEngine.Random.Range(10, 300);

            LogSmith.Information(
                "Network event: {EventType} {NetworkEvent} | Latency: {Latency}ms | Server: {Server}",
                "NetworkEvent",
                eventType,
                latency,
                $"server-{UnityEngine.Random.Range(1, 5)}"
            );
        }

        private void LogAssetLoad()
        {
            var assetTypes = new[] { "Texture", "Model", "Audio", "Prefab", "Scene" };
            var assetType = assetTypes[UnityEngine.Random.Range(0, assetTypes.Length)];
            var loadTime = UnityEngine.Random.Range(10, 2000);

            LogSmith.Information(
                "Asset loaded: {EventType} {AssetType} '{AssetName}' | Size: {SizeKB}KB | LoadTime: {LoadTime}ms",
                "AssetLoad",
                assetType,
                $"{assetType}_{UnityEngine.Random.Range(1, 100)}",
                UnityEngine.Random.Range(50, 5000),
                loadTime
            );
        }

        private void LogPerformanceMetric()
        {
            LogSmith.Information(
                "Performance metrics: {EventType} | FPS: {FPS:F1} | DrawCalls: {DrawCalls} | Triangles: {Triangles} | Memory: {MemoryMB}MB",
                "PerformanceMetric",
                1f / Time.deltaTime,
                UnityEngine.Random.Range(50, 500),
                UnityEngine.Random.Range(1000, 100000),
                UnityEngine.Random.Range(200, 1500)
            );
        }

        private void LogComplexEvent()
        {
            var gameState = new
            {
                EventType = "GameStateSnapshot",
                Player = new
                {
                    Id = Guid.NewGuid().ToString(),
                    Level = UnityEngine.Random.Range(1, 100),
                    Health = UnityEngine.Random.Range(0, 100),
                    Position = new { X = UnityEngine.Random.Range(-100f, 100f), Y = UnityEngine.Random.Range(-100f, 100f) },
                    Inventory = new[] { "Sword", "Shield", "Potion" }
                },
                World = new
                {
                    Scene = "MainGame",
                    EnemyCount = UnityEngine.Random.Range(0, 20),
                    WeatherCondition = new[] { "Sunny", "Rainy", "Stormy" }[UnityEngine.Random.Range(0, 3)]
                },
                Performance = new
                {
                    FPS = 1f / Time.deltaTime,
                    MemoryMB = UnityEngine.Random.Range(200, 1500)
                }
            };

            LogSmith.Information("Complex event: {@GameState}", gameState);
        }
    }
}
