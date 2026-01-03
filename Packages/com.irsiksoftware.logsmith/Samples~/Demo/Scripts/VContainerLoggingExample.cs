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
            // If logger was injected by VContainer, use it
            if (_injectedLogger != null && useDependencyInjection)
            {
                _injectedLogger.Info("Using injected logger from VContainer!");
                _injectedLogger.Debug("Dependency injection is working correctly");
            }
            else
            {
                // Fall back to static API if DI not available or disabled
                if (_injectedLogger == null && useDependencyInjection)
                {
                    Debug.LogWarning("DI enabled but logger not injected. Make sure LoggingLifetimeScope is in the scene.");
                }
                UseFallbackLogger();
            }
        }

        private void UseFallbackLogger()
        {
            var fallbackLogger = Log.CreateLogger("VContainerDemo");
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
            if (_injectedLogger != null)
            {
                Debug.Log("✓ LogSmith logger was injected via VContainer DI");
                _injectedLogger.Info("DI check: Logger is properly injected");
            }
            else
            {
                Debug.Log("✗ Logger not injected - using static fallback API");
                var fallbackLogger = Log.CreateLogger("VContainerDemo");
                fallbackLogger.Info("DI check: Using static API");
            }
        }
    }
}
