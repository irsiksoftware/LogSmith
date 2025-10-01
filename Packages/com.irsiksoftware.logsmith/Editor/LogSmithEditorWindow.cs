using UnityEditor;
using UnityEngine;
using System;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Main Editor Window for configuring LogSmith categories, sinks, and templates.
    /// </summary>
    public class LogSmithEditorWindow : EditorWindow
    {
        private enum Tab
        {
            Categories,
            Sinks,
            Templates
        }

        private Tab _currentTab = Tab.Categories;
        private LoggingSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;

        // Tab drawers
        private CategoriesTab _categoriesTab;
        private SinksTab _sinksTab;
        private TemplatesTab _templatesTab;

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
            _categoriesTab = new CategoriesTab();
            _sinksTab = new SinksTab();
            _templatesTab = new TemplatesTab();
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

            if (GUILayout.Toggle(_currentTab == Tab.Categories, "Categories", EditorStyles.toolbarButton))
                _currentTab = Tab.Categories;
            if (GUILayout.Toggle(_currentTab == Tab.Sinks, "Sinks", EditorStyles.toolbarButton))
                _currentTab = Tab.Sinks;
            if (GUILayout.Toggle(_currentTab == Tab.Templates, "Templates", EditorStyles.toolbarButton))
                _currentTab = Tab.Templates;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                _serializedSettings.Update();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCurrentTab()
        {
            switch (_currentTab)
            {
                case Tab.Categories:
                    _categoriesTab.Draw(_serializedSettings, _settings);
                    break;
                case Tab.Sinks:
                    _sinksTab.Draw(_serializedSettings, _settings);
                    break;
                case Tab.Templates:
                    _templatesTab.Draw(_serializedSettings, _settings);
                    break;
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
