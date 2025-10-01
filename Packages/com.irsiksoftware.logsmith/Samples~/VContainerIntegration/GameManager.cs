using VContainer.Unity;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Example game manager using constructor-injected ILog.
    /// Demonstrates typical VContainer + LogSmith usage.
    /// </summary>
    public class GameManager : IStartable, ITickable
    {
        private readonly ILog _log;
        private readonly NetworkManager _networkManager;
        private int _frameCount;

        // Constructor injection - VContainer provides these automatically
        public GameManager(ILog log, NetworkManager networkManager)
        {
            _log = log;
            _networkManager = networkManager;

            _log.Debug("GameManager constructor called");
        }

        public void Start()
        {
            _log.Info("=== Game Starting ===");
            _log.Info("LogSmith initialized with VContainer DI");

            // Call other services that also use logging
            _networkManager.Connect();
        }

        public void Tick()
        {
            _frameCount++;

            // Example: Periodic logging
            if (_frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                _log.Debug("Heartbeat: Frame {Frame}", _frameCount);
            }
        }
    }

    /// <summary>
    /// Example service demonstrating category-specific logger.
    /// </summary>
    public class NetworkManager
    {
        private readonly ILog _log;

        // Request a category-specific logger via ILogFactory
        public NetworkManager(ILogFactory logFactory)
        {
            _log = logFactory.CreateLogger("Network");
            _log.Debug("NetworkManager created");
        }

        public void Connect()
        {
            _log.Info("Connecting to server...");
            _log.Info("Connection established to 192.168.1.100:7777");
        }
    }

    /// <summary>
    /// Example transient service - each instance gets the same logger.
    /// </summary>
    public class PlayerController
    {
        private readonly ILog _log;

        public PlayerController(ILog log)
        {
            _log = log;
            _log.Trace("PlayerController instantiated");
        }
    }
}
