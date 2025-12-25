# Clean Architecture Template

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, PostgreSQL
  - Serilog
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Elasticsearch for distributed log aggregation and analysis
  - Configure `ELASTIC_URL` environment variable to connect to your Elasticsearch instance
  - Logs are indexed by app name (e.g., `logs-clean-architecture-web-api-2025.12`)
- Testing projects
  - Architecture testing

## Environment Configuration

Create a `.env` file in the project root by copying `.env.example`:

```bash
cp .env.example .env
```

Update the values in `.env` with your local or production configuration:

- `ELASTIC_URL`: Elasticsearch server URL (e.g., http://localhost:9200 or http://elasticsearch:9200 if using Docker)
- `APP_NAME`: Unique application identifier (used in Elasticsearch index naming and log properties for cross-app identification)
- `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`: JWT configuration
- `DB_*`: PostgreSQL connection details
- `OTEL_ENDPOINT`: OpenTelemetry collector endpoint
- `SEQ_URL`: Seq server URL (optional)

The application will automatically load the `.env` file during development via DotNetEnv.

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!



If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!
