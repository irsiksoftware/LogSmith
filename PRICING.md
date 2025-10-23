# LogSmith Pricing & License

## Freemium Model

LogSmith uses a freemium model to balance accessibility with sustainability:

### LogSmith (Free)
**Price:** FREE
**License:** MIT (Open Source)
**Source:** GitHub + Unity Asset Store

**What's Included:**
- ✅ Full LogSmith core functionality
- ✅ Console and File sinks
- ✅ In-game debug overlay with filtering
- ✅ Runtime category management
- ✅ VContainer DI integration
- ✅ Static API fallback (no DI required)
- ✅ Message templating engine
- ✅ All render pipeline support (Built-in, URP, HDRP)
- ✅ 100% test coverage
- ✅ Complete documentation and samples
- ✅ Commercial use allowed
- ✅ No limitations, no watermarks
- ✅ Full source code access

**Perfect For:**
- Solo developers and indie studios
- Prototyping and early development
- Projects that don't need external logging services
- Learning professional Unity development patterns
- Contributing to open source

### LogSmith Pro ($14.99)
**Price:** $14.99 (one-time purchase)
**License:** Unity Asset Store EULA
**Source:** Unity Asset Store only

**What's Included:**
- ✅ Everything in LogSmith (Free) +
- ✅ **HTTP/REST Sink** - Generic HTTP endpoint logging
- ✅ **Sentry Sink** - Error tracking and monitoring
- ✅ **Seq Sink** - Structured logging with CLEF format
- ✅ **Elasticsearch Sink** - ECS-compatible bulk indexing
- ✅ Priority email support
- ✅ Early access to new Pro features

**Perfect For:**
- Professional studios with existing logging infrastructure
- Live-service games needing centralized error tracking
- Teams using Sentry, Seq, or Elasticsearch
- Projects requiring HTTP endpoint logging
- Production deployments with advanced telemetry needs

---

## Frequently Asked Questions

### Can I use LogSmith (Free) in commercial projects?
**Yes!** LogSmith is MIT licensed. Use it in any project, commercial or personal, with no restrictions.

### Do I need LogSmith Pro?
**Only if you need external logging integrations.** The free version is fully functional and production-ready. Pro adds optional sinks for services like Sentry, Seq, and Elasticsearch.

### Can I build my own custom sinks without Pro?
**Absolutely!** LogSmith (Free) includes the full `ILogSink` interface. You can implement custom sinks for any backend. The Pro sinks are pre-built, tested implementations for popular services.

### Is the free version "crippled" or limited?
**No.** The free version is the full LogSmith experience. There are no artificial limitations, nag screens, or watermarks. Pro is purely additive - it adds integrations you may or may not need.

### Can I upgrade from Free to Pro later?
**Yes.** Install LogSmith Pro from the Asset Store, and it will add the sinks package alongside your existing free installation. No migration required.

### What happens if I uninstall Pro but keep Free?
Your project will work fine with the free version. You'll just lose access to the optional sinks (HTTP, Sentry, Seq, Elasticsearch). All core logging continues to work.

### Why charge for Pro sinks?
Building and maintaining production-ready integrations with external services requires significant effort:
- Testing against real Sentry/Seq/Elasticsearch instances
- Handling auth, retries, batching, and edge cases
- Keeping up with protocol changes
- Providing support for integration issues

The $14.99 price supports this ongoing work while keeping the core free for everyone.

### Can I contribute to the open source version?
**Yes!** LogSmith (Free) is open source on GitHub. Contributions are welcome via pull requests. See CONTRIBUTING.md for guidelines.

### What about enterprise licenses?
For teams needing custom licenses, priority support, or custom sink development, contact support@irsiksoftware.com.

---

## Comparison Chart

| Feature | Free | Pro |
|---------|------|-----|
| **Core Functionality** | | |
| Console & File Sinks | ✅ | ✅ |
| Debug Overlay | ✅ | ✅ |
| Category Management | ✅ | ✅ |
| Message Templates | ✅ | ✅ |
| VContainer DI | ✅ | ✅ |
| Static API | ✅ | ✅ |
| All Render Pipelines | ✅ | ✅ |
| 100% Test Coverage | ✅ | ✅ |
| **Pro Sinks** | | |
| HTTP/REST Sink | ❌ | ✅ |
| Sentry Sink | ❌ | ✅ |
| Seq Sink | ❌ | ✅ |
| Elasticsearch Sink | ❌ | ✅ |
| **Support** | | |
| Community Support (GitHub) | ✅ | ✅ |
| Priority Email Support | ❌ | ✅ |
| **License** | | |
| Open Source (MIT) | ✅ | ❌ |
| Commercial Use | ✅ | ✅ |
| Source Code Access | ✅ | Asset Store EULA |

---

## Asset Store Links

- **LogSmith (Free)**: [Coming Soon - Unity Asset Store]
- **LogSmith Pro ($14.99)**: [Coming Soon - Unity Asset Store]
- **GitHub (Open Source)**: https://github.com/DakotaIrsik/LogSmith

---

## Support

- **Free Version**: GitHub Issues and Discussions
- **Pro Version**: Priority email support + GitHub
- **Enterprise**: Custom support contracts available

For questions, email: support@irsiksoftware.com
