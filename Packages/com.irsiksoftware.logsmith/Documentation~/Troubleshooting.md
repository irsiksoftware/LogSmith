# Troubleshooting

## Logs Not Appearing

### Console Sink Not Working
**Symptoms**: No logs in Unity Console

**Solutions**:
1. Check Console Sink enabled: Window → LogSmith → Settings → Sinks
2. Verify minimum log level allows messages:
   ```csharp
   router.SetGlobalMinimumLevel(LogLevel.Trace); // Allow all levels
   ```
3. Check category is enabled:
   ```csharp
   registry.SetEnabled("MyCategory", true);
   ```

### File Sink Not Writing
**Symptoms**: Log file empty or missing

**Solutions**:
1. Check File Sink enabled in settings
2. Verify platform supports file I/O (WebGL doesn't):
   ```csharp
   var platform = LogSmith.Resolve<IPlatformCapabilities>();
   Debug.Log($"Writable path: {platform.HasWritablePersistentDataPath}");
   ```
3. Check log file path permissions
4. Call `Flush()` before checking file:
   ```csharp
   var router = LogSmith.Resolve<ILogRouter>();
   router.Flush(); // Force write to disk
   ```

## Dependency Injection Issues

### "ILog not registered"
**Cause**: LogSmith services not registered in DI container

**Solution**:
```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterLogSmith(); // Add this
    }
}
```

### Wrong Logger Instance
**Cause**: Creating logger before VContainer initialized

**Solution**: Use constructor injection, not field initializers:
```csharp
// ❌ Wrong
public class MyService
{
    private ILog _log = LogSmith.GetLogger("MyService"); // Too early
}

// ✅ Correct
public class MyService
{
    private readonly ILog _log;

    public MyService(ILog log) // Injected after container built
    {
        _log = log;
    }
}
```

## Performance Issues

### High GC Allocations
**Cause**: Too many log messages in hot paths

**Solutions**:
1. Filter verbose categories in production:
   ```csharp
   #if !DEVELOPMENT_BUILD
   registry.SetMinimumLevel("AI", LogLevel.Warn);
   #endif
   ```
2. Disable categories in tight loops:
   ```csharp
   registry.SetEnabled("Physics", false);
   ```

### Slow File Writes
**Cause**: Synchronous I/O blocking game thread

**Solution**: Reduce log frequency or increase buffer size (custom FileSink).

## Configuration Issues

### Settings Not Applying
**Cause**: Settings loaded after LogSmith initialized

**Solution**: Load settings before first log:
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void InitLogging()
{
    var settings = Resources.Load<LoggingSettings>("CustomSettings");
    LogSmith.ReloadSettings(settings);
}
```

### Live Reload Not Working
**Cause**: Live reload disabled in settings

**Solution**: Window → LogSmith → Settings → Enable "Live Reload"

## Debug Overlay Issues

### Overlay Not Showing
**Symptoms**: F1 doesn't toggle overlay

**Solutions**:
1. Check in Play mode (overlay disabled in Edit mode)
2. Verify overlay enabled in settings
3. Check for UI conflicts (F1 bound elsewhere)

### Overlay Performance Lag
**Cause**: Too many messages in circular buffer

**Solution**: Reduce buffer size in `LoggingSettings.OverlayMaxMessages` (default: 1000).

## Build Issues

### Missing References
**Cause**: Package not in manifest

**Solution**: Ensure `Packages/manifest.json` includes LogSmith:
```json
{
  "dependencies": {
    "com.irsiksoftware.logsmith": "file:../Packages/com.irsiksoftware.logsmith"
  }
}
```

### IL2CPP Compilation Errors
**Cause**: AOT compatibility issue

**Solution**: LogSmith is IL2CPP-compatible. Check custom sinks for reflection/dynamic code.

## Platform-Specific Issues

### WebGL: File Sink Not Available
**Expected**: WebGL doesn't support file I/O

**Solution**: Use ConsoleSink or custom cloud sink.

### Android: Permission Denied
**Cause**: Missing WRITE_EXTERNAL_STORAGE permission

**Solution**: Use `Application.persistentDataPath` (no permission needed).

### iOS: Log File Not Persisting
**Cause**: App sandbox restrictions

**Solution**: Use `Application.persistentDataPath` for logs (automatically persisted).

## Getting Help

1. Check existing documentation (Architecture, FAQ, Samples)
2. Review samples: `Packages/com.irsiksoftware.logsmith/Samples~/`
3. Enable debug logging:
   ```csharp
   registry.RegisterCategory("LogSmith.Internal", LogLevel.Trace);
   ```
4. File issue on GitHub with logs and reproduction steps
