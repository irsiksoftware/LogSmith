using UnityEngine;
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

namespace Irsik.LogSmith.Samples
{
    /// <summary>
    /// Example demonstrating how to configure and use the Seq Sink.
    /// This sample shows how to send structured logs to Seq for powerful log analysis.
    /// </summary>
    public class SeqSinkExample : MonoBehaviour
    {
        [Header("Seq Configuration")]
        [Tooltip("The Seq server URL (e.g., http://localhost:5341)")]
        [SerializeField] private string seqServerUrl = "http://localhost:5341";

        [Tooltip("API key for Seq authentication (optional)")]
        [SerializeField] private string apiKey = "";

        [Tooltip("Minimum log level to send to Seq")]
        [SerializeField] private LogEventLevel minimumLevel = LogEventLevel.Verbose;

        [Tooltip("Batch size for batching logs before sending")]
        [SerializeField] private int batchSizeLimit = 100;

        [Header("Gameplay Simulation")]
        [SerializeField] private float simulationSpeed = 1f;
        private float _gameTime = 0f;
        private int _playerScore = 0;
        private int _enemiesDefeated = 0;

        private ILogger _logger;

        void Start()
        {
            // Configure LogSmith with Seq Sink
            var config = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("Application", "LogSmith-SeqExample")
                .Enrich.WithProperty("Environment", "Development")
                .Enrich.WithProperty("MachineName", System.Environment.MachineName);

            if (string.IsNullOrEmpty(apiKey))
            {
                config.WriteTo.Seq(
                    serverUrl: seqServerUrl,
                    restrictedToMinimumLevel: minimumLevel,
                    batchPostingLimit: batchSizeLimit
                );
            }
            else
            {
                config.WriteTo.Seq(
                    serverUrl: seqServerUrl,
                    apiKey: apiKey,
                    restrictedToMinimumLevel: minimumLevel,
                    batchPostingLimit: batchSizeLimit
                );
            }

            _logger = config.CreateLogger();
            LogSmith.Initialize(_logger);

            Debug.Log("Seq Sink Example started. Logs will be sent to: " + seqServerUrl);

            // Log application startup
            LogSmith.Information("Application started with Seq logging");
            LogSmith.Debug("Scene loaded: {SceneName}", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        void Update()
        {
            _gameTime += Time.deltaTime * simulationSpeed;
        }

        void OnDestroy()
        {
            // Log application shutdown with final stats
            LogSmith.Information(
                "Application shutdown - Final Stats: PlayTime={PlayTime:0.0}s Score={Score} Enemies={Enemies}",
                _gameTime,
                _playerScore,
                _enemiesDefeated
            );

            // Ensure all logs are flushed before shutting down
            LogSmith.CloseAndFlush();
        }

        // UI for testing structured logging
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.Label("Seq Sink Example - Structured Logging", GUI.skin.box);
            GUILayout.Label($"Server: {seqServerUrl}");
            GUILayout.Label($"Game Time: {_gameTime:F1}s | Score: {_playerScore} | Enemies: {_enemiesDefeated}");
            GUILayout.Space(10);

            GUILayout.Label("Basic Logging:", GUI.skin.box);

            if (GUILayout.Button("Log Verbose"))
            {
                LogSmith.Verbose("Detailed trace information at {Timestamp}", System.DateTime.Now);
            }

            if (GUILayout.Button("Log Debug"))
            {
                LogSmith.Debug("Debug message with value: {DebugValue}", Random.Range(1, 100));
            }

            if (GUILayout.Button("Log Information"))
            {
                LogSmith.Information("Information message logged");
            }

            if (GUILayout.Button("Log Warning"))
            {
                LogSmith.Warning("Warning: Low memory detected {AvailableMemory}MB", Random.Range(100, 500));
            }

            GUILayout.Space(10);
            GUILayout.Label("Structured Logging Examples:", GUI.skin.box);

            if (GUILayout.Button("Player Action - Jump"))
            {
                LogGameEvent("PlayerJump", new
                {
                    PlayerId = "player-001",
                    Position = new Vector3(Random.Range(-10f, 10f), Random.Range(0f, 5f), 0),
                    Velocity = Random.Range(5f, 10f),
                    GameTime = _gameTime
                });
            }

            if (GUILayout.Button("Player Action - Collect Item"))
            {
                string itemType = Random.value > 0.5f ? "Coin" : "PowerUp";
                int itemValue = itemType == "Coin" ? 10 : 100;
                _playerScore += itemValue;

                LogSmith.Information(
                    "Player collected {ItemType} worth {Value} points. Total score: {TotalScore}",
                    itemType,
                    itemValue,
                    _playerScore
                );
            }

            if (GUILayout.Button("Enemy Defeated"))
            {
                _enemiesDefeated++;
                string enemyType = new[] { "Slime", "Goblin", "Dragon", "Boss" }[Random.Range(0, 4)];
                int xpGained = Random.Range(50, 200);

                LogSmith.Information(
                    "Enemy defeated: {EnemyType} | XP: {Experience} | Total: {TotalEnemies} | Time: {GameTime:0.0}s",
                    enemyType,
                    xpGained,
                    _enemiesDefeated,
                    _gameTime
                );
            }

            if (GUILayout.Button("Performance Metrics"))
            {
                LogSmith.Information(
                    "Performance: FPS={FPS:0.0} DrawCalls={DrawCalls} Vertices={Vertices} Memory={Memory}MB",
                    1f / Time.deltaTime,
                    Random.Range(50, 200),
                    Random.Range(1000, 50000),
                    Random.Range(100, 500)
                );
            }

            if (GUILayout.Button("Complex Structured Event"))
            {
                LogComplexEvent();
            }

            GUILayout.Space(10);
            GUILayout.Label("Query Examples in Seq:", GUI.skin.box);
            GUILayout.Label("- Select * from stream where ItemType = 'PowerUp'");
            GUILayout.Label("- Select * from stream where TotalScore > 100");
            GUILayout.Label("- Select * from stream where EnemyType = 'Boss'");
            GUILayout.Label("- Select * from stream where FPS < 30");

            GUILayout.EndArea();
        }

        private void LogGameEvent(string eventType, object data)
        {
            LogSmith.Information(
                "Game Event: {EventType} {@EventData}",
                eventType,
                data
            );
        }

        private void LogComplexEvent()
        {
            var gameState = new
            {
                Player = new
                {
                    Id = "player-001",
                    Name = "TestPlayer",
                    Level = Random.Range(1, 50),
                    Score = _playerScore,
                    Position = new { X = Random.Range(-10f, 10f), Y = Random.Range(-10f, 10f), Z = 0f },
                    Inventory = new[]
                    {
                        new { Item = "Sword", Quantity = 1, Rarity = "Legendary" },
                        new { Item = "Potion", Quantity = 5, Rarity = "Common" },
                        new { Item = "Shield", Quantity = 1, Rarity = "Rare" }
                    }
                },
                Session = new
                {
                    Id = System.Guid.NewGuid(),
                    Duration = _gameTime,
                    EnemiesDefeated = _enemiesDefeated
                },
                System = new
                {
                    Platform = Application.platform.ToString(),
                    UnityVersion = Application.unityVersion,
                    SystemMemory = SystemInfo.systemMemorySize
                }
            };

            LogSmith.Information("Complex game state snapshot: {@GameState}", gameState);
        }
    }
}
