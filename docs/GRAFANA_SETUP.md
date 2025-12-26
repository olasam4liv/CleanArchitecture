# Grafana Dashboard Setup

This guide explains how to access and use Grafana to visualize your application metrics and logs.

## Quick Start

### 1. Start All Services

```bash
docker-compose up -d
```

### 2. Access Grafana

Open your browser and go to: **http://localhost:3000**

**Default Credentials:**
- Username: `admin`
- Password: `admin`

You'll be prompted to change the password on first login (optional for local development).

---

## Pre-configured Data Sources

Grafana is automatically configured with these data sources:

| Data Source | Purpose | URL |
|------------|---------|-----|
| **Prometheus** | Metrics (request rate, CPU, memory) | http://prometheus:9090 |
| **Loki** | Application logs | http://loki:3100 |
| **Elasticsearch** | Log search and analysis | http://elasticsearch:9200 |

### Verify Data Sources

1. Click **Configuration** (??) ? **Data Sources**
2. You should see all three data sources with a green checkmark
3. Click **Test** on each to verify connectivity

---

## Pre-loaded Dashboard

A sample dashboard is automatically loaded: **"Clean Architecture - Application Dashboard"**

### Access the Dashboard

1. Click **Dashboards** (??) icon in the left sidebar
2. Click **Browse**
3. Select **"Clean Architecture - Application Dashboard"**

### Dashboard Panels

The default dashboard includes:

1. **HTTP Request Rate** - Requests per second
2. **Response Time (95th percentile)** - API performance
3. **CPU Usage** - Application CPU consumption
4. **Memory Usage** - Application memory consumption
5. **Application Logs** - Real-time log viewer (from Loki)

---

## Creating Custom Dashboards

### 1. Create a New Dashboard

1. Click **+** (Create) ? **Dashboard**
2. Click **Add visualization**
3. Select a data source (Prometheus, Loki, or Elasticsearch)

### 2. Example: Query Prometheus Metrics

**Query Request Rate:**
```promql
rate(http_requests_received_total[5m])
```

**Query Error Rate:**
```promql
rate(http_requests_received_total{status_code=~"5.."}[5m])
```

**Query Response Time (99th percentile):**
```promql
histogram_quantile(0.99, rate(http_request_duration_milliseconds_bucket[5m]))
```

**Query Memory Usage:**
```promql
process_working_set_bytes / 1024 / 1024
```

### 3. Example: Query Loki Logs

**All Application Logs:**
```logql
{job="clean-architecture"}
```

**Error Logs Only:**
```logql
{job="clean-architecture"} |= "error" or "Error" or "ERROR"
```

**Logs from Specific Namespace:**
```logql
{job="clean-architecture", namespace="Application"}
```

**Count Errors Over Time:**
```logql
count_over_time({job="clean-architecture"} |= "error" [5m])
```

### 4. Example: Query Elasticsearch

**Search Logs:**
- Index pattern: `logs-*`
- Query: `level:error`
- Time field: `@timestamp`

---

## Common Queries for .NET Applications

### ASP.NET Core Metrics

**Request Count by Endpoint:**
```promql
sum by (endpoint) (rate(http_requests_received_total[5m]))
```

**Request Count by Status Code:**
```promql
sum by (status_code) (rate(http_requests_received_total[5m]))
```

**Active Connections:**
```promql
kestrel_active_connections
```

**Thread Pool Queue Length:**
```promql
dotnet_threadpool_queue_length
```

### Database Metrics

**Database Connection Pool Size:**
```promql
microsoft_entityframeworkcore_connections_in_use
```

**Database Query Duration:**
```promql
microsoft_entityframeworkcore_query_duration_milliseconds
```

### Application Health

**Process Uptime:**
```promql
process_start_time_seconds
```

**GC Collections:**
```promql
rate(dotnet_gc_collections_total[5m])
```

---

## Alerting

### Create an Alert Rule

1. Open a dashboard panel
2. Click **Edit**
3. Go to **Alert** tab
4. Click **Create alert rule from this panel**
5. Configure:
   - **Condition:** e.g., CPU > 80%
   - **Evaluation:** Every 1m for 5m
   - **Actions:** Email, Slack, webhook

### Example Alert: High Error Rate

**Condition:**
```promql
rate(http_requests_received_total{status_code=~"5.."}[5m]) > 0.1
```

**Description:**
```
Error rate is above 10% over the last 5 minutes
```

---

## Dashboard Organization

### Create Folders

1. **Dashboards** ? **Browse**
2. Click **New** ? **New folder**
3. Name it (e.g., "Production", "Development", "Infrastructure")
4. Move dashboards into folders

### Recommended Folder Structure

```
??? Application
?   ??? API Overview
?   ??? Performance Metrics
?   ??? Error Tracking
??? Infrastructure
?   ??? RabbitMQ Monitoring
?   ??? Elasticsearch Health
?   ??? Database Performance
??? Business Metrics
    ??? User Activity
    ??? Todo Completion Rates
```

---

## Variables and Templating

### Create a Dashboard Variable

1. Dashboard settings (??) ? **Variables**
2. Click **New variable**
3. Configure:
   - **Name:** `environment`
   - **Type:** Custom
   - **Values:** `development,staging,production`

### Use Variable in Query

```promql
http_requests_received_total{environment="$environment"}
```

This creates a dropdown to filter by environment.

---

## Exporting and Importing Dashboards

### Export Dashboard

1. Dashboard settings (??) ? **JSON Model**
2. Click **Copy to Clipboard** or **Save to file**
3. Share with team or commit to Git

### Import Dashboard

1. **+** (Create) ? **Import**
2. Paste JSON or upload file
3. Select data source
4. Click **Import**

### Popular Public Dashboards

Visit [Grafana Dashboard Library](https://grafana.com/grafana/dashboards/) for pre-built dashboards:

- **ASP.NET Core Dashboard:** #10427
- **RabbitMQ Dashboard:** #10991
- **Elasticsearch Dashboard:** #266
- **Docker Dashboard:** #179

---

## Performance Tips

### 1. Limit Time Range
- Use recent time ranges (last 6h, 24h)
- Avoid queries over months of data

### 2. Use Query Caching
- Enable **Cache timeout** in panel settings
- Set to 60s for frequently accessed panels

### 3. Reduce Refresh Rate
- Default: 5s (high load)
- Recommended: 30s or 1m for most dashboards

### 4. Use Variables
- Filter large datasets with variables
- Reduce query scope

---

## Troubleshooting

### Data Source Connection Failed

**Check Docker containers:**
```bash
docker ps | grep -E "prometheus|loki|elasticsearch"
```

**Check Grafana logs:**
```bash
docker logs grafana
```

**Test data source manually:**
```bash
# Test Prometheus
curl http://localhost:9090/-/healthy

# Test Loki
curl http://localhost:3100/ready

# Test Elasticsearch
curl http://localhost:9200
```

### No Data in Panels

**1. Verify metrics are being scraped:**
```bash
# Check Prometheus targets
open http://localhost:9090/targets
```

**2. Check if your application exposes metrics:**
```bash
curl http://localhost:8080/metrics
```

**3. Verify time range:**
- Make sure time range includes recent data
- Try "Last 15 minutes"

### Dashboard Not Loading

**1. Clear browser cache:**
- Hard refresh: `Ctrl+Shift+R` (Windows) or `Cmd+Shift+R` (Mac)

**2. Check dashboard JSON:**
- Make sure data source UIDs match
- Re-import the dashboard if needed

---

## Advanced Features

### 1. Annotations
Mark important events on dashboards:
- Deployments
- Incidents
- Configuration changes

### 2. Playlists
Rotate through dashboards automatically:
1. **Dashboards** ? **Playlists**
2. Create new playlist
3. Add dashboards
4. Set interval (e.g., 30s)

### 3. Snapshots
Share dashboard state with others:
1. **Share** ? **Snapshot**
2. Set expiration
3. Copy link

### 4. API Integration
Automate dashboard creation:
```bash
curl -X POST http://admin:admin@localhost:3000/api/dashboards/db \
  -H "Content-Type: application/json" \
  -d @dashboard.json
```

---

## Useful Grafana Plugins

### Install Plugins

Add to `docker-compose.yml`:
```yaml
environment:
  - GF_INSTALL_PLUGINS=grafana-clock-panel,grafana-piechart-panel,grafana-worldmap-panel
```

### Recommended Plugins

- **Clock Panel** - Show current time
- **Pie Chart Panel** - Visualize proportions
- **Worldmap Panel** - Geographic data visualization
- **Table Panel** - Enhanced tables

---

## Best Practices

### 1. Dashboard Design
- ? One metric per panel
- ? Use consistent colors
- ? Add panel descriptions
- ? Group related metrics
- ? Don't overcrowd dashboards

### 2. Naming Conventions
- Use descriptive titles
- Include units (ms, %, req/s)
- Prefix with category (API, DB, Infrastructure)

### 3. Organization
- Create folders for different teams/services
- Use tags for searchability
- Star frequently used dashboards

### 4. Security
- Change default admin password
- Create separate users for different teams
- Use authentication (LDAP, OAuth)
- Restrict dashboard editing permissions

---

## Resources

- **Grafana Documentation:** https://grafana.com/docs/
- **PromQL Tutorial:** https://prometheus.io/docs/prometheus/latest/querying/basics/
- **LogQL Tutorial:** https://grafana.com/docs/loki/latest/logql/
- **Dashboard Examples:** https://grafana.com/grafana/dashboards/
- **Community Forum:** https://community.grafana.com/

---

## Support

- **Grafana Logs:** `docker logs grafana`
- **Restart Grafana:** `docker-compose restart grafana`
- **Reset Grafana:** `docker-compose down -v && docker-compose up -d`

For application-specific issues, check:
- `docs/SERVICES_SETUP.md` - Service configuration
- `docs/QUICK_REFERENCE.md` - Quick commands
