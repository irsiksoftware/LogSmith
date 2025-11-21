using UnityEditor;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Editor
{
    /// <summary>
    /// Categories tab for the LogSmith Editor Window.
    /// Allows adding, removing, renaming categories and setting their properties.
    /// </summary>
    public class CategoriesTab : IEditorTab
    {
        public string TabName => "Categories";

        private string _newCategoryName = "";
        private int _categoryToRemove = -1;

        public void Draw(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Category Management", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Categories organize your logs. Each category can have its own color, minimum level, and enabled state.",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Add new category section
            DrawAddCategorySection(serializedSettings, settings);

            EditorGUILayout.Space(15);

            // Existing categories list
            DrawCategoriesList(serializedSettings, settings);
        }

        private void DrawAddCategorySection(SerializedObject serializedSettings, LoggingSettings settings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Category", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category Name:", GUILayout.Width(120));
            _newCategoryName = EditorGUILayout.TextField(_newCategoryName);

            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newCategoryName));
            if (GUILayout.Button("Add Category", GUILayout.Width(120)))
            {
                AddCategory(serializedSettings, settings, _newCategoryName.Trim());
                _newCategoryName = "";
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawCategoriesList(SerializedObject serializedSettings, LoggingSettings settings)
        {
            var categoriesProperty = serializedSettings.FindProperty("categories");

            if (categoriesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No categories defined. Add your first category above.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Registered Categories ({categoriesProperty.arraySize})", EditorStyles.boldLabel);

            _categoryToRemove = -1;

            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                var categoryProp = categoriesProperty.GetArrayElementAtIndex(i);
                DrawCategoryItem(i, categoryProp, serializedSettings);
            }

            // Handle removal after iteration
            if (_categoryToRemove >= 0)
            {
                Undo.RecordObject(settings, "Remove Category");
                categoriesProperty.DeleteArrayElementAtIndex(_categoryToRemove);
                serializedSettings.ApplyModifiedProperties();
            }
        }

        private void DrawCategoryItem(int index, SerializedProperty categoryProp, SerializedObject serializedSettings)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header row with name and remove button
            EditorGUILayout.BeginHorizontal();

            var nameProp = categoryProp.FindPropertyRelative("categoryName");
            var enabledProp = categoryProp.FindPropertyRelative("enabled");

            // Enabled toggle
            enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue, GUILayout.Width(20));

            // Category name (editable)
            EditorGUILayout.LabelField("Name:", GUILayout.Width(50));
            string oldName = nameProp.stringValue;
            string newName = EditorGUILayout.TextField(oldName, GUILayout.MinWidth(150));

            if (newName != oldName && !string.IsNullOrWhiteSpace(newName))
            {
                Undo.RecordObject(serializedSettings.targetObject, "Rename Category");
                nameProp.stringValue = newName.Trim();
            }

            GUILayout.FlexibleSpace();

            // Remove button
            if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog(
                    "Remove Category",
                    $"Are you sure you want to remove category '{nameProp.stringValue}'?",
                    "Remove", "Cancel"))
                {
                    _categoryToRemove = index;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Properties row
            EditorGUILayout.BeginHorizontal();

            // Color
            var colorProp = categoryProp.FindPropertyRelative("color");
            EditorGUILayout.LabelField("Color:", GUILayout.Width(50));
            colorProp.colorValue = EditorGUILayout.ColorField(colorProp.colorValue, GUILayout.Width(100));

            GUILayout.Space(20);

            // Minimum Level
            var minLevelProp = categoryProp.FindPropertyRelative("minimumLevel");
            EditorGUILayout.LabelField("Min Level:", GUILayout.Width(70));
            minLevelProp.enumValueIndex = (int)(LogLevel)EditorGUILayout.EnumPopup(
                (LogLevel)minLevelProp.enumValueIndex,
                GUILayout.Width(100)
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void AddCategory(SerializedObject serializedSettings, LoggingSettings settings, string categoryName)
        {
            // Check for duplicates
            var categoriesProperty = serializedSettings.FindProperty("categories");
            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                var existingName = categoriesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("categoryName").stringValue;
                if (existingName == categoryName)
                {
                    EditorUtility.DisplayDialog("Duplicate Category",
                        $"Category '{categoryName}' already exists.", "OK");
                    return;
                }
            }

            Undo.RecordObject(settings, "Add Category");

            int newIndex = categoriesProperty.arraySize;
            categoriesProperty.InsertArrayElementAtIndex(newIndex);

            var newCategory = categoriesProperty.GetArrayElementAtIndex(newIndex);
            newCategory.FindPropertyRelative("categoryName").stringValue = categoryName;
            newCategory.FindPropertyRelative("color").colorValue = GetNextColor(categoriesProperty.arraySize - 1);
            newCategory.FindPropertyRelative("minimumLevel").enumValueIndex = (int)LogLevel.Debug;
            newCategory.FindPropertyRelative("enabled").boolValue = true;

            serializedSettings.ApplyModifiedProperties();
        }

        private Color GetNextColor(int index)
        {
            // Predefined pleasant colors for categories
            Color[] colors = new[]
            {
                new Color(0.3f, 0.7f, 1.0f),    // Light blue
                new Color(0.3f, 1.0f, 0.3f),    // Light green
                new Color(1.0f, 0.8f, 0.2f),    // Yellow
                new Color(1.0f, 0.4f, 0.4f),    // Light red
                new Color(0.8f, 0.4f, 1.0f),    // Purple
                new Color(1.0f, 0.6f, 0.2f),    // Orange
                new Color(0.4f, 1.0f, 1.0f),    // Cyan
                new Color(1.0f, 0.4f, 0.8f),    // Pink
            };

            return colors[index % colors.Length];
        }
    }
}
