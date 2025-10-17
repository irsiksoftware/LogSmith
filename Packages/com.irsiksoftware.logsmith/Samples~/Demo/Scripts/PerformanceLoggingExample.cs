using UnityEngine;
using IrsikSoftware.LogSmith;

namespace IrsikSoftware.LogSmith.Samples
{
    /// <summary>
    /// Demonstrates performance-related logging with frame timing.
    /// Useful for profiling and performance monitoring.
    /// </summary>
    public class PerformanceLoggingExample : MonoBehaviour
    {
        [Header("Performance Monitoring")]
        [SerializeField] private bool monitorFrameRate = true;
        [SerializeField] private float checkInterval = 5f;
        [SerializeField] private int lowFpsThreshold = 30;
        [SerializeField] private int targetFps = 60;

        private ILog _performanceLogger;
        private float _lastCheckTime;
        private int _frameCount;
        private float _deltaSum;

        void Start()
        {
            _performanceLogger = LogSmith.CreateLogger("Performance");
            _performanceLogger.Info("Performance monitoring started");
            _performanceLogger.Info($"Target FPS: {targetFps}, Low FPS threshold: {lowFpsThreshold}");
        }

        void Update()
        {
            if (!monitorFrameRate) return;

            _frameCount++;
            _deltaSum += Time.deltaTime;

            if (Time.time - _lastCheckTime >= checkInterval)
            {
                CheckPerformance();
                _lastCheckTime = Time.time;
                _frameCount = 0;
                _deltaSum = 0f;
            }
        }

        private void CheckPerformance()
        {
            float avgFrameTime = _deltaSum / _frameCount;
            float avgFps = 1f / avgFrameTime;

            if (avgFps < lowFpsThreshold)
            {
                _performanceLogger.Error($"Critical FPS: {avgFps:F1} (avg frame time: {avgFrameTime * 1000:F2}ms)");
            }
            else if (avgFps < targetFps)
            {
                _performanceLogger.Warn($"Below target FPS: {avgFps:F1} (target: {targetFps})");
            }
            else
            {
                _performanceLogger.Info($"Performance OK: {avgFps:F1} FPS (avg frame time: {avgFrameTime * 1000:F2}ms)");
            }
        }

        [ContextMenu("Log System Info")]
        public void LogSystemInfo()
        {
            _performanceLogger.Info("=== System Information ===");
            _performanceLogger.Info($"Platform: {Application.platform}");
            _performanceLogger.Info($"Unity Version: {Application.unityVersion}");
            _performanceLogger.Info($"System Memory: {SystemInfo.systemMemorySize} MB");
            _performanceLogger.Info($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
            _performanceLogger.Info($"Graphics Device: {SystemInfo.graphicsDeviceName}");
            _performanceLogger.Info($"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
        }

        [ContextMenu("Simulate Performance Spike")]
        public void SimulatePerformanceSpike()
        {
            _performanceLogger.Warn("Performance spike detected!");
            _performanceLogger.Warn("GC allocation spike: 50MB");
            _performanceLogger.Error("Frame time exceeded budget: 45ms (target: 16.67ms)");
        }
    }
}
