# Message Templates

Templates control log message formatting.

## Default Template

```
[{timestamp}] [{level}] [{category}] {message}
```

Output:
```
[2025-10-01T12:34:56] [INFO] [Gameplay] Player spawned
```

## Available Tokens

| Token | Description | Example |
|-------|-------------|---------|
| `{timestamp}` | ISO 8601 timestamp | `2025-10-01T12:34:56` |
| `{timestamp:FORMAT}` | Custom format | `{timestamp:HH:mm:ss}` → `12:34:56` |
| `{level}` | Log level | `INFO`, `WARN`, `ERROR` |
| `{category}` | Category name | `Gameplay`, `AI` |
| `{message}` | Log message | User-provided text |
| `{context}` | Context key-values | `userId=123, score=45` |
| `{context:json}` | JSON context | `{"userId":123,"score":45}` |
| `{thread}` | Thread name+ID | `Main(1)` |
| `{stack}` | Stack trace | Full call stack |
| `{memory}` | Memory usage | `1024 MB` |

## Per-Category Templates

### Via Editor
1. Window → LogSmith → Settings
2. Templates tab
3. Select category
4. Edit template
5. Live preview updates

### Via Code
```csharp
var engine = LogSmith.Resolve<IMessageTemplateEngine>();
engine.SetCategoryTemplate("Network", "[{timestamp:HH:mm:ss}] [{level}] {message}");
```

## JSON Format

```csharp
engine.SetCategoryTemplate("API", @"{
  ""timestamp"": ""{timestamp}"",
  ""level"": ""{level}"",
  ""category"": ""{category}"",
  ""message"": ""{message}"",
  ""context"": {context:json}
}");
```

Output:
```json
{
  "timestamp": "2025-10-01T12:34:56",
  "level": "INFO",
  "category": "API",
  "message": "Request completed",
  "context": {"statusCode":200,"duration":125}
}
```

## Context Tokens

Use context dictionary keys as tokens:

```csharp
log.Info("Request completed", new Dictionary<string, object>
{
    ["statusCode"] = 200,
    ["duration"] = 125
});
```

Template:
```
[{level}] {message} (status={statusCode}, took={duration}ms)
```

Output:
```
[INFO] Request completed (status=200, took=125ms)
```

## Best Practices

1. **Text for files**: Human-readable, grep-friendly
2. **JSON for parsing**: Machine-readable, structured logging
3. **Include timestamp**: Essential for debugging
4. **Minimize overhead**: Avoid `{stack}` in production unless needed
5. **Test templates**: Use live preview to verify formatting
