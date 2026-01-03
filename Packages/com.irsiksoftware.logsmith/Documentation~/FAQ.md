# Frequently Asked Questions

## General

**Q: What Unity versions are supported?**
A: Minimum Unity 2022.3 LTS. Developed and tested on Unity 6000.2 LTS.

**Q: Does LogSmith require VContainer?**
A: No. VContainer integration is optional. LogSmith works with static access patterns without any DI framework.

**Q: Can I use LogSmith with other DI frameworks (Zenject, etc.)?**
A: Yes, but you'll need custom registration code. VContainer integration is built-in.

**Q: Is LogSmith thread-safe?**
A: Yes. Router, registry, and sinks handle concurrent access safely.

## Performance

**Q: What's the performance overhead?**
A: < 0.2ms/frame at 1,000 messages/second (on baseline dev hardware). Filtering happens before formatting, minimizing overhead for disabled logs.

**Q: Does logging allocate GC memory?**
A: Yes, for message strings and context dictionaries. Use filtering to minimize allocations in hot paths.

**Q: Can I disable logging in production builds?**
A: Yes, via categories or global minimum level. Set to `LogLevel.Warn` to disable Info/Debug.

## Configuration

**Q: Where are settings stored?**
A: In a `LoggingSettings` ScriptableObject (default: `Assets/Resources/LoggingSettings.asset`).

**Q: Can I have different settings per scene?**
A: Yes. Load different `LoggingSettings` assets and call `Log.ReloadSettings(newSettings)`.

**Q: How do I configure log file path?**
A: Window → LogSmith → Settings → Sinks tab. Relative paths use `Application.persistentDataPath`.

## Features

**Q: How do I add custom sinks?**
A: Implement `ILogSink` and register via `router.RegisterSink(sink)`. See `Samples~/CustomSinks/`.

**Q: Can I log to multiple files simultaneously?**
A: Yes. Create multiple `FileSink` instances with different paths and register all.

**Q: How do I toggle the debug overlay?**
A: Press **F1** in Play mode (configurable in settings).

**Q: Can I filter logs by level in the overlay?**
A: Yes. Use dropdown in overlay to filter by Trace/Debug/Info/Warn/Error/Critical.

## Troubleshooting

**Q: Logs not appearing in Console**
A: Check Console Sink is enabled (Window → LogSmith → Settings → Sinks).

**Q: File sink not writing**
A: Ensure platform supports file I/O (WebGL doesn't). Check `Application.persistentDataPath` permissions.

**Q: Category colors not showing**
A: Colors display in debug overlay. Unity Console shows standard colors (white/yellow/red).

**Q: "ILog not registered" error**
A: Call `builder.RegisterLogSmith()` in your VContainer LifetimeScope.

**Q: High memory usage**
A: Check file sink retention count. Lower max file size or retention count to reduce disk usage.

## Integration

**Q: Can I use LogSmith with Unity's built-in Debug.Log?**
A: Yes. LogSmith's ConsoleSink calls `Debug.Log` internally, so they coexist.

**Q: Does LogSmith work with IL2CPP?**
A: Yes. Fully tested and compatible with IL2CPP/AOT platforms.

**Q: Can I use LogSmith in Editor scripts?**
A: Yes. Static access works in both Edit and Play modes.

## Extending

**Q: Can I create custom log levels?**
A: No. LogSmith uses fixed levels: Trace, Debug, Info, Warn, Error, Critical.

**Q: Can I add custom tokens to templates?**
A: Yes, via context dictionary. Add `{"myToken": value}` and use `{myToken}` in template.

**Q: Can I replace the backend (com.unity.logging)?**
A: Yes. Swap `NativeUnityLoggerAdapter` implementation. See Architecture docs.
