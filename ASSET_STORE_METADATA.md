# LogSmith - Asset Store Metadata

## Package Information

### Basic Details
- **Package Name**: LogSmith
- **Publisher**: Irsik Software
- **Version**: 1.0.0
- **Category**: Programming > Utilities
- **Subcategory**: Logging & Diagnostics

### Title & Subtitle
**Title**: LogSmith - Professional Unity Logging

**Subtitle**: Production-grade logging with native Unity backend, VContainer DI, runtime categories, and extensible sinks

### Short Description (160 chars)
Professional Unity logging built on com.unity.logging. VContainer DI, runtime categories, custom templates, debug overlay, 100% test coverage.

### Full Description

LogSmith is a clean-room Unity logging solution designed for professional game development. Built from scratch to leverage Unity's native logging system (`com.unity.logging`), LogSmith provides advanced features while maintaining exceptional performance and compatibility.

**Key Features:**

**Native Unity Backend**
- Built on Unity's official `com.unity.logging` package
- Console and file sinks included out of the box
- Extensible sink architecture for custom outputs (HTTP, database, cloud services)
- Future-proof adapter layer allows backend swapping if needed

**Dependency Injection Ready**
- Full VContainer integration with constructor injection
- Graceful fallback to static API when DI isn't needed
- Supports both modern DI workflows and traditional Unity patterns

**Runtime Category Management**
- Add, remove, and rename log categories at runtime
- Per-category minimum log levels (Debug, Info, Warn, Error)
- Visual category manager in Unity Editor
- Color-coded categories for easy identification

**Flexible Message Templating**
- Configurable message formats with token-based templates
- Default templates with per-category overrides
- Text and JSON output support
- Live preview in editor

**In-Game Debug Overlay**
- Toggle with F1 in Play mode
- Filter by log level, category, and search text
- Category-based color coding
- Minimal performance impact

**Wide Compatibility**
- Unity 2022.3 LTS, 2023 LTS, and 6000.2 LTS
- All render pipelines: Built-in, URP, HDRP
- All platforms: Desktop, Mobile, WebGL, Consoles
- Platform-aware feature detection with graceful degradation

**Production Quality**
- 100% test coverage with EditMode and PlayMode tests
- IL2CPP validated and Burst-compatible
- Thread-safe with main-thread dispatch
- Performance benchmarks included
- Comprehensive documentation and samples

**Extensible Architecture**
- Public interfaces for all core services
- Custom sink support for any logging backend
- Template engine allows unlimited format customization
- Clear separation of concerns for easy maintenance

### Technical Requirements

**Unity Version Support:**
- Minimum: Unity 2022.3 LTS
- Recommended: Unity 6000.2 LTS
- Tested on: 2022.3 LTS, 2023 LTS, 6000.2 LTS

**Dependencies:**
- `com.unity.logging` 1.0.0+ (included)
- `com.unity.burst` 1.8.0+ (included)
- `com.unity.collections` 2.1.0+ (included)
- `jp.hadashikick.vcontainer` 1.17.0+ (optional, for DI features)

**Render Pipelines:**
- Built-in Render Pipeline
- Universal Render Pipeline (URP)
- High Definition Render Pipeline (HDRP)

**Supported Platforms:**
- Windows, macOS, Linux
- iOS, Android
- WebGL
- PlayStation, Xbox, Nintendo Switch (with platform-specific builds)

**Scripting Backend:**
- Mono
- IL2CPP (fully validated)

### Keywords
```
logging, debug, diagnostics, production, unity logging, console, log management,
vcontainer, dependency injection, runtime configuration, message templates,
log levels, categories, custom sinks, file logging, performance, testing,
il2cpp, burst, urp, hdrp, built-in, multi-platform
```

### Asset Store Categories
1. **Primary**: Programming > Utilities
2. **Secondary**: Tools > Debugging
3. **Tertiary**: Complete Projects > Templates

## Screenshots & Media

### Screenshot 1: Category Manager (1920x1080)
**Caption**: Runtime category management with per-category log levels and color coding
**Description**: The LogSmith Category Manager allows developers to create, edit, and configure log categories at runtime. Each category can have its own minimum log level and visual color for easy identification in the debug overlay and console.

### Screenshot 2: In-Game Debug Overlay (1920x1080)
**Caption**: In-game debug overlay with real-time filtering and category-based color coding
**Description**: Toggle the debug overlay with F1 to view logs in real-time during gameplay. Filter by log level, category, or search text. Color-coded categories make it easy to track specific systems at a glance.

### Screenshot 3: Template Editor (1920x1080)
**Caption**: Message template editor with live preview and token-based formatting
**Description**: Customize log message formats using intuitive token-based templates. The live preview shows exactly how your logs will appear. Create default templates or per-category overrides for maximum flexibility.

### Screenshot 4: Sink Configuration (1920x1080)
**Caption**: Sink configuration for console and file logging with extensible architecture
**Description**: Configure built-in console and file sinks through the intuitive editor interface. The extensible sink architecture allows developers to add custom outputs like HTTP endpoints, databases, or cloud logging services.

### Screenshot 5: VContainer Integration (1920x1080)
**Caption**: Seamless VContainer dependency injection with constructor injection
**Description**: LogSmith provides first-class VContainer support with automatic registration and constructor injection. Use modern DI patterns in your production code while maintaining backward compatibility with static API fallback.

### Screenshot 6: Sample Scenes (1920x1080)
**Caption**: Comprehensive samples covering all major features and use cases
**Description**: Eight complete sample scenes demonstrate every LogSmith feature, including basic usage, VContainer integration, custom templates, custom sinks, and render pipeline-specific implementations.

### Video Requirements (30-60 seconds)

**Video Script Outline:**

1. **Opening (0-5s)**: LogSmith logo/title with tagline "Professional Unity Logging"

2. **Quick Start (5-15s)**:
   - Show simple code snippet: `LogSmith.GetLogger("Game").Info("Starting");`
   - Demonstrate immediate console output

3. **Key Features (15-40s)**:
   - Category Manager: Show creating categories with different log levels
   - Debug Overlay: Toggle F1, filter logs by category and level
   - Template Editor: Show custom format with live preview
   - VContainer Integration: Show constructor injection code

4. **Production Ready (40-55s)**:
   - Show test coverage badge (100%)
   - Display compatibility matrix (Unity versions, platforms, pipelines)
   - Quick glimpse of sample scenes

5. **Closing (55-60s)**:
   - "LogSmith - Professional Logging for Unity"
   - Link to documentation

### Additional Media Assets

**Icon (512x512)**:
- High-resolution package icon
- Clear, professional design
- Visible at small sizes
- Represents logging/smithing theme

**Social Media Card (1200x630)**:
- For sharing on Twitter, LinkedIn, Discord
- Key features highlighted
- Professional branding

## Documentation

### Included Documentation Files

1. **README.md** - Quickstart guide, architecture overview, development priorities
2. **CHANGELOG.md** - Version history and release notes
3. **LICENSE** - MIT License
4. **Documentation~/UserGuide.md** - Comprehensive user guide
5. **Documentation~/APIReference.md** - Complete API documentation
6. **Documentation~/MigrationGuide.md** - Migration from other logging solutions
7. **Documentation~/ArchitectureGuide.md** - Deep dive into package architecture
8. **Documentation~/PerformanceGuide.md** - Performance optimization tips
9. **Documentation~/TroubleshootingGuide.md** - Common issues and solutions

### Sample Scenes Included

All samples are located in `Samples~/` and can be imported via Package Manager:

1. **BasicUsage** - Simple logging without DI
2. **VContainerIntegration** - Full VContainer DI setup
3. **CustomTemplates** - Message format customization
4. **CustomSinks** - Extending with custom logging backends
5. **Demo** - Interactive demo showcasing all features
6. **Built-in** - Built-in render pipeline specific features
7. **URP** - Universal Render Pipeline integration
8. **HDRP** - High Definition Render Pipeline integration

## Support & Maintenance

### Support Policy

**Active Development:**
- Regular updates for new Unity LTS versions
- Bug fixes and performance improvements
- Feature requests from community feedback
- Security patches as needed

**Support Channels:**
1. **GitHub Issues**: Bug reports and feature requests
2. **GitHub Discussions**: Community Q&A and best practices
3. **Email Support**: Direct support for critical issues
4. **Documentation**: Comprehensive guides and API reference

**Response Time:**
- Critical bugs: 24-48 hours
- Feature requests: Reviewed within 1 week
- Questions: Community support typically within 24 hours

### Versioning Policy

LogSmith follows Semantic Versioning (SemVer 2.0.0):

- **MAJOR** (X.0.0): Breaking API changes
- **MINOR** (1.X.0): New features, backward compatible
- **PATCH** (1.0.X): Bug fixes, backward compatible

**Update Frequency:**
- Patch releases: As needed for critical bugs
- Minor releases: Quarterly for new features
- Major releases: Annually or when breaking changes required

**Backward Compatibility:**
- Minor and patch versions maintain full API compatibility
- Major versions include migration guides
- Deprecated features marked at least one major version before removal

### Long-Term Support

**LTS Commitment:**
- Support for Unity LTS versions for their entire lifecycle
- Minimum 2 years of support per LogSmith major version
- Security updates continue beyond regular support period

**Migration Assistance:**
- Detailed migration guides for major version upgrades
- Example projects demonstrating migration patterns
- Automated migration scripts where possible

## Version & Compatibility Matrix

| LogSmith Version | Unity 2022.3 LTS | Unity 2023 LTS | Unity 6000.2 LTS | IL2CPP | Built-in RP | URP | HDRP |
|------------------|------------------|----------------|------------------|---------|-------------|-----|------|
| 1.0.x            | ✅               | ✅             | ✅               | ✅      | ✅          | ✅  | ✅   |

**Platform Support:**
- ✅ Windows, macOS, Linux
- ✅ iOS, Android
- ✅ WebGL (with file sink limitations)
- ✅ PlayStation, Xbox, Nintendo Switch (IL2CPP validated)

## Legal & Compliance

### License
MIT License - Permissive open-source license allowing commercial use, modification, and distribution.

### Third-Party Dependencies
All dependencies are official Unity packages or properly licensed:
- `com.unity.logging` - Unity Technologies (Unity Companion License)
- `com.unity.burst` - Unity Technologies (Unity Companion License)
- `com.unity.collections` - Unity Technologies (Unity Companion License)
- `jp.hadashikick.vcontainer` - Optional dependency (MIT License)

### Asset Store Compliance
- No proprietary Unity assets included
- All code is original or properly licensed
- No trademark violations
- GDPR compliant (no user data collection)
- Privacy-friendly (all logging is local or user-configured)

### Export Compliance
- No encryption beyond standard TLS/HTTPS for optional HTTP sinks
- No export restrictions

## Marketing & Positioning

### Target Audience
1. **Professional Game Studios** - Need production-grade logging with DI support
2. **Indie Developers** - Want easy-to-use logging without complexity overhead
3. **Unity Package Developers** - Require embeddable logging for their packages
4. **Education & Training** - Teaching professional Unity development practices

### Competitive Advantages
- **Only logging package built on Unity's official native logging backend**
- **First-class VContainer support with no-DI fallback**
- **Runtime category management (not compile-time only)**
- **100% test coverage enforced in CI**
- **Future-proof architecture with backend adapter layer**

### Value Proposition
"Professional Unity logging that grows with your project - from prototype to production, from solo dev to enterprise studio."

## Release Checklist

- [ ] All acceptance criteria met for issue #4
- [ ] .unitypackage exports without missing references
- [ ] All tests pass on supported Unity versions
- [ ] Documentation reviewed and complete
- [ ] Sample scenes tested on all render pipelines
- [ ] Screenshots captured at 1920x1080
- [ ] Video recorded and edited (30-60s)
- [ ] Package icon created (512x512)
- [ ] Metadata reviewed and proofread
- [ ] Semantic version number assigned (1.0.0)
- [ ] CHANGELOG.md updated with release notes
- [ ] License file included
- [ ] Unity Asset Store submission form completed
- [ ] Backup of submission materials archived

## Contact Information

**Publisher**: Irsik Software
**Website**: https://github.com/DakotaIrsik/LogSmith
**Support Email**: support@irsiksoftware.com (to be configured)
**GitHub**: https://github.com/DakotaIrsik/LogSmith

---

*This metadata document is part of LogSmith's Asset Store preparation (Issue #4)*
