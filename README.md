# LogSmith

![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black)](https://unity.com)

**A production-grade Unity logging package designed for the Unity Asset Store**

LogSmith is a clean-room Unity logging solution built from scratch, leveraging Unity's native logging system (`com.unity.logging`) with advanced features for professional game development. Designed for Unity 6000.2 with broad compatibility back to Unity 2022.3 LTS.

## Open Source & Freemium Model

**LogSmith Core** (this repository) is **100% free and open source** under the MIT License. It includes:
- Full logging functionality with Console and File sinks
- In-game debug overlay with filtering
- Runtime category management
- VContainer DI integration + Static API fallback
- Message templating engine
- 100% test coverage

**LogSmith Pro** ($4.99 on Unity Asset Store) adds optional sink integrations:
- HTTP/REST Sink (generic endpoint)
- Sentry Sink (error tracking and monitoring)
- Seq Sink (structured logging with CLEF format)
- Elasticsearch Sink (ECS-compatible bulk indexing)
- Priority email support

> **Why freemium?** The core logging functionality should be accessible to everyone. Pro sinks enable integration with enterprise logging infrastructure for teams that need it.

## Quickstart (Under 5 Minutes)

### Option 1: Basic Usage (No DI)

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

### Option 2: With VContainer

```csharp
using IrsikSoftware.LogSmith;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register LogSmith (automatic)
        builder.RegisterLogSmith();

        // Your other registrations...
        builder.RegisterEntryPoint<GameManager>();
    }
}

public class GameManager : IStartable
{
    private readonly ILog _log;

    // Constructor injection
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
3. **Configure**:
   - **Categories Tab**: Add/edit categories, set minimum levels, assign colors
   - **Sinks Tab**: Enable Console/File sinks, configure paths and formats
   - **Templates Tab**: Customize message formats with live preview

### In-Game Debug Overlay

Press **F1** in Play mode to toggle the debug overlay (filters by level, category, search text).

### See Full Examples

Check `Packages/com.irsiksoftware.logsmith/Samples~/` for:
- **BasicUsage**: Simple logging without DI
- **VContainerIntegration**: Full DI setup
- **CustomTemplates**: Message format customization
- **CustomSinks**: Extending with HTTP, database, or cloud sinks

## Key Goals for LogSmith v1.0

1. **Native Unity Logging Backend** - Console + file sinks out of the box; extensible via public sink interface
2. **DI-First with VContainer** - Full VContainer integration with graceful no-DI fallback
3. **Runtime-Managed Categories** - Add/remove/rename categories from UI with per-category minimum levels
4. **Message Templating** - Default + per-category template overrides with text and JSON outputs
5. **100% Test Coverage** - EditMode + PlayMode tests with CI gates across Unity versions
6. **Asset Store Ready** - UPM + .unitypackage packaging with samples and comprehensive docs
7. **Wide Compatibility** - Develop on 6000.2 LTS; minimum 2022.3 LTS; platform-aware features
8. **Future-Proof Architecture** - Clear adapter boundary allows backend swapping if Unity deprecates `com.unity.logging`

## Development Priority Order

Issues should be completed in this dependency-based sequence:

### Phase 1: Foundation (Required First)
1. [#1](../../issues/1) Define Unity version support & package metadata ✅ **COMPLETED**
2. [#6](../../issues/6) UPM package skeleton structure ✅ **COMPLETED**
3. [#7](../../issues/7) Public interfaces & core services ✅ **COMPLETED**
4. [#44](../../issues/44) VContainer integration & no-DI fallback ✅ **COMPLETED**
5. [#3](../../issues/3) CI matrix across Unity versions & platforms ⏸️ **DEFERRED - DO NOT ATTEMPT CLAUDE**

### Phase 2: Core Logging Implementation ✅ **COMPLETED**
5. [#9](../../issues/9) Unity logging bootstrapper ✅
6. [#10](../../issues/10) Console & file sink adapters ✅
7. [#14](../../issues/14) Runtime category registry ✅
8. [#16](../../issues/16) Message templating engine (text & JSON) ✅
9. [#15](../../issues/15) Per-category minimum levels ✅

### Phase 3: Configuration & DI Integration ✅ **COMPLETED**
10. [#17](../../issues/17) LoggingSettings ScriptableObject & provider ✅
11. ~~[#12](../../issues/12) VContainer installer & extensions~~ (superseded by #44)
12. ~~[#13](../../issues/13) No-DI fallback path~~ (superseded by #44)

### Phase 4: Platform & Build Support ✅ **COMPLETED**
13. [#2](../../issues/2) Platform capability flags & conditional compilation ✅
14. [#8](../../issues/8) IL2CPP & stripping configuration ✅
15. [#22](../../issues/22) IL2CPP/AOT validation ✅

### Phase 5: UI & User Experience ✅ **COMPLETED**
16. [#18](../../issues/18) Editor window (categories, sinks, templates) ✅
17. [#19](../../issues/19) In-game debug overlay ✅
18. [#11](../../issues/11) Sink extensibility hooks ✅

### Phase 6: Performance & Quality ✅ **COMPLETED**
19. [#20](../../issues/20) Thread safety & main-thread dispatch ✅
20. [#21](../../issues/21) Performance benchmarks & budget ✅
21. [#23](../../issues/23) Unit tests (core services) ✅
22. [#24](../../issues/24) Integration tests (sinks) ✅
23. [#25](../../issues/25) Overlay UI tests ✅
24. [#26](../../issues/26) CI coverage gate enforcement ✅

### Phase 7: Documentation & Samples ✅ **COMPLETED**
25. [#27](../../issues/27) Samples & quickstart guides ✅
26. [#28](../../issues/28) Comprehensive documentation site ✅

### Phase 8: Render Pipeline Support ✅ **COMPLETED**
27. [#33](../../issues/33) RP adapter assemblies ✅
28. [#34](../../issues/34) Built-in RP adapter ✅
29. [#35](../../issues/35) URP adapter (ScriptableRendererFeature) ✅
30. [#36](../../issues/36) HDRP adapter (Custom Pass) ✅
31. [#37](../../issues/37) Runtime adapter selection ✅
32. [#38](../../issues/38) Editor pipeline warnings ✅
33. [#39](../../issues/39) RP-specific sample scenes ✅
34. [#40](../../issues/40) CI validation across pipelines ✅
35. [#41](../../issues/41) RP setup documentation ✅

### Phase 9: Release Preparation ✅ **COMPLETED**
36. [#4](../../issues/4) Asset Store packaging & submission prep ✅
37. [#29](../../issues/29) Store metadata & compliance ✅
38. [#5](../../issues/5) Backend swap readiness documentation ✅
39. [#30](../../issues/30) Semantic versioning & v1.0 release ✅

### Phase 10: Post-Release Extensions
40. [#31](../../issues/31) Optional sinks pack (HTTP, Sentry, Seq, etc.)
41. [#32](../../issues/32) In-editor live log console

## Cross-Cutting Requirements

- **Unity Compatibility**: 2022.3 LTS, 2023 LTS, 6000.2 LTS
- **Backend**: `com.unity.logging` behind swappable adapters
- **DI Integration**: VContainer-first with identical no-DI fallback
- **Runtime Management**: Categories via editor window with per-category controls
- **Message Templates**: Configurable tokens with JSON output support
- **Test Coverage**: 100% enforced in CI across EditMode + PlayMode
- **Platform Awareness**: WebGL/Switch file sink warnings; graceful feature degradation
- **Asset Store**: UPM + .unitypackage with demo scenes and comprehensive samples

## Package Structure: Assets vs Packages

**Important for contributors:** This project uses Unity's Package Manager structure:

- **`/Packages/com.irsiksoftware.logsmith/`** - The package source code (distributed to users)
  - Contains all C# scripts, interfaces, implementations
  - This is what gets published to the Asset Store or UPM registry
  - Never put scene-specific or project-specific assets here

- **`/Assets/`** - Your local project workspace (NOT distributed)
  - Where you create your own settings instances
  - Where you create test scenes and prefabs
  - Where you put VContainer settings/prefabs for testing
  - This folder demonstrates usage but doesn't ship with the package

**For VContainer setup:** When you create LoggingSettings assets or LoggingLifetimeScope prefabs for testing, put them in `/Assets/Settings/` or `/Assets/Prefabs/`. The package only provides the script definitions - users will create their own instances in their projects.

## Getting Started

LogSmith supports two initialization modes:

### Option 1: Static API (No DI Required)

The simplest way to use LogSmith - just call the static API:

```csharp
using IrsikSoftware.LogSmith;

// Use the default logger
LogSmith.Logger.Info("Hello from LogSmith!");
LogSmith.Logger.Debug("Debug information");
LogSmith.Logger.Warn("Warning message");
LogSmith.Logger.Error("Error occurred");

// Create category-specific loggers
var logger = LogSmith.CreateLogger("MySystem");
logger.Info("Initialized MySystem");
```

### Option 2: VContainer Dependency Injection

For projects using VContainer, LogSmith can be injected:

**Step 1: Create LoggingSettings Asset (in your Assets folder)**
1. In Unity: Right-click in Project → Create → LogSmith → Logging Settings
2. Save it in `/Assets/Settings/` (or wherever you keep project settings)
3. Configure your settings (console output, file logging, etc.)

**Step 2: Create LoggingLifetimeScope GameObject**
1. Create a new GameObject in your first scene
2. Add the `LoggingLifetimeScope` component (from IrsikSoftware.LogSmith.DI namespace)
3. Assign your LoggingSettings asset to the Settings field
4. The component automatically persists with `DontDestroyOnLoad`
5. **Optional:** Save as a prefab in `/Assets/Prefabs/` for reuse

**Step 3 (Optional - VContainer 1.17+ Settings Workflow):**
If you're using VContainer's centralized settings:
1. Save the LoggingLifetimeScope GameObject as a prefab
2. Reference it in your VContainer Settings
3. Otherwise, just keep the GameObject in your first scene

**Step 4: Inject ILog into your classes**

```csharp
using IrsikSoftware.LogSmith;
using VContainer;
using VContainer.Unity;

public class MyGameSystem : IStartable
{
    private readonly ILog _log;

    // Constructor injection
    public MyGameSystem(ILog log)
    {
        _log = log;
    }

    public void Start()
    {
        _log.Info("MyGameSystem started");

        // Create category-specific logger
        var customLogger = _log.WithCategory("CustomCategory");
        customLogger.Debug("Custom debug message");
    }
}
```

**Step 5: Register your systems with VContainer**

```csharp
using VContainer;
using VContainer.Unity;
using IrsikSoftware.LogSmith.DI;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // LogSmith is already registered via LoggingLifetimeScope
        // Just register your own systems
        builder.RegisterEntryPoint<MyGameSystem>();
    }
}
```

### Advanced: Manual VContainer Registration

You can also manually register LogSmith in your own LifetimeScope:

```csharp
using IrsikSoftware.LogSmith.DI;

protected override void Configure(IContainerBuilder builder)
{
    var settings = LoggingSettings.CreateDefault();
    builder.AddLogSmithLogging(settings);

    // Register your systems
    builder.RegisterEntryPoint<MyGameSystem>();
}
```

### Checking DI Status

```csharp
if (LogSmith.IsUsingDependencyInjection)
{
    Debug.Log("LogSmith is using VContainer");
}
else
{
    Debug.Log("LogSmith is using static fallback");
}
```

## Architecture

LogSmith uses a layered architecture designed for extensibility and maintainability:

- **Public Interfaces**: `ILog`, `ILogRouter`, `ILogSink`, `IMessageTemplateEngine`, `ICategoryRegistry`, `ILogConfigProvider`
- **Unity Adapter Layer**: `NativeUnityLoggerAdapter` encapsulates all `com.unity.logging` calls
- **DI Integration**: VContainer bindings with service locator fallback
- **Platform Abstraction**: Conditional compilation and capability detection
- **Render Pipeline Support**: Optional visual debug adapters for Built-in, URP, and HDRP

This design ensures that if Unity deprecates `com.unity.logging`, only the adapter layer needs replacement.

## Contributing

This project follows a strict development process:

### Branch Creation & README Updates

When starting work on any GitHub issue:

1. **Create feature branch**: `git checkout -b feature/gh-##-description` (where ## is the issue number)
2. **Update README immediately**: Mark the issue as "In Progress" in the priority order list above
3. **Commit and push README only**: `git add README.md && git commit -m "Mark issue ### as in progress" && git push -u origin feature/gh-##-description`

### Git Workflow & Code Review

**IMPORTANT**: All implementation work must be reviewed before committing:

1. **Complete the implementation** of all tasks for the issue
2. **Present your work** to the user for review (show file structure, key changes, approach)
3. **Wait for user approval** before making any commits
4. **Only after approval**: Stage, commit, and push changes
5. **Never commit implementation code** without explicit user approval

The only exception is the initial README update marking an issue as "In Progress" - this can be committed immediately.

### Development Requirements

1. All features must be tracked via GitHub issues
2. 100% test coverage is mandatory
3. CI must pass on all supported Unity versions
4. Platform compatibility must be validated
5. Documentation must accompany all public APIs

## License

*License information will be added during release preparation.*