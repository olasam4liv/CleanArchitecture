# All-in-One Server Setup Guide (Linux)

This guide shows you how to install **all dependency services on a single Linux server** (Ubuntu 22.04 LTS) for cost-effective deployment.

## Overview

Instead of creating separate VMs for each service, you'll run everything on one server:

```
??????????????????????????????????????????????????????
?         Single Linux Server (Ubuntu 22.04)         ?
?                                                    ?
?  ????????????????  ????????????????              ?
?  ? SQL Server   ?  ?  RabbitMQ    ?              ?
?  ? Port: 1433   ?  ?  Port: 5672  ?              ?
?  ????????????????  ????????????????              ?
?                                                    ?
?  ????????????????  ????????????????              ?
?  ?Elasticsearch ?  ?     Seq      ?              ?
?  ? Port: 9200   ?  ?  Port: 5341  ?              ?
?  ????????????????  ????????????????              ?
?                                                    ?
?  ????????????????  ????????????????              ?
?  ?  Prometheus  ?  ?   Jaeger     ?              ?
?  ? Port: 9090   ?  ?  Port: 16686 ?              ?
?  ????????????????  ????????????????              ?
?                                                    ?
?  ????????????????                                 ?
?  ?  Your App    ?                                 ?
?  ? Port: 8080   ?                                 ?
?  ????????????????                                 ?
??????????????????????????????????????????????????????
```

## Server Requirements

### Minimum Specifications
- **CPU:** 4 cores
- **RAM:** 16 GB
- **Storage:** 100 GB SSD
- **OS:** Ubuntu 22.04 LTS Server

### Recommended Specifications (Production)
- **CPU:** 8 cores
- **RAM:** 32 GB
- **Storage:** 250 GB SSD
- **OS:** Ubuntu 22.04 LTS Server

### Azure VM Recommendation
**Standard_D4s_v3** or **Standard_D8s_v3**
- D4s_v3: 4 vCPUs, 16 GB RAM (~$280/month)
- D8s_v3: 8 vCPUs, 32 GB RAM (~$560/month)

---

## Step 1: Provision the Server

### Option A: Azure VM

```bash
# Set variables
RESOURCE_GROUP="rg-cleanarch"
VM_NAME="vm-cleanarch-allinone"
LOCATION="eastus"
VM_SIZE="Standard_D4s_v3"  # 4 vCPU, 16GB RAM
ADMIN_USERNAME="azureuser"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Create VM
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --image Ubuntu2204 \
  --size $VM_SIZE \
  --admin-username $ADMIN_USERNAME \
  --generate-ssh-keys \
  --public-ip-sku Standard \
  --public-ip-address-allocation static

# Open all required ports
az vm open-port --port 1433 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1000
az vm open-port --port 5672 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1010
az vm open-port --port 15672 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1020
az vm open-port --port 9200 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1030
az vm open-port --port 5341 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1040
az vm open-port --port 9090 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1050
az vm open-port --port 16686 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1060
az vm open-port --port 8080 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1070
az vm open-port --port 80 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1080
az vm open-port --port 443 --resource-group $RESOURCE_GROUP --name $VM_NAME --priority 1090

# Get the public IP
VM_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name $VM_NAME \
  --query publicIps -o tsv)

echo "Server IP: $VM_IP"
echo "SSH: ssh $ADMIN_USERNAME@$VM_IP"
```

### Option B: Other Cloud Providers or On-Premises

```bash
# Simply provision an Ubuntu 22.04 server with:
# - 4+ CPU cores
# - 16+ GB RAM
# - 100+ GB storage
# - Public IP address (or accessible from your network)
```

---

## Step 2: Initial Server Setup

SSH into your server and run these commands:

```bash
# SSH to server
ssh azureuser@<your-server-ip>

# Update system
sudo apt-get update && sudo apt-get upgrade -y

# Install essential tools
sudo apt-get install -y \
  curl \
  wget \
  git \
  unzip \
  vim \
  htop \
  net-tools \
  software-properties-common \
  apt-transport-https \
  ca-certificates \
  gnupg \
  lsb-release

# Set timezone (optional)
sudo timedatectl set-timezone UTC

# Configure firewall (UFW)
sudo ufw allow OpenSSH
sudo ufw allow 1433/tcp    # SQL Server
sudo ufw allow 5672/tcp    # RabbitMQ AMQP
sudo ufw allow 15672/tcp   # RabbitMQ Management
sudo ufw allow 9200/tcp    # Elasticsearch
sudo ufw allow 5341/tcp    # Seq
sudo ufw allow 9090/tcp    # Prometheus
sudo ufw allow 16686/tcp   # Jaeger
sudo ufw allow 8080/tcp    # Application
sudo ufw allow 80/tcp      # HTTP
sudo ufw allow 443/tcp     # HTTPS
sudo ufw --force enable
```

---

## Step 3: Install SQL Server

```bash
# Add Microsoft repository
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"

# Install SQL Server
sudo apt-get update
sudo apt-get install -y mssql-server

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup
# Choose: 2) Developer Edition (free)
# Set SA password: YourStrongPassword123!

# Enable SQL Server on startup
sudo systemctl enable mssql-server
sudo systemctl start mssql-server

# Install SQL Server command-line tools
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list
sudo apt-get update
sudo apt-get install -y mssql-tools unixodbc-dev

# Add tools to PATH
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
source ~/.bashrc

# Create database
sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' << EOF
CREATE DATABASE CleanArchitecture;
GO
EXIT
EOF

# Verify installation
sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "SELECT @@VERSION"

echo "? SQL Server installed on port 1433"
```

**Connection String:**
```
Server=<server-ip>,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;
```

---

## Step 4: Install RabbitMQ

```bash
# Install Erlang (required for RabbitMQ)
sudo apt-get install -y erlang

# Add RabbitMQ repository
curl -fsSL https://packagecloud.io/rabbitmq/rabbitmq-server/gpgkey | sudo apt-key add -
sudo add-apt-repository "deb https://packagecloud.io/rabbitmq/rabbitmq-server/ubuntu/ jammy main"

# Install RabbitMQ
sudo apt-get update
sudo apt-get install -y rabbitmq-server

# Enable and start RabbitMQ
sudo systemctl enable rabbitmq-server
sudo systemctl start rabbitmq-server

# Enable management plugin
sudo rabbitmq-plugins enable rabbitmq_management

# Create admin user
sudo rabbitmqctl add_user admin StrongPassword123!
sudo rabbitmqctl set_user_tags admin administrator
sudo rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"

# Optional: Delete default guest user (security best practice)
# sudo rabbitmqctl delete_user guest

# Verify installation
sudo rabbitmqctl status

echo "? RabbitMQ installed"
echo "   AMQP Port: 5672"
echo "   Management UI: http://<server-ip>:15672"
echo "   Username: admin"
echo "   Password: StrongPassword123!"
```

**Application Configuration:**
```
RabbitMQ__Host=<server-ip>
RabbitMQ__Username=admin
RabbitMQ__Password=StrongPassword123!
```

---

## Step 5: Install Elasticsearch

```bash
# Import Elasticsearch GPG key
wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | sudo apt-key add -

# Add repository
echo "deb https://artifacts.elastic.co/packages/8.x/apt stable main" | sudo tee /etc/apt/sources.list.d/elastic-8.x.list

# Install Elasticsearch
sudo apt-get update
sudo apt-get install -y elasticsearch

# Configure Elasticsearch
sudo tee -a /etc/elasticsearch/elasticsearch.yml > /dev/null << EOF

# Network settings
network.host: 0.0.0.0
http.port: 9200

# Security (disable for development - enable in production!)
xpack.security.enabled: false
xpack.security.enrollment.enabled: false

# Single node
discovery.type: single-node

# Memory settings
bootstrap.memory_lock: true
EOF

# Set JVM heap size (use 50% of available RAM, max 32GB)
# For 16GB RAM system, use 8GB
sudo mkdir -p /etc/elasticsearch/jvm.options.d
sudo tee /etc/elasticsearch/jvm.options.d/heap.options > /dev/null << EOF
-Xms8g
-Xmx8g
EOF

# Increase memory lock limit
sudo tee -a /etc/security/limits.conf > /dev/null << EOF
elasticsearch soft memlock unlimited
elasticsearch hard memlock unlimited
EOF

# Configure systemd
sudo mkdir -p /etc/systemd/system/elasticsearch.service.d
sudo tee /etc/systemd/system/elasticsearch.service.d/override.conf > /dev/null << EOF
[Service]
LimitMEMLOCK=infinity
EOF

# Reload systemd and start Elasticsearch
sudo systemctl daemon-reload
sudo systemctl enable elasticsearch.service
sudo systemctl start elasticsearch.service

# Wait for Elasticsearch to start
sleep 30

# Verify installation
curl -X GET "localhost:9200"

echo "? Elasticsearch installed on port 9200"
```

**Application Configuration:**
```
ELASTIC_URL=http://<server-ip>:9200
```

---

## Step 6: Install Seq (via Docker)

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add current user to docker group
sudo usermod -aG docker $USER

# Apply group membership (or logout/login)
newgrp docker

# Run Seq container
docker run -d \
  --name seq \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -p 5342:5341 \
  -v seq-data:/data \
  datalust/seq:latest

# Verify installation
docker ps | grep seq

echo "? Seq installed on port 5341"
echo "   Access: http://<server-ip>:5341"
```

**Application Configuration:**
```
SEQ_URL=http://<server-ip>:5341
```

---

## Step 7: Install Prometheus

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

# Create configuration
sudo tee /etc/prometheus/prometheus.yml > /dev/null << EOF
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
  
  - job_name: 'application'
    static_configs:
      - targets: ['localhost:8080']
EOF

sudo chown prometheus:prometheus /etc/prometheus/prometheus.yml

# Create systemd service
sudo tee /etc/systemd/system/prometheus.service > /dev/null << EOF
[Unit]
Description=Prometheus
Wants=network-online.target
After=network-online.target

[Service]
User=prometheus
Group=prometheus
Type=simple
ExecStart=/usr/local/bin/prometheus \\
  --config.file=/etc/prometheus/prometheus.yml \\
  --storage.tsdb.path=/var/lib/prometheus/ \\
  --web.console.templates=/etc/prometheus/consoles \\
  --web.console.libraries=/etc/prometheus/console_libraries

[Install]
WantedBy=multi-user.target
EOF

# Start Prometheus
sudo systemctl daemon-reload
sudo systemctl enable prometheus
sudo systemctl start prometheus

# Verify installation
curl http://localhost:9090/-/healthy

echo "? Prometheus installed on port 9090"
echo "   Access: http://<server-ip>:9090"
```

---

## Step 8: Install Jaeger (via Docker)

```bash
# Run Jaeger all-in-one
docker run -d \
  --name jaeger \
  --restart unless-stopped \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 16686:16686 \
  -p 14250:14250 \
  -p 14268:14268 \
  -p 4317:4317 \
  -p 4318:4318 \
  jaegertracing/all-in-one:1.57

# Verify installation
docker ps | grep jaeger

echo "? Jaeger installed"
echo "   UI: http://<server-ip>:16686"
echo "   Collector: <server-ip>:4317 (gRPC)"
```

---

## Step 9: Install OpenTelemetry Collector (Optional)

```bash
# Download OTel Collector
cd /tmp
wget https://github.com/open-telemetry/opentelemetry-collector-releases/releases/download/v0.90.0/otelcol-contrib_0.90.0_linux_amd64.tar.gz
tar -xvf otelcol-contrib_0.90.0_linux_amd64.tar.gz

# Move binary
sudo mv otelcol-contrib /usr/local/bin/

# Create configuration directory
sudo mkdir -p /etc/otelcol

# Create configuration
sudo tee /etc/otelcol/config.yaml > /dev/null << EOF
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
    endpoint: localhost:14250
    tls:
      insecure: true

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, jaeger]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging]
EOF

# Create systemd service
sudo tee /etc/systemd/system/otelcol.service > /dev/null << EOF
[Unit]
Description=OpenTelemetry Collector
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/otelcol-contrib --config=/etc/otelcol/config.yaml
Restart=on-failure

[Install]
WantedBy=multi-user.target
EOF

# Start OTel Collector
sudo systemctl daemon-reload
sudo systemctl enable otelcol
sudo systemctl start otelcol

echo "? OpenTelemetry Collector installed on ports 4317/4318"
```

---

## Step 10: Install Nginx (Reverse Proxy - Optional)

```bash
# Install Nginx
sudo apt-get install -y nginx

# Create reverse proxy configuration
sudo tee /etc/nginx/sites-available/cleanarch > /dev/null << 'EOF'
# Application
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Seq
server {
    listen 5341;
    location / {
        proxy_pass http://localhost:5341;
        proxy_set_header Host $host;
    }
}
EOF

# Enable site
sudo ln -s /etc/nginx/sites-available/cleanarch /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default

# Test and reload Nginx
sudo nginx -t
sudo systemctl enable nginx
sudo systemctl restart nginx

echo "? Nginx installed and configured"
```

---

## Step 11: Deploy Your Application

### Option A: Using Docker

```bash
# Pull your image (if using Azure Container Registry)
docker login your-acr.azurecr.io -u <username> -p <password>

# Run your application
docker run -d \
  --name web-api \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ConnectionStrings__Database="Server=localhost,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;" \
  -e RabbitMQ__Host=localhost \
  -e RabbitMQ__Username=admin \
  -e RabbitMQ__Password=StrongPassword123! \
  -e ELASTIC_URL=http://localhost:9200 \
  -e SEQ_URL=http://localhost:5341 \
  -e Jwt__Secret=your-production-secret-minimum-32-characters \
  -e Jwt__Issuer=https://your-domain.com \
  -e Jwt__Audience=https://your-domain.com \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e OpenTelemetry__Otlp__Endpoint=http://localhost:4317 \
  --network host \
  your-acr.azurecr.io/web-api:latest

# Check logs
docker logs -f web-api
```

### Option B: Running Directly with .NET

```bash
# Install .NET 10 SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Add to PATH
export PATH="$PATH:$HOME/.dotnet"
echo 'export PATH="$PATH:$HOME/.dotnet"' >> ~/.bashrc

# Clone your repository
git clone https://github.com/olasam4liv/CleanArchitecture.git
cd CleanArchitecture

# Publish the application
dotnet publish src/Web.Api/Web.Api.csproj -c Release -o /var/www/cleanarch

# Create systemd service
sudo tee /etc/systemd/system/cleanarch.service > /dev/null << EOF
[Unit]
Description=Clean Architecture API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/cleanarch
ExecStart=/root/.dotnet/dotnet /var/www/cleanarch/Web.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=cleanarch
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:8080
Environment=ConnectionStrings__Database=Server=localhost,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;
Environment=RabbitMQ__Host=localhost
Environment=RabbitMQ__Username=admin
Environment=RabbitMQ__Password=StrongPassword123!
Environment=ELASTIC_URL=http://localhost:9200
Environment=SEQ_URL=http://localhost:5341

[Install]
WantedBy=multi-user.target
EOF

# Start the application
sudo systemctl daemon-reload
sudo systemctl enable cleanarch
sudo systemctl start cleanarch

# Check status
sudo systemctl status cleanarch
```

---

## Step 12: Verify All Services

Run this script to check all services:

```bash
#!/bin/bash

echo "========================================="
echo "Service Status Check"
echo "========================================="

# SQL Server
echo -n "SQL Server (1433): "
if sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "SELECT 1" &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# RabbitMQ
echo -n "RabbitMQ (5672): "
if sudo rabbitmqctl status &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# Elasticsearch
echo -n "Elasticsearch (9200): "
if curl -s http://localhost:9200 &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# Seq
echo -n "Seq (5341): "
if docker ps | grep seq &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# Prometheus
echo -n "Prometheus (9090): "
if curl -s http://localhost:9090/-/healthy &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# Jaeger
echo -n "Jaeger (16686): "
if docker ps | grep jaeger &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

# Application
echo -n "Application (8080): "
if curl -s http://localhost:8080/health &>/dev/null; then
    echo "? Running"
else
    echo "? Not running"
fi

echo "========================================="
echo "Port Usage:"
netstat -tuln | grep -E ":(1433|5672|15672|9200|5341|9090|16686|8080)"
echo "========================================="
```

Save as `check-services.sh`, make executable, and run:

```bash
chmod +x check-services.sh
./check-services.sh
```

---

## Step 13: Configure Automatic Startup

Ensure all services start on boot:

```bash
# Enable all services
sudo systemctl enable mssql-server
sudo systemctl enable rabbitmq-server
sudo systemctl enable elasticsearch
sudo systemctl enable prometheus
# Docker containers already have --restart unless-stopped

# Check enabled services
sudo systemctl list-unit-files --state=enabled | grep -E "mssql|rabbit|elastic|prometheus"
```

---

## Port Summary

| Service | Port(s) | Access URL |
|---------|---------|------------|
| SQL Server | 1433 | `<server-ip>:1433` |
| RabbitMQ (AMQP) | 5672 | `<server-ip>:5672` |
| RabbitMQ (Management) | 15672 | `http://<server-ip>:15672` |
| Elasticsearch | 9200 | `http://<server-ip>:9200` |
| Seq | 5341 | `http://<server-ip>:5341` |
| Prometheus | 9090 | `http://<server-ip>:9090` |
| Jaeger (UI) | 16686 | `http://<server-ip>:16686` |
| Jaeger (Collector) | 4317, 14250 | `<server-ip>:4317` |
| Your Application | 8080 | `http://<server-ip>:8080` |
| Nginx (optional) | 80, 443 | `http://<server-ip>` |

---

## Application Configuration

Update your `appsettings.Production.json` or environment variables:

```json
{
  "ConnectionStrings": {
    "Database": "Server=<server-ip>,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "Host": "<server-ip>",
    "Username": "admin",
    "Password": "StrongPassword123!"
  },
  "ELASTIC_URL": "http://<server-ip>:9200",
  "SEQ_URL": "http://<server-ip>:5341",
  "OpenTelemetry": {
    "Otlp": {
      "Endpoint": "http://<server-ip>:4317"
    }
  },
  "Jwt": {
    "Secret": "your-production-secret-minimum-32-characters",
    "Issuer": "https://your-domain.com",
    "Audience": "https://your-domain.com"
  }
}
```

---

## Monitoring and Maintenance

### Check Resource Usage

```bash
# CPU and Memory
htop

# Disk usage
df -h

# Service-specific memory usage
ps aux | grep -E "sqlservr|beam.smp|elasticsearch|prometheus" | awk '{print $2, $3, $4, $11}'

# Docker container stats
docker stats
```

### Log Locations

```bash
# SQL Server
sudo tail -f /var/opt/mssql/log/errorlog

# RabbitMQ
sudo tail -f /var/log/rabbitmq/rabbit@$(hostname).log

# Elasticsearch
sudo tail -f /var/log/elasticsearch/elasticsearch.log

# Prometheus
sudo journalctl -u prometheus -f

# Seq (Docker)
docker logs -f seq

# Jaeger (Docker)
docker logs -f jaeger

# Your Application
docker logs -f web-api
# Or if using systemd:
sudo journalctl -u cleanarch -f
```

### Backup Script

Create a backup script:

```bash
#!/bin/bash

BACKUP_DIR="/backup/$(date +%Y%m%d)"
mkdir -p $BACKUP_DIR

# Backup SQL Server
sqlcmd -S localhost -U sa -P 'YourStrongPassword123!' -Q "BACKUP DATABASE CleanArchitecture TO DISK='$BACKUP_DIR/cleanarch.bak' WITH COMPRESSION"

# Backup Elasticsearch
curl -X PUT "localhost:9200/_snapshot/backup_repo/$BACKUP_DIR" -H 'Content-Type: application/json' -d'
{
  "indices": "*",
  "ignore_unavailable": true,
  "include_global_state": false
}'

# Backup Seq
docker exec seq tar czf /data/backup.tar.gz /data

echo "Backup completed: $BACKUP_DIR"
```

---

## Security Hardening

### 1. Disable Public Access (Recommended for Production)

```bash
# Bind services to localhost only
# Edit /etc/elasticsearch/elasticsearch.yml
network.host: 127.0.0.1  # Instead of 0.0.0.0

# Use Nginx as reverse proxy for external access
# Only expose ports 80/443 publicly
```

### 2. Enable SSL/TLS

```bash
# Install Certbot for Let's Encrypt
sudo apt-get install -y certbot python3-certbot-nginx

# Get SSL certificate
sudo certbot --nginx -d your-domain.com

# Auto-renewal is configured by default
```

### 3. Enable Elasticsearch Security

```bash
# Edit /etc/elasticsearch/elasticsearch.yml
xpack.security.enabled: true

# Generate passwords
sudo /usr/share/elasticsearch/bin/elasticsearch-reset-password -u elastic
```

---

## Cost Comparison

### Single Server (All-in-One)
- **Azure VM (D4s_v3):** ~$280/month
- **Storage:** ~$20/month
- **Bandwidth:** ~$10/month
- **Total:** ~$310/month

### Multiple Servers (Separate VMs)
- **SQL Server VM:** ~$280/month
- **RabbitMQ VM:** ~$30/month
- **Elasticsearch VM:** ~$140/month
- **App VM:** ~$140/month
- **Total:** ~$590/month

**Savings: ~$280/month (47% cheaper!)**

---

## When to Use Single Server vs Multiple Servers

### ? Use Single Server When:
- Budget is limited
- Workload is small to medium
- Simplified management is priority
- Testing or staging environment
- < 100 requests/second
- < 10GB logs/day

### ? Use Multiple Servers When:
- High availability is critical
- Large workload (> 1000 req/sec)
- Independent scaling needed
- Production with strict SLAs
- Compliance requires service isolation

---

## Troubleshooting

### Service Won't Start

```bash
# Check status
sudo systemctl status <service-name>

# Check logs
sudo journalctl -xe -u <service-name>

# Check ports
sudo netstat -tuln | grep <port>

# Check if process is running
ps aux | grep <service-name>
```

### Port Already in Use

```bash
# Find process using port
sudo lsof -i :<port>

# Kill process
sudo kill -9 <PID>
```

### Out of Memory

```bash
# Check memory
free -h

# Add swap space
sudo fallocate -l 8G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

---

## Next Steps

1. ? Server provisioned and all services installed
2. ? Deploy your application
3. ? Test all endpoints
4. ? Configure monitoring and alerts
5. ? Set up automated backups
6. ? Configure SSL/TLS
7. ? Performance tuning based on load
8. ? Document your configuration

---

## Additional Resources

- [SQL Server on Linux](https://docs.microsoft.com/sql/linux/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Elasticsearch Guide](https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
