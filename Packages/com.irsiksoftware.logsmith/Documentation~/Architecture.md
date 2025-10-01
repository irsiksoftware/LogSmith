# Architecture

LogSmith is built with a clean layered architecture designed for maintainability, testability, and future extensibility.

## Core Components

### 1. LogSmith (Facade)
**Location**: `Runtime/LogSmith.cs`

The main entry point providing static and DI-friendly access:
- `LogSmith.GetLogger(category)` - Static logger retrieval
- `LogSmith.Resolve<T>()` - Service resolution (VContainer-aware)
- `LogSmith.CreateLogger()` - Factory method for custom loggers

### 2. Logger (ILog)
**Location**: `Runtime/Core/Logger.cs`

Implements the `ILog` interface with level-based logging methods:
- `Trace()`, `Debug()`, `Info()`, `Warn()`, `Error()`, `Critical()`
- Context support via dictionary parameter
- Category-based filtering
- Template-based formatting

### 3. LogRouter (ILogRouter)
**Location**: `Runtime/Core/LogRouter.cs`

Central message routing and filtering:
- Routes messages to registered sinks
- Applies global minimum level filtering
- Supports per-category filtering
- Maintains subscriber list for real-time log consumption
- Thread-safe concurrent access

### 4. CategoryRegistry (ICategoryRegistry)
**Location**: `Runtime/Core/CategoryRegistry.cs`

Runtime category management:
- Register/unregister categories
- Per-category metadata (color, minimum level, enabled state)
- Thread-safe operations
- Persistence via LoggingSettings

### 5. MessageTemplateEngine (IMessageTemplateEngine)
**Location**: `Runtime/Core/MessageTemplateEngine.cs`

Message formatting and templating:
- Default template: `[{timestamp}] [{level}] [{category}] {message}`
- Per-category template overrides
- Text and JSON output formats
- Token replacement: `{timestamp}`, `{level}`, `{category}`, `{message}`, `{context}`, `{thread}`, `{stack}`, `{memory}`

## Sinks

### Built-in Sinks
- **ConsoleSink**: Unity Debug.Log output with color coding
- **FileSink**: Rotating file logs with size-based rotation

### Sink Interface (ILogSink)
Custom sinks implement:
```csharp
public interface ILogSink : IDisposable
{
    void Write(LogLevel level, string category, string message, Dictionary<string, object> context);
    void Flush();
}
```

Example custom sinks in `Samples~/CustomSinks/`:
- HTTP sink
- Database sink
- Cloud logging service integration

## Dependency Injection

### VContainer Integration
**Location**: `Runtime/DI/ContainerBuilderExtensions.cs`

```csharp
builder.RegisterLogSmith(); // Registers all LogSmith services
```

Registers:
- `ILog` - Category-aware logger
- `ILogRouter` - Message router
- `ICategoryRegistry` - Category management
- `IMessageTemplateEngine` - Template engine
- Sinks (based on configuration)

### No-DI Fallback
When VContainer is not active, LogSmith uses static instances managed by `UnityLoggingBootstrap`.

## Configuration

### LoggingSettings (ScriptableObject)
**Location**: `Runtime/LoggingSettings.cs`

Stores:
- Category definitions (name, color, min level, enabled)
- Sink configuration (console/file enabled, paths, formats)
- Template overrides
- Live reload settings

### Editor Integration
**Location**: `Editor/LogSmithEditorWindow.cs`

GUI for managing:
- Categories tab
- Sinks tab
- Templates tab

## Platform Abstraction

### IPlatformCapabilities
**Location**: `Runtime/Interfaces/IPlatformCapabilities.cs`

Detects platform capabilities:
- File I/O availability
- Persistent data path access
- Platform name for conditional features

## Backend Adapter

### NativeUnityLoggerAdapter
**Location**: `Runtime/Adapters/NativeUnityLoggerAdapter.cs`

Encapsulates all `com.unity.logging` API calls. This single adapter makes backend swapping straightforward if Unity deprecates the logging package.

## Message Flow

```
User Code (ILog)
    ↓
Logger.Info(message, context)
    ↓
MessageTemplateEngine.Format(template, message, context)
    ↓
LogRouter.Route(level, category, formattedMessage)
    ↓  (filtering by level/category)
    ├→ ConsoleSink.Write()
    ├→ FileSink.Write()
    ├→ CustomSink.Write()
    └→ Subscribers (Debug Overlay)
```

## Thread Safety

All core components are thread-safe:
- LogRouter uses locks for sink/subscriber management
- CategoryRegistry uses locks for metadata access
- MessageTemplateEngine is stateless (thread-safe by design)
- Sinks are responsible for their own thread safety

## Testing Architecture

- **EditMode Tests**: Unit tests for core logic (`Tests/`)
- **PlayMode Tests**: Integration tests for runtime behavior (`Tests/PlayMode/`)
- **Coverage Target**: 100% (enforced via CI gate when active)

## Render Pipeline Support

**Location**: `Runtime.BuiltIn/`, `Runtime.URP/`, `Runtime.HDRP/`

Separate assemblies for render pipeline-specific features:
- Version defines detect RP package availability
- Core package has no SRP dependencies
- Adapters provide RP-specific sinks/hooks

## Future Extension Points

1. **Custom Sinks**: Implement `ILogSink` for new destinations
2. **Custom Templates**: Register templates via `IMessageTemplateEngine`
3. **Custom Categories**: Add via `ICategoryRegistry` at runtime
4. **Backend Replacement**: Swap `NativeUnityLoggerAdapter` for alternate backends
