# LogSmith

**A production-grade Unity logging package designed for the Unity Asset Store**

LogSmith is a clean-room Unity logging solution built from scratch, leveraging Unity's native logging system (`com.unity.logging`) with advanced features for professional game development.

## Features

- **Native Unity Logging Backend** - Console + file sinks out of the box; extensible via public sink interface
- **DI-First with VContainer** - Full VContainer integration with graceful no-DI fallback
- **Runtime-Managed Categories** - Add/remove/rename categories from UI with per-category minimum levels
- **Message Templating** - Default + per-category template overrides with text and JSON outputs
- **Wide Compatibility** - Supports Unity 2022.3 LTS and newer versions
- **Future-Proof Architecture** - Clear adapter boundary allows backend swapping if Unity deprecates `com.unity.logging`

## Installation

### Via Package Manager

1. Open the Package Manager in Unity
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/DakotaIrsik/LogSmith.git?path=/Packages/com.irsiksoftware.logsmith`

### Via manifest.json

Add this line to your `Packages/manifest.json` dependencies:

```json
{
  "dependencies": {
    "com.irsiksoftware.logsmith": "https://github.com/DakotaIrsik/LogSmith.git?path=/Packages/com.irsiksoftware.logsmith"
  }
}
```

## Requirements

- Unity 2022.3 LTS or newer
- Unity Logging package (`com.unity.logging`)

## Quick Start

### Basic Usage (No DI)

```csharp
using IrsikSoftware.LogSmith;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Get a logger for this category
        var log = LogSmith.GetLogger("Gameplay");

        // Log at different levels
        log.Info("Game starting");
        log.Warn("Low memory detected");
        log.Error("Failed to load level");
    }
}
```

### With VContainer

```csharp
using IrsikSoftware.LogSmith;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register LogSmith
        builder.RegisterLogSmith();

        // Your other registrations...
        builder.RegisterEntryPoint<GameManager>();
    }
}

public class GameManager : IStartable
{
    private readonly ILog _log;

    public GameManager(ILog log)
    {
        _log = log;
    }

    public void Start()
    {
        _log.Info("Game manager started");
    }
}
```

### Configuration

1. **Create Settings**: Right-click in Project → Create → LogSmith → Logging Settings
2. **Open Editor**: Window → LogSmith → Settings
3. **Configure categories, sinks, and message templates**

### In-Game Debug Overlay

Press **F1** in Play mode to toggle the debug overlay.

### Full Documentation

See [Documentation~](Documentation~/index.md) for comprehensive guides covering:
- Architecture & dependency injection
- Category management & templates
- Custom sinks & render pipeline support
- IL2CPP compatibility & troubleshooting

## License

MIT License - See LICENSE file for details.