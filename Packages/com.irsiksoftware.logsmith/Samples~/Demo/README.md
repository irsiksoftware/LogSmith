# LogSmith Demo Samples

Comprehensive examples demonstrating all LogSmith features.

## Getting Started

1. Import this sample via Package Manager
2. Open the Demo scene (or create a new scene)
3. Add the example scripts to GameObjects
4. Toggle GameObjects on/off to see different logging scenarios

## Example Scripts

### 1. BasicLoggingExample
**Purpose:** Simple logging with the static API
**Features:**
- Log on Start and Update
- All log levels (Debug, Info, Warn, Error)
- Configurable log intervals

**Usage:** Add to any GameObject and toggle settings in Inspector

---

### 2. CategoryLoggingExample
**Purpose:** Category-specific logging
**Features:**
- Custom category names
- Category-based filtering
- Context menu shortcuts

**Usage:** Create multiple instances with different category names

---

### 3. GameSystemsExample
**Purpose:** Simulate multiple game systems logging independently
**Features:**
- Network, Physics, AI, Audio loggers
- Auto-simulation mode
- Realistic game system messages

**Usage:** Great for testing category filters and colors

---

### 4. PerformanceLoggingExample
**Purpose:** Performance monitoring and profiling
**Features:**
- FPS monitoring with thresholds
- System information logging
- Performance spike detection

**Usage:** Use to test warning/error thresholds

---

### 5. PlayerActionExample
**Purpose:** Player action logging (gameplay events)
**Features:**
- Keyboard shortcuts (1-5 keys)
- Health/damage system
- Inventory and scoring

**Keyboard Controls:**
- `1` - Take Damage
- `2` - Heal
- `3` - Fire Weapon
- `4` - Collect Item
- `5` - Add Score

---

### 6. LogLevelExample
**Purpose:** Demonstrate all log levels with examples
**Features:**
- All 6 log levels (Trace, Debug, Info, Warn, Error, Critical)
- Realistic logging scenarios
- Use via Context Menu (right-click in Inspector)

**Usage:** Best for understanding when to use each log level

---

### 7. VContainerLoggingExample
**Purpose:** Dependency injection with VContainer
**Features:**
- ILog injection
- Automatic DI detection
- Fallback to static API

**Requirements:** LoggingLifetimeScope GameObject in scene

---

## Scene Setup Example

Create a new scene with:

```
Scene
├── Main Camera
├── Directional Light
├── LoggingLifetimeScope (if using VContainer)
└── Demo Examples (Empty GameObject)
    ├── BasicLogging (BasicLoggingExample)
    ├── GameSystems (GameSystemsExample)
    ├── Performance (PerformanceLoggingExample)
    ├── Player (PlayerActionExample)
    └── LogLevels (LogLevelExample)
```

## Testing Tips

1. **Enable/Disable GameObjects** - Toggle examples on/off to control output
2. **Use Context Menus** - Right-click scripts in Inspector for quick actions
3. **Configure Categories** - Open `Window → LogSmith → Settings` to customize
4. **Filter Logs** - Use Unity Console filters or category-specific settings
5. **Keyboard Shortcuts** - PlayerActionExample responds to number keys 1-5

## Configuration Window

Access via `Window → LogSmith → Settings`:
- **Categories Tab:** Add/remove categories, set colors and levels
- **Sinks Tab:** Configure console/file output
- **Templates Tab:** Customize message formatting with live preview

## Best Practices Demonstrated

- ✓ Use categories for different game systems
- ✓ Use appropriate log levels (Debug for verbose, Info for important events)
- ✓ Include context in messages (player name, values, timestamps)
- ✓ Use Warn for recoverable issues, Error for failures
- ✓ Create category-specific loggers in Start() for performance
- ✓ Use structured messages with data (not just text)

## Next Steps

1. Create a LoggingSettings asset: `Assets → Create → LogSmith → Logging Settings`
2. Configure categories with meaningful names and colors
3. Adjust minimum log levels per category
4. Try different message templates
5. Enable file logging to test file output

## Support

For more information, see the main LogSmith documentation or visit the GitHub repository.