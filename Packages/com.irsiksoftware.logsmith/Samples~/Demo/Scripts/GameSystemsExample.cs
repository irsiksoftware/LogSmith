using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Simulates different game systems logging independently.
    /// Toggle child GameObjects to enable/disable specific systems.
    /// </summary>
    public class GameSystemsExample : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private bool autoSimulate = true;
        [SerializeField] private float simulationInterval = 2f;

        private ILog _networkLogger;
        private ILog _physicsLogger;
        private ILog _aiLogger;
        private ILog _audioLogger;
        private float _lastSimTime;

        void Start()
        {
            // Create loggers for different game systems
            _networkLogger = Log.CreateLogger("Network");
            _physicsLogger = Log.CreateLogger("Physics");
            _aiLogger = Log.CreateLogger("AI");
            _audioLogger = Log.CreateLogger("Audio");

            _networkLogger.Info("Network system online");
            _physicsLogger.Info("Physics system initialized");
            _aiLogger.Info("AI system ready");
            _audioLogger.Info("Audio system loaded");
        }

        void Update()
        {
            if (autoSimulate && Time.time - _lastSimTime >= simulationInterval)
            {
                SimulateGameSystems();
                _lastSimTime = Time.time;
            }
        }

        [ContextMenu("Simulate Game Systems")]
        public void SimulateGameSystems()
        {
            // Network
            int ping = Random.Range(10, 100);
            if (ping > 80)
                _networkLogger.Warn($"High ping detected: {ping}ms");
            else
                _networkLogger.Debug($"Network ping: {ping}ms");

            // Physics
            int activeRigidbodies = Random.Range(50, 200);
            _physicsLogger.Debug($"Active rigidbodies: {activeRigidbodies}");

            // AI
            int activeAgents = Random.Range(5, 20);
            _aiLogger.Info($"AI agents active: {activeAgents}");

            if (Random.value < 0.1f)
                _aiLogger.Warn("AI pathfinding recalculation triggered");

            // Audio
            int activeSources = Random.Range(3, 15);
            _audioLogger.Debug($"Active audio sources: {activeSources}");
        }

        [ContextMenu("Simulate Error Condition")]
        public void SimulateError()
        {
            _networkLogger.Error("Connection to server lost!");
            _physicsLogger.Error("Physics simulation unstable!");
            _aiLogger.Error("AI navigation mesh corrupted!");
        }
    }
}
