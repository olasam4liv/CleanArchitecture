# Self-Hosting Services on Azure VMs

This guide shows how to deploy the **exact same services** you use locally (RabbitMQ, Elasticsearch, etc.) on Azure Virtual Machines instead of using Azure's native managed services.

## Why Self-Host on Azure VMs?

### ? Advantages
- **No code changes** - Use identical setup as local development
- **Cloud-agnostic** - Easy to migrate to AWS, GCP, or on-premises later
- **Full control** - Configure services exactly as needed
- **Familiar tools** - Same monitoring, debugging, and management tools

### ? Disadvantages
- **You manage everything** - Updates, patches, backups, security
- **More expensive** - Pay for VM compute + storage + bandwidth
- **Manual scaling** - No auto-scaling out of the box
- **Availability** - You configure high availability and disaster recovery

---

## Architecture Overview

```
???????????????????????????????????????????????????????????
?                    Azure Virtual Network                 ?
?  ??????????????????  ??????????????????                ?
?  ?  App Service   ?  ?   Azure SQL    ?                ?
?  ?  (Web API)     ?  ?   Database     ?                ?
?  ??????????????????  ??????????????????                ?
?         ?                                                ?
?         ?  Connects to VMs via Private Endpoints        ?
?         ?                                                ?
?  ????????????????????????????????????????????          ?
?  ?           Subnet: Services (10.0.1.0/24) ?          ?
?  ?  ????????????  ????????????  ????????????          ?
?  ?  ? RabbitMQ ?  ?Elasticsea?  ?   Seq   ??          ?
?  ?  ?    VM    ?  ? rch VM   ?  ?   VM    ??          ?
?  ?  ????????????  ????????????  ????????????          ?
?  ????????????????????????????????????????????          ?
???????????????????????????????????????????????????????????
```

---

## Prerequisites

```bash
# Install Azure CLI
# Windows: Download from https://aka.ms/installazurecliwindows
# Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Login to Azure
az login

# Set subscription
az account set --subscription "Your Subscription Name"

# Set variables (customize these)
RESOURCE_GROUP="rg-cleanarch-prod"
LOCATION="eastus"
VNET_NAME="vnet-cleanarch"
SUBNET_NAME="subnet-services"
VM_USERNAME="azureuser"
```

---

## Step 1: Create Virtual Network

```bash
# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Create virtual network
az network vnet create \
  --resource-group $RESOURCE_GROUP \
  --name $VNET_NAME \
  --address-prefix 10.0.0.0/16 \
  --subnet-name $SUBNET_NAME \
  --subnet-prefix 10.0.1.0/24

# Create Network Security Group
az network nsg create \
  --resource-group $RESOURCE_GROUP \
  --name nsg-services

# Associate NSG with subnet
az network vnet subnet update \
  --resource-group $RESOURCE_GROUP \
  --vnet-name $VNET_NAME \
  --name $SUBNET_NAME \
  --network-security-group nsg-services
```

---

## Step 2: Deploy SQL Server VM

```bash
# Create SQL Server VM
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-sqlserver \
  --image "MicrosoftSQLServer:sql2022-ws2022:sqldev-gen2:latest" \
  --size Standard_D4s_v3 \
  --admin-username $VM_USERNAME \
  --admin-password 'YourStrongPassword123!' \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard \
  --public-ip-address vm-sqlserver-ip

# Open SQL Server port
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowSQLServer \
  --priority 1000 \
  --source-address-prefixes '*' \
  --destination-port-ranges 1433 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# Get public IP
SQL_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-sqlserver \
  --query publicIps -o tsv)

echo "SQL Server IP: $SQL_IP"
```

**Connection String:**
```
Server=$SQL_IP,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;Encrypt=False;TrustServerCertificate=True;
```

---

## Step 3: Deploy RabbitMQ VM

```bash
# Create Ubuntu VM for RabbitMQ
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-rabbitmq \
  --image Ubuntu2204 \
  --size Standard_B2s \
  --admin-username $VM_USERNAME \
  --generate-ssh-keys \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard \
  --public-ip-address vm-rabbitmq-ip

# Open RabbitMQ ports
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowRabbitMQ \
  --priority 1010 \
  --destination-port-ranges 5672 15672 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# Get public IP
RABBITMQ_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-rabbitmq \
  --query publicIps -o tsv)

# SSH and install RabbitMQ
ssh $VM_USERNAME@$RABBITMQ_IP << 'EOF'
# Install Erlang and RabbitMQ
sudo apt-get update
sudo apt-get install -y erlang
sudo apt-get install -y rabbitmq-server

# Enable management plugin
sudo rabbitmq-plugins enable rabbitmq_management

# Create admin user
sudo rabbitmqctl add_user admin StrongPassword123!
sudo rabbitmqctl set_user_tags admin administrator
sudo rabbitmqctl set_permissions -p / admin ".*" ".*" ".*"

# Restart RabbitMQ
sudo systemctl restart rabbitmq-server
EOF

echo "RabbitMQ IP: $RABBITMQ_IP"
echo "Management UI: http://$RABBITMQ_IP:15672"
echo "Username: admin, Password: StrongPassword123!"
```

**Application Configuration:**
```
RabbitMQ__Host=$RABBITMQ_IP
RabbitMQ__Username=admin
RabbitMQ__Password=StrongPassword123!
```

---

## Step 4: Deploy Elasticsearch VM

```bash
# Create Ubuntu VM for Elasticsearch
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-elasticsearch \
  --image Ubuntu2204 \
  --size Standard_D2s_v3 \
  --admin-username $VM_USERNAME \
  --generate-ssh-keys \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard \
  --public-ip-address vm-elasticsearch-ip

# Open Elasticsearch ports
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowElasticsearch \
  --priority 1020 \
  --destination-port-ranges 9200 9300 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# Get public IP
ELASTIC_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-elasticsearch \
  --query publicIps -o tsv)

# SSH and install Elasticsearch
ssh $VM_USERNAME@$ELASTIC_IP << 'EOF'
# Import Elasticsearch GPG key
wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | sudo apt-key add -

# Add repository
echo "deb https://artifacts.elastic.co/packages/8.x/apt stable main" | sudo tee /etc/apt/sources.list.d/elastic-8.x.list

# Install Elasticsearch
sudo apt-get update
sudo apt-get install -y elasticsearch

# Configure Elasticsearch
sudo bash -c 'cat >> /etc/elasticsearch/elasticsearch.yml << EOL
network.host: 0.0.0.0
http.port: 9200
xpack.security.enabled: false
discovery.type: single-node
EOL'

# Enable and start
sudo systemctl daemon-reload
sudo systemctl enable elasticsearch.service
sudo systemctl start elasticsearch.service
EOF

echo "Elasticsearch IP: $ELASTIC_IP"
echo "Elasticsearch URL: http://$ELASTIC_IP:9200"
```

**Application Configuration:**
```
ELASTIC_URL=http://$ELASTIC_IP:9200
```

---

## Step 5: Deploy Seq Log Server VM

```bash
# Create Windows VM for Seq (or use Docker on Linux)
# Option 1: Windows VM
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-seq \
  --image Win2022Datacenter \
  --size Standard_B2s \
  --admin-username $VM_USERNAME \
  --admin-password 'YourStrongPassword123!' \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard \
  --public-ip-address vm-seq-ip

# Open Seq port
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowSeq \
  --priority 1030 \
  --destination-port-ranges 5341 80 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# Get public IP
SEQ_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-seq \
  --query publicIps -o tsv)

# RDP to Windows VM and download Seq installer from datalust.co/download
# Or use Docker on Linux:

# Option 2: Linux VM with Docker
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-seq \
  --image Ubuntu2204 \
  --size Standard_B2s \
  --admin-username $VM_USERNAME \
  --generate-ssh-keys \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard

SEQ_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-seq \
  --query publicIps -o tsv)

# SSH and install Docker + Seq
ssh $VM_USERNAME@$SEQ_IP << 'EOF'
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Run Seq container
sudo docker run -d \
  --name seq \
  --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 80:80 \
  -p 5341:5341 \
  -v seq-data:/data \
  datalust/seq:latest
EOF

echo "Seq IP: $SEQ_IP"
echo "Seq URL: http://$SEQ_IP"
```

**Application Configuration:**
```
SEQ_URL=http://$SEQ_IP
```

---

## Step 6: Deploy Prometheus VM

```bash
# Create Ubuntu VM for Prometheus
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-prometheus \
  --image Ubuntu2204 \
  --size Standard_B2s \
  --admin-username $VM_USERNAME \
  --generate-ssh-keys \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard \
  --public-ip-address vm-prometheus-ip

# Open Prometheus port
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowPrometheus \
  --priority 1040 \
  --destination-port-ranges 9090 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# Get public IP
PROM_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-prometheus \
  --query publicIps -o tsv)

# SSH and install Prometheus
ssh $VM_USERNAME@$PROM_IP << 'EOF'
# Create user
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
sudo mkdir -p /etc/prometheus /var/lib/prometheus
sudo chown prometheus:prometheus /var/lib/prometheus

# Create configuration
sudo bash -c 'cat > /etc/prometheus/prometheus.yml << EOL
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: "prometheus"
    static_configs:
      - targets: ["localhost:9090"]
EOL'

sudo chown prometheus:prometheus /etc/prometheus/prometheus.yml

# Create systemd service
sudo bash -c 'cat > /etc/systemd/system/prometheus.service << EOL
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
  --storage.tsdb.path=/var/lib/prometheus/

[Install]
WantedBy=multi-user.target
EOL'

# Start Prometheus
sudo systemctl daemon-reload
sudo systemctl enable prometheus
sudo systemctl start prometheus
EOF

echo "Prometheus IP: $PROM_IP"
echo "Prometheus URL: http://$PROM_IP:9090"
```

---

## Step 7: Deploy Your Application

### Option A: Azure App Service (with VNet Integration)

```bash
# Create App Service Plan
az appservice plan create \
  --name plan-cleanarch \
  --resource-group $RESOURCE_GROUP \
  --is-linux \
  --sku P1V2

# Create Web App
az webapp create \
  --name app-cleanarch-prod \
  --resource-group $RESOURCE_GROUP \
  --plan plan-cleanarch \
  --deployment-container-image-name your-acr.azurecr.io/web-api:latest

# Enable VNet Integration
az webapp vnet-integration add \
  --name app-cleanarch-prod \
  --resource-group $RESOURCE_GROUP \
  --vnet $VNET_NAME \
  --subnet $SUBNET_NAME

# Configure App Settings with private IPs
az webapp config appsettings set \
  --name app-cleanarch-prod \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ConnectionStrings__Database="Server=$SQL_IP,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;" \
    RabbitMQ__Host=$RABBITMQ_IP \
    RabbitMQ__Username=admin \
    RabbitMQ__Password=StrongPassword123! \
    ELASTIC_URL=http://$ELASTIC_IP:9200 \
    SEQ_URL=http://$SEQ_IP \
    Jwt__Secret=your-production-secret \
    ASPNETCORE_ENVIRONMENT=Production
```

### Option B: Azure VM with Docker

```bash
# Create application VM
az vm create \
  --resource-group $RESOURCE_GROUP \
  --name vm-app \
  --image Ubuntu2204 \
  --size Standard_D2s_v3 \
  --admin-username $VM_USERNAME \
  --generate-ssh-keys \
  --vnet-name $VNET_NAME \
  --subnet $SUBNET_NAME \
  --public-ip-sku Standard

APP_IP=$(az vm show -d \
  --resource-group $RESOURCE_GROUP \
  --name vm-app \
  --query publicIps -o tsv)

# Open application port
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowHTTP \
  --priority 1050 \
  --destination-port-ranges 80 443 8080 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp

# SSH and deploy application
ssh $VM_USERNAME@$APP_IP << EOF
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Login to ACR (if using Azure Container Registry)
# sudo docker login your-acr.azurecr.io -u <username> -p <password>

# Run your application
sudo docker run -d \
  --name web-api \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ConnectionStrings__Database="Server=$SQL_IP,1433;Database=CleanArchitecture;User Id=sa;Password=YourStrongPassword123!;" \
  -e RabbitMQ__Host=$RABBITMQ_IP \
  -e RabbitMQ__Username=admin \
  -e RabbitMQ__Password=StrongPassword123! \
  -e ELASTIC_URL=http://$ELASTIC_IP:9200 \
  -e SEQ_URL=http://$SEQ_IP \
  -e Jwt__Secret=your-production-secret \
  -e ASPNETCORE_ENVIRONMENT=Production \
  your-acr.azurecr.io/web-api:latest
EOF

echo "Application URL: http://$APP_IP:8080"
```

---

## Security Hardening

### 1. Use Private IPs Only (No Public Access)

```bash
# Remove public IPs from service VMs
az vm deallocate --resource-group $RESOURCE_GROUP --name vm-rabbitmq
az vm update --resource-group $RESOURCE_GROUP --name vm-rabbitmq --remove publicIps

# Repeat for other VMs
# Access via bastion host or VPN only
```

### 2. Enable Azure Bastion (Secure SSH/RDP)

```bash
# Create bastion subnet
az network vnet subnet create \
  --resource-group $RESOURCE_GROUP \
  --vnet-name $VNET_NAME \
  --name AzureBastionSubnet \
  --address-prefixes 10.0.2.0/27

# Create bastion host
az network bastion create \
  --name bastion-cleanarch \
  --resource-group $RESOURCE_GROUP \
  --vnet-name $VNET_NAME \
  --location $LOCATION \
  --public-ip-address bastion-pip \
  --sku Basic
```

### 3. Configure Firewall Rules

```bash
# Restrict SQL Server to only App Service
az network nsg rule update \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-services \
  --name AllowSQLServer \
  --source-address-prefixes 10.0.1.0/24  # Only from services subnet

# Similar for other services
```

### 4. Enable Disk Encryption

```bash
# Create Key Vault
az keyvault create \
  --name kv-cleanarch-$RANDOM \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enabled-for-disk-encryption

# Enable disk encryption on VMs
az vm encryption enable \
  --resource-group $RESOURCE_GROUP \
  --name vm-sqlserver \
  --disk-encryption-keyvault kv-cleanarch-*
```

---

## Cost Estimation (Monthly)

| Service | VM Size | Estimated Cost |
|---------|---------|----------------|
| SQL Server VM | Standard_D4s_v3 | $~280 |
| RabbitMQ VM | Standard_B2s | $~30 |
| Elasticsearch VM | Standard_D2s_v3 | $~140 |
| Seq VM | Standard_B2s | $~30 |
| Prometheus VM | Standard_B2s | $~30 |
| Application VM | Standard_D2s_v3 | $~140 |
| **Total** | | **~$650/month** |

**Notes:**
- Add ~20% for storage, networking, bandwidth
- Can reduce costs with Reserved Instances (1-3 year commitment)
- Managed PaaS alternative costs ~$400-500/month

---

## Backup Strategy

```bash
# Enable backup for VMs
az backup vault create \
  --resource-group $RESOURCE_GROUP \
  --name vault-backup-cleanarch \
  --location $LOCATION

# Enable backup for SQL Server VM
az backup protection enable-for-vm \
  --resource-group $RESOURCE_GROUP \
  --vault-name vault-backup-cleanarch \
  --vm vm-sqlserver \
  --policy-name DefaultPolicy
```

---

## Monitoring

```bash
# Enable Azure Monitor for VMs
az monitor log-analytics workspace create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name law-cleanarch

# Enable VM insights
az vm extension set \
  --resource-group $RESOURCE_GROUP \
  --vm-name vm-rabbitmq \
  --name OmsAgentForLinux \
  --publisher Microsoft.EnterpriseCloud.Monitoring
```

---

## When to Use This Approach

? **Use Self-Hosted VMs When:**
- You need specific versions/configurations
- You want to avoid vendor lock-in
- You have existing expertise with these tools
- You plan to migrate to other clouds later
- Budget allows for operational overhead

? **Use Azure PaaS When:**
- You want minimal operational burden
- You need auto-scaling and HA out of the box
- You prefer pay-as-you-go pricing
- You want Azure ecosystem integration
- Team lacks DevOps expertise

---

## Summary

This guide showed you how to deploy **the exact same stack** (RabbitMQ, Elasticsearch, etc.) on Azure VMs. You now have:

? Full control over your infrastructure  
? No code changes from local development  
? Cloud hosting with Azure reliability  
? Flexibility to migrate to other clouds  

However, remember you're responsible for:
- Security patches and updates
- High availability configuration
- Backup and disaster recovery
- Monitoring and alerting
- Scaling infrastructure

**Need help?** Consider Azure PaaS services (Azure Service Bus, Application Insights, etc.) which provide these features out of the box.
