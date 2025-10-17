# Custom Templates Sample

This sample demonstrates how to customize message formatting with LogSmith templates.

## Overview

Templates control how log messages appear in the console and files. LogSmith supports:
- **Default Template**: Applied to all categories
- **Per-Category Overrides**: Custom formats for specific categories
- **Output Formats**: Text (human-readable) and JSON (machine-parsable)

## Configuration via Editor

1. Open **Window → LogSmith → Settings**
2. Go to **Templates Tab**
3. Edit the default template or add category-specific overrides
4. Use the **Live Preview** to see results instantly

## Template Tokens

Available tokens (case-insensitive):

| Token | Description | Example Output |
|-------|-------------|----------------|
| `{Timestamp}` | ISO 8601 timestamp | `2025-10-01T09:15:30.123Z` |
| `{Level}` | Log level name | `INFO`, `WARN`, `ERROR` |
| `{Category}` | Category name | `Gameplay`, `Network` |
| `{Message}` | The log message | `Player scored 100 points` |
| `{NewLine}` | Platform newline | `\n` or `\r\n` |

## Example Templates

### Default (Detailed)
```
[{Timestamp}] [{Level}] {Category}: {Message}
```
**Output**: `[2025-10-01T09:15:30.123Z] [INFO] Gameplay: Player scored 100 points`

### Minimal
```
{Level}: {Message}
```
**Output**: `INFO: Player scored 100 points`

### Category-Focused
```
[{Category}] {Message} ({Level})
```
**Output**: `[Gameplay] Player scored 100 points (INFO)`

### Production (JSON)
Select **JSON** format in Sinks tab for structured logging:
```json
{"timestamp":"2025-10-01T09:15:30.123Z","level":"INFO","category":"Gameplay","message":"Player scored 100 points"}
```

## Per-Category Customization

Different categories can use different templates:

1. **Gameplay Category**: Concise format for frequent logs
   - Template: `{Level}: {Message}`

2. **Network Category**: Include timestamp for debugging
   - Template: `[{Timestamp}] Network.{Level}: {Message}`

3. **Error Category**: Full detail for investigation
   - Template: `[{Timestamp}] [{Level}] {Category}: {Message}{NewLine}    (See logs for stack trace)`

## Programmatic Configuration

For advanced scenarios, configure templates via code:

```csharp
using IrsikSoftware.LogSmith;

// Get settings asset
var settings = Resources.Load<LoggingSettings>("LoggingSettings");

// Modify default template
settings.defaultTemplate = "[{Level}] {Category}: {Message}";

// Add per-category override
var categoryOverride = new CategoryTemplateOverride
{
    categoryName = "Network",
    template = "[NET] {Timestamp} - {Message}"
};
settings.categoryTemplateOverrides.Add(categoryOverride);
```

## Output Format: Text vs JSON

### Text Format
- **Use Case**: Human reading in Unity Console, log files
- **Pros**: Easy to read, configurable via templates
- **Cons**: Harder to parse programmatically

### JSON Format
- **Use Case**: Log aggregation systems (Elasticsearch, Splunk), automated analysis
- **Pros**: Machine-parsable, consistent structure
- **Cons**: Less readable for humans

Configure per sink in the **Sinks Tab**.

## Best Practices

1. **Development**: Use detailed templates with timestamps
2. **Production Builds**: Consider minimal templates to reduce log size
3. **Performance-Critical**: Shorter templates = less string allocation
4. **Log Aggregation**: Use JSON format for external systems
5. **Category Overrides**: Reserve for categories with unique needs

## Next Steps

- Experiment with templates in the Editor Window
- Try both Text and JSON formats
- Create category-specific templates for your use cases
