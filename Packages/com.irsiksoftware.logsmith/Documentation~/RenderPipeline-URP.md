# Universal Render Pipeline (URP) Setup

LogSmith's visual debug renderer integrates with URP via a **Renderer Feature** that must be manually added to your URP Renderer asset.

## Prerequisites

- Unity 2022.3 LTS or newer
- URP package installed (`com.unity.render-pipelines.universal`)
- A configured URP Renderer asset

## Setup Steps

### 1. Add the Renderer Feature

1. Open your **URP Renderer** asset (usually in `Assets/Settings/`)
   - For Forward Renderer: `UniversalRenderer`
   - For 2D Renderer: `Renderer2D`

2. Click **Add Renderer Feature**

3. Select **LogSmith Visual Debug Renderer**

4. **Important**: Place it **after** all other renderer features to ensure shapes render on top

![URP Renderer Feature](../Documentation~/Images/urp-renderer-feature.png)

### 2. Configure the Feature

| Setting | Default | Description |
|---------|---------|-------------|
| **Render Pass Event** | `After Rendering Post Processing` | When to inject visual debug rendering |
| **Layer Mask** | `Everything` | Which layers to render (usually leave as Everything) |

### 3. Add the Demo Script

Open `Samples~/URP/URPDemo.unity` or add `URPDemo.cs` to a GameObject:

```csharp
using UnityEngine;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

public class URPDemo : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

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
        if (_renderer != null)
        {
            _renderer.DrawLine(Vector3.zero, Vector3.up * 2f, Color.cyan);
        }
    }
}
```

### 4. Enable Visual Debug

- Open **Tools > LogSmith Settings**
- Under **Visual Debug** tab, check `Enable Visual Debug`
- Press Play

## How It Works

The URP adapter uses a **ScriptableRendererFeature** to inject rendering commands into URP's rendering pipeline:

1. **Renderer Feature** registers a `ScriptableRenderPass`
2. **Render Pass** executes at the configured `RenderPassEvent` (default: After Post Processing)
3. Visual debug shapes are drawn using `CommandBuffer` and Unity's immediate-mode rendering

```csharp
// Internal implementation (for reference)
public class LogSmithVisualDebugRenderFeature : ScriptableRendererFeature
{
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_renderPass);
    }
}
```

## Features

| Feature | Supported | Notes |
|---------|-----------|-------|
| **Overlay UI** | ✅ | Pipeline-agnostic, always available |
| **DrawLine** | ✅ | CommandBuffer-based |
| **DrawQuad** | ✅ | Supports transparency |
| **DrawCircle** | ✅ | Approximated with line segments |
| **Depth Testing** | ✅ | Configured via Renderer Feature |
| **Post-Processing** | ✅ | Shapes render after post-processing by default |
| **2D Renderer** | ✅ | Works with URP 2D Renderer |

## Configuration

### Render Pass Event

Control when shapes are rendered:

| Event | Use Case |
|-------|----------|
| `Before Rendering Transparents` | Shapes appear before transparent objects |
| `After Rendering Post Processing` | **Default**. Shapes on top of everything |
| `Before Rendering Post Processing` | Shapes affected by post-processing (bloom, etc.) |

To change:
1. Select your URP Renderer asset
2. Find **LogSmith Visual Debug Renderer** feature
3. Change **Render Pass Event**

### Multiple Cameras (URP-Specific)

For Camera Stacks (Overlay cameras):

```csharp
// Base camera
var baseAdapter = new RenderPipelineAdapterService();
baseAdapter.Initialize(baseCamera, true);

// Overlay camera (optional, if you want shapes on overlay too)
var overlayAdapter = new RenderPipelineAdapterService();
overlayAdapter.Initialize(overlayCamera, true);
```

The Renderer Feature will automatically handle rendering for all cameras using that Renderer asset.

## Known Limitations

### 1. **Renderer Feature Required**
   - **Must** add the feature manually to each Renderer asset
   - Forgetting this step = no shapes will appear
   - **Symptom**: Overlay UI works, but no debug shapes

   **Workaround**: LogSmith logs a warning if URP is detected but the feature is missing. Check Console.

### 2. **SRP Batcher Compatibility**
   - Visual debug rendering is SRP Batcher compatible
   - No performance penalty from batching breaks

### 3. **URP Version Differences**
   - Tested with URP 12.x (Unity 2022 LTS) and URP 14.x (Unity 2023 LTS)
   - Older URP versions (<10.x) may have API differences

### 4. **Depth Write Behavior**
   - Shapes use `ZWrite Off` by default to prevent occluding scene geometry
   - To change: Modify the `LogSmithVisualDebugRenderPass` material settings (advanced)

### 5. **Mobile Performance**
   - On mobile GPUs, limit shapes to < 300 per frame
   - Use URP's `FrameDebugger` to inspect overdraw

## Troubleshooting

### Shapes Don't Appear (Overlay UI Works)

**Most Common Issue**: Renderer Feature not added

1. Open your URP Renderer asset
2. Verify **LogSmith Visual Debug Renderer** is in the list
3. Check Console for warnings:
   ```
   [LogSmith] URP detected but Renderer Feature not found. Add LogSmithVisualDebugRenderFeature to your Renderer asset.
   ```

### Shapes Render Behind Objects

**Cause**: Render Pass Event is too early

**Solution**: Set Render Pass Event to `After Rendering Post Processing`

### Shapes Flicker or Disappear

**Possible Causes**:
- Camera near/far plane clipping
- Shapes outside camera frustum
- Renderer Feature disabled in Renderer asset

**Debug**:
```csharp
if (_renderer == null)
{
    Debug.LogWarning("[URPDemo] Renderer is null - URP adapter may have failed to initialize");
}
```

### Performance Drops with Many Shapes

**Solutions**:
- Reduce shape count (throttle with `Time.frameCount % N`)
- Use URP's Quality Settings to disable the feature on low-end devices:

  ```csharp
  #if UNITY_ANDROID || UNITY_IOS
      // Disable shapes on mobile
      _adapterService.Initialize(camera, enableShapes: false);
  #endif
  ```

### Post-Processing Doesn't Affect Shapes

**Expected Behavior**: By default, shapes render **after** post-processing

**To Change**: Set Render Pass Event to `Before Rendering Post Processing`

## Example Scene

See `Samples~/URP/URPDemo.unity`:
- Pre-configured URP Renderer with feature added
- Demo script with animated shapes
- Ground plane and reference objects

## Migration from Built-in RP

1. **Install URP** via Package Manager
2. **Upgrade Materials** using `Edit > Rendering > Materials > Convert to URP`
3. **Add Renderer Feature** to your URP Renderer asset (see Setup Steps above)
4. **No code changes needed** - the adapter automatically detects URP

## API Reference

### Key Methods (Same as Built-in RP)

```csharp
// Initialize adapter (auto-detects URP)
_adapterService.Initialize(Camera camera, bool enableShapes);

// Draw primitives
_renderer.DrawLine(Vector3 start, Vector3 end, Color color);
_renderer.DrawQuad(Vector3 center, Quaternion rotation, Vector2 size, Color color);

// Cleanup
_adapterService.Cleanup();
```

## Next Steps

- [HDRP Setup](RenderPipeline-HDRP.md) - If using HDRP instead
- [Built-in RP Setup](RenderPipeline-BuiltIn.md) - Comparison with Built-in RP
- [Visual Debug API](Architecture.md#visual-debug-rendering) - Full API reference
- [URP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest) - Unity's official URP docs
