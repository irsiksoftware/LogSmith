# Categories

Categories organize logs by logical component or system.

## Creating Categories

### Via Editor
1. Window → LogSmith → Settings
2. Categories tab
3. Click "Add Category"
4. Configure: Name, Color, Minimum Level, Enabled

### Via Code
```csharp
var registry = Log.Resolve<ICategoryRegistry>();
registry.RegisterCategory("AI", LogLevel.Debug, Color.cyan, enabled: true);
```

## Using Categories

```csharp
var log = Log.GetLogger("AI");
log.Debug("Pathfinding started");
log.Info("Target acquired");
```

## Category Properties

| Property | Type | Description |
|----------|------|-------------|
| Name | string | Unique identifier |
| Color | Color | Display color (overlay, console) |
| MinimumLevel | LogLevel | Logs below this are filtered |
| Enabled | bool | If false, all logs blocked |

## Filtering

### Global Minimum Level
```csharp
router.SetGlobalMinimumLevel(LogLevel.Info); // Blocks Trace/Debug globally
```

### Per-Category Level
```csharp
registry.SetMinimumLevel("AI", LogLevel.Trace); // AI gets verbose logging
registry.SetMinimumLevel("Network", LogLevel.Warn); // Network only warns/errors
```

### Per-Router Override
```csharp
router.SetCategoryFilter("UI", LogLevel.Error); // Override just for this router
router.ClearCategoryFilter("UI"); // Remove override, use registry level
```

## Precedence Order
1. Router category filter (highest)
2. Category registry minimum level
3. Global minimum level (lowest)

## Common Categories

- **Gameplay**: Player actions, game state
- **AI**: Pathfinding, behavior trees
- **Network**: Connections, sync, RPC
- **UI**: Menu navigation, input
- **Audio**: Sound playback, music
- **Physics**: Collisions, raycasts
- **Save**: Persistence, serialization

## Best Practices

1. **Consistent naming**: Use PascalCase (e.g., "PlayerController")
2. **Granularity**: Not too broad ("Game") or too narrow ("PlayerJumpLogic")
3. **Runtime changes**: Adjust levels during testing without recompilation
4. **Color coding**: Use distinct colors for visual debugging
5. **Disable in production**: Turn off verbose categories for release builds
