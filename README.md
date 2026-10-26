[![Build & Test .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/dotnet-core.yml) [![CodeQL .NET 10.0](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/leandro-cervelin/cf_api_net_core/actions/workflows/codeql-analysis.yml)

# .NET 10 Customer API: an example app

A small REST API for managing customers, built with ASP.NET Core, Entity Framework Core and SQL Server.
You can sign up, log in to get a token, and read, update or delete your own customer record.

It's meant as a reference you can read from top to bottom. The business logic is kept separate from the web and
database code, and there are unit and integration tests.

- [Quick start](#quick-start)
- [Try it](#try-it)
- [How the code is organized](#how-the-code-is-organized)
- [Running the tests](#running-the-tests)
- [Going further](#going-further): local development without Docker, authentication details, deployment and
  hardening

## Quick start

You need [Docker](https://www.docker.com/products/docker-desktop/) (with Linux containers). That's all: Docker
builds the app and runs SQL Server for you.

1. Clone the repository and go to the `src` folder:

   ```bash
   git clone https://github.com/leandro-cervelin/cf_api_net_core.git
   cd cf_api_net_core/src
   ```

2. Create the settings file. It holds the database passwords and the key used to sign login tokens:

   ```bash
   cp CF.Api/.env.example CF.Api/.env
   ```

   The sample values are fine for trying the app on your own machine. Replace them with your own random values
   before running it anywhere else. `.env` is gitignored, so it won't be committed.

3. Start everything:

   ```bash
   docker compose -f CF.Api/docker-compose.yml up --build
   ```

   The first run takes a few minutes while the images download. When you see the API log that it's listening,
   it's ready at **http://localhost:8888**.

To stop it, press `Ctrl+C`. `docker compose -f CF.Api/docker-compose.yml down` removes the containers. Add `-v` to
that command to also delete the database.

## Try it

These examples use `curl` against the Docker setup. If you'd rather click through it, run the API locally instead
(see [Local development](#local-development-without-docker)) and use the interactive docs page at
https://localhost:7242/scalar/v1.

**1. Sign up.** Anyone can create an account:

```bash
curl -i -X POST http://localhost:8888/api/v1/customer \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Ada","surname":"Lovelace","email":"ada@example.com","password":"Password1@","confirmPassword":"Password1@"}'
```

The response is `201 Created` with the new customer's id, for example `{"id":1}`.

**2. Log in.** Exchange the email and password for an access token:

```bash
curl -X POST http://localhost:8888/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"email":"ada@example.com","password":"Password1@"}'
```

```json
{ "accessToken": "eyJhbGciOi...", "tokenType": "Bearer", "expiresAt": "..." }
```

**3. Call a protected endpoint.** Send the token in the `Authorization` header:

```bash
curl http://localhost:8888/api/v1/customer/1 \
  -H "Authorization: Bearer <paste the accessToken here>"
```

```json
{ "id": 1, "email": "ada@example.com", "firstName": "Ada", "surname": "Lovelace", "fullName": "Ada Lovelace" }
```

Things to try next:

- Leave out the header: you get `401 Unauthorized`.
- Ask for another customer's id: you get `403 Forbidden`. Customers can only see their own record.
- Sign up with a weak password or an email that's already taken: you get `400 Bad Request` with the reason.

All the endpoints:

| Endpoint | What it does | Who can call it |
| --- | --- | --- |
| `POST /api/v1/customer` | sign up | anyone |
| `POST /api/v1/auth/token` | log in, returns a token | anyone |
| `GET /api/v1/customer/{id}` | read a customer | that customer, or an admin |
| `PUT /api/v1/customer/{id}` | update a customer | that customer, or an admin |
| `DELETE /api/v1/customer/{id}` | delete a customer | that customer, or an admin |
| `GET /api/v1/customer` | list and search customers | admins only |

Admins are explained in [Authentication in detail](#authentication-in-detail).

## How the code is organized

The code is split into projects by responsibility. The arrows show which project uses which:

```text
CF.Api / CF.ConsoleApp        entry points: HTTP endpoints, configuration, startup
        │
        ▼
CF.Customer.Application       use cases and DTOs (the shapes of requests and responses)
        │
        ▼
CF.Customer.Domain            the Customer entity and the business rules

CF.Customer.Infrastructure    database access with EF Core (implements interfaces from Domain and Application)
CF.Migrations                 EF Core migrations, which create and update the database schema
```

The important rule: **Domain and Application don't know about ASP.NET or EF Core.** They only declare interfaces
such as `ICustomerRepository`, and Infrastructure provides the implementations. So the business rules can be
tested without a web server or a database, and the same core can be reused by other kinds of apps.

### Following one request

Here's what happens when someone calls `GET /api/v1/customer/1`:

1. **[`CustomerController`](src/CF.Api/Controllers/CustomerController.cs)** (Api) receives the HTTP request,
   checks the caller is allowed to see customer 1, and calls the facade.
2. **[`CustomerFacade`](src/CF.Customer.Application/Facades/CustomerFacade.cs)** (Application) uses
   [`CustomerMapper`](src/CF.Customer.Application/Mappers/CustomerMapper.cs) to turn the request DTO into a domain
   filter, and calls the service.
3. **[`CustomerService`](src/CF.Customer.Domain/Services/CustomerService.cs)** (Domain) applies the business rules,
   such as page size limits, unique emails and password hashing, and asks the repository for the data. The rules
   for a single customer (a valid email, a strong enough password) are methods on the
   [`Customer`](src/CF.Customer.Domain/Entities/Customer.cs) entity itself.
4. **[`CustomerRepository`](src/CF.Customer.Infrastructure/Repositories/CustomerRepository.cs)** (Infrastructure)
   queries SQL Server through EF Core's
   [`CustomerContext`](src/CF.Customer.Infrastructure/DbContext/CustomerContext.cs).
5. The result goes back up the same way. The facade maps the `Customer` entity to a `CustomerResponseDto`, so
   fields like the password hash never leave the API.

If a rule is broken, the service throws a `ValidationException` (or `EntityNotFoundException`), and
[`ExceptionFilter`](src/CF.Api/Filters/ExceptionFilter.cs) turns it into a `400` (or `404`) response. That filter is
the one place where exceptions become HTTP status codes.

All of these classes are connected with dependency injection in one place:
[`AddCustomerCore`](src/CF.Customer.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs).

### The console app

[`CF.ConsoleApp`](src/CF.ConsoleApp/Program.cs) is a second entry point that uses the same core, registered with
the same `AddCustomerCore` call. It creates a customer, reads it back and lists the first page, all without
ASP.NET. It's there to show that the business logic doesn't depend on the web layer. See
[Running the console app](#running-the-console-app) to run it.

### Where to start reading

- [`Program.cs`](src/CF.Api/Program.cs): how the API starts, in about 40 lines.
- [`Customer.cs`](src/CF.Customer.Domain/Entities/Customer.cs): the entity and its validation rules.
- [`CustomerService.cs`](src/CF.Customer.Domain/Services/CustomerService.cs): the business rules that need the
  database, such as "the email isn't taken".
- [`CustomerController.cs`](src/CF.Api/Controllers/CustomerController.cs): the HTTP endpoints.
- [`CustomerServiceTest.cs`](src/CF.Customer.UnitTest/Domain/Services/CustomerServiceTest.cs): how the rules are
  tested.

Skip [`Startup/ProductionSetup.cs`](src/CF.Api/Startup/ProductionSetup.cs) at first. It holds the production
concerns, such as rate limiting, security headers and health checks, which are explained in
[Going further](#going-further).

### Adding another entity

The project has one entity, but it's set up for more. Say you want customers to have addresses. You'd add an
`Address` class and follow the same path through each layer:

| Layer | Add |
| --- | --- |
| Domain | `Entities/Address.cs` with its validation rules; `Repositories/IAddressRepository.cs` extending `IRepositoryBase<Address>`; `Services/AddressService.cs` and its interface |
| Infrastructure | a `DbSet<Address>` and its table mapping in `CustomerContext`; `Repositories/AddressRepository.cs`; registrations in `AddCustomerCore` |
| Application | the DTOs, an `AddressMapper`, and `AddressFacade` with its interface |
| Api | `Controllers/AddressController.cs` |
| Migrations | a new migration (see below) |
| Tests | the same kinds of tests that exist for `Customer` |

[`RepositoryBase<T>`](src/CF.Customer.Infrastructure/Repositories/RepositoryBase.cs) is what saves you work here.
It already has add, get, update, remove and save for any entity, so `AddressRepository` only needs the queries
specific to addresses:

```csharp
public class AddressRepository(CustomerContext context)
    : RepositoryBase<Address>(context), IAddressRepository
{
    // Only address-specific queries go here, e.g. GetByCustomerIdAsync.
}
```

To create the migration, run this from the `src` folder. It needs the EF Core tool, which you can install with
`dotnet tool install --global dotnet-ef`.

```bash
dotnet ef migrations add AddAddress --project CF.Migrations --startup-project CF.Api
```

A generic repository is one common way to organize data access, not the only one. EF Core's `DbContext` already
works like a repository, and many projects use it directly from their services.

## Running the tests

From the repository root:

```bash
dotnet test
```

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). The test projects:

- `CF.Customer.UnitTest` and `CF.Api.UnitTest`: unit tests. They don't need a database.
- `CF.IntegrationTest`: starts a real SQL Server in Docker with [Testcontainers](https://testcontainers.com/) and
  calls the real API, so Docker must be running.

---

## Going further

Everything below is optional. It covers running the API without Docker, the details of authentication, and what
the app does to be safe to deploy. You don't need any of it to understand how the code is organized.

Most of the deployment and hardening code is in [`Startup/ProductionSetup.cs`](src/CF.Api/Startup/ProductionSetup.cs),
and the authentication code is in [`Authentication/`](src/CF.Api/Authentication).

### Local development (without Docker)

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and a SQL Server you can connect to.

The committed `appsettings.json` intentionally leaves out the database password and the signing key. Provide them
with [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), so no credential is committed.
From the `src` folder:

```bash
dotnet user-secrets --project CF.Api set "ConnectionStrings:DbConnection" "Data Source=localhost;Initial Catalog=CF;User ID=sa;Password=<your-password>;TrustServerCertificate=True;"
```

```bash
dotnet user-secrets --project CF.Api set "Jwt:SigningKey" "<random value, e.g. from: openssl rand -base64 48>"
```

Then run the API:

```bash
dotnet run --project CF.Api
```

In Development the API applies pending migrations on startup (`Database:MigrateOnStartup` is `true` in
`appsettings.Development.json`). The interactive API docs are at https://localhost:7242/scalar/v1. To use them,
call `POST /api/v1/auth/token`, paste the `accessToken` into the authentication panel (Bearer), and the protected
endpoints (marked with a lock) send it automatically.

#### Running the console app

`CF.ConsoleApp`'s `launchSettings.json` sets `DOTNET_ENVIRONMENT=Development`, so `dotnet run` picks up the API's
user-secrets (they share a `UserSecretsId`) for the connection string. You can also pass it with the
`ConnectionStrings__DbConnection` environment variable instead.

```bash
dotnet run --project CF.ConsoleApp
```

### What Docker Compose runs

Compose starts four services, in order:

| Service | What it does | Database login |
| --- | --- | --- |
| `db` | SQL Server, with data in the `mssql-data` volume. Reports healthy once it accepts connections. | |
| `db-init` | Creates the `CF` database and the API's `cf_app` login ([`init-app-login.sql`](src/CF.Api/db/init-app-login.sql)), then exits. | `sa` |
| `migrate` | Runs the EF Core migration bundle against `db`, then exits. | `sa` |
| `api` | Starts only after `migrate` has finished successfully. Listens on http://localhost:8888. | `cf_app` |

Compose runs the API in the Production environment, so the Scalar docs page isn't served there. Use local
development for that. It also serves plain HTTP only, so it turns off HTTPS redirection with
`HttpsRedirection__Enabled=false`. Leave that setting on in a real deployment.

### Authentication in detail

Tokens are JWTs signed with `Jwt:SigningKey` (HMAC-SHA256, at least 32 bytes). The app refuses to start without
it. The key is never committed: use user-secrets locally, and `JWT_SIGNING_KEY` in `CF.Api/.env` with Docker
Compose.

Admins are listed by customer id in `Jwt:AdminCustomerIds` (e.g. `Jwt__AdminCustomerIds__0=1`). Ids are used
rather than emails because emails aren't verified, and anyone could register an admin address that isn't taken
yet.

#### Changing the password

`PUT /api/v1/customer/{id}` always carries the full record, including `password`. Sending the existing password
proves the caller knows it, so name and email edits need nothing else. To set a **new** password, also send
`currentPassword`. Without it, or if it's wrong, the request fails with 400. This way a stolen token alone can't
take over an account. Admins editing another customer's account are exempt, as they can't know that password.

Passwords are limited to 72 bytes (UTF-8), because bcrypt ignores anything beyond that.

#### Token revocation

Each customer has a security stamp, embedded in every token they get. Changing the password or email rotates it,
and deleting the customer removes it. Either way, all previously issued tokens are rejected (401) and the client
has to log in again. Changing only the name keeps tokens valid.

Stamps are cached per instance for `Jwt:SecurityStampCacheSeconds` (default 30). A change made through an instance
takes effect there immediately; other instances pick it up when their cache entry expires. Set it to `0` to check
the database on every request.

### Deployment

#### Database migrations

Outside Development the API does **not** migrate on startup (`Database:MigrateOnStartup` defaults to `false`).
With several instances, startup migrations race each other, and they force the app's database login to have
schema-change (DDL) rights. Apply migrations as a separate step before rolling out the new version instead.

With Docker, build the `migrator` target and run it once with `ConnectionStrings__DbConnection` set. That's what
the `migrate` service in Compose does; in Kubernetes it fits a Job or an init container. From the `src` folder:

```bash
docker build -f CF.Api/Dockerfile --target migrator -t cf-api-migrator .
```

Without Docker, build a bundle and run it from your pipeline:

```bash
dotnet ef migrations bundle --project CF.Migrations --startup-project CF.Api -o efbundle
```

```bash
./efbundle --connection "<connection string>"
```

The bundle only applies migrations that are still pending, so running it on every deploy is safe.

#### Behind a reverse proxy or load balancer

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

#### Health checks

| Endpoint | Checks | Access | Rate limited |
| --- | --- | --- | --- |
| `/health/live` | the process is up | anonymous | no |
| `/health/ready` | the database is reachable | anonymous | no |
| `/health` | everything, as a detailed JSON report | admin token | yes |

Point liveness and readiness probes at `/health/live` and `/health/ready`. The detailed report names the checks and
their timings, so it's limited to admins.

### Hardening

#### Least privilege

- **Database:** the API connects as `cf_app`, a login that can only read and write data (`db_datareader` and
  `db_datawriter` in `CF`). It can't change the schema or reach other databases, which limits the damage if the
  app is ever compromised. Only the migration step uses a privileged login. Outside Compose, create an equivalent
  login with the same script (run it as an admin with `sqlcmd -v AppLogin=... AppPassword=...`). Keep
  `Database:MigrateOnStartup` off with such a login, since it can't apply migrations.
- **Container:** the API and migrator images run as the unprivileged `app` user (UID 1654) from the .NET base
  image and listen on port 8080. They log to stdout only, so the app folder doesn't need to be writable.

#### Rate limiting

Each client IP gets `RateLimiting:PermitLimit` requests per `RateLimiting:WindowSeconds` (60 per 60 seconds by
default). Beyond that the API answers `429 Too Many Requests`.

#### Security headers

Every response carries `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`
and `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` (the Development-only Scalar page is
exempt from the CSP). API responses are also marked `Cache-Control: no-store`, so customer data isn't kept by
browsers or proxies. Outside Development the API sends HSTS on HTTPS responses, and Kestrel's `Server` header is
turned off.
