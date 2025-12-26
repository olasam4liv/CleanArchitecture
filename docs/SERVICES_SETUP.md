# Dependency Services Setup Guide

This guide covers setting up all required dependency services for the Clean Architecture application in different environments.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Local Development Setup (Docker)](#local-development-setup-docker)
- [Manual Server Setup (Windows/Linux)](#manual-server-setup-windowslinux)
- [Azure Cloud Setup](#azure-cloud-setup)
- [Service Configuration Details](#service-configuration-details)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### For Local Development
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0+
- .NET 10 SDK
- Git

### For Manual Server Setup
- Windows Server 2019+ or Ubuntu 20.04+
- Administrator/Root access
- Open ports: 5672, 15672, 9200, 5341, 9090, 1433

### For Azure Cloud
- Azure Subscription
- Azure CLI installed
- Contributor or Owner role on subscription

---

## Local Development Setup (Docker)

### Quick Start
The easiest way to run all services locally is using Docker Compose:

```bash
# Clone the repository
git clone https://github.com/olasam4liv/CleanArchitecture.git
cd CleanArchitecture

# Create .env file
cp src/Web.Api/.env.example src/Web.Api/.env
# Edit .env with your local settings

# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f web-api
```

### Services Included
All services will be automatically configured and connected:

| Service | Port | Dashboard URL | Default Credentials |
|---------|------|---------------|-------------------|
| Web API | 8000 | http://localhost:8000/swagger | N/A |
| RabbitMQ | 5672 | http://localhost:15672 | guest/guest |
| Elasticsearch | 9200 | http://localhost:9200 | N/A |
| Seq | 5341 | http://localhost:5341 | N/A |
| Prometheus | 9090 | http://localhost:9090 | N/A |
| Jaeger | 16686 | http://localhost:16686 | N/A |
| Loki | 3100 | http://localhost:3100 | N/A |

### Stopping Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

---

## Manual Server Setup (Windows/Linux)

Use this when you need to install services directly on a server without Docker.

### 1. SQL Server Setup

#### Windows Server
```powershell
# Download SQL Server Express
# Visit: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Install using GUI or command line
Setup.exe /Q /ACTION=Install /FEATURES=SQLEngine /INSTANCENAME=SQLEXPRESS /SECURITYMODE=SQL /SAPWD="YourStrongPassword123!" /SQLSVCACCOUNT="NT AUTHORITY\SYSTEM" /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /TCPENABLED=1

# Enable TCP/IP
Import-Module SQLPS
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQLServer\SuperSocketNetLib\Tcp\IPAll" -Name TcpPort -Value 1433

# Restart SQL Server
Restart-Service -Name "MSSQL`$SQLEXPRESS"

# Create database
sqlcmd -S localhost\SQLEXPRESS -Q "CREATE DATABASE CleanArchitecture"
```

#### Linux (Ubuntu)
```bash
# Add Microsoft repository
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list)"

# Install SQL Server
sudo apt-get update
sudo apt-get install -y mssql-server

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup
# Choose: 2) Developer Edition
# Set SA password (strong password required)

# Enable SQL Server to start on boot
sudo systemctl enable mssql-server
sudo systemctl start mssql-server

# Create database
sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "CREATE DATABASE CleanArchitecture"
```

**Connection String:**
```
Server=localhost;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;
```

---

### 2. RabbitMQ Setup

#### Windows Server
```powershell
# Install Erlang (required for RabbitMQ)
# Download from: https://www.erlang.org/downloads
# Install using the .exe installer

# Download RabbitMQ
# Visit: https://www.rabbitmq.com/download.html

# Install RabbitMQ using the installer

# Enable Management Plugin
cd "C:\Program Files\RabbitMQ Server\rabbitmq_server-3.x.x\sbin"
.\rabbitmq-plugins.exe enable rabbitmq_management

# Restart RabbitMQ
Restart-Service RabbitMQ

# Access Management UI: http://localhost:15672 (guest/guest)
```

#### Linux (Ubuntu)
```bash
# Install Erlang
sudo apt-get install -y erlang

# Add RabbitMQ repository
curl -fsSL https://packagecloud.io/rabbitmq/rabbitmq-server/gpgkey | sudo apt-key add -
sudo add-apt-repository "deb https://packagecloud.io/rabbitmq/rabbitmq-server/ubuntu/ focal main"

# Install RabbitMQ
sudo apt-get update
sudo apt-get install -y rabbitmq-server

# Enable and start service
sudo systemctl enable rabbitmq-server
sudo systemctl start rabbitmq-server

# Enable Management Plugin
sudo rabbitmq-plugins enable rabbitmq_management

# Create admin user (optional, but recommended)
sudo rabbitmqctl add_user admin StrongPassword123!
sudo rabbitmqctl set_user_tags admin administrator
sudo rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"

# Access Management UI: http://localhost:15672 (guest/guest or admin/StrongPassword123!)
```

**Configuration:**
```bash
# RabbitMQ host: localhost
# Port: 5672 (AMQP), 15672 (Management UI)
# Username: guest (or admin)
# Password: guest (or StrongPassword123!)
```

---

### 3. Elasticsearch Setup

#### Windows Server
```powershell
# Download Elasticsearch
# Visit: https://www.elastic.co/downloads/elasticsearch

# Extract to C:\elasticsearch-8.x.x

# Disable security for development (optional)
# Edit config\elasticsearch.yml
# Add: xpack.security.enabled: false

# Run Elasticsearch
cd C:\elasticsearch-8.x.x\bin
.\elasticsearch.bat

# Or install as Windows Service
.\elasticsearch-service.bat install
.\elasticsearch-service.bat start

# Test: http://localhost:9200
```

#### Linux (Ubuntu)
```bash
# Import Elasticsearch GPG key
wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | sudo apt-key add -

# Add repository
echo "deb https://artifacts.elastic.co/packages/8.x/apt stable main" | sudo tee /etc/apt/sources.list.d/elastic-8.x.list

# Install Elasticsearch
sudo apt-get update
sudo apt-get install -y elasticsearch

# Configure Elasticsearch
sudo nano /etc/elasticsearch/elasticsearch.yml
# Uncomment and set:
# network.host: 0.0.0.0
# http.port: 9200
# xpack.security.enabled: false  # For development only

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable elasticsearch.service
sudo systemctl start elasticsearch.service

# Test
curl -X GET "localhost:9200"
```

**Configuration:**
```
ELASTIC_URL=http://localhost:9200
```

---

### 4. Seq (Log Server) Setup

#### Windows/Linux
```bash
# Download Seq
# Windows: https://datalust.co/download
# Linux: Use Docker (recommended)

# Windows Installation
# Run the .msi installer

# Linux Installation (Docker)
docker run -d \
  --name seq \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -p 5342:5341 \
  -v seq-data:/data \
  datalust/seq:latest

# Access UI: http://localhost:5341
# Default: No authentication required
```

**Configuration:**
```
SEQ_URL=http://localhost:5341
```

---

### 5. Prometheus Setup

#### Windows Server
```powershell
# Download Prometheus
# Visit: https://prometheus.io/download/

# Extract to C:\prometheus

# Create prometheus.yml configuration file
# (See configuration section below)

# Run Prometheus
cd C:\prometheus
.\prometheus.exe --config.file=prometheus.yml

# Access UI: http://localhost:9090
```

#### Linux (Ubuntu)
```bash
# Create Prometheus user
sudo useradd --no-create-home --shell /bin/false prometheus

# Download Prometheus
cd /tmp
wget https://github.com/prometheus/prometheus/releases/download/v2.45.0/prometheus-2.45.0.linux-amd64.tar.gz
tar -xvf prometheus-2.45.0.linux-amd64.tar.gz

# Move binaries
sudo cp prometheus-2.45.0.linux-amd64/prometheus /usr/local/bin/
sudo cp prometheus-2.45.0.linux-amd64/promtool /usr/local/bin/
sudo chown prometheus:prometheus /usr/local/bin/prometheus
sudo chown prometheus:prometheus /usr/local/bin/promtool

# Create directories
sudo mkdir -p /etc/prometheus
sudo mkdir -p /var/lib/prometheus
sudo chown prometheus:prometheus /var/lib/prometheus

# Copy configuration
sudo cp prometheus-2.45.0.linux-amd64/prometheus.yml /etc/prometheus/
sudo chown prometheus:prometheus /etc/prometheus/prometheus.yml

# Create systemd service
sudo nano /etc/systemd/system/prometheus.service
```

**prometheus.service:**
```ini
[Unit]
Description=Prometheus
Wants=network-online.target
After=network-online.target

[Service]
User=prometheus
Group=prometheus
Type=simple
ExecStart=/usr/local/bin/prometheus \
  --config.file=/etc/prometheus/prometheus.yml \
  --storage.tsdb.path=/var/lib/prometheus/ \
  --web.console.templates=/etc/prometheus/consoles \
  --web.console.libraries=/etc/prometheus/console_libraries

[Install]
WantedBy=multi-user.target
```

```bash
# Start Prometheus
sudo systemctl daemon-reload
sudo systemctl enable prometheus
sudo systemctl start prometheus

# Access UI: http://localhost:9090
```

---

### 6. Jaeger (Distributed Tracing) Setup

#### Using Docker (Recommended)
```bash
docker run -d \
  --name jaeger \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 16686:16686 \
  -p 14250:14250 \
  -p 14268:14268 \
  jaegertracing/all-in-one:1.57

# Access UI: http://localhost:16686
```

#### Binary Installation (Linux)
```bash
# Download Jaeger
wget https://github.com/jaegertracing/jaeger/releases/download/v1.57.0/jaeger-1.57.0-linux-amd64.tar.gz
tar -xvf jaeger-1.57.0-linux-amd64.tar.gz

# Run Jaeger All-in-One
cd jaeger-1.57.0-linux-amd64
./jaeger-all-in-one --collector.otlp.enabled=true

# Access UI: http://localhost:16686
```

---

## Azure Cloud Setup

For production deployments, use Azure managed services instead of self-hosted ones.

### 1. Azure SQL Database

```bash
# Login to Azure
az login

# Create Resource Group
az group create --name rg-cleanarch-prod --location eastus

# Create SQL Server
az sql server create \
  --name sql-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --location eastus \
  --admin-user sqladmin \
  --admin-password 'YourStrongPassword123!'

# Configure firewall (allow Azure services)
az sql server firewall-rule create \
  --resource-group rg-cleanarch-prod \
  --server sql-cleanarch-prod \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group rg-cleanarch-prod \
  --server sql-cleanarch-prod \
  --name CleanArchitecture \
  --service-objective S0 \
  --zone-redundant false
```

**Connection String:**
```
Server=sql-cleanarch-prod.database.windows.net;Database=CleanArchitecture;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=True;
```

---

### 2. Azure Service Bus (Replaces RabbitMQ)

```bash
# Create Service Bus Namespace
az servicebus namespace create \
  --name sb-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --location eastus \
  --sku Standard

# Create Queue (example)
az servicebus queue create \
  --name todo-events \
  --namespace-name sb-cleanarch-prod \
  --resource-group rg-cleanarch-prod

# Create Topic (for pub/sub)
az servicebus topic create \
  --name domain-events \
  --namespace-name sb-cleanarch-prod \
  --resource-group rg-cleanarch-prod

# Create Subscription
az servicebus topic subscription create \
  --name web-worker-subscription \
  --topic-name domain-events \
  --namespace-name sb-cleanarch-prod \
  --resource-group rg-cleanarch-prod

# Get connection string
az servicebus namespace authorization-rule keys list \
  --resource-group rg-cleanarch-prod \
  --namespace-name sb-cleanarch-prod \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

**Configuration:**
```
ServiceBus__ConnectionString=Endpoint=sb://sb-cleanarch-prod.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key
```

---

### 3. Application Insights (Replaces Elasticsearch/Seq/Prometheus/Jaeger)

```bash
# Create Application Insights
az monitor app-insights component create \
  --app ai-cleanarch-prod \
  --location eastus \
  --resource-group rg-cleanarch-prod \
  --application-type web

# Get instrumentation key
az monitor app-insights component show \
  --app ai-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --query instrumentationKey \
  --output tsv

# Get connection string
az monitor app-insights component show \
  --app ai-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --query connectionString \
  --output tsv
```

**Configuration:**
```
APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=your-key;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/
```

---

### 4. Azure Container Registry (For Docker Images)

```bash
# Create Container Registry
az acr create \
  --name acrcleanarchprod \
  --resource-group rg-cleanarch-prod \
  --sku Basic \
  --admin-enabled true

# Get login credentials
az acr credential show \
  --name acrcleanarchprod \
  --resource-group rg-cleanarch-prod

# Login to ACR
az acr login --name acrcleanarchprod

# Build and push image
az acr build \
  --registry acrcleanarchprod \
  --image web-api:latest \
  --file src/Web.Api/Dockerfile \
  .
```

---

### 5. Azure App Service (Linux Container)

```bash
# Create App Service Plan
az appservice plan create \
  --name plan-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --is-linux \
  --sku B1

# Create Web App
az webapp create \
  --name app-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --plan plan-cleanarch-prod \
  --deployment-container-image-name acrcleanarchprod.azurecr.io/web-api:latest

# Configure App Service to use ACR
az webapp config container set \
  --name app-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --docker-custom-image-name acrcleanarchprod.azurecr.io/web-api:latest \
  --docker-registry-server-url https://acrcleanarchprod.azurecr.io \
  --docker-registry-server-user $(az acr credential show -n acrcleanarchprod --query username -o tsv) \
  --docker-registry-server-password $(az acr credential show -n acrcleanarchprod --query passwords[0].value -o tsv)

# Enable Managed Identity
az webapp identity assign \
  --name app-cleanarch-prod \
  --resource-group rg-cleanarch-prod

# Configure App Settings
az webapp config appsettings set \
  --name app-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Database="Server=sql-cleanarch-prod.database.windows.net;Database=CleanArchitecture;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=True;" \
    Jwt__Secret="your-production-jwt-secret-minimum-32-characters" \
    Jwt__Issuer="https://app-cleanarch-prod.azurewebsites.net" \
    Jwt__Audience="https://app-cleanarch-prod.azurewebsites.net" \
    APPLICATIONINSIGHTS_CONNECTION_STRING="your-app-insights-connection-string"
```

---

### 6. Azure Key Vault (For Secrets)

```bash
# Create Key Vault
az keyvault create \
  --name kv-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --location eastus

# Store secrets
az keyvault secret set \
  --vault-name kv-cleanarch-prod \
  --name JwtSecret \
  --value "your-production-jwt-secret-minimum-32-characters"

az keyvault secret set \
  --vault-name kv-cleanarch-prod \
  --name SqlConnectionString \
  --value "Server=sql-cleanarch-prod.database.windows.net;Database=CleanArchitecture;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=True;"

# Grant App Service access to Key Vault
# Get App Service identity
APP_IDENTITY=$(az webapp identity show --name app-cleanarch-prod --resource-group rg-cleanarch-prod --query principalId -o tsv)

# Grant access
az keyvault set-policy \
  --name kv-cleanarch-prod \
  --object-id $APP_IDENTITY \
  --secret-permissions get list

# Update App Settings to reference Key Vault
az webapp config appsettings set \
  --name app-cleanarch-prod \
  --resource-group rg-cleanarch-prod \
  --settings \
    Jwt__Secret="@Microsoft.KeyVault(SecretUri=https://kv-cleanarch-prod.vault.azure.net/secrets/JwtSecret/)" \
    ConnectionStrings__Database="@Microsoft.KeyVault(SecretUri=https://kv-cleanarch-prod.vault.azure.net/secrets/SqlConnectionString/)"
```

---

## Service Configuration Details

### Required Configuration Files

#### 1. prometheus.yml
Create this file for Prometheus monitoring:

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'clean-architecture-api'
    static_configs:
      - targets: ['localhost:8000']
    metrics_path: '/metrics'
```

#### 2. otel-collector-config.yaml
Create this file for OpenTelemetry:

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:

exporters:
  logging:
    loglevel: debug
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true
  prometheus:
    endpoint: "0.0.0.0:8889"

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, jaeger]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, prometheus]
```

#### 3. Loki config (`.containers/loki/config.yml`)
```yaml
auth_enabled: false

server:
  http_listen_port: 3100

ingester:
  lifecycler:
    address: 127.0.0.1
    ring:
      kvstore:
        store: inmemory
      replication_factor: 1
    final_sleep: 0s
  chunk_idle_period: 5m
  chunk_retain_period: 30s

schema_config:
  configs:
    - from: 2020-05-15
      store: boltdb
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 168h

storage_config:
  boltdb:
    directory: /loki/index
  filesystem:
    directory: /loki/chunks

limits_config:
  enforce_metric_name: false
  reject_old_samples: true
  reject_old_samples_max_age: 168h

chunk_store_config:
  max_look_back_period: 0s

table_manager:
  retention_deletes_enabled: false
  retention_period: 0s
```

---

## Troubleshooting

### Common Issues

#### 1. SQL Server Connection Issues
```bash
# Check if SQL Server is running
# Windows:
Get-Service MSSQL*

# Linux:
sudo systemctl status mssql-server

# Test connection
sqlcmd -S localhost -U sa -P 'YourPassword' -Q "SELECT @@VERSION"
```

#### 2. RabbitMQ Not Accessible
```bash
# Check RabbitMQ status
# Windows:
Get-Service RabbitMQ

# Linux:
sudo systemctl status rabbitmq-server

# Check logs
# Windows: C:\Users\{User}\AppData\Roaming\RabbitMQ\log\
# Linux: /var/log/rabbitmq/

# Reset guest user password
sudo rabbitmqctl change_password guest guest
```

#### 3. Elasticsearch Memory Issues
```bash
# Increase heap size
# Edit jvm.options
-Xms2g
-Xmx2g

# Or set environment variable
export ES_JAVA_OPTS="-Xms2g -Xmx2g"
```

#### 4. Port Already in Use
```powershell
# Windows: Find process using port
netstat -ano | findstr :8000
taskkill /PID <process_id> /F

# Linux: Find and kill process
sudo lsof -i :8000
sudo kill -9 <process_id>
```

#### 5. Docker Compose Issues
```bash
# Clean rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d

# View logs for specific service
docker-compose logs -f web-api

# Check resource usage
docker stats
```

#### 6. Azure Connection Issues
```bash
# Test connectivity
curl https://your-app.azurewebsites.net/health

# View App Service logs
az webapp log tail --name app-cleanarch-prod --resource-group rg-cleanarch-prod

# Check App Settings
az webapp config appsettings list --name app-cleanarch-prod --resource-group rg-cleanarch-prod
```

---

## Performance Tuning

### SQL Server
```sql
-- Enable query store
ALTER DATABASE CleanArchitecture SET QUERY_STORE = ON;

-- Update statistics
EXEC sp_updatestats;

-- Rebuild indexes
ALTER INDEX ALL ON [dbo].[Users] REBUILD;
```

### RabbitMQ
```bash
# Increase file descriptors
sudo rabbitmqctl set_vm_memory_high_watermark 0.6

# Enable lazy queues for large backlogs
rabbitmqctl set_policy lazy-queue "^lazy-queue-" '{"queue-mode":"lazy"}' --apply-to queues
```

### Elasticsearch
```bash
# Increase refresh interval for better indexing performance
curl -X PUT "localhost:9200/logs/_settings" -H 'Content-Type: application/json' -d'
{
  "index": {
    "refresh_interval": "30s"
  }
}'
```

---

## Security Checklist

### Before Production

- [ ] Change all default passwords
- [ ] Enable TLS/SSL on all services
- [ ] Configure firewalls to restrict access
- [ ] Enable authentication on Elasticsearch
- [ ] Use strong JWT secrets (minimum 32 characters)
- [ ] Store secrets in Azure Key Vault or environment variables
- [ ] Enable Azure Defender for all Azure services
- [ ] Configure backup policies for databases
- [ ] Set up monitoring and alerts
- [ ] Implement rate limiting
- [ ] Enable audit logging
- [ ] Review and update security groups/NSGs

---

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Elasticsearch Documentation](https://www.elastic.co/guide/index.html)
- [Azure SQL Database Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)

---

## Support

For issues specific to this application, please open an issue on GitHub:
https://github.com/olasam4liv/CleanArchitecture/issues

For Azure support, contact Azure Support or visit:
https://azure.microsoft.com/support/
