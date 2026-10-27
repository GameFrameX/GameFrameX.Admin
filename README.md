<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
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

## Project Overview

> One-line positioning: **an operations + publishing backend platform for indie game developers**. Backend `.NET 10 + SqlSugar` with three hosts (Admin / Client / Hub), frontend `Vue 3 + PrimeVue` — a full-stack pipeline from admin login to player payment callbacks, ready out of the box.

**GameFrameX Admin** is a **game-operations-oriented** backend system: the Admin API lets operations / GM / customer service staff edit menus, query orders, push versions, and view logs; the Client API handles game-side login / payments / config fetching; the Hub API serves external systems (CI builds, Tapd WebHooks, third-party callbacks). One repository, three hosts, deployed on separate databases so they never drag each other down.

It is **not** a generic OA / CRM / e-commerce system. Menus, permissions, dictionaries, forms, and client configs are all designed around real "game backend" business scenarios.

This project is built on secondary development of [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET); all Furion framework leftovers and original Admin.NET business naming have been replaced and rebaselined (U5 / U8 / C37–C41). Credits to the original author [zuohuaijun](https://gitee.com/zuohuaijun) and contributors.

### Repository Structure

Single repository (monorepo): frontend and backend live in one codebase. Since 2026-10-19, the historical sub-repositories `GameFrameX.Admin.Unified.Api` / `GameFrameX.Admin.Unified.Vue` have been frozen read-only and merged into this repository as snapshots; all new code lands here.

```
Admin/
├── GameFrameX.Admin.Unified.Api/    # .NET backend: Admin / Client / Hub hosts + Application / Entity / Core layers
├── GameFrameX.Admin.Unified.Vue/    # Vue frontend: PrimeVue + Pinia + vue-i18n (5 languages)
├── build/                           # Nginx config + deploy scripts (deploy-*.sh / docker-build-push.sh / entrypoint.sh)
├── doc/                             # README images
├── .github/workflows/               # CI: three API images + frontend image builds, frontend tests, external image sync
├── docker-compose.yml               # One-command full-stack orchestration (PostgreSQL + 3 APIs + frontend)
├── .env.example                     # Compose environment variable template (copy to .env and override as needed)
├── AGENTS.md / CLAUDE.md            # AI agent entry points (root instructions + readme_i18n config)
└── LICENSE.md                       # License
```

## Quick Start

> Suppose you want to set up an operations backend for a **new game**. After reading this section you should be able to go from zero to logging in and seeing the menu tree with the TeamS admin account.

### Prerequisites

- .NET 10 SDK
- Node.js ≥ 20 (20 recommended) + pnpm ≥ 11.5.2 (`corepack enable`)
- No database required — running from local source defaults to zero-config SQLite; production uses PostgreSQL / MySQL and 34 DbTypes in total

### Steps

```bash
# 1. Get the code (single repository, frontend and backend included)
git clone https://github.com/GameFrameX/Admin.git
cd Admin

# 2. Start the backend (from the repository root! Connection strings resolve relative to the working directory)
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api     # Admin API → http://localhost:20802
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Client.Api  # Client API → http://localhost:20808
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Hub.Api     # Hub API → http://localhost:20806

# 3. Start the frontend
cd GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # Defaults to http://localhost:20800 (hash routing)
```

Open your browser → you should see the login page → account `admin` / password `123456` / tenant `1300000000001` (on first startup, SqlSugar Code-First automatically creates the tables and seeds the data).

### Full Stack with One Command (unified compose at the repository root)

```bash
docker compose up -d --build
```

Compose ships PostgreSQL + the three hosts (20802/20806/20808) + the Admin frontend (20800, mutually exclusive with `pnpm run dev` — stop the dev server before starting the containers); once the healthchecks turn green you are good to go.

Environment variable entry point: copy the root [`.env.example`](.env.example) to `.env` and override as needed (unset variables fall back to the compose defaults; `.env` is not committed to git).

For finer-grained commands and pitfalls, see [`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md) (backend manual) and [`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md) (frontend manual).

## Who It's For / Not For

| Good fit | Not a good fit |
|---|---|
| Indie / small-team games that need a working operations backend | You already have a complete backend and only want one module |
| Multi-end (admin / client / external systems) scenarios needing clear boundaries | Simple single-end backends (Admin.NET / vue-next-admin directly is lighter) |
| Multi-tenant SaaS-style game services | Single-tenant in-house products with no plans to open up |
| Teams that require the .NET + SqlSugar stack | Teams only familiar with Java / Go / Node |
| Need multi-language (5-language i18n) + AI-Agent-friendly repository | — |
| Want a monolith starting point that can evolve toward microservices | — |

## Tech Stack

- **Backend**: .NET 10 + ASP.NET Core native hosting + SqlSugar (Code-First, 34 DbTypes) + JWT/HMAC + SignalR + Serilog.
- **Frontend**: Vue 3.5 + Vite 6 + TypeScript strict + PrimeVue 4.5 + Pinia 3 + vue-i18n (5 languages) + Axios.

For the full technology list and version matrix, see [Backend README · Tech Stack](GameFrameX.Admin.Unified.Api/README.md#tech-stack) and [Frontend README · Tech Stack](GameFrameX.Admin.Unified.Vue/README.md#tech-stack).

## Three Hosts: Admin / Client / Hub

One repository, three hosts, deployed on separate databases so they never drag each other down: **Admin API** (20802, admin console), **Client API** (20808, game client login/payment/config), **Hub API** (20806, external systems / CI / WebHooks). All three share Entities and SqlSugar repositories; dependencies only point downward (`Application.*` / `Core.*` must never reference hosts back).

### The Biggest Differences Between the Three Hosts

| Dimension | Admin API | Client API | Hub API |
|---|---|---|---|
| Routing | Dynamic API (write a Service, get an endpoint automatically) | Dynamic API | Traditional Controller (`[Route("api/[controller]/[action]")]`) |
| Data access | Via Application Service → repository | Via Application Service → repository | Controller injects `Repository<T>` directly |
| Reuses Application layer | Yes | Yes | No (validates on its own) |
| Adding a new endpoint | Write a Service | Write a Service | Create a Controller + inject repositories |
| URL rules | Drop the `Service` suffix + camelCase | Drop the `Service` suffix + camelCase | `Controller` name + `Action` name |
| Rejects SuperAdmin | No (SuperAdmin bypasses all filters) | No | **Yes** — Hub rejects super-admin access |

> **The `Service` suffix must NEVER appear in URLs.** This is a trap 100% of newcomers hit — the frontend generates calls based on this convention; change the class name and the frontend breaks everywhere.

## Multi-Tenancy + App Scopes

A three-layer model, from the outside in:

```
SuperAdmin (platform level) ── can access global config, cross-tenant queries
    ↓
Tenant (multi-tenant SaaS) ── shared across apps
    ↓
App (a specific game / product under a tenant) ── AppId scope
```

| Scope | Meaning | Route examples |
|---|---|---|
| `platform` | Platform level (visible to super admins, cross-tenant) | `/platform/*` |
| `tenant` | Tenant level (shared by all apps, no appId query) | `/dashboard/*`, `/system/*` |
| `app` | Inside an app workspace (must carry the `appId` query) | `/gameCenter/*` |

Multi-tenancy and database boundaries take effect automatically via entity attributes: `[Tenant]` automatically adds `TenantId` filtering + soft delete; `[SystemTable]` routes to the main database, `[LogTable]` routes to the log database, and unmarked entities route by the JWT `TenantId`. For details see [Backend README · Repositories and Multi-Database Routing](GameFrameX.Admin.Unified.Api/README.md#repositories-and-multi-database-routing).

## 30 Feature Modules

Grouped by "the scenarios you'll actually use", not by directory structure.

**Identity & Permissions**

| Module | What it does |
|---|---|
| User management | Tenant user / system admin maintenance; can bind user positions, orgs, roles, data permissions |
| Org management | Multi-level tree-shaped organization structure |
| Position management | User job title tags |
| Menu management | Three-level permissions: directory / menu / button |
| Role management | Roles bound to menus + data authorization scope |
| Dictionary management | Global / tenant two-level dictionaries |
| Open authorization | AccessKey / Secret + HMAC signature authentication (external APIs) |

**Platform Configuration (visible to SuperAdmin only)**

| Module | What it does |
|---|---|
| Platform tenant management | Tenant CRUD, automatic tenant database provisioning / disabling / expiry |
| Global platform / language / channel definitions | Platform-level enums and multilingual baselines |
| Region management | 5-level administrative division tree, supports syncing from the statistics bureau |
| System config | Key-value pairs; high-frequency configs migrated into the main database `SystemConfig` (JWT / password policy / captcha) |
| Database management | Table / column CRUD, online schema management |

**System Capabilities**

| Module | What it does |
|---|---|
| Service monitoring | Host info, CPU / memory / disk / loaded assemblies |
| Online users | Real-time list + forced sign-out, based on SignalR |
| Announcement management | System notifications + real-time SignalR push |
| File management | Local storage + Aliyun OSS + Tencent COS (multi-provider) |
| Scheduled jobs | `ProjectScheduler` project-owned scheduler; built-in / script-compiled / HTTP job types; runtime-compiled C# dynamic APIs |
| Email / SMS | `Mail.Abstract` multi-provider + `Sms.Abstract` Aliyun implementation |
| System APIs | Swashbuckle Swagger; not mounted in production by default (requires `Swagger__Enabled=true`) |
| Four log types | Operation / exception / access / data-change (field-level diff audit), with Excel export |
| Runtime plugins | C# code dynamically compiled and registered as API endpoints |
| Cache management | Unified Memory / Redis abstraction |

**Game Backend (76 Application.Game services)**

| Module | What it does |
|---|---|
| Game accounts / characters / servers | Player accounts, character management, Docker container lifecycle |
| Orders / delivery | Multi-dimensional filtering; calls the game-server API over HTTP to deliver items |
| Mail / redemption codes | Server-wide / single-server mail; redemption code channels and distribution |
| Web shop | Products + orders + categories |
| Items / goods | Item catalog, consumable / permanent item management |
| Announcements / banners | In-game + post-login announcements |
| App versions / resource packs | App versions + force update + resource pack versions and history rollback |
| App-scoped config | Remote parameters, app validation, client KV |
| Mail operations | Templates + campaigns + multilingual |
| Ad callbacks | TopOn / AdMob / UnityAds multi-provider |
| Game server integration | Player registration / login / order callbacks |

**Integrations**

| Integration | Notes |
|---|---|
| WeChat | `Wechat.Abstract` + `Wechat.Open` dual SDK; Official Account OAuth2, Mini Program login, template messages, JS-SDK |
| WeChat Pay | JSAPI orders (direct / ISV) + payment callbacks (`Payment.Wechat`) |
| Payments | Alipay / Apple IAP (incl. App Store Server API v2) / Google Play / Xsolla, abstraction layer + multi-provider |
| Object storage | Local / Tencent COS (multi-provider) |
| SMS | Aliyun SMS provider |
| Email | Tencent SES / SMTP (multi-provider) |

**Frontend Framework Features**

- **Backend-driven menus**: menus, permissions, and copy are all controlled by the backend. The frontend doesn't define menus — add them in the backend menu management; the frontend only renders the menu tree returned by the backend.
- **Automatic JWT renewal**: before the token from login expires, the frontend automatically adds `X-Authorization` so the backend silently reissues it — one login lasts a long time.
- **App scopes**: routes tagged with `meta.appScope` + `meta.menuContext` for the platform / tenant / app scopes; the frontend decides menu visibility based on the scope.
- **i18n completeness gate**: `pnpm run lint:i18n` checks that all 5 languages have complete keys; one missing key fails immediately.

## Security Baseline

Not "recommendations" — hard constraints that **take effect by default**.

| Threat | Mitigation |
|---|---|
| Weak passwords | Project-wide **Argon2id** (C214 system users / C215 players); legacy PBKDF2 kept only for `AppSecretHasher` (high-entropy random key scenarios where Argon2id isn't needed) |
| SQL injection | Fully parameterized SqlSugar, no string concatenation |
| Unauthorized access | Multi-tenant filtering + soft-delete filtering; SuperAdmin bypasses them, but Hub API rejects super admins |
| Replay attacks | HMAC signatures + nonce cache + 5-minute clock skew tolerance |
| Information leakage | Response DTOs must include Id/RowVersion/CreatedTime; sensitive fields (password hashes / AccessSecret) are **forbidden** in any Response; guarded by contract tests |
| API regression | `ThreeHostApiRoutingContractTests` + `AdminEndpointReturnContractTests` + `AdminActionStyleContractTests` + `NamingAbbreviationGuardTests` prevent regressions |

## Database

**Zero-config by default**: running from local source defaults to SQLite (under `db/` at the repository root); production uses PostgreSQL (MySQL / Oracle and 34 DbTypes in total are also supported). `GameFrameX.Application/Configuration/Database.json` is the single source of connection strings (shared by the three hosts — change it once and all three pick it up).

- Main / tenant / log databases are routed automatically by entity attributes (see the multi-tenancy section above).
- Tenant databases are created automatically: named `gameframex_admin_tenant_<TenantId>` by tenant ID, no manual connection strings needed; the default tenant database is likewise derived automatically.
- For connection-string syntax when switching databases, see [`Database-ConnectionStrings.md`](GameFrameX.Admin.Unified.Api/GameFrameX.Application/Configuration/Database-ConnectionStrings.md); for paired environment variable overrides (`GFX_MAIN_DB_TYPE` + `GFX_MAIN_DB_CONNECTION_STRING`) and other deployment details, see [Backend README · Database](GameFrameX.Admin.Unified.Api/README.md#database-zero-config-by-default-postgresql-for-production).

> SQLite is for local development only. Forbidden in production — weak concurrent writes, no standalone service process, and insufficient backup/restore and ops monitoring capabilities.

## Demo & Getting Started

| Resource | Link |
|---|---|
| Demo site (**do not use it for your own production**) | <https://game.admin.web.vue.alianblank.com> |
| Documentation site | <https://gameframex.doc.alianblank.com> |
| Backend operations manual | [`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md) |
| Frontend operations manual | [`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md) |

## Project Repositories

| Project | Purpose | Repository |
|---|---|---|
| This repository | Single repository for the Admin / Client / Hub hosts (Cron-based) | [GameFrameX/Admin](https://github.com/GameFrameX/Admin) |
| Client API (separate repository) | Early split; now unified into this repository | [GameFrameX.Admin.Client.Api](https://github.com/GameFrameX/GameFrameX.Admin.Client.Api) |
| Admin API (paid source) | Historical naming | [GameFrameX.Admin.Web.Api](https://github.com/GameFrameX/GameFrameX.Admin.Web.Api) |
| Admin Web frontend (paid source) | Historical naming | [GameFrameX.Admin.Web.Vue](https://github.com/AlianBlank/GameFrameX.Admin.Web.Vue) |

## Release & Deployment

```bash
docker compose up -d --build
# Ships PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + frontend(20800); wait for healthchecks to turn green (the frontend container and the dev server both grab port 20800 — they are mutually exclusive)
```

For single-container `docker run`, paired environment variable overrides (a half-paired override fails fast at startup), and other details, see [Backend README · Deployment](GameFrameX.Admin.Unified.Api/README.md#deployment).

## Development & Contributing

Want to add a new CRUD service? See [`GameFrameX.Admin.Unified.Api/README.md` → Add a New CRUD Service](GameFrameX.Admin.Unified.Api/README.md). In the Admin API, simply implement `IDynamicApiController` and you get REST endpoints automatically (Page / Add / Update / Get / List / Delete — six in total), with no Controller to write.

CI gates (anti-regression):

- `ThreeHostApiRoutingContractTests` — three-host routing / verbs / parameter binding / error-code semantics
- `AdminEndpointReturnContractTests` — Response DTO contracts
- `AdminActionStyleContractTests` — Admin action style conventions
- `NamingAbbreviationGuardTests` + `NamingAbbreviationBaseline.baseline.txt` — business abbreviations banned system-wide

## Feature Preview

### Platform Management (SuperAdmin)

![Login page](doc/img/1.png)
![Dashboard](doc/img/2.png)
![Menu management](doc/img/3.png)
![Config parameters](doc/img/4.png)
![Dictionary management](doc/img/5.png)
![Job scheduling](doc/img/6.png)
![Service monitoring](doc/img/7.png)
![Cache management](doc/img/8.png)
![Administrative divisions](doc/img/9.png)
![Dynamic plugins](doc/img/10.png)
![Open APIs](doc/img/11.png)
![Captcha config](doc/img/12.png)
![Tenant management](doc/img/13.png)
![Table management](doc/img/14.png)

### App Workspace Core Features (Chinese/English side by side)

The 5 core feature pages you see after entering a tenant app workspace. The same pages shown under Chinese and English UI languages (switch instantly via the language switcher in the top-right corner, no refresh needed):

| Feature | 简体中文 | English |
|------|----------|---------|
| Remote Config | ![Remote Config-CN](doc/img/15.png) | ![Remote Config-EN](doc/img/16.png) |
| Version Update | ![Version Update-CN](doc/img/17.png) | ![Version Update-EN](doc/img/18.png) |
| Package Management | ![Package Management-CN](doc/img/19.png) | ![Package Management-EN](doc/img/20.png) |
| Payment Config | ![Payment Config-CN](doc/img/21.png) | ![Payment Config-EN](doc/img/22.png) |
| Ad Callback Verification | ![Ad Callback-CN](doc/img/23.png) | ![Ad Callback-EN](doc/img/24.png) |

## Ecosystem Architecture

GameFrameX consists of the repositories below: the build-time toolchain generates protocol and config code, while game clients, game servers and the operations backend cooperate at runtime.

```mermaid
%%{init: {"flowchart": {"curve": "linear", "nodeSpacing": 30, "rankSpacing": 50}}}%%
flowchart TB
    subgraph Toolchain["Build-time toolchain"]
        direction TB
        Luban["luban<br/>Luban fork"] -- "Luban.dll · daily sync" --> Config["Config<br/>Excel tables"]
        Tools["Tools<br/>ProtoExport"] -- "ProtoExport.dll · weekly sync" --> Protobuf["Protobuf<br/>proto3 definitions"]
        FairyGUI["FairyGUIProject<br/>UI source"]
    end
    subgraph Runtime["Runtime"]
        direction TB
        subgraph Clients["Game clients"]
            direction TB
            Unity["Unity<br/>Unity 2019.4.40+ · C#"]
            Godot["Godot<br/>Godot 4.7 · C#"]
            Cocos["CocosCreator<br/>Cocos 3.8 · TS"]
            Laya["LayaBox<br/>LayaAir · TS"]
        end
        subgraph Backend["Backend"]
            direction TB
            Admin["Admin<br/>Admin · Client · Hub API"]
            subgraph GameServers["Game servers"]
                direction TB
                Server["Server<br/>NuGet framework"]
                ServerSource["Server.Source<br/>source framework"]
            end
        end
    end
    subgraph Infrastructure["Infrastructure"]
        direction TB
        subgraph Storage["Storage"]
            direction TB
            Mongo[("MongoDB<br/>game servers")]
            Postgres[("PostgreSQL<br/>Admin")]
        end
        subgraph Libraries["Base libraries · NuGet"]
            direction TB
            Foundation["Foundation<br/>.NET base libraries"]
            SuperSocket["SuperSocket<br/>socket framework"]
        end
    end
    Toolchain -- "generated code · config data · UI packages" --> Runtime
    Clients -- "TCP / WebSocket / HTTP" --> Backend
    Admin -- "HTTP management API" --> GameServers
    Runtime -- "data storage · NuGet packages" --> Infrastructure
```

### Core Projects

| Project | Role | Tech Stack | Integration |
|------|------|------|------|
| [Unity](https://github.com/GameFrameX/GameFrameX.Unity) | Unity client project: assembles 90+ framework packages from the GameFrameX UPM registry, HybridCLR hot update, login / bag / room samples | Unity 2019.4.40+, HybridCLR, YooAsset, FairyGUI, UniTask | Consumes Protobuf / Config / FairyGUIProject output; TCP 29100 + HTTP 28080 to Server, HTTP 20808 to Admin Client API |
| [Server](https://github.com/GameFrameX/GameFrameX.Server) | Actor-model game server: persistent Apps state layer strictly separated from the hot-updatable Hotfix logic layer; single- or multi-process deployment | .NET 10, MongoDB, Docker | Listens on TCP 29100 / WebSocket 29110 / HTTP 28080; HTTP management endpoints (mail, item grants, player management) for Admin |
| [Admin](https://github.com/GameFrameX/GameFrameX.Admin) | Multi-tenant operations backend: Admin (20802) / Client (20808) / Hub (20806) hosts + Vue console, covering game areas, announcements, mail, orders & payment, remote config, LiveOps | .NET 10, SqlSugar, PostgreSQL, Vue 3.5, PrimeVue | Calls Server through each game area's `HttpManageUrl`; Client API serves global info, login and payment callbacks to game clients |
| [luban](https://github.com/GameFrameX/luban) | GameFrameX fork of Luban: merged upstream v5.1.0, adds the `gameframex` table importer and `cs-dotnet-json` / `cs-dotnet-bin` code targets | .NET 10, Scriban | Publishes Luban tar.gz and Docker image; synced daily into Config `Tools/` |
| [Config](https://github.com/GameFrameX/GameFrameX.Config) | Config table repository: Excel data tables + schema + localization tables, one-click scripts generate client and server code and data | Excel, Luban | `gen-client-*` → Unity; `gen-server-json` → Server; `gen-server-bin` → Server.Source; `gen-godot-json` / `gen-cocos-json` / `gen-laya-json` → Godot / CocosCreator / LayaBox |
| [Protobuf](https://github.com/GameFrameX/GameFrameX.Protobuf) | Unified network protocol definitions: proto3, messages and error codes grouped by the numeric module ID in each file name (positive = client↔server, negative = server-internal) | proto3 | `Proto2CsExport_Server` → Server; `Proto2CsExport_Client` → Unity; `Proto2CsExport_Godot` / `Proto2TsExport_CocosCreator` / `Proto2TsExport_LayaBox` → other clients; CI publishes a six-language Release on every push |
| [Tools](https://github.com/GameFrameX/GameFrameX.Tools) | ProtoExport protocol code generator: C# / TypeScript / C++ / Lua / Go, available as CLI, Avalonia GUI and Docker image | .NET 10, Avalonia 12 | `ProtoExport.dll` synced weekly into Protobuf `Tools/`; Docker image `gameframex/gameframex-tools` |

### Peripheral Systems

| Project | Role | Tech Stack | Integration |
|------|------|------|------|
| [Foundation](https://github.com/GameFrameX/GameFrameX.Foundation) | Modular .NET libraries: localization, JSON, command-line options, logging, HTTP, encryption, hashing, ORM base entities, idempotency | .NET | NuGet packages referenced by Server, Server.Source, Admin, Tools |
| [SuperSocket](https://github.com/GameFrameX/GameFrameX.SuperSocket) | GameFrameX fork of SuperSocket: .NET 10 targets, dependency injection, KCP / ReliableSession for weak-network game traffic | .NET 10 | NuGet: Server (ClientEngine), Server.Source (Server / Kcp / Udp / WebSocket) |
| [Server.Source](https://github.com/GameFrameX/GameFrameX.Server.Source) | Server variant with the framework as in-repo source: service discovery, cross-process RemoteMessaging (TCP / KCP), multi-process deployment | .NET 10, MongoDB, Docker | Receives Config `gen-server-bin` output; multi-process via `docker-compose.multi.yml` |
| [Godot](https://github.com/GameFrameX/GameFrameX.Godot) | Godot .NET client with reusable runtime modules, editor plugins and hot-update assemblies | Godot 4.7, C# | Config `gen-godot-json`, Protobuf `Proto2CsExport_Godot`; TCP 29100 / WebSocket 29110 to Server |
| [CocosCreator](https://github.com/GameFrameX/GameFrameX.CocosCreator) | Cocos Creator client, migrated from the Unity version | Cocos Creator 3.8, TypeScript | `npm run gen:proto` / `gen:config` call Protobuf / Config scripts; WebSocket to Server |
| [LayaBox](https://github.com/GameFrameX/GameFrameX.LayaBox) | LayaBox client | LayaAir, TypeScript | Config `gen-laya-json`, Protobuf `Proto2TsExport_LayaBox`; WebSocket to Server |
| [FairyGUIProject](https://github.com/GameFrameX/GameFrameX.FairyGUIProject) | FairyGUI editor project: design source of all game UI | FairyGUI Editor | Publishes UI packages to Unity `Assets/Bundles/UI` and code to `Assets/Hotfix/UI/FairyGUI/` |

## Documentation & Resources

- Project documentation site: <https://gameframex.doc.alianblank.com>
- Backend operations manual (detailed): [`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md)
- Frontend operations manual (detailed): [`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md)
- Issue feedback: [GitHub Issues](https://github.com/GameFrameX/Admin/issues)
- Client / Admin / Web repositories: see [Project Repositories](#project-repositories)

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
