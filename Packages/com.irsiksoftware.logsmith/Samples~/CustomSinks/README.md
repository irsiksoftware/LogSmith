# Custom Sink Examples

This directory contains example implementations of custom log sinks for LogSmith.

## Creating a Custom Sink

To create your own custom sink:

### 1. Implement ILogSink Interface

```csharp
using IrsikSoftware.LogSmith;

public class MyCustomSink : ILogSink
{
    public string Name => "MyCustomSink";

    public MessageFormat CurrentFormat { get; set; } = MessageFormat.Text;

    public void Write(LogMessage message)
    {
        // Your custom logic here
    }

    public void Flush()
    {
        // Flush any buffered messages
    }

    public void Dispose()
    {
        // Clean up resources
    }
}
```

### 2. Register Your Sink

```csharp
// At application startup or in your bootstrap code:
var mySink = new MyCustomSink();
Log.Router.RegisterSink(mySink);
```

### 3. Enable/Disable at Runtime

```csharp
// Unregister when no longer needed
Log.Router.UnregisterSink(mySink);
```

## Sink Ordering

Sinks are called in the order they are registered. To control execution order:

```csharp
// Register in desired order
Log.Router.RegisterSink(firstSink);
Log.Router.RegisterSink(secondSink);
Log.Router.RegisterSink(thirdSink);
```

## Examples

### HttpSink.cs

A stub implementation showing how to send logs to an HTTP endpoint. Features:
- URL configuration
- JSON/Text format support
- Error handling patterns
- Production implementation guidance

**Production Considerations:**
- Implement message batching for efficiency
- Add retry logic with exponential backoff
- Use background threading to avoid blocking
- Implement circuit breaker for failing endpoints
- Add authentication/authorization headers
- Monitor and alert on delivery failures

### Creating Other Custom Sinks

Common sink types you might implement:

**DatabaseSink**
```csharp
public class DatabaseSink : ILogSink
{
    private readonly IDbConnection _connection;

    public void Write(LogMessage message)
    {
        // INSERT INTO logs (level, category, message, timestamp, ...)
    }
}
```

**CloudSink** (e.g., AWS CloudWatch, Azure Monitor)
```csharp
public class CloudWatchSink : ILogSink
{
    private readonly IAmazonCloudWatchLogs _client;

    public void Write(LogMessage message)
    {
        // Send to AWS CloudWatch Logs
    }
}
```

**SlackSink** (for critical alerts)
```csharp
public class SlackSink : ILogSink
{
    public void Write(LogMessage message)
    {
        if (message.Level >= LogLevel.Error)
        {
            // POST to Slack webhook
        }
    }
}
```

## Best Practices

1. **Non-Blocking**: Sinks should not block the logging thread
   - Use background queues for I/O operations
   - Return quickly from `Write()`

2. **Error Handling**: Never throw exceptions from `Write()`
   - Catch and log errors internally
   - Consider fallback mechanisms

3. **Resource Management**: Implement `Dispose()` properly
   - Flush pending messages
   - Release connections
   - Cancel background tasks

4. **Performance**: Consider the impact on your application
   - Batch messages when possible
   - Use async I/O
   - Monitor memory usage

5. **Format Support**: Respect `CurrentFormat` property
   - Allow users to choose Text or JSON
   - Use the provided `IMessageTemplateEngine`

## Testing Your Sink

```csharp
[Test]
public void CustomSink_Write_ProcessesMessage()
{
    var sink = new MyCustomSink();
    var message = new LogMessage
    {
        Level = LogLevel.Info,
        Message = "Test",
        Category = "Test",
        Timestamp = DateTime.UtcNow
    };

    Assert.DoesNotThrow(() => sink.Write(message));
}
```

## Third-Party Sink Acceptance

Third-party sinks can be added **without modifying LogSmith core assemblies**. Simply:
1. Create your sink class in your own assembly
2. Implement `ILogSink`
3. Register it with `Log.Router.RegisterSink()`

This demonstrates LogSmith's extensibility commitment.
