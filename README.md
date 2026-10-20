<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin

> 一句话定位：**面向独立游戏开发者的运营 + 联运后台平台**。后端 `.NET 10 + SqlSugar` 三主机（Admin / Client / Hub），前端 `Vue 3 + PrimeVue`，从管理员登录到玩家支付回调一条链路全栈跑通，开箱即用。

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Admin.Unified.Api)](https://github.com/GameFrameX/GameFrameX.Admin.Unified.Api/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#5-分钟跑通) · QQ群: 467608841 / 233840761

</div>

---

## 这是什么 / 不是什么

**GameFrameX Admin** 是一套**面向游戏运营**的后台系统：管理端给运营 / GM / 客服改菜单、查订单、推版本、看日志；客户端 API 给游戏端登录 / 支付 / 拉配置；Hub API 给外部系统（CI 打包、Tapd WebHook、第三方回调）。一套仓库三套主机，分库部署互不拖累。

它**不是**通用 OA / CRM / 商城系统。菜单、权限、字典、表单、客户端配置等都是围绕"游戏后台"的真实业务场景设计的。

本项目基于 [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET) 二次开发，所有 Furion 框架残留与原 Admin.NET 业务命名已完成替换与重基线（U5 / U8 / C37–C41）。致谢原作者 [zuohuaijun](https://gitee.com/zuohuaijun) 及其贡献者。

## 5 分钟跑通

> 假设你要给一个**新游戏**接一套运营后台。读完这一节你应该能从 0 到登录、能用 TeamS admin 看见菜单树。

### 准备

- .NET 10 SDK
- Node.js ≥ 20（推荐 20）+ pnpm ≥ 11.5.2（`corepack enable`）
- 没有数据库也行——本地源码直跑默认 SQLite 零配置；生产用 PostgreSQL / MySQL 等 34 种 DbType

### 步骤

```bash
# 1. 取代码
git clone https://github.com/GameFrameX/GameFrameX.Admin.Unified.Api.git
git clone https://github.com/GameFrameX/GameFrameX.Admin.Unified.Vue.git

# 2. 起后端（在仓库根目录！连接串相对工作目录解析）
cd GameFrameX.Admin.Unified.Api
dotnet run --project GameFrameX.Web.Api     # Admin API → http://localhost:20802
dotnet run --project GameFrameX.Client.Api   # Client API → http://localhost:20808
dotnet run --project GameFrameX.Hub.Api      # Hub API → http://localhost:20806

# 3. 起前端
cd ../GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # 默认 http://localhost:20800（hash 路由）
```

打开浏览器 → 看到登录页 → 账号 `admin` / 密码 `123456` / 租户 `1300000000001`（首次启动 SqlSugar Code-First 会自动建表 + 灌种子）。

### 一条命令全栈（仓库根统一 compose）

```bash
docker compose up -d --build
```

Compose 内置 PostgreSQL + 三个 host（20802/20806/20808）+ Admin 前端（20800，与 `pnpm run dev` 互斥，起容器前先停 dev server），起完等 healthcheck 变绿即可访问。

更细的命令与踩坑见 [`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md)（后端 893 行手册）与 [`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md)（前端 962 行手册）。

## 它适合谁 / 不适合谁

| 适合 | 不太适合 |
|---|---|
| 独立 / 小团队游戏，需要一套能跑起来的运营后台 | 你已经有完整后台只想要其中一个模块 |
| 多端（管理 / 客户端 / 外部系统）需要清晰边界 | 单端简单后台（直接用 Admin.NET / vue-next-admin 更轻） |
| 多租户 SaaS 化游戏服务 | 单租户自营不打算对外开放 |
| 强需求 .NET + SqlSugar 技术栈 | 团队只熟悉 Java / Go / Node |
| 需要多语言（5 语 i18n）+ AI Agent 友好仓库 | — |
| 想要可演进到微服务的单体起步 | — |

## 技术栈

**后端（[`GameFrameX.Admin.Unified.Api`](GameFrameX.Admin.Unified.Api/)）**

| 层 | 选型 | 版本 / 备注 |
|---|---|---|
| 运行时 | .NET 10 | `Directory.Build.props`: `net10.0` / `LangVersion=10` |
| 框架 | ASP.NET Core 原生宿主 | 无第三方框架（自研 `IDynamicApiController` + `Web.Core/ApiRouting/AdminApiRouteRules`） |
| ORM | SqlSugar | Code-First，34 种 DbType；`SqlSugarRepository<T>` 自动主库 / 租户库 / 日志库路由 |
| 认证 | JWT Bearer + HMAC Signature | 三 host 共用密钥；HMAC-SHA256 签名（开放接口走 `SignatureAuthenticationHandler`） |
| 实时通信 | SignalR | `OnlineUserHub` 按租户分组（`GROUP_ONLINE_{tenantId}`），可选 Redis Backplane |
| 缓存 | Memory / Redis | `SystemCacheService` 统一抽象 |
| 调度 | `ProjectScheduler`（项目自有） | `GameFrameX.Core/Job/`，内置 / 脚本编译 / HTTP 三类任务，多实例通过 `DbJobPersistence` 协调 |
| 日志 | SqlSugar AOP + Serilog | 操作 / 异常 / 访问 / 数据变更 四类入库 |
| 数据库 | SQLite 本地 / PostgreSQL 生产 | 默认零配置；生产推荐 PG |
| 分布式 ID | Foundation Snowflake 紧凑布局 | 13-14 位数字，< 2^53 JS 安全域 |

**前端（[`GameFrameX.Admin.Unified.Vue`](GameFrameX.Admin.Unified.Vue/)）**

| 层 | 选型 | 版本 |
|---|---|---|
| 框架 | Vue 3（Composition API + `<script setup lang="ts">`） | 3.5.x |
| 构建 | Vite | 6.x |
| UI 组件库 | PrimeVue（Aura 主题）+ PrimeFlex + PrimeIcons | 4.5.x |
| 状态 | Pinia + pinia-plugin-persistedstate | 3.x |
| 路由 | Vue Router（hash 模式，后端动态菜单驱动） | 4.x |
| 语言 | TypeScript strict | 5.3.x |
| 国际化 | vue-i18n（5 语言：zh-cn / en / zh-tw / ja / ko） | 11.x，`pnpm run lint:i18n` 守护 |
| 图表 | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| 实时 | @microsoft/signalr | 10.x |
| 富文本 | wangEditor + editor-for-vue | 5.x |
| 编辑器 | Monaco Editor | 0.44.x |
| HTTP | Axios + 自研拦截器（JWT 自动续期 / 错误码 i18n） | 1.x |
| 图标 | FontAwesome 7（vue-fontawesome + 自动按需） | 7.x |
| 包管理 | pnpm | ≥ 11.5.2 |
| Node | — | ≥ 16（推荐 20） |

## 三套主机：Admin / Client / Hub

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

依赖方向（左到右是引用方向）：

```
Web.Api → Web.Core → Application.{System, Game, Client, Tenant} → Core → Entity → Core.Enum
Client.Api（独立，仅引用 Entity + Payment）
Hub.Api（独立，引用 Application.Game + Core + Entity，但 Controller 不调 Application Service）
```

**关键约定**：业务层永远不知道 `Web.Api` 的存在。`Application.*` 与 `Core.*` 严禁反向引用宿主——这样 Client / Hub 才能干净地把 Entity 拎出去独立用。

### 三个主机最大的差异

| 维度 | Admin API | Client API | Hub API |
|---|---|---|---|
| 路由 | 动态 API（写 Service 自动出接口） | 动态 API | 传统 Controller（`[Route("api/[controller]/[action]")]`） |
| 数据访问 | 经 Application Service → 仓储 | 经 Application Service → 仓储 | Controller 直接注入 `Repository<T>` |
| 复用 Application 层 | 是 | 是 | 否（自己校验） |
| 加新接口 | 写 Service | 写 Service | 建 Controller + 注入仓储 |
| URL 规则 | 去 `Service` 后缀 + camelCase | 去 `Service` 后缀 + camelCase | `Controller` 名 + `Action` 名 |
| 是否拒绝 SuperAdmin | 否（SuperAdmin 绕过所有过滤） | 否 | **是**——Hub 拒绝超管访问 |

> **URL 里严禁出现 `Service` 后缀**。这是新人 100% 会踩的坑——前端按这个约定生成调用，类名一变，前端全炸。

## 多租户 + 应用作用域

三层模型，从外到内：

```
SuperAdmin（平台级）── 可访问全局配置、跨租户查询
    ↓
Tenant 租户（多租户 SaaS 化）── 跨应用共享
    ↓
App 应用（租户下的具体游戏 / 产品）── AppId 作用域
```

| Scope | 含义 | 路由示例 |
|---|---|---|
| `platform` | 平台级（超管可见，跨租户） | `/platform/*` |
| `tenant` | 租户级（所有应用共有，无 appId query） | `/dashboard/*`、`/system/*` |
| `app` | 应用工作台内（必须带 `appId` query） | `/gameCenter/*` |

**写实体的两条铁律**：
1. **多租户是天生自带的**：实体打 `[Tenant]` 标记，查询自动加 `TenantId` 过滤 + 自动软删除。只有 SuperAdmin 能穿墙。写业务的时候几乎感觉不到。
2. **数据库边界由特性决定**：`[SystemTable]` 路由主库；`[LogTable]` 路由日志库；`[Tenant]` 路由租户库；无标记按 JWT `TenantId` 路由。仓储替你做了。

## 30 个功能模块

按"你会用到的场景"分组，不按目录结构。

**身份与权限**

| 模块 | 干啥 |
|---|---|
| 用户管理 | 租户用户 / 系统管理员维护，可绑定用户职务、机构、角色、数据权限 |
| 机构管理 | 多层级树形组织架构 |
| 职位管理 | 用户职务标签 |
| 菜单管理 | 目录 / 菜单 / 按钮 三级权限 |
| 角色管理 | 角色绑定菜单 + 数据授权范围 |
| 字典管理 | 全局 / 租户两级字典 |
| 开放授权 | AccessKey / Secret + HMAC 签名认证（外部接口） |

**平台配置（仅 SuperAdmin 可见）**

| 模块 | 干啥 |
|---|---|
| 平台租户管理 | 租户 CRUD、租户库自动开通 / 停用 / 到期 |
| 全局平台 / 语言 / 渠道定义 | 平台级枚举与多语言基线 |
| 区域管理 | 行政区划 5 级树，支持从统计局同步 |
| 系统配置 | 键值对；高频配置迁入主库 `SystemConfig`（JWT / 密码策略 / 图形验证码） |
| 数据库管理 | 表 / 列 CRUD、库表结构在线管理 |

**系统能力**

| 模块 | 干啥 |
|---|---|
| 服务监控 | 主机信息、CPU / 内存 / 磁盘 / 已加载程序集 |
| 在线用户 | 实时列表 + 强制下线，基于 SignalR |
| 公告管理 | 系统通知 + SignalR 实时推送 |
| 文件管理 | 本地存储 + 阿里云 OSS + 腾讯 COS（多 provider） |
| 定时任务 | `ProjectScheduler` 项目自有调度器；内置 / 脚本编译 / HTTP 三类；运行时编译 C# 动态 API |
| 邮件 / 短信 | `Mail.Abstract` 多 provider + `Sms.Abstract` Aliyun 实现 |
| 系统接口 | Swashbuckle Swagger；生产默认不挂（需 `Swagger__Enabled=true`） |
| 四类日志 | 操作 / 异常 / 访问 / 数据变更（字段级 Diff 审计），支持 Excel 导出 |
| 运行时插件 | C# 代码动态编译并注册为 API 端点 |
| 缓存管理 | Memory / Redis 统一抽象 |

**游戏后台（76 个 Application.Game 服务）**

| 模块 | 干啥 |
|---|---|
| 游戏账号 / 角色 / 区服 | 玩家账号、角色管理、Docker 容器生命周期 |
| 订单 / 发货 | 多维度筛选；HTTP 调游戏服 API 发道具 |
| 邮件 / 兑换码 | 全服 / 单服邮件；兑换码渠道与投放 |
| Web 商城 | 商品 + 订单 + 分类 |
| 道具 / 商品 | 道具目录、消耗品 / 永久品管理 |
| 公告 / 横幅 | 游戏内 + 登录后公告 |
| 应用版本 / 资源包 | App 版本 + 强制更新 + 资源包版本与历史回滚 |
| 应用作用域配置 | 远程参数、应用校验、客户端 KV |
| 邮件运营 | 模板 + Campaign + 多语言 |
| 广告回调 | TopOn / AdMob / UnityAds 多 provider |
| 游戏服对接 | 玩家注册 / 登录 / 订单回调 |

**集成**

| 集成 | 说明 |
|---|---|
| 微信 | `Wechat.Abstract` + `Wechat.Open` 双 SDK；公众号 OAuth2、小程序登录、模板消息、JS-SDK |
| 微信支付 | JSAPI 下单（直连 / 服务商）+ 支付回调（`Payment.Wechat`） |
| 支付 | 支付宝 / Apple IAP（含 App Store Server API v2）/ Google Play / Xsolla，抽象层 + 多 provider |
| 对象存储 | Local / Tencent COS（多 provider） |
| 短信 | Aliyun 短信 Provider |
| 邮件 | Tencent SES / SMTP（多 provider） |

**前端框架特性**

- **后端驱动菜单**：菜单、权限、文案 全部由后端控制。前端不写菜单，加菜单要去后端菜单管理里加；前端只负责渲染后端返回的菜单树。
- **JWT 自动续期**：登录拿到的 token 过期前自动加 `X-Authorization`，让后端静悄悄换发，登录一次能爽很久。
- **应用作用域**：路由 `meta.appScope` + `meta.menuContext` 标注 platform / tenant / app 三种 scope，前端按 scope 决定菜单可见性。
- **i18n 完整性门禁**：`pnpm run lint:i18n` 检查 5 语言键完整，缺一个直接 fail。

## 安全基线

不是"建议"，是**默认就生效**的硬约束。

| 威胁 | 应对 |
|---|---|
| 弱密码 | 全项目统一 **Argon2id**（C214 系统用户 / C215 玩家）；旧 PBKDF2 仅保留 `AppSecretHasher`（高熵随机密钥场景，不需要 Argon2id） |
| SQL 注入 | SqlSugar 全参数化，无字符串拼接 |
| 越权访问 | 多租户过滤 + 软删除过滤；SuperAdmin 绕过但 Hub API 拒绝超管 |
| 重放攻击 | HMAC 签名 + nonce 缓存 + 5 分钟时间偏差 |
| 信息泄露 | Response DTO 必含 Id/RowVersion/CreatedTime；敏感字段（密码哈希 / AccessSecret）**禁止**出现在任何 Response；契约测试守护 |
| 接口变更回潮 | `ThreeHostApiRoutingContractTests` + `AdminEndpointReturnContractTests` + `AdminActionStyleContractTests` + `NamingAbbreviationGuardTests` 防回潮 |

## 数据库

**默认零配置**：什么都不用配。`GameFrameX.Application/Configuration/Database.json` 是**单一连接串来源**（三 host 共享，改一处三处生效），本地默认 SQLite（仓库根 `db/` 下，WAL 模式）；生产用 PostgreSQL。

**主库 / 租户库 / 日志库自动路由**：

- `[SystemTable]` → 主库（平台定义、SuperAdmin bootstrap）
- `[LogTable]` → 日志库（操作 / 异常 / 访问 / 数据变更）
- `[Tenant]` → 租户库（业务数据）
- 无标记 → 按 JWT `TenantId` 路由

**租户库是自动建的**：创建普通租户时，按租户 ID 自动生成 `gameframex_admin_tenant_<TenantId>` 数据库和对应登录角色，**不需要 SuperAdmin 手动填租户库连接串**。默认租户库同样自动推导（PostgreSQL / MySQL 仅替换库名，SQLite 落同目录）。

**支持 34 种 DbType**：SQLite / PostgreSQL / MySQL / 达梦 / Oracle / SqlServer / …换库连改 `Database.json` 即可。连接串写法见 [`Database-ConnectionStrings.md`](GameFrameX.Admin.Unified.Api/GameFrameX.Application/Configuration/Database-ConnectionStrings.md)。

> SQLite 只面向本地开发。生产禁用——并发写入弱、无独立服务进程、备份恢复与运维监控能力不足。

## 演示与上手

| 资源 | 链接 |
|---|---|
| 演示站点（**请勿用于自己的生产环境**） | <https://game.admin.web.vue.alianblank.com> |
| 文档站 | <https://gameframex.doc.alianblank.com> |
| 后端运行手册 | [`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md) |
| 前端运行手册 | [`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md) |

## 项目仓库

| 项目 | 用途 | 仓库 |
|---|---|---|
| 本仓库 | Admin / Client / Hub 三 host 单仓库（Cron 化） | `GameFrameX/Admin` |
| 客户端接口（独立仓库） | 早期拆分；现统一在本仓库 | [GameFrameX.Admin.Client.Api](https://github.com/GameFrameX/GameFrameX.Admin.Client.Api) |
| 后台管理接口（付费源码） | 历史命名 | [GameFrameX.Admin.Web.Api](https://github.com/GameFrameX/GameFrameX.Admin.Web.Api) |
| 后台 Web 页面（付费源码） | 历史命名 | [GameFrameX.Admin.Web.Vue](https://github.com/AlianBlank/GameFrameX.Admin.Web.Vue) |

## 发布与部署

### Docker Compose 一键起（推荐）

```bash
docker compose up -d --build
# 内置 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + 前端(20800)，起完等 healthcheck 变绿（前端容器与 dev server 同抢 20800，互斥）
```

### 单容器

```bash
docker run -p 20802:20802 \
  -e GFX_MAIN_DB_TYPE=PostgreSQL \
  -e GFX_MAIN_DB_CONNECTION_STRING="Host=...;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  -e GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL \
  -e GFX_DEFAULT_TENANT_DB_CONNECTION_STRING="Host=...;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=...;Password=..." \
  gameframex-admin-web:1.0.0.0
```

环境变量成对设置（`GFX_MAIN_DB_TYPE` + `GFX_MAIN_DB_CONNECTION_STRING`），半套覆盖会在启动期直接报错。

## 开发与贡献

想加一个新 CRUD 服务？看 [`GameFrameX.Admin.Unified.Api/README.md` → 加一个新 CRUD 服务](GameFrameX.Admin.Unified.Api/README.md)。Admin API 只要实现 `IDynamicApiController` 就自动有 REST 端点（Page / Add / Update / Get / List / Delete 六个），无需手写 Controller。

CI 门禁（防回潮）：

- `ThreeHostApiRoutingContractTests` — 三 host 路由 / 谓词 / 参数绑定 / 错误码语义域
- `AdminEndpointReturnContractTests` — Response DTO 契约
- `AdminActionStyleContractTests` — Admin action 写法规范
- `NamingAbbreviationGuardTests` + `NamingAbbreviationBaseline.baseline.txt` — 全系统禁业务缩写

## 功能预览

### 平台管理（SuperAdmin）

![登录页](doc/img/1.png)
![主控台](doc/img/2.png)
![菜单管理](doc/img/3.png)
![参数配置](doc/img/4.png)
![字典管理](doc/img/5.png)
![任务调度](doc/img/6.png)
![服务监控](doc/img/7.png)
![缓存管理](doc/img/8.png)
![行政区划](doc/img/9.png)
![动态插件](doc/img/10.png)
![开放接口](doc/img/11.png)
![验证码配置](doc/img/12.png)
![租户管理](doc/img/13.png)
![库表管理](doc/img/14.png)

### 应用工作台核心功能（中英双语对照）

进入租户应用工作台后的 5 个核心功能页面。同一页面在中英文界面语言下的实际效果（右上角语言切换器实时切换，无需刷新）：

| 功能 | 简体中文 | English |
|------|----------|---------|
| 远程配置 | ![远程配置-中文](doc/img/15.png) | ![Remote Config-EN](doc/img/16.png) |
| 版本更新 | ![版本更新-中文](doc/img/17.png) | ![Version Update-EN](doc/img/18.png) |
| 分包管理 | ![分包管理-中文](doc/img/19.png) | ![Package Management-EN](doc/img/20.png) |
| 支付配置 | ![支付配置-中文](doc/img/21.png) | ![Payment Config-EN](doc/img/22.png) |
| 广告回调验签 | ![广告回调验签-中文](doc/img/23.png) | ![Ad Callback-EN](doc/img/24.png) |

## 文档与资源

- 项目文档站：<https://gameframex.doc.alianblank.com>
- 后端运行手册（详细）：[`GameFrameX.Admin.Unified.Api/README.md`](GameFrameX.Admin.Unified.Api/README.md)
- 前端运行手册（详细）：[`GameFrameX.Admin.Unified.Vue/README.md`](GameFrameX.Admin.Unified.Vue/README.md)
- Issue 反馈：[GitHub Issues](https://github.com/GameFrameX/GameFrameX.Admin.Unified.Api/issues)
- 客户端 / 后台 / Web 三仓库：见 [项目仓库](#项目仓库)

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