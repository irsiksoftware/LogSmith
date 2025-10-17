using System;
using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Runtime category registry managing log categories and their metadata (color, enabled state, minimum levels).
    /// </summary>
    public class CategoryRegistry : ICategoryRegistry
    {
        private readonly Dictionary<string, CategoryMetadata> _categories = new Dictionary<string, CategoryMetadata>();
        private readonly object _lock = new object();
        private readonly LogLevel _defaultMinimumLevel = LogLevel.Info;
        private readonly Color _defaultColor = Color.white;

        public void RegisterCategory(string category, LogLevel minimumLevel)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                _categories[category] = new CategoryMetadata
                {
                    Name = category,
                    MinimumLevel = minimumLevel,
                    Enabled = true,
                    Color = _defaultColor
                };
            }
        }

        public void RegisterCategory(string category, CategoryMetadata metadata)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                metadata.Name = category; // Ensure name is set correctly
                _categories[category] = metadata;
            }
        }

        public void UnregisterCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                _categories.Remove(category);
            }
        }

        public void RenameCategory(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName)) throw new ArgumentNullException(nameof(oldName));
            if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException(nameof(newName));

            lock (_lock)
            {
                if (_categories.TryGetValue(oldName, out var metadata))
                {
                    _categories.Remove(oldName);
                    metadata.Name = newName;
                    _categories[newName] = metadata;
                }
            }
        }

        public void SetMinimumLevel(string category, LogLevel level)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                if (_categories.TryGetValue(category, out var metadata))
                {
                    metadata.MinimumLevel = level;
                    _categories[category] = metadata;
                }
                else
                {
                    // Auto-register category if not exists
                    RegisterCategory(category, level);
                }
            }
        }

        public LogLevel GetMinimumLevel(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultMinimumLevel;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata) ? metadata.MinimumLevel : _defaultMinimumLevel;
            }
        }

        public void SetEnabled(string category, bool enabled)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                if (_categories.TryGetValue(category, out var metadata))
                {
                    metadata.Enabled = enabled;
                    _categories[category] = metadata;
                }
            }
        }

        public bool IsEnabled(string category)
        {
            if (string.IsNullOrEmpty(category)) return true; // Default categories are enabled

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata) ? metadata.Enabled : true;
            }
        }

        public void SetColor(string category, Color color)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                if (_categories.TryGetValue(category, out var metadata))
                {
                    metadata.Color = color;
                    _categories[category] = metadata;
                }
            }
        }

        public Color GetColor(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultColor;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata) ? metadata.Color : _defaultColor;
            }
        }

        public CategoryMetadata GetMetadata(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return new CategoryMetadata
                {
                    Name = category,
                    MinimumLevel = _defaultMinimumLevel,
                    Enabled = true,
                    Color = _defaultColor
                };
            }

            lock (_lock)
            {
                if (_categories.TryGetValue(category, out var metadata))
                {
                    return metadata;
                }

                return new CategoryMetadata
                {
                    Name = category,
                    MinimumLevel = _defaultMinimumLevel,
                    Enabled = true,
                    Color = _defaultColor
                };
            }
        }

        public IReadOnlyList<string> GetCategories()
        {
            lock (_lock)
            {
                return new List<string>(_categories.Keys);
            }
        }

        public bool HasCategory(string category)
        {
            if (string.IsNullOrEmpty(category)) return false;

            lock (_lock)
            {
                return _categories.ContainsKey(category);
            }
        }
    }
}