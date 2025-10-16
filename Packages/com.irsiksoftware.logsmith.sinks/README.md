# LogSmith Optional Sinks

A collection of optional sinks for LogSmith that enable logging to external services and platforms.

## Overview

This package provides production-ready sink implementations for popular logging backends. Each sink is designed to work seamlessly with LogSmith's core logging infrastructure while remaining completely optional.

## Available Sinks

### HTTP/REST Sink
Send logs to any HTTP/REST endpoint with batching support.

**Features:**
- Configurable batch size and flush intervals
- JSON serialization
- API key authentication support
- Async delivery using Unity coroutines

**Usage:**
```csharp
var httpSink = new HttpSink(
    endpoint: "https://your-api.com/logs",
    apiKey: "your-api-key",
    batchSize: 10,
    flushInterval: 5.0f,
    coroutineRunner: this
);

logRouter.RegisterSink(httpSink);
```

### Sentry Sink
Error tracking and monitoring with Sentry integration.

**Features:**
- Automatic error aggregation
- Stack trace capture
- Environment and release tagging
- Configurable minimum log level (default: Error)

**Usage:**
```csharp
var sentrySink = new SentrySink(
    dsn: "https://key@sentry.io/project-id",
    environment: "production",
    release: "1.0.0",
    minimumLevel: LogLevel.Error,
    coroutineRunner: this
);

logRouter.RegisterSink(sentrySink);
```

### Seq Sink
Structured logging to Seq with CLEF format support.

**Features:**
- CLEF (Compact Log Event Format) serialization
- Rich structured logging
- Batch upload support
- API key authentication

**Usage:**
```csharp
var seqSink = new SeqSink(
    serverUrl: "http://localhost:5341",
    apiKey: "your-api-key",
    batchSize: 10,
    flushInterval: 5.0f,
    coroutineRunner: this
);

logRouter.RegisterSink(seqSink);
```

### Elasticsearch Sink
Send logs to Elasticsearch with ECS compatibility.

**Features:**
- Bulk API for efficient indexing
- ECS (Elastic Common Schema) compatible fields
- Date-based index patterns
- Basic authentication support

**Usage:**
```csharp
var elasticsearchSink = new ElasticsearchSink(
    nodeUrl: "http://localhost:9200",
    indexName: "logsmith-{0:yyyy.MM.dd}",
    username: "elastic",
    password: "changeme",
    batchSize: 50,
    flushInterval: 10.0f,
    coroutineRunner: this
);

logRouter.RegisterSink(elasticsearchSink);
```

## Installation

### Via Package Manager

1. Open Window > Package Manager
2. Click the "+" button
3. Select "Add package from git URL"
4. Enter: `https://github.com/irsiksoftware/LogSmith-Sinks.git`

### Via manifest.json

Add to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.irsiksoftware.logsmith.sinks": "1.0.0"
  }
}
```

## Requirements

- Unity 2022.3 or later
- LogSmith core package (com.irsiksoftware.logsmith) version 1.0.0 or later
- MonoBehaviour instance for coroutine execution (required for HTTP-based sinks)

## Architecture

All sinks implement the `ILogSink` interface from LogSmith core:

```csharp
public interface ILogSink
{
    void Write(LogMessage message);
    void Flush();
    string Name { get; }
}
```

This ensures consistent behavior and easy integration with the LogSmith routing system.

## Best Practices

1. **Use batching** - Configure appropriate batch sizes for HTTP-based sinks to reduce network overhead
2. **Set flush intervals** - Balance between real-time logging and performance
3. **Filter by level** - Use minimum log levels to avoid sending excessive data
4. **Handle disposal** - Always call `Dispose()` on sinks when shutting down
5. **MonoBehaviour lifecycle** - Ensure coroutine runners remain alive during logging operations

## Examples

### Multiple Sinks

```csharp
// Local development: Console + Seq
logRouter.RegisterSink(new ConsoleSink());
logRouter.RegisterSink(new SeqSink("http://localhost:5341", coroutineRunner: this));

// Production: Console + Sentry (errors only) + Elasticsearch
logRouter.RegisterSink(new ConsoleSink());
logRouter.RegisterSink(new SentrySink(dsn, minimumLevel: LogLevel.Error, coroutineRunner: this));
logRouter.RegisterSink(new ElasticsearchSink(nodeUrl, coroutineRunner: this));
```

### Custom HTTP Endpoint

```csharp
var customSink = new HttpSink(
    endpoint: "https://custom-logging-service.com/ingest",
    apiKey: Environment.GetEnvironmentVariable("LOGGING_API_KEY"),
    batchSize: 25,
    flushInterval: 2.0f,
    coroutineRunner: this
);
```

## Testing

The package includes unit tests for all sinks. Run tests via Unity Test Runner:

1. Open Window > General > Test Runner
2. Select "PlayMode" tab
3. Run "IrsikSoftware.LogSmith.Sinks.Tests"

## Versioning

This package follows semantic versioning (SemVer). It maintains compatibility with LogSmith core but is versioned independently to allow rapid iteration on sink implementations.

## License

MIT License - see LICENSE file for details

## Support

For issues, questions, or contributions:
- GitHub Issues: https://github.com/irsiksoftware/LogSmith/issues
- Documentation: https://github.com/irsiksoftware/LogSmith/wiki

## Changelog

See CHANGELOG.md for version history and migration guides.
