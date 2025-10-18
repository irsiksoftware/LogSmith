# Seq Sink Example

This sample demonstrates how to configure and use the Seq Sink for powerful structured logging and log analysis.

## Overview

The Seq Sink integrates with [Seq](https://datalust.co/seq) to provide:
- Structured logging with rich queryable data
- Real-time log streaming and search
- Powerful query language (SQL-like)
- Beautiful web-based UI for log exploration
- Dashboards and visualizations
- Alerting and monitoring

## Setup Instructions

### 1. Install Seq

**Option A: Docker (Recommended)**
```bash
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

**Option B: Windows/macOS/Linux**
Download and install from [datalust.co/download](https://datalust.co/download)

**Option C: Seq Cloud**
Sign up for a cloud-hosted Seq instance at [datalust.co](https://datalust.co)

### 2. Access Seq UI

Open your browser to:
- Local: [http://localhost:5341](http://localhost:5341)
- Docker: [http://localhost:5341](http://localhost:5341)
- Cloud: Your provided Seq URL

### 3. Import the Sample

Import this sample through the Unity Package Manager:
1. Open Package Manager (Window > Package Manager)
2. Select "LogSmith Sinks" package
3. Expand "Samples" section
4. Click "Import" next to "Seq Sink"

### 4. Configure Seq Connection

1. Open the `SeqSinkExample.unity` scene
2. Select the `SeqSinkExample` GameObject
3. Configure the `Seq Server Url` (default: http://localhost:5341)
4. If using authentication, set the `Api Key` field

### 5. Run the Example

1. Press Play
2. Use the on-screen buttons to generate structured logs
3. Open the Seq UI in your browser to explore the logs

## Configuration Options

- **Seq Server Url**: The Seq server URL (e.g., http://localhost:5341)
- **Api Key**: Optional API key for authentication (leave empty for local development)
- **Minimum Level**: Minimum log level to send to Seq (typically Verbose for development)
- **Batch Size Limit**: Number of events to batch before sending (default: 100)

## Code Example

```csharp
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

// Configure logger with Seq Sink
var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithProperty("Application", "MyGame")
    .Enrich.WithProperty("Environment", "Production")
    .WriteTo.Seq(
        serverUrl: "http://localhost:5341",
        restrictedToMinimumLevel: LogEventLevel.Verbose,
        batchPostingLimit: 100
    )
    .CreateLogger();

LogSmith.Initialize(logger);

// Use structured logging
LogSmith.Information(
    "Player {PlayerId} collected {ItemType} worth {Points} points",
    "player-001",
    "PowerUp",
    100
);
```

## Structured Logging Features

### Simple Properties
```csharp
LogSmith.Information("Player scored {Score} points", 9999);
// Query in Seq: Score > 5000
```

### Multiple Properties
```csharp
LogSmith.Information(
    "Enemy {EnemyType} defeated at level {Level} for {XP} experience",
    "Dragon",
    10,
    500
);
// Query in Seq: EnemyType = 'Dragon' and Level >= 10
```

### Destructuring Objects
Use `@` to capture entire objects:
```csharp
var player = new { Id = 123, Name = "Hero", Level = 50 };
LogSmith.Information("Player state: {@Player}", player);
// Seq will index all properties: Player.Id, Player.Name, Player.Level
```

### Complex Nested Data
```csharp
var gameState = new {
    Player = new { Id = 123, Position = new { X = 10, Y = 20 } },
    Inventory = new[] { "Sword", "Shield", "Potion" }
};
LogSmith.Information("Game state snapshot: {@GameState}", gameState);
```

## Querying in Seq

The Seq UI provides a powerful query language:

### Basic Queries
```sql
-- All errors
select * from stream where @Level = 'Error'

-- Specific property value
select * from stream where ItemType = 'PowerUp'

-- Numeric comparisons
select * from stream where Score > 1000

-- Time-based
select * from stream where @Timestamp > Now() - 1h
```

### Advanced Queries
```sql
-- Multiple conditions
select * from stream
where EnemyType = 'Boss' and Level >= 10

-- Property existence
select * from stream where Score is not null

-- String matching
select * from stream where PlayerName like '%Hero%'

-- Nested properties
select * from stream where Player.Level > 50
```

### Aggregations
```sql
-- Count by type
select count(*) from stream group by ItemType

-- Average score
select avg(Score) from stream

-- Error rate
select count(*) from stream
where @Level = 'Error'
group by time(1h)
```

## Example Scenarios in the Sample

The sample demonstrates several structured logging patterns:

1. **Player Actions**: Jump, collect items with positions and values
2. **Enemy Defeats**: Type, XP gained, running totals
3. **Performance Metrics**: FPS, draw calls, memory usage
4. **Complex Events**: Nested game state with player, session, and system data

## Best Practices

1. **Use Structured Properties**: Capture data as properties, not in message text
   - Good: `LogSmith.Information("Score: {Score}", 100)`
   - Bad: `LogSmith.Information($"Score: {score}")`

2. **Destructure Complex Objects**: Use `@` for objects you want to query
   ```csharp
   LogSmith.Information("Player: {@Player}", playerObject);
   ```

3. **Add Context with Enrichment**: Add common properties to all logs
   ```csharp
   .Enrich.WithProperty("Environment", "Production")
   .Enrich.WithProperty("ServerRegion", "US-West")
   ```

4. **Consistent Property Names**: Use the same property names across your codebase
   - `PlayerId` everywhere, not sometimes `UserId` or `PlayerID`

5. **Log at Appropriate Levels**:
   - Verbose: Detailed trace for debugging
   - Debug: Developer-focused information
   - Information: Important business events
   - Warning: Unexpected but handled situations
   - Error: Failures that need attention
   - Fatal: Critical failures requiring immediate action

## Troubleshooting

**Logs not appearing in Seq:**
- Verify Seq is running (check http://localhost:5341)
- Check Unity Console for connection errors
- Ensure firewall allows connections to Seq
- Verify the server URL is correct

**Performance impact:**
- Increase `batchSizeLimit` for fewer network calls
- Use appropriate log levels (don't log Verbose in production)
- Consider using sampling for high-volume events

**Seq is slow or unresponsive:**
- Reduce log volume by increasing minimum level
- Set up log retention policies in Seq settings
- Use signals and filters to reduce noise

## Additional Resources

- [Seq Documentation](https://docs.datalust.co/docs)
- [Seq Query Language Reference](https://docs.datalust.co/docs/the-seq-query-language)
- [Serilog Seq Sink](https://github.com/serilog/serilog-sinks-seq)
- [LogSmith Documentation](https://github.com/irsiksoftware/LogSmith)
- [Structured Logging Best Practices](https://nblumhardt.com/2016/06/structured-logging-concepts-in-net-series-1/)
