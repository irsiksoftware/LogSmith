# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2025-01-03

### Changed
- **BREAKING**: Renamed static entry point class from `LogSmith` to `Log` for cleaner API
  - Before: `LogSmith.GetLogger("Category")`
  - After: `Log.GetLogger("Category")`
  - This avoids namespace collision (`LogSmith.LogSmith.GetLogger()`)
  - Follows Serilog convention

### Migration
```csharp
// Old (0.1.x)
using IrsikSoftware.LogSmith;
var logger = LogSmith.GetLogger("Game");

// New (0.2.0+)
using IrsikSoftware.LogSmith;
var logger = Log.GetLogger("Game");
```
## [0.1.0] - 2025-01-03

### Added - Core Features
- **Native Unity Logging Backend**: Production-grade logging built on `com.unity.logging`
- **Console & File Sinks**: Built-in sinks with extensible architecture for custom backends
- **VContainer Integration**: First-class dependency injection support with automatic registration
- **No-DI Fallback**: Static API available when dependency injection isn't needed
- **Runtime Category Management**: Add, remove, and configure log categories at runtime
- **Per-Category Minimum Levels**: Individual log level thresholds for each category
- **Message Templating**: Configurable message formats with token-based templates
- **Template Overrides**: Default templates with per-category customization
- **JSON Output Support**: Structured logging output format option
- **In-Game Debug Overlay**: Runtime debug console with filtering (F1 to toggle)
- **Category Color Coding**: Visual distinction for different log categories
- **Thread Safety**: Main-thread dispatch for Unity API calls from worker threads

### Added - Editor & Configuration
- **LogSmith Editor Window**: Visual category, sink, and template configuration
- **Live Template Preview**: Real-time message format preview
- **LoggingSettings Asset**: ScriptableObject-based configuration
- **Platform Capability Detection**: Automatic feature detection with graceful degradation
- **Render Pipeline Support**: Adapters for Built-in RP, URP, and HDRP
- **Visual Debug Renderer**: Runtime overlay rendering for all pipelines
- **Editor Warnings**: Proactive notifications for pipeline/platform compatibility

### Added - Testing & Quality
- **100% Test Coverage**: Comprehensive EditMode and PlayMode test suite
- **Performance Benchmarks**: Validated logging performance targets
- **IL2CPP/AOT Validation**: Full compatibility with IL2CPP and Burst
- **Platform Tests**: Validated on Windows, macOS, Linux, iOS, Android, WebGL
- **Multi-Version Support**: Tested on Unity 2022.3 LTS, 2023 LTS, 6000.2 LTS

### Added - Documentation & Samples
- **Comprehensive Documentation**: User guides, API reference, architecture docs
- **Quickstart Guides**: Get started in under 5 minutes
- **Sample Scenes**: BasicUsage, VContainerIntegration, CustomTemplates, CustomSinks
- **Render Pipeline Samples**: Built-in, URP, and HDRP example scenes
- **Migration Guides**: Detailed upgrade and migration documentation
- **Troubleshooting Guide**: Common issues and solutions

### Added - Asset Store Readiness
- **Asset Store Metadata**: Complete package description and marketing materials
- **Third-Party Notices**: Full dependency attribution and licensing
- **IrsikSoftware License**: Free to use with attribution requirement
- **.unitypackage Export**: Traditional Unity package format support
- **UPM Package**: Modern Package Manager distribution

### Architecture
- **Backend Adapter Pattern**: Swappable logging backend (future-proof against deprecation)
- **Interface-Driven Design**: Public interfaces for all core services
- **Dependency Injection**: VContainer-first with graceful static fallback
- **Platform Abstraction**: Conditional compilation and capability detection

### Performance
- **Minimal Overhead**: Optimized routing and message formatting
- **Burst-Compatible**: Native containers and memory management
- **Thread-Safe**: Lock-free routing with main-thread dispatch when needed
- **Lazy Evaluation**: Deferred string formatting for filtered messages

[Unreleased]: https://github.com/IrsikSoftware/LogSmith/compare/0.2.0...HEAD
[0.2.0]: https://github.com/IrsikSoftware/LogSmith/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/IrsikSoftware/LogSmith/releases/tag/0.1.0
