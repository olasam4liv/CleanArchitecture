# Clean Architecture Template

A production-ready .NET 10 Clean Architecture template with comprehensive observability, messaging, and deployment options.

## What's Included

### Core Architecture
- **SharedKernel** - Common Domain-Driven Design abstractions
- **Domain Layer** - Sample entities with audit trail support
- **Application Layer** - CQRS, use cases, cross-cutting concerns (logging, validation)
- **Infrastructure Layer** - Authentication, authorization, EF Core, messaging
- **Web API** - RESTful API with Swagger, versioning, health checks
- **Web Worker** - Background service for message processing

### Observability Stack
- **Elasticsearch** - Distributed log aggregation and search
- **Seq** - Structured log viewer (http://localhost:5341)
- **Prometheus** - Metrics collection and monitoring
- **Jaeger** - Distributed tracing
- **OpenTelemetry** - Unified observability framework

### Features
- ? Audit Trail & Soft Delete
- ? Message Queue (RabbitMQ/MassTransit)
- ? Outbox Pattern for reliable messaging
- ? JWT Authentication
- ? Health Checks
- ? Docker Compose for local development
- ? Multiple deployment options (Azure PaaS, VMs, ACI)

---

## Quick Start

### Use as a dotnet new template (recommended)

```bash
# Install from GitHub source
dotnet new --install https://github.com/olasam4liv/CleanArchitecture

# After publishing the package (PackageId: sam-clean-architecture)
dotnet new --install sam-clean-architecture

# Scaffold a new solution
dotnet new sam-clean-architecture -n MyProject
cd MyProject
dotnet restore

# (Optional) Pack locally for testing the template
dotnet pack templatepack/Sam.CleanArchitecture.Template.csproj
dotnet new --install nupkg/sam-clean-architecture.0.1.0.nupkg
```

### Prerequisites
- .NET 10 SDK
- Docker Desktop
- SQL Server Express (or use Docker)

### Run Locally

```bash
# (If you haven't already) scaffold a project
# dotnet new sam-clean-architecture -n MyProject
cd MyProject

# Copy environment file
cp src/Web.Api/.env.example src/Web.Api/.env

# Edit .env with your settings (SQL Server connection string, etc.)
# notepad src/Web.Api/.env

# Start all services with Docker Compose
docker-compose up -d

# Apply database migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api

# Or run migrations automatically on startup (already configured in appsettings)
```

### Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| Web API | http://localhost:8000 | N/A |
| Swagger UI | http://localhost:8000/swagger | N/A |
| Grafana (Dashboards) | http://localhost:3000 | admin/admin |
| Seq Logs | http://localhost:5341 | N/A |
| RabbitMQ | http://localhost:15672 | guest/guest |
| Elasticsearch | http://localhost:9200 | N/A |
| Prometheus | http://localhost:9090 | N/A |
| Jaeger | http://localhost:16686 | N/A |

---

## Environment Configuration

### Local Development

Create and configure your `.env` file:

```bash
cp src/Web.Api/.env.example src/Web.Api/.env
```

Key variables to configure:
- `DATABASE`: SQL Server connection string
- `JWT_SECRET`: JWT signing key (minimum 32 characters)
- `JWT_ISSUER` / `JWT_AUDIENCE`: JWT configuration
- `ELASTIC_URL`: Elasticsearch URL (http://localhost:9200)
- `SEQ_URL`: Seq log server URL (http://localhost:5341)

**Note:** The `.env` file is for **local development only**. Production uses Azure App Service Configuration.

### Production Configuration

See deployment guides in `docs/` folder:
- **Azure PaaS:** Managed services (recommended)
- **Azure VMs:** Self-hosted services
- **Hybrid:** Mix of managed and self-hosted

---

## Deployment Options

### Option 1: Azure PaaS (Managed Services) - **Recommended**

Deploy to Azure using fully managed services:
- **Azure SQL Database** (replaces SQL Server Express)
- **Azure Service Bus** (replaces RabbitMQ)
- **Application Insights** (replaces Elasticsearch/Seq/Prometheus)
- **Azure App Service** (hosts your API)

**Pros:** Zero infrastructure management, auto-scaling, high availability  
**Cons:** Requires code changes, vendor lock-in

?? **Guide:** [docs/AZURE_DEPLOYMENT.md](docs/AZURE_DEPLOYMENT.md)

### Option 2: Self-Hosted on Azure VMs

Deploy the exact same stack (RabbitMQ, Elasticsearch, etc.) on Azure Virtual Machines.

#### Single Server (All-in-One) - **Most Cost-Effective**
Install all services on one Linux server:
- **Cost:** ~$310/month (vs ~$590 for separate VMs)
- **Setup:** 2-3 hours
- **Best for:** Small to medium workloads, budget-conscious deployments

**Pros:** No code changes, full control, cloud-agnostic, simplified management  
**Cons:** You manage updates, scaling, backups; single point of failure

?? **Guide:** [docs/SINGLE_SERVER_SETUP.md](docs/SINGLE_SERVER_SETUP.md)

#### Multiple Servers (Separate VMs)
Deploy each service on its own VM:
- **Cost:** ~$590/month
- **Setup:** 1 week
- **Best for:** High availability, production with strict SLAs

**Pros:** Independent scaling, better fault isolation, production-grade  
**Cons:** More expensive, complex management

?? **Guide:** [docs/AZURE_VM_DEPLOYMENT.md](docs/AZURE_VM_DEPLOYMENT.md)


---

## Documentation

| Document | Description |
|----------|-------------|
| [Environment Configuration](docs/ENVIRONMENT_CONFIGURATION.md) | Local vs production configuration strategy |
| [Services Setup](docs/SERVICES_SETUP.md) | Set up all dependency services (local, server, Azure) |
| [Grafana Setup](docs/GRAFANA_SETUP.md) | Configure Grafana dashboards for monitoring |
| [Azure Deployment](docs/AZURE_DEPLOYMENT.md) | Deploy to Azure using PaaS services |
| [Azure VM Deployment](docs/AZURE_VM_DEPLOYMENT.md) | Deploy to Azure using self-hosted VMs |
| [Deployment Decision Guide](docs/DEPLOYMENT_DECISION_GUIDE.md) | Choose the right deployment strategy |

---

## Architecture

```
???????????????????????????????????????????????????
?                  Web.Api (Presentation)          ?
?  - REST API Endpoints                           ?
?  - Swagger Documentation                         ?
?  - Health Checks                                 ?
???????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????
?              Application Layer                   ?
?  - CQRS (Commands/Queries)                      ?
?  - Validation (FluentValidation)                ?
?  - Domain Event Handlers                        ?
???????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????
?               Domain Layer                       ?
?  - Entities (User, TodoItem)                    ?
?  - Domain Events                                ?
?  - Business Logic                               ?
???????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????
?            Infrastructure Layer                  ?
?  - EF Core (Database)                           ?
?  - MassTransit (Messaging)                      ?
?  - Authentication/Authorization                 ?
?  - Audit Trail & Soft Delete                    ?
???????????????????????????????????????????????????
```

---

## Key Features

### 1. Audit Trail
All entities automatically track:
- Created by / Created at
- Modified by / Modified at
- Deleted by / Deleted at (soft delete)
- Remote IP address

### 2. Outbox Pattern
Reliable message publishing:
- Domain events saved to outbox table
- Background worker processes outbox
- Guarantees at-least-once delivery

### 3. Soft Delete
Entities are never physically deleted:
- Global query filter excludes soft-deleted items
- Use `.IgnoreQueryFilters()` to include them

### 4. Observability
Comprehensive monitoring:
- Structured logging (Serilog ? Seq/Elasticsearch)
- Distributed tracing (OpenTelemetry ? Jaeger)
- Metrics (Prometheus)
- Health checks

---

## Development

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 or Rider
- Docker Desktop
- SQL Server Express or Docker SQL Server

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/Web.Api

# Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Web.Api
```

### Run Locally (without Docker)

```bash
# Start SQL Server, RabbitMQ, etc. manually or via Docker
docker-compose up -d rabbitmq elasticsearch seq

# Run API
dotnet run --project src/Web.Api

# Run Worker (in separate terminal)
dotnet run --project src/Web.Worker
```

---

## Project Structure

```
CleanArchitecture/
??? src/
?   ??? Domain/              # Domain entities, events, errors
?   ??? Application/         # Use cases (commands/queries)
?   ??? Infrastructure/      # Data access, external services
?   ??? SharedKernel/        # Common abstractions
?   ??? Web.Api/            # REST API
?   ??? Web.Worker/         # Background worker
??? tests/
?   ??? ArchitectureTests/  # Architecture validation tests
??? docs/                   # Documentation
??? docker-compose.yml      # Local development services
??? README.md
```

---

## Technologies

- **.NET 10** - Latest .NET framework
- **EF Core 10** - Object-relational mapper
- **MassTransit** - Distributed application framework
- **RabbitMQ** - Message broker
- **Serilog** - Structured logging
- **FluentValidation** - Validation library
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization
- **Azure** - Cloud platform

---

## Security

### Local Development
- Default credentials (acceptable for local)
- No encryption on some connections
- Secrets in `.env` (not in source control)

### Production
- ? Use Azure Key Vault for secrets
- ? Enable HTTPS only
- ? Use Managed Identity for service-to-service auth
- ? Configure CORS properly
- ? Enable Azure Defender

See: [docs/AZURE_DEPLOYMENT.md#security-best-practices](docs/AZURE_DEPLOYMENT.md#security-best-practices)

---

## Cost Estimation

### Local Development
**Free** - All services run in Docker on your machine

### Azure PaaS (Recommended)
- **Small workload:** ~$100-200/month
- **Medium workload:** ~$400-600/month
- **Large workload:** ~$1000+/month

### Azure VMs (Self-Hosted)
- **Small deployment:** ~$300-500/month
- **Medium deployment:** ~$650-800/month
- **Large deployment:** ~$1200+/month

**Note:** Add ~20% for bandwidth, storage, backups

Use [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/) for accurate estimates.

---

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## Roadmap

- [ ] Add integration tests
- [ ] Add unit tests for domain logic
- [ ] Implement caching with Redis
- [ ] Add API rate limiting
- [ ] Implement feature flags
- [ ] Add GraphQL endpoint
- [ ] Kubernetes deployment manifests
- [ ] Terraform/Bicep infrastructure as code

---

## Learn More

This template by Samuel Olatunji

Includes:
- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- Advanced OpenTelemetry
- Outbox pattern implementation
- API Versioning strategies
- Comprehensive testing

---

## Support

- **GitHub Issues:** [Report bugs or request features](https://github.com/olasam4liv/CleanArchitecture/issues)
- **Documentation:** Check the `docs/` folder
- **Azure Support:** https://azure.microsoft.com/support/

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Acknowledgments

- Milan Jovanovi? for Clean Architecture inspiration
- .NET team for excellent framework
- Azure team for cloud platform
- Community contributors

---

**Stay awesome!** ??
