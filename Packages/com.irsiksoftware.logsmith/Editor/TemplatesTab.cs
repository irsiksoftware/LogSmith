using UnityEditor;
using UnityEngine;
using IrsikSoftware.LogSmith.Core;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Templates tab for the LogSmith Editor Window.
    /// Allows configuring default and per-category message templates with live preview.
    /// </summary>
    public class TemplatesTab : IEditorTab
    {
        public string TabName => "Templates";

        private IMessageTemplateEngine _templateEngine;
        private LogMessage _previewMessage;
        private bool _showTokenReference = true;

        public TemplatesTab()
        {
            _templateEngine = new MessageTemplateEngine();
            CreatePreviewMessage();
        }

        public void Draw(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Template Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure message templates for formatting log output. Use tokens like {timestamp}, {level}, {category}, {message}.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            // Token Reference (collapsible)
            DrawTokenReference();

            EditorGUILayout.Space(15);

            // Default Template
            DrawDefaultTemplate(serializedSettings);

            EditorGUILayout.Space(15);

            // Per-Category Templates
            DrawPerCategoryTemplates(serializedSettings, settings);
        }

        private void DrawTokenReference()
        {
            _showTokenReference = EditorGUILayout.Foldout(_showTokenReference, "Available Tokens", true, EditorStyles.foldoutHeader);

            if (_showTokenReference)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                DrawTokenRow("{timestamp}", "Current timestamp (supports format: {timestamp:yyyy-MM-dd HH:mm:ss})");
                DrawTokenRow("{level}", "Log level (Debug, Info, Warn, Error, Fatal)");
                DrawTokenRow("{category}", "Category name");
                DrawTokenRow("{message}", "Log message content");
                DrawTokenRow("{frame}", "Unity frame number");
                DrawTokenRow("{thread}", "Thread ID");
                DrawTokenRow("{file}", "Source file name");
                DrawTokenRow("{method}", "Source method name");
                DrawTokenRow("{memoryMB}", "Current memory usage in MB");
                DrawTokenRow("{stack}", "Stack trace (when available)");
                DrawTokenRow("{context}", "Contextual data (when provided)");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTokenRow(string token, string description)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(token, EditorStyles.boldLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDefaultTemplate(SerializedObject serializedSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Default Template", EditorStyles.boldLabel);

            var defaultTemplateProp = serializedSettings.FindProperty("defaultTextTemplate");

            EditorGUILayout.LabelField("Template:", EditorStyles.miniBoldLabel);
            string oldTemplate = defaultTemplateProp.stringValue;
            string newTemplate = EditorGUILayout.TextArea(oldTemplate, GUILayout.Height(60));

            if (newTemplate != oldTemplate)
            {
                defaultTemplateProp.stringValue = newTemplate;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(newTemplate))
            {
                EditorGUILayout.HelpBox("⚠ Template cannot be empty", MessageType.Error);
            }
            else if (!ValidateTemplate(newTemplate))
            {
                EditorGUILayout.HelpBox("⚠ Template contains malformed tokens", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            // Live Preview
            EditorGUILayout.LabelField("Live Preview:", EditorStyles.miniBoldLabel);
            DrawLivePreview(newTemplate, MessageFormat.Text);

            EditorGUILayout.EndVertical();
        }

        private void DrawPerCategoryTemplates(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Per-Category Template Overrides", EditorStyles.boldLabel);

            var categoriesProperty = serializedSettings.FindProperty("categories");
            var templateOverridesProp = serializedSettings.FindProperty("categoryTemplateOverrides");

            if (categoriesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No categories defined. Go to the Categories tab to add categories first.",
                    MessageType.Info
                );
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.HelpBox(
                "Override templates for specific categories. If not set, the default template is used.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Add override section
            DrawAddOverrideSection(serializedSettings, settings, categoriesProperty, templateOverridesProp);

            EditorGUILayout.Space(10);

            // Existing overrides
            DrawExistingOverrides(serializedSettings, templateOverridesProp);

            EditorGUILayout.EndVertical();
        }

        private void DrawAddOverrideSection(SerializedObject serializedSettings, LoggingSettings settings,
            SerializedProperty categoriesProperty, SerializedProperty templateOverridesProp)
        {
            EditorGUILayout.LabelField("Add Override for Category:", EditorStyles.miniBoldLabel);

            // Build category dropdown
            string[] categoryNames = new string[categoriesProperty.arraySize];
            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                categoryNames[i] = categoriesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("categoryName").stringValue;
            }

            EditorGUILayout.BeginHorizontal();
            int selectedIndex = EditorGUILayout.Popup("Category:", 0, categoryNames, GUILayout.Width(300));

            if (GUILayout.Button("Add Override", GUILayout.Width(120)))
            {
                string categoryName = categoryNames[selectedIndex];
                AddTemplateOverride(serializedSettings, settings, templateOverridesProp, categoryName);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawExistingOverrides(SerializedObject serializedSettings, SerializedProperty templateOverridesProp)
        {
            if (templateOverridesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No category-specific overrides configured.", MessageType.None);
                return;
            }

            EditorGUILayout.LabelField($"Active Overrides ({templateOverridesProp.arraySize}):", EditorStyles.miniBoldLabel);

            int indexToRemove = -1;

            for (int i = 0; i < templateOverridesProp.arraySize; i++)
            {
                var overrideProp = templateOverridesProp.GetArrayElementAtIndex(i);
                var categoryNameProp = overrideProp.FindPropertyRelative("categoryName");
                var templateProp = overrideProp.FindPropertyRelative("template");
                var useJsonProp = overrideProp.FindPropertyRelative("useJsonFormat");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Category: {categoryNameProp.stringValue}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(20)))
                {
                    indexToRemove = i;
                }
                EditorGUILayout.EndHorizontal();

                // Template
                EditorGUILayout.LabelField("Template:", EditorStyles.miniLabel);
                templateProp.stringValue = EditorGUILayout.TextArea(templateProp.stringValue, GUILayout.Height(50));

                // JSON toggle
                useJsonProp.boolValue = EditorGUILayout.Toggle("Use JSON Format", useJsonProp.boolValue);

                // Preview
                EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);
                var format = useJsonProp.boolValue ? MessageFormat.Json : MessageFormat.Text;
                DrawLivePreview(templateProp.stringValue, format);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            // Handle removal
            if (indexToRemove >= 0)
            {
                templateOverridesProp.DeleteArrayElementAtIndex(indexToRemove);
                serializedSettings.ApplyModifiedProperties();
            }
        }

        private void DrawLivePreview(string template, MessageFormat format)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                EditorGUILayout.HelpBox("(Empty template)", MessageType.None);
                return;
            }

            try
            {
                // Update engine with current template
                var tempEngine = new MessageTemplateEngine();
                tempEngine.SetDefaultTemplate(template);

                string preview = tempEngine.Format(_previewMessage, format);

                GUIStyle previewStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    padding = new RectOffset(8, 8, 8, 8)
                };

                EditorGUILayout.TextArea(preview, previewStyle, GUILayout.Height(format == MessageFormat.Json ? 80 : 40));
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox($"⚠ Preview error: {ex.Message}", MessageType.Error);
            }
        }

        private void AddTemplateOverride(SerializedObject serializedSettings, LoggingSettings settings,
            SerializedProperty templateOverridesProp, string categoryName)
        {
            // Check for duplicate
            for (int i = 0; i < templateOverridesProp.arraySize; i++)
            {
                var existingName = templateOverridesProp.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("categoryName").stringValue;
                if (existingName == categoryName)
                {
                    EditorUtility.DisplayDialog("Override Exists",
                        $"Template override for category '{categoryName}' already exists.", "OK");
                    return;
                }
            }

            Undo.RecordObject(settings, "Add Template Override");

            int newIndex = templateOverridesProp.arraySize;
            templateOverridesProp.InsertArrayElementAtIndex(newIndex);

            var newOverride = templateOverridesProp.GetArrayElementAtIndex(newIndex);
            newOverride.FindPropertyRelative("categoryName").stringValue = categoryName;
            newOverride.FindPropertyRelative("template").stringValue = settings.defaultTextTemplate;
            newOverride.FindPropertyRelative("useJsonFormat").boolValue = false;

            serializedSettings.ApplyModifiedProperties();
        }

        private bool ValidateTemplate(string template)
        {
            // Basic validation: check for properly closed braces
            int openCount = 0;
            foreach (char c in template)
            {
                if (c == '{') openCount++;
                else if (c == '}') openCount--;
                if (openCount < 0) return false;
            }
            return openCount == 0;
        }

        private void CreatePreviewMessage()
        {
            _previewMessage = new LogMessage
            {
                Level = LogLevel.Info,
                Category = "ExampleCategory",
                Message = "This is a preview message",
                Timestamp = System.DateTime.Now,
                Frame = 12345,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId
            };
        }
    }
}
