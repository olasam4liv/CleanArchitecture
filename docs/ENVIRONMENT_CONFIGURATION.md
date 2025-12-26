# Environment Configuration Strategy

## Overview
This project uses **different configurations** for local development and Azure production deployment.

## Local Development (Docker Compose)

### Configuration Location
- **File**: `docker-compose.yml`
- **Purpose**: Run all services locally in containers
- **Environment Variables**: Defined in `docker-compose.yml` `environment:` section

### Services Included
- Web API (your application)
- RabbitMQ (message queue)
- Elasticsearch (search & logs)
- Seq (log viewer)
- Prometheus (metrics)
- Jaeger (tracing)
- OpenTelemetry Collector

### Running Locally
```bash
docker-compose up -d
```

Access:
- API: http://localhost:8000
- Swagger: http://localhost:8000/swagger
- Seq Logs: http://localhost:5341
- RabbitMQ Management: http://localhost:15672
- Jaeger UI: http://localhost:16686
- Prometheus: http://localhost:9090

---

## Azure Production Deployment

### Configuration Location
- **Azure App Service ? Configuration ? Application Settings**
- **Purpose**: Production deployment with Azure-managed services
- **Environment Variables**: Configured in Azure Portal

### Azure Deployment Options

You have **multiple options** for deploying dependency services in Azure:

#### Option 1: Azure Native Managed Services (Recommended)
| Local Service | Azure Native Service | Benefits |
|--------------|---------------------|----------|
| RabbitMQ | Azure Service Bus | Fully managed, auto-scaling, HA |
| Elasticsearch | Application Insights | Native .NET integration, automatic collection |
| Seq | Application Insights Logs | Integrated with Azure ecosystem |
| Prometheus | Azure Monitor Metrics | Built-in dashboards, alerts |
| Jaeger | Application Insights Distributed Tracing | End-to-end visibility |
| SQL Server Express | Azure SQL Database | Automatic backups, scaling, geo-replication |

**Pros:** No infrastructure management, auto-scaling, highly available  
**Cons:** Requires code changes, vendor lock-in

#### Option 2: Self-Hosted on Azure VMs

**A) Single Server (All Services on One VM) - Cost-Effective**

| All Services | Single Ubuntu VM | Connection |
|--------------|------------------|------------|
| SQL Server | Port 1433 | `<server-ip>:1433` |
| RabbitMQ | Port 5672 (AMQP), 15672 (Management) | `<server-ip>:5672` |
| Elasticsearch | Port 9200 | `http://<server-ip>:9200` |
| Seq | Port 5341 | `http://<server-ip>:5341` |
| Prometheus | Port 9090 | `http://<server-ip>:9090` |
| Jaeger | Port 16686 | `http://<server-ip>:16686` |
| Your App | Port 8080 | `http://<server-ip>:8080` |

**VM Specs:** Standard_D4s_v3 (4 vCPU, 16GB RAM) or Standard_D8s_v3 (8 vCPU, 32GB RAM)  
**Cost:** ~$310/month (vs ~$590 for separate VMs)  
**Best For:** Small to medium workloads, budget-conscious deployments

?? **Complete Setup Guide:** [docs/SINGLE_SERVER_SETUP.md](./SINGLE_SERVER_SETUP.md)

**B) Multiple VMs (Separate Server Per Service) - Production Scale**

| Service | Deployment Method | Connection |
|---------|-------------------|------------|
| RabbitMQ | Ubuntu VM + RabbitMQ install | `<vm-public-ip>:5672` |
| Elasticsearch | Ubuntu VM + Elasticsearch install | `http://<vm-public-ip>:9200` |
| Seq | Windows/Linux VM + Seq install | `http://<vm-public-ip>:5341` |
| Prometheus | Ubuntu VM + Prometheus install | `http://<vm-public-ip>:9090` |
| SQL Server | SQL Server VM image | `<vm-public-ip>,1433` |

**Cost:** ~$590/month  
**Best For:** High availability, independent scaling, production with strict SLAs

?? **Complete Setup Guide:** [docs/AZURE_VM_DEPLOYMENT.md](./AZURE_VM_DEPLOYMENT.md)

**Pros:** Same stack as local, no code changes, cloud-agnostic  
**Cons:** You manage updates, patches, high availability

#### Option 3: Third-Party Managed Services
| Service | Managed Provider | Details |
|---------|-----------------|---------|
| Elasticsearch | Elastic Cloud on Azure | elastic.co/cloud |
| RabbitMQ | CloudAMQP | cloudamqp.com |
| PostgreSQL | Azure Database for PostgreSQL | Managed PostgreSQL |

**Pros:** Managed by experts, minimal code changes  
**Cons:** Additional cost, third-party dependency

#### Option 4: Azure Container Instances (Docker)
Deploy your entire `docker-compose.yml` stack to Azure Container Instances:

```bash
# Convert docker-compose to Azure Container Instances
az container create \
  --resource-group rg-cleanarch \
  --name aci-cleanarch \
  --image your-acr.azurecr.io/web-api:latest \
  --ports 8080 \
  --environment-variables \
    RabbitMQ__Host=<rabbitmq-aci-dns> \
    ELASTIC_URL=http://<elastic-aci-dns>:9200
```

**Pros:** Container-based, Docker Compose compatible  
**Cons:** Limited orchestration features vs Kubernetes

### Deployment Process
1. **Build Docker Image**
   ```bash
   docker build -f src/Web.Api/Dockerfile -t your-acr.azurecr.io/web-api:latest .
   ```

2. **Push to Azure Container Registry**
   ```bash
   docker push your-acr.azurecr.io/web-api:latest
   ```

3. **Deploy to App Service**
   ```bash
   az webapp config container set \
     --name your-app-service \
     --resource-group your-rg \
     --docker-custom-image-name your-acr.azurecr.io/web-api:latest
   ```

4. **Configure Application Settings** in Azure Portal

For detailed Azure configuration, see: **[docs/AZURE_DEPLOYMENT.md](./AZURE_DEPLOYMENT.md)**

---

## Key Principles

### ? DO:
- ? Keep environment-specific settings **separate** from Docker image
- ? Use `docker-compose.yml` for **local development only**
- ? Configure Azure settings in **Azure Portal or ARM/Bicep**
- ? Store secrets in **Azure Key Vault**
- ? Use **Managed Identity** for Azure service authentication

### ? DON'T:
- ? Don't hardcode environment variables in Dockerfile
- ? Don't use Docker Compose in Azure (not supported)
- ? Don't commit secrets to source control
- ? Don't use localhost URLs in Azure configuration
- ? Don't use Docker Compose service names in Azure

---

## Configuration Comparison

### Connection String Examples

#### Local Development
```
Server=host.docker.internal\SQLEXPRESS;Database=CleanArchitecture;Integrated Security=true;Encrypt=False;
```

#### Azure Production
```
Server=your-server.database.windows.net;Database=CleanArchitecture;Authentication=Active Directory Managed Identity;Encrypt=True;
```

### RabbitMQ Configuration

#### Local Development
```
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest
```

#### Azure Production
```
# Use Azure Service Bus connection string
ServiceBus__ConnectionString=Endpoint=sb://your-namespace.servicebus.windows.net/;...
```

---

## Security Notes

### Local Development
- Uses default credentials (acceptable for local)
- No encryption on some connections
- Secrets in docker-compose.yml (not committed to Git)

### Azure Production
- **Always** use managed identities
- **Always** encrypt connections (TLS/SSL)
- **Never** store secrets in App Settings (use Key Vault references)
- Enable Azure Defender for all services

---

## Migration Checklist

When moving from local to Azure:

- [ ] Provision Azure SQL Database
- [ ] Provision Azure Service Bus
- [ ] Provision Application Insights
- [ ] Provision Azure Container Registry
- [ ] Provision Azure App Service (Linux, Container)
- [ ] Enable Managed Identity on App Service
- [ ] Configure Application Settings in Azure Portal
- [ ] Update Infrastructure code to use Azure SDKs
- [ ] Set up Key Vault for secrets
- [ ] Configure CI/CD pipeline
- [ ] Test in Azure staging environment
- [ ] Configure custom domain & SSL
- [ ] Set up monitoring alerts

---

## References

- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database](https://docs.microsoft.com/azure/azure-sql/)
- [Azure Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Application Insights](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [Managed Identity](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)
