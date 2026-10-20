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

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761
<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

## 項目簡介

> 一句話定位：**面向獨立遊戲開發者的營運 + 聯運後台平台**。後端 `.NET 10 + SqlSugar` 三主機（Admin / Client / Hub），前端 `Vue 3 + PrimeVue`，從管理員登入到玩家支付回調一條鏈路全棧跑通，開箱即用。

**GameFrameX Admin** 是一套**面向遊戲營運**的後台系統：管理端給營運 / GM / 客服改選單、查訂單、推版本、看日誌；客戶端 API 給遊戲端登入 / 支付 / 拉配置；Hub API 給外部系統（CI 打包、Tapd WebHook、第三方回調）。一套倉庫三套主機，分庫部署互不拖累。

它**不是**通用 OA / CRM / 商城系統。選單、權限、字典、表單、客戶端配置等都是圍繞「遊戲後台」的真實業務場景設計的。

本項目基於 [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET) 二次開發，所有 Furion 框架殘留與原 Admin.NET 業務命名已完成替換與重基線（U5 / U8 / C37–C41）。致謝原作者 [zuohuaijun](https://gitee.com/zuohuaijun) 及其貢獻者。

### 倉庫結構

單倉庫（monorepo）：前後端同一套程式碼庫。2026-10-19 起歷史子倉 `GameFrameX.Admin.Unified.Api` / `GameFrameX.Admin.Unified.Vue` 已凍結唯讀、快照併入本倉，新程式碼只進本倉。

```
Admin/
├── GameFrameX.Admin.Unified.Api/    # .NET 後端：Admin / Client / Hub 三 host + Application / Entity / Core 分層
├── GameFrameX.Admin.Unified.Vue/    # Vue 前端：PrimeVue + Pinia + vue-i18n（5 語言）
├── gfx-doc/                         # 專案文件系統（agent 手冊 / spec / 決策 / 待辦）
├── doc/                             # README 配圖
├── docker-compose.yml               # 一鍵全棧編排（PostgreSQL + 三 API + 前端）
└── .env.example                     # compose 環境變數模板（複製為 .env 後按需覆蓋）
```

## 快速開始

> 假設你要給一個**新遊戲**接一套營運後台。讀完這一節你應該能從 0 到登入、能用 TeamS admin 看見選單樹。

### 準備

- .NET 10 SDK
- Node.js ≥ 20（推薦 20）+ pnpm ≥ 11.5.2（`corepack enable`）
- 沒有資料庫也行——本地源碼直跑預設 SQLite 零配置；生產用 PostgreSQL / MySQL 等 34 種 DbType

### 步驟

```bash
# 1. 取得程式碼（單倉庫，前後端都在內）
git clone https://github.com/GameFrameX/Admin.git
cd Admin

# 2. 啟動後端（在倉庫根目錄！連線字串相對工作目錄解析）
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api     # Admin API → http://localhost:20802
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Client.Api  # Client API → http://localhost:20808
dotnet run --project GameFrameX.Admin.Unified.Api/GameFrameX.Hub.Api     # Hub API → http://localhost:20806

# 3. 啟動前端
cd GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # 預設 http://localhost:20800（hash 路由）
```

打開瀏覽器 → 看到登入頁 → 帳號 `admin` / 密碼 `123456` / 租戶 `1300000000001`（首次啟動 SqlSugar Code-First 會自動建表 + 灌種子）。

### 一條命令全棧（倉庫根統一 compose）

```bash
docker compose up -d --build
```

Compose 內建 PostgreSQL + 三個 host（20802/20806/20808）+ Admin 前端（20800，與 `pnpm run dev` 互斥，起容器前先停 dev server），起完等 healthcheck 變綠即可存取。

環境變數統一入口：複製根目錄 [`.env.example`](.env.example) 為 `.env` 後按需覆蓋（未填寫的變數走 compose 預設值；`.env` 不進 git）。

更細的命令與踩坑見 [`GameFrameX.Admin.Unified.Api/README.zh-TW.md`](GameFrameX.Admin.Unified.Api/README.zh-TW.md)（後端手冊）與 [`GameFrameX.Admin.Unified.Vue/README.zh-TW.md`](GameFrameX.Admin.Unified.Vue/README.zh-TW.md)（前端手冊）。

## 它適合誰 / 不適合誰

| 適合 | 不太適合 |
|---|---|
| 獨立 / 小團隊遊戲，需要一套能跑起來的營運後台 | 你已經有完整後台只想要其中一個模組 |
| 多端（管理 / 客戶端 / 外部系統）需要清晰邊界 | 單端簡單後台（直接用 Admin.NET / vue-next-admin 更輕） |
| 多租戶 SaaS 化遊戲服務 | 單租戶自營不打算對外開放 |
| 強需求 .NET + SqlSugar 技術棧 | 團隊只熟悉 Java / Go / Node |
| 需要多語言（5 語 i18n）+ AI Agent 友善倉庫 | — |
| 想要可演進到微服務的單體起步 | — |

## 技術棧

- **後端**：.NET 10 + ASP.NET Core 原生宿主 + SqlSugar（Code-First，34 種 DbType）+ JWT/HMAC + SignalR + Serilog。
- **前端**：Vue 3.5 + Vite 6 + TypeScript strict + PrimeVue 4.5 + Pinia 3 + vue-i18n（5 語言）+ Axios。

完整選型清單與版本矩陣見 [後端 README · 技術棧](GameFrameX.Admin.Unified.Api/README.zh-TW.md#技術棧) 與 [前端 README · 技術棧](GameFrameX.Admin.Unified.Vue/README.zh-TW.md#技術棧)。

## 三套主機：Admin / Client / Hub

一套倉庫三套主機，分庫部署互不拖累：**Admin API**（20802，管理後台）、**Client API**（20808，遊戲客戶端登入/支付/配置）、**Hub API**（20806，外部系統 / CI / WebHook）。三者共享 Entity 與 SqlSugar 倉儲，依賴只能往下走（`Application.*` / `Core.*` 嚴禁反向引用宿主）。

### 三個主機最大的差異

| 維度 | Admin API | Client API | Hub API |
|---|---|---|---|
| 路由 | 動態 API（寫 Service 自動出介面） | 動態 API | 傳統 Controller（`[Route("api/[controller]/[action]")]`） |
| 資料存取 | 經 Application Service → 倉儲 | 經 Application Service → 倉儲 | Controller 直接注入 `Repository<T>` |
| 複用 Application 層 | 是 | 是 | 否（自己校驗） |
| 加新介面 | 寫 Service | 寫 Service | 建 Controller + 注入倉儲 |
| URL 規則 | 去 `Service` 後綴 + camelCase | 去 `Service` 後綴 + camelCase | `Controller` 名 + `Action` 名 |
| 是否拒絕 SuperAdmin | 否（SuperAdmin 繞過所有過濾） | 否 | **是**——Hub 拒絕超管存取 |

> **URL 裡嚴禁出現 `Service` 後綴**。這是新人 100% 會踩的坑——前端按這個約定生成調用，類名一變，前端全炸。

## 多租戶 + 應用作用域

三層模型，從外到內：

```
SuperAdmin（平台級）── 可存取全域配置、跨租戶查詢
    ↓
Tenant 租戶（多租戶 SaaS 化）── 跨應用共享
    ↓
App 應用（租戶下的具體遊戲 / 產品）── AppId 作用域
```

| Scope | 含義 | 路由示例 |
|---|---|---|
| `platform` | 平台級（超管可見，跨租戶） | `/platform/*` |
| `tenant` | 租戶級（所有應用共有，無 appId query） | `/dashboard/*`、`/system/*` |
| `app` | 應用工作台內（必須帶 `appId` query） | `/gameCenter/*` |

多租戶與資料庫邊界由實體特性自動生效：`[Tenant]` 自動加 `TenantId` 過濾 + 軟刪除；`[SystemTable]` 路由主庫、`[LogTable]` 路由日誌庫、無標記按 JWT `TenantId` 路由。細節見 [後端 README · 倉儲與多資料庫路由](GameFrameX.Admin.Unified.Api/README.zh-TW.md#倉儲與多資料庫路由)。

## 30 個功能模組

按「你會用到的場景」分組，不按目錄結構。

**身份與權限**

| 模組 | 用途 |
|---|---|
| 使用者管理 | 租戶使用者 / 系統管理員維護，可綁定使用者職務、機構、角色、資料權限 |
| 機構管理 | 多層級樹形組織架構 |
| 職位管理 | 使用者職務標籤 |
| 選單管理 | 目錄 / 選單 / 按鈕 三級權限 |
| 角色管理 | 角色綁定選單 + 資料授權範圍 |
| 字典管理 | 全域 / 租戶兩級字典 |
| 開放授權 | AccessKey / Secret + HMAC 簽名認證（外部介面） |

**平台配置（僅 SuperAdmin 可見）**

| 模組 | 用途 |
|---|---|
| 平台租戶管理 | 租戶 CRUD、租戶庫自動開通 / 停用 / 到期 |
| 全域平台 / 語言 / 渠道定義 | 平台級枚舉與多語言基線 |
| 區域管理 | 行政區劃 5 級樹，支援從統計局同步 |
| 系統配置 | 鍵值對；高頻配置遷入主庫 `SystemConfig`（JWT / 密碼策略 / 圖形驗證碼） |
| 資料庫管理 | 表 / 列 CRUD、庫表結構線上管理 |

**系統能力**

| 模組 | 用途 |
|---|---|
| 服務監控 | 主機資訊、CPU / 記憶體 / 磁碟 / 已載入程式集 |
| 線上使用者 | 即時列表 + 強制下線，基於 SignalR |
| 公告管理 | 系統通知 + SignalR 即時推送 |
| 檔案管理 | 本地儲存 + 阿里雲 OSS + 騰訊 COS（多 provider） |
| 定時任務 | `ProjectScheduler` 項目自有調度器；內建 / 腳本編譯 / HTTP 三類；執行時編譯 C# 動態 API |
| 郵件 / 簡訊 | `Mail.Abstract` 多 provider + `Sms.Abstract` Aliyun 實作 |
| 系統介面 | Swashbuckle Swagger；生產預設不掛（需 `Swagger__Enabled=true`） |
| 四類日誌 | 操作 / 異常 / 存取 / 資料變更（欄位級 Diff 稽核），支援 Excel 匯出 |
| 執行時插件 | C# 程式碼動態編譯並註冊為 API 端點 |
| 快取管理 | Memory / Redis 統一抽象 |

**遊戲後台（76 個 Application.Game 服務）**

| 模組 | 用途 |
|---|---|
| 遊戲帳號 / 角色 / 區服 | 玩家帳號、角色管理、Docker 容器生命週期 |
| 訂單 / 發貨 | 多維度篩選；HTTP 調遊戲服 API 發道具 |
| 郵件 / 兌換碼 | 全服 / 單服郵件；兌換碼渠道與投放 |
| Web 商城 | 商品 + 訂單 + 分類 |
| 道具 / 商品 | 道具目錄、消耗品 / 永久品管理 |
| 公告 / 橫幅 | 遊戲內 + 登入後公告 |
| 應用版本 / 資源包 | App 版本 + 強制更新 + 資源包版本與歷史回滾 |
| 應用作用域配置 | 遠端參數、應用校驗、客戶端 KV |
| 郵件營運 | 模板 + Campaign + 多語言 |
| 廣告回調 | TopOn / AdMob / UnityAds 多 provider |
| 遊戲服對接 | 玩家註冊 / 登入 / 訂單回調 |

**整合**

| 整合 | 說明 |
|---|---|
| 微信 | `Wechat.Abstract` + `Wechat.Open` 雙 SDK；公眾號 OAuth2、小程式登入、模板訊息、JS-SDK |
| 微信支付 | JSAPI 下單（直連 / 服務商）+ 支付回調（`Payment.Wechat`） |
| 支付 | 支付寶 / Apple IAP（含 App Store Server API v2）/ Google Play / Xsolla，抽象層 + 多 provider |
| 物件儲存 | Local / Tencent COS（多 provider） |
| 簡訊 | Aliyun 簡訊 Provider |
| 郵件 | Tencent SES / SMTP（多 provider） |

**前端框架特性**

- **後端驅動選單**：選單、權限、文案 全部由後端控制。前端不寫選單，加選單要去後端選單管理裡加；前端只負責渲染後端回傳的選單樹。
- **JWT 自動續期**：登入拿到的 token 過期前自動加 `X-Authorization`，讓後端靜悄悄換發，登入一次能爽很久。
- **應用作用域**：路由 `meta.appScope` + `meta.menuContext` 標注 platform / tenant / app 三種 scope，前端按 scope 決定選單可見性。
- **i18n 完整性門禁**：`pnpm run lint:i18n` 檢查 5 語言鍵完整，缺一個直接 fail。

## 安全基線

不是「建議」，是**預設就生效**的硬約束。

| 威脅 | 應對 |
|---|---|
| 弱密碼 | 全項目統一 **Argon2id**（C214 系統使用者 / C215 玩家）；舊 PBKDF2 僅保留 `AppSecretHasher`（高熵隨機密鑰場景，不需要 Argon2id） |
| SQL 注入 | SqlSugar 全參數化，無字串拼接 |
| 越權存取 | 多租戶過濾 + 軟刪除過濾；SuperAdmin 繞過但 Hub API 拒絕超管 |
| 重放攻擊 | HMAC 簽名 + nonce 快取 + 5 分鐘時間偏差 |
| 資訊洩露 | Response DTO 必含 Id/RowVersion/CreatedTime；敏感欄位（密碼雜湊 / AccessSecret）**禁止**出現在任何 Response；契約測試守護 |
| 介面變更回潮 | `ThreeHostApiRoutingContractTests` + `AdminEndpointReturnContractTests` + `AdminActionStyleContractTests` + `NamingAbbreviationGuardTests` 防回潮 |

## 資料庫

**預設零配置**：本地源碼直跑預設 SQLite（倉庫根 `db/` 下），生產用 PostgreSQL（也支援 MySQL / Oracle 等共 34 種 DbType）。`GameFrameX.Application/Configuration/Database.json` 是單一連線字串來源（三 host 共享，改一處三處生效）。

- 主庫 / 租戶庫 / 日誌庫按實體特性自動路由（見上文多租戶一節）。
- 租戶庫自動建立：按租戶 ID 生成 `gameframex_admin_tenant_<TenantId>`，無需手填連線字串；預設租戶庫同樣自動推導。
- 換庫連線字串寫法見 [`Database-ConnectionStrings.md`](GameFrameX.Admin.Unified.Api/GameFrameX.Application/Configuration/Database-ConnectionStrings.md)；環境變數成對覆蓋（`GFX_MAIN_DB_TYPE` + `GFX_MAIN_DB_CONNECTION_STRING`）等部署細節見 [後端 README · 資料庫](GameFrameX.Admin.Unified.Api/README.zh-TW.md#資料庫預設零配置生產用-postgresql)。

> SQLite 只面向本地開發。生產禁用——併發寫入弱、無獨立服務程序、備份恢復與維運監控能力不足。

## 演示與上手

| 資源 | 連結 |
|---|---|
| 演示站點（**請勿用於自己的生產環境**） | <https://game.admin.web.vue.alianblank.com> |
| 文檔站 | <https://gameframex.doc.alianblank.com> |
| 後端運行手冊 | [`GameFrameX.Admin.Unified.Api/README.zh-TW.md`](GameFrameX.Admin.Unified.Api/README.zh-TW.md) |
| 前端運行手冊 | [`GameFrameX.Admin.Unified.Vue/README.zh-TW.md`](GameFrameX.Admin.Unified.Vue/README.zh-TW.md) |

## 項目倉庫

| 項目 | 用途 | 倉庫 |
|---|---|---|
| 本倉庫 | Admin / Client / Hub 三 host 單倉庫（Cron 化） | [GameFrameX/Admin](https://github.com/GameFrameX/Admin) |
| 客戶端介面（獨立倉庫） | 早期拆分；現統一在本倉庫 | [GameFrameX.Admin.Client.Api](https://github.com/GameFrameX/GameFrameX.Admin.Client.Api) |
| 後台管理介面（付費源碼） | 歷史命名 | [GameFrameX.Admin.Web.Api](https://github.com/GameFrameX/GameFrameX.Admin.Web.Api) |
| 後台 Web 頁面（付費源碼） | 歷史命名 | [GameFrameX.Admin.Web.Vue](https://github.com/AlianBlank/GameFrameX.Admin.Web.Vue) |

## 發布與部署

```bash
docker compose up -d --build
# 內建 PostgreSQL + Admin(20802) + Client(20808) + Hub(20806) + 前端(20800)，起完等 healthcheck 變綠（前端容器與 dev server 同搶 20800，互斥）
```

單容器 `docker run`、環境變數成對覆蓋（半套覆蓋啟動期直接報錯）等細節見 [後端 README · 部署](GameFrameX.Admin.Unified.Api/README.zh-TW.md#部署)。

## 開發與貢獻

想加一個新 CRUD 服務？看 [`GameFrameX.Admin.Unified.Api/README.zh-TW.md` → 加一個新 CRUD 服務](GameFrameX.Admin.Unified.Api/README.zh-TW.md)。Admin API 只要實作 `IDynamicApiController` 就自動有 REST 端點（Page / Add / Update / Get / List / Delete 六個），無需手寫 Controller。

CI 門禁（防回潮）：

- `ThreeHostApiRoutingContractTests` — 三 host 路由 / 謂詞 / 參數綁定 / 錯誤碼語義域
- `AdminEndpointReturnContractTests` — Response DTO 契約
- `AdminActionStyleContractTests` — Admin action 寫法規範
- `NamingAbbreviationGuardTests` + `NamingAbbreviationBaseline.baseline.txt` — 全系統禁業務縮寫

## 功能預覽

### 平台管理（SuperAdmin）

![登入頁](doc/img/1.png)
![主控台](doc/img/2.png)
![選單管理](doc/img/3.png)
![參數配置](doc/img/4.png)
![字典管理](doc/img/5.png)
![任務調度](doc/img/6.png)
![服務監控](doc/img/7.png)
![快取管理](doc/img/8.png)
![行政區劃](doc/img/9.png)
![動態插件](doc/img/10.png)
![開放介面](doc/img/11.png)
![驗證碼配置](doc/img/12.png)
![租戶管理](doc/img/13.png)
![庫表管理](doc/img/14.png)

### 應用工作台核心功能（中英雙語對照）

進入租戶應用工作台後的 5 個核心功能頁面。同一頁面在中英文介面語言下的實際效果（右上角語言切換器即時切換，無需重新整理）：

| 功能 | 簡體中文 | English |
|------|----------|---------|
| 遠端配置 | ![遠端配置-中文](doc/img/15.png) | ![Remote Config-EN](doc/img/16.png) |
| 版本更新 | ![版本更新-中文](doc/img/17.png) | ![Version Update-EN](doc/img/18.png) |
| 分包管理 | ![分包管理-中文](doc/img/19.png) | ![Package Management-EN](doc/img/20.png) |
| 支付配置 | ![支付配置-中文](doc/img/21.png) | ![Payment Config-EN](doc/img/22.png) |
| 廣告回調驗簽 | ![廣告回調驗簽-中文](doc/img/23.png) | ![Ad Callback-EN](doc/img/24.png) |

## 文檔與資源

- 項目文檔站：<https://gameframex.doc.alianblank.com>
- 後端運行手冊（詳細）：[`GameFrameX.Admin.Unified.Api/README.zh-TW.md`](GameFrameX.Admin.Unified.Api/README.zh-TW.md)
- 前端運行手冊（詳細）：[`GameFrameX.Admin.Unified.Vue/README.zh-TW.md`](GameFrameX.Admin.Unified.Vue/README.zh-TW.md)
- Issue 回饋：[GitHub Issues](https://github.com/GameFrameX/Admin/issues)
- 客戶端 / 後台 / Web 三倉庫：見 [項目倉庫](#項目倉庫)

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
