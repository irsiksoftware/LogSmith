# LogSmith Optional Sinks Documentation

## Introduction

The LogSmith Optional Sinks package provides production-ready integrations with popular logging backends and services. This package is designed to extend LogSmith's core functionality without adding dependencies to the base package.

## Package Philosophy

This optional sinks package follows these principles:

1. **Zero Core Impact** - The core LogSmith package has no dependencies on this package
2. **Production Ready** - All sinks are designed for production use with proper error handling
3. **Extensible** - Easy to add custom sinks following the same patterns
4. **Performance Focused** - Batching, async delivery, and minimal allocation overhead

## Architecture

All sinks in this package implement the `ILogSink` interface:

```csharp
public interface ILogSink
{
    void Write(LogMessage message);
    void Flush();
    string Name { get; }
}
```

### Common Patterns

#### Batching
Most HTTP-based sinks implement batching to reduce network overhead:
- Configurable batch size (number of messages)
- Configurable flush interval (time-based)
- Manual flush via `Flush()` method

#### Thread Safety
All sinks use lock-based synchronization to ensure thread-safe operation.

#### Async Delivery
HTTP-based sinks use Unity coroutines for async delivery, requiring a `MonoBehaviour` instance.

## Sink Reference

### HttpSink

Generic HTTP/REST endpoint sink.

**Constructor Parameters:**
- `endpoint` (string, required) - HTTP endpoint URL
- `apiKey` (string, optional) - API key for Bearer token auth
- `batchSize` (int, default: 10) - Messages per batch
- `flushInterval` (float, default: 5.0) - Seconds between auto-flush
- `coroutineRunner` (MonoBehaviour, required) - Coroutine runner

**JSON Format:**
```json
{
  "logs": [
    {
      "timestamp": "2025-10-16T10:30:00.000Z",
      "level": "Info",
      "category": "Game",
      "message": "Player spawned",
      "frame": 120,
      "threadId": 1,
      "context": {}
    }
  ]
}
```

### SentrySink

Sentry error tracking integration.

**Constructor Parameters:**
- `dsn` (string, required) - Sentry DSN
- `environment` (string, default: "production") - Environment name
- `release` (string, default: Application.version) - Release identifier
- `minimumLevel` (LogLevel, default: Error) - Minimum level to send
- `coroutineRunner` (MonoBehaviour, required) - Coroutine runner

**Features:**
- Automatic event aggregation in Sentry UI
- Stack trace capture and formatting
- Environment and release tagging
- ECS metadata

### SeqSink

Seq structured logging integration.

**Constructor Parameters:**
- `serverUrl` (string, required) - Seq server URL
- `apiKey` (string, optional) - API key
- `batchSize` (int, default: 10) - Messages per batch
- `flushInterval` (float, default: 5.0) - Seconds between auto-flush
- `coroutineRunner` (MonoBehaviour, required) - Coroutine runner

**Format:** CLEF (Compact Log Event Format)

**Features:**
- Full structured logging support
- Query and filter in Seq UI
- Level mapping to Serilog levels
- Source context preservation

### ElasticsearchSink

Elasticsearch integration with ECS compatibility.

**Constructor Parameters:**
- `nodeUrl` (string, required) - Elasticsearch node URL
- `indexName` (string, default: "logsmith-{0:yyyy.MM.dd}") - Index pattern
- `username` (string, optional) - Basic auth username
- `password` (string, optional) - Basic auth password
- `batchSize` (int, default: 50) - Messages per batch
- `flushInterval` (float, default: 10.0) - Seconds between auto-flush
- `coroutineRunner` (MonoBehaviour, required) - Coroutine runner

**Features:**
- ECS (Elastic Common Schema) compatible fields
- Bulk API for efficient indexing
- Date-based index patterns
- Full Kibana integration

## Performance Considerations

### Batching Configuration

Choose batch sizes based on your needs:

| Use Case | Batch Size | Flush Interval |
|----------|-----------|----------------|
| Real-time debugging | 1-5 | 1-2s |
| Development | 10-25 | 5s |
| Production | 50-100 | 10-30s |

### Memory Usage

Each batched message is held in memory until flushed. Monitor memory usage in production:

```csharp
// Lower memory, more frequent network calls
var sink = new HttpSink(endpoint, batchSize: 10, flushInterval: 2.0f);

// Higher memory, fewer network calls
var sink = new HttpSink(endpoint, batchSize: 100, flushInterval: 30.0f);
```

### Network Considerations

All HTTP-based sinks perform network I/O on Unity's main thread via coroutines. For high-throughput scenarios, consider:

1. Increasing batch sizes
2. Using local aggregation services (e.g., local Seq instance)
3. Implementing custom async delivery mechanisms

## Extending the Package

### Creating Custom Sinks

Implement `ILogSink` interface:

```csharp
using IrsikSoftware.LogSmith;

public class CustomSink : ILogSink
{
    public string Name => "Custom";

    public void Write(LogMessage message)
    {
        // Your implementation
    }

    public void Flush()
    {
        // Flush any buffered messages
    }
}
```

### Best Practices

1. Use batching for network-based sinks
2. Implement proper error handling (log to Debug.LogError)
3. Make sinks disposable if they hold resources
4. Use lock-based synchronization for thread safety
5. Follow naming conventions (e.g., "SinkName" for Name property)

## Troubleshooting

### Logs Not Appearing

1. Check network connectivity to endpoint
2. Verify API keys and authentication
3. Check Unity console for error messages
4. Ensure coroutine runner is still alive
5. Call `Flush()` manually to force delivery

### Performance Issues

1. Increase batch sizes
2. Increase flush intervals
3. Filter by minimum log level
4. Use local aggregation services

### Coroutine Errors

Ensure the `MonoBehaviour` instance:
- Is attached to an active GameObject
- Lives for the duration of logging
- Is not destroyed during scene transitions

Use `DontDestroyOnLoad` if needed:

```csharp
var runner = new GameObject("LogSinkRunner").AddComponent<CoroutineRunner>();
DontDestroyOnLoad(runner.gameObject);
```

## Migration Guide

### From Custom HTTP Implementation

If you have custom HTTP logging:

```csharp
// Before
SendLogsToServer(logData);

// After
var httpSink = new HttpSink("https://your-server.com/logs", coroutineRunner: this);
logRouter.RegisterSink(httpSink);
```

### Adding to Existing LogSmith Setup

```csharp
// Existing setup
var logRouter = container.Resolve<ILogRouter>();
logRouter.RegisterSink(new ConsoleSink());

// Add optional sinks
logRouter.RegisterSink(new SeqSink("http://localhost:5341", coroutineRunner: this));
logRouter.RegisterSink(new SentrySink(dsn, minimumLevel: LogLevel.Error, coroutineRunner: this));
```

## Support and Contributing

- Report issues: https://github.com/irsiksoftware/LogSmith/issues
- Contribute: https://github.com/irsiksoftware/LogSmith/pulls
- Documentation: https://github.com/irsiksoftware/LogSmith/wiki
