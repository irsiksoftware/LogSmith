# Sentry Sink Example

This sample demonstrates how to configure and use the Sentry Sink for error tracking and exception monitoring.

## Overview

The Sentry Sink integrates with [Sentry.io](https://sentry.io) to provide:
- Real-time error tracking
- Exception monitoring with stack traces
- Breadcrumb trails for debugging context
- Release tracking and environment separation
- Performance monitoring capabilities

## Setup Instructions

### 1. Create Sentry Account

1. Sign up at [sentry.io](https://sentry.io) (free tier available)
2. Create a new project (select "Unity" or "C#" as the platform)
3. Copy your DSN (Data Source Name) from the project settings

### 2. Import the Sample

Import this sample through the Unity Package Manager:
1. Open Package Manager (Window > Package Manager)
2. Select "LogSmith Sinks" package
3. Expand "Samples" section
4. Click "Import" next to "Sentry Sink"

### 3. Configure Sentry DSN

1. Open the `SentrySinkExample.unity` scene
2. Select the `SentrySinkExample` GameObject
3. In the Inspector, paste your Sentry DSN into the `Sentry Dsn` field
4. Configure environment and release version as needed

### 4. Run the Example

1. Press Play
2. Use the on-screen buttons to test different error scenarios
3. Check your Sentry dashboard to see captured errors

## Configuration Options

The example script exposes several configuration options:

- **Sentry Dsn**: Your unique Sentry Data Source Name
- **Minimum Level**: Minimum log level to send to Sentry (typically Warning or Error)
- **Environment**: Environment name (Development, Staging, Production)
- **Release**: Version identifier for tracking errors across releases

## Code Example

```csharp
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

// Configure logger with Sentry Sink
var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Sentry(o =>
    {
        o.Dsn = "https://your-key@o123456.ingest.sentry.io/123456";
        o.Environment = "Production";
        o.Release = "1.0.0";
        o.MinimumEventLevel = LogEventLevel.Warning;
        o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
        o.AttachStacktrace = true;
    })
    .CreateLogger();

LogSmith.Initialize(logger);

// Log errors and exceptions
try
{
    // Your code here
}
catch (Exception ex)
{
    LogSmith.Error(ex, "Something went wrong: {Context}", contextInfo);
}
```

## Error Tracking Features

### Exception Tracking
The sample demonstrates several exception scenarios:
- Null reference exceptions
- Index out of range exceptions
- Custom error messages
- Fatal errors

### Breadcrumbs
Breadcrumbs provide context leading up to an error:
```csharp
LogSmith.Information("Player started level {Level}", 5);
LogSmith.Debug("Loading assets for level");
// ... later when error occurs ...
LogSmith.Error(ex, "Failed to load level assets");
// Sentry will show the breadcrumb trail
```

### Structured Data
Attach custom data to errors:
```csharp
LogSmith.Error(
    "Player save failed for user {UserId} {UserName}",
    12345,
    "PlayerOne"
);
```

## Sentry Dashboard Features

Once errors are captured, you can use Sentry's dashboard to:
- View stack traces with source file and line numbers
- See breadcrumb trails leading to errors
- Track error frequency and affected users
- Set up alerts for critical errors
- Group similar errors together
- Mark errors as resolved and track regressions

## Best Practices

1. **Environment Separation**: Use different DSNs or environments for development vs production
2. **Minimum Level**: Set to `Warning` or `Error` in production to avoid noise
3. **Release Tracking**: Update the release version with each build
4. **Breadcrumbs**: Log important events at Debug/Info level for context
5. **Structured Data**: Include relevant context (user IDs, game state) in error logs
6. **Rate Limiting**: Sentry automatically rate-limits errors to prevent quota exhaustion

## Troubleshooting

**Errors not appearing in Sentry:**
- Verify DSN is correct (copy-paste from Sentry dashboard)
- Check Unity Console for Sentry initialization errors
- Ensure internet connectivity
- Verify firewall allows connections to sentry.io
- Check Sentry project quota hasn't been exceeded

**Too many errors being sent:**
- Increase `minimumLevel` to Error or Fatal
- Use Sentry's rate limiting and filtering features
- Add custom filters to ignore specific error types

**Missing context in errors:**
- Lower `minimumBreadcrumbLevel` to capture more context
- Add more structured properties to log messages
- Use Sentry's context APIs for user/game state

## Additional Resources

- [Sentry Documentation](https://docs.sentry.io/)
- [Serilog Sentry Sink](https://github.com/serilog-contrib/serilog-sinks-sentry)
- [LogSmith Documentation](https://github.com/irsiksoftware/LogSmith)
- [Sentry Unity SDK](https://docs.sentry.io/platforms/unity/)
