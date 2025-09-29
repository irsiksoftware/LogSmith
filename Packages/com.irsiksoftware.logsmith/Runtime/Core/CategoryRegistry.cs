using System;
using System.Collections.Generic;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Minimal category registry managing log categories and their minimum levels.
    /// </summary>
    public class CategoryRegistry : ICategoryRegistry
    {
        private readonly Dictionary<string, LogLevel> _categories = new Dictionary<string, LogLevel>();
        private readonly object _lock = new object();
        private readonly LogLevel _defaultMinimumLevel = LogLevel.Info;

        public void RegisterCategory(string category, LogLevel minimumLevel)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                _categories[category] = minimumLevel;
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
                if (_categories.TryGetValue(oldName, out var level))
                {
                    _categories.Remove(oldName);
                    _categories[newName] = level;
                }
            }
        }

        public void SetMinimumLevel(string category, LogLevel level)
        {
            if (string.IsNullOrEmpty(category)) throw new ArgumentNullException(nameof(category));

            lock (_lock)
            {
                if (_categories.ContainsKey(category))
                {
                    _categories[category] = level;
                }
                else
                {
                    _categories.Add(category, level);
                }
            }
        }

        public LogLevel GetMinimumLevel(string category)
        {
            if (string.IsNullOrEmpty(category)) return _defaultMinimumLevel;

            lock (_lock)
            {
                return _categories.TryGetValue(category, out var level) ? level : _defaultMinimumLevel;
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