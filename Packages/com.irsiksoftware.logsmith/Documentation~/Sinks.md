# Sinks

Sinks are output destinations for log messages.

## Built-in Sinks

### ConsoleSink
Writes to Unity Console with color coding.

**Configuration**:
- Window → LogSmith → Settings → Sinks tab
- Toggle "Enable Console Sink"

**Features**:
- Color-coded by level (Info=white, Warn=yellow, Error=red)
- Category colors displayed
- Stack traces for Error/Critical

### FileSink
Writes to rotating log files.

**Configuration**:
```csharp
Settings:
- Enable File Sink: ☑
- Log Path: Logs/logsmith.log (relative to project)
- Max File Size: 10 MB
- Retention Count: 5
```

**Features**:
- Size-based rotation
- Automatic cleanup (keeps last N files)
- Customizable path
- Platform-aware (disables on WebGL, etc.)

## Custom Sinks

### Creating a Sink

```csharp
using IrsikSoftware.LogSmith;

public class HttpSink : ILogSink
{
    private readonly string _endpoint;

    public HttpSink(string endpoint)
    {
        _endpoint = endpoint;
    }

    public void Write(LogLevel level, string category, string message, Dictionary<string, object> context)
    {
        var payload = new { level, category, message, context };
        // Send HTTP POST to _endpoint
    }

    public void Flush()
    {
        // Ensure all pending requests complete
    }

    public void Dispose()
    {
        // Clean up resources
    }
}
```

### Registering a Sink

#### With DI
```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterLogSmith();
    builder.Register<HttpSink>(Lifetime.Singleton)
           .WithParameter("endpoint", "https://logs.example.com/api");

    // Register with router
    builder.RegisterBuildCallback(container =>
    {
        var router = container.Resolve<ILogRouter>();
        var httpSink = container.Resolve<HttpSink>();
        router.RegisterSink(httpSink);
    });
}
```

#### Without DI
```csharp
var router = Log.Resolve<ILogRouter>();
var httpSink = new HttpSink("https://logs.example.com/api");
router.RegisterSink(httpSink);
```

## Sink Examples

See `Samples~/CustomSinks/` for:
- **HttpSink**: POST logs to REST API
- **DatabaseSink**: Store in SQLite
- **CloudSink**: Send to AWS CloudWatch, Azure Monitor, GCP Logging

## Sink Lifecycle

```
1. Router.RegisterSink(sink)
2. Message logged → Router.Route()
3. Router calls sink.Write() for each message
4. On shutdown → sink.Flush()
5. On disposal → sink.Dispose()
```

## Thread Safety

Sinks must be thread-safe if logging from multiple threads:

```csharp
public class ThreadSafeSink : ILogSink
{
    private readonly object _lock = new object();
    private readonly StreamWriter _writer;

    public void Write(LogLevel level, string category, string message, Dictionary<string, object> context)
    {
        lock (_lock)
        {
            _writer.WriteLine(message);
        }
    }
}
```

## Best Practices

1. **Flush on critical errors**: Ensure data persists before crash
2. **Async for I/O**: Use async writes to avoid blocking game thread
3. **Batch writes**: Buffer messages for network/database sinks
4. **Error handling**: Don't throw from Write() - log errors internally
5. **Dispose properly**: Clean up file handles, connections, etc.
