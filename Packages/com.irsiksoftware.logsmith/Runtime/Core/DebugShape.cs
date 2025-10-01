using UnityEngine;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Represents a debug shape to be rendered.
    /// </summary>
    public struct DebugShape
    {
        public DebugShapeType Type;
        public Vector3 Start;
        public Vector3 End;
        public Vector2 Size;
        public Color Color;
        public float ExpiryTime;

        public bool IsExpired(float currentTime)
        {
            return ExpiryTime > 0f && currentTime >= ExpiryTime;
        }
    }

    /// <summary>
    /// Types of debug shapes that can be rendered.
    /// </summary>
    public enum DebugShapeType
    {
        Line,
        Quad
    }
}
