namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Provides information about platform capabilities for feature detection.
    /// Used to gracefully disable features that aren't supported on specific platforms.
    /// </summary>
    public interface IPlatformCapabilities
    {
        /// <summary>
        /// Gets whether the current platform has a writable persistent data path.
        /// Returns false for platforms like WebGL and Nintendo Switch where file I/O is restricted.
        /// </summary>
        bool HasWritablePersistentDataPath { get; }

        /// <summary>
        /// Gets the name of the current platform for diagnostic purposes.
        /// </summary>
        string PlatformName { get; }
    }
}
