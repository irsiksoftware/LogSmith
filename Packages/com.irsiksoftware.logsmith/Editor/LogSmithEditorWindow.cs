using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Main editor window for LogSmith configuration.
    /// Accessible via Window/LogSmith/Settings
    /// </summary>
    public class LogSmithEditorWindow : EditorWindow
    {
        private LoggingSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Categories", "Sinks", "Templates" };

        // Categories tab state
        private int _categoryColorIndex = 0;

        // Templates tab state
        private string _previewCategory = "Default";
        private LogLevel _previewLevel = LogLevel.Info;
        private string _previewMessage = "This is a preview message";

        [MenuItem("Window/LogSmith/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<LogSmithEditorWindow>("LogSmith Settings");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Try to find existing settings asset
            var guids = AssetDatabase.FindAssets("t:LoggingSettings");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<LoggingSettings>(path);
            }

            // Create new settings if none found
            if (_settings == null)
            {
                _settings = CreateInstance<LoggingSettings>();
                _settings.categories.Add(new CategoryDefinition("Default", Color.white, LogLevel.Debug, true));
            }

            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        private void OnGUI()
        {
            if (_settings == null || _serializedSettings == null)
            {
                EditorGUILayout.HelpBox("No LoggingSettings asset found. Create one via: Assets > Create > LogSmith > Logging Settings", MessageType.Warning);
                if (GUILayout.Button("Create New Settings Asset"))
                {
                    CreateNewSettingsAsset();
                }
                return;
            }

            _serializedSettings.Update();

            // Tab selection
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            EditorGUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0:
                    DrawCategoriesTab();
                    break;
                case 1:
                    DrawSinksTab();
                    break;
                case 2:
                    DrawTemplatesTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            _serializedSettings.ApplyModifiedProperties();

            // Save button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Save Settings", GUILayout.Height(30)))
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Settings Saved", "LogSmith settings have been saved successfully.", "OK");
            }
        }

        private void DrawCategoriesTab()
        {
            EditorGUILayout.LabelField("Log Categories", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure log categories with individual colors, minimum levels, and enable/disable states.", MessageType.Info);

            EditorGUILayout.Space(5);

            var categoriesProp = _serializedSettings.FindProperty("categories");

            // Draw existing categories
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var categoryProp = categoriesProp.GetArrayElementAtIndex(i);
                DrawCategory(categoryProp, i, categoriesProp);
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(10);

            // Add new category section
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+ Add Category", GUILayout.Width(150), GUILayout.Height(25)))
            {
                Undo.RecordObject(_settings, "Add Category");
                var colors = LoggingSettings.GetPredefinedColors();
                var newCategory = new CategoryDefinition(
                    $"Category{categoriesProp.arraySize + 1}",
                    colors[_categoryColorIndex % colors.Length],
                    LogLevel.Debug,
                    true
                );
                _settings.categories.Add(newCategory);
                _categoryColorIndex++;
                _serializedSettings.Update();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategory(SerializedProperty categoryProp, int index, SerializedProperty categoriesProp)
        {
            var nameProp = categoryProp.FindPropertyRelative("name");
            var colorProp = categoryProp.FindPropertyRelative("color");
            var levelProp = categoryProp.FindPropertyRelative("minimumLevel");
            var enabledProp = categoryProp.FindPropertyRelative("enabled");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            // Enabled toggle
            EditorGUI.BeginChangeCheck();
            var enabled = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Toggle Category");
                enabledProp.boolValue = enabled;
            }

            // Category name
            EditorGUI.BeginChangeCheck();
            var categoryName = EditorGUILayout.TextField(nameProp.stringValue, GUILayout.MinWidth(150));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Rename Category");
                // Validate unique name
                if (!_settings.categories.Any(c => c != _settings.categories[index] && c.name == categoryName))
                {
                    nameProp.stringValue = categoryName;
                }
                else
                {
                    EditorUtility.DisplayDialog("Duplicate Name", "A category with this name already exists.", "OK");
                }
            }

            // Color picker
            EditorGUI.BeginChangeCheck();
            var color = EditorGUILayout.ColorField(GUIContent.none, colorProp.colorValue, false, false, false, GUILayout.Width(50));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change Category Color");
                colorProp.colorValue = color;
            }

            // Minimum level
            EditorGUI.BeginChangeCheck();
            var level = (LogLevel)EditorGUILayout.EnumPopup(GUIContent.none, (LogLevel)levelProp.intValue, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change Category Level");
                levelProp.intValue = (int)level;
            }

            GUILayout.FlexibleSpace();

            // Delete button
            if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("Delete Category", $"Are you sure you want to delete '{nameProp.stringValue}'?", "Delete", "Cancel"))
                {
                    Undo.RecordObject(_settings, "Delete Category");
                    categoriesProp.DeleteArrayElementAtIndex(index);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSinksTab()
        {
            EditorGUILayout.LabelField("Output Sinks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure where log messages are written (console, file, etc.).", MessageType.Info);

            EditorGUILayout.Space(10);

            // Console Sink
            EditorGUILayout.LabelField("Console Sink", EditorStyles.boldLabel);
            var consoleEnabledProp = _serializedSettings.FindProperty("enableConsoleSink");
            EditorGUI.BeginChangeCheck();
            var consoleEnabled = EditorGUILayout.Toggle("Enable Console Output", consoleEnabledProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Toggle Console Sink");
                consoleEnabledProp.boolValue = consoleEnabled;
            }

            EditorGUILayout.Space(15);

            // File Sink
            EditorGUILayout.LabelField("File Sink", EditorStyles.boldLabel);
            var fileEnabledProp = _serializedSettings.FindProperty("enableFileSink");
            EditorGUI.BeginChangeCheck();
            var fileEnabled = EditorGUILayout.Toggle("Enable File Output", fileEnabledProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Toggle File Sink");
                fileEnabledProp.boolValue = fileEnabled;
            }

            if (fileEnabledProp.boolValue)
            {
                EditorGUI.indentLevel++;

                // File path
                var filePathProp = _serializedSettings.FindProperty("logFilePath");
                EditorGUI.BeginChangeCheck();
                var filePath = EditorGUILayout.TextField("Log File Path", filePathProp.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_settings, "Change File Path");
                    filePathProp.stringValue = filePath;
                }

                EditorGUILayout.HelpBox($"Full path: {Application.persistentDataPath}/{filePath}", MessageType.None);

                // Output format
                var formatProp = _serializedSettings.FindProperty("fileOutputFormat");
                EditorGUI.BeginChangeCheck();
                var format = (MessageFormatMode)EditorGUILayout.EnumPopup("Output Format", (MessageFormatMode)formatProp.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_settings, "Change Output Format");
                    formatProp.intValue = (int)format;
                }

                // Rotation settings
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Log Rotation", EditorStyles.boldLabel);

                var maxSizeProp = _serializedSettings.FindProperty("maxFileSizeMB");
                EditorGUI.BeginChangeCheck();
                var maxSize = EditorGUILayout.IntSlider("Max File Size (MB)", maxSizeProp.intValue, 0, 100);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_settings, "Change Max File Size");
                    maxSizeProp.intValue = maxSize;
                }

                if (maxSizeProp.intValue == 0)
                {
                    EditorGUILayout.HelpBox("0 = No rotation (unlimited file size)", MessageType.Info);
                }

                var retainedProp = _serializedSettings.FindProperty("retainedFileCount");
                EditorGUI.BeginChangeCheck();
                var retained = EditorGUILayout.IntSlider("Retained Files", retainedProp.intValue, 1, 20);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_settings, "Change Retained File Count");
                    retainedProp.intValue = retained;
                }

                EditorGUI.indentLevel--;

                // Platform warnings
                #if UNITY_WEBGL
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Warning: File output is not supported on WebGL.", MessageType.Warning);
                #endif

                #if UNITY_SWITCH
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Warning: File output may have restrictions on Nintendo Switch.", MessageType.Warning);
                #endif
            }
        }

        private void DrawTemplatesTab()
        {
            EditorGUILayout.LabelField("Message Templates", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Configure message formatting templates with token replacements.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Token reference (collapsible)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Available Tokens", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("{timestamp} - Log timestamp", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("{level} - Log level (Debug, Info, Warning, etc.)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("{category} - Category name", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("{message} - Log message content", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Default templates
            EditorGUILayout.LabelField("Default Templates", EditorStyles.boldLabel);

            var textTemplateProp = _serializedSettings.FindProperty("defaultTextTemplate");
            EditorGUI.BeginChangeCheck();
            var textTemplate = EditorGUILayout.TextArea(textTemplateProp.stringValue, GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change Text Template");
                textTemplateProp.stringValue = textTemplate;
            }

            EditorGUILayout.Space(5);

            var jsonTemplateProp = _serializedSettings.FindProperty("defaultJsonTemplate");
            EditorGUI.BeginChangeCheck();
            var jsonTemplate = EditorGUILayout.TextArea(jsonTemplateProp.stringValue, GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change JSON Template");
                jsonTemplateProp.stringValue = jsonTemplate;
            }

            EditorGUILayout.Space(15);

            // Preview section
            EditorGUILayout.LabelField("Template Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category:", GUILayout.Width(70));
            _previewCategory = EditorGUILayout.TextField(_previewCategory);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level:", GUILayout.Width(70));
            _previewLevel = (LogLevel)EditorGUILayout.EnumPopup(_previewLevel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Message:", GUILayout.Width(70));
            _previewMessage = EditorGUILayout.TextField(_previewMessage);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Generate preview
            var previewText = GeneratePreview(textTemplateProp.stringValue);
            EditorGUILayout.LabelField("Preview Output:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(previewText, EditorStyles.textArea, GUILayout.Height(40));

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Per-category template overrides
            EditorGUILayout.LabelField("Category Template Overrides", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Override templates for specific categories. Leave blank to use default.", MessageType.Info);

            var overridesProp = _serializedSettings.FindProperty("templateOverrides");

            for (int i = 0; i < overridesProp.arraySize; i++)
            {
                var overrideProp = overridesProp.GetArrayElementAtIndex(i);
                DrawTemplateOverride(overrideProp, i, overridesProp);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("+ Add Template Override", GUILayout.Height(25)))
            {
                Undo.RecordObject(_settings, "Add Template Override");
                _settings.templateOverrides.Add(new CategoryTemplateOverride("NewCategory", "", ""));
                _serializedSettings.Update();
            }
        }

        private void DrawTemplateOverride(SerializedProperty overrideProp, int index, SerializedProperty overridesProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            var categoryNameProp = overrideProp.FindPropertyRelative("categoryName");
            EditorGUI.BeginChangeCheck();
            var categoryName = EditorGUILayout.TextField("Category:", categoryNameProp.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change Override Category");
                categoryNameProp.stringValue = categoryName;
            }

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                Undo.RecordObject(_settings, "Remove Template Override");
                overridesProp.DeleteArrayElementAtIndex(index);
                return;
            }
            EditorGUILayout.EndHorizontal();

            var textTemplateProp = overrideProp.FindPropertyRelative("textTemplate");
            EditorGUI.BeginChangeCheck();
            var textTemplate = EditorGUILayout.TextArea(textTemplateProp.stringValue, GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Change Override Text Template");
                textTemplateProp.stringValue = textTemplate;
            }

            EditorGUILayout.EndVertical();
        }

        private string GeneratePreview(string template)
        {
            if (string.IsNullOrEmpty(template))
                return "[Empty template]";

            var preview = template
                .Replace("{timestamp}", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"))
                .Replace("{level}", _previewLevel.ToString().ToUpper())
                .Replace("{category}", _previewCategory)
                .Replace("{message}", _previewMessage);

            return preview;
        }

        private void CreateNewSettingsAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create LoggingSettings Asset",
                "LoggingSettings",
                "asset",
                "Choose where to save the LoggingSettings asset");

            if (!string.IsNullOrEmpty(path))
            {
                _settings = CreateInstance<LoggingSettings>();
                _settings.categories.Add(new CategoryDefinition("Default", Color.white, LogLevel.Debug, true));
                AssetDatabase.CreateAsset(_settings, path);
                AssetDatabase.SaveAssets();
                _serializedSettings = new SerializedObject(_settings);
                EditorUtility.DisplayDialog("Settings Created", "LoggingSettings asset created successfully.", "OK");
            }
        }
    }
}
