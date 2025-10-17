using UnityEngine;
using IrsikSoftware.LogSmith;

#if VCONTAINER_PRESENT
using VContainer;
using VContainer.Unity;
#endif

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Demonstrates dependency injection with VContainer.
    /// Requires LoggingLifetimeScope in the scene.
    /// </summary>
    public class VContainerLoggingExample : MonoBehaviour
    {
        // This will be injected by VContainer if available
        private ILog _injectedLogger;

        [Header("Settings")]
        [SerializeField] private bool useDependencyInjection = true;

        void Start()
        {
            if (useDependencyInjection && LogSmith.IsUsingDependencyInjection)
            {
                // Logger was injected by VContainer
                if (_injectedLogger != null)
                {
                    _injectedLogger.Info("Using injected logger from VContainer!");
                    _injectedLogger.Debug("Dependency injection is working correctly");
                }
                else
                {
                    Debug.LogWarning("DI enabled but logger not injected. Make sure LoggingLifetimeScope is in the scene.");
                    UseFallbackLogger();
                }
            }
            else
            {
                UseFallbackLogger();
            }
        }

        private void UseFallbackLogger()
        {
            var fallbackLogger = LogSmith.CreateLogger("VContainerDemo");
            fallbackLogger.Info("Using static API fallback (no DI)");
            fallbackLogger.Debug("This works without VContainer setup");
        }

#if VCONTAINER_PRESENT
        // VContainer will call this method to inject dependencies
        [Inject]
        public void Construct(ILog log)
        {
            _injectedLogger = log;
            Debug.Log("VContainerLoggingExample: Logger injected successfully!");
        }
#endif

        [ContextMenu("Check DI Status")]
        public void CheckDIStatus()
        {
            if (LogSmith.IsUsingDependencyInjection)
            {
                Debug.Log("✓ LogSmith is using VContainer DI");
                if (_injectedLogger != null)
                    _injectedLogger.Info("DI check: Logger is properly injected");
            }
            else
            {
                Debug.Log("✗ LogSmith is using static fallback (no DI)");
            }
        }
    }
}
