# Basic Usage Sample

This sample demonstrates the simplest way to use LogSmith without dependency injection.

## Quick Start

1. Copy `BasicLoggingExample.cs` to your project
2. Attach it to any GameObject
3. Enter Play mode
4. Check the Console for log messages

## Key Concepts

### Getting a Logger

```csharp
var log = Log.GetLogger("CategoryName");
```

### Logging at Different Levels

```csharp
log.Trace("Detailed diagnostic info");
log.Debug("Debug information");
log.Info("General information");
log.Warn("Warning messages");
log.Error("Error conditions");
log.Critical("Critical failures");
```

### Category Management

Categories are automatically created when first used. Configure them via:
- **Window → LogSmith → Settings → Categories Tab**

### Structured Logging

LogSmith captures structured data for better filtering and analysis:

```csharp
log.Info("Player scored {Score} points", playerScore);
log.Warn("Health low: {Health}/{MaxHealth}", currentHealth, maxHealth);
```

## Next Steps

- **VContainerIntegration**: Learn how to inject loggers via DI
- **CustomTemplates**: Customize message formatting
- **CustomSinks**: Send logs to external systems
