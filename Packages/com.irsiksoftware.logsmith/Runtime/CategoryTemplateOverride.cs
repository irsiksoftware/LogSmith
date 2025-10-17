using System;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Template override for a specific category.
    /// </summary>
    [Serializable]
    public class CategoryTemplateOverride
    {
        public string categoryName;
        public string textTemplate;
        public string jsonTemplate;

        public CategoryTemplateOverride()
        {
        }

        public CategoryTemplateOverride(string categoryName, string textTemplate = "", string jsonTemplate = "")
        {
            this.categoryName = categoryName;
            this.textTemplate = textTemplate;
            this.jsonTemplate = jsonTemplate;
        }
    }
}
