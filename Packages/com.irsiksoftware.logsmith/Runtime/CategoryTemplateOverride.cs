using System;
using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Template override for a specific category.
    /// </summary>
    [Serializable]
    public class CategoryTemplateOverride
    {
        [Tooltip("Category name to apply the template to")]
        public string categoryName;

        [Tooltip("Custom template for this category")]
        [TextArea(2, 4)]
        public string template;

        [Tooltip("Whether to use JSON format for this category")]
        public bool useJsonFormat;

        public CategoryTemplateOverride()
        {
        }

        public CategoryTemplateOverride(string categoryName, string template = "", bool useJsonFormat = false)
        {
            this.categoryName = categoryName;
            this.template = template;
            this.useJsonFormat = useJsonFormat;
        }
    }
}
