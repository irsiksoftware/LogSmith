# High Definition Render Pipeline (HDRP) Setup

LogSmith's visual debug renderer integrates with HDRP via a **Custom Pass** that must be manually added to your HDRP Volume or camera.

## Prerequisites

- Unity 2022.3 LTS or newer
- HDRP package installed (`com.unity.render-pipelines.high-definition`)
- An HDRP project (not URP/Built-in)

## Setup Steps

### 1. Add a Custom Pass Volume

#### Option A: Global Volume (Recommended)

1. **Create** → **Volume** → **Custom Pass**
2. Name it `LogSmith Visual Debug Volume`
3. In the Inspector, under **Custom Passes**:
   - Click **+** to add a new pass
   - Select **LogSmith Visual Debug Pass**

4. **Configure**:
   - **Injection Point**: `After Post Processing` (default)
   - **Target Camera**: `None` (applies to all cameras)
   - **Layer Mask**: `Everything`

#### Option B: Per-Camera Volume

1. Add a **Custom Pass Volume** component to your Camera GameObject
2. Set **Mode** to `Camera`
3. Follow step 3-4 from Option A

![HDRP Custom Pass](../Documentation~/Images/hdrp-custom-pass.png)

### 2. Add the Demo Script

Open `Samples~/HDRP/HDRPDemo.unity` or add `HDRPDemo.cs` to a GameObject:

```csharp
using UnityEngine;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Core;

public class HDRPDemo : MonoBehaviour
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
            _renderer.DrawLine(Vector3.zero, Vector3.up * 2f, Color.magenta);
        }
    }
}
```

### 3. Enable Visual Debug

- Open **Tools > LogSmith Settings**
- Under **Visual Debug** tab, check `Enable Visual Debug`
- Press Play

## How It Works

The HDRP adapter uses **Custom Passes** to inject rendering commands into HDRP's rendering pipeline:

1. **Custom Pass Volume** defines when and where to inject
2. **LogSmith Visual Debug Pass** executes the actual drawing
3. Visual debug shapes are rendered using `CommandBuffer` with HDRP-compatible shaders

```csharp
// Internal implementation (for reference)
public class LogSmithVisualDebugPass : CustomPass
{
    protected override void Execute(CustomPassContext ctx)
    {
        // Render visual debug shapes
        _renderer.Render(ctx.cmd, ctx.hdCamera.camera);
    }
}
```

## Features

| Feature | Supported | Notes |
|---------|-----------|-------|
| **Overlay UI** | ✅ | Pipeline-agnostic, always available |
| **DrawLine** | ✅ | CommandBuffer-based |
| **DrawQuad** | ✅ | Supports transparency and HDR colors |
| **DrawCircle** | ✅ | Approximated with line segments |
| **Depth Testing** | ✅ | Configured via Custom Pass |
| **Post-Processing** | ✅ | Shapes render after post-processing by default |
| **Raytracing** | ⚠️ | Shapes use rasterization, not raytraced |
| **HDR Output** | ✅ | Supports HDR color values > 1.0 |

## Configuration

### Injection Point

Control when shapes are rendered in the HDRP pipeline:

| Injection Point | Use Case |
|-----------------|----------|
| `Before Rendering` | Shapes render early, affected by all post-processing |
| `After Opaque` | Shapes render after opaque geometry, before transparents |
| `Before Post Processing` | Shapes affected by post-processing (TAA, motion blur, etc.) |
| `After Post Processing` | **Default**. Shapes on top of everything, unaffected by post-processing |

To change:
1. Select your **Custom Pass Volume**
2. Expand the **LogSmith Visual Debug Pass**
3. Change **Injection Point**

### Multiple Cameras (HDRP-Specific)

For multi-camera setups:

**Global Volume** (one pass for all cameras):
```csharp
// Adapter per camera
var adapter1 = new RenderPipelineAdapterService();
adapter1.Initialize(camera1, true);

var adapter2 = new RenderPipelineAdapterService();
adapter2.Initialize(camera2, true);
```

**Per-Camera Volumes**:
- Add a Custom Pass Volume to each camera
- Set **Mode** to `Camera`
- Add the LogSmith Visual Debug Pass to each volume

### HDR Color Support

HDRP supports HDR colors (intensity > 1.0):

```csharp
// Emissive red with 2x intensity
Color hdrRed = new Color(2f, 0f, 0f, 1f);
_renderer.DrawLine(start, end, hdrRed);
```

## Known Limitations

### 1. **Custom Pass Required**
   - **Must** add the Custom Pass manually to a volume
   - Forgetting this step = no shapes will appear
   - **Symptom**: Overlay UI works, but no debug shapes

   **Workaround**: LogSmith logs a warning if HDRP is detected but the Custom Pass is missing. Check Console.

### 2. **Raytracing Compatibility**
   - Visual debug shapes use **rasterization**, not raytracing
   - Shapes will not appear in raytraced reflections or GI
   - **Workaround**: Not applicable to debug tools (expected behavior)

### 3. **HDRP Version Differences**
   - Tested with HDRP 12.x (Unity 2022 LTS) and HDRP 14.x (Unity 2023 LTS)
   - Older HDRP versions (<10.x) may have different Custom Pass APIs

### 4. **Depth Write Behavior**
   - Shapes use `ZWrite Off` by default to prevent occluding scene geometry
   - Cannot be changed without modifying the Custom Pass shader (advanced)

### 5. **Performance Overhead**
   - Custom Passes have slightly higher overhead than Built-in RP callbacks
   - Limit shapes to < 400 per frame on high-end desktop, < 200 on consoles

### 6. **TAA (Temporal Anti-Aliasing) Ghosting**
   - Animated shapes may cause TAA ghosting if they move quickly
   - **Workaround**: Set Injection Point to `After Post Processing` to bypass TAA

## Troubleshooting

### Shapes Don't Appear (Overlay UI Works)

**Most Common Issue**: Custom Pass not added

1. Check for a **Custom Pass Volume** in your scene or on your camera
2. Verify **LogSmith Visual Debug Pass** is in the list
3. Check Console for warnings:
   ```
   [LogSmith] HDRP detected but Custom Pass not found. Add LogSmithVisualDebugPass to a Custom Pass Volume.
   ```

**Debug Steps**:
- Open **Window** → **Rendering** → **Graphics Debugger**
- Look for `LogSmithVisualDebugPass` in the rendering event list

### Shapes Flicker or Disappear

**Possible Causes**:
- Custom Pass disabled in the volume
- Volume priority conflicts (another volume overriding)
- Camera near/far plane clipping

**Solution**:
1. Select the Custom Pass Volume
2. Ensure **Enabled** is checked
3. Increase **Priority** if multiple volumes exist

### Shapes Have Wrong Colors (Too Bright/Dark)

**Cause**: HDR color space mismatch

**Solution**:
- If using Linear color space, ensure colors are in linear space:
  ```csharp
  Color linear = color.linear; // Convert gamma to linear
  _renderer.DrawLine(start, end, linear);
  ```

- Or use HDR colors directly:
  ```csharp
  Color hdr = new Color(1.5f, 0.5f, 0.2f, 1f); // Emissive orange
  ```

### Performance Drops with Many Shapes

**Solutions**:
- Reduce shape count (throttle with `Time.frameCount % N`)
- Disable the Custom Pass on low-end hardware:

  ```csharp
  #if UNITY_STANDALONE_WIN
      // Enable on PC only
      _adapterService.Initialize(camera, enableShapes: true);
  #else
      _adapterService.Initialize(camera, enableShapes: false);
  #endif
  ```

- Use HDRP's **Frame Debugger** to inspect overdraw and batch counts

### Post-Processing Artifacts (TAA Ghosting, Motion Blur)

**Cause**: Shapes render before post-processing

**Solution**: Set Injection Point to `After Post Processing`

## Example Scene

See `Samples~/HDRP/HDRPDemo.unity`:
- Pre-configured Custom Pass Volume with LogSmith pass
- Demo script with rotating cube wireframe
- HDRP-compatible materials and lighting

## Migration from URP/Built-in RP

1. **Install HDRP** via Package Manager
2. **Upgrade Project** using `Edit > Rendering > Upgrade Project Materials to HDRP`
3. **Add Custom Pass Volume** (see Setup Steps above)
4. **No code changes needed** - the adapter automatically detects HDRP

**Note**: Migrating to HDRP requires significant project changes. Consult [Unity's HDRP Migration Guide](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest) first.

## API Reference

### Key Methods (Same as Built-in RP & URP)

```csharp
// Initialize adapter (auto-detects HDRP)
_adapterService.Initialize(Camera camera, bool enableShapes);

// Draw primitives
_renderer.DrawLine(Vector3 start, Vector3 end, Color color);
_renderer.DrawQuad(Vector3 center, Quaternion rotation, Vector2 size, Color color);

// Cleanup
_adapterService.Cleanup();
```

### HDRP-Specific: HDR Color Support

```csharp
// Emissive colors with intensity > 1.0
Color hdrColor = new Color(2f, 1f, 0.5f, 1f);
_renderer.DrawQuad(center, rotation, size, hdrColor);
```

## Next Steps

- [URP Setup](RenderPipeline-URP.md) - Comparison with URP
- [Built-in RP Setup](RenderPipeline-BuiltIn.md) - Comparison with Built-in RP
- [Visual Debug API](Architecture.md#visual-debug-rendering) - Full API reference
- [HDRP Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest) - Unity's official HDRP docs
- [Custom Passes](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@latest/index.html?subfolder=/manual/Custom-Pass.html) - HDRP Custom Pass documentation
