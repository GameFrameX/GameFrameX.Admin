<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Unified API

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Admin.Unified.Api)](https://github.com/GameFrameX/GameFrameX.Admin.Unified.Api/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

独立游戏前后端一体化解决方案的后端那半边 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

</div>

---

这是 **GameFrameX 游戏管理平台**的后端服务，用 .NET 10 + SqlSugar 写的。前端同学在浏览器里点的每一个按钮、游戏客户端发出的每一个请求，最后都是这里在接单。

> 配套前端在 [`GameFrameX.Admin.Unified.Vue`](../GameFrameX.Admin.Unified.Vue)，数据库本地默认 SQLite 零配置，生产用 PostgreSQL。本仓库只管"出接口、存数据、跑定时任务"这些事。

## 目录

- [这是干什么的](#这是干什么的)
- [技术栈](#技术栈)
- [三个主机，各管各的](#三个主机各管各的)
- [仓库目录结构](#仓库目录结构)
- [快速开始](#快速开始)
- [它是怎么跑起来的](#它是怎么跑起来的)
- [功能模块一览](#功能模块一览)
- [加一个新 CRUD 服务](#加一个新-crud-服务)
- [开发规范](#开发规范)
- [常见问题与踩坑](#常见问题与踩坑)
- [部署](#部署)
- [导出 OpenAPI 给前端](#导出-openapi-给前端)
- [命令速查](#命令速查)
- [写在最后](#写在最后)

---

## 这是干什么的

一句话：给游戏运营平台提供所有后端接口。运营加用户、改菜单、看订单、发邮件、推版本、看日志；游戏客户端登录、付钱、拿配置；CI 打包机回写版本——这些活儿的数据出口都在这里。

后端里住着**三个独立的主机**，各跑各的进程、各占各的端口，谁也不拖累谁：

- **Admin API**（`GameFrameX.Web.Api`，端口 `20802`）—— 给管理后台用的接口
- **Client API**（`GameFrameX.Client.Api`，端口 `20808`）—— 给游戏客户端用的接口（登录、支付、拿配置）
- **Hub API**（`GameFrameX.Hub.Api`，端口 `20806`）—— 给外部系统 / 游戏端 / CI 打包 / 第三方 WebHook 用的聚合接口

Client API 和 Hub API 都更"瘦"：Client API 只引用 Entity + Payment，不碰旧框架；Hub API 走传统 Controller 路由、直接用仓储，不走动态 API 那套（后面细讲）。

**最值得知道的三件事**：

1. **写个 Service 就自动有接口**（Admin API）。实现 `IDynamicApiController`，类里的 public 方法会自动变成 REST 端点，你不用手写 Controller、不用手贴 `[Route]`。这是 Admin API 的灵魂。
2. **写个实体就自动建表**。SqlSugar Code-First：启动时扫实体类 → 建表 → 填种子数据。你不用写一行建表 SQL，改了字段重启一下，表结构就跟上了。
3. **多租户是天生自带的**。实体打个标记，查询就自动加 `TenantId` 过滤、自动软删除，只有 SuperAdmin 能穿墙。你写业务的时候几乎感觉不到它的存在。

---

## 技术栈

| 类别 | 选型 | 小白注解 |
|------|------|----------|
| 运行时 | .NET 10 | 跑这套代码需要的发动机（`Directory.Build.props` 里 `net10.0`） |
| 语言版本 | C# 10 | `LangVersion=10`，可以用 record、file-scoped namespace 等 |
| 框架 | ASP.NET Core 原生宿主 | 没用额外框架，就靠原生那一套 |
| ORM | SqlSugar (Code-First) | 写 C# 实体类，它帮你生成数据库表 |
| 认证 | JWT Bearer + HMAC Signature | Admin/Client/Hub 共用同一份 JWT；开放接口另外用签名 |
| 实时通信 | SignalR (可选 Redis Backplane) | 服务器主动往浏览器推消息用的 |
| 缓存 | Memory / Redis | 本机内存够用，多机就上 Redis |
| 定时任务 | 项目自有 Scheduler | 任务存数据库，多实例能协调，不会重复跑 |
| 日志 | 数据库 + ElasticSearch + Serilog | 操作日志进库，大数据量走 ES |
| 数据库 | SQLite（本地默认）/ PostgreSQL（生产），也支持 MySQL / Oracle 等十余种 | 换库连接串写法见[速查文档](GameFrameX.Application/Configuration/Database-ConnectionStrings.md) |
| 容器化 | Docker Compose | 一条命令把三个主机都跑起来 |

---

## 三个主机，各管各的

```
Admin API（20802）              Client API（20808）            Hub API（20806）
    │                              │                              │
    │ 管理后台用                    │ 游戏客户端用                  │ 外部系统 / 游戏端 / CI 用
    │ 登录 / 用户 / 菜单 /          │ 登录 / 支付 / 拿配置          │ 区服 / 资源版本 / 道具 /
    │ 订单 / 日志 / 定时任务         │                              │ 兑换码 / 打包上报 / Tapd WebHook
    │                              │                              │
    └──────────── 共享 Entity ──────────────┬─────────────────────┘
                                           │
                                           ├── 共享 Payment（仅 Client 用）
                                           └── 三者各自直接用 SqlSugar 仓储
```

依赖关系长这样（左到右是引用方向）：

```
Web.Api → Web.Core → Application.{System, Game, Client, Tenant} → Core → Entity → Core.Enum
Client.Api（独立，仅引用 Entity + Payment）
Hub.Api（独立，引用 Application.Game + Core + Entity，但 Controller 不调 Application Service）
```

记住一条：**业务层不反向引用宿主**。`Application.*` 和 `Core.*` 永远不知道 `Web.Api` 的存在，依赖只能往下走。这样 Client API / Hub API 才能干净地把 Entity 单独拎出去用。

### Hub API 为什么和 Admin API 长得不一样

Admin API 是"动态 API"——写 Service 自动出接口；Hub API 是"传统 Controller"——`[Route("api/[controller]/[action]")]` 手写路由，Controller 里直接注入 `Repository<T>` 操作实体，**不调 Application Service**。这是 Hub.Api 刻意的约定：它面向外部系统，逻辑简单、要轻、要快，所以 Controller 自己查重、自己校验归属，不走 Admin 那套分层。

详细的差异见 [它是怎么跑起来的](#它是怎么跑起来的) 里的 [Hub API：第三张门](#hub-api第三张门)。

---

## 仓库目录结构

```
GameFrameX.Admin.Unified.Api/
├── GameFrameX.Web.Api/            # Admin API 宿主（启动入口，compose 服务名 web-entry）
├── GameFrameX.Client.Api/         # Client API 宿主（独立 WebApplication）
├── GameFrameX.Hub.Api/            # Hub API 宿主（独立 WebApplication，传统 Controller 路由）
├── GameFrameX.Web.Core/           # Admin 的启动逻辑、中间件、DI 配置（重点）
├── GameFrameX.Application/        # 应用层（配置文件、种子数据）
├── GameFrameX.Application.System/ # 系统管理服务（用户/角色/菜单/租户/日志...）
├── GameFrameX.Application.Game/   # 游戏管理服务（区服/订单/邮件/兑换码...）
├── GameFrameX.Application.Client/ # 客户端管理服务（版本/渠道/资源包...）
├── GameFrameX.Application.Tenant/ # 租户级服务（设备 / 设备分组）
├── GameFrameX.Entity/             # 实体层（Base / System / Game / Client）
├── GameFrameX.Core/               # 核心层（仓储、服务基类、认证、SignalR）
├── GameFrameX.Core.Enum/          # 枚举定义
├── GameFrameX.Core.Extension/     # 扩展方法与公共 DTO（重点，常来这儿找轮子）
├── GameFrameX.Core.SqlSugar/      # SqlSugar 配置、AOP、拦截器
├── GameFrameX.Core.Utility/       # 工具类
├── GameFrameX.Core.BaseInputOutput/ # 基础输入输出模型
├── Payment/                       # 支付集成（仅 Client API 引用）
│   ├── GameFrameX.Pay.Abstract/          # 支付抽象接口（先定义后实现）
│   ├── GameFrameX.Pay.AliPay/            # 支付宝
│   ├── GameFrameX.Pay.Apple/             # Apple IAP（内购验证）
│   ├── GameFrameX.Pay.Apple.AppStoreServer/  # App Store Server API v2
│   ├── GameFrameX.Pay.Google/            # Google Play
│   └── GameFrameX.Pay.Xsolla/            # Xsolla
├── Dockerfile.web-entry           # Admin API 的 Dockerfile（sdk:10.0 → aspnet:10.0）
├── Dockerfile.client-api          # Client API 的 Dockerfile
├── Dockerfile.hub-api             # Hub API 的 Dockerfile
└── docker-compose.yml             # 三个主机一把起
```

平时改代码，99% 时间在 `Application.*` 和 `Entity` 里转悠。`Core.*` 是地基，动它之前最好先看明白谁在依赖它。

---

## 快速开始

### 环境要求

- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code，挑个你顺手的
- 数据库不需要预装：本地源码直跑默认用 SQLite（文件落在仓库根 `db/` 目录）；要连 PostgreSQL 时再装，或直接用下面的 Docker Compose（内置 PG）

### 数据库：默认零配置，生产用 PostgreSQL

**本地源码直跑（默认 SQLite）**：什么都不用配。默认连接在 `GameFrameX.Application/Configuration/Database.json` 里，三个 host 共享仓库根 `db/` 目录下的同一份 SQLite 库文件（WAL 模式）。约定是**在仓库根目录执行 `dotnet run`**（连接串相对进程工作目录解析）。

⚠️ SQLite 只面向本地开发/调试。线上/生产环境禁止使用 SQLite（并发写入弱、无独立服务进程、备份恢复与运维监控能力不足），请用 PostgreSQL。

**生产 / 容器（PostgreSQL，环境变量成对覆盖）**：覆盖数据库时类型和连接串必须**成对**设置，半套覆盖会在启动期直接报错。三个 host（Web / Client / Hub）用**同一对变量**：

- `GFX_MAIN_DB_TYPE=PostgreSQL` + `GFX_MAIN_DB_CONNECTION_STRING=Host=...`
- 默认租户库不用配：从主库连接串自动推导（仅换库名，见下）
- 存量部署的裸 `DbType` / `ConnectionString` 启动参数仍可用（兼容层），启动会打迁移警告，请尽快切换
- 换其他数据库（MySQL / Oracle / 达梦……全部 34 种 DbType）的连接串写法与示例：[`GameFrameX.Application/Configuration/Database-ConnectionStrings.md`](GameFrameX.Application/Configuration/Database-ConnectionStrings.md)

关于数据库，还有几个约定你要知道：

- **租户库是自动建的**。创建普通租户时，系统会按租户 ID 自动生成 `gameframex_admin_tenant_<TenantId>` 数据库和对应的登录角色，**不需要 SuperAdmin 手动填租户库连接串**。
- **默认租户库也是自动推导的（C269）**。主库连接串 → 默认租户库连接串：PostgreSQL / MySQL 仅替换库名为 `gameframex_admin_tenant_1300000000001`（与普通租户库同规律，账号/密码继承主库，MySQL 建库强制 utf8mb4）；SQLite 落在主库同目录 `gameframex_admin_tenant_1300000000001.db`。仅支持 PostgreSQL / MySQL / SQLite，其他类型启动报错。显式配置优先：`Database.json` 加回 `ConfigId=1300000000001` 项或成对设置 `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING`。
- **既有部署升级（BREAKING）**：默认租户库名由 `gameframex_admin_tenant_default` 变为 `gameframex_admin_tenant_1300000000001`，二选一：a. 在线改名 `ALTER DATABASE gameframex_admin_tenant_default RENAME TO gameframex_admin_tenant_1300000000001`（注册表会自动跟上推导值）；b. 成对设置 `GFX_DEFAULT_TENANT_DB_*` 指向旧库名冻结不动。
- **Compose 内置 PostgreSQL**。`docker compose up` 会自带起一个 `postgres:16-alpine`（首次启动自动建库，见 `docker/postgres/init.sql`，数据落在 named volume）。要复用宿主机已有的 postgres 容器，在 `.env` 里设 `POSTGRES_HOST=host.docker.internal` 等变量并移除 api 服务对 `postgres` 的 `depends_on` 即可。
- **三个 host 共读同一份默认配置**。Client / Hub 启动时自动加载 `Database.json`（与 Admin 同源，改一处三处生效）；环境变量覆盖写法与 compose 一致。

### 启动方式一：Docker Compose 一键起（推荐，零配置）

```bash
docker compose up -d --build
# 内置 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806)，起完等 healthcheck 变绿
```

### 启动方式二：源码直跑（零配置，SQLite）

```bash
# 注意：在仓库根目录执行，四个 host 共享 ./db/ 下的 SQLite 库文件

# Admin API（管理后台）
dotnet run --project GameFrameX.Web.Api
# 监听 http://*:20802，Swagger UI: http://localhost:20802/kapi

# Client API（游戏客户端）
dotnet run --project GameFrameX.Client.Api
# 监听 http://*:20808

# Hub API（外部系统 / CI / WebHook）
dotnet run --project GameFrameX.Hub.Api
# 监听 http://localhost:20806，Swagger UI: http://localhost:20806/swagger

# Game API（游戏服务器侧）已并入 Client API（C268）：GameUser* 控制器随 Client.Api 暴露（http://*:20808）
```

第一次启动 Admin API 时，SqlSugar Code-First 会自动建库建表，顺手把种子数据（管理员、角色、菜单、字典这些）灌进去。所以你看到一个空数据库突然冒出一堆表，别慌，那是正常操作。

### 启动方式三：单容器 docker run

镜像内默认配置就是 SQLite（零环境变量可直接起，数据在容器内 `/app/db/`，挂 volume 可持久化）。生产用法是注入成对环境变量连 PostgreSQL：

```bash
docker run -p 20802:20802 \
  -e GFX_MAIN_DB_TYPE=PostgreSQL \
  -e GFX_MAIN_DB_CONNECTION_STRING="Host=你的PG;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  -e GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL \
  -e GFX_DEFAULT_TENANT_DB_CONNECTION_STRING="Host=你的PG;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=...;Password=..." \
  你的镜像
```

### 卡住了？

| 现象 | 大概率原因 |
|------|------------|
| 启动报数据库连接失败 | `Database.json`（或 `GFX_MAIN_DB_*` 环境变量）连接串不对，或者 PG 没起 |
| 启动报「DbType 为 Sqlite/PostgreSQL，但连接串不是 …风格」 | 环境变量半套覆盖了：`GFX_MAIN_DB_TYPE` 和 `GFX_MAIN_DB_CONNECTION_STRING` 必须成对设置 |
| 启动报「默认租户库不支持自动推导」 | 主库用了 PostgreSQL / MySQL / SQLite 之外的 DbType，需成对设置 `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING` 显式指定 |
| SQLite 库文件落错位置 | 连接串相对进程工作目录解析——回到仓库根目录执行 `dotnet run`；项目目录里跑出来的 `db/` 删掉即可 |
| 表没建出来 | Code-First 开关默认在代码中全开（C278）；若显式配置过关闭项，检查 `Database.json` 或环境变量 |
| Swagger 打不开 | 非开发环境默认不挂 Swagger，见下面的 [导出 OpenAPI](#导出-openapi-给前端) |
| 种子数据没灌进去 | 种子数据开关被关了；另外种子是按主键 Upsert 的，改了种子要删旧记录才会重灌 |
| Hub API 启动报"未配置 JWTSettings" | Hub 复用 Admin 的 JWT 配置，`Hub.Api/appsettings.json` 里要有和 Admin 一致的 `JWTSettings` 节点 |
| 端口被占 | 20802 / 20808 / 20806 / 20810 被别的进程占了，`lsof -i :20806` 查一下 |

---

## 它是怎么跑起来的

### 动态 API：写 Service 就是写接口（Admin API）

Admin API 的服务实现 `IDynamicApiController` 之后，框架会自动把类里的 public 方法变成 REST 端点（路由是 camelCase），你不用手写 Controller：

```csharp
public class MyService : ITransient, IDynamicApiController
{
    // 自动变成 GET /api/my/list
    public List<MyEntity> List() => ...;
}
```

路由生成规则（务必记住，前端 100% 会按这个调你）：

1. 拿类名，去掉 `Service` / `AppService` / `Controller` 后缀（只去一次）
2. 首字母小写
3. 加默认前缀 `api/`

| 类名 | 去后缀 | 最终路由前缀 |
|------|--------|-------------|
| `GameAppService` | `GameApp` | `/api/gameApp` |
| `SystemUserService` | `SystemUser` | `/api/systemUser` |
| `GameOrderService` | `GameOrder` | `/api/gameOrder` |

方法名同理拼到后面：`Page()` → `/api/gameApp/page`，`ResetSecret()` → `/api/gameApp/resetSecret`。

> **URL 里严禁出现 `Service` 后缀**。这不是建议，是硬规则——前端按这个约定生成的调用，你后端类名一变，前端全炸。

需要精细控制时，`[HttpPost]`、`[AllowAnonymous]`、`[NonAction]` 这些特性照常用。

### 依赖注入：打标记就注册

不用手写 `services.AddScoped<>()`，靠标记接口自动注册：

| 接口 | 生命周期 | 啥时候用 |
|------|----------|----------|
| `ITransient` | 瞬态 | 每次 resolve 都是新对象，大多数服务用这个 |
| `IScoped` | 作用域 | 一次请求内复用 |
| `ISingleton` | 单例 | 全局唯一，放有状态的东西要小心 |

基础设施组件（SqlSugar、JWT、SignalR、EventBus 这些）在 `GameFrameX.Web.Core/Startup.cs` 里手动注册，因为它们要配置参数。

### 仓储与多数据库路由

`SqlSugarRepository<T>` 继承自 SqlSugar 的 `SimpleClient<T>`，构造函数会根据实体上的特性决定把数据存到哪个库：

| 实体特性 | 路由目标 |
|----------|----------|
| `[SystemTable]` | 主数据库 |
| `[LogTable]` | 日志数据库 |
| `[Tenant]` | 租户独立数据库 |
| 无标记 | 按 JWT Claim 里的 TenantId 路由 |

你只管写实体、打标记，剩下路由的事仓储替你做了。

### 认证体系：JWT 共用，外加签名

**JWT Bearer（三个主机共用同一份密钥）**

- `JwtHandler` 基于 ASP.NET Core 授权处理器实现（Admin API）
- 三者读同一份 `JWTSettings`——所以 **Admin 登录签发的 token，Client API 和 Hub API 也直接认**
- 自动续签：Admin API 的响应头 `X-Authorization` 带新 Token，前端静默换发
- 权限校验：SuperAdmin 绕过所有检查；普通用户按路由名匹配按钮权限

**HMAC Signature（开放 API 用）**

- `SignatureAuthenticationHandler` 实现 HMAC-SHA256 签名认证
- 请求头要带四个：`accessKey` + `timestamp` + `nonce` + `sign`
- 验签公式：`HMACSHA256(secret, "METHOD&URL&accessKey&timestamp&nonce")`
- 防重放：nonce 缓存 + 时间偏差校验（默认允许 5 分钟偏差）

### 多租户与软删除：查询自动过滤

- `ITenantIdFilter`：查询自动附加 `TenantId` 过滤，租户之间数据天然隔离
- `ISafeDeletedFilter`：查询自动附加 `IsDeleted == false`，删除只是打个标记
- SuperAdmin 绕过所有过滤，所以超管能看见全部数据（**但 Hub API 会拒绝 SuperAdmin，见下**）

这两条是"你写业务几乎感觉不到、但它一直在帮你兜底"的那种机制。

### 审计字段：自动填充，不用手写

SqlSugar AOP 在 `SqlSugarSetup.SetDbAop()` 里帮你干了这事：

| 操作 | 自动填的字段 |
|------|-------------|
| Insert | `CreatedTime`、`CreatedId`、`CreatedName`、`TenantId` |
| Update | `UpdateTime`、`UpdatedId`、`UpdatedName` |

同时顺手把数据变更写进审计日志（`SystemLogDiff` / `TenantLogDiff`）。所以你建实体的时候，审计字段不用自己赋值，框架替你填。

### 雪花 ID

分布式唯一 ID 用 Foundation Snowflake 的**紧凑短 ID 布局**生成（2026-09-15 决策冻结，见 `gfx-doc/decisions/2026-09-15-compact-snowflake-id.md`）：

| 布局参数 | 值 | 说明 |
|---|---|---|
| epoch | 2026-09-01 | 部署纪元，有数据落库后不可改 |
| WorkerId | 3 位（0-7） | 唯一的实例区分维度 |
| DataCenterId | 0 位 | 单数据中心，维度已移除 |
| 序列 | 10 位 | 每 WorkerId 每毫秒 1024 个 |

产出 13-14 位数字（< 2^53，JS 里不丢精度），Admin 页面直接展示主键不用转字符串。布局已冻结：改 epoch 或位宽 = 与历史 Id 空间重叠，等于数据完整性事故。

多实例防撞**不靠 Redis**（进程内租约无法跨进程互斥，那套已退役），靠显式配置互斥：同库写入的每个实例（Web.Api 多副本 / Hub / Client）必须互斥配置 `SnowFlake:WorkerId`（环境变量 `SnowFlake__WorkerId`，0-7），越界启动期直接失败——单数据中心同库实例上限 8。默认值 Web.Api=1（appsettings）/ Hub=2 / Client=3（代码兜底），单机全默认部署互不冲突。

SqlSugar 的主键回填取号也统一走这套：`StaticConfig.CustomSnowFlakeFunc` 由 `SqlSugarSnowflakeFallbackInitializer`（ModuleInitializer）在 Core.Extension 模块加载时进程级安装，任何路径（包括 Hub / Client 和测试进程）都不会退回 SqlSugar 内置的 18-19 位长布局。

### 全局响应包装

Admin API 的 `AdminResultProvider` 把所有响应统一套了一层壳：

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

前端就认这个结构，`Code === 200` 放行，其它走错误处理。Hub API 用的是自己那套 `HttpJsonResultData<T>`（`Success` / `Fail` 工厂方法），结构类似但独立，不经过 `AdminResultProvider`。

### SignalR 实时通信

- Hub：`OnlineUserHub`，路径 `/hubs/onlineUser`
- 按租户分组（`GROUP_ONLINE_{tenantId}`），A 租户的消息不会飘到 B 租户
- 能干：在线用户列表、强制下线、消息发送（单播 / 广播 / 排除自己 / 指定用户）
- 多实例部署时可选 Redis Backplane + Data Protection Key 持久化，让消息跨实例推送

### Hub API：第三张门

Hub API（`GameFrameX.Hub.Api`，端口 `20806`）是给**外部系统**用的聚合接口，定位和 Admin API、Client API 都不同。它服务这几类调用方：

| 调用方 | 干啥 | 对应 Controller |
|--------|------|-----------------|
| 游戏端 / 运营工具 | 拉区服、查资源版本、查道具、核销兑换码 | `GameAreaController` / `GameAssetPackageVersionController` / `GameGoodsController` / `GameRedeemCodeController` |
| CI / 打包机 | 打包完成后回写资源版本 / 整包版本 | `PackageBuildReportController` |
| 第三方平台 | 接收 Tapd 的 WebHook 回调 | `TapdWebHookController` |

它有几个和别人不一样的地方，写代码前一定要知道：

**1. 传统 Controller 路由，不走动态 API。**
基类 `BaseController` 标了 `[Route("api/[controller]/[action]")]`，路由是手写的，不靠类名自动生成。所以这里的 URL **不会**自动去掉 `Service` 后缀——直接按 `Controller` 名 + `Action` 名拼。

**2. Controller 直接用 `Repository<T>`，不调 Application Service。**
这是 Hub.Api 的铁律。Controller 构造函数注入仓储，自己查、自己写、自己查重、自己校验归属。PackageBuildReportController 的注释里写得很直白："遵循 Hub.Api Controller 惯例（Repository 直接操作 Entity，不调 Application Service），故未提层 ScopeValidator / 未复用 Application.Client 的 CheckXxxUnique——Controller 自写归属与查重。"

> 含义：你给 Admin API 提的那些 Validator、Application 层的查重方法，**对 Hub.Api 是死代码**，别指望 Hub 能复用到。Hub 要校验就自己在 Controller 里写。

**3. 三种鉴权场景，各有各的走法。**

| 场景 | 怎么鉴权 | 租户怎么来 | 示例 |
|------|----------|-----------|------|
| 登录用户 | JWT（`BaseController` 默认 `[Authorize]`） | 从 JWT 解析 | 区服 / 资源 / 道具 / 兑换码查询 |
| 匿名 WebHook | `[AllowAnonymous]` | 从请求头来 | `TapdWebHookController` |
| 机器对机器 | `[AllowAnonymous]` + `TenantGameAppOpenAccessAuthMiddleware` | 凭证绑定的 App | `PackageBuildReportController`（打包上报） |

机器凭证那套校验的是 AccessKey + 签名 + scope，**不是 JWT**，所以 Controller 标 `[AllowAnonymous]` 跳过 JWT，由专用中间件接管。

**4. 租户靠中间件统一解析，且拒绝超级管理员。**
`HubTenantMiddleware`（除了 `/health` 都走）把租户 Id 统一写进 `HttpContext.Items`：鉴权请求从 JWT 取，匿名 WebHook 从请求头来。然后 `DelegateSqlSugarTenantIdResolver` 把这个 Id 喂给 SqlSugar 做租户路由。**SuperAdmin 会被直接拒绝**——Hub 是租户域的服务，平台级超管不该借 Hub 钻进某个租户的数据。

**5. 自带乐观锁中间件和健康检查。**
`OptimisticConcurrencyMiddleware` 在管道最前面，给需要并发控制的写操作兜底。健康检查端点有三个：`/health`、`/health/live`、`/health/ready`（compose 用它做存活探测）。

Hub API 的中间件管道顺序（`Program.cs`）：

```
OptimisticConcurrencyMiddleware
  → Swagger（开关控制）
  → CORS
  → Authentication → Authorization
  → HubTenantMiddleware（/health 除外；写租户 Id；拒绝超管）
  → TenantGameAppOpenAccessAuthMiddleware（仅 /api/PackageBuildReport）
  → HealthChecks / MapControllers
```

### 配置文件：独立 JSON，按需加载

Admin API 的独立 JSON 配置都在 `GameFrameX.Application/Configuration/`，启动时自动扫：

| 文件 | 管什么 |
|------|--------|
| `Database.json` | 数据库连接、Code-First 开关、种子数据开关 |
| `Swagger.json` | Swagger / Knife4j UI 配置 |
| `SMS.json` | 短信服务配置 |
| `Upload.json` | 文件上传、OSS 配置 |
| `Wechat.json` | 微信公众号 / 小程序配置 |
| `Enum.json` | 枚举扫描程序集 |

有两个"已经搬家"的配置要留意，别再到处找它们了：

> 平台 JWT 配置已经搬进主库 `SystemConfig` 的 `platform_jwt_settings`，**不再用 `JWT.json` 作为运行时来源**（Hub API 仍读自己 `appsettings.json` 里的 `JWTSettings` 节点，但密钥要和 Admin 一致）。
>
> `App.json` 已经退役：监听端口、动态 API、CORS、雪花 ID 这些主机和框架配置，搬进了 `Web.Entry/appsettings.json`；Cryptogram 密钥改成环境变量 `GFX_CRYPTOGRAM_*` + 内置默认（见 `CryptogramSecretResolver`）；密码强度策略搬进主库 `SystemConfig` 的 `platform_password_policy_config`；区服 Server HTTP 配置搬到了区服级 DB 配置。

---

## 功能模块一览

> 这里的服务主要跑在 Admin API 上（动态 API）。Hub API 的接口是独立的一套传统 Controller，见 [Hub API：第三张门](#hub-api第三张门)。

### 系统管理

基于项目自有动态 API 路由的完整后台管理模块。这些是"不管做什么游戏都得有"的那部分。

| 功能 | 服务类 | 干啥的 |
|------|--------|--------|
| 认证登录 | `SystemAuthService` | 账号密码、手机验证码登录，签发 JWT + 自动续签，验证码 |
| 用户管理 | `SystemUserService` | 用户增删改查、密码重置、用户-角色绑定（注意：这个服务实际管的是 `TenantUser`，命名有点误导） |
| 角色管理 | `TenantRoleService` | 角色 CRUD、数据范围控制、角色-菜单权限绑定 |
| 菜单权限 | `GlobalMenuService` | 树形菜单、按钮级权限解析、路由与 API 映射 |
| 组织机构 | `TenantOrgService` | 树形组织 CRUD、角色-组织数据范围 |
| 岗位管理 | `TenantPosService` | 岗位 CRUD，归属校验 |
| 租户管理 | `SystemTenantService` | 多租户 CRUD、独立数据库连接配置 |
| 字典管理 | `GlobalDictTypeService` / `GlobalDictDataService` | 字典类型与字典值 CRUD、状态切换 |
| 文件管理 | `TenantFileService` | 文件上传下载、OSS 对象存储 |
| 定时任务 | `SystemJobService` | 作业与触发器 CRUD、暂停 / 启动 / 立即执行、集群协调 |
| 数据库管理 | `SystemDatabaseService` | 表 / 列 CRUD、库表结构在线管理 |
| 通知公告 | `TenantNoticeService` | 公告发布、SignalR 广播、已读 / 未读跟踪 |
| 在线用户 | `TenantOnlineUserService` | 实时在线列表、强制下线、单点登录 |
| 日志系统 | `LogOperationService` / `LogExceptionService` / `LogVisitService` / `LogDiffService` | 操作日志、异常日志、访问日志、数据变更审计，支持 Excel 导出 |
| 缓存管理 | `SystemCacheService` | 统一缓存抽象（Memory / Redis），缓存键管理 |
| 系统配置 | `SystemConfigService` | 键值对配置 CRUD |
| 枚举服务 | `SystemEnumService` | 反射获取程序集里的枚举类型和值 |
| 常量服务 | `SystemConstService` | 获取 `[ConstAttribute]` 标注的常量值 |
| 短信服务 | `SystemSmsService` | 阿里云短信验证码，60 秒 TTL |
| 地区管理 | `SystemRegionService` | 行政区划 5 级树，支持从统计局同步 |
| 打印模板 | `TenantPrintService` | 打印模板 CRUD |
| 开放接口 | `SystemOpenAccessService` | AccessKey / Secret 管理、HMAC 签名认证 |
| 插件管理 | `SystemPluginService` | 运行时编译 C# 代码，动态注册成 API 端点 |
| 微信 | `SystemWechatService` / `SystemWxOpenService` | 公众号 OAuth2、小程序登录、模板消息、JS-SDK |
| 微信支付 | `SystemWechatPayService` | JSAPI 下单（直连 / 服务商）、支付回调 |
| 服务器监控 | `SystemServerService` | 主机信息、CPU / 内存、磁盘、已加载程序集 |

### 游戏管理

面向游戏运营的业务模块，所有服务继承 `GenericCrudService` 基类（写一个 service 自动有六个接口，后面会讲）。

| 功能 | 服务类 | 干啥的 |
|------|--------|--------|
| 游戏账号 | `GameUserEntityService` | 邮箱注册、封禁 / 解封 |
| 游戏角色 | `GameRoleEntityService` | 角色管理、沙盒模式切换、从游戏服同步数据 |
| 游戏区服 | `GameAreaEntityService` | 区服管理、Docker 容器生命周期（创建 / 启动 / 停止 / 重启） |
| 区服分类 | `GameAreaClassifyEntityService` | 区服集群分类 |
| 订单管理 | `GameOrderEntityService` | 支付订单、多维度筛选（状态 / 角色 / 区服 / 渠道 / 平台 / 日期） |
| 订单发货 | `GameOrderShipService` | 共享发货逻辑，HTTP 调游戏服 API 发道具 |
| 游戏邮件 | `GameMailEntityService` | 邮件发送、道具附件解析、收件人校验 |
| 兑换码 | `GameRedeemCodeEntityService` | 兑换码管理、道具附件解析 |
| 兑换码渠道 | `GameRedeemCodeChannelEntityService` | 兑换码投放渠道 |
| 兑换记录 | `GameRedeemCodeUseRecordEntityService` | 兑换码使用记录 |
| Web 商城 | `GameWebShopEntityService` | Web 商城商品管理 |
| 商城订单 | `GameWebShopOrderEntityService` | Web 商城订单 |
| 商品管理 | `GameCommodityEntityService` | 商品 / 产品管理 |
| 道具物品 | `GameGoodsEntityService` | 游戏内物品 / 道具 |
| 物品目录 | `GameGoodsCatalogueEntityService` | 物品分类 |
| 定时活动 | `GameTimeJobEntityService` | 游戏内定时任务 / 活动 |
| Docker 标签 | `GameDockerTagEntityService` | 游戏服 Docker 镜像版本管理 |
| 公告 | `GameAnnouncementEntityService` / `GameBannerAnnouncementEntityService` | 游戏公告、轮播公告 |
| 首页配置 | `GameHomeEntityService` | 游戏首页配置 |

### 应用中心

面向客户端 App 的配置管理模块。

| 功能 | 服务类 | 干啥的 |
|------|--------|--------|
| 应用版本 | `GameAppReleaseVersionService` | App 版本管理、强制更新控制 |
| 资源包 | `GameAppAssetPackageService` | AssetBundle / 资源包管理 |
| 资源版本 | `GameAppAssetPackageVersionService` | 资源包版本追踪 |
| 资源历史 | `GameAppAssetPackageVersionHistoryService` | 资源版本变更记录 |
| 渠道定义 | `GlobalChannelDefinitionService` / `TenantChannelDefinitionService` | 分发渠道（App Store、Google Play 等） |
| 设备管理 | `TenantDeviceService` | 设备注册与识别（位于 `Application.Tenant`） |
| 设备分组 | `TenantDeviceGroupService` | 设备分组（灰度 / A/B 测试，位于 `Application.Tenant`） |
| 多语言 | `GlobalLanguageService` | 支持语言配置 |
| 分包管理 | `GameAppPackageService` | 应用分包、渠道绑定与母包继承管理 |
| 平台管理 | `GlobalPlatformService` | 运行平台（iOS、Android 等） |
| 版本定义 | `GameAppVersionDefinitionService` | 版本资源映射 |
| 客户端配置 | `GameAppClientConfigService` | 客户端键值对配置 |
| 配置键 | `GameAppClientConfigKeyService` | 客户端配置内容键 |

### Hub API 的接口（独立一套）

这些是传统 Controller，跑在 Hub API（20806）上，**不走动态 API、不调 Application Service**：

| 接口 | Controller | 鉴权 | 干啥 |
|------|-----------|------|------|
| 区服查询 | `GameAreaController` | JWT | 给游戏端拉区服列表 |
| 资源包版本 | `GameAssetPackageVersionController` | JWT | 查资源版本、带乐观锁更新 |
| 道具查询 | `GameGoodsController` | JWT | 查游戏内道具 |
| 兑换码 | `GameRedeemCodeController` | JWT | 兑换码核销相关 |
| 打包上报 | `PackageBuildReportController` | 机器凭证 | CI / 打包机回写资源版本 / 整包版本（幂等 upsert） |
| Tapd WebHook | `TapdWebHookController` | 匿名（请求头取租户） | 接收 Tapd 回调 |

### 支付集成

只有 Client API 用。抽象层 + 多渠道实现，加新渠道照着抽象层实现一遍就行：

| 渠道 | 项目 |
|------|------|
| 支付宝 | `GameFrameX.Pay.AliPay` |
| Apple IAP | `GameFrameX.Pay.Apple`（内购收据验证） |
| App Store Server | `GameFrameX.Pay.Apple.AppStoreServer`（API v2 通知处理） |
| Google Play | `GameFrameX.Pay.Google` |
| Xsolla | `GameFrameX.Pay.Xsolla` |

---

## 加一个新 CRUD 服务

> 这一节针对 **Admin API** 的动态 API 服务。Hub API 是传统 Controller，不套这套——加 Hub 接口直接建 Controller + 注入仓储就行，参考 `GameFrameX.Hub.Api/Controllers/` 里现有的。

跟着做一遍，你就掌握了 80% 的日常后端活儿。假设要加一个"游戏道具"。

### 1. 建实体

在 `GameFrameX.Entity/` 对应子目录里建实体类：

```csharp
[SugarTable(null, "游戏道具")]
[SystemTable]
[IncrementTable]
public class GameItemEntity : EntityBasic
{
    [SugarColumn(ColumnDescription = "道具名称", Length = 64)]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "道具数量")]
    public int Count { get; set; }
}
```

实体基类按需挑：

| 基类 | 特点 | 适用场景 |
|------|------|----------|
| `EntityBaseId` | 只有 `Id` | 关联表、无租户实体 |
| `EntityTenantBase` | Id + 租户 + 审计 + 软删除 | 系统表（外部引用） |
| `EntityBasic` | 继承 `EntityTenantBase` | 通用业务实体（最常用） |
| `EntityBasicData` | 再加组织数据权限 | 需按组织过滤的数据 |
| `EntityBasicSelect` | 再加 Name + Description | 下拉选择器实体 |

### 2. 建种子数据（可选）

在 `GameFrameX.Application/SeedData/` 对应子目录建种子类：

```csharp
public class GameItemSeedData : ISqlSugarEntitySeedData<GameItemEntity>
{
    public IEnumerable<GameItemEntity> HasData()
    {
        return new[]
        {
            new GameItemEntity { Id = YitIdHelper.NextId(), Name = "金币", Count = 0 }
        };
    }
}
```

种子数据用 `StorageableByObject` 按主键 InsertOrUpdate，所以反复执行也安全。**注意**：因为是按主键 Upsert，如果你改了种子的内容但主键没变，旧记录不会被覆盖更新——要看到新值得删掉旧行再重启。

### 3. 写 Service

在 `GameFrameX.Application.Game/Service/`（或 System / Client / Tenant）下建服务：

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

收工。你现在自动拥有六个接口：

| 端点 | 干啥 |
|------|------|
| `Page` | 分页查询 |
| `Add` | 新增 |
| `Update` | 修改 |
| `Get` | 按 Id 查单条 |
| `List` | 列表（不分页） |
| `Delete` | 删除（软删除） |

如果还要下拉选择器支持，把基类换成 `GenericCrudSelectService`，额外白嫖 `Select` 和 `CheckNameUnique` 两个端点。

### 4. （需要的话）自定义 DTO

简单场景直接复用基类那几个：`BaseAddInput`、`BaseUpdateInput`（带 `Id`）、`BaseIdInput`、`BasePageInput`（带 `SearchKey`、`Page`、`PageSize`、排序字段）。

字段不够用时，在服务的 `Dto/` 子目录或 `GameFrameX.Core.Extension/Dto/` 里加：

```csharp
// 输出 DTO——继承实体即可
public class GameItemOutput : GameItemEntity, IBasePageOutput<GameItemEntity> { }

// 更新 DTO——继承基类再加字段
public class UpdateGameItemInput : BaseUpdateInput
{
    [Required] public string Name { get; set; }
}
```

### 5. 告诉前端

后端 OpenAPI 变了之后，前端要重新导出 `openapi.json` 跑 `pnpm gen:api`，不然前端调用点对不上。具体步骤见 [导出 OpenAPI 给前端](#导出-openapi-给前端)。

---

## 开发规范

> 这一节是**约定**——项目里大家都这么干，照着来省心。

### Admin API vs Hub API：别把两套混了

| 维度 | Admin API | Hub API |
|------|-----------|---------|
| 路由 | 动态 API（`IDynamicApiController`） | 传统 Controller（`[Route("api/[controller]/[action]")]`） |
| 数据访问 | 经 Application Service → 仓储 | Controller 直接注入 `Repository<T>` |
| 响应包装 | `AdminResultProvider` 自动套壳 | 手动返回 `HttpJsonResultData<T>` |
| 复用 Application 层 | 是 | 否（Validator / CheckXxxUnique 对 Hub 是死代码） |
| 加新接口 | 写 Service | 建 Controller + 注入仓储 |

给 Hub 加接口时，**别**想着复用 Admin 那边提层的校验——Hub 自己写。

### 仓储扩展方法：先找轮子再造轮子

`GameFrameX.Core.Extension/RepositoryExtension.cs` 里已经备好常用的了（Hub API 的 `Repository<T>` 也能用其中一部分，比如带乐观锁的更新）：

| 方法 | 干啥的 |
|------|--------|
| `FakeDelete` / `FakeDeleteAsync` | 软删除（`IsDeleted = true`） |
| `OrderBuilder(input)` | 根据 `BasePageInput` 动态排序 |
| `UpdateWithDiffLog` / `InsertWithDiffLog` | 带审计日志的增删改 |
| `UpdateWithOptimisticLockAsync` | 带乐观锁的更新（Hub API 打包上报在用） |
| `AS<T>()` | 跨库查询 / 操作 |
| `RunWithoutFilter` | 临时跳过查询过滤器（比如要查已删除的数据） |

### 定时任务

实现 `IJob` 接口，然后要么在管理后台的"定时任务"页面配，要么用 `[JobDetail]` 特性标注：

```csharp
[JobDetail("job_my_task", Description = "我的定时任务")]
public class MyTask : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken ct)
    {
        // 你的任务逻辑
    }
}
```

任务支持三种类型：内置类型、脚本编译、HTTP 调用。通过 `DbJobPersistence` 持久化到数据库，所以多实例部署时它能协调好，不会几个实例同时跑同一个任务。

> 注意：订单发货那块的进程锁在多实例下会失效，这块还在评估，暂时别指望它能跨实例互斥。

### 中间件管道：请求是怎么被一层层处理的

Admin API 的请求处理顺序长这样（从外到内）：

```
异常处理 / HSTS
  → 统一错误状态码
  → 本地化
  → 静态文件
  → 路由
  → CORS
  → IP / 客户端限流
  → 认证 / 授权
  → Schedule UI
  → Knife4j (Swagger)
  → Project DI
  → SignalR Hub + MVC 路由
```

Hub API 的管道不一样，见 [Hub API：第三张门](#hub-api第三张门)。排查请求问题时，按对应主机的管道顺序想想"哪一层把它挡了"，比无脑重启管用。

### C# 代码风格

这个项目按 **C# 10 语法**、**.NET 10 运行时**（`Directory.Build.props`：`LangVersion=10` + `net10.0`）。可以用 record、target-typed new、file-scoped namespace、global using。但有一条铁律：**所有控制流语句必须带大括号**，哪怕 body 只有一行。`using var x = ...;` 这种单行 using 声明也别用，老老实实写 `using (var x = ...) { }`。

---

## 常见问题与踩坑

### FAQ

**为什么我写了 Service 却调不到接口？**
检查三件事：类有没有实现 `IDynamicApiController`、方法是 public 吗、有没有被 `[NonAction]` 标了。再不行看启动日志，路由注册时会打印。（这只针对 Admin API——Hub API 是手写 Controller，没有动态路由这回事。）

**前端调的 URL 和我的类名对不上？**
那就是忘了"去 Service 后缀 + 首字母小写"的规则（Admin API）。`SystemUserService` 的 URL 是 `/api/systemUser/...`，不是 `/api/systemUserService/...`。前端 URL 严禁带 `Service`。Hub API 不走这套，URL 按 Controller 名 + Action 名拼。

**改了实体，表结构没更新？**
Code-First 默认只建表、加列。删列、改列类型这种破坏性变更，可能要手动处理或重启时留意。生产环境改表前先备份。

**SuperAdmin 看到的数据比别人多，是不是 bug？**
不是。SuperAdmin 绕过所有租户过滤和软删除过滤，所以能看见全部。这是设计如此。**但 Hub API 会拒绝 SuperAdmin 访问**——Hub 只服务租户域。

**种子数据改了，重启后还是旧的？**
种子按主键 Upsert。主键没变的话，已有记录不会被覆盖。删掉那行再重启，或者换个主键。

**Hub API 报 401，但同一个 token 调 Admin API 是好的？**
Hub 复用 Admin 的 JWT 密钥，但读的是自己 `appsettings.json` 里的 `JWTSettings` 节点。两边密钥（`IssuerSigningKey` / `ValidIssuer` / `ValidAudience`）必须一致。不一致就签出来的 token 互相不认。

**打包上报（PackageBuildReport）报 401？**
那不是 JWT 鉴权，是机器凭证（AccessKey + 签名 + scope）。走的是 `TenantGameAppOpenAccessAuthMiddleware`，凭证要先在 Admin 后台的开门接口管理里建好、绑到对应 App。

### 踩坑

**多数据库路由不生效**
检查实体上的特性有没有打对：`[SystemTable]` / `[LogTable]` / `[Tenant]`。没打标记的实体默认按 JWT Claim 里的 TenantId 路由，没登录上下文时就路由到主库。

**审计字段是空的**
审计靠 SqlSugar AOP 自动填充。如果你绕过仓储直接操作 `SqlSugarClient`，AOP 可能不走，字段就空了。老老实实用 `SqlSugarRepository<T>`。（Hub API 直接用仓储，是 OK 的。）

**JWT 一直过期 / 续签不生效**
看响应头有没有 `X-Authorization`。没有的话检查 `JwtHandler` 的续签逻辑和 token 有效期配置。平台 JWT 配置已经搬进主库 `SystemConfig` 的 `platform_jwt_settings`，别再去 `JWT.json` 里改。

**给 Hub 加接口，结果校验逻辑和 Admin 对不上**
正常。Hub 不复用 Admin 的 Application 层校验。你要么在 Hub Controller 里手写，要么接受两边校验逻辑各写一份——这是 Hub 当前的明确取舍。

**定时任务在多实例下重复执行了**
任务靠数据库 + 集群协调。确认 `DbJobPersistence` 工作正常、集群协调配置没漏。订单发货那块的进程锁多实例下确实会失效，已知的坑。

---

## 部署

### Docker Compose（推荐）

```bash
docker-compose up --build
```

一条命令把三个主机全拉起来。端口映射和本地 `dotnet run` **完全一致**：

| compose 服务 | 容器名 | 端口 | 对应主机 | Dockerfile |
|--------------|--------|------|----------|-----------|
| `web-entry` | `gameframex-web-entry` | `20802:20802` | Admin API | `Dockerfile.web-entry` |
| `client-api` | `gameframex-client-api` | `20808:20808` | Client API | `Dockerfile.client-api` |
| `hub-api` | `gameframex-hub-api` | `20806:20806` | Hub API | `Dockerfile.hub-api` |

三个 Dockerfile 都是多阶段构建（`sdk:10.0` 打包 → `aspnet:10.0` 运行），以非 root 用户跑，时区设成 `Asia/Shanghai`。配置文件通过 volume 挂载，所以**镜像构建完还能改配置**，不用重新 build。三个容器共用宿主机那个 PostgreSQL（走 `host.docker.internal:5432`），不再单独起数据库容器。

> Swagger 在 Production 容器里默认不挂，三个主机都用 `Swagger__Enabled=true` 显式打开（见 compose 的环境变量段）。

> 注意历史命名：compose 里的 `web-entry` 就是 Admin API（Web.Api），名字沿用了旧版的 "Web.Entry"。别被它绕进去。

### 发布前清单：数据库重灌与紧凑短 ID

紧凑短 ID 布局的发布口径是**可重置、无迁移脚本**（决策 2026-09-15）：新旧 ID 布局不兼容，生产 PG 主库 / 租户库按「清空重灌」发布，不允许新旧 ID 混写。发布前过一遍：

1. **清空重灌**：生产 PG 主库 + 全部租户库清空，由各 host 启动重建（CodeFirst 建表 + 种子数据重灌）。旧库里 18-19 位长 Id 的数据不要往新布局迁——新旧 Id 空间重叠是数据完整性事故。
2. **WorkerId 互斥核对**：同库写入的所有实例（Web.Api 副本 / Hub / Client）`SnowFlake__WorkerId` 两两不同且在 0-7。默认值 1/2/3 只保证单机部署不撞；扩副本前必须显式分配。
3. **量级抽查**：重灌后抽查业务表主键应为 13-14 位数字（< 2^53）。看到 18-19 位说明有路径没走 Foundation 取号，停下来排查。

---

## 导出 OpenAPI 给前端

前端那套 TypeScript API 客户端，是靠 `openapi.json` + `pnpm gen:api` 生成的。**后端只负责把 Swagger JSON 端点暴露出来**，导出动作在前端仓库做。

| 项 | 值 | 出处 |
|---|---|---|
| Admin Swagger 文档名 | `web.api` | `GameFrameX.Web.Core/ApiRouting/AdminOpenApiGroups.cs` |
| Admin Swagger JSON | `http://<host>:20802/swagger/web.api/swagger.json` | `AdminOpenApiGroups.WebGroup.RouteTemplate` |
| Client API Swagger JSON | `http://<host>:20808/swagger/client.api/swagger.json` | `GameFrameX.Client.Api/Program.cs` |
| Hub API Swagger JSON | `http://<host>:20806/swagger/v1/swagger.json` | `GameFrameX.Hub.Api/Program.cs`（`SwaggerDoc("v1", ...)`） |
| **默认开关**（三者一致） | 开发环境自动开；其他环境必须 `Swagger:Enabled=true`（环境变量 `Swagger__Enabled=true`） | 各自 `Program.cs` / `Startup.cs` |
| **生产 / Docker 默认不暴露** | 是——Swagger 中间件压根不挂载，没有 `/swagger/...` 端点 | 同上 |

> Hub API 的 Swagger 文档名是 `v1`（不是 `hub.api`），所以 URL 是 `/swagger/v1/swagger.json`。Admin 前端一般只用 Admin API 的 `web.api`，Hub 和 Client 的 swagger 主要是给游戏端 / CI 集成时查接口用。

生产默认不挂 Swagger 是故意的——别把接口文档往公网裸奔。

```bash
# 1. 让 Admin API 的 Swagger 可访问（二选一）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api &
# 或
Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api &

# 2. 拉到前端仓库根
curl -sSL http://localhost:20802/swagger/web.api/swagger.json \
  -o ../GameFrameX.Admin.Unified.Vue/openapi.json
ls -lh ../GameFrameX.Admin.Unified.Vue/openapi.json   # 大概 3 MB 才对

# 3. 前端跑生成
( cd ../GameFrameX.Admin.Unified.Vue && pnpm gen:api && git diff src/api/ )
```

> 后端 OpenAPI 一旦变更（新增 / 修改 / 删除 service 端点），必须重新导出 + 让前端 `pnpm gen:api`，否则前端调用点和后端对不上。前端 `pnpm run quality` 里的 `check:routes` 会用后端菜单种子对齐前端组件路径，对不上会报。

---

## 命令速查

| 命令 | 干嘛的 |
|------|--------|
| `dotnet run --project GameFrameX.Web.Api` | 启动 Admin API（20802） |
| `dotnet run --project GameFrameX.Client.Api` | 启动 Client API（20808） |
| `dotnet run --project GameFrameX.Hub.Api` | 启动 Hub API（20806） |
| `dotnet build` | 编译整个解决方案 |
| `Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api` | 非 dev 环境启动并暴露 Swagger |
| `ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api` | dev 环境启动（Swagger 自动开） |
| `docker-compose up --build` | Docker 一把起三个主机 |
| `curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o ../GameFrameX.Admin.Unified.Vue/openapi.json` | 导出 Admin Swagger JSON 给前端 |

常用地址：

| 地址 | 是啥 |
|------|------|
| `http://localhost:20802/kapi` | Admin API 的 Knife4j / Swagger UI |
| `http://localhost:20802/swagger/web.api/swagger.json` | Admin Swagger JSON（docName=`web.api`） |
| `http://localhost:20806/swagger` | Hub API 的 Swagger UI |
| `http://localhost:20806/swagger/v1/swagger.json` | Hub Swagger JSON（docName=`v1`） |
| `http://localhost:20806/health` | Hub API 健康检查 |
| `http://localhost:20808` | Client API |
| `/hubs/onlineUser` | Admin API 的 SignalR 在线用户 Hub |

---

## 写在最后

口诀：

> **三个主机各管各的：Admin 写 Service 就是写接口，Client 管登录支付，Hub 是对外传统 Controller。写实体就是建表，URL 不带 Service，多租户自动兜底。Hub 不复用 Admin 的分层，自己写校验。改了 OpenAPI 别忘了喊前端重新生成。**

卡住了别死磕：先看上面的踩坑，没解决就翻 `gfx-doc/`（项目根的那套文档系统），再不行问同事。

想看具体代码就跳这几个地方：

- `GameFrameX.Web.Core/Startup.cs` —— Admin 启动配置、DI 注册、中间件管道
- `GameFrameX.Hub.Api/Program.cs` —— Hub 启动配置、中间件管道、JWT 复用
- `GameFrameX.Hub.Api/Controllers/PackageBuildReportController.cs` —— Hub "仓储直接操作、不调 Application Service" 的范例
- `GameFrameX.Core.SqlSugar/` —— SqlSugar 配置、AOP、多数据库路由
- `GameFrameX.Core/` —— 仓储基类、`GenericCrudService`、认证、SignalR
- `GameFrameX.Core.Extension/RepositoryExtension.cs` —— 仓储扩展方法（常用轮子）
- `GameFrameX.Application/SeedData/` —— 种子数据
- `GameFrameX.Application/Configuration/` —— 独立 JSON 配置文件

## 社区与支持

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

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
