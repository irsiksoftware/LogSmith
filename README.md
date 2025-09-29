# LogSmith

**A production-grade Unity logging package designed for the Unity Asset Store**

LogSmith is a clean-room Unity logging solution built from scratch, leveraging Unity's native logging system (`com.unity.logging`) with advanced features for professional game development. Designed for Unity 6000.2 with broad compatibility back to Unity 2022.3 LTS.

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
3. [#7](../../issues/7) Public interfaces & core services 🚧 **IN PROGRESS**
4. [#3](../../issues/3) CI matrix across Unity versions & platforms

### Phase 2: Core Logging Implementation
5. [#9](../../issues/9) Unity logging bootstrapper
6. [#10](../../issues/10) Console & file sink adapters
7. [#14](../../issues/14) Runtime category registry
8. [#16](../../issues/16) Message templating engine (text & JSON)
9. [#15](../../issues/15) Per-category minimum levels

### Phase 3: Configuration & DI Integration
10. [#17](../../issues/17) LoggingSettings ScriptableObject & provider
11. [#12](../../issues/12) VContainer installer & extensions
12. [#13](../../issues/13) No-DI fallback path

### Phase 4: Platform & Build Support
13. [#2](../../issues/2) Platform capability flags & conditional compilation
14. [#8](../../issues/8) IL2CPP & stripping configuration
15. [#22](../../issues/22) IL2CPP/AOT validation

### Phase 5: UI & User Experience
16. [#18](../../issues/18) Editor window (categories, sinks, templates)
17. [#19](../../issues/19) In-game debug overlay
18. [#11](../../issues/11) Sink extensibility hooks

### Phase 6: Performance & Quality
19. [#20](../../issues/20) Thread safety & main-thread dispatch
20. [#21](../../issues/21) Performance benchmarks & budget
21. [#23](../../issues/23) Unit tests (core services)
22. [#24](../../issues/24) Integration tests (sinks)
23. [#25](../../issues/25) Overlay UI tests
24. [#26](../../issues/26) CI coverage gate enforcement

### Phase 7: Documentation & Samples
25. [#27](../../issues/27) Samples & quickstart guides
26. [#28](../../issues/28) Comprehensive documentation site

### Phase 8: Render Pipeline Support
27. [#33](../../issues/33) RP adapter assemblies
28. [#34](../../issues/34) Built-in RP adapter
29. [#35](../../issues/35) URP adapter (ScriptableRendererFeature)
30. [#36](../../issues/36) HDRP adapter (Custom Pass)
31. [#37](../../issues/37) Runtime adapter selection
32. [#38](../../issues/38) Editor pipeline warnings
33. [#39](../../issues/39) RP-specific sample scenes
34. [#40](../../issues/40) CI validation across pipelines
35. [#41](../../issues/41) RP setup documentation

### Phase 9: Release Preparation
36. [#4](../../issues/4) Asset Store packaging & submission prep
37. [#29](../../issues/29) Store metadata & compliance
38. [#5](../../issues/5) Backend swap readiness documentation
39. [#30](../../issues/30) Semantic versioning & v1.0 release

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

## Getting Started

*Coming soon - samples and quickstart guides will be available once the foundation is complete.*

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
3. **Commit and push**: `git add README.md && git commit -m "Mark issue ### as in progress" && git push -u origin feature/gh-##-description`

### Development Requirements

1. All features must be tracked via GitHub issues
2. 100% test coverage is mandatory
3. CI must pass on all supported Unity versions
4. Platform compatibility must be validated
5. Documentation must accompany all public APIs

## License

*License information will be added during release preparation.*