# Testing Logging Integration

This guide shows how to verify logs are flowing to all three systems: **Seq**, **Elasticsearch**, and **Grafana (via Loki/Promtail)**.

## Prerequisites

Before testing, ensure all services are running and the database is properly migrated.

### 1. Check All Services Are Running

```bash
docker-compose ps
```

Expected output should show all containers running:
- ? web-api
- ? seq
- ? elasticsearch
- ? loki
- ? promtail
- ? grafana
- ? rabbitmq
- ? prometheus
- ? jaeger
- ? otel-collector

### 2. Fix Pending Database Migrations

The application currently fails to start because there are pending EF Core migrations. You need to:

**Option A: Create and Apply Migration (Recommended)**

```bash
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create migration for audit trail fields
dotnet ef migrations add AddAuditTrailFields --project src/Infrastructure --startup-project src/Web.Api

# Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api
```

**Option B: Use Existing Database Without New Fields**

If you want to test without the audit fields, temporarily remove the audit properties from entities:
- Remove `IAuditableEntity` interface from `User.cs` and `TodoItem.cs`
- Or comment out the audit handling in `ApplicationDbContext.SaveChangesAsync`

---

## Testing Logging to All Three Systems

Once the application starts successfully, follow these steps:

### Step 1: Start All Services

```bash
# Clean start
docker-compose down -v
docker-compose up -d

# Wait for services to initialize (30 seconds)
Start-Sleep -Seconds 30

# Check web-api logs
docker logs web-api --tail=20
```

### Step 2: Generate Test Logs

Make some API requests to generate logs:

```powershell
# Health check endpoint
Invoke-WebRequest -Uri http://localhost:8000/health -UseBasicParsing

# Swagger UI (generates logs)
Invoke-WebRequest -Uri http://localhost:8000/swagger -UseBasicParsing

# Get users (will generate logs)
Invoke-WebRequest -Uri http://localhost:8000/api/v1/users -UseBasicParsing

# Register a new user (generates multiple logs)
$body = @{
    email = "test@example.com"
    firstName = "John"
    lastName = "Doe"
    password = "Test123!"
} | ConvertTo-Json

Invoke-WebRequest -Uri http://localhost:8000/api/v1/users/register `
    -Method POST `
    -ContentType "application/json" `
    -Body $body `
    -UseBasicParsing
```

### Step 3: Verify Logs in Seq

**Access Seq:**
- Open: http://localhost:5341
- You should see logs immediately

**What to Look For:**
- ? Application startup logs
- ? HTTP request logs
- ? Serilog structured properties (Level, Timestamp, Message)
- ? Request context (Path, Method, StatusCode)

**Example Query in Seq:**
```
Level = "Information"
```

**Screenshot Check:**
- Look for logs with source context like `Web.Api`, `Infrastructure.Database`
- Check timestamps are recent
- Verify structured properties are being captured

---

### Step 4: Verify Logs in Elasticsearch

**Option A: Check via Grafana (Recommended)**

1. Open Grafana: http://localhost:3000 (admin/admin)
2. Go to **Explore** (compass icon)
3. Select **Elasticsearch** data source
4. Query:
   ```
   index: logs-*
   query: level:Information
   ```

**Option B: Check via Elasticsearch API**

```powershell
# Check if Elasticsearch is receiving logs
Invoke-WebRequest -Uri http://localhost:9200/_cat/indices?v -UseBasicParsing

# Search for recent logs
$query = @{
    query = @{
        match_all = @{}
    }
    sort = @(
        @{
            "@timestamp" = @{
                order = "desc"
            }
        }
    )
    size = 10
} | ConvertTo-Json -Depth 5

Invoke-WebRequest -Uri "http://localhost:9200/logs-*/_search" `
    -Method POST `
    -ContentType "application/json" `
    -Body $query `
    -UseBasicParsing | Select-Object -ExpandProperty Content
```

**What to Look For:**
- ? Index exists (e.g., `logs-clean-architecture-web-api-2025.12.26`)
- ? Documents are being indexed
- ? Logs contain structured fields (@timestamp, level, message, sourceContext)

---

### Step 5: Verify Logs in Grafana (Loki)

**Access Grafana:**
- Open: http://localhost:3000 (admin/admin)

**Method 1: Use Pre-built Dashboard**

1. Go to **Dashboards** ? **Browse**
2. Open **"Clean Architecture - Application Dashboard"**
3. Scroll to the **Application Logs** panel at the bottom
4. You should see logs streaming in real-time

**Method 2: Use Explore**

1. Go to **Explore** (compass icon)
2. Select **Loki** data source
3. Enter query:
   ```logql
   {container="web-api"}
   ```
4. Click **Run Query**

**Alternative Queries to Try:**
```logql
# All logs from web-api
{container="web-api"}

# Only error logs
{container="web-api"} |= "error" or "Error" or "ERROR"

# Logs from specific namespace
{container="web-api", namespace="Application"}

# Logs with specific HTTP status
{container="web-api"} |~ "StatusCode.*[45][0-9]{2}"
```

**What to Look For:**
- ? Logs appear with recent timestamps
- ? Log entries show full message content
- ? Can filter by container, job, or other labels
- ? Logs update in real-time

---

### Step 6: Verify Promtail is Collecting Logs

Check if Promtail is reading Docker container logs:

```powershell
# Check Promtail logs
docker logs promtail --tail=50

# Should see lines like:
# level=info msg="Successfully sent batch of X entries"
# level=info msg="Tailer: got new file" filename=/var/lib/docker/containers/...
```

**If Promtail isn't working:**

1. Check Promtail configuration:
```powershell
cat .containers/promtail/config.yml
```

2. Verify Docker socket is mounted:
```powershell
docker inspect promtail | Select-String "docker.sock"
```

3. Check Loki is reachable from Promtail:
```powershell
docker exec promtail wget -O- http://loki:3100/ready
```

---

## Complete Verification Checklist

Use this checklist to confirm everything is working:

### ? Seq Logs
- [ ] Seq UI accessible at http://localhost:5341
- [ ] Logs appear with recent timestamps
- [ ] Structured properties visible (Level, Timestamp, SourceContext)
- [ ] Can filter logs by level, source, or text search
- [ ] Request/response logs captured

### ? Elasticsearch Logs
- [ ] Elasticsearch accessible at http://localhost:9200
- [ ] Indices created (e.g., `logs-clean-architecture-*`)
- [ ] Can query logs via Grafana Elasticsearch datasource
- [ ] Logs have structured fields (@timestamp, level, message)
- [ ] Full-text search works

### ? Grafana/Loki Logs
- [ ] Grafana accessible at http://localhost:3000
- [ ] Loki datasource configured and healthy
- [ ] Logs appear in "Application Logs" dashboard panel
- [ ] Can query logs in Explore with LogQL
- [ ] Promtail is collecting from Docker containers
- [ ] Logs update in real-time

---

## Troubleshooting

### Issue: Web API Won't Start (Migration Error)

**Error:**
```
The model for context 'ApplicationDbContext' has pending changes.
Add a new migration before updating the database.
```

**Solution:**
```bash
dotnet ef migrations add AddAuditTrailFields --project src/Infrastructure --startup-project src/Web.Api
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api
```

### Issue: No Logs in Seq

**Check:**
1. Seq container is running:
```powershell
docker ps | Select-String seq
```

2. Application is configured to send to Seq:
```powershell
# Check appsettings.json or environment variables
docker exec web-api cat /app/appsettings.json | Select-String SEQ
```

3. Seq logs for errors:
```powershell
docker logs seq --tail=50
```

### Issue: No Logs in Elasticsearch

**Check:**
1. Elasticsearch is running and healthy:
```powershell
Invoke-WebRequest -Uri http://localhost:9200/_cluster/health -UseBasicParsing
```

2. Serilog Elasticsearch sink is configured:
```csharp
// Should be in appsettings.json or Program.cs
"WriteTo": [
  {
    "Name": "Elasticsearch",
    "Args": {
      "nodeUris": "http://elasticsearch:9200"
    }
  }
]
```

3. Check for indexing errors:
```powershell
docker logs web-api | Select-String "Elasticsearch"
```

### Issue: No Logs in Grafana/Loki

**Check:**
1. Loki is running:
```powershell
docker ps | Select-String loki
```

2. Promtail is running and configured:
```powershell
docker ps | Select-String promtail
docker logs promtail --tail=50
```

3. Loki is receiving data:
```powershell
# Check Loki labels (should show containers)
Invoke-WebRequest -Uri http://localhost:3100/loki/api/v1/labels -UseBasicParsing
```

4. Grafana datasource is configured:
- Go to Grafana ? Configuration ? Data Sources
- Check "Loki" datasource shows green checkmark
- Click "Test" button

### Issue: Promtail Not Collecting Logs

**Check Docker socket mount:**
```yaml
# In docker-compose.yml, promtail service should have:
volumes:
  - /var/run/docker.sock:/var/run/docker.sock:ro
  - /var/lib/docker/containers:/var/lib/docker/containers:ro
```

**On Windows with Docker Desktop:**
- Docker socket path is different
- May need to use named pipe instead
- Check Docker Desktop settings for WSL2 integration

---

## Expected Results

After successful setup, you should see:

### Seq Dashboard
```
[12:34:56 INF] HTTP GET /health responded 200 in 45.2ms
[12:34:57 INF] HTTP GET /swagger responded 200 in 123.5ms
[12:34:58 INF] Application started successfully
```

### Elasticsearch Query Result
```json
{
  "hits": {
    "total": { "value": 150 },
    "hits": [
      {
        "_source": {
          "@timestamp": "2025-12-26T12:34:56.789Z",
          "level": "Information",
          "message": "HTTP GET /health responded 200",
          "sourceContext": "Microsoft.AspNetCore.Hosting.Diagnostics"
        }
      }
    ]
  }
}
```

### Grafana Loki Logs Panel
```
2025-12-26 12:34:56  INFO  HTTP GET /health responded 200 in 45.2ms
2025-12-26 12:34:57  INFO  HTTP GET /swagger responded 200 in 123.5ms
2025-12-26 12:34:58  INFO  Application started successfully
```

---

## Next Steps

Once logging is verified:

1. ? **Configure Log Levels** - Adjust verbosity in `appsettings.json`
2. ? **Set Up Alerts** - Configure Grafana alerts for errors
3. ? **Create Dashboards** - Build custom Grafana dashboards
4. ? **Configure Retention** - Set log retention policies
5. ? **Monitor Performance** - Use logs to identify bottlenecks

---

## Summary

Your application logs to **three different systems** for different purposes:

| System | Purpose | Best For |
|--------|---------|----------|
| **Seq** | Structured log viewer | .NET development, debugging, local troubleshooting |
| **Elasticsearch** | Log search & analysis | Production log search, long-term storage, analytics |
| **Grafana/Loki** | Unified monitoring | Correlating logs with metrics, real-time monitoring, dashboards |

**All three should show the same logs**, but each provides different capabilities for viewing and analyzing them.
