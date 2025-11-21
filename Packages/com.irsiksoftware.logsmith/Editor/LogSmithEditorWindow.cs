using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Main Editor Window for configuring LogSmith categories, sinks, and templates.
    /// </summary>
    public class LogSmithEditorWindow : EditorWindow
    {
        private LoggingSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;

        // Tab management
        private IEditorTab[] _tabs;
        private string[] _tabNames;
        private int _selectedTab;

        [MenuItem("Window/LogSmith/Settings", priority = 2000)]
        public static void ShowWindow()
        {
            var window = GetWindow<LogSmithEditorWindow>("LogSmith Settings");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // Try to find existing settings asset
            _settings = FindOrCreateSettings();
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }

            // Initialize tabs
            _tabs = new IEditorTab[]
            {
                new CategoriesTab(),
                new SinksTab(),
                new TemplatesTab(),
                new VisualDebugTab()
            };
            _tabNames = _tabs.Select(t => t.TabName).ToArray();

            // Activate the first tab
            if (_tabs.Length > 0)
            {
                _tabs[_selectedTab].OnTabEnabled();
            }
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                DrawNoSettingsGUI();
                return;
            }

            _serializedSettings.Update();

            // Draw toolbar
            DrawToolbar();

            EditorGUILayout.Space(5);

            // Draw active tab
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawCurrentTab();
            EditorGUILayout.EndScrollView();

            // Apply changes
            if (_serializedSettings.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settings);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Tab selector with lifecycle management
            int newTab = GUILayout.Toolbar(_selectedTab, _tabNames, EditorStyles.toolbarButton);
            if (newTab != _selectedTab)
            {
                _tabs[_selectedTab].OnTabDisabled();
                _selectedTab = newTab;
                _tabs[_selectedTab].OnTabEnabled();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _serializedSettings.Update();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurrentTab()
        {
            if (_tabs != null && _selectedTab >= 0 && _selectedTab < _tabs.Length)
            {
                _tabs[_selectedTab].Draw(_serializedSettings, _settings);
            }
        }

        private void DrawNoSettingsGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "No LoggingSettings asset found. Create one to configure LogSmith.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Create LoggingSettings Asset", GUILayout.Height(30)))
            {
                CreateSettingsAsset();
            }
        }

        private LoggingSettings FindOrCreateSettings()
        {
            // Try to find existing settings
            string[] guids = AssetDatabase.FindAssets("t:LoggingSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<LoggingSettings>(path);
            }

            return null;
        }

        private void CreateSettingsAsset()
        {
            _settings = LoggingSettings.CreateDefault();

            string path = EditorUtility.SaveFilePanelInProject(
                "Create LoggingSettings",
                "LoggingSettings",
                "asset",
                "Choose where to save the LoggingSettings asset"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(_settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                _serializedSettings = new SerializedObject(_settings);

                EditorUtility.DisplayDialog(
                    "Settings Created",
                    $"LoggingSettings asset created at:\n{path}",
                    "OK"
                );
            }
        }
    }
}
