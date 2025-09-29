# LogSmith

**A production-grade Unity logging package designed for the Unity Asset Store**

LogSmith is a clean-room Unity logging solution built from scratch, leveraging Unity's native logging system (`com.unity.logging`) with advanced features for professional game development.

## Features

- **Native Unity Logging Backend** - Console + file sinks out of the box; extensible via public sink interface
- **DI-First with VContainer** - Full VContainer integration with graceful no-DI fallback
- **Runtime-Managed Categories** - Add/remove/rename categories from UI with per-category minimum levels
- **Message Templating** - Default + per-category template overrides with text and JSON outputs
- **Wide Compatibility** - Supports Unity 2022.3 LTS and newer versions
- **Future-Proof Architecture** - Clear adapter boundary allows backend swapping if Unity deprecates `com.unity.logging`

## Installation

### Via Package Manager

1. Open the Package Manager in Unity
2. Click the `+` button and select "Add package from git URL"
3. Enter: `https://github.com/DakotaIrsik/LogSmith.git?path=/Packages/com.irsiksoftware.logsmith`

### Via manifest.json

Add this line to your `Packages/manifest.json` dependencies:

```json
{
  "dependencies": {
    "com.irsiksoftware.logsmith": "https://github.com/DakotaIrsik/LogSmith.git?path=/Packages/com.irsiksoftware.logsmith"
  }
}
```

## Requirements

- Unity 2022.3 LTS or newer
- Unity Logging package (`com.unity.logging`)

## Quick Start

*Documentation and samples will be available once the foundation is complete.*

## License

MIT License - See LICENSE file for details.