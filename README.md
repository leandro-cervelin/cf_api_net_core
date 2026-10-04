[![Build & Test .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml) [![CodeQL .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)
# .NET 10.0 Example App / API
## .Net 10.0 API using SQL Server with Entity Framework Core
## Unit Tests and Integration Tests

## Project structure

The projects are split by responsibility and the references only point inward:

- `CF.Customer.Domain` - entities and business rules. Doesn't reference the other layers.
- `CF.Customer.Application` - the use cases (the facade) and DTOs. References Domain.
- `CF.Customer.Infrastructure` - EF Core, the `DbContext` and repositories. Implements the interfaces declared in the inner layers.
- `CF.Api` and `CF.ConsoleApp` - the entry points. They read config, wire up the services and call into the Application layer.
- `CF.Migrations` - the EF Core migrations, kept out of Infrastructure so they don't get in the way.

Because Domain and Application don't know about EF Core or ASP.NET, the business rules stay easy to test and the same core can run under more than one host. The API and the console app both register it the same way (`AddCustomerCore`); the console app is there mainly to prove that point.

Tests: `CF.Customer.UnitTest` and `CF.Api.UnitTest` run without a database. `CF.IntegrationTest` starts SQL Server with Testcontainers and exercises the real API.

## Docker with Compose

1. Switch Docker to Linux containers.
2. Copy `CF.Api/.env.example` to `CF.Api/.env` and set strong `SA_PASSWORD` and `APP_DB_PASSWORD` values and a
   random `JWT_SIGNING_KEY`. The file is gitignored; it supplies the two database passwords and the signing key.
3. From the `src` folder:

   - docker compose -f CF.Api/docker-compose.yml up --build

Compose starts four services, in order:

| Service | What it does | Database login |
| --- | --- | --- |
| `db` | SQL Server, with data in the `mssql-data` volume. Reports healthy once it accepts connections. | |
| `db-init` | Creates the `CF` database and the API's `cf_app` login ([`CF.Api/db/init-app-login.sql`](src/CF.Api/db/init-app-login.sql)), then exits. | `sa` |
| `migrate` | Runs the EF Core migration bundle against `db`, then exits. | `sa` |
| `api` | Starts only after `migrate` has finished successfully. Listens on http://localhost:8888. | `cf_app` |

Compose runs the API in the Production environment, so the Scalar docs page isn't served there; use local
development for that.

## Local development (without Docker)

The committed `appsettings.json` intentionally omits the database password and the signing key.
Provide them with user-secrets so no credential is committed:

- dotnet user-secrets --project CF.Api set "ConnectionStrings:DbConnection" "Data Source=localhost;Initial Catalog=CF;User ID=sa;Password=<your-password>;TrustServerCertificate=True;"
- dotnet user-secrets --project CF.Api set "Jwt:SigningKey" "<random value, e.g. from: openssl rand -base64 48>"

In Development the API applies pending migrations on startup (`Database:MigrateOnStartup` is `true` in
`appsettings.Development.json`), and the API docs are at https://localhost:7242/scalar/v1.

## Authentication

The customer endpoints require a JWT bearer token:

| Endpoint | Access |
| --- | --- |
| `POST /api/v1/customer` (sign-up) | anonymous |
| `POST /api/v1/auth/token` (login: `{ "email", "password" }`) | anonymous |
| `GET/PUT/DELETE /api/v1/customer/{id}` | that customer, or an admin |
| `GET /api/v1/customer` (list) | admin only |

Tokens are signed with `Jwt:SigningKey` (HMAC-SHA256, at least 32 bytes). The app refuses to start without it.
The key is never committed: use user-secrets locally (see above), and `JWT_SIGNING_KEY` in `CF.Api/.env` with
Docker Compose.

Admins are listed by customer id in `Jwt:AdminCustomerIds` (e.g. `Jwt__AdminCustomerIds__0=1`). Ids rather than
emails, because emails aren't verified and anyone could register an admin address that isn't taken yet.

### Changing the password

`PUT /api/v1/customer/{id}` always carries the full record, including `password`. Sending the existing password
proves the caller knows it, so name and email edits need nothing else. To set a **new** password, also send
`currentPassword`; without it (or if it's wrong) the request fails with 400. This way a stolen token alone can't
take over an account. Admins editing another customer's account are exempt, as they can't know that password.

Passwords are limited to 72 bytes (UTF-8), because bcrypt ignores anything beyond that.

### Token revocation

Each customer has a security stamp, embedded in every token they get. Changing the password or email rotates it,
and deleting the customer removes it; either way, all previously issued tokens are rejected (401) and the client has
to log in again. Changing only the name keeps tokens valid.

Stamps are cached per instance for `Jwt:SecurityStampCacheSeconds` (default 30). A change made through an instance
takes effect there immediately; other instances pick it up when their cache entry expires. Set it to `0` to check
the database on every request.

### Trying it in the API docs

In Development, open `/scalar/v1`, call `POST /api/v1/auth/token`, paste the `accessToken` into the
authentication panel (Bearer), and the protected endpoints (marked with a lock) send it automatically.

## Deployment

### Database migrations

Outside Development the API does **not** migrate on startup (`Database:MigrateOnStartup` defaults to `false`).
With several instances, startup migrations race each other, and they force the app's database login to have
schema-change (DDL) rights. Apply migrations as a separate step before rolling out the new version instead:

- Docker image: build the `migrator` target and run it once with `ConnectionStrings__DbConnection` set. That's
  what the `migrate` service in Compose does; in Kubernetes it fits a Job or an init container.

  - docker build -f CF.Api/Dockerfile --target migrator -t cf-api-migrator .

- Without Docker: build a bundle and run it from your pipeline.

  - dotnet ef migrations bundle --project CF.Migrations --startup-project CF.Api -o efbundle
  - ./efbundle --connection "<connection string>"

The bundle only applies migrations that are still pending, so running it on every deploy is safe.

### Behind a reverse proxy or load balancer

Rate limiting is per client IP, and HTTPS redirection and HSTS depend on the request scheme. Behind a proxy, both
come from `X-Forwarded-For` / `X-Forwarded-Proto`, which are trusted **only** from the proxies you list. Trusting
them from anyone would let clients choose their own IP and get around the rate limit.

```json
"ForwardedHeaders": {
  "KnownProxies": [ "10.0.0.5" ],
  "KnownNetworks": [ "10.0.0.0/8" ]
}
```

Or with environment variables: `ForwardedHeaders__KnownProxies__0=10.0.0.5`. Loopback is always trusted.

### Health checks

| Endpoint | Checks | Access | Rate limited |
| --- | --- | --- | --- |
| `/health/live` | the process is up | anonymous | no |
| `/health/ready` | the database is reachable | anonymous | no |
| `/health` | everything, as a detailed JSON report | admin token | yes |

Point liveness and readiness probes at `/health/live` and `/health/ready`. The detailed report names the
checks and their timings, so it's limited to admins.

### Least privilege

- **Database:** the API connects as `cf_app`, a login that can only read and write data (`db_datareader` and
  `db_datawriter` in `CF`). It can't change the schema or reach other databases, which limits the damage if the
  app is ever compromised. Only the migration step uses a privileged login. Outside Compose, create an equivalent
  login with the same script (run it as an admin with `sqlcmd -v AppLogin=... AppPassword=...`). Keep
  `Database:MigrateOnStartup` off with such a login, since it can't apply migrations.
- **Container:** the API and migrator images run as the unprivileged `app` user (UID 1654) from the .NET base
  image and listen on port 8080. They log to stdout only, so the app folder doesn't need to be writable.

### Security headers

Every response carries `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`
and `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` (the Development-only Scalar page is
exempt from the CSP). API responses are also marked `Cache-Control: no-store`, so customer data isn't kept by
browsers or proxies. Outside Development the API sends HSTS on HTTPS responses, and Kestrel's `Server` header is
turned off.

## Console demo

`CF.ConsoleApp` resolves `ICustomerFacade` from the shared core and runs a small
create / read / list flow against the database. It uses the same `AddCustomerCore`
registration (in `CF.Customer.Infrastructure/DependencyInjection`) as the API.

Its `launchSettings.json` sets `DOTNET_ENVIRONMENT=Development`, so `dotnet run` picks up
the API's user-secrets (same `UserSecretsId`) for the connection string. You can also pass
it with the `ConnectionStrings__DbConnection` environment variable instead.

- dotnet run --project CF.ConsoleApp
