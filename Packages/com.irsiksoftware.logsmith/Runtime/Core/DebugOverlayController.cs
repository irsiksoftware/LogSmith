using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// In-game debug overlay displaying real-time log messages.
    /// Subscribes to ILogRouter and displays filtered logs with search and throttling.
    /// </summary>
    public class DebugOverlayController : MonoBehaviour
    {
        private const int MAX_LOG_BUFFER_SIZE = 500;
        private const float UPDATE_THROTTLE_SECONDS = 1f / 30f; // 30 Hz

        private ILogRouter _logRouter;
        private IDisposable _subscription;
        private CircularBuffer<LogMessage> _logBuffer;
        private List<LogMessage> _filteredLogs;

        // UI State
        private bool _isVisible = true;
        private Vector2 _scrollPosition;
        private string _searchText = "";
        private LogLevel _minDisplayLevel = LogLevel.Trace;
        private string _selectedCategory = "All";
        private HashSet<string> _availableCategories;
        private bool _autoScroll = true;

        // Throttling
        private float _lastUpdateTime;
        private bool _needsUpdate;

        // UI Layout
        private Rect _windowRect = new Rect(10, 10, 800, 400);
        private GUIStyle _logStyle;
        private GUIStyle _headerStyle;

        public void Initialize(ILogRouter logRouter)
        {
            if (_logRouter != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[DebugOverlay] Already initialized");
#endif
                return;
            }

            _logRouter = logRouter ?? throw new ArgumentNullException(nameof(logRouter));
            _logBuffer = new CircularBuffer<LogMessage>(MAX_LOG_BUFFER_SIZE);
            _filteredLogs = new List<LogMessage>();
            _availableCategories = new HashSet<string>();

            // Subscribe to log events
            _subscription = _logRouter.Subscribe(OnLogMessage);

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        private void OnLogMessage(LogMessage message)
        {
            _logBuffer.Add(message);
            _availableCategories.Add(message.Category);
            _needsUpdate = true;
        }

        private void Update()
        {
            // Toggle visibility with F1
            // Wrapped in try-catch to handle Input System package conflicts
            try
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    _isVisible = !_isVisible;
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore - happens when new Input System package is active
                // Users should use the new Input System actions if available
            }

            // Throttled update
            if (_needsUpdate && Time.realtimeSinceStartup - _lastUpdateTime >= UPDATE_THROTTLE_SECONDS)
            {
                UpdateFilteredLogs();
                _lastUpdateTime = Time.realtimeSinceStartup;
                _needsUpdate = false;
            }
        }

        private void UpdateFilteredLogs()
        {
            _filteredLogs.Clear();

            foreach (var log in _logBuffer.GetAll())
            {
                // Filter by level
                if (log.Level < _minDisplayLevel)
                    continue;

                // Filter by category
                if (_selectedCategory != "All" && log.Category != _selectedCategory)
                    continue;

                // Filter by search text
                if (!string.IsNullOrEmpty(_searchText) &&
                    !log.Message.Contains(_searchText, StringComparison.OrdinalIgnoreCase) &&
                    !log.Category.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    continue;

                _filteredLogs.Add(log);
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            InitializeStyles();

            _windowRect = GUILayout.Window(0, _windowRect, DrawOverlayWindow, "LogSmith Debug Overlay");
        }

        private void InitializeStyles()
        {
            if (_logStyle == null)
            {
                _logStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    wordWrap = false,
                    richText = true,
                    padding = new RectOffset(4, 4, 2, 2)
                };

                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
            }
        }

        private void DrawOverlayWindow(int windowId)
        {
            GUILayout.BeginVertical();

            // Header with controls
            DrawHeader();

            GUILayout.Space(5);

            // Log list
            DrawLogList();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            // Search
            GUILayout.Label("Search:", GUILayout.Width(60));
            string newSearch = GUILayout.TextField(_searchText, GUILayout.Width(150));
            if (newSearch != _searchText)
            {
                _searchText = newSearch;
                _needsUpdate = true;
            }

            GUILayout.Space(10);

            // Level filter
            GUILayout.Label("Min Level:", GUILayout.Width(70));
            LogLevel newLevel = (LogLevel)GUILayout.SelectionGrid(
                (int)_minDisplayLevel,
                Enum.GetNames(typeof(LogLevel)),
                6,
                GUILayout.Width(400)
            );
            if (newLevel != _minDisplayLevel)
            {
                _minDisplayLevel = newLevel;
                _needsUpdate = true;
            }

            GUILayout.FlexibleSpace();

            // Auto-scroll toggle
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", GUILayout.Width(100));

            // Clear button
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                _logBuffer.Clear();
                _needsUpdate = true;
            }

            GUILayout.EndHorizontal();

            // Category filter
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("Category:", GUILayout.Width(70));

            var categories = new List<string> { "All" };
            categories.AddRange(_availableCategories.OrderBy(c => c));

            int currentCategoryIndex = categories.IndexOf(_selectedCategory);
            if (currentCategoryIndex < 0) currentCategoryIndex = 0;

            int newCategoryIndex = GUILayout.SelectionGrid(
                currentCategoryIndex,
                categories.ToArray(),
                Mathf.Min(categories.Count, 8)
            );

            if (newCategoryIndex != currentCategoryIndex)
            {
                _selectedCategory = categories[newCategoryIndex];
                _needsUpdate = true;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLogList()
        {
            GUILayout.Label($"Logs: {_filteredLogs.Count} / {_logBuffer.Count}", _headerStyle);

            if (_autoScroll)
            {
                _scrollPosition.y = float.MaxValue;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            foreach (var log in _filteredLogs)
            {
                DrawLogEntry(log);
            }

            GUILayout.EndScrollView();
        }

        private void DrawLogEntry(LogMessage log)
        {
            string colorCode = GetColorCode(log.Level);
            string timeStamp = log.Timestamp.ToString("HH:mm:ss.fff");
            string formattedLog = $"<color={colorCode}>[{timeStamp}] [{log.Level}] [{log.Category}]</color> {log.Message}";

            GUILayout.Label(formattedLog, _logStyle);
        }

        private string GetColorCode(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => "#888888",
                LogLevel.Debug => "#00BFFF",
                LogLevel.Info => "#00FF00",
                LogLevel.Warn => "#FFFF00",
                LogLevel.Error => "#FF6347",
                LogLevel.Critical => "#FF0000",
                _ => "#FFFFFF"
            };
        }
    }

    /// <summary>
    /// Circular buffer with fixed capacity for efficient log storage.
    /// </summary>
    public class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _start;
        private int _count;
        private readonly int _capacity;
        private readonly object _lock = new object();

        public int Count
        {
            get { lock (_lock) return _count; }
        }

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            lock (_lock)
            {
                int index = (_start + _count) % _capacity;
                _buffer[index] = item;

                if (_count < _capacity)
                {
                    _count++;
                }
                else
                {
                    _start = (_start + 1) % _capacity;
                }
            }
        }

        public List<T> GetAll()
        {
            lock (_lock)
            {
                var result = new List<T>(_count);
                for (int i = 0; i < _count; i++)
                {
                    int index = (_start + i) % _capacity;
                    result.Add(_buffer[index]);
                }
                return result;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _start = 0;
                _count = 0;
            }
        }
    }
}
