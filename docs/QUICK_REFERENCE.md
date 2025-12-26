# Quick Reference: Azure Deployment Options

## TL;DR - Which Should I Choose?

### Choose Azure PaaS if:
- ? You want zero infrastructure management
- ? Your team is small or lacks DevOps expertise
- ? You need auto-scaling and high availability
- ? Budget allows for premium services (~$400/month)
- ? You're okay with vendor lock-in

**Cost:** ~$200-500/month  
**Setup Time:** Hours  
**Maintenance:** Minimal

### Choose Single Server (All-in-One) if:
- ? Budget is limited (~$300/month)
- ? You want to avoid vendor lock-in
- ? Workload is small to medium (< 100 req/sec)
- ? You have basic Linux administration skills
- ? You want simplified management (one server)
- ? Can tolerate brief downtime for updates

**Cost:** ~$310/month (+ DevOps time)  
**Setup Time:** 2-3 hours  
**Maintenance:** High

?? **Complete Guide:** [docs/SINGLE_SERVER_SETUP.md](SINGLE_SERVER_SETUP.md)

### Choose Multiple Servers (Separate VMs) if:
- ? You need high availability (99.9%+)
- ? High traffic (> 1000 req/sec)
- ? Need independent service scaling
- ? Have dedicated DevOps team
- ? Budget allows for ~$600+/month
- ? Production with strict SLAs

**Cost:** ~$640/month (+ DevOps time)  
**Setup Time:** 1 week  
**Maintenance:** Very High

?? **Complete Guide:** [docs/AZURE_VM_DEPLOYMENT.md](AZURE_VM_DEPLOYMENT.md)

### Choose Hybrid if:
- ? You want best of both worlds
- ? Different needs for different services
- ? Gradual migration strategy
- ? Optimize cost per service

**Cost:** Variable  
**Setup Time:** 2-3 weeks  
**Maintenance:** Medium

---

## Service-by-Service Comparison

| Service | Azure PaaS | Self-Hosted VM | Best For |
|---------|-----------|----------------|----------|
| **Database** | Azure SQL Database | SQL Server on VM | PaaS (most cases) |
| **Messaging** | Azure Service Bus | RabbitMQ on VM | PaaS (unless need RabbitMQ features) |
| **Logging** | Application Insights | Elasticsearch on VM | PaaS (integrated) or Elastic Cloud |
| **Metrics** | Azure Monitor | Prometheus on VM | PaaS (built-in) |
| **Tracing** | App Insights | Jaeger on VM | PaaS (native .NET support) |

---

## Quick Start Commands

### Azure PaaS Setup

```bash
# 1. Create resources
az group create --name rg-cleanarch --location eastus
az sql server create --name sql-cleanarch --resource-group rg-cleanarch --admin-user sqladmin --admin-password 'Password123!'
az sql db create --name CleanArchitecture --server sql-cleanarch --resource-group rg-cleanarch
az servicebus namespace create --name sb-cleanarch --resource-group rg-cleanarch
az monitor app-insights component create --app ai-cleanarch --resource-group rg-cleanarch

# 2. Deploy app
az webapp create --name app-cleanarch --resource-group rg-cleanarch --plan plan-cleanarch --deployment-container-image-name your-acr.azurecr.io/web-api:latest

# 3. Configure app
az webapp config appsettings set --name app-cleanarch --resource-group rg-cleanarch --settings ConnectionStrings__Database="Server=sql-cleanarch.database.windows.net;..."
```

### Self-Hosted VM Setup

```bash
# 1. Create VNet
az network vnet create --name vnet-cleanarch --resource-group rg-cleanarch

# 2. Create VMs
az vm create --name vm-sqlserver --image MicrosoftSQLServer:sql2022-ws2022:sqldev-gen2:latest --resource-group rg-cleanarch
az vm create --name vm-rabbitmq --image Ubuntu2204 --resource-group rg-cleanarch
az vm create --name vm-elasticsearch --image Ubuntu2204 --resource-group rg-cleanarch

# 3. Install services on each VM
ssh azureuser@<vm-ip>
# Follow installation from docs/SERVICES_SETUP.md
```

### Docker Compose (Local)

```bash
# Start everything
docker-compose up -d

# Stop everything
docker-compose down

# Clean restart
docker-compose down -v && docker-compose up -d --build
```

---

## Environment Variables Quick Reference

### Local Development (.env)
```bash
DATABASE=Server=localhost\SQLEXPRESS;Database=CleanArchitecture;Integrated Security=true;Encrypt=False;
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
ELASTIC_URL=http://elasticsearch:9200
SEQ_URL=http://seq:80
```

### Azure PaaS (App Settings)
```bash
ConnectionStrings__Database=Server=sql-cleanarch.database.windows.net;Database=CleanArchitecture;Authentication=Active Directory Managed Identity;
ServiceBus__ConnectionString=Endpoint=sb://sb-cleanarch.servicebus.windows.net/;SharedAccessKeyName=...
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=...;IngestionEndpoint=https://...
```

### Azure VMs (App Settings)
```bash
ConnectionStrings__Database=Server=<sql-vm-ip>,1433;Database=CleanArchitecture;User Id=sa;Password=...;
RabbitMQ__Host=<rabbitmq-vm-ip>
RabbitMQ__Username=admin
RabbitMQ__Password=StrongPassword123!
ELASTIC_URL=http://<elasticsearch-vm-ip>:9200
```

---

## Common Commands

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web.Api

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api

# Remove last migration
dotnet ef migrations remove --project src/Infrastructure --startup-project src/Web.Api
```

### Docker
```bash
# Build image
docker build -f src/Web.Api/Dockerfile -t web-api:latest .

# Run container
docker run -p 8080:8080 -e ConnectionStrings__Database="..." web-api:latest

# View logs
docker logs -f web-api

# Shell into container
docker exec -it web-api /bin/bash
```

### Azure CLI
```bash
# Login
az login

# List resource groups
az group list --output table

# View app logs
az webapp log tail --name app-cleanarch --resource-group rg-cleanarch

# Restart app
az webapp restart --name app-cleanarch --resource-group rg-cleanarch

# List app settings
az webapp config appsettings list --name app-cleanarch --resource-group rg-cleanarch
```

---

## Port Reference

| Service | Port | URL |
|---------|------|-----|
| Web API | 8000 | http://localhost:8000 |
| Swagger | 8000 | http://localhost:8000/swagger |
| SQL Server | 1433 | Server=localhost,1433 |
| RabbitMQ (AMQP) | 5672 | amqp://localhost:5672 |
| RabbitMQ (Management) | 15672 | http://localhost:15672 |
| Elasticsearch | 9200 | http://localhost:9200 |
| Seq | 5341 | http://localhost:5341 |
| Prometheus | 9090 | http://localhost:9090 |
| Jaeger | 16686 | http://localhost:16686 |
| Loki | 3100 | http://localhost:3100 |

---

## Cost Reference (Monthly USD)

### Azure PaaS
| Service | Tier | Cost |
|---------|------|------|
| Azure SQL Database | S3 (100 DTU) | $120 |
| Azure Service Bus | Standard | $10 + usage |
| Application Insights | 5GB free, then $2.30/GB | $0-230 |
| App Service | B1 | $13 |
| **Total** | | **~$143-373** |

### Self-Hosted VMs
| Service | VM Size | Cost |
|---------|---------|------|
| SQL Server | Standard_D4s_v3 | $280 |
| RabbitMQ | Standard_B2s | $30 |
| Elasticsearch | Standard_D2s_v3 | $140 |
| Seq | Standard_B2s | $30 |
| Application | Standard_D2s_v3 | $140 |
| **Total** | | **~$620** |

**Note:** Add 20% for storage, networking, backups

---

## Troubleshooting Quick Fixes

### Can't connect to SQL Server
```bash
# Check if running
docker ps | grep sql
# Or on Windows
Get-Service MSSQL*

# Test connection
sqlcmd -S localhost -U sa -P 'YourPassword' -Q "SELECT @@VERSION"
```

### RabbitMQ not accessible
```bash
# Check status
docker logs rabbitmq
# Or on Linux
sudo systemctl status rabbitmq-server

# Reset password
docker exec rabbitmq rabbitmqctl change_password guest guest
```

### Docker Compose issues
```bash
# Clean restart
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d

# Check logs
docker-compose logs -f
```

### Azure deployment fails
```bash
# Check app logs
az webapp log tail --name app-cleanarch --resource-group rg-cleanarch

# View deployment logs
az webapp log deployment show --name app-cleanarch --resource-group rg-cleanarch

# Restart app
az webapp restart --name app-cleanarch --resource-group rg-cleanarch
```

---

## Next Steps

1. ? Choose deployment strategy: [docs/DEPLOYMENT_DECISION_GUIDE.md](DEPLOYMENT_DECISION_GUIDE.md)
2. ? Set up services: [docs/SERVICES_SETUP.md](SERVICES_SETUP.md)
3. ? Deploy to Azure: [docs/AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) or [docs/AZURE_VM_DEPLOYMENT.md](AZURE_VM_DEPLOYMENT.md)
4. ? Configure monitoring and alerts
5. ? Set up CI/CD pipeline
6. ? Test in staging environment
7. ? Deploy to production

---

## Support Links

- ?? **Full Documentation:** [docs/](../docs/)
- ?? **Report Issues:** https://github.com/olasam4liv/CleanArchitecture/issues
- ?? **Discussions:** https://github.com/olasam4liv/CleanArchitecture/discussions
- ?? **Azure Docs:** https://docs.microsoft.com/azure/
- ?? **Cost Calculator:** https://azure.microsoft.com/pricing/calculator/
