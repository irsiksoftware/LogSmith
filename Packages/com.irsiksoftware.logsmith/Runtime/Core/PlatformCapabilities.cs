using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Default implementation of IPlatformCapabilities that detects platform capabilities at runtime.
    /// </summary>
    public class PlatformCapabilities : IPlatformCapabilities
    {
        /// <summary>
        /// Gets whether the current platform has a writable persistent data path.
        /// WebGL and Switch do not support file I/O to persistent data path.
        /// </summary>
        public bool HasWritablePersistentDataPath
        {
            get
            {
#if UNITY_WEBGL || UNITY_SWITCH
                return false;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Gets the name of the current platform for diagnostic purposes.
        /// </summary>
        public string PlatformName
        {
            get
            {
#if UNITY_WEBGL
                return "WebGL";
#elif UNITY_SWITCH
                return "Switch";
#elif UNITY_STANDALONE_WIN
                return "Windows";
#elif UNITY_STANDALONE_OSX
                return "macOS";
#elif UNITY_STANDALONE_LINUX
                return "Linux";
#elif UNITY_IOS
                return "iOS";
#elif UNITY_ANDROID
                return "Android";
#elif UNITY_PS4
                return "PlayStation 4";
#elif UNITY_PS5
                return "PlayStation 5";
#elif UNITY_XBOXONE
                return "Xbox One";
#elif UNITY_GAMECORE
                return "Xbox (Game Core)";
#else
                return Application.platform.ToString();
#endif
            }
        }
    }
}
