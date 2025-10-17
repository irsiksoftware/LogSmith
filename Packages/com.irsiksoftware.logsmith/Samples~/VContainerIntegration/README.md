# VContainer Integration Sample

This sample demonstrates how to use LogSmith with VContainer dependency injection.

## Prerequisites

Install VContainer from the Package Manager or add to manifest.json:
```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
```

## Quick Start

1. Copy `GameLifetimeScope.cs` and `GameManager.cs` to your project
2. Create an empty GameObject named "GameLifetimeScope"
3. Attach the `GameLifetimeScope` script
4. Enter Play mode

## Key Concepts

### Registering LogSmith

```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterLogSmith();
}
```

This single line registers all LogSmith services with VContainer.

### Constructor Injection

```csharp
public class GameManager
{
    private readonly ILog _log;

    public GameManager(ILog log)  // Automatic injection
    {
        _log = log;
    }
}
```

### Per-Category Loggers

If you need loggers for specific categories:

```csharp
public class NetworkManager
{
    private readonly ILog _log;

    public NetworkManager(ILogFactory logFactory)
    {
        _log = logFactory.CreateLogger("Network");
    }
}
```

### Scoped Loggers

LogSmith loggers are singletons per category - they're automatically shared across all consumers.

## Benefits of DI Approach

1. **Testability**: Easy to mock `ILog` in unit tests
2. **Explicit Dependencies**: Constructor shows what each class needs
3. **Lifetime Management**: VContainer handles creation and disposal
4. **Consistent Pattern**: Same injection style as other dependencies

## Next Steps

- See `BasicUsage` sample for non-DI usage
- See `CustomSinks` for extending LogSmith with external systems
