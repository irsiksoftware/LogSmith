# Built-in Render Pipeline Setup

LogSmith's visual debug renderer works seamlessly with Unity's Built-in Render Pipeline with zero additional configuration required.

## Quick Start

1. **Add the Demo Script**
   - Open the `BuiltInDemo.unity` scene from `Samples~/Built-in/`
   - Or add `BuiltInRPDemo.cs` component to any GameObject in your scene

2. **Assign a Camera**
   - The demo script will automatically find `Camera.main`
   - Alternatively, assign a camera manually in the Inspector

3. **Enable Visual Debug** (if needed)
   - Open **Tools > LogSmith Settings**
   - Under **Visual Debug** tab, ensure `Enable Visual Debug` is checked

4. **Press Play**
   - The overlay will appear showing log messages
   - Debug shapes will be rendered in the Scene and Game views

## How It Works

The Built-in RP adapter uses **`Camera.OnPostRender`** callbacks to inject visual debug rendering after the main camera render:

```csharp
private IRenderPipelineAdapterService _adapterService;
private IVisualDebugRenderer _renderer;

void Start()
{
    _adapterService = new RenderPipelineAdapterService();
    _adapterService.Initialize(targetCamera, enableShapes: true);
    _renderer = _adapterService.ActiveRenderer;
}

void Update()
{
    // Draw a line
    _renderer.DrawLine(startPos, endPos, Color.green);

    // Draw a quad
    _renderer.DrawQuad(center, rotation, size, color);
}
```

## Features

| Feature | Supported | Notes |
|---------|-----------|-------|
| **Overlay UI** | ✅ | Pipeline-agnostic, always available |
| **DrawLine** | ✅ | Single-pixel lines via GL commands |
| **DrawQuad** | ✅ | Textured or solid quads |
| **DrawCircle** | ✅ | Approximated with line segments |
| **Depth Testing** | ✅ | Respects depth buffer by default |
| **Alpha Blending** | ✅ | Transparent shapes supported |

## Configuration

### Enable/Disable Visual Debug

```csharp
// In LoggingSettings (ScriptableObject)
public bool enableVisualDebug = true;
```

Or via code:

```csharp
var settings = LogSmith.Resolve<LoggingSettings>();
settings.enableVisualDebug = true;
```

### Camera Assignment

The adapter automatically searches for `Camera.main`. To use a different camera:

```csharp
_adapterService.Initialize(myCustomCamera, enableShapes: true);
```

## Known Limitations

### 1. **Injection Order**
   - Visual debug renders **after** post-processing
   - Shapes appear on top of all camera effects
   - **Workaround**: If you need shapes to appear before post-processing, use a separate camera with a lower depth

### 2. **Multiple Cameras**
   - Only one camera per adapter instance
   - For multi-camera setups, create multiple adapter instances:

   ```csharp
   var adapter1 = new RenderPipelineAdapterService();
   adapter1.Initialize(camera1, true);

   var adapter2 = new RenderPipelineAdapterService();
   adapter2.Initialize(camera2, true);
   ```

### 3. **Performance**
   - Each `DrawLine`/`DrawQuad` call issues GL commands
   - Limit to < 500 shapes per frame for 60fps on mobile
   - Use batching for large numbers of primitives

### 4. **Shader Compatibility**
   - Uses Unity's built-in `Hidden/Internal-Colored` shader
   - Works in linear and gamma color space
   - No custom shader support currently

## Troubleshooting

### Shapes Don't Appear

**Check:**
1. `enableVisualDebug` is `true` in LoggingSettings
2. Camera is assigned and active
3. Shapes are within camera frustum and near/far planes
4. No compile errors in Console

**Verify Adapter Initialization:**
```csharp
if (_renderer == null)
{
    Debug.LogError("Visual debug renderer failed to initialize");
}
```

### Overlay UI Missing

**Check:**
1. `enableVisualDebug` is `true`
2. No UI canvas set to "Screen Space - Camera" covering the overlay
3. Check Console for initialization warnings

### Performance Issues

**Solutions:**
- Reduce shape count (use `Time.frameCount % N` to throttle)
- Disable shapes when not in development builds:

  ```csharp
  #if DEVELOPMENT_BUILD || UNITY_EDITOR
      _renderer.DrawLine(start, end, color);
  #endif
  ```

- Use the Performance Profiler to identify bottlenecks

## Example Scene

See `Samples~/Built-in/BuiltInDemo.unity` for a complete working example:
- Camera setup
- Demo script with rotating shapes
- Ground plane and reference objects

## API Reference

### Key Methods

```csharp
// Initialize adapter
_adapterService.Initialize(Camera camera, bool enableShapes);

// Draw primitives
_renderer.DrawLine(Vector3 start, Vector3 end, Color color);
_renderer.DrawQuad(Vector3 center, Quaternion rotation, Vector2 size, Color color);

// Cleanup
_adapterService.Cleanup();
```

See [Architecture](Architecture.md) for full API details.

## Next Steps

- [URP Setup](RenderPipeline-URP.md) - If migrating to URP
- [HDRP Setup](RenderPipeline-HDRP.md) - If using HDRP
- [Visual Debug API](Architecture.md#visual-debug-rendering) - Full API reference
