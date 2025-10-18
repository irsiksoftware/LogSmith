using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// In-Editor live log console that mirrors runtime log stream with filters, search, and jump-to-source.
    /// </summary>
    public class LiveLogConsoleWindow : EditorWindow
    {
        private const int MAX_LOG_ENTRIES = 10000;
        private const int SCROLL_BUFFER = 100;

        private List<LogEntry> _logEntries = new List<LogEntry>();
        private List<LogEntry> _filteredEntries = new List<LogEntry>();
        private Vector2 _scrollPosition;
        private IDisposable _logSubscription;
        private bool _autoScroll = true;
        private string _searchText = "";
        private LogLevel _minLogLevel = LogLevel.Trace;
        private string _categoryFilter = "";
        private bool _isPaused = false;
        private List<LogEntry> _pausedBuffer = new List<LogEntry>();

        // UI state
        private GUIStyle _logEntryStyle;
        private GUIStyle _selectedLogEntryStyle;
        private int _selectedIndex = -1;
        private bool _showTimestamps = true;
        private bool _showCategories = true;
        private bool _showThreadInfo = false;
        private Dictionary<LogLevel, bool> _levelToggles = new Dictionary<LogLevel, bool>();

        [MenuItem("Window/LogSmith/Live Log Console", priority = 2001)]
        public static void ShowWindow()
        {
            var window = GetWindow<LiveLogConsoleWindow>("LogSmith Console");
            window.minSize = new Vector2(800, 400);
            window.Show();
        }

        private void OnEnable()
        {
            InitializeLevelToggles();
            SubscribeToLogs();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            UnsubscribeFromLogs();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void InitializeLevelToggles()
        {
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                _levelToggles[level] = true;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SubscribeToLogs();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                UnsubscribeFromLogs();
            }
        }

        private void SubscribeToLogs()
        {
            UnsubscribeFromLogs();

            if (!Application.isPlaying) return;

            try
            {
                var router = LogSmith.GetRouter();
                if (router != null)
                {
                    _logSubscription = router.Subscribe(OnLogReceived);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LogSmith] Could not subscribe to log stream: {ex.Message}");
            }
        }

        private void UnsubscribeFromLogs()
        {
            _logSubscription?.Dispose();
            _logSubscription = null;
        }

        private void OnLogReceived(LogMessage message)
        {
            var entry = new LogEntry
            {
                Message = message.Message,
                Level = message.Level,
                Category = message.Category,
                Timestamp = message.Timestamp,
                ThreadId = message.ThreadId,
                ThreadName = message.ThreadName,
                CallerFilePath = message.CallerFilePath,
                CallerMemberName = message.CallerMemberName,
                CallerLineNumber = message.CallerLineNumber,
                StackTrace = message.StackTrace,
                Frame = message.Frame
            };

            if (_isPaused)
            {
                _pausedBuffer.Add(entry);
                return;
            }

            _logEntries.Add(entry);

            // Trim old entries if exceeding max
            if (_logEntries.Count > MAX_LOG_ENTRIES)
            {
                _logEntries.RemoveRange(0, _logEntries.Count - MAX_LOG_ENTRIES);
            }

            ApplyFilters();
            Repaint();
        }

        private void OnGUI()
        {
            InitializeStyles();

            DrawToolbar();
            DrawFilterBar();
            DrawLogList();
            DrawStatusBar();
        }

        private void InitializeStyles()
        {
            if (_logEntryStyle == null)
            {
                _logEntryStyle = new GUIStyle(EditorStyles.label)
                {
                    richText = true,
                    wordWrap = false,
                    padding = new RectOffset(5, 5, 2, 2)
                };

                _selectedLogEntryStyle = new GUIStyle(_logEntryStyle)
                {
                    normal = { background = MakeTexture(2, 2, new Color(0.24f, 0.48f, 0.9f, 0.4f)) }
                };
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                ClearLogs();
            }

            GUILayout.Space(5);

            // Pause/Resume button
            string pauseLabel = _isPaused ? "Resume" : "Pause";
            if (GUILayout.Button(pauseLabel, EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                TogglePause();
            }

            GUILayout.Space(5);

            // Auto-scroll toggle
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", EditorStyles.toolbarButton, GUILayout.Width(80));

            GUILayout.Space(10);

            // Display options
            _showTimestamps = GUILayout.Toggle(_showTimestamps, "Time", EditorStyles.toolbarButton, GUILayout.Width(50));
            _showCategories = GUILayout.Toggle(_showCategories, "Category", EditorStyles.toolbarButton, GUILayout.Width(70));
            _showThreadInfo = GUILayout.Toggle(_showThreadInfo, "Thread", EditorStyles.toolbarButton, GUILayout.Width(60));

            GUILayout.FlexibleSpace();

            // Connection status
            bool isConnected = Application.isPlaying && _logSubscription != null;
            string statusText = isConnected ? "Connected" : "Disconnected";
            Color statusColor = isConnected ? Color.green : Color.gray;

            GUI.color = statusColor;
            GUILayout.Label(statusText, EditorStyles.toolbarButton, GUILayout.Width(90));
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Search field
            GUILayout.Label("Search:", GUILayout.Width(50));
            string newSearch = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarTextField, GUILayout.Width(200));
            if (newSearch != _searchText)
            {
                _searchText = newSearch;
                ApplyFilters();
            }

            GUILayout.Space(10);

            // Category filter
            GUILayout.Label("Category:", GUILayout.Width(60));
            string newCategory = EditorGUILayout.TextField(_categoryFilter, EditorStyles.toolbarTextField, GUILayout.Width(150));
            if (newCategory != _categoryFilter)
            {
                _categoryFilter = newCategory;
                ApplyFilters();
            }

            GUILayout.Space(10);

            // Log level toggles
            GUILayout.Label("Levels:", GUILayout.Width(50));
            bool filterChanged = false;

            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                bool wasEnabled = _levelToggles[level];
                bool isEnabled = GUILayout.Toggle(wasEnabled, level.ToString(), EditorStyles.toolbarButton, GUILayout.Width(60));

                if (wasEnabled != isEnabled)
                {
                    _levelToggles[level] = isEnabled;
                    filterChanged = true;
                }
            }

            if (filterChanged)
            {
                ApplyFilters();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLogList()
        {
            var entries = _filteredEntries;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                bool isSelected = i == _selectedIndex;
                var style = isSelected ? _selectedLogEntryStyle : _logEntryStyle;

                EditorGUILayout.BeginHorizontal(style);

                // Build display text
                string displayText = BuildLogDisplayText(entry);

                // Handle selection
                if (GUILayout.Button(displayText, style, GUILayout.ExpandWidth(true)))
                {
                    _selectedIndex = i;
                    HandleLogSelection(entry);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Auto-scroll to bottom
            if (_autoScroll && entries.Count > 0 && Event.current.type == EventType.Repaint)
            {
                _scrollPosition.y = float.MaxValue;
            }
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label($"Total: {_logEntries.Count} | Filtered: {_filteredEntries.Count}", EditorStyles.miniLabel);

            if (_isPaused && _pausedBuffer.Count > 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Buffered: {_pausedBuffer.Count}", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private string BuildLogDisplayText(LogEntry entry)
        {
            var parts = new List<string>();

            if (_showTimestamps)
            {
                parts.Add($"<color=#888888>[{entry.Timestamp:HH:mm:ss.fff}]</color>");
            }

            // Level with color
            string levelColor = GetLogLevelColor(entry.Level);
            parts.Add($"<color={levelColor}>[{entry.Level}]</color>");

            if (_showCategories)
            {
                parts.Add($"<color=#aaaaff>[{entry.Category}]</color>");
            }

            if (_showThreadInfo && !string.IsNullOrEmpty(entry.ThreadName))
            {
                parts.Add($"<color=#ffaa88>[{entry.ThreadName}]</color>");
            }

            parts.Add(entry.Message);

            return string.Join(" ", parts);
        }

        private string GetLogLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace: return "#888888";
                case LogLevel.Debug: return "#00ff00";
                case LogLevel.Info: return "#00ccff";
                case LogLevel.Warn: return "#ffff00";
                case LogLevel.Error: return "#ff6600";
                case LogLevel.Critical: return "#ff0000";
                default: return "#ffffff";
            }
        }

        private void HandleLogSelection(LogEntry entry)
        {
            // Jump to source if available
            if (!string.IsNullOrEmpty(entry.CallerFilePath) && entry.CallerLineNumber > 0)
            {
                // Try to open the file at the specific line
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.CallerFilePath);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset, entry.CallerLineNumber);
                }
                else
                {
                    Debug.Log($"Source: {entry.CallerFilePath}:{entry.CallerLineNumber}");
                }
            }

            // Show stack trace if available
            if (!string.IsNullOrEmpty(entry.StackTrace))
            {
                Debug.Log($"Stack Trace:\n{entry.StackTrace}");
            }
        }

        private void ApplyFilters()
        {
            _filteredEntries = _logEntries.Where(entry =>
            {
                // Level filter
                if (!_levelToggles[entry.Level])
                    return false;

                // Category filter
                if (!string.IsNullOrEmpty(_categoryFilter) &&
                    !entry.Category.Contains(_categoryFilter, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Search filter
                if (!string.IsNullOrEmpty(_searchText) &&
                    !entry.Message.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }).ToList();
        }

        private void ClearLogs()
        {
            _logEntries.Clear();
            _filteredEntries.Clear();
            _pausedBuffer.Clear();
            _selectedIndex = -1;
            Repaint();
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;

            if (!_isPaused && _pausedBuffer.Count > 0)
            {
                // Resume and add buffered entries
                _logEntries.AddRange(_pausedBuffer);
                _pausedBuffer.Clear();

                // Trim if needed
                if (_logEntries.Count > MAX_LOG_ENTRIES)
                {
                    _logEntries.RemoveRange(0, _logEntries.Count - MAX_LOG_ENTRIES);
                }

                ApplyFilters();
            }
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private class LogEntry
        {
            public string Message;
            public LogLevel Level;
            public string Category;
            public DateTime Timestamp;
            public int ThreadId;
            public string ThreadName;
            public string CallerFilePath;
            public string CallerMemberName;
            public int CallerLineNumber;
            public string StackTrace;
            public int Frame;
        }
    }
}
