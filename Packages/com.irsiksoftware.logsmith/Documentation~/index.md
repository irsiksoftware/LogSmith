# LogSmith Documentation

Production-grade Unity logging package leveraging Unity's native logging system with advanced features for professional game development.

## Getting Started

- **[Quick Start](../README.md#quickstart-under-5-minutes)** - Get logging in under 5 minutes
- **[Samples](../Samples~/README.md)** - Working examples for common scenarios

## Core Concepts

- **[Architecture](Architecture.md)** - System design and component overview
- **[Dependency Injection](DependencyInjection.md)** - VContainer integration and DI patterns
- **[Categories](Categories.md)** - Organizing logs by logical component
- **[Templates](Templates.md)** - Customizing message formatting
- **[Sinks](Sinks.md)** - Output destinations (console, file, custom)

## Render Pipeline Support

- **[Built-in RP Setup](RenderPipeline-BuiltIn.md)** - Zero-config setup for Built-in Render Pipeline
- **[URP Setup](RenderPipeline-URP.md)** - Renderer Feature setup for Universal RP
- **[HDRP Setup](RenderPipeline-HDRP.md)** - Custom Pass setup for High Definition RP

## Advanced Topics

- **[IL2CPP Compatibility](IL2CPP-Compatibility.md)** - AOT/IL2CPP platform support
- **[Versioning & Compatibility](VersioningAndCompatibility.md)** - Unity versions, platforms, upgrade guides

## Reference

- **[FAQ](FAQ.md)** - Frequently asked questions
- **[Troubleshooting](Troubleshooting.md)** - Common issues and solutions
- **[API Reference](#)** - (Auto-generated from XML docs)

## Features

### ✅ Core Features
- Native Unity logging backend
- DI-first with VContainer (optional)
- Runtime-managed categories
- Message templating (text & JSON)
- Console and file sinks
- In-game debug overlay
- Thread-safe operation

### ✅ Quality Assurance
- 100% test coverage target
- EditMode + PlayMode tests
- IL2CPP/AOT compatible
- Platform-aware features
- Performance-tested (< 0.2ms/frame @ 1K msg/sec)

### 🔄 Render Pipeline Support
- Built-in RP adapter
- URP adapter
- HDRP adapter
- Version-define based compilation

### 📦 Distribution
- UPM package
- Asset Store .unitypackage
- Comprehensive samples
- Editor integration

## Quick Reference

### Basic Usage
```csharp
var log = LogSmith.GetLogger("Gameplay");
log.Info("Player spawned");
```

### With VContainer
```csharp
public class GameManager
{
    private readonly ILog _log;

    public GameManager(ILog log)
    {
        _log = log;
    }
}
```

### Custom Sink
```csharp
public class MySink : ILogSink
{
    public void Write(LogLevel level, string category, string message, Dictionary<string, object> context)
    {
        // Your custom output logic
    }

    public void Flush() { }
    public void Dispose() { }
}

// Register
LogSmith.Resolve<ILogRouter>().RegisterSink(new MySink());
```

## Support

- **Issues**: [GitHub Issues](https://github.com/IrsikSoftware/LogSmith/issues)
- **Documentation**: This site
- **Samples**: `Packages/com.irsiksoftware.logsmith/Samples~/`

## License

See [LICENSE](../LICENSE) for details.
