namespace IrsikSoftware.LogSmith.URP
{
    /// <summary>
    /// Adapter for Universal Render Pipeline (URP).
    /// Provides URP-specific logging capabilities when the URP package is present.
    /// </summary>
    public class URPAdapter
    {
        /// <summary>
        /// Gets whether the Universal Render Pipeline is available.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
#if LOGSMITH_URP_PRESENT
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Initializes the URP adapter.
        /// </summary>
        public void Initialize()
        {
            // Future implementation: register URP-specific sinks or hooks
        }
    }
}
