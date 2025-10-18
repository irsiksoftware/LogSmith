# HTTP Sink Example

This sample demonstrates how to configure and use the HTTP Sink to send logs to a remote HTTP endpoint.

## Overview

The HTTP Sink allows you to send logs to any HTTP endpoint that accepts POST requests. This is useful for:
- Custom log aggregation services
- Internal logging infrastructure
- Testing and debugging

## Setup Instructions

### 1. Import the Sample

Import this sample through the Unity Package Manager:
1. Open Package Manager (Window > Package Manager)
2. Select "LogSmith Sinks" package
3. Expand "Samples" section
4. Click "Import" next to "HTTP Sink"

### 2. Configure HTTP Endpoint

Before running the example, you need to set up an HTTP endpoint to receive logs:

**Option A: Local Test Server (Simple)**
```bash
# Using Python
python -m http.server 5000

# Or using Node.js
npx http-server -p 5000
```

**Option B: Production Service**
Configure the `httpEndpoint` field in the example script to point to your production logging service.

### 3. Run the Example

1. Open the `HttpSinkExample.unity` scene
2. Update the `Http Endpoint` field in the Inspector if needed
3. Press Play
4. Use the on-screen buttons to test different log levels

## Configuration Options

The example script exposes several configuration options:

- **Http Endpoint**: The URL to send logs to (e.g., `http://localhost:5000/logs`)
- **Minimum Level**: Minimum log level to send (Verbose, Debug, Information, Warning, Error, Fatal)
- **Batch Size Limit**: Number of log events to batch before sending (default: 50)
- **Batch Period Seconds**: Time in seconds to wait before sending batch (default: 2)

## Code Example

```csharp
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

// Configure logger with HTTP Sink
var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Http(
        requestUri: "http://localhost:5000/logs",
        restrictedToMinimumLevel: LogEventLevel.Information,
        batchPostingLimit: 50,
        period: System.TimeSpan.FromSeconds(2)
    )
    .CreateLogger();

LogSmith.Initialize(logger);

// Use LogSmith as normal
LogSmith.Information("Hello from HTTP Sink!");
LogSmith.Warning("This is a warning");
LogSmith.Error("This is an error");
```

## HTTP Endpoint Format

The HTTP Sink sends logs as JSON in the following format:

```json
{
  "events": [
    {
      "Timestamp": "2025-10-18T14:30:12.123Z",
      "Level": "Information",
      "MessageTemplate": "Hello from HTTP Sink!",
      "RenderedMessage": "Hello from HTTP Sink!",
      "Properties": {}
    }
  ]
}
```

## Troubleshooting

**Logs not appearing at endpoint:**
- Verify the HTTP endpoint is accessible
- Check Unity Console for connection errors
- Ensure firewall allows outbound connections
- Verify the endpoint accepts POST requests with JSON body

**Performance issues:**
- Increase `batchSizeLimit` to send fewer, larger batches
- Increase `batchPeriodSeconds` to batch logs over longer periods
- Raise `minimumLevel` to filter out verbose logs

## Additional Resources

- [HTTP Sink Documentation](https://github.com/serilog-contrib/serilog-sinks-http)
- [LogSmith Documentation](https://github.com/irsiksoftware/LogSmith)
- [Serilog Documentation](https://serilog.net/)
