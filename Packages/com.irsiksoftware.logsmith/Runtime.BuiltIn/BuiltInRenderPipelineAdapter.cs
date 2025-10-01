namespace IrsikSoftware.LogSmith.BuiltIn
{
    /// <summary>
    /// Adapter for Built-in Render Pipeline.
    /// Provides render pipeline-specific logging capabilities when neither URP nor HDRP is present.
    /// </summary>
    public class BuiltInRenderPipelineAdapter
    {
        /// <summary>
        /// Gets whether the Built-in Render Pipeline is active.
        /// Returns true only when URP and HDRP are not available.
        /// </summary>
        public static bool IsActive
        {
            get
            {
#if !LOGSMITH_URP_AVAILABLE && !LOGSMITH_HDRP_AVAILABLE
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Initializes the Built-in Render Pipeline adapter.
        /// </summary>
        public void Initialize()
        {
            // Future implementation: register built-in RP-specific sinks or hooks
        }
    }
}
