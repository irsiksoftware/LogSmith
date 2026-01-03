namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Formats log messages using templates with token substitution.
    /// </summary>
    public interface IMessageTemplateEngine
    {
        /// <summary>
        /// Formats a log message using the specified template format.
        /// </summary>
        string Format(LogMessage message, MessageFormat format);

        /// <summary>
        /// Sets the template for a specific category.
        /// </summary>
        void SetCategoryTemplate(string category, string template);

        /// <summary>
        /// Gets the template for a specific category, or the default if not set.
        /// </summary>
        string GetCategoryTemplate(string category);
    }

    /// <summary>
    /// Output format for formatted messages.
    /// </summary>
    public enum MessageFormat
    {
        /// <summary>
        /// Human-readable text format.
        /// </summary>
        Text,

        /// <summary>
        /// Structured JSON format.
        /// </summary>
        Json
    }
}
