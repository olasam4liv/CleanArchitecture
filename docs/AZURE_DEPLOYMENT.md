# Azure App Service Configuration Settings

## Required Application Settings for Azure App Service

When deploying to Azure App Service, configure these settings in:
**Azure Portal ? Your App Service ? Configuration ? Application Settings**

### Core Settings
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### Database Configuration
```
ConnectionStrings__Database=Server=your-azure-sql-server.database.windows.net;Database=CleanArchitecture;User Id=your-admin-user;Password=your-password;Encrypt=True;TrustServerCertificate=False;
```

**Recommendation**: Use Azure SQL Database with Managed Identity:
```
ConnectionStrings__Database=Server=your-azure-sql-server.database.windows.net;Database=CleanArchitecture;Authentication=Active Directory Managed Identity;Encrypt=True;
```

### JWT Configuration
```
Jwt__Secret=your-production-jwt-secret-key-minimum-32-characters
Jwt__Issuer=https://your-domain.azurewebsites.net
Jwt__Audience=https://your-domain.azurewebsites.net
```

**Recommendation**: Store JWT__Secret in **Azure Key Vault** and reference it:
```
@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/JwtSecret/)
```

### Messaging - Azure Service Bus (Replace RabbitMQ)
```
RabbitMQ__Host=your-servicebus-namespace.servicebus.windows.net
RabbitMQ__Username=$ConnectionString
RabbitMQ__Password=Endpoint=sb://your-servicebus-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key
```

**Better Approach**: Update code to use Azure Service Bus SDK directly instead of RabbitMQ/MassTransit compatibility layer.

### Logging & Monitoring - Application Insights (Replace Elasticsearch/Seq)
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=your-instrumentation-key;IngestionEndpoint=https://region.in.applicationinsights.azure.com/;LiveEndpoint=https://region.livediagnostics.monitor.azure.com/
```

No need for:
- ~~ELASTIC_URL~~ ? Use Application Insights
- ~~SEQ_URL~~ ? Use Application Insights / Log Analytics
- ~~SEQ_API_KEY~~ ? Use Application Insights

### OpenTelemetry Configuration (Optional - if keeping OTel)
```
OpenTelemetry__Otlp__Endpoint=https://your-otel-collector.azurewebsites.net:4317
```

**Recommendation**: Use Application Insights instead, which has native .NET integration:
- Remove OpenTelemetry__Otlp__Endpoint
- Application Insights will auto-collect telemetry

## Azure Services to Provision

### Required Services
1. **Azure App Service** (Linux, .NET 10)
   - Container deployment
   - Always On enabled
   - Managed Identity enabled

2. **Azure SQL Database**
   - Standard or Premium tier
   - Geo-replication (optional)
   - Firewall rules for App Service

3. **Azure Service Bus**
   - Standard or Premium tier
   - Topics/Queues for your message types
   - Managed Identity access

4. **Application Insights**
   - Connected to App Service
   - Log Analytics workspace
   - Automatic instrumentation

### Optional Services
5. **Azure Key Vault**
   - Store secrets (JWT keys, connection strings)
   - Managed Identity access from App Service

6. **Azure Container Registry (ACR)**
   - Store your Docker images
   - Managed Identity pull access

7. **Azure Front Door or Application Gateway**
   - SSL/TLS termination
   - WAF protection
   - CDN for static assets

## Deployment Options

### Option 1: GitHub Actions (Recommended)
```yaml
# .github/workflows/deploy-to-azure.yml
- name: Build and push Docker image to ACR
- name: Deploy to Azure App Service
```

### Option 2: Azure DevOps Pipelines
- Build Docker image
- Push to ACR
- Deploy to App Service

### Option 3: Direct Docker Deploy
```bash
az acr build --registry your-acr --image web-api:latest .
az webapp config container set --name your-app --resource-group your-rg --docker-custom-image-name your-acr.azurecr.io/web-api:latest
```

## Connection String Format Changes

### Local (Docker Compose)
```
Server=host.docker.internal\SQLEXPRESS;Database=CleanArchitecture;Integrated Security=true;Encrypt=False;
```

### Azure App Service
```
Server=your-server.database.windows.net;Database=CleanArchitecture;Authentication=Active Directory Managed Identity;Encrypt=True;
```

## Migration to Azure Services

### Current ? Azure Mapping
- **RabbitMQ** ? Azure Service Bus
- **Elasticsearch** ? Application Insights + Log Analytics
- **Seq** ? Application Insights Logs
- **Prometheus** ? Azure Monitor Metrics
- **Grafana** ? Azure Monitor Dashboards
- **Jaeger** ? Application Insights Distributed Tracing
- **Loki** ? Azure Monitor Logs
- **SQL Server Express** ? Azure SQL Database

## Cost Optimization Tips
1. Use **Managed Identity** instead of connection strings
2. Enable **auto-scaling** for App Service
3. Use **Azure SQL Database Serverless** for dev/test
4. Configure **log retention** policies
5. Use **Azure Service Bus Basic** tier initially

## Security Best Practices
1. ? Store secrets in Key Vault
2. ? Enable Managed Identity for all service-to-service calls
3. ? Use Private Endpoints for SQL Database
4. ? Enable WAF on Application Gateway
5. ? Configure CORS policies properly
6. ? Use HTTPS only
7. ? Enable Azure Defender for App Service

## Local vs Azure Environment Variables

| Variable | Local (docker-compose.yml) | Azure (App Settings) |
|----------|---------------------------|----------------------|
| Database | host.docker.internal\SQLEXPRESS | Azure SQL FQDN |
| RabbitMQ | rabbitmq (service name) | Azure Service Bus endpoint |
| Elasticsearch | elasticsearch:9200 | Application Insights connection string |
| Seq | seq:80 | Not needed (use App Insights) |

---

## Next Steps for Azure Deployment

1. **Provision Azure resources** using Azure Portal or ARM/Bicep templates
2. **Update Infrastructure layer** to use Azure SDKs:
   - Azure Service Bus SDK (replace MassTransit/RabbitMQ)
   - Application Insights SDK
3. **Configure Managed Identity** for secure service access
4. **Set up CI/CD pipeline** for automated deployments
5. **Test in Azure** staging environment before production
