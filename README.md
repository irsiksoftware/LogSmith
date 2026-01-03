# LogSmith

![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen)
![License](https://img.shields.io/badge/license-IrsikSoftware-orange)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black)](https://unity.com)

> Enterprise-grade logging for every Unity developer. Free to use, built for the AI age.

## What You Can Do

- **Filter the noise** - See only the log categories you care about. Hide the rest with a click.
- **Debug in-game** - Press F1 to see filtered, color-coded logs without leaving play mode.
- **Tame AI-generated code** - When Claude, Gemini, or GPT write `[Category] - Message` logs, LogSmith routes and organizes them automatically.
- **Ship everywhere** - IL2CPP, WebGL, consoles, mobile. If Unity runs there, LogSmith works there.
- **Control at runtime** - Add, remove, and reconfigure categories on the fly. No recompile needed.
- **Integrate with your stack** - Works standalone or with VContainer dependency injection.

## Installation

Add to your Unity project via Package Manager:

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL**
3. Enter: `https://github.com/IrsikSoftware/LogSmith.git?path=Packages/com.irsiksoftware.logsmith`

## The Magic Moment

You're debugging a complex AI-driven NPC. Your AI assistant just generated 200 lines of code with logs scattered across `[AI]`, `[Pathfinding]`, `[Combat]`, and `[Animation]` categories.

Press F1. Toggle off everything except `[AI]`. Suddenly, you see exactly what the decision-making logic is doing. No console spam. No squinting. Just clarity.

*This is what debugging should feel like.*

## Who This Is For

**Solo and indie developers** who want the structured logging that big studios have - without paying enterprise prices.

**AI-assisted development workflows** where AI tools generate categorized logs that need organizing, not drowning in.

**Teams shipping to console and mobile** who need IL2CPP-safe, platform-tested logging that doesn't break in production builds.

## LogSmith Pro

The free version is fully-featured. LogSmith Pro adds integrations for teams with existing logging infrastructure:

- HTTP/REST endpoints
- Sentry error tracking
- Seq structured logging
- Elasticsearch indexing

[Available on the Unity Asset Store](https://assetstore.unity.com/)

## License

LogSmith is free to use in any project, including commercial projects. See [LICENSE](LICENSE) for full terms.

**Attribution Required:** Projects using LogSmith must credit "IrsikSoftware.LogSmith" in credits or documentation.

**No Modifications:** The source code may not be modified or used to create derivative works.
