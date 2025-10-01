# Dependency Injection

LogSmith supports both DI-based and non-DI usage patterns.

## VContainer Integration

### Setup

```csharp
using IrsikSoftware.LogSmith;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register LogSmith services
        builder.RegisterLogSmith();

        // Your game services
        builder.RegisterEntryPoint<GameManager>();
        builder.Register<PlayerController>(Lifetime.Scoped);
    }
}
```

### Constructor Injection

```csharp
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

### What Gets Registered

`builder.RegisterLogSmith()` registers:

| Interface | Implementation | Lifetime |
|-----------|---------------|----------|
| `ILog` | `Logger` | Scoped (per-category) |
| `ILogRouter` | `LogRouter` | Singleton |
| `ICategoryRegistry` | `CategoryRegistry` | Singleton |
| `IMessageTemplateEngine` | `MessageTemplateEngine` | Singleton |
| `ILogConfigProvider` | `ScriptableObjectConfigProvider` | Singleton |

### Custom Configuration

```csharp
protected override void Configure(IContainerBuilder builder)
{
    // Manual registration with custom settings
    var settings = Resources.Load<LoggingSettings>("CustomLogSettings");
    builder.RegisterInstance(settings);
    builder.RegisterLogSmith();
}
```

## No-DI Usage

### Static Access

```csharp
using IrsikSoftware.LogSmith;

public class GameManager : MonoBehaviour
{
    private ILog _log;

    private void Awake()
    {
        _log = LogSmith.GetLogger("Gameplay");
    }

    private void Start()
    {
        _log.Info("Game started");
    }
}
```

### Direct Service Access

```csharp
// Access router directly
var router = LogSmith.Resolve<ILogRouter>();
router.SetGlobalMinimumLevel(LogLevel.Debug);

// Access category registry
var registry = LogSmith.Resolve<ICategoryRegistry>();
registry.RegisterCategory("MyCategory", LogLevel.Info, Color.green);
```

## Mixing DI and Non-DI

LogSmith detects VContainer automatically:

```csharp
// In VContainer scope - uses DI
public class DIService
{
    public DIService(ILog log) // Injected
    {
        log.Info("DI service created");
    }
}

// Outside VContainer scope - uses static
public class StaticService : MonoBehaviour
{
    private void Start()
    {
        var log = LogSmith.GetLogger("Static");
        log.Info("Static service started");
    }
}
```

Both approaches share the same underlying router, sinks, and configuration.

## Lifetime Scopes

### Category-Based Loggers

Each `ILog` instance is scoped to a category:

```csharp
public class PlayerController
{
    private readonly ILog _log;

    // Automatically gets "PlayerController" category
    public PlayerController(ILog log)
    {
        _log = log;
    }
}
```

### Singleton Services

Router, registry, and template engine are singletons, ensuring:
- All logs route through same pipeline
- Categories are globally consistent
- Templates apply uniformly

## Testing with DI

### EditMode Tests

```csharp
[Test]
public void TestWithMockLog()
{
    var mockLog = Substitute.For<ILog>();
    var service = new MyService(mockLog);

    service.DoSomething();

    mockLog.Received().Info("Expected message");
}
```

### PlayMode Tests

```csharp
[UnityTest]
public IEnumerator TestWithRealDI()
{
    var builder = new ContainerBuilder();
    builder.RegisterLogSmith();
    var container = builder.Build();

    var log = container.Resolve<ILog>();
    log.Info("Test message");

    yield return null;
}
```

## Best Practices

1. **Use DI in production code** - Better testability and decoupling
2. **Use static in utilities** - Quick scripts, prototypes, or editor tools
3. **Inject ILog, not Logger** - Depend on interface for flexibility
4. **Register once per scope** - Call `RegisterLogSmith()` only in root scopes
5. **Category naming** - Use class name or logical component name

## Troubleshooting

### Issue: "ILog not registered"
**Solution**: Ensure `builder.RegisterLogSmith()` is called in your LifetimeScope.

### Issue: Different loggers in DI vs static
**Cause**: Both use the same router, but logger instances may differ.
**Solution**: This is expected. Messages still route correctly.

### Issue: Settings not applying
**Cause**: Custom settings not registered before `RegisterLogSmith()`.
**Solution**: Register settings instance first:
```csharp
builder.RegisterInstance(customSettings);
builder.RegisterLogSmith();
```
