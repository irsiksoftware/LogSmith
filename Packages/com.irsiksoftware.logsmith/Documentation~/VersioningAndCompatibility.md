# Versioning & Compatibility

## Semantic Versioning

LogSmith follows [SemVer 2.0.0](https://semver.org/):

- **MAJOR**: Incompatible API changes
- **MINOR**: Backward-compatible functionality
- **PATCH**: Backward-compatible bug fixes

Example: `1.2.3` = Major 1, Minor 2, Patch 3

## Unity Compatibility

| LogSmith Version | Unity Version | Status |
|------------------|---------------|--------|
| 1.x | 6000.2+ LTS | Recommended |
| 1.x | 2023.3 LTS | Supported |
| 1.x | 2022.3 LTS | Minimum |

### Tested Versions
- Unity 6000.2.5f1 (primary development)
- Unity 2023.3 LTS (compatibility testing)
- Unity 2022.3 LTS (minimum version)

## Package Dependencies

| Package | Version | Required |
|---------|---------|----------|
| com.unity.logging | 1.3+ | ✅ Yes |
| Unity.Burst | Latest | ✅ Yes |
| Unity.Collections | Latest | ✅ Yes |
| VContainer | 1.13+ | ❌ Optional |

### Render Pipeline Packages (Optional)
- com.unity.render-pipelines.universal (URP support)
- com.unity.render-pipelines.high-definition (HDRP support)
- Built-in RP works without additional packages

## Platform Support

| Platform | Status | Notes |
|----------|--------|-------|
| Windows | ✅ Supported | All features |
| macOS | ✅ Supported | All features |
| Linux | ✅ Supported | All features |
| Android | ✅ Supported | File sink uses persistentDataPath |
| iOS | ✅ Supported | File sink uses persistentDataPath |
| WebGL | ⚠️ Limited | Console sink only (no file I/O) |
| Consoles | ⚠️ Untested | Should work, pending testing |

## Build Pipeline Support

- **Mono**: ✅ Fully supported
- **IL2CPP**: ✅ Fully supported and tested
- **AOT**: ✅ Compatible (no dynamic code generation)

## API Stability

### Stable APIs (1.x)
Won't change in minor/patch releases:
- `ILog` interface
- `Log.GetLogger()`
- `ILogSink` interface
- `LogLevel` enum
- Core routing and filtering

### Experimental APIs
May change in minor releases:
- Render pipeline adapters
- Advanced template features
- Internal routing optimizations

## Deprecation Policy

1. **Deprecation Notice**: Marked with `[Obsolete]` attribute
2. **Grace Period**: Minimum 1 minor version
3. **Removal**: Next major version

Example:
```csharp
// Version 1.2: Deprecated
[Obsolete("Use GetLogger(category) instead")]
public static ILog CreateLogger(string category) { ... }

// Version 1.3: Still available, deprecated
// Version 2.0: Removed
```

## Upgrade Guide

### From 1.0 to 1.1+
- No breaking changes
- New features backward-compatible

### Future Major Versions
- Migration guides provided in CHANGELOG
- Automated migration tools where possible

## Backend Compatibility

LogSmith uses `com.unity.logging` as backend. If Unity deprecates this:

1. Adapter layer (`NativeUnityLoggerAdapter`) isolates backend
2. Swap adapter implementation
3. Public APIs remain stable

## Testing & CI

### Coverage Requirements
- Minimum: 100% (enforced when CI active)
- EditMode tests: Core logic
- PlayMode tests: Integration, runtime behavior

### CI Matrix
- Unity versions: 2022.3, 2023.3, 6000.2
- Platforms: Windows, macOS, Linux
- Build types: Mono, IL2CPP

## Release Process

1. Tag version: `git tag 1.2.3`
2. Generate release notes from CHANGELOG
3. Publish to GitHub Releases
4. Update Asset Store package
5. Update UPM registry

## Support Policy

- **Current Major Version**: Full support (bug fixes, features)
- **Previous Major Version**: Security/critical fixes only
- **Older Versions**: No support

Example (hypothetical):
- v2.x: Full support
- v1.x: Critical fixes for 6 months after v2.0
- v0.x: No support

## Contact

- **Issues**: [GitHub Issues](https://github.com/IrsikSoftware/LogSmith/issues)
- **Discussions**: GitHub Discussions
- **Email**: Support contact in Asset Store listing
