using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Dispatches actions to Unity's main thread with bounded queue and drop policy.
    /// Thread-safe singleton for routing background thread callbacks to the main thread.
    /// </summary>
    internal class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;
        private static readonly object _lock = new object();

        private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private const int MaxQueueSize = 1000; // Bounded queue to prevent memory issues
        private int _droppedMessageCount = 0;

        /// <summary>
        /// Gets the singleton instance, creating it if necessary.
        /// </summary>
        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // Create a new GameObject for the dispatcher
                            var go = new GameObject("[LogSmith] MainThreadDispatcher");
                            _instance = go.AddComponent<MainThreadDispatcher>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Enqueues an action to be executed on the main thread.
        /// If the queue is full, the action is dropped and a warning is logged.
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        public void Enqueue(Action action)
        {
            if (action == null) return;

            // Check queue size to prevent unbounded growth
            if (_executionQueue.Count >= MaxQueueSize)
            {
                _droppedMessageCount++;
                if (_droppedMessageCount % 100 == 1) // Log every 100 drops to avoid spam
                {
                    Debug.LogWarning($"[LogSmith] Main thread queue full ({MaxQueueSize}), dropping messages. " +
                                   $"Total dropped: {_droppedMessageCount}. Consider reducing logging frequency.");
                }
                return;
            }

            _executionQueue.Enqueue(action);
        }

        /// <summary>
        /// Unity Update loop - process queued actions on the main thread.
        /// </summary>
        private void Update()
        {
            // Process all queued actions (up to a reasonable limit per frame to avoid spikes)
            int processedCount = 0;
            const int maxActionsPerFrame = 100;

            while (processedCount < maxActionsPerFrame && _executionQueue.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LogSmith] Main thread action failed: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
