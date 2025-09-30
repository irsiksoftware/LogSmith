using System;
using System.Collections.Generic;
using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Manages log categories and their metadata (minimum levels, colors, enabled state).
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
                _categories[category] = new CategoryMetadata(minimumLevel, _defaultColor, true);
            }
        }

        public void RegisterCategory(string category, CategoryMetadata metadata)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            lock (_lock)
            {
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
                }
                else
                {
                    _categories[category] = new CategoryMetadata(level, _defaultColor, true);
                }
            }
        }

        public LogLevel GetMinimumLevel(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultMinimumLevel;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata)
                    ? metadata.MinimumLevel
                    : _defaultMinimumLevel;
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
                }
                else
                {
                    _categories[category] = new CategoryMetadata(_defaultMinimumLevel, color, true);
                }
            }
        }

        public Color GetColor(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultColor;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata)
                    ? metadata.Color
                    : _defaultColor;
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
                }
                else
                {
                    _categories[category] = new CategoryMetadata(_defaultMinimumLevel, _defaultColor, enabled);
                }
            }
        }

        public bool IsEnabled(string category)
        {
            if (string.IsNullOrEmpty(category)) return true;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata)
                    ? metadata.Enabled
                    : true;
            }
        }

        public CategoryMetadata GetMetadata(string category)
        {
            if (string.IsNullOrEmpty(category)) return null;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var metadata)
                    ? metadata
                    : null;
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