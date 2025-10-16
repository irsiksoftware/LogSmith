# Changelog

All notable changes to LogSmith Optional Sinks will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-16

### Added
- Initial release of LogSmith Optional Sinks package
- HTTP/REST sink with batching and JSON serialization
- Sentry sink for error tracking and monitoring
- Seq sink with CLEF format support
- Elasticsearch sink with ECS compatibility
- Unit tests for all sink implementations
- Comprehensive documentation and usage examples

### Features
- All sinks support async delivery via Unity coroutines
- Configurable batch sizes and flush intervals
- Authentication support (API keys, basic auth)
- Rich context and metadata preservation
- Thread-safe operation with lock-based synchronization

### Requirements
- Unity 2022.3 or later
- LogSmith core package v1.0.0 or later
