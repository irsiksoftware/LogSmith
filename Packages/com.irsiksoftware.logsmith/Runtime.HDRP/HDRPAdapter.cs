namespace IrsikSoftware.LogSmith.HDRP
{
    /// <summary>
    /// Adapter for High Definition Render Pipeline (HDRP).
    /// Provides HDRP-specific logging capabilities when the HDRP package is present.
    /// </summary>
    public class HDRPAdapter
    {
        /// <summary>
        /// Gets whether the High Definition Render Pipeline is available.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
#if LOGSMITH_HDRP_PRESENT
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Initializes the HDRP adapter.
        /// </summary>
        public void Initialize()
        {
            // Future implementation: register HDRP-specific sinks or hooks
        }
    }
}
