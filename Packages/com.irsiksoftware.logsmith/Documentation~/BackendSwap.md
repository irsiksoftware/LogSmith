# Backend Swap Readiness

LogSmith is architected to isolate the Unity native logging backend (`com.unity.logging`) behind a thin adapter layer. This design ensures that if Unity deprecates `com.unity.logging` or you need to swap to a different logging backend, the changes required are minimal and localized.

## Architecture Overview

LogSmith uses the **Adapter Pattern** to decouple core logging logic from backend-specific implementations:

```
┌─────────────────────────────────────────────────────────────────┐
│  Public API Layer                                                │
│  - ILog, ILogRouter, ILogSink, IMessageTemplateEngine           │
│  - LogSmith (facade), Logger, LogRouter, CategoryRegistry       │
└───────────────────────┬─────────────────────────────────────────┘
                        │
                        │ Interfaces (stable contracts)
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│  Core Logic Layer                                                │
│  - Message routing and filtering                                │
│  - Category management                                           │
│  - Template formatting                                           │
│  - Sink orchestration                                            │
└───────────────────────┬─────────────────────────────────────────┘
                        │
                        │ Adapter abstraction
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│  Adapter Layer (BACKEND-SPECIFIC)                                │
│  - NativeUnityLoggerAdapter.cs                                   │
│  - UnityLoggingBootstrap.cs                                      │
└─────────────────────────────────────────────────────────────────┘
                        │
                        │ Unity package dependency
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│  Unity Native Logging                                            │
│  - com.unity.logging package                                     │
│  - Unity.Logging.Log APIs                                        │
└─────────────────────────────────────────────────────────────────┘
```

## Key Principle: Interface-Driven Design

All core LogSmith logic depends on **public interfaces**, not concrete implementations:

- `ILog` - Logger interface used by client code
- `ILogRouter` - Message routing interface
- `ILogSink` - Sink interface for custom output targets
- `IMessageTemplateEngine` - Message formatting interface
- `ICategoryRegistry` - Category management interface
- `ILogConfigProvider` - Configuration provider interface

**Result**: Core logic has ZERO direct dependency on `com.unity.logging`. All Unity-specific calls are isolated in the adapter layer.

## Adapter Layer: The Backend Boundary

### 1. NativeUnityLoggerAdapter

**Location**: `Runtime/Adapters/NativeUnityLoggerAdapter.cs`

This static class encapsulates **all** direct calls to `com.unity.logging`:

```csharp
// BEFORE (not used in LogSmith):
Unity.Logging.Log.Info("message"); // Direct Unity API call

// AFTER (LogSmith pattern):
NativeUnityLoggerAdapter.Write(LogLevel.Info, "category", "message");
```

**Responsibilities**:
- Initialize Unity's logging system
- Translate LogSmith `LogLevel` to Unity's `UnityLogLevel`
- Format and write messages via Unity's native logger

**All Unity API usage**:
```csharp
using Unity.Logging;                    // ✓ Only used here
using UnityLog = Unity.Logging.Log;     // ✓ Aliased to prevent leakage

// Initialize Unity logger
UnityLog.Logger = new Logger(new LoggerConfig()...);

// Write messages
UnityLog.Verbose(message);
UnityLog.Debug(message);
UnityLog.Info(message);
UnityLog.Warning(message);
UnityLog.Error(message);
UnityLog.Fatal(message);
```

### 2. UnityLoggingBootstrap

**Location**: `Runtime/Core/UnityLoggingBootstrap.cs`

Manages lifecycle and configuration of the Unity logging backend:

**Responsibilities**:
- Initialize `NativeUnityLoggerAdapter`
- Configure global and per-category minimum log levels
- Register sinks with Unity's sink system
- Handle settings reloads
- Dispose/cleanup on shutdown

**Unity-specific dependencies**:
```csharp
using Unity.Logging;
using Unity.Logging.Sinks;

// Configuration
LoggerConfig config = new LoggerConfig()
    .MinimumLevel.Set(unityLevel)
    .WriteTo.UnityDebugLog();
```

## Swapping the Backend

If Unity deprecates `com.unity.logging` or you need an alternative backend, follow these steps:

### Step 1: Identify Required Changes

**Files to modify**:
1. `Runtime/Adapters/NativeUnityLoggerAdapter.cs` - Replace Unity API calls
2. `Runtime/Core/UnityLoggingBootstrap.cs` - Replace initialization logic
3. `package.json` - Update dependency from `com.unity.logging` to new backend

**Files that remain unchanged**:
- All core logic files (`Logger.cs`, `LogRouter.cs`, `CategoryRegistry.cs`, etc.)
- All interface definitions (`ILog.cs`, `ILogRouter.cs`, etc.)
- All editor UI code
- All tests (may need test doubles for new backend)
- All public API contracts

### Step 2: Replace NativeUnityLoggerAdapter

Create a new adapter (e.g., `CustomLoggerAdapter.cs`) matching the same method signatures:

```csharp
namespace IrsikSoftware.LogSmith.Adapters
{
    internal static class CustomLoggerAdapter
    {
        private static bool _initialized;

        // Initialize your backend
        public static void Initialize()
        {
            if (_initialized) return;

            // Your backend initialization here
            // Example: CustomLoggingLibrary.Setup();

            _initialized = true;
        }

        // Translate LogSmith LogLevel to your backend's log levels
        public static void Write(LogLevel level, string category, string message)
        {
            var formattedMessage = $"[{category}] {message}";

            switch (level)
            {
                case LogLevel.Trace:
                    // CustomLoggingLibrary.Trace(formattedMessage);
                    break;
                case LogLevel.Debug:
                    // CustomLoggingLibrary.Debug(formattedMessage);
                    break;
                case LogLevel.Info:
                    // CustomLoggingLibrary.Info(formattedMessage);
                    break;
                case LogLevel.Warn:
                    // CustomLoggingLibrary.Warn(formattedMessage);
                    break;
                case LogLevel.Error:
                    // CustomLoggingLibrary.Error(formattedMessage);
                    break;
                case LogLevel.Critical:
                    // CustomLoggingLibrary.Fatal(formattedMessage);
                    break;
            }
        }
    }
}
```

### Step 3: Update Bootstrap

Modify `UnityLoggingBootstrap.cs` (or create `CustomLoggingBootstrap.cs`):

```csharp
public class CustomLoggingBootstrap : IDisposable
{
    public CustomLoggingBootstrap(ILogRouter router, ILogConfigProvider config)
    {
        // Initialize custom backend
        CustomLoggerAdapter.Initialize();

        // Configure minimum levels
        var settings = config.GetSettings();
        SetGlobalMinimumLevel(settings.MinimumLevel);

        // Register sinks (if backend supports)
        if (settings.EnableConsoleSink)
        {
            var consoleSink = new ConsoleSink(router);
            router.RegisterSink(consoleSink);
        }

        if (settings.EnableFileSink)
        {
            var fileSink = new FileSink(router, settings.LogFilePath);
            router.RegisterSink(fileSink);
        }
    }

    private void SetGlobalMinimumLevel(LogLevel level)
    {
        // Translate to custom backend's level setting
        // CustomLoggingLibrary.SetMinLevel(level);
    }

    public void Dispose()
    {
        // Cleanup custom backend
        // CustomLoggingLibrary.Shutdown();
    }
}
```

### Step 4: Update DI Registrations

Update `ContainerBuilderExtensions.cs` to use the new bootstrap:

```csharp
// Replace:
builder.Register<UnityLoggingBootstrap>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .AsSelf();

// With:
builder.Register<CustomLoggingBootstrap>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .AsSelf();
```

### Step 5: Update package.json

Remove Unity logging dependency:

```json
// Remove:
"com.unity.logging": "1.0.0",

// Add (if needed):
"com.custom.logging": "2.0.0"
```

### Step 6: Run Tests

LogSmith's comprehensive test suite validates behavior independently of the backend. After swapping, run:

```bash
./run-tests.ps1
```

All tests should pass if the adapter maintains equivalent behavior.

## Code Map: Backend Touch Points

### Files with Direct Unity Logging Dependencies

| File | Lines | Purpose | Swap Impact |
|------|-------|---------|-------------|
| `NativeUnityLoggerAdapter.cs` | 61 | Write messages to Unity logger | **HIGH** - Full rewrite |
| `UnityLoggingBootstrap.cs` | ~150 | Initialize and configure Unity backend | **HIGH** - Full rewrite |
| `package.json` | 1 | Dependency declaration | **MEDIUM** - Update dependency |

### Files with Zero Unity Logging Dependencies

| Component | Files | Lines | Swap Impact |
|-----------|-------|-------|-------------|
| Public Interfaces | 6 files | ~200 | **ZERO** |
| Core Logic | `Logger.cs`, `LogRouter.cs`, `CategoryRegistry.cs`, `MessageTemplateEngine.cs` | ~800 | **ZERO** |
| Sinks | `ConsoleSink.cs`, `FileSink.cs` | ~400 | **ZERO** |
| Editor UI | `LogSmithEditorWindow.cs`, tabs | ~1200 | **ZERO** |
| Tests | 305 tests across 15 test files | ~4000 | **MINIMAL** (test doubles only) |
| Documentation | 15+ markdown files | ~3000 | **MINIMAL** (update backend references) |

**Total**: Less than 5% of the codebase touches the Unity logging backend.

## Example: Swapping to Serilog

Hypothetical example using Serilog as the backend:

### 1. SerilogAdapter.cs

```csharp
using Serilog;
using Serilog.Events;

namespace IrsikSoftware.LogSmith.Adapters
{
    internal static class SerilogAdapter
    {
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            _initialized = true;
        }

        public static void Write(LogLevel level, string category, string message)
        {
            var logEvent = TranslateLevel(level);
            Serilog.Log.Logger.Write(logEvent, "[{Category}] {Message}", category, message);
        }

        private static LogEventLevel TranslateLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Info => LogEventLevel.Information,
                LogLevel.Warn => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}
```

### 2. Update Bootstrap

```csharp
public SerilogBootstrap(ILogRouter router, ILogConfigProvider config)
{
    SerilogAdapter.Initialize();
    // ... rest of bootstrap logic
}
```

### 3. Update package.json

```json
"dependencies": {
    "com.serilog.unity": "2.10.0"  // Hypothetical
}
```

**Result**: LogSmith now uses Serilog with ~200 lines changed.

## Deprecation Hedge Strategy

If Unity announces `com.unity.logging` deprecation:

1. **Assessment** (1-2 hours): Identify alternative backend (NLog, Serilog, custom)
2. **Adapter Implementation** (4-8 hours): Write new adapter matching current contract
3. **Testing** (2-4 hours): Run full test suite, fix adapter bugs
4. **Documentation** (1-2 hours): Update backend references in docs
5. **Migration Guide** (2-3 hours): Write migration guide for users

**Total Effort**: 10-20 hours for complete backend swap.

## Benefits of This Architecture

1. **Minimal Blast Radius**: Backend changes isolated to <5% of codebase
2. **Stable Public API**: Users' code never breaks due to backend changes
3. **Testability**: Core logic tested independently of backend
4. **Future-Proof**: Protected against Unity package deprecation
5. **Flexibility**: Easy to add alternative backends (e.g., cloud logging)
6. **Maintainability**: Clear separation of concerns

## Verification Checklist

To verify LogSmith maintains backend isolation:

- [ ] Search codebase for `using Unity.Logging` - should only appear in adapter layer
- [ ] Verify all `ILog` usages are interface-based, not concrete `Logger`
- [ ] Check that tests mock/stub the adapter, not Unity APIs directly
- [ ] Confirm DI registrations use interfaces, not concrete adapters
- [ ] Validate that editor UI depends on `ILogRouter`, not `NativeUnityLoggerAdapter`

**Current Status**: ✅ All checks pass. LogSmith is fully adapter-isolated.

## Conclusion

LogSmith's architecture ensures that **alternate backends require changes only in the adapter layer**. The public API, core logic, editor UI, and tests remain untouched. This design provides long-term stability and flexibility, protecting users from breaking changes due to Unity backend deprecation.

---

**References**:
- [Architecture.md](Architecture.md) - Full architecture overview
- [FAQ.md](FAQ.md) - Frequently asked questions
- [VersioningAndCompatibility.md](VersioningAndCompatibility.md) - Version support policy
