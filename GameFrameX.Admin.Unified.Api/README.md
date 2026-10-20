<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Unified API

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/Admin)](https://github.com/GameFrameX/Admin/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761
<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

This is the backend service of the **GameFrameX game management platform**, written in .NET 10 + SqlSugar. Every button the frontend team clicks in the browser and every request a game client sends ultimately lands here.

> The companion frontend lives in [`GameFrameX.Admin.Unified.Vue`](../GameFrameX.Admin.Unified.Vue). The database defaults to zero-config SQLite locally and PostgreSQL in production. This repository only handles "expose APIs, store data, run scheduled jobs".

## Table of Contents

- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Three Hosts, Each on Its Own](#three-hosts-each-on-its-own)
- [Repository Directory Structure](#repository-directory-structure)
- [Quick Start](#quick-start)
- [How It All Runs](#how-it-all-runs)
- [Feature Modules Overview](#feature-modules-overview)
- [Adding a New CRUD Service](#adding-a-new-crud-service)
- [Development Conventions](#development-conventions)
- [FAQ and Pitfalls](#faq-and-pitfalls)
- [Deployment](#deployment)
- [Exporting OpenAPI for the Frontend](#exporting-openapi-for-the-frontend)
- [Command Cheatsheet](#command-cheatsheet)
- [Final Words](#final-words)
- [Documentation & Resources](#documentation--resources)
- [Changelog](#changelog)

---

## Project Overview

In one sentence: this provides every backend API for the game operations platform. Operations staff adding users, editing menus, viewing orders, sending mails, pushing versions, checking logs; game clients logging in, paying, fetching config; CI build machines writing back versions — the data outlet for all of these lives here.

The backend hosts **three independent hosts**, each running its own process on its own port, so none of them drags the others down:

- **Admin API** (`GameFrameX.Web.Api`, port `20802`) — APIs for the admin console
- **Client API** (`GameFrameX.Client.Api`, port `20808`) — APIs for game clients (login, payment, fetching config)
- **Hub API** (`GameFrameX.Hub.Api`, port `20806`) — aggregate APIs for external systems / game servers / CI builds / third-party WebHooks

Client API and Hub API are both "leaner": Client API only references Entity + Payment and never touches the old framework; Hub API uses traditional Controller routing and works directly with repositories, skipping the dynamic API machinery (more on that later).

**The three things most worth knowing**:

1. **Write a Service and you get APIs for free** (Admin API). Implement `IDynamicApiController`, and the public methods in your class automatically become REST endpoints — no hand-written Controllers, no hand-pasted `[Route]`. This is the soul of Admin API.
2. **Write an entity and tables get created for free**. SqlSugar Code-First: at startup it scans entity classes → creates tables → fills seed data. You never write a line of DDL SQL; change a field, restart, and the table schema catches up.
3. **Multi-tenancy comes out of the box**. Tag an entity, and queries automatically get a `TenantId` filter plus automatic soft delete; only SuperAdmin can punch through the wall. When writing business code you barely notice it exists.

---

## Tech Stack

| Category | Choice | Plain-Notes |
|------|------|----------|
| Runtime | .NET 10 | The engine this code runs on (`net10.0` in `Directory.Build.props`) |
| Language version | C# 10 | `LangVersion=10`; you can use records, file-scoped namespaces, etc. |
| Framework | ASP.NET Core native hosting | No extra framework; it's the stock stack |
| ORM | SqlSugar (Code-First) | Write C# entity classes, it generates the database tables for you |
| Authentication | JWT Bearer + HMAC Signature | Admin/Client/Hub share the same JWT; open APIs additionally use signatures |
| Real-time | SignalR (optional Redis Backplane) | For the server pushing messages to browsers |
| Cache | Memory / Redis | In-process memory is enough; switch to Redis across multiple machines |
| Scheduled jobs | In-house Scheduler | Jobs are stored in the database; multiple instances coordinate so nothing runs twice |
| Logging | Database + ElasticSearch + Serilog | Operation logs go to the DB; high volumes go to ES |
| Database | SQLite (local default) / PostgreSQL (production); also supports MySQL / Oracle and ten-plus others | See the [cheatsheet doc](GameFrameX.Application/Configuration/Database-ConnectionStrings.md) for other connection strings |
| Containerization | Docker Compose | One command brings up all three hosts |

---

## Three Hosts, Each on Its Own

```
Admin API (20802)                Client API (20808)              Hub API (20806)
    │                              │                              │
    │ For the admin console        │ For game clients             │ For external systems / game servers / CI
    │ Login / users / menus /      │ Login / payment / config     │ Zones / asset versions / goods /
    │ orders / logs / jobs         │                              │ redeem codes / build reports / Tapd WebHook
    │                              │                              │
    └──────────── Shared Entity ─────────────┬─────────────────────┘
                                           │
                                           ├── Shared Payment (Client only)
                                           └── Each of the three uses SqlSugar repositories directly
```

The dependency graph looks like this (arrows point left to right, i.e. reference direction):

```
Web.Api → Web.Core → Application.{System, Game, Client, Tenant} → Core → Entity → Core.Enum
Client.Api (independent, only references Entity + Payment)
Hub.Api (independent, references Application.Game + Core + Entity, but its Controllers do not call Application Services)
```

Remember one rule: **the business layer never references back up to the hosts**. `Application.*` and `Core.*` never know `Web.Api` exists; dependencies only point downward. That is what lets Client API / Hub API cleanly pull Entity out and use it alone.

### Why Hub API Looks Different from Admin API

Admin API is a "dynamic API" — write a Service and the API appears; Hub API is a "traditional Controller" — hand-written `[Route("api/[controller]/[action]")]` routing, Controllers directly inject `Repository<T>` to operate on entities, and **do not call Application Services**. This is a deliberate convention in Hub.Api: it faces external systems, the logic is simple, and it must be light and fast, so the Controller does its own duplicate checks and ownership validation instead of going through Admin's layered stack.

For details, see [Hub API: The Third Door](#hub-api-the-third-door) under [How It All Runs](#how-it-all-runs).

---

## Repository Directory Structure

```
GameFrameX.Admin.Unified.Api/
├── GameFrameX.Web.Api/            # Admin API host (startup entry, compose service name web-api)
├── GameFrameX.Client.Api/         # Client API host (standalone WebApplication)
├── GameFrameX.Hub.Api/            # Hub API host (standalone WebApplication, traditional Controller routing)
├── GameFrameX.Web.Core/           # Admin startup logic, middleware, DI setup (important)
├── GameFrameX.Application/        # Application layer (config files, seed data)
├── GameFrameX.Application.System/ # System management services (users/roles/menus/tenants/logs...)
├── GameFrameX.Application.Game/   # Game management services (zones/orders/mails/redeem codes...)
├── GameFrameX.Application.Client/ # Client management services (versions/channels/asset packages...)
├── GameFrameX.Application.Tenant/ # Tenant-level services (devices / device groups)
├── GameFrameX.Entity/             # Entity layer (Base / System / Game / Client)
├── GameFrameX.Core/               # Core layer (repositories, service base classes, auth, SignalR)
├── GameFrameX.Core.Enum/          # Enum definitions
├── GameFrameX.Core.Extension/     # Extension methods and shared DTOs (important; look here for wheels first)
├── GameFrameX.Core.SqlSugar/      # SqlSugar configuration, AOP, interceptors
├── GameFrameX.Core.Utility/       # Utility classes
├── GameFrameX.Core.BaseInputOutput/ # Base input/output models
├── Payment/                       # Payment integrations (referenced only by Client API)
│   ├── GameFrameX.Pay.Abstract/          # Payment abstractions (define first, implement after)
│   ├── GameFrameX.Pay.AliPay/            # Alipay
│   ├── GameFrameX.Pay.Apple/             # Apple IAP (in-app purchase verification)
│   ├── GameFrameX.Pay.Apple.AppStoreServer/  # App Store Server API v2
│   ├── GameFrameX.Pay.Google/            # Google Play
│   └── GameFrameX.Pay.Xsolla/            # Xsolla
├── Dockerfile.web-api             # Admin API Dockerfile (sdk:10.0 → aspnet:10.0)
├── Dockerfile.client-api          # Client API Dockerfile
├── Dockerfile.hub-api             # Hub API Dockerfile
└── (Unified orchestration has moved to the repository root docker-compose.yml: PostgreSQL + three APIs + frontend, 5 services in total)
```

In day-to-day coding you'll spend 99% of your time inside `Application.*` and `Entity`. `Core.*` is the foundation — read who depends on it before touching it.

---

## Quick Start

### Prerequisites

- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code — pick whichever you like
- No database to pre-install: running from source locally defaults to SQLite (the file lands in the repository root `db/` directory); install PostgreSQL only when you need it, or just use the Docker Compose below (PG included)

### Database: Zero-Config by Default, PostgreSQL for Production

**Running from source locally (default SQLite)**: nothing to configure. The default connection lives in `GameFrameX.Application/Configuration/Database.json`; the three hosts share the same SQLite database file (WAL mode) in the repository root `db/` directory. The convention is to **run `dotnet run` from the repository root** (the connection string resolves relative to the process working directory).

Note: SQLite is for local development/debugging only. SQLite is forbidden in live/production environments (weak concurrent writes, no standalone service process, insufficient backup/restore and ops monitoring) — use PostgreSQL.

**Production / containers (PostgreSQL, override env vars in pairs)**: when overriding the database, the type and the connection string must be set **as a pair**; a half override fails outright at startup. All three hosts (Web / Client / Hub) use **the same pair of variables**:

- `GFX_MAIN_DB_TYPE=PostgreSQL` + `GFX_MAIN_DB_CONNECTION_STRING=Host=...`
- Default tenant database needs no configuration: it is derived automatically from the main connection string (only the database name changes, see below)
- The legacy bare `DbType` / `ConnectionString` startup parameters still work (compatibility layer); startup prints a migration warning — switch soon
- For other databases (MySQL / Oracle / DM… all 34 DbTypes), connection string syntax and examples: [`GameFrameX.Application/Configuration/Database-ConnectionStrings.md`](GameFrameX.Application/Configuration/Database-ConnectionStrings.md)

A few more database conventions worth knowing:

- **Tenant databases are created automatically**. When creating a normal tenant, the system automatically generates the `gameframex_admin_tenant_<TenantId>` database and the corresponding login role — **SuperAdmin does not need to fill in tenant connection strings by hand**.
- **The default tenant database is also derived automatically (C269)**. Main connection string → default tenant connection string: for PostgreSQL / MySQL only the database name is replaced with `gameframex_admin_tenant_1300000000001` (same rule as normal tenant DBs; account/password inherited from the main DB; MySQL forces utf8mb4 on database creation); for SQLite it lands next to the main database file as `gameframex_admin_tenant_1300000000001.db`. Only PostgreSQL / MySQL / SQLite are supported; other types fail at startup. Explicit configuration wins: add back the `ConfigId=1300000000001` entry in `Database.json` or set `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING` as a pair.
- **Upgrading existing deployments (BREAKING)**: the default tenant database name changes from `gameframex_admin_tenant_default` to `gameframex_admin_tenant_1300000000001`. Pick one: a. rename online with `ALTER DATABASE gameframex_admin_tenant_default RENAME TO gameframex_admin_tenant_1300000000001` (the registry follows the derived value automatically); b. set `GFX_DEFAULT_TENANT_DB_*` as a pair pointing at the old database name and freeze it.
- **Compose ships PostgreSQL**. Running `docker compose up` from the repository root starts a `postgres:16-alpine` (first startup creates the databases automatically, see `docker/postgres/init.sql`; data lands in the repository root `postgres-data/` bind mount). To reuse an existing postgres container on the host, set `POSTGRES_HOST=host.docker.internal` etc. in the repository root `.env` and remove the api services' `depends_on` on `postgres`.
- **All three hosts read the same default config**. Client / Hub automatically load `Database.json` at startup (same source as Admin; change it once and all three pick it up); environment variable overrides work the same way as compose.

### Startup Option 1: Docker Compose One Command (Recommended, Zero Config)

```bash
# Run from the repository root (not inside the Api subdirectory)
docker compose up -d --build
# Ships PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + frontend(20800); wait for healthchecks to turn green
```

### Startup Option 2: Run from Source (Zero Config, SQLite)

```bash
# Note: run from the repository root; the four hosts share the SQLite database files under ./db/

# Admin API (admin console)
dotnet run --project GameFrameX.Web.Api
# Listens on http://*:20802, Swagger UI: http://localhost:20802/kapi

# Client API (game clients)
dotnet run --project GameFrameX.Client.Api
# Listens on http://*:20808

# Hub API (external systems / CI / WebHook)
dotnet run --project GameFrameX.Hub.Api
# Listens on http://localhost:20806, Swagger UI: http://localhost:20806/swagger

# Game API (game-server side) has been merged into Client API (C268): GameUser* controllers are exposed via Client.Api (http://*:20808)
```

On first startup of Admin API, SqlSugar Code-First creates the database and tables automatically and pours in the seed data (admin account, roles, menus, dictionaries, and so on). So don't panic when an empty database suddenly grows a pile of tables — that's normal.

### Startup Option 3: Single Container docker run

The default config inside the image is SQLite (starts with zero env vars; data lives at `/app/db/` in the container, mount a volume to persist). Production usage injects the paired env vars to connect PostgreSQL:

```bash
docker run -p 20802:20802 \
  -e GFX_MAIN_DB_TYPE=PostgreSQL \
  -e GFX_MAIN_DB_CONNECTION_STRING="Host=your-pg;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  -e GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL \
  -e GFX_DEFAULT_TENANT_DB_CONNECTION_STRING="Host=your-pg;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=...;Password=..." \
  your-image
```

### Stuck?

| Symptom | Most Likely Cause |
|------|------------|
| Startup reports database connection failure | Wrong connection string in `Database.json` (or the `GFX_MAIN_DB_*` env vars), or PG isn't running |
| Startup reports "DbType is Sqlite/PostgreSQL but the connection string is not in … style" | Half-overridden env vars: `GFX_MAIN_DB_TYPE` and `GFX_MAIN_DB_CONNECTION_STRING` must be set as a pair |
| Startup reports "default tenant database does not support auto-derivation" | The main database uses a DbType other than PostgreSQL / MySQL / SQLite; set `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING` as a pair to specify explicitly |
| SQLite database file in the wrong place | The connection string resolves relative to the process working directory — go back to the repository root and run `dotnet run`; just delete the stray `db/` that appeared in the project directory |
| Tables not created | Code-First switches default to fully on in code (C278); if you explicitly configured them off, check `Database.json` or env vars |
| Swagger won't open | Non-development environments don't mount Swagger by default; see [Exporting OpenAPI](#exporting-openapi-for-the-frontend) below |
| Seed data not inserted | The seed switch was turned off; also, seeds are upserted by primary key — after changing seeds you must delete the old rows for them to be re-inserted |
| Hub API startup reports "JWTSettings not configured" | Hub reuses Admin's JWT config; `Hub.Api/appsettings.json` must contain a `JWTSettings` node matching Admin's |
| Port already in use | 20800 / 20802 / 20808 / 20806 occupied by another process (frontend container and dev server both fight over 20800); check with `lsof -i :20806` |

---

## How It All Runs

### Dynamic API: Writing a Service Is Writing the API (Admin API)

After an Admin API service implements `IDynamicApiController`, the framework automatically turns the public methods of the class into REST endpoints (routes are camelCase). No hand-written Controllers:

```csharp
public class MyService : ITransient, IDynamicApiController
{
    // Automatically becomes GET /api/my/list
    public List<MyEntity> List() => ...;
}
```

Route generation rules (memorize these — the frontend will call you 100% by this convention):

1. Take the class name and strip the `Service` / `AppService` / `Controller` suffix (only once)
2. Lowercase the first letter
3. Prepend the default prefix `api/`

| Class Name | Suffix Stripped | Final Route Prefix |
|------|--------|-------------|
| `GameAppService` | `GameApp` | `/api/gameApp` |
| `SystemUserService` | `SystemUser` | `/api/systemUser` |
| `GameOrderService` | `GameOrder` | `/api/gameOrder` |

Method names are appended the same way: `Page()` → `/api/gameApp/page`, `ResetSecret()` → `/api/gameApp/resetSecret`.

> **The `Service` suffix must never appear in URLs.** This is not advice, it's a hard rule — frontend calls are generated by this convention; rename your backend class and the frontend blows up.

When you need fine-grained control, attributes like `[HttpPost]`, `[AllowAnonymous]`, `[NonAction]` work as usual.

### Dependency Injection: Register by Marker Interface

No hand-written `services.AddScoped<>()`; registration happens automatically via marker interfaces:

| Interface | Lifetime | When to Use |
|------|----------|----------|
| `ITransient` | Transient | New object on every resolve; most services use this |
| `IScoped` | Scoped | Reused within a single request |
| `ISingleton` | Singleton | Globally unique; be careful putting stateful things here |

Infrastructure components (SqlSugar, JWT, SignalR, EventBus and friends) are registered manually in `GameFrameX.Web.Core/Startup.cs` because they need configuration parameters.

### Repositories and Multi-Database Routing

`SqlSugarRepository<T>` inherits from SqlSugar's `SimpleClient<T>`; its constructor decides which database the data goes to based on the attributes on the entity:

| Entity Attribute | Routing Target |
|----------|----------|
| `[SystemTable]` | Main database |
| `[LogTable]` | Log database |
| `[Tenant]` | Tenant-specific database |
| No attribute | Routed by the TenantId in the JWT Claim |

You just write entities and tag them; the repository handles the rest of the routing.

### Authentication: Shared JWT, Plus Signatures

**JWT Bearer (all three hosts share the same key)**

- `JwtHandler` is implemented on top of the ASP.NET Core authorization handler (Admin API)
- All three read the same `JWTSettings` — so **a token issued by Admin login is accepted directly by Client API and Hub API**
- Automatic renewal: the Admin API response header `X-Authorization` carries a new token; the frontend swaps it silently
- Permission checks: SuperAdmin bypasses all checks; regular users match button permissions by route name

**HMAC Signature (for open APIs)**

- `SignatureAuthenticationHandler` implements HMAC-SHA256 signature authentication
- Requests must carry four headers: `accessKey` + `timestamp` + `nonce` + `sign`
- Signature formula: `HMACSHA256(secret, "METHOD&URL&accessKey&timestamp&nonce")`
- Anti-replay: nonce cache + timestamp skew check (5 minutes of skew allowed by default)

### Multi-Tenancy and Soft Delete: Automatic Query Filtering

- `ITenantIdFilter`: queries automatically get a `TenantId` filter appended; tenant data is isolated by nature
- `ISafeDeletedFilter`: queries automatically get `IsDeleted == false` appended; deletion is just a marker
- SuperAdmin bypasses all filters, so the super admin can see everything (**but Hub API rejects SuperAdmin, see below**)

These two are the kind of mechanisms you barely notice while writing business code, yet they keep covering your back.

### Audit Fields: Auto-Filled, No Manual Work

SqlSugar AOP does this for you in `SqlSugarSetup.SetDbAop()`:

| Operation | Fields Auto-Filled |
|------|-------------|
| Insert | `CreatedTime`, `CreatedId`, `CreatedName`, `TenantId` |
| Update | `UpdateTime`, `UpdatedId`, `UpdatedName` |

It also writes data changes into the audit log (`SystemLogDiff` / `TenantLogDiff`) along the way. So when you create entities you never assign audit fields yourself — the framework fills them in.

### Snowflake IDs

Distributed unique IDs are generated with Foundation Snowflake's **compact short ID layout** (decision frozen 2026-09-15, see `gfx-doc/decisions/2026-09-15-compact-snowflake-id.md`):

| Layout Parameter | Value | Notes |
|---|---|---|
| epoch | 2026-09-01 | Deployment epoch; immutable once data is persisted |
| WorkerId | 3 bits (0-7) | The only instance-distinguishing dimension |
| DataCenterId | 0 bits | Single data center; dimension removed |
| Sequence | 10 bits | 1024 per millisecond per WorkerId |

The output is 13-14 digit numbers (< 2^53, no precision loss in JS), so Admin pages can display primary keys directly without converting to strings. The layout is frozen: changing the epoch or bit widths = overlapping the historical Id space, which is a data-integrity incident.

Multi-instance collision prevention **does not rely on Redis** (in-process leases cannot mutex across processes; that approach is retired); it relies on explicitly configured mutual exclusion: every instance writing to the same database (Web.Api replicas / Hub / Client) must be configured with a mutually exclusive `SnowFlake:WorkerId` (env var `SnowFlake__WorkerId`, 0-7); out-of-range values fail at startup — the cap is 8 same-database instances per data center. Defaults: Web.Api=1 (appsettings) / Hub=2 / Client=3 (code fallback); an all-default single-machine deployment never collides.

SqlSugar's primary-key backfill also goes through this uniformly: `StaticConfig.CustomSnowFlakeFunc` is installed process-wide by `SqlSugarSnowflakeFallbackInitializer` (ModuleInitializer) when the Core.Extension module loads, so no code path (including Hub / Client and test processes) ever falls back to SqlSugar's built-in 18-19 digit long layout.

### Global Response Wrapper

Admin API's `AdminResultProvider` wraps every response in a uniform envelope:

```json
{
  "Code": 200,
  "Type": "ok",
  "Message": null,
  "Result": { ... },
  "Extras": null,
  "Time": "2026-01-01T00:00:00"
}
```

The frontend only recognizes this structure: `Code === 200` passes, everything else goes to error handling. Hub API has its own `HttpJsonResultData<T>` (`Success` / `Fail` factory methods) — similar in shape but independent, and it does not go through `AdminResultProvider`.

### SignalR Real-Time Communication

- Hub: `OnlineUserHub`, path `/hubs/onlineUser`
- Grouped by tenant (`GROUP_ONLINE_{tenantId}`); tenant A's messages never drift into tenant B
- Capabilities: online user list, force logout, message sending (unicast / broadcast / exclude-self / target user)
- For multi-instance deployment, optional Redis Backplane + Data Protection Key persistence lets messages fan out across instances

### Hub API: The Third Door

Hub API (`GameFrameX.Hub.Api`, port `20806`) is the aggregate API for **external systems**, positioned differently from both Admin API and Client API. It serves these callers:

| Caller | What They Do | Controller |
|--------|------|-----------------|
| Game servers / ops tools | Fetch zones, query asset versions, query goods, redeem codes | `GameAreaController` / `GameAssetPackageVersionController` / `GameGoodsController` / `GameRedeemCodeController` |
| CI / build machines | Write back asset versions / full-package versions after a build | `PackageBuildReportController` |
| Third-party platforms | Receive Tapd WebHook callbacks | `TapdWebHookController` |

It has a few things that differ from the others — know these before writing code:

**1. Traditional Controller routing, not the dynamic API.**
The base class `BaseController` is tagged `[Route("api/[controller]/[action]")]`; routes are hand-written, not generated from class names. So URLs here do **not** strip the `Service` suffix automatically — they are built directly from the `Controller` name + `Action` name.

**2. Controllers use `Repository<T>` directly and do not call Application Services.**
This is the iron law of Hub.Api. Controllers inject repositories in their constructors and do their own queries, writes, duplicate checks, and ownership validation. The comment in PackageBuildReportController says it plainly: "Follows the Hub.Api Controller convention (Repository operates on Entity directly, no Application Service calls), so the layer's ScopeValidator is not involved and Application.Client's CheckXxxUnique is not reused — the Controller does its own ownership and duplicate checks."

> Implication: the Validators and Application-layer uniqueness checks you contribute for Admin API are **dead code for Hub.Api** — don't expect Hub to reuse them. If Hub needs validation, write it in the Controller yourself.

**3. Three authentication scenarios, each with its own path.**

| Scenario | How Auth Works | Where Tenant Comes From | Example |
|------|----------|-----------|------|
| Logged-in user | JWT (`BaseController` defaults to `[Authorize]`) | Parsed from JWT | Zone / asset / goods / redeem code queries |
| Anonymous WebHook | `[AllowAnonymous]` | From request headers | `TapdWebHookController` |
| Machine to machine | `[AllowAnonymous]` + `TenantGameAppOpenAccessAuthMiddleware` | The credential's bound App | `PackageBuildReportController` (build reporting) |

The machine-credential path validates AccessKey + signature + scope, **not JWT**, so the Controller is marked `[AllowAnonymous]` to skip JWT and a dedicated middleware takes over.

**4. Tenant resolution is centralized in middleware, and the super admin is rejected.**
`HubTenantMiddleware` (applied to everything except `/health`) writes the tenant Id uniformly into `HttpContext.Items`: authenticated requests take it from the JWT, anonymous WebHooks from request headers. Then `DelegateSqlSugarTenantIdResolver` feeds that Id into SqlSugar for tenant routing. **SuperAdmin is rejected outright** — Hub is a tenant-domain service, and a platform-level super admin shouldn't tunnel into some tenant's data through Hub.

**5. Ships its own optimistic-lock middleware and health checks.**
`OptimisticConcurrencyMiddleware` sits at the very front of the pipeline, backstopping write operations that need concurrency control. There are three health check endpoints: `/health`, `/health/live`, `/health/ready` (compose uses them for liveness probes).

Hub API's middleware pipeline order (`Program.cs`):

```
OptimisticConcurrencyMiddleware
  → Swagger (switch-controlled)
  → CORS
  → Authentication → Authorization
  → HubTenantMiddleware (except /health; writes tenant Id; rejects super admin)
  → TenantGameAppOpenAccessAuthMiddleware (only /api/PackageBuildReport)
  → HealthChecks / MapControllers
```

### Config Files: Standalone JSON, Loaded on Demand

Admin API's standalone JSON configs all live in `GameFrameX.Application/Configuration/`, scanned automatically at startup:

| File | What It Governs |
|------|--------|
| `Database.json` | Database connections, Code-First switches, seed data switch |
| `Swagger.json` | Swagger / Knife4j UI configuration |
| `SMS.json` | SMS service configuration |
| `Upload.json` | File upload, OSS configuration |
| `Wechat.json` | WeChat official account / mini program configuration |
| `Enum.json` | Enum scanning assemblies |

Two configs "have already moved" — stop hunting for them:

> The platform JWT config has moved into `platform_jwt_settings` in the main database `SystemConfig` and **`JWT.json` is no longer a runtime source** (Hub API still reads the `JWTSettings` node from its own `appsettings.json`, but the key must match Admin's).
>
> `App.json` has been retired: host and framework settings like listening ports, dynamic API, CORS, and snowflake IDs moved into `Web.Entry/appsettings.json`; Cryptogram keys became env vars `GFX_CRYPTOGRAM_*` plus built-in defaults (see `CryptogramSecretResolver`); the password strength policy moved into `platform_password_policy_config` in main-database `SystemConfig`; zone-server HTTP settings moved to zone-level DB configuration.

---

## Feature Modules Overview

> These services run mainly on Admin API (dynamic API). Hub API's endpoints are a separate set of traditional Controllers; see [Hub API: The Third Door](#hub-api-the-third-door).

### System Management

A complete back-office management module built on the in-house dynamic API routing. This is the "every game needs it regardless of genre" part.

| Feature | Service Class | What It Does |
|------|--------|--------|
| Auth login | `SystemAuthService` | Account/password and SMS-code login, JWT issuance + auto renewal, captcha |
| User management | `SystemUserService` | User CRUD, password reset, user-role binding (note: this service actually manages `TenantUser`; the naming is a bit misleading) |
| Role management | `TenantRoleService` | Role CRUD, data-scope control, role-menu permission binding |
| Menu permissions | `GlobalMenuService` | Tree menus, button-level permission resolution, route-to-API mapping |
| Organizations | `TenantOrgService` | Tree-shaped org CRUD, role-org data scope |
| Positions | `TenantPosService` | Position CRUD, ownership validation |
| Tenant management | `SystemTenantService` | Multi-tenant CRUD, per-tenant database connection configuration |
| Dictionary management | `GlobalDictTypeService` / `GlobalDictDataService` | Dictionary type and value CRUD, status toggling |
| File management | `TenantFileService` | File upload/download, OSS object storage |
| Scheduled jobs | `SystemJobService` | Job and trigger CRUD, pause / start / run now, cluster coordination |
| Database management | `SystemDatabaseService` | Table / column CRUD, online schema management |
| Notices | `TenantNoticeService` | Notice publishing, SignalR broadcast, read / unread tracking |
| Online users | `TenantOnlineUserService` | Real-time online list, force logout, single sign-on |
| Logging | `LogOperationService` / `LogExceptionService` / `LogVisitService` / `LogDiffService` | Operation, exception, visit logs and data-change audits, with Excel export |
| Cache management | `SystemCacheService` | Unified cache abstraction (Memory / Redis), cache key management |
| System config | `SystemConfigService` | Key-value config CRUD |
| Enum service | `SystemEnumService` | Reflects enum types and values from assemblies |
| Constant service | `SystemConstService` | Reads constants annotated with `[ConstAttribute]` |
| SMS service | `SystemSmsService` | Aliyun SMS verification codes, 60-second TTL |
| Region management | `SystemRegionService` | 5-level administrative division tree, syncs from the statistics bureau |
| Print templates | `TenantPrintService` | Print template CRUD |
| Open APIs | `SystemOpenAccessService` | AccessKey / Secret management, HMAC signature authentication |
| Plugin management | `SystemPluginService` | Compiles C# code at runtime, dynamically registers it as API endpoints |
| WeChat | `SystemWechatService` / `SystemWxOpenService` | Official account OAuth2, mini program login, template messages, JS-SDK |
| WeChat Pay | `SystemWechatPayService` | JSAPI orders (direct / partner), payment callbacks |
| Server monitoring | `SystemServerService` | Host info, CPU / memory, disk, loaded assemblies |

### Game Management

Business modules for game operations; every service inherits the `GenericCrudService` base class (write one service, get six endpoints — more below).

| Feature | Service Class | What It Does |
|------|--------|--------|
| Game accounts | `GameUserEntityService` | Email registration, ban / unban |
| Game roles | `GameRoleEntityService` | Role management, sandbox mode toggling, data sync from game servers |
| Game zones | `GameAreaEntityService` | Zone management, Docker container lifecycle (create / start / stop / restart) |
| Zone categories | `GameAreaClassifyEntityService` | Zone cluster classification |
| Order management | `GameOrderEntityService` | Payment orders, multi-dimension filtering (status / role / zone / channel / platform / date) |
| Order fulfillment | `GameOrderShipService` | Shared delivery logic; calls the game server API over HTTP to deliver goods |
| Game mail | `GameMailEntityService` | Mail sending, goods attachment parsing, recipient validation |
| Redeem codes | `GameRedeemCodeEntityService` | Redeem code management, goods attachment parsing |
| Redeem code channels | `GameRedeemCodeChannelEntityService` | Redeem code distribution channels |
| Redemption records | `GameRedeemCodeUseRecordEntityService` | Redeem code usage records |
| Web shop | `GameWebShopEntityService` | Web shop product management |
| Shop orders | `GameWebShopOrderEntityService` | Web shop orders |
| Product management | `GameCommodityEntityService` | Product / commodity management |
| Goods items | `GameGoodsEntityService` | In-game items / goods |
| Item catalog | `GameGoodsCatalogueEntityService` | Item classification |
| Scheduled events | `GameTimeJobEntityService` | In-game scheduled tasks / events |
| Docker tags | `GameDockerTagEntityService` | Game server Docker image version management |
| Announcements | `GameAnnouncementEntityService` / `GameBannerAnnouncementEntityService` | Game announcements, carousel announcements |
| Home page config | `GameHomeEntityService` | Game home page configuration |

### App Center

Configuration management modules for client apps.

| Feature | Service Class | What It Does |
|------|--------|--------|
| App versions | `GameAppReleaseVersionService` | App version management, force-update control |
| Asset packages | `GameAppAssetPackageService` | AssetBundle / asset package management |
| Asset versions | `GameAppAssetPackageVersionService` | Asset package version tracking |
| Asset history | `GameAppAssetPackageVersionHistoryService` | Asset version change history |
| Channel definitions | `GlobalChannelDefinitionService` / `TenantChannelDefinitionService` | Distribution channels (App Store, Google Play, etc.) |
| Device management | `TenantDeviceService` | Device registration and identification (in `Application.Tenant`) |
| Device groups | `TenantDeviceGroupService` | Device grouping (grayscale / A/B testing, in `Application.Tenant`) |
| Languages | `GlobalLanguageService` | Supported language configuration |
| Package splitting | `GameAppPackageService` | App package splitting, channel binding and base-package inheritance |
| Platform management | `GlobalPlatformService` | Runtime platforms (iOS, Android, etc.) |
| Version definitions | `GameAppVersionDefinitionService` | Version-to-asset mapping |
| Client config | `GameAppClientConfigService` | Client key-value configuration |
| Config keys | `GameAppClientConfigKeyService` | Client config content keys |

### Hub API Endpoints (A Separate Set)

These are traditional Controllers running on Hub API (20806); they **do not use the dynamic API and do not call Application Services**:

| Endpoint | Controller | Auth | What It Does |
|------|-----------|------|------|
| Zone queries | `GameAreaController` | JWT | Lets game servers pull zone lists |
| Asset package versions | `GameAssetPackageVersionController` | JWT | Query asset versions, update with optimistic lock |
| Goods queries | `GameGoodsController` | JWT | Query in-game goods |
| Redeem codes | `GameRedeemCodeController` | JWT | Redeem code verification and related operations |
| Build reporting | `PackageBuildReportController` | Machine credentials | CI / build machines write back asset versions / full-package versions (idempotent upsert) |
| Tapd WebHook | `TapdWebHookController` | Anonymous (tenant from headers) | Receives Tapd callbacks |

### Payment Integration

Used only by Client API. An abstraction layer plus multiple channel implementations; to add a channel, just implement the abstraction:

| Channel | Project |
|------|------|
| Alipay | `GameFrameX.Pay.AliPay` |
| Apple IAP | `GameFrameX.Pay.Apple` (in-app purchase receipt verification) |
| App Store Server | `GameFrameX.Pay.Apple.AppStoreServer` (API v2 notification handling) |
| Google Play | `GameFrameX.Pay.Google` |
| Xsolla | `GameFrameX.Pay.Xsolla` |

---

## Adding a New CRUD Service

> This section targets **Admin API** dynamic API services. Hub API uses traditional Controllers and does not follow this recipe — to add a Hub endpoint just create a Controller + inject a repository, following the existing ones in `GameFrameX.Hub.Api/Controllers/`.

Follow along once and you'll have mastered 80% of the daily backend work. Suppose we're adding a "game item".

### 1. Create the Entity

Create the entity class in the matching subdirectory of `GameFrameX.Entity/`:

```csharp
[SugarTable(null, "Game item")]
[SystemTable]
[IncrementTable]
public class GameItemEntity : EntityBasic
{
    [SugarColumn(ColumnDescription = "Item name", Length = 64)]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "Item count")]
    public int Count { get; set; }
}
```

Pick an entity base class as needed:

| Base Class | Characteristics | Use Case |
|------|------|----------|
| `EntityBaseId` | Only `Id` | Join tables, non-tenant entities |
| `EntityTenantBase` | Id + tenant + audit + soft delete | System tables (externally referenced) |
| `EntityBasic` | Inherits `EntityTenantBase` | General-purpose business entities (most common) |
| `EntityBasicData` | Adds org data permissions | Data that must be filtered by organization |
| `EntityBasicSelect` | Adds Name + Description | Dropdown-picker entities |

### 2. Create Seed Data (Optional)

Create the seed class in the matching subdirectory of `GameFrameX.Application/SeedData/`:

```csharp
public class GameItemSeedData : ISqlSugarEntitySeedData<GameItemEntity>
{
    public IEnumerable<GameItemEntity> HasData()
    {
        return new[]
        {
            new GameItemEntity { Id = YitIdHelper.NextId(), Name = "Coin", Count = 0 }
        };
    }
}
```

Seed data uses `StorageableByObject` to InsertOrUpdate by primary key, so repeated runs are safe. **Note**: because it upserts by primary key, if you change a seed's content but not its key, the old row is not overwritten — to see the new value, delete the old row and restart.

### 3. Write the Service

Create the service under `GameFrameX.Application.Game/Service/` (or System / Client / Tenant):

```csharp
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameItemService : GenericCrudService<GameItemEntity, BaseAddInput, BaseUpdateInput,
    GameItemEntity, BasePageInput, ITransient
{
    protected override ISugarQueryable<GameItemEntity> BuildPageQuery(BasePageInput input)
    {
        return Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                u => u.Name.Contains(input.SearchKey));
    }
}
```

Done. You now automatically own six endpoints:

| Endpoint | What It Does |
|------|------|
| `Page` | Paged query |
| `Add` | Create |
| `Update` | Update |
| `Get` | Fetch one by Id |
| `List` | List (unpaged) |
| `Delete` | Delete (soft delete) |

If you also need dropdown-picker support, switch the base class to `GenericCrudSelectService` and you get the `Select` and `CheckNameUnique` endpoints for free.

### 4. (Optional) Custom DTOs

For simple cases just reuse the base-class ones: `BaseAddInput`, `BaseUpdateInput` (with `Id`), `BaseIdInput`, `BasePageInput` (with `SearchKey`, `Page`, `PageSize`, sort fields).

When the fields aren't enough, add them in the service's `Dto/` subdirectory or in `GameFrameX.Core.Extension/Dto/`:

```csharp
// Output DTO — just inherit the entity
public class GameItemOutput : GameItemEntity, IBasePageOutput<GameItemEntity> { }

// Update DTO — inherit the base and add fields
public class UpdateGameItemInput : BaseUpdateInput
{
    [Required] public string Name { get; set; }
}
```

### 5. Tell the Frontend

Once the backend OpenAPI changes, the frontend must re-export `openapi.json` and run `pnpm gen:api`, otherwise frontend call sites fall out of sync. See [Exporting OpenAPI for the Frontend](#exporting-openapi-for-the-frontend) for the exact steps.

---

## Development Conventions

> This section is about **conventions** — this is how everyone in the project does it; following along saves pain.

### Admin API vs Hub API: Don't Mix the Two

| Dimension | Admin API | Hub API |
|------|-----------|---------|
| Routing | Dynamic API (`IDynamicApiController`) | Traditional Controllers (`[Route("api/[controller]/[action]")]`) |
| Data access | Via Application Service → repository | Controller directly injects `Repository<T>` |
| Response wrapping | `AdminResultProvider` auto-wraps | Manually return `HttpJsonResultData<T>` |
| Reuses Application layer | Yes | No (Validators / CheckXxxUnique are dead code for Hub) |
| Adding an endpoint | Write a Service | Create a Controller + inject a repository |

When adding Hub endpoints, **don't** try to reuse the lifted-up validation from the Admin side — Hub writes its own.

### Repository Extension Methods: Look for the Wheel Before Building One

`GameFrameX.Core.Extension/RepositoryExtension.cs` already ships the common ones (Hub API's `Repository<T>` can use some of them too, like the optimistic-lock update):

| Method | What It Does |
|------|--------|
| `FakeDelete` / `FakeDeleteAsync` | Soft delete (`IsDeleted = true`) |
| `OrderBuilder(input)` | Dynamic ordering based on `BasePageInput` |
| `UpdateWithDiffLog` / `InsertWithDiffLog` | Insert/update/delete with audit logging |
| `UpdateWithOptimisticLockAsync` | Update with optimistic lock (used by Hub API build reporting) |
| `AS<T>()` | Cross-database queries / operations |
| `RunWithoutFilter` | Temporarily skip query filters (e.g. to query deleted data) |

### Scheduled Jobs

Implement the `IJob` interface, then either configure it on the admin console's "Scheduled Jobs" page or annotate with the `[JobDetail]` attribute:

```csharp
[JobDetail("job_my_task", Description = "My scheduled job")]
public class MyTask : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken ct)
    {
        // Your job logic
    }
}
```

Jobs support three types: built-in, script compilation, and HTTP invocation. They are persisted to the database via `DbJobPersistence`, so under multi-instance deployment they coordinate properly — no two instances run the same job simultaneously.

> Note: the process lock around order fulfillment fails under multiple instances; this is still under evaluation — don't rely on it for cross-instance mutual exclusion for now.

### Middleware Pipeline: How a Request Is Processed Layer by Layer

Admin API's request processing order looks like this (outside in):

```
Exception handling / HSTS
  → Unified error status codes
  → Localization
  → Static files
  → Routing
  → CORS
  → IP / client rate limiting
  → Authentication / authorization
  → Schedule UI
  → Knife4j (Swagger)
  → Project DI
  → SignalR Hub + MVC routing
```

Hub API's pipeline is different; see [Hub API: The Third Door](#hub-api-the-third-door). When debugging request issues, walk the pipeline order of the relevant host and think "which layer blocked it" — far more effective than blindly restarting.

### C# Code Style

This project targets **C# 10 syntax** on the **.NET 10 runtime** (`Directory.Build.props`: `LangVersion=10` + `net10.0`). You can use records, target-typed new, file-scoped namespaces, and global usings. But there is one iron rule: **every control-flow statement must use braces**, even if the body is a single line. Also skip single-line `using var x = ...;` declarations — write `using (var x = ...) { }` properly.

---

## FAQ and Pitfalls

### FAQ

**I wrote a Service but the endpoint isn't reachable?**
Check three things: does the class implement `IDynamicApiController`, is the method public, is it tagged `[NonAction]`. If still stuck, check the startup logs — route registration prints them. (This only applies to Admin API — Hub API uses hand-written Controllers; there's no such thing as dynamic routing there.)

**The URL the frontend calls doesn't match my class name?**
You probably forgot the "strip the Service suffix + lowercase first letter" rule (Admin API). The URL of `SystemUserService` is `/api/systemUser/...`, not `/api/systemUserService/...`. Frontend URLs must never contain `Service`. Hub API doesn't follow this — its URLs are built from the Controller name + Action name.

**Changed the entity but the table schema didn't update?**
Code-First by default only creates tables and adds columns. Destructive changes like dropping columns or changing column types may need manual handling or a careful restart. Back up before altering production tables.

**SuperAdmin sees more data than everyone else — is that a bug?**
No. SuperAdmin bypasses all tenant filters and soft-delete filters, so it sees everything. That's by design. **But Hub API rejects SuperAdmin access** — Hub only serves the tenant domain.

**Changed seed data but it's still old after restart?**
Seeds upsert by primary key. If the key didn't change, existing rows aren't overwritten. Delete the row and restart, or use a different key.

**Hub API returns 401, but the same token works against Admin API?**
Hub reuses Admin's JWT key but reads the `JWTSettings` node from its own `appsettings.json`. The key settings (`IssuerSigningKey` / `ValidIssuer` / `ValidAudience`) must match on both sides. If they don't, the tokens each side issues aren't accepted by the other.

**Build reporting (PackageBuildReport) returns 401?**
That's not JWT auth — it's machine credentials (AccessKey + signature + scope). It goes through `TenantGameAppOpenAccessAuthMiddleware`; the credential must be created in the Admin console's open-access management first and bound to the corresponding App.

### Pitfalls

**Multi-database routing not taking effect**
Check that the attributes on the entity are correct: `[SystemTable]` / `[LogTable]` / `[Tenant]`. Unmarked entities route by the TenantId in the JWT Claim by default; without a login context they route to the main database.

**Audit fields are empty**
Auditing relies on SqlSugar AOP auto-fill. If you bypass the repository and operate `SqlSugarClient` directly, the AOP may not run and the fields stay empty. Use `SqlSugarRepository<T>` properly. (Hub API uses repositories directly — that's fine.)

**JWT keeps expiring / renewal not working**
Check whether the response carries the `X-Authorization` header. If not, inspect `JwtHandler`'s renewal logic and the token lifetime configuration. The platform JWT config has moved into `platform_jwt_settings` in main-database `SystemConfig` — stop editing `JWT.json`.

**Added a Hub endpoint and the validation logic disagrees with Admin's**
Normal. Hub doesn't reuse Admin's Application-layer validation. Either hand-write it in the Hub Controller or accept that both sides keep their own copy of the validation — that's Hub's current explicit trade-off.

**Scheduled jobs running twice under multiple instances**
Jobs rely on the database + cluster coordination. Confirm `DbJobPersistence` works and the cluster coordination config isn't missing. The process lock around order fulfillment genuinely fails under multiple instances — a known pitfall.

---

## Deployment

### Docker Compose (Recommended)

```bash
cd .. && docker compose up --build   # Unified orchestration lives at the repository root (5 services including the frontend)
```

One command brings up all three hosts. Port mappings are **identical** to local `dotnet run`:

| Compose Service | Container Name | Port | Host | Dockerfile |
|--------------|--------|------|----------|-----------|
| `web-api` | `gameframex-web-api` | `20802:20802` | Admin API | `Dockerfile.web-api` |
| `client-api` | `gameframex-client-api` | `20808:20808` | Client API | `Dockerfile.client-api` |
| `hub-api` | `gameframex-hub-api` | `20806:20806` | Hub API | `Dockerfile.hub-api` |

All three Dockerfiles are multi-stage builds (`sdk:10.0` for packaging → `aspnet:10.0` for running), run as a non-root user, with the timezone set to `Asia/Shanghai`. Config files are mounted as volumes, so **you can still change the config after the image is built** — no rebuild needed. The three containers share the host's PostgreSQL (via `host.docker.internal:5432`); no separate database container is started.

> Swagger is not mounted by default in Production containers; all three hosts must opt in explicitly with `Swagger__Enabled=true` (see the compose environment variable section).

> Naming history: the compose service was once named `web-entry` (carried over from the old "Web.Entry"); renamed to `web-api` as of 2026-10-20 to align with GameFrameX.Web.Api.

### Pre-Release Checklist: Database Re-Seeding and Compact Short IDs

The compact short ID layout ships with a **resettable, no-migration-script policy** (decision 2026-09-15): old and new ID layouts are incompatible; production PG main / tenant databases ship by "wipe and re-seed", and mixing old and new IDs is not allowed. Go through this before release:

1. **Wipe and re-seed**: empty the production PG main database and all tenant databases; each host rebuilds on startup (CodeFirst table creation + seed re-insertion). Do not migrate data with 18-19 digit long Ids from old databases into the new layout — overlapping old/new Id spaces is a data-integrity incident.
2. **WorkerId mutual-exclusion check**: all instances writing to the same database (Web.Api replicas / Hub / Client) must have pairwise-different `SnowFlake__WorkerId` values in 0-7. The defaults 1/2/3 only guarantee no collisions on a single-machine deployment; assign explicitly before scaling replicas.
3. **Magnitude spot-check**: after re-seeding, spot-check business-table primary keys — they should be 13-14 digit numbers (< 2^53). Seeing 18-19 digits means some path skipped Foundation's ID generation; stop and investigate.

---

## Exporting OpenAPI for the Frontend

The frontend's TypeScript API client is generated from `openapi.json` + `pnpm gen:api`. **The backend's only job is to expose the Swagger JSON endpoint**; the export step happens in the frontend repository.

| Item | Value | Source |
|---|---|---|
| Admin Swagger document name | `web.api` | `GameFrameX.Web.Core/ApiRouting/AdminOpenApiGroups.cs` |
| Admin Swagger JSON | `http://<host>:20802/swagger/web.api/swagger.json` | `AdminOpenApiGroups.WebGroup.RouteTemplate` |
| Client API Swagger JSON | `http://<host>:20808/swagger/client.api/swagger.json` | `GameFrameX.Client.Api/Program.cs` |
| Hub API Swagger JSON | `http://<host>:20806/swagger/v1/swagger.json` | `GameFrameX.Hub.Api/Program.cs` (`SwaggerDoc("v1", ...)`) |
| **Default switch** (same for all three) | On automatically in Development; elsewhere requires `Swagger:Enabled=true` (env var `Swagger__Enabled=true`) | Each `Program.cs` / `Startup.cs` |
| **Not exposed in Production / Docker by default** | Yes — the Swagger middleware isn't mounted at all; there is no `/swagger/...` endpoint | Same as above |

> Hub API's Swagger document name is `v1` (not `hub.api`), hence the URL `/swagger/v1/swagger.json`. The Admin frontend generally only uses Admin API's `web.api`; Hub and Client swaggers are mainly for game-server / CI integration lookups.

Not mounting Swagger in production by default is deliberate — don't leave your API docs naked on the public internet.

```bash
# 1. Make Admin API's Swagger reachable (pick one)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api &
# or
Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api &

# 2. Pull it into the frontend repository root
curl -sSL http://localhost:20802/swagger/web.api/swagger.json \
  -o ../GameFrameX.Admin.Unified.Vue/openapi.json
ls -lh ../GameFrameX.Admin.Unified.Vue/openapi.json   # should be roughly 3 MB

# 3. Run generation in the frontend
( cd ../GameFrameX.Admin.Unified.Vue && pnpm gen:api && git diff src/api/ )
```

> Whenever the backend OpenAPI changes (added / modified / removed service endpoints), you must re-export and have the frontend run `pnpm gen:api`, otherwise frontend call sites drift from the backend. The `check:routes` step in the frontend's `pnpm run quality` aligns frontend component paths against backend menu seeds and fails on mismatch.

---

## Command Cheatsheet

| Command | What It Does |
|------|--------|
| `dotnet run --project GameFrameX.Web.Api` | Start Admin API (20802) |
| `dotnet run --project GameFrameX.Client.Api` | Start Client API (20808) |
| `dotnet run --project GameFrameX.Hub.Api` | Start Hub API (20806) |
| `dotnet build` | Build the entire solution |
| `Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api` | Start in a non-dev environment with Swagger exposed |
| `ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api` | Start in dev environment (Swagger on automatically) |
| `cd .. && docker compose up --build` | Bring everything up with Docker (root-level orchestration: PG + three APIs + frontend) |
| `curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o ../GameFrameX.Admin.Unified.Vue/openapi.json` | Export Admin Swagger JSON for the frontend |

Common addresses:

| Address | What It Is |
|------|------|
| `http://localhost:20802/kapi` | Admin API's Knife4j / Swagger UI |
| `http://localhost:20802/swagger/web.api/swagger.json` | Admin Swagger JSON (docName=`web.api`) |
| `http://localhost:20806/swagger` | Hub API's Swagger UI |
| `http://localhost:20806/swagger/v1/swagger.json` | Hub Swagger JSON (docName=`v1`) |
| `http://localhost:20806/health` | Hub API health check |
| `http://localhost:20808` | Client API |
| `/hubs/onlineUser` | Admin API's SignalR online-user Hub |

---

## Final Words

The mantra:

> **Three hosts, each on its own: in Admin, writing a Service is writing the API; Client handles login and payment; Hub is the outward-facing traditional Controller. Writing an entity creates the table, URLs never carry Service, multi-tenancy has your back automatically. Hub doesn't reuse Admin's layers — it writes its own validation. And when the OpenAPI changes, don't forget to tell the frontend to regenerate.**

When stuck, don't grind forever: check the pitfalls above first, then dig through `gfx-doc/` (the documentation system at the project root), then ask a colleague.

To read concrete code, jump to these places:

- `GameFrameX.Web.Core/Startup.cs` —— Admin startup configuration, DI registration, middleware pipeline
- `GameFrameX.Hub.Api/Program.cs` —— Hub startup configuration, middleware pipeline, JWT reuse
- `GameFrameX.Hub.Api/Controllers/PackageBuildReportController.cs` —— the canonical example of Hub's "repository directly, no Application Service" style
- `GameFrameX.Core.SqlSugar/` —— SqlSugar configuration, AOP, multi-database routing
- `GameFrameX.Core/` —— repository base classes, `GenericCrudService`, auth, SignalR
- `GameFrameX.Core.Extension/RepositoryExtension.cs` —— repository extension methods (common wheels)
- `GameFrameX.Application/SeedData/` —— seed data
- `GameFrameX.Application/Configuration/` —— standalone JSON config files

## Documentation & Resources

- Project documentation site: <https://gameframex.doc.alianblank.com>
- Project overview: [`../README.md`](../README.md)
- Frontend operations manual: [`../GameFrameX.Admin.Unified.Vue/README.md`](../GameFrameX.Admin.Unified.Vue/README.md)
- Issue feedback: <https://github.com/GameFrameX/Admin/issues>

## Community & Support

[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)

## Changelog

See [GitHub Releases](https://github.com/GameFrameX/Admin/releases) for release notes.

## License

See [LICENSE.md](LICENSE.md) for license information.

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
