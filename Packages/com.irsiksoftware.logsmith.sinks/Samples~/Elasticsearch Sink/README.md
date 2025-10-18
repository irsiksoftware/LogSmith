# Elasticsearch Sink Example

This sample demonstrates how to configure and use the Elasticsearch Sink for large-scale log storage and analysis.

## Overview

The Elasticsearch Sink integrates with [Elasticsearch](https://www.elastic.co/elasticsearch/) to provide:
- Scalable log storage and indexing
- Full-text search capabilities
- Time-series data analysis
- Integration with Kibana for visualization
- Aggregations and analytics
- Cluster support for high availability

## Setup Instructions

### 1. Install Elasticsearch and Kibana

**Option A: Docker Compose (Recommended)**

Create `docker-compose.yml`:
```yaml
version: '3.8'
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - esdata:/usr/share/elasticsearch/data

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch

volumes:
  esdata:
```

Run:
```bash
docker-compose up -d
```

**Option B: Manual Installation**
1. Download from [elastic.co/downloads](https://www.elastic.co/downloads/)
2. Install Elasticsearch and start the service
3. Install Kibana and configure it to connect to Elasticsearch
4. Access Kibana at [http://localhost:5601](http://localhost:5601)

**Option C: Elastic Cloud**
Sign up for managed Elasticsearch at [cloud.elastic.co](https://cloud.elastic.co)

### 2. Verify Elasticsearch

Test the connection:
```bash
curl http://localhost:9200
```

You should see cluster information in the response.

### 3. Import the Sample

Import this sample through the Unity Package Manager:
1. Open Package Manager (Window > Package Manager)
2. Select "LogSmith Sinks" package
3. Expand "Samples" section
4. Click "Import" next to "Elasticsearch Sink"

### 4. Configure Connection

1. Open the `ElasticsearchSinkExample.unity` scene
2. Select the `ElasticsearchSinkExample` GameObject
3. Configure connection settings:
   - **Elasticsearch Nodes**: URL(s) of your Elasticsearch nodes
   - **Index Format**: Pattern for index names (supports date formatting)
   - **Username/Password**: If authentication is enabled

### 5. Run the Example

1. Press Play in Unity
2. Use the on-screen buttons to generate logs
3. Open Kibana at [http://localhost:5601](http://localhost:5601)
4. Create an index pattern for `logsmith-*`
5. Explore logs in the Discover tab

## Configuration Options

- **Elasticsearch Nodes**: Comma-separated list of node URLs (supports multiple nodes for clustering)
- **Index Format**: Index name pattern with date formatting (e.g., `logsmith-{0:yyyy.MM.dd}`)
- **Username/Password**: Credentials for authentication (optional)
- **Minimum Level**: Minimum log level to send to Elasticsearch
- **Batch Size Limit**: Number of events to batch before sending

## Code Example

```csharp
using Irsik.LogSmith;
using Irsik.LogSmith.Sinks;
using Serilog;
using Serilog.Events;

// Configure logger with Elasticsearch Sink
var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithProperty("Application", "MyGame")
    .Enrich.WithProperty("Environment", "Production")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "mygame-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true,
        MinimumLogEventLevel = LogEventLevel.Verbose,
        BatchPostingLimit = 50
    })
    .CreateLogger();

LogSmith.Initialize(logger);

// Use structured logging
LogSmith.Information(
    "Player action: {ActionType} at {Position}",
    "Jump",
    new Vector3(10, 20, 0)
);
```

## Elasticsearch Features

### Index Management

Logs are organized into time-based indices:
- `logsmith-2025.10.18` - Today's logs
- `logsmith-2025.10.17` - Yesterday's logs
- Etc.

This allows for:
- Easy data lifecycle management
- Time-based retention policies
- Improved query performance

### Full-Text Search

Search across all log fields:
```
Jump AND Level:Information
```

### Time-Series Analysis

Query logs within specific time ranges in Kibana:
- Last 15 minutes
- Last hour
- Last 24 hours
- Custom ranges

### Aggregations

Kibana visualizations support:
- Counting events by type
- Averaging performance metrics
- Percentile calculations
- Trend analysis

## Kibana Setup

### 1. Create Index Pattern

1. Open Kibana at [http://localhost:5601](http://localhost:5601)
2. Go to Management > Stack Management > Index Patterns
3. Click "Create index pattern"
4. Enter pattern: `logsmith-*`
5. Select `@timestamp` as the time field
6. Click "Create index pattern"

### 2. Explore Logs

1. Go to Analytics > Discover
2. Select your `logsmith-*` index pattern
3. Use the search bar to query logs
4. Add/remove columns to customize the view

### 3. Create Visualizations

Create charts and dashboards:
1. Go to Analytics > Dashboard
2. Click "Create dashboard"
3. Add visualizations:
   - Event count over time
   - Error rate
   - Performance metrics (FPS, memory)
   - Player action distribution

## Query Examples

In Kibana's search bar (KQL - Kibana Query Language):

```
# All errors
Level: "Error"

# Player actions
EventType: "PlayerAction" AND ActionType: "Jump"

# Performance issues
EventType: "PerformanceMetric" AND FPS < 30

# Specific time range with filters
EventType: "NetworkEvent" AND Latency > 100

# Complex queries
(EventType: "PlayerAction" OR EventType: "SystemEvent") AND Level: "Information"
```

## Sample Event Types

The example generates various event types:

1. **PlayerAction**: Jump, Attack, Defend, UseItem, Move
2. **SystemEvent**: SceneLoaded, ConfigUpdated, CacheCleared
3. **NetworkEvent**: Connected, Disconnected, MessageSent
4. **AssetLoad**: Texture, Model, Audio loading events
5. **PerformanceMetric**: FPS, memory, draw calls

## Best Practices

### 1. Index Naming
Use time-based indices for better management:
```csharp
IndexFormat = "appname-{0:yyyy.MM.dd}"
```

### 2. Field Mapping
Elasticsearch auto-detects field types, but you can define custom mappings for:
- Optimized storage
- Better search performance
- Precise data types

### 3. Retention Policies
Configure Index Lifecycle Management (ILM) in Elasticsearch:
- Hot phase: Recent logs (fast storage)
- Warm phase: Older logs (slower storage)
- Cold phase: Archive
- Delete: Remove after X days

### 4. Performance
- Use appropriate batch sizes (50-100 events)
- Increase batch size for high-volume logging
- Monitor Elasticsearch cluster health
- Use multiple nodes for production

### 5. Security
For production:
- Enable authentication
- Use HTTPS for connections
- Implement role-based access control
- Encrypt data at rest

## Troubleshooting

**Connection errors:**
- Verify Elasticsearch is running: `curl http://localhost:9200`
- Check firewall settings
- Ensure correct URL and port
- Verify authentication credentials

**Logs not appearing in Kibana:**
- Verify index pattern matches your indices
- Check time filter (ensure it covers your log timestamps)
- Refresh index pattern to pick up new fields
- Check Elasticsearch logs for indexing errors

**Performance issues:**
- Increase batch size to reduce network calls
- Reduce log volume by filtering less important logs
- Add more Elasticsearch nodes for scaling
- Optimize index mappings

**Index errors:**
- Check index naming format is valid
- Ensure date format is correct
- Verify Elasticsearch has write permissions
- Check disk space on Elasticsearch nodes

## Advanced Features

### Clustering
Configure multiple Elasticsearch nodes for high availability:
```csharp
elasticsearchNodes = "http://node1:9200,http://node2:9200,http://node3:9200"
```

### Custom Mappings
Define field types for optimized storage and search.

### Alerting
Use Kibana Alerting to get notified of critical errors.

### Machine Learning
Use Elasticsearch ML to detect anomalies in log patterns.

## Additional Resources

- [Elasticsearch Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Kibana User Guide](https://www.elastic.co/guide/en/kibana/current/index.html)
- [Serilog Elasticsearch Sink](https://github.com/serilog-contrib/serilog-sinks-elasticsearch)
- [LogSmith Documentation](https://github.com/irsiksoftware/LogSmith)
- [KQL Query Syntax](https://www.elastic.co/guide/en/kibana/current/kuery-query.html)
