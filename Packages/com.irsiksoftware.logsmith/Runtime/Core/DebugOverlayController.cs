using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// In-game debug overlay displaying real-time log messages.
    /// Subscribes to ILogRouter and displays filtered logs with search and throttling.
    /// Provides runtime category management controls for changing log levels and enabling/disabling categories.
    /// </summary>
    public class DebugOverlayController : MonoBehaviour
    {
        private const int MAX_LOG_BUFFER_SIZE = 500;
        private const float UPDATE_THROTTLE_SECONDS = 1f / 30f; // 30 Hz
        private const float CATEGORY_PANEL_WIDTH = 300f;

        private ILogRouter _logRouter;
        private ICategoryRegistry _categoryRegistry;
        private IDisposable _subscription;
        private CircularBuffer<LogMessage> _logBuffer;
        private List<LogMessage> _filteredLogs;

        // UI State
        private bool _isVisible = true;
        private Vector2 _scrollPosition;
        private Vector2 _categoryScrollPosition;
        private string _searchText = "";
        private LogLevel _minDisplayLevel = LogLevel.Trace;
        private string _selectedCategory = "All";
        private HashSet<string> _availableCategories;
        private bool _autoScroll = true;
        private bool _isCategoryManagementPanelVisible;

        // Throttling
        private float _lastUpdateTime;
        private bool _needsUpdate;

        // UI Layout
        private Rect _windowRect = new Rect(10, 10, 800, 400);
        private Rect _categoryPanelRect = new Rect(820, 10, CATEGORY_PANEL_WIDTH, 400);
        private GUIStyle _logStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _categoryHeaderStyle;

        /// <summary>
        /// Gets whether the category management panel is currently visible.
        /// </summary>
        public bool IsCategoryManagementPanelVisible => _isCategoryManagementPanelVisible;

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

        /// <summary>
        /// Initializes the category registry for runtime category management.
        /// </summary>
        /// <param name="categoryRegistry">The category registry to use for managing categories.</param>
        public void InitializeCategoryRegistry(ICategoryRegistry categoryRegistry)
        {
            _categoryRegistry = categoryRegistry ?? throw new ArgumentNullException(nameof(categoryRegistry));
        }

        /// <summary>
        /// Sets the minimum log level for a category at the registry level.
        /// </summary>
        /// <param name="category">The category name.</param>
        /// <param name="level">The minimum log level.</param>
        public void SetCategoryMinimumLevel(string category, LogLevel level)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (_categoryRegistry == null) return;

            _categoryRegistry.SetMinimumLevel(category, level);
        }

        /// <summary>
        /// Enables or disables a category at the registry level.
        /// </summary>
        /// <param name="category">The category name.</param>
        /// <param name="enabled">Whether the category is enabled.</param>
        public void SetCategoryEnabled(string category, bool enabled)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (_categoryRegistry == null) return;

            // Auto-register category if not exists
            if (!_categoryRegistry.HasCategory(category))
            {
                _categoryRegistry.RegisterCategory(category, LogLevel.Info);
            }

            _categoryRegistry.SetEnabled(category, enabled);
        }

        /// <summary>
        /// Gets the metadata for a category.
        /// </summary>
        /// <param name="category">The category name.</param>
        /// <returns>The category metadata.</returns>
        public CategoryMetadata GetCategoryMetadata(string category)
        {
            if (_categoryRegistry == null)
            {
                return new CategoryMetadata
                {
                    Name = category,
                    MinimumLevel = LogLevel.Info,
                    Enabled = true,
                    Color = Color.white
                };
            }

            return _categoryRegistry.GetMetadata(category);
        }

        /// <summary>
        /// Gets all categories currently being managed (visible in overlay).
        /// </summary>
        /// <returns>A read-only list of category names.</returns>
        public IReadOnlyList<string> GetManagedCategories()
        {
            return _availableCategories?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Toggles the visibility of the category management panel.
        /// </summary>
        public void ToggleCategoryManagementPanel()
        {
            _isCategoryManagementPanelVisible = !_isCategoryManagementPanelVisible;
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

            // Draw category management panel if visible
            if (_isCategoryManagementPanelVisible && _categoryRegistry != null)
            {
                _categoryPanelRect = GUILayout.Window(1, _categoryPanelRect, DrawCategoryManagementPanel, "Category Management");
            }
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

                _categoryHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.8f, 0.8f, 1f) }
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

            // Category management toggle button (only show if registry is available)
            if (_categoryRegistry != null)
            {
                string buttonText = _isCategoryManagementPanelVisible ? "Hide Categories" : "Manage Categories";
                if (GUILayout.Button(buttonText, GUILayout.Width(120)))
                {
                    ToggleCategoryManagementPanel();
                }
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

        private void DrawCategoryManagementPanel(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Runtime Category Settings", _categoryHeaderStyle);
            GUILayout.Space(5);

            _categoryScrollPosition = GUILayout.BeginScrollView(_categoryScrollPosition, GUILayout.ExpandHeight(true));

            // Get all categories (from overlay's seen categories)
            var categories = _availableCategories?.OrderBy(c => c).ToList() ?? new List<string>();

            if (categories.Count == 0)
            {
                GUILayout.Label("No categories detected yet.", _logStyle);
            }
            else
            {
                foreach (var category in categories)
                {
                    DrawCategoryControls(category);
                    GUILayout.Space(2);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, _categoryPanelRect.width, 20));
        }

        private void DrawCategoryControls(string category)
        {
            var metadata = _categoryRegistry.GetMetadata(category);

            GUILayout.BeginVertical(GUI.skin.box);

            // Category name row with enabled toggle
            GUILayout.BeginHorizontal();

            bool newEnabled = GUILayout.Toggle(metadata.Enabled, "", GUILayout.Width(20));
            if (newEnabled != metadata.Enabled)
            {
                SetCategoryEnabled(category, newEnabled);
            }

            GUILayout.Label(category, _headerStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Minimum level selector
            GUILayout.BeginHorizontal();
            GUILayout.Label("Min Level:", GUILayout.Width(60));

            var levelNames = Enum.GetNames(typeof(LogLevel));
            int currentLevelIndex = (int)metadata.MinimumLevel;

            int newLevelIndex = GUILayout.SelectionGrid(
                currentLevelIndex,
                levelNames,
                levelNames.Length,
                GUILayout.Height(18)
            );

            if (newLevelIndex != currentLevelIndex)
            {
                SetCategoryMinimumLevel(category, (LogLevel)newLevelIndex);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
