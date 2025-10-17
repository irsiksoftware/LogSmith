using UnityEngine;

namespace IrsikSoftware.LogSmith
{
    /// <summary>
    /// Interface for rendering debug shapes and primitives on-screen.
    /// Implemented by render pipeline-specific adapters (Built-in, URP, HDRP).
    /// </summary>
    public interface IVisualDebugRenderer
    {
        /// <summary>
        /// Gets whether this renderer is currently enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Initializes the renderer with the specified camera.
        /// </summary>
        /// <param name="camera">The camera to render debug shapes on.</param>
        void Initialize(Camera camera);

        /// <summary>
        /// Cleans up resources and removes rendering hooks.
        /// </summary>
        void Cleanup();

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="start">Start position in world space.</param>
        /// <param name="end">End position in world space.</param>
        /// <param name="color">Line color.</param>
        /// <param name="duration">How long the line should persist (0 = single frame).</param>
        void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0f);

        /// <summary>
        /// Draws a filled quad (rectangle) in screen space.
        /// </summary>
        /// <param name="center">Center position in world space.</param>
        /// <param name="size">Size of the quad.</param>
        /// <param name="color">Quad color.</param>
        /// <param name="duration">How long the quad should persist (0 = single frame).</param>
        void DrawQuad(Vector3 center, Vector2 size, Color color, float duration = 0f);

        /// <summary>
        /// Clears all debug shapes.
        /// </summary>
        void Clear();
    }
}
