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

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761
<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

這是 **GameFrameX 遊戲管理平台**的後端服務，用 .NET 10 + SqlSugar 寫的。前端同學在瀏覽器裡點的每一個按鈕、遊戲客戶端發出的每一個請求，最後都是這裡在接單。

> 配套前端在 [`GameFrameX.Admin.Unified.Vue`](../GameFrameX.Admin.Unified.Vue)，資料庫本地預設 SQLite 零配置，生產用 PostgreSQL。本倉儲只管「出接口、存資料、跑定時任務」這些事。

## 目錄

- [項目簡介](#項目簡介)
- [技術棧](#技術棧)
- [三個主機，各管各的](#三個主機各管各的)
- [倉儲目錄結構](#倉儲目錄結構)
- [快速開始](#快速開始)
- [它是怎麼跑起來的](#它是怎麼跑起來的)
- [功能模組一覽](#功能模組一覽)
- [新增一個 CRUD 服務](#新增一個-crud-服務)
- [開發規範](#開發規範)
- [常見問題與踩坑](#常見問題與踩坑)
- [部署](#部署)
- [匯出 OpenAPI 給前端](#匯出-openapi-給前端)
- [命令速查](#命令速查)
- [寫在最後](#寫在最後)
- [文檔與資源](#文檔與資源)
- [更新日誌](#更新日誌)

---

## 項目簡介

一句話：給遊戲營運平台提供所有後端接口。營運加用戶、改選單、看訂單、發郵件、推版本、看日誌；遊戲客戶端登入、付錢、拿組態；CI 打包機回寫版本——這些活兒的資料出口都在這裡。

後端裡住著**三個獨立的主機**，各跑各的處理程序、各佔各的埠，誰也不拖累誰：

- **Admin API**（`GameFrameX.Web.Api`，埠 `20802`）—— 給管理後台用的接口
- **Client API**（`GameFrameX.Client.Api`，埠 `20808`）—— 給遊戲客戶端用的接口（登入、支付、拿組態）
- **Hub API**（`GameFrameX.Hub.Api`，埠 `20806`）—— 給外部系統 / 遊戲端 / CI 打包 / 第三方 WebHook 用的聚合接口

Client API 和 Hub API 都更「瘦」：Client API 只引用 Entity + Payment，不碰舊框架；Hub API 走傳統 Controller 路由、直接用倉儲，不走動態 API 那套（後面細講）。

**最值得知道的三件事**：

1. **寫個 Service 就自動有接口**（Admin API）。實作 `IDynamicApiController`，類別裡的 public 方法會自動變成 REST 端點，你不用手寫 Controller、不用手貼 `[Route]`。這是 Admin API 的靈魂。
2. **寫個實體就自動建表**。SqlSugar Code-First：啟動時掃實體類別 → 建表 → 填種子資料。你不用寫一行建表 SQL，改了欄位重啟一下，表結構就跟上了。
3. **多租戶是天生自帶的**。實體打個標記，查詢就自動加 `TenantId` 過濾、自動軟刪除，只有 SuperAdmin 能穿牆。你寫業務的時候幾乎感覺不到它的存在。

---

## 技術棧

| 類別 | 選型 | 小白註解 |
|------|------|----------|
| 執行時 | .NET 10 | 跑這套程式碼需要的發動機（`Directory.Build.props` 裡 `net10.0`） |
| 語言版本 | C# 10 | `LangVersion=10`，可以用 record、file-scoped namespace 等 |
| 框架 | ASP.NET Core 原生宿主 | 沒用額外框架，就靠原生那一套 |
| ORM | SqlSugar (Code-First) | 寫 C# 實體類別，它幫你產生資料庫表 |
| 認證 | JWT Bearer + HMAC Signature | Admin/Client/Hub 共用同一份 JWT；開放接口另外用簽名 |
| 即時通訊 | SignalR (可選 Redis Backplane) | 伺服器主動往瀏覽器推訊息用的 |
| 快取 | Memory / Redis | 本機記憶體夠用，多機就上 Redis |
| 定時任務 | 專案自有 Scheduler | 任務存資料庫，多實例能協調，不會重複跑 |
| 日誌 | 資料庫 + ElasticSearch + Serilog | 操作日誌進庫，大資料量走 ES |
| 資料庫 | SQLite（本地預設）/ PostgreSQL（生產），也支援 MySQL / Oracle 等十餘種 | 換庫連接串寫法見[速查文檔](GameFrameX.Application/Configuration/Database-ConnectionStrings.md) |
| 容器化 | Docker Compose | 一條命令把三個主機都跑起來 |

---

## 三個主機，各管各的

```
Admin API（20802）              Client API（20808）            Hub API（20806）
    │                              │                              │
    │ 管理後台用                    │ 遊戲客戶端用                  │ 外部系統 / 遊戲端 / CI 用
    │ 登入 / 用戶 / 選單 /          │ 登入 / 支付 / 拿組態          │ 區服 / 資源版本 / 道具 /
    │ 訂單 / 日誌 / 定時任務         │                              │ 兌換碼 / 打包上報 / Tapd WebHook
    │                              │                              │
    └──────────── 共享 Entity ──────────────┬─────────────────────┘
                                           │
                                           ├── 共享 Payment（僅 Client 用）
                                           └── 三者各自直接用 SqlSugar 倉儲
```

依賴關係長這樣（左到右是引用方向）：

```
Web.Api → Web.Core → Application.{System, Game, Client, Tenant} → Core → Entity → Core.Enum
Client.Api（獨立，僅引用 Entity + Payment）
Hub.Api（獨立，引用 Application.Game + Core + Entity，但 Controller 不調 Application Service）
```

記住一條：**業務層不反向引用宿主**。`Application.*` 和 `Core.*` 永遠不知道 `Web.Api` 的存在，依賴只能往下走。這樣 Client API / Hub API 才能乾淨地把 Entity 單獨拎出去用。

### Hub API 為什麼和 Admin API 長得不一樣

Admin API 是「動態 API」——寫 Service 自動出接口；Hub API 是「傳統 Controller」——`[Route("api/[controller]/[action]")]` 手寫路由，Controller 裡直接注入 `Repository<T>` 操作實體，**不調 Application Service**。這是 Hub.Api 刻意的約定：它面向外部系統，邏輯簡單、要輕、要快，所以 Controller 自己查重、自己校驗歸屬，不走 Admin 那套分層。

詳細的差異見 [它是怎麼跑起來的](#它是怎麼跑起來的) 裡的 [Hub API：第三張門](#hub-api第三張門)。

---

## 倉儲目錄結構

```
GameFrameX.Admin.Unified.Api/
├── GameFrameX.Web.Api/            # Admin API 宿主（啟動入口，compose 服務名 web-api）
├── GameFrameX.Client.Api/         # Client API 宿主（獨立 WebApplication）
├── GameFrameX.Hub.Api/            # Hub API 宿主（獨立 WebApplication，傳統 Controller 路由）
├── GameFrameX.Web.Core/           # Admin 的啟動邏輯、中介軟體、DI 組態（重點）
├── GameFrameX.Application/        # 應用層（組態檔、種子資料）
├── GameFrameX.Application.System/ # 系統管理服務（用戶/角色/選單/租戶/日誌...）
├── GameFrameX.Application.Game/   # 遊戲管理服務（區服/訂單/郵件/兌換碼...）
├── GameFrameX.Application.Client/ # 客戶端管理服務（版本/渠道/資源包...）
├── GameFrameX.Application.Tenant/ # 租戶級服務（設備 / 設備分組）
├── GameFrameX.Entity/             # 實體層（Base / System / Game / Client）
├── GameFrameX.Core/               # 核心層（倉儲、服務基類、認證、SignalR）
├── GameFrameX.Core.Enum/          # 列舉定義
├── GameFrameX.Core.Extension/     # 擴充方法與公共 DTO（重點，常來這兒找輪子）
├── GameFrameX.Core.SqlSugar/      # SqlSugar 組態、AOP、攔截器
├── GameFrameX.Core.Utility/       # 工具類別
├── GameFrameX.Core.BaseInputOutput/ # 基礎輸入輸出模型
├── Payment/                       # 支付整合（僅 Client API 引用）
│   ├── GameFrameX.Pay.Abstract/          # 支付抽象介面（先定義後實作）
│   ├── GameFrameX.Pay.AliPay/            # 支付寶
│   ├── GameFrameX.Pay.Apple/             # Apple IAP（內購驗證）
│   ├── GameFrameX.Pay.Apple.AppStoreServer/  # App Store Server API v2
│   ├── GameFrameX.Pay.Google/            # Google Play
│   └── GameFrameX.Pay.Xsolla/            # Xsolla
├── Dockerfile.web-api             # Admin API 的 Dockerfile（sdk:10.0 → aspnet:10.0）
├── Dockerfile.client-api          # Client API 的 Dockerfile
├── Dockerfile.hub-api             # Hub API 的 Dockerfile
└── (統一編排已上移倉儲根 docker-compose.yml：PostgreSQL + 三 API + 前端共 5 服務)
```

平時改程式碼，99% 時間在 `Application.*` 和 `Entity` 裡轉悠。`Core.*` 是地基，動它之前最好先看明白誰在依賴它。

---

## 快速開始

### 環境要求

- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code，挑個你順手的
- 資料庫不需要預裝：本地原始碼直跑預設用 SQLite（檔案落在倉儲根 `db/` 目錄）；要連 PostgreSQL 時再裝，或直接用下面的 Docker Compose（內建 PG）

### 資料庫：預設零配置，生產用 PostgreSQL

**本地原始碼直跑（預設 SQLite）**：什麼都不用配。預設連接在 `GameFrameX.Application/Configuration/Database.json` 裡，三個 host 共享倉儲根 `db/` 目錄下的同一份 SQLite 庫檔案（WAL 模式）。約定是**在倉儲根目錄執行 `dotnet run`**（連接串相對處理程序工作目錄解析）。

注意：SQLite 只面向本地開發/偵錯。線上/生產環境禁止使用 SQLite（並發寫入弱、無獨立服務處理程序、備份還原與維運監控能力不足），請用 PostgreSQL。

**生產 / 容器（PostgreSQL，環境變數成對覆蓋）**：覆蓋資料庫時類型和連接串必須**成對**設定，半套覆蓋會在啟動期直接報錯。三個 host（Web / Client / Hub）用**同一對變數**：

- `GFX_MAIN_DB_TYPE=PostgreSQL` + `GFX_MAIN_DB_CONNECTION_STRING=Host=...`
- 預設租戶庫不用配：從主庫連接串自動推導（僅換庫名，見下）
- 存量部署的裸 `DbType` / `ConnectionString` 啟動參數仍可用（相容層），啟動會打遷移警告，請盡快切換
- 換其他資料庫（MySQL / Oracle / 達夢……全部 34 種 DbType）的連接串寫法與範例：[`GameFrameX.Application/Configuration/Database-ConnectionStrings.md`](GameFrameX.Application/Configuration/Database-ConnectionStrings.md)

關於資料庫，還有幾個約定你要知道：

- **租戶庫是自動建的**。建立普通租戶時，系統會按租戶 ID 自動產生 `gameframex_admin_tenant_<TenantId>` 資料庫和對應的登入角色，**不需要 SuperAdmin 手動填租戶庫連接串**。
- **預設租戶庫也是自動推導的（C269）**。主庫連接串 → 預設租戶庫連接串：PostgreSQL / MySQL 僅替換庫名為 `gameframex_admin_tenant_1300000000001`（與普通租戶庫同規律，帳號/密碼繼承主庫，MySQL 建庫強制 utf8mb4）；SQLite 落在主庫同目錄 `gameframex_admin_tenant_1300000000001.db`。僅支援 PostgreSQL / MySQL / SQLite，其他類型啟動報錯。顯式組態優先：`Database.json` 加回 `ConfigId=1300000000001` 項或成對設定 `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING`。
- **既有部署升級（BREAKING）**：預設租戶庫名由 `gameframex_admin_tenant_default` 變為 `gameframex_admin_tenant_1300000000001`，二選一：a. 線上改名 `ALTER DATABASE gameframex_admin_tenant_default RENAME TO gameframex_admin_tenant_1300000000001`（註冊表會自動跟上推導值）；b. 成對設定 `GFX_DEFAULT_TENANT_DB_*` 指向舊庫名凍結不動。
- **Compose 內建 PostgreSQL**。在倉儲根執行 `docker compose up` 會自帶起一個 `postgres:16-alpine`（首次啟動自動建庫，見 `docker/postgres/init.sql`，資料落在倉儲根 `postgres-data/` bind mount）。要複用宿主機已有的 postgres 容器，在倉儲根 `.env` 裡設 `POSTGRES_HOST=host.docker.internal` 等變數並移除 api 服務對 `postgres` 的 `depends_on` 即可。
- **三個 host 共讀同一份預設組態**。Client / Hub 啟動時自動載入 `Database.json`（與 Admin 同源，改一處三處生效）；環境變數覆蓋寫法與 compose 一致。

### 啟動方式一：Docker Compose 一鍵起（推薦，零配置）

```bash
# 在倉儲根執行（不在 Api 子目錄）
docker compose up -d --build
# 內建 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + 前端(20800)，起完等 healthcheck 變綠
```

### 啟動方式二：原始碼直跑（零配置，SQLite）

```bash
# 注意：在倉儲根目錄執行，四個 host 共享 ./db/ 下的 SQLite 庫檔案

# Admin API（管理後台）
dotnet run --project GameFrameX.Web.Api
# 監聽 http://*:20802，Swagger UI: http://localhost:20802/kapi

# Client API（遊戲客戶端）
dotnet run --project GameFrameX.Client.Api
# 監聽 http://*:20808

# Hub API（外部系統 / CI / WebHook）
dotnet run --project GameFrameX.Hub.Api
# 監聽 http://localhost:20806，Swagger UI: http://localhost:20806/swagger

# Game API（遊戲伺服器側）已併入 Client API（C268）：GameUser* 控制器隨 Client.Api 暴露（http://*:20808）
```

第一次啟動 Admin API 時，SqlSugar Code-First 會自動建庫建表，順手把種子資料（管理員、角色、選單、字典這些）灌進去。所以你看到一個空資料庫突然冒出一堆表，別慌，那是正常操作。

### 啟動方式三：單容器 docker run

鏡像內預設組態就是 SQLite（零環境變數可直接起，資料在容器內 `/app/db/`，掛 volume 可持久化）。生產用法是注入成對環境變數連 PostgreSQL：

```bash
docker run -p 20802:20802 \
  -e GFX_MAIN_DB_TYPE=PostgreSQL \
  -e GFX_MAIN_DB_CONNECTION_STRING="Host=你的PG;Port=5432;Database=gameframex_admin;Username=...;Password=..." \
  -e GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL \
  -e GFX_DEFAULT_TENANT_DB_CONNECTION_STRING="Host=你的PG;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=...;Password=..." \
  你的鏡像
```

### 卡住了？

| 現象 | 大機率原因 |
|------|------------|
| 啟動報資料庫連線失敗 | `Database.json`（或 `GFX_MAIN_DB_*` 環境變數）連接串不對，或者 PG 沒起 |
| 啟動報「DbType 為 Sqlite/PostgreSQL，但連接串不是 …風格」 | 環境變數半套覆蓋了：`GFX_MAIN_DB_TYPE` 和 `GFX_MAIN_DB_CONNECTION_STRING` 必須成對設定 |
| 啟動報「預設租戶庫不支援自動推導」 | 主庫用了 PostgreSQL / MySQL / SQLite 之外的 DbType，需成對設定 `GFX_DEFAULT_TENANT_DB_TYPE` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING` 顯式指定 |
| SQLite 庫檔案落錯位置 | 連接串相對處理程序工作目錄解析——回到倉儲根目錄執行 `dotnet run`；專案目錄裡跑出來的 `db/` 刪掉即可 |
| 表沒建出來 | Code-First 開關預設在程式碼中全開（C278）；若顯式組態過關閉項，檢查 `Database.json` 或環境變數 |
| Swagger 打不開 | 非開發環境預設不掛 Swagger，見下面的 [匯出 OpenAPI](#匯出-openapi-給前端) |
| 種子資料沒灌進去 | 種子資料開關被關了；另外種子是按主鍵 Upsert 的，改了種子要刪舊記錄才會重灌 |
| Hub API 啟動報「未組態 JWTSettings」 | Hub 複用 Admin 的 JWT 組態，`Hub.Api/appsettings.json` 裡要有和 Admin 一致的 `JWTSettings` 節點 |
| 埠被佔 | 20800 / 20802 / 20808 / 20806 被別的處理程序佔了（前端容器與 dev server 同搶 20800），`lsof -i :20806` 查一下 |

---

## 它是怎麼跑起來的

### 動態 API：寫 Service 就是寫接口（Admin API）

Admin API 的服務實作 `IDynamicApiController` 之後，框架會自動把類別裡的 public 方法變成 REST 端點（路由是 camelCase），你不用手寫 Controller：

```csharp
public class MyService : ITransient, IDynamicApiController
{
    // 自動變成 GET /api/my/list
    public List<MyEntity> List() => ...;
}
```

路由產生規則（務必記住，前端 100% 會按這個調你）：

1. 拿類別名，去掉 `Service` / `AppService` / `Controller` 後綴（只去一次）
2. 首字母小寫
3. 加預設前綴 `api/`

| 類別名 | 去後綴 | 最終路由前綴 |
|------|--------|-------------|
| `GameAppService` | `GameApp` | `/api/gameApp` |
| `SystemUserService` | `SystemUser` | `/api/systemUser` |
| `GameOrderService` | `GameOrder` | `/api/gameOrder` |

方法名同理拼到後面：`Page()` → `/api/gameApp/page`，`ResetSecret()` → `/api/gameApp/resetSecret`。

> **URL 裡嚴禁出現 `Service` 後綴**。這不是建議，是硬規則——前端按這個約定產生的調用，你後端類別名一變，前端全炸。

需要精細控制時，`[HttpPost]`、`[AllowAnonymous]`、`[NonAction]` 這些特性照常用。

### 依賴注入：打標記就註冊

不用手寫 `services.AddScoped<>()`，靠標記介面自動註冊：

| 介面 | 生命週期 | 什麼時候用 |
|------|----------|----------|
| `ITransient` | 瞬態 | 每次 resolve 都是新物件，大多數服務用這個 |
| `IScoped` | 作用域 | 一次請求內複用 |
| `ISingleton` | 單例 | 全域唯一，放有狀態的東西要小心 |

基礎設施元件（SqlSugar、JWT、SignalR、EventBus 這些）在 `GameFrameX.Web.Core/Startup.cs` 裡手動註冊，因為它們要組態參數。

### 倉儲與多資料庫路由

`SqlSugarRepository<T>` 繼承自 SqlSugar 的 `SimpleClient<T>`，建構函式會根據實體上的特性決定把資料存到哪個庫：

| 實體特性 | 路由目標 |
|----------|----------|
| `[SystemTable]` | 主資料庫 |
| `[LogTable]` | 日誌資料庫 |
| `[Tenant]` | 租戶獨立資料庫 |
| 無標記 | 按 JWT Claim 裡的 TenantId 路由 |

你只管寫實體、打標記，剩下路由的事倉儲替你做了。

### 認證體系：JWT 共用，外加簽名

**JWT Bearer（三個主機共用同一份金鑰）**

- `JwtHandler` 基於 ASP.NET Core 授權處理器實作（Admin API）
- 三者讀同一份 `JWTSettings`——所以 **Admin 登入簽發的 token，Client API 和 Hub API 也直接認**
- 自動續簽：Admin API 的回應標頭 `X-Authorization` 帶新 Token，前端靜默換發
- 權限校驗：SuperAdmin 繞過所有檢查；普通用戶按路由名匹配按鈕權限

**HMAC Signature（開放 API 用）**

- `SignatureAuthenticationHandler` 實作 HMAC-SHA256 簽名認證
- 請求標頭要帶四個：`accessKey` + `timestamp` + `nonce` + `sign`
- 驗簽公式：`HMACSHA256(secret, "METHOD&URL&accessKey&timestamp&nonce")`
- 防重放：nonce 快取 + 時間偏差校驗（預設允許 5 分鐘偏差）

### 多租戶與軟刪除：查詢自動過濾

- `ITenantIdFilter`：查詢自動附加 `TenantId` 過濾，租戶之間資料天然隔離
- `ISafeDeletedFilter`：查詢自動附加 `IsDeleted == false`，刪除只是打個標記
- SuperAdmin 繞過所有過濾，所以超管能看見全部資料（**但 Hub API 會拒絕 SuperAdmin，見下**）

這兩條是「你寫業務幾乎感覺不到、但它一直在幫你兜底」的那種機制。

### 稽核欄位：自動填充，不用手寫

SqlSugar AOP 在 `SqlSugarSetup.SetDbAop()` 裡幫你幹了這事：

| 操作 | 自動填的欄位 |
|------|-------------|
| Insert | `CreatedTime`、`CreatedId`、`CreatedName`、`TenantId` |
| Update | `UpdateTime`、`UpdatedId`、`UpdatedName` |

同時順手把資料變更寫進稽核日誌（`SystemLogDiff` / `TenantLogDiff`）。所以你建實體的時候，稽核欄位不用自己賦值，框架替你填。

### 雪花 ID

分散式唯一 ID 用 Foundation Snowflake 的**緊湊短 ID 佈局**產生（2026-09-15 決策凍結，見 `gfx-doc/decisions/2026-09-15-compact-snowflake-id.md`）：

| 佈局參數 | 值 | 說明 |
|---|---|---|
| epoch | 2026-09-01 | 部署紀元，有資料落庫後不可改 |
| WorkerId | 3 位（0-7） | 唯一的實例區分維度 |
| DataCenterId | 0 位 | 單資料中心，維度已移除 |
| 序列 | 10 位 | 每 WorkerId 毫秒 1024 個 |

產出 13-14 位數字（< 2^53，JS 裡不丟精度），Admin 頁面直接展示主鍵不用轉字串。佈局已凍結：改 epoch 或位寬 = 與歷史 Id 空間重疊，等於資料完整性事故。

多實例防撞**不靠 Redis**（處理程序內租約無法跨處理程序互斥，那套已退役），靠顯式組態互斥：同庫寫入的每個實例（Web.Api 多副本 / Hub / Client）必須互斥組態 `SnowFlake:WorkerId`（環境變數 `SnowFlake__WorkerId`，0-7），越界啟動期直接失敗——單資料中心同庫實例上限 8。預設值 Web.Api=1（appsettings）/ Hub=2 / Client=3（程式碼兜底），單機全預設部署互不衝突。

SqlSugar 的主鍵回填取號也統一走這套：`StaticConfig.CustomSnowFlakeFunc` 由 `SqlSugarSnowflakeFallbackInitializer`（ModuleInitializer）在 Core.Extension 模組載入時處理程序級安裝，任何路徑（包括 Hub / Client 和測試處理程序）都不會退回 SqlSugar 內建的 18-19 位長佈局。

### 全域回應包裝

Admin API 的 `AdminResultProvider` 把所有回應統一套了一層殼：

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

前端就認這個結構，`Code === 200` 放行，其它走錯誤處理。Hub API 用的是自己那套 `HttpJsonResultData<T>`（`Success` / `Fail` 工廠方法），結構類似但獨立，不經過 `AdminResultProvider`。

### SignalR 即時通訊

- Hub：`OnlineUserHub`，路徑 `/hubs/onlineUser`
- 按租戶分組（`GROUP_ONLINE_{tenantId}`），A 租戶的訊息不會飄到 B 租戶
- 能幹：線上用戶列表、強制下線、訊息發送（單播 / 廣播 / 排除自己 / 指定用戶）
- 多實例部署時可選 Redis Backplane + Data Protection Key 持久化，讓訊息跨實例推送

### Hub API：第三張門

Hub API（`GameFrameX.Hub.Api`，埠 `20806`）是給**外部系統**用的聚合接口，定位和 Admin API、Client API 都不同。它服務這幾類調用方：

| 調用方 | 幹啥 | 對應 Controller |
|--------|------|-----------------|
| 遊戲端 / 營運工具 | 拉區服、查資源版本、查道具、核銷兌換碼 | `GameAreaController` / `GameAssetPackageVersionController` / `GameGoodsController` / `GameRedeemCodeController` |
| CI / 打包機 | 打包完成後回寫資源版本 / 整包版本 | `PackageBuildReportController` |
| 第三方平台 | 接收 Tapd 的 WebHook 回調 | `TapdWebHookController` |

它有幾個和別人不一樣的地方，寫程式碼前一定要知道：

**1. 傳統 Controller 路由，不走動態 API。**
基類 `BaseController` 標了 `[Route("api/[controller]/[action]")]`，路由是手寫的，不靠類別名自動產生。所以這裡的 URL **不會**自動去掉 `Service` 後綴——直接按 `Controller` 名 + `Action` 名拼。

**2. Controller 直接用 `Repository<T>`，不調 Application Service。**
這是 Hub.Api 的鐵律。Controller 建構函式注入倉儲，自己查、自己寫、自己查重、自己校驗歸屬。PackageBuildReportController 的註釋裡寫得很直白：「遵循 Hub.Api Controller 慣例（Repository 直接操作 Entity，不調 Application Service），故未提層 ScopeValidator / 未複用 Application.Client 的 CheckXxxUnique——Controller 自寫歸屬與查重。」

> 含義：你給 Admin API 提的那些 Validator、Application 層的查重方法，**對 Hub.Api 是死程式碼**，別指望 Hub 能複用到。Hub 要校驗就自己在 Controller 裡寫。

**3. 三種鑑權場景，各有各的走法。**

| 場景 | 怎麼鑑權 | 租戶怎麼來 | 範例 |
|------|----------|-----------|------|
| 登入用戶 | JWT（`BaseController` 預設 `[Authorize]`） | 從 JWT 解析 | 區服 / 資源 / 道具 / 兌換碼查詢 |
| 匿名 WebHook | `[AllowAnonymous]` | 從請求標頭來 | `TapdWebHookController` |
| 機器對機器 | `[AllowAnonymous]` + `TenantGameAppOpenAccessAuthMiddleware` | 憑證綁定的 App | `PackageBuildReportController`（打包上報） |

機器憑證那套校驗的是 AccessKey + 簽名 + scope，**不是 JWT**，所以 Controller 標 `[AllowAnonymous]` 跳過 JWT，由專用中介軟體接管。

**4. 租戶靠中介軟體統一解析，且拒絕超級管理員。**
`HubTenantMiddleware`（除了 `/health` 都走）把租戶 Id 統一寫進 `HttpContext.Items`：鑑權請求從 JWT 取，匿名 WebHook 從請求標頭來。然後 `DelegateSqlSugarTenantIdResolver` 把這個 Id 餵給 SqlSugar 做租戶路由。**SuperAdmin 會被直接拒絕**——Hub 是租戶域的服務，平台級超管不該借 Hub 鑽進某個租戶的資料。

**5. 自帶樂觀鎖中介軟體和健康檢查。**
`OptimisticConcurrencyMiddleware` 在管道最前面，給需要並發控制的寫操作兜底。健康檢查端點有三個：`/health`、`/health/live`、`/health/ready`（compose 用它做存活探測）。

Hub API 的中介軟體管道順序（`Program.cs`）：

```
OptimisticConcurrencyMiddleware
  → Swagger（開關控制）
  → CORS
  → Authentication → Authorization
  → HubTenantMiddleware（/health 除外；寫租戶 Id；拒絕超管）
  → TenantGameAppOpenAccessAuthMiddleware（僅 /api/PackageBuildReport）
  → HealthChecks / MapControllers
```

### 組態檔：獨立 JSON，按需載入

Admin API 的獨立 JSON 組態都在 `GameFrameX.Application/Configuration/`，啟動時自動掃：

| 檔案 | 管什麼 |
|------|--------|
| `Database.json` | 資料庫連線、Code-First 開關、種子資料開關 |
| `Swagger.json` | Swagger / Knife4j UI 組態 |
| `SMS.json` | 簡訊服務組態 |
| `Upload.json` | 檔案上傳、OSS 組態 |
| `Wechat.json` | 微信公眾號 / 小程式組態 |
| `Enum.json` | 列舉掃描組件 |

有兩個「已經搬家」的組態要留意，別再到處找它們了：

> 平台 JWT 組態已經搬進主庫 `SystemConfig` 的 `platform_jwt_settings`，**不再用 `JWT.json` 作為執行時來源**（Hub API 仍讀自己 `appsettings.json` 裡的 `JWTSettings` 節點，但金鑰要和 Admin 一致）。
>
> `App.json` 已經退役：監聽埠、動態 API、CORS、雪花 ID 這些主機和框架組態，搬進了 `Web.Entry/appsettings.json`；Cryptogram 金鑰改成環境變數 `GFX_CRYPTOGRAM_*` + 內建預設（見 `CryptogramSecretResolver`）；密碼強度策略搬進主庫 `SystemConfig` 的 `platform_password_policy_config`；區服 Server HTTP 組態搬到了區服級 DB 組態。

---

## 功能模組一覽

> 這裡的服務主要跑在 Admin API 上（動態 API）。Hub API 的接口是獨立的一套傳統 Controller，見 [Hub API：第三張門](#hub-api第三張門)。

### 系統管理

基於專案自有動態 API 路由的完整後台管理模組。這些是「不管做什麼遊戲都得有」的那部分。

| 功能 | 服務類別 | 幹啥的 |
|------|--------|--------|
| 認證登入 | `SystemAuthService` | 帳號密碼、手機驗證碼登入，簽發 JWT + 自動續簽，驗證碼 |
| 用戶管理 | `SystemUserService` | 用戶增刪改查、密碼重置、用戶-角色綁定（注意：這個服務實際管的是 `TenantUser`，命名有點誤導） |
| 角色管理 | `TenantRoleService` | 角色 CRUD、資料範圍控制、角色-選單權限綁定 |
| 選單權限 | `GlobalMenuService` | 樹狀選單、按鈕級權限解析、路由與 API 映射 |
| 組織機構 | `TenantOrgService` | 樹狀組織 CRUD、角色-組織資料範圍 |
| 崗位管理 | `TenantPosService` | 崗位 CRUD，歸屬校驗 |
| 租戶管理 | `SystemTenantService` | 多租戶 CRUD、獨立資料庫連線組態 |
| 字典管理 | `GlobalDictTypeService` / `GlobalDictDataService` | 字典類型與字典值 CRUD、狀態切換 |
| 檔案管理 | `TenantFileService` | 檔案上傳下載、OSS 物件儲存 |
| 定時任務 | `SystemJobService` | 作業與觸發器 CRUD、暫停 / 啟動 / 立即執行、叢集協調 |
| 資料庫管理 | `SystemDatabaseService` | 表 / 欄 CRUD、庫表結構線上管理 |
| 通知公告 | `TenantNoticeService` | 公告發布、SignalR 廣播、已讀 / 未讀追蹤 |
| 線上用戶 | `TenantOnlineUserService` | 即時線上列表、強制下線、單點登入 |
| 日誌系統 | `LogOperationService` / `LogExceptionService` / `LogVisitService` / `LogDiffService` | 操作日誌、異常日誌、存取日誌、資料變更稽核，支援 Excel 匯出 |
| 快取管理 | `SystemCacheService` | 統一快取抽象（Memory / Redis），快取鍵管理 |
| 系統組態 | `SystemConfigService` | 鍵值對組態 CRUD |
| 列舉服務 | `SystemEnumService` | 反射取得組件裡的列舉類型和值 |
| 常量服務 | `SystemConstService` | 取得 `[ConstAttribute]` 標註的常量值 |
| 簡訊服務 | `SystemSmsService` | 阿里雲簡訊驗證碼，60 秒 TTL |
| 地區管理 | `SystemRegionService` | 行政區劃 5 級樹，支援從統計局同步 |
| 列印模板 | `TenantPrintService` | 列印模板 CRUD |
| 開放接口 | `SystemOpenAccessService` | AccessKey / Secret 管理、HMAC 簽名認證 |
| 外掛管理 | `SystemPluginService` | 執行時編譯 C# 程式碼，動態註冊成 API 端點 |
| 微信 | `SystemWechatService` / `SystemWxOpenService` | 公眾號 OAuth2、小程式登入、模板訊息、JS-SDK |
| 微信支付 | `SystemWechatPayService` | JSAPI 下單（直連 / 服務商）、支付回調 |
| 伺服器監控 | `SystemServerService` | 主機資訊、CPU / 記憶體、磁碟、已載入組件 |

### 遊戲管理

面向遊戲營運的業務模組，所有服務繼承 `GenericCrudService` 基類（寫一個 service 自動有六個接口，後面會講）。

| 功能 | 服務類別 | 幹啥的 |
|------|--------|--------|
| 遊戲帳號 | `GameUserEntityService` | 郵箱註冊、封禁 / 解封 |
| 遊戲角色 | `GameRoleEntityService` | 角色管理、沙盒模式切換、從遊戲服同步資料 |
| 遊戲區服 | `GameAreaEntityService` | 區服管理、Docker 容器生命週期（建立 / 啟動 / 停止 / 重啟） |
| 區服分類 | `GameAreaClassifyEntityService` | 區服叢集分類 |
| 訂單管理 | `GameOrderEntityService` | 支付訂單、多維度篩選（狀態 / 角色 / 區服 / 渠道 / 平台 / 日期） |
| 訂單發貨 | `GameOrderShipService` | 共享發貨邏輯，HTTP 調遊戲服 API 發道具 |
| 遊戲郵件 | `GameMailEntityService` | 郵件發送、道具附件解析、收件人校驗 |
| 兌換碼 | `GameRedeemCodeEntityService` | 兌換碼管理、道具附件解析 |
| 兌換碼渠道 | `GameRedeemCodeChannelEntityService` | 兌換碼投放渠道 |
| 兌換記錄 | `GameRedeemCodeUseRecordEntityService` | 兌換碼使用記錄 |
| Web 商城 | `GameWebShopEntityService` | Web 商城商品管理 |
| 商城訂單 | `GameWebShopOrderEntityService` | Web 商城訂單 |
| 商品管理 | `GameCommodityEntityService` | 商品 / 產品管理 |
| 道具物品 | `GameGoodsEntityService` | 遊戲內物品 / 道具 |
| 物品目錄 | `GameGoodsCatalogueEntityService` | 物品分類 |
| 定時活動 | `GameTimeJobEntityService` | 遊戲內定時任務 / 活動 |
| Docker 標籤 | `GameDockerTagEntityService` | 遊戲服 Docker 映像版本管理 |
| 公告 | `GameAnnouncementEntityService` / `GameBannerAnnouncementEntityService` | 遊戲公告、輪播公告 |
| 首頁組態 | `GameHomeEntityService` | 遊戲首頁組態 |

### 應用中心

面向客戶端 App 的組態管理模組。

| 功能 | 服務類別 | 幹啥的 |
|------|--------|--------|
| 應用版本 | `GameAppReleaseVersionService` | App 版本管理、強制更新控制 |
| 資源包 | `GameAppAssetPackageService` | AssetBundle / 資源包管理 |
| 資源版本 | `GameAppAssetPackageVersionService` | 資源包版本追蹤 |
| 資源歷史 | `GameAppAssetPackageVersionHistoryService` | 資源版本變更記錄 |
| 渠道定義 | `GlobalChannelDefinitionService` / `TenantChannelDefinitionService` | 分發渠道（App Store、Google Play 等） |
| 設備管理 | `TenantDeviceService` | 設備註冊與識別（位於 `Application.Tenant`） |
| 設備分組 | `TenantDeviceGroupService` | 設備分組（灰度 / A/B 測試，位於 `Application.Tenant`） |
| 多語言 | `GlobalLanguageService` | 支援語言組態 |
| 分包管理 | `GameAppPackageService` | 應用分包、渠道綁定與母包繼承管理 |
| 平台管理 | `GlobalPlatformService` | 執行平台（iOS、Android 等） |
| 版本定義 | `GameAppVersionDefinitionService` | 版本資源映射 |
| 客戶端組態 | `GameAppClientConfigService` | 客戶端鍵值對組態 |
| 組態鍵 | `GameAppClientConfigKeyService` | 客戶端組態內容鍵 |

### Hub API 的接口（獨立一套）

這些是傳統 Controller，跑在 Hub API（20806）上，**不走動態 API、不調 Application Service**：

| 接口 | Controller | 鑑權 | 幹啥 |
|------|-----------|------|------|
| 區服查詢 | `GameAreaController` | JWT | 給遊戲端拉區服列表 |
| 資源包版本 | `GameAssetPackageVersionController` | JWT | 查資源版本、帶樂觀鎖更新 |
| 道具查詢 | `GameGoodsController` | JWT | 查遊戲內道具 |
| 兌換碼 | `GameRedeemCodeController` | JWT | 兌換碼核銷相關 |
| 打包上報 | `PackageBuildReportController` | 機器憑證 | CI / 打包機回寫資源版本 / 整包版本（冪等 upsert） |
| Tapd WebHook | `TapdWebHookController` | 匿名（請求標頭取租戶） | 接收 Tapd 回調 |

### 支付整合

只有 Client API 用。抽象層 + 多渠道實作，加新渠道照著抽象層實作一遍就行：

| 渠道 | 專案 |
|------|------|
| 支付寶 | `GameFrameX.Pay.AliPay` |
| Apple IAP | `GameFrameX.Pay.Apple`（內購收據驗證） |
| App Store Server | `GameFrameX.Pay.Apple.AppStoreServer`（API v2 通知處理） |
| Google Play | `GameFrameX.Pay.Google` |
| Xsolla | `GameFrameX.Pay.Xsolla` |

---

## 新增一個 CRUD 服務

> 這一節針對 **Admin API** 的動態 API 服務。Hub API 是傳統 Controller，不套這套——加 Hub 接口直接建 Controller + 注入倉儲就行，參考 `GameFrameX.Hub.Api/Controllers/` 裡現有的。

跟著做一遍，你就掌握了 80% 的日常後端活兒。假設要加一個「遊戲道具」。

### 1. 建實體

在 `GameFrameX.Entity/` 對應子目錄裡建實體類別：

```csharp
[SugarTable(null, "遊戲道具")]
[SystemTable]
[IncrementTable]
public class GameItemEntity : EntityBasic
{
    [SugarColumn(ColumnDescription = "道具名稱", Length = 64)]
    public string Name { get; set; }

    [SugarColumn(ColumnDescription = "道具數量")]
    public int Count { get; set; }
}
```

實體基類按需挑：

| 基類 | 特點 | 適用場景 |
|------|------|----------|
| `EntityBaseId` | 只有 `Id` | 關聯表、無租戶實體 |
| `EntityTenantBase` | Id + 租戶 + 稽核 + 軟刪除 | 系統表（外部引用） |
| `EntityBasic` | 繼承 `EntityTenantBase` | 通用業務實體（最常用） |
| `EntityBasicData` | 再加組織資料權限 | 需按組織過濾的資料 |
| `EntityBasicSelect` | 再加 Name + Description | 下拉選擇器實體 |

### 2. 建種子資料（可選）

在 `GameFrameX.Application/SeedData/` 對應子目錄建種子類別：

```csharp
public class GameItemSeedData : ISqlSugarEntitySeedData<GameItemEntity>
{
    public IEnumerable<GameItemEntity> HasData()
    {
        return new[]
        {
            new GameItemEntity { Id = YitIdHelper.NextId(), Name = "金幣", Count = 0 }
        };
    }
}
```

種子資料用 `StorageableByObject` 按主鍵 InsertOrUpdate，所以反覆執行也安全。**注意**：因為是按主鍵 Upsert，如果你改了種子的內容但主鍵沒變，舊記錄不會被覆蓋更新——要看到新值得刪掉舊行再重啟。

### 3. 寫 Service

在 `GameFrameX.Application.Game/Service/`（或 System / Client / Tenant）下建服務：

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

收工。你現在自動擁有六個接口：

| 端點 | 幹啥 |
|------|------|
| `Page` | 分頁查詢 |
| `Add` | 新增 |
| `Update` | 修改 |
| `Get` | 按 Id 查單條 |
| `List` | 列表（不分頁） |
| `Delete` | 刪除（軟刪除） |

如果還要下拉選擇器支援，把基類換成 `GenericCrudSelectService`，額外白嫖 `Select` 和 `CheckNameUnique` 兩個端點。

### 4. （需要的話）自訂 DTO

簡單場景直接複用基類那幾個：`BaseAddInput`、`BaseUpdateInput`（帶 `Id`）、`BaseIdInput`、`BasePageInput`（帶 `SearchKey`、`Page`、`PageSize`、排序欄位）。

欄位不夠用時，在服務的 `Dto/` 子目錄或 `GameFrameX.Core.Extension/Dto/` 裡加：

```csharp
// 輸出 DTO——繼承實體即可
public class GameItemOutput : GameItemEntity, IBasePageOutput<GameItemEntity> { }

// 更新 DTO——繼承基類再加欄位
public class UpdateGameItemInput : BaseUpdateInput
{
    [Required] public string Name { get; set; }
}
```

### 5. 告訴前端

後端 OpenAPI 變了之後，前端要重新匯出 `openapi.json` 跑 `pnpm gen:api`，不然前端調用點對不上。具體步驟見 [匯出 OpenAPI 給前端](#匯出-openapi-給前端)。

---

## 開發規範

> 這一節是**約定**——專案裡大家都這麼幹，照著來省心。

### Admin API vs Hub API：別把兩套混了

| 維度 | Admin API | Hub API |
|------|-----------|---------|
| 路由 | 動態 API（`IDynamicApiController`） | 傳統 Controller（`[Route("api/[controller]/[action]")]`） |
| 資料存取 | 經 Application Service → 倉儲 | Controller 直接注入 `Repository<T>` |
| 回應包裝 | `AdminResultProvider` 自動套殼 | 手動返回 `HttpJsonResultData<T>` |
| 複用 Application 層 | 是 | 否（Validator / CheckXxxUnique 對 Hub 是死程式碼） |
| 加新接口 | 寫 Service | 建 Controller + 注入倉儲 |

給 Hub 加接口時，**別**想著複用 Admin 那邊提層的校驗——Hub 自己寫。

### 倉儲擴充方法：先找輪子再造輪子

`GameFrameX.Core.Extension/RepositoryExtension.cs` 裡已經備好常用的了（Hub API 的 `Repository<T>` 也能用其中一部分，比如帶樂觀鎖的更新）：

| 方法 | 幹啥的 |
|------|--------|
| `FakeDelete` / `FakeDeleteAsync` | 軟刪除（`IsDeleted = true`） |
| `OrderBuilder(input)` | 根據 `BasePageInput` 動態排序 |
| `UpdateWithDiffLog` / `InsertWithDiffLog` | 帶稽核日誌的增刪改 |
| `UpdateWithOptimisticLockAsync` | 帶樂觀鎖的更新（Hub API 打包上報在用） |
| `AS<T>()` | 跨庫查詢 / 操作 |
| `RunWithoutFilter` | 臨時跳過查詢過濾器（比如要查已刪除的資料） |

### 定時任務

實作 `IJob` 介面，然後要麼在管理後台的「定時任務」頁面配，要麼用 `[JobDetail]` 特性標註：

```csharp
[JobDetail("job_my_task", Description = "我的定時任務")]
public class MyTask : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken ct)
    {
        // 你的任務邏輯
    }
}
```

任務支援三種類型：內建類型、指令碼編譯、HTTP 調用。透過 `DbJobPersistence` 持久化到資料庫，所以多實例部署時它能協調好，不會幾個實例同時跑同一個任務。

> 注意：訂單發貨那塊的處理程序鎖在多實例下會失效，這塊還在評估，暫時別指望它能跨實例互斥。

### 中介軟體管道：請求是怎麼被一層層處理的

Admin API 的請求處理順序長這樣（從外到內）：

```
異常處理 / HSTS
  → 統一錯誤狀態碼
  → 本地化
  → 靜態檔案
  → 路由
  → CORS
  → IP / 客戶端限流
  → 認證 / 授權
  → Schedule UI
  → Knife4j (Swagger)
  → Project DI
  → SignalR Hub + MVC 路由
```

Hub API 的管道不一樣，見 [Hub API：第三張門](#hub-api第三張門)。排查請求問題時，按對應主機的管道順序想想「哪一層把它擋了」，比無腦重啟管用。

### C# 程式碼風格

這個專案按 **C# 10 語法**、**.NET 10 執行時**（`Directory.Build.props`：`LangVersion=10` + `net10.0`）。可以用 record、target-typed new、file-scoped namespace、global using。但有一條鐵律：**所有控制流語句必須帶大括號**，哪怕 body 只有一行。`using var x = ...;` 這種單行 using 宣告也別用，老老實實寫 `using (var x = ...) { }`。

---

## 常見問題與踩坑

### FAQ

**為什麼我寫了 Service 卻調不到接口？**
檢查三件事：類別有沒有實作 `IDynamicApiController`、方法是 public 嗎、有沒有被 `[NonAction]` 標了。再不行看啟動日誌，路由註冊時會列印。（這只針對 Admin API——Hub API 是手寫 Controller，沒有動態路由這回事。）

**前端調的 URL 和我的類別名對不上？**
那就是忘了「去 Service 後綴 + 首字母小寫」的規則（Admin API）。`SystemUserService` 的 URL 是 `/api/systemUser/...`，不是 `/api/systemUserService/...`。前端 URL 嚴禁帶 `Service`。Hub API 不走這套，URL 按 Controller 名 + Action 名拼。

**改了實體，表結構沒更新？**
Code-First 預設只建表、加欄。刪欄、改欄類型這種破壞性變更，可能要手動處理或重啟時留意。生產環境改表前先備份。

**SuperAdmin 看到的資料比別人多，是不是 bug？**
不是。SuperAdmin 繞過所有租戶過濾和軟刪除過濾，所以能看見全部。這是設計如此。**但 Hub API 會拒絕 SuperAdmin 存取**——Hub 只服務租戶域。

**種子資料改了，重啟後還是舊的？**
種子按主鍵 Upsert。主鍵沒變的話，已有記錄不會被覆蓋。刪掉那行再重啟，或者換個主鍵。

**Hub API 報 401，但同一個 token 調 Admin API 是好的？**
Hub 複用 Admin 的 JWT 金鑰，但讀的是自己 `appsettings.json` 裡的 `JWTSettings` 節點。兩邊金鑰（`IssuerSigningKey` / `ValidIssuer` / `ValidAudience`）必須一致。不一致就簽出來的 token 互相不認。

**打包上報（PackageBuildReport）報 401？**
那不是 JWT 鑑權，是機器憑證（AccessKey + 簽名 + scope）。走的是 `TenantGameAppOpenAccessAuthMiddleware`，憑證要先在 Admin 後台的開門接口管理裡建好、綁到對應 App。

### 踩坑

**多資料庫路由不生效**
檢查實體上的特性有沒有打對：`[SystemTable]` / `[LogTable]` / `[Tenant]`。沒打標記的實體預設按 JWT Claim 裡的 TenantId 路由，沒登入上下文時就路由到主庫。

**稽核欄位是空的**
稽核靠 SqlSugar AOP 自動填充。如果你繞過倉儲直接操作 `SqlSugarClient`，AOP 可能不走，欄位就空了。老老實實用 `SqlSugarRepository<T>`。（Hub API 直接用倉儲，是 OK 的。）

**JWT 一直過期 / 續簽不生效**
看回應標頭有沒有 `X-Authorization`。沒有的話檢查 `JwtHandler` 的續簽邏輯和 token 有效期組態。平台 JWT 組態已經搬進主庫 `SystemConfig` 的 `platform_jwt_settings`，別再去 `JWT.json` 裡改。

**給 Hub 加接口，結果校驗邏輯和 Admin 對不上**
正常。Hub 不複用 Admin 的 Application 層校驗。你要麼在 Hub Controller 裡手寫，要麼接受兩邊校驗邏輯各寫一份——這是 Hub 當前的明確取捨。

**定時任務在多實例下重複執行了**
任務靠資料庫 + 叢集協調。確認 `DbJobPersistence` 工作正常、叢集協調組態沒漏。訂單發貨那塊的處理程序鎖多實例下確實會失效，已知的坑。

---

## 部署

### Docker Compose（推薦）

```bash
cd .. && docker compose up --build   # 統一編排位於倉儲根（含前端共 5 服務）
```

一條命令把三個主機全拉起來。埠映射和本地 `dotnet run` **完全一致**：

| compose 服務 | 容器名 | 埠 | 對應主機 | Dockerfile |
|--------------|--------|------|----------|-----------|
| `web-api` | `gameframex-web-api` | `20802:20802` | Admin API | `Dockerfile.web-api` |
| `client-api` | `gameframex-client-api` | `20808:20808` | Client API | `Dockerfile.client-api` |
| `hub-api` | `gameframex-hub-api` | `20806:20806` | Hub API | `Dockerfile.hub-api` |

三個 Dockerfile 都是多階段構建（`sdk:10.0` 打包 → `aspnet:10.0` 執行），以非 root 用戶跑，時區設成 `Asia/Shanghai`。組態檔透過 volume 掛載，所以**鏡像構建完還能改組態**，不用重新 build。三個容器共用宿主機那個 PostgreSQL（走 `host.docker.internal:5432`），不再單獨起資料庫容器。

> Swagger 在 Production 容器裡預設不掛，三個主機都用 `Swagger__Enabled=true` 顯式打開（見 compose 的環境變數段）。

> 歷史命名：compose 服務曾名 `web-entry`（沿用舊版 "Web.Entry"），2026-10-20 起更名 `web-api`，與 GameFrameX.Web.Api 對齊。

### 發布前清單：資料庫重灌與緊湊短 ID

緊湊短 ID 佈局的發布口徑是**可重置、無遷移指令碼**（決策 2026-09-15）：新舊 ID 佈局不相容，生產 PG 主庫 / 租戶庫按「清空重灌」發布，不允許新舊 ID 混寫。發布前過一遍：

1. **清空重灌**：生產 PG 主庫 + 全部租戶庫清空，由各 host 啟動重建（CodeFirst 建表 + 種子資料重灌）。舊庫裡 18-19 位長 Id 的資料不要往新佈局遷——新舊 Id 空間重疊是資料完整性事故。
2. **WorkerId 互斥核對**：同庫寫入的所有實例（Web.Api 副本 / Hub / Client）`SnowFlake__WorkerId` 兩兩不同且在 0-7。預設值 1/2/3 只保證單機部署不撞；擴副本前必須顯式分配。
3. **量級抽查**：重灌後抽查業務表主鍵應為 13-14 位數字（< 2^53）。看到 18-19 位說明有路徑沒走 Foundation 取號，停下來排查。

---

## 匯出 OpenAPI 給前端

前端那套 TypeScript API 客戶端，是靠 `openapi.json` + `pnpm gen:api` 產生的。**後端只負責把 Swagger JSON 端點暴露出來**，匯出動作在前端倉儲做。

| 項 | 值 | 出處 |
|---|---|---|
| Admin Swagger 文檔名 | `web.api` | `GameFrameX.Web.Core/ApiRouting/AdminOpenApiGroups.cs` |
| Admin Swagger JSON | `http://<host>:20802/swagger/web.api/swagger.json` | `AdminOpenApiGroups.WebGroup.RouteTemplate` |
| Client API Swagger JSON | `http://<host>:20808/swagger/client.api/swagger.json` | `GameFrameX.Client.Api/Program.cs` |
| Hub API Swagger JSON | `http://<host>:20806/swagger/v1/swagger.json` | `GameFrameX.Hub.Api/Program.cs`（`SwaggerDoc("v1", ...)`） |
| **預設開關**（三者一致） | 開發環境自動開；其他環境必須 `Swagger:Enabled=true`（環境變數 `Swagger__Enabled=true`） | 各自 `Program.cs` / `Startup.cs` |
| **生產 / Docker 預設不暴露** | 是——Swagger 中介軟體壓根不掛載，沒有 `/swagger/...` 端點 | 同上 |

> Hub API 的 Swagger 文檔名是 `v1`（不是 `hub.api`），所以 URL 是 `/swagger/v1/swagger.json`。Admin 前端一般只用 Admin API 的 `web.api`，Hub 和 Client 的 swagger 主要是給遊戲端 / CI 整合時查接口用。

生產預設不掛 Swagger 是故意的——別把接口文檔往公網裸奔。

```bash
# 1. 讓 Admin API 的 Swagger 可存取（二選一）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api &
# 或
Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api &

# 2. 拉到前端倉儲根
curl -sSL http://localhost:20802/swagger/web.api/swagger.json \
  -o ../GameFrameX.Admin.Unified.Vue/openapi.json
ls -lh ../GameFrameX.Admin.Unified.Vue/openapi.json   # 大概 3 MB 才對

# 3. 前端跑產生
( cd ../GameFrameX.Admin.Unified.Vue && pnpm gen:api && git diff src/api/ )
```

> 後端 OpenAPI 一旦變更（新增 / 修改 / 刪除 service 端點），必須重新匯出 + 讓前端 `pnpm gen:api`，否則前端調用點和後端對不上。前端 `pnpm run quality` 裡的 `check:routes` 會用後端選單種子對齊前端組件路徑，對不上會報。

---

## 命令速查

| 命令 | 幹嘛的 |
|------|--------|
| `dotnet run --project GameFrameX.Web.Api` | 啟動 Admin API（20802） |
| `dotnet run --project GameFrameX.Client.Api` | 啟動 Client API（20808） |
| `dotnet run --project GameFrameX.Hub.Api` | 啟動 Hub API（20806） |
| `dotnet build` | 編譯整個解決方案 |
| `Swagger__Enabled=true dotnet run --project GameFrameX.Web.Api` | 非 dev 環境啟動並暴露 Swagger |
| `ASPNETCORE_ENVIRONMENT=Development dotnet run --project GameFrameX.Web.Api` | dev 環境啟動（Swagger 自動開） |
| `cd .. && docker compose up --build` | Docker 一把起（倉儲根統一編排：PG + 三 API + 前端） |
| `curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o ../GameFrameX.Admin.Unified.Vue/openapi.json` | 匯出 Admin Swagger JSON 給前端 |

常用地址：

| 地址 | 是啥 |
|------|------|
| `http://localhost:20802/kapi` | Admin API 的 Knife4j / Swagger UI |
| `http://localhost:20802/swagger/web.api/swagger.json` | Admin Swagger JSON（docName=`web.api`） |
| `http://localhost:20806/swagger` | Hub API 的 Swagger UI |
| `http://localhost:20806/swagger/v1/swagger.json` | Hub Swagger JSON（docName=`v1`） |
| `http://localhost:20806/health` | Hub API 健康檢查 |
| `http://localhost:20808` | Client API |
| `/hubs/onlineUser` | Admin API 的 SignalR 線上用戶 Hub |

---

## 寫在最後

口訣：

> **三個主機各管各的：Admin 寫 Service 就是寫接口，Client 管登入支付，Hub 是對外傳統 Controller。寫實體就是建表，URL 不帶 Service，多租戶自動兜底。Hub 不複用 Admin 的分層，自己寫校驗。改了 OpenAPI 別忘了喊前端重新產生。**

卡住了別死磕：先看上面的踩坑，沒解決就翻 `gfx-doc/`（專案根的那套文檔系統），再不行問同事。

想看具體程式碼就跳這幾個地方：

- `GameFrameX.Web.Core/Startup.cs` —— Admin 啟動組態、DI 註冊、中介軟體管道
- `GameFrameX.Hub.Api/Program.cs` —— Hub 啟動組態、中介軟體管道、JWT 複用
- `GameFrameX.Hub.Api/Controllers/PackageBuildReportController.cs` —— Hub 「倉儲直接操作、不調 Application Service」的範例
- `GameFrameX.Core.SqlSugar/` —— SqlSugar 組態、AOP、多資料庫路由
- `GameFrameX.Core/` —— 倉儲基類、`GenericCrudService`、認證、SignalR
- `GameFrameX.Core.Extension/RepositoryExtension.cs` —— 倉儲擴充方法（常用輪子）
- `GameFrameX.Application/SeedData/` —— 種子資料
- `GameFrameX.Application/Configuration/` —— 獨立 JSON 組態檔

## 文檔與資源

- 專案文檔站：<https://gameframex.doc.alianblank.com>
- 專案總覽：[`../README.zh-TW.md`](../README.zh-TW.md)
- 前端執行手冊：[`../GameFrameX.Admin.Unified.Vue/README.zh-TW.md`](../GameFrameX.Admin.Unified.Vue/README.zh-TW.md)
- Issue 回報：<https://github.com/GameFrameX/Admin/issues>

## 社區與支援

![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)

## 更新日誌

發布記錄見 [GitHub Releases](https://github.com/GameFrameX/Admin/releases)。

## 開源協議

詳見 [LICENSE.md](LICENSE.md) 檔案。

<!--
EN: See [LICENSE.md](LICENSE.md) for license information.
zh-CN: 详见 [LICENSE.md](LICENSE.md) 文件。
zh-TW: 詳見 [LICENSE.md](LICENSE.md) 檔案。
ja: 詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
ko: 자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
-->
