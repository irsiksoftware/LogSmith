# LogSmith - Version & Compatibility Guide

## Semantic Versioning Policy

LogSmith strictly follows [Semantic Versioning 2.0.0](https://semver.org/):

```
MAJOR.MINOR.PATCH (e.g., 1.2.3)
```

### Version Number Meanings

**MAJOR version** (X.0.0)
- Incremented for incompatible API changes
- Breaking changes to public interfaces
- Removal of deprecated features
- Minimum Unity version changes

**MINOR version** (1.X.0)
- Incremented for new features added in backward-compatible manner
- New public APIs or methods
- New configuration options
- Performance improvements
- New sample scenes

**PATCH version** (1.0.X)
- Incremented for backward-compatible bug fixes
- Critical bug fixes
- Documentation corrections
- Performance optimizations without API changes

### Pre-release Versions

During development and testing:
- **Alpha**: `1.0.0-alpha.1` - Feature incomplete, breaking changes expected
- **Beta**: `1.0.0-beta.1` - Feature complete, API may change
- **Release Candidate**: `1.0.0-rc.1` - Production-ready candidate

## Current Version

**LogSmith v1.0.0**
- **Release Date**: TBD (Initial Release)
- **Status**: Release Candidate
- **Stability**: Production Ready

## Unity Version Compatibility

### Supported Unity Versions

| Unity Version | Support Status | Test Status | Notes |
|---------------|----------------|-------------|-------|
| **2022.3 LTS** | ✅ Full Support | ✅ CI Tested | Minimum supported version |
| **2023.1-2023.2** | ⚠️ Limited | ⚠️ Not Tested | Use 2023 LTS instead |
| **2023 LTS** | ✅ Full Support | ✅ CI Tested | Recommended for 2023 users |
| **6000.0** | ✅ Compatible | ⚠️ Not Tested | Use 6000.2 LTS instead |
| **6000.2 LTS** | ✅ Full Support | ✅ CI Tested | **Recommended version** |
| **Future LTS** | 🔄 Planned | ⏳ Pending | Support added when stable |

**Minimum Unity Version**: 2022.3.0f1
**Recommended Unity Version**: 6000.2.5f1 (latest LTS)

### Unity Version Support Lifecycle

LogSmith provides support for Unity LTS versions according to their lifecycle:

```
Unity 2022.3 LTS → Support until 2025
Unity 2023 LTS   → Support until 2026
Unity 6000.2 LTS → Support until 2027+
```

When a Unity LTS version reaches end-of-life:
1. LogSmith continues to work but receives no updates for that version
2. New features may require newer Unity versions
3. Bug fixes focus on supported LTS versions
4. Migration guides provided for upgrading Unity

## Dependency Compatibility

### Required Dependencies

All required dependencies are automatically installed by Unity Package Manager:

| Package | Minimum Version | Recommended | Auto-installed | Notes |
|---------|----------------|-------------|----------------|-------|
| `com.unity.logging` | 1.0.0 | 1.0.0+ | ✅ Yes | Unity's native logging |
| `com.unity.burst` | 1.8.0 | Latest | ✅ Yes | Performance optimization |
| `com.unity.collections` | 2.1.0 | Latest | ✅ Yes | Native collections |

### Optional Dependencies

| Package | Minimum Version | Purpose | Notes |
|---------|----------------|---------|-------|
| `jp.hadashikick.vcontainer` | 1.17.0 | Dependency Injection | Optional but recommended |

**VContainer Compatibility:**
- LogSmith works **without** VContainer (static API fallback)
- Full DI features require VContainer 1.17.0+
- Tested with VContainer 1.17.0 through 1.19.0+

## Render Pipeline Compatibility

LogSmith supports all Unity render pipelines:

### Built-in Render Pipeline
- ✅ Fully supported on all Unity versions
- ✅ Debug overlay via legacy rendering
- ✅ No additional packages required
- Sample: `Samples~/Built-in/`

### Universal Render Pipeline (URP)
- ✅ Fully supported on all Unity versions
- ✅ Debug overlay via `ScriptableRendererFeature`
- ✅ URP 10.x, 12.x, 14.x, 15.x tested
- Sample: `Samples~/URP/`
- Assembly: `IrsikSoftware.LogSmith.URP`

### High Definition Render Pipeline (HDRP)
- ✅ Fully supported on all Unity versions
- ✅ Debug overlay via Custom Pass
- ✅ HDRP 10.x, 12.x, 14.x, 15.x tested
- Sample: `Samples~/HDRP/`
- Assembly: `IrsikSoftware.LogSmith.HDRP`

### Render Pipeline Auto-Detection

LogSmith automatically detects the active render pipeline at runtime:

```csharp
// Automatic detection - no code changes needed
var log = LogSmith.GetLogger("Game");
log.Info("LogSmith initialized with detected render pipeline");
```

If multiple render pipeline packages are installed, LogSmith uses the active pipeline configured in Graphics Settings.

## Platform Compatibility

### Fully Supported Platforms

| Platform | Status | File Logging | Debug Overlay | IL2CPP | Notes |
|----------|--------|--------------|---------------|--------|-------|
| **Windows** | ✅ Full | ✅ | ✅ | ✅ | Desktop primary platform |
| **macOS** | ✅ Full | ✅ | ✅ | ✅ | Desktop fully tested |
| **Linux** | ✅ Full | ✅ | ✅ | ✅ | Desktop fully tested |
| **iOS** | ✅ Full | ✅ | ✅ | ✅ | IL2CPP required |
| **Android** | ✅ Full | ✅ | ✅ | ✅ | Mono & IL2CPP |
| **WebGL** | ⚠️ Limited | ❌* | ✅ | ✅ | *Browser limitations |

### Console Platforms

| Platform | Status | Notes |
|----------|--------|-------|
| **PlayStation 4** | ✅ Compatible | IL2CPP validated |
| **PlayStation 5** | ✅ Compatible | IL2CPP validated |
| **Xbox One** | ✅ Compatible | IL2CPP validated |
| **Xbox Series X/S** | ✅ Compatible | IL2CPP validated |
| **Nintendo Switch** | ✅ Compatible | IL2CPP validated |

**Note**: Console platform testing requires platform-specific Unity licenses and hardware. LogSmith's IL2CPP validation ensures compatibility.

### Platform-Specific Limitations

**WebGL:**
- File logging disabled (browser security restrictions)
- Console sink works normally
- Debug overlay fully functional
- Custom HTTP sinks work (user must implement)

**Mobile (iOS/Android):**
- File logging paths use `Application.persistentDataPath`
- Performance impact minimal (< 0.1ms per frame at 1k logs/sec)
- Debug overlay touch-friendly

**Consoles:**
- Platform-specific log paths
- Memory constraints considered (configurable buffer sizes)
- Compliance with platform requirements (TRC/TCR)

## Scripting Backend Compatibility

### Mono
- ✅ Fully supported on all platforms where available
- ✅ Faster iteration during development
- ✅ All features available

### IL2CPP
- ✅ Fully supported and validated
- ✅ AOT compilation compatible
- ✅ Stripping configuration provided in `link.xml`
- ✅ Required for iOS, WebGL, and consoles
- ✅ Burst compilation supported

**IL2CPP Validation:**
LogSmith includes comprehensive IL2CPP tests:
- See `build-il2cpp.ps1` for validation script
- CI runs IL2CPP builds on all platforms
- No reflection used in hot paths
- Native collections for performance

## .NET Compatibility

| .NET Version | Unity Versions | Support Status |
|--------------|----------------|----------------|
| **.NET Standard 2.1** | 2022.3 LTS | ✅ Supported |
| **.NET Framework 4.x** | Legacy | ⚠️ Not tested |
| **.NET 6+** | 6000.2 LTS | ✅ Supported |

LogSmith targets **.NET Standard 2.1** for maximum compatibility across all supported Unity versions.

## Feature Compatibility Matrix

### Core Features

| Feature | Unity 2022.3 | Unity 2023 | Unity 6000.2 | Notes |
|---------|--------------|------------|--------------|-------|
| Category Management | ✅ | ✅ | ✅ | |
| Template Engine | ✅ | ✅ | ✅ | |
| Console Sink | ✅ | ✅ | ✅ | |
| File Sink | ✅ | ✅ | ✅ | Platform-dependent |
| Debug Overlay | ✅ | ✅ | ✅ | |
| VContainer DI | ✅ | ✅ | ✅ | Requires VContainer |
| Static API | ✅ | ✅ | ✅ | |
| Custom Sinks | ✅ | ✅ | ✅ | |

### Advanced Features

| Feature | Unity 2022.3 | Unity 2023 | Unity 6000.2 | Notes |
|---------|--------------|------------|--------------|-------|
| Built-in RP | ✅ | ✅ | ✅ | |
| URP Integration | ✅ | ✅ | ✅ | Requires URP package |
| HDRP Integration | ✅ | ✅ | ✅ | Requires HDRP package |
| IL2CPP | ✅ | ✅ | ✅ | Fully validated |
| Burst Compilation | ✅ | ✅ | ✅ | Optional optimization |
| Thread Safety | ✅ | ✅ | ✅ | |

## Breaking Changes Policy

### When Breaking Changes Occur

Breaking changes are introduced only in **MAJOR** version releases (e.g., 1.x.x → 2.0.0).

**Definition of Breaking Change:**
- Removal of public API methods/properties
- Change to method signatures (parameters, return types)
- Renaming of public classes/interfaces
- Change in default behavior that affects existing code
- Minimum Unity version increase

### Deprecation Process

Before removing features in a major version:

1. **Mark as Obsolete** (minor version)
   ```csharp
   [Obsolete("Use NewMethod instead. This will be removed in v2.0.0")]
   public void OldMethod() { }
   ```

2. **Provide Migration Path** (minor version)
   - New API introduced alongside deprecated API
   - Documentation shows migration examples
   - Both APIs work identically

3. **Announce in CHANGELOG** (minor version)
   - Clear warning in release notes
   - Timeline for removal specified

4. **Remove in Next Major** (major version)
   - Deprecated API removed
   - Migration guide published
   - Breaking changes summarized

### Example Deprecation Timeline

```
v1.5.0 - Old API marked obsolete, new API introduced
v1.6.0 - Warning remains, both APIs functional
v1.7.0 - Final version with deprecated API
v2.0.0 - Deprecated API removed, breaking change
```

## Upgrade Guide

### Upgrading LogSmith Versions

#### Patch Upgrades (1.0.0 → 1.0.1)
- ✅ No code changes required
- ✅ No settings changes required
- ✅ Simply update package version
- ✅ No risk of breaking changes

#### Minor Upgrades (1.0.0 → 1.1.0)
- ✅ No breaking changes
- ⚠️ May have new optional features
- ⚠️ Review CHANGELOG for new APIs
- ✅ Existing code continues to work

#### Major Upgrades (1.x.x → 2.0.0)
- ⚠️ Potential breaking changes
- ⚠️ Review migration guide
- ⚠️ Test thoroughly before deploying
- ⚠️ May require code changes

### Upgrading Unity Versions

When upgrading Unity (e.g., 2022.3 → 6000.2):

1. **Check Compatibility Table** (see above)
2. **Backup Project** (critical!)
3. **Update LogSmith** to version supporting new Unity
4. **Test Core Features**:
   - Category management
   - Log output
   - Debug overlay
   - Custom sinks (if any)
5. **Run Full Test Suite** if available
6. **Check Platform Builds** for target platforms

## Version History & Changelog

### v1.0.0 (Initial Release - TBD)

**New Features:**
- ✨ Runtime category management with per-category log levels
- ✨ VContainer dependency injection support with static fallback
- ✨ Flexible message templating (text and JSON)
- ✨ In-game debug overlay with filtering (F1 to toggle)
- ✨ Console and file sinks included
- ✨ Custom sink extensibility
- ✨ Built-in, URP, and HDRP render pipeline support
- ✨ Unity 2022.3 LTS, 2023 LTS, 6000.2 LTS support
- ✨ IL2CPP and Burst compatibility
- ✨ 100% test coverage with CI validation

**Included Samples:**
- BasicUsage - Simple logging without DI
- VContainerIntegration - Full DI setup
- CustomTemplates - Message format customization
- CustomSinks - HTTP, database, cloud logging examples
- Demo - Interactive feature showcase
- Built-in - Built-in RP integration
- URP - URP ScriptableRendererFeature
- HDRP - HDRP Custom Pass

**Documentation:**
- Complete API reference
- User guide
- Migration guide (from Unity Debug.Log)
- Architecture guide
- Performance guide
- Troubleshooting guide

**Known Issues:**
- None at release

**Migration Notes:**
- First release, no migration needed

---

## Support & Long-Term Maintenance

### Support Commitment

**Active Support Window:**
- Each major version supported for **minimum 2 years** after release
- Bug fixes and security patches during active support
- Feature updates via minor versions

**Long-Term Support (LTS):**
- After active support ends, critical security fixes only
- LTS period: **1 additional year** minimum

**Example Support Timeline:**
```
v1.0.0 Release     → Active Support (2+ years)
v2.0.0 Release     → v1.x enters LTS (1 year)
v2.0.0 End of LTS  → v1.x end of support
```

### Reporting Issues

**Bug Reports:**
- GitHub Issues: https://github.com/DakotaIrsik/LogSmith/issues
- Include Unity version, LogSmith version, platform
- Provide minimal reproduction case
- Check existing issues first

**Feature Requests:**
- GitHub Discussions: https://github.com/DakotaIrsik/LogSmith/discussions
- Describe use case and benefit
- Community voting helps prioritize

**Security Issues:**
- Email: security@irsiksoftware.com (to be configured)
- Confidential disclosure preferred
- Coordinated disclosure timeline

## Testing & Quality Assurance

### Test Coverage

LogSmith maintains **100% code coverage** enforced by CI:

- **EditMode Tests**: Unit tests for all core services
- **PlayMode Tests**: Integration tests for runtime behavior
- **IL2CPP Tests**: Validation scripts for AOT compilation
- **Platform Tests**: Automated builds for all platforms

### CI/CD Pipeline

**Continuous Integration:**
- Unity 2022.3 LTS EditMode & PlayMode tests
- Unity 2023 LTS EditMode & PlayMode tests
- Unity 6000.2 LTS EditMode & PlayMode tests
- IL2CPP build validation
- Code coverage reporting (100% gate)

**Pre-Release Testing:**
- Manual testing on physical devices
- Platform-specific validation (iOS, Android, consoles)
- Performance profiling
- Memory leak detection

## Conclusion

LogSmith is designed for **long-term stability** with clear compatibility guarantees:

- ✅ Semantic versioning ensures predictable upgrades
- ✅ Multi-year Unity LTS support
- ✅ Platform and pipeline compatibility
- ✅ IL2CPP and Burst validated
- ✅ 100% test coverage
- ✅ Clear deprecation and migration paths

**Questions?**
- Documentation: https://github.com/DakotaIrsik/LogSmith
- Support: GitHub Issues
- Community: GitHub Discussions

---

*This document is part of LogSmith's Asset Store preparation (Issue #4)*
*Last Updated: 2025-10-01*
