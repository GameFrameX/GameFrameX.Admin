<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Web

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE)
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

GameFrameX 遊戲管理平台的前端工程，基於 Vue 3 + TypeScript + Vite + PrimeVue 構建。

> 本項目 fork 自 [vue-next-admin](https://gitee.com/lyt-top/vue-next-admin)，按 GameFrameX 的多租戶 / 應用作用域模型做了改造。當前版本 ![version](https://img.shields.io/github/package-json/v/GameFrameX/Admin?filename=GameFrameX.Admin.Unified.Vue/package.json)（取自 package.json，自動同步）。

## 目錄

- [項目簡介](#項目簡介)
- [技術棧](#技術棧)
- [倉庫目錄結構](#倉庫目錄結構)
- [快速開始](#快速開始)
- [它是怎麼工作的](#它是怎麼工作的)
- [API 與後端約定](#api-與後端約定)
- [加一個新頁面](#加一個新頁面)
- [開發規範](#開發規範)
- [常見問題與踩坑](#常見問題與踩坑)
- [部署](#部署)
- [命令速查](#命令速查)
- [從後端導出 OpenAPI](#從後端導出-openapi)
- [寫在最後](#寫在最後)
- [文檔與資源](#文檔與資源)
- [更新日誌](#更新日誌)

---

## 項目簡介

這是給 **GameFrameX 遊戲管理平台**寫的前端。營運、GM、客服、運維每天在瀏覽器裡改選單、加使用者、看訂單、推版本、看日誌——**這個工程就是幹這個的**。

後端是 .NET 寫的，倉庫在 [`GameFrameX.Admin.Unified.Api`](../GameFrameX.Admin.Unified.Api)；資料庫預設 PostgreSQL。前端用的是 Vue 3 + Vite + PrimeVue + Pinia + Vue Router + vue-i18n + Axios 這些大路貨，沒有奇技淫巧。

**最值得知道的三件事**：

1. **選單、權限、文案 全部由後端控制**。前端不寫選單，加選單要去後端選單管理裡加；前端只負責渲染後端回傳的選單樹。
2. **後端動態路由**。登入後前端會調 `/api/globalMenu/loginMenuTree` 拿選單，再用 `component` 欄位去 `src/views/` 找對應 `.vue` 檔案，最後用 `router.addRoute()` 註冊。所以加頁面檔案就夠了，不用動路由檔案。
3. **JWT 自動續期**。登入拿到的 token 存在 `localStorage`，過期前前端會在請求頭裡塞 `X-Authorization` 讓後端靜悄悄換發，所以你登入一次能爽很久。

---

## 技術棧

| 維度 | 選型 | 版本 |
|---|---|---|
| 框架 | Vue 3（Composition API + `<script setup lang="ts">`） | 3.5.x |
| 構建 | Vite | 6.x |
| UI 元件庫 | PrimeVue（Aura 主題）+ PrimeFlex + PrimeIcons | 4.5.x |
| 狀態管理 | Pinia + pinia-plugin-persistedstate | 3.x |
| 路由 | Vue Router（hash 模式，後端動態控制） | 4.x |
| 語言 | TypeScript（strict） | 5.3.x |
| 國際化 | vue-i18n（5 語言） | 11.x |
| 圖表 | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| 即時通信 | @microsoft/signalr | 10.x |
| 富文字 | wangEditor + editor-for-vue | 5.x |
| 程式碼編輯器 | Monaco Editor | 0.44.x |
| HTTP | Axios + 自研攔截器 | 1.x |
| 單元測試 | Vitest（jsdom + @vue/test-utils） | 3.x |
| 端到端測試 | Playwright（系統 Edge） | 1.64.x |
| 圖示 | FontAwesome 7（vue-fontawesome + 自動按需） | 7.x |
| 表單 | @primevue/forms + 自研 formResolver | 4.5.x |
| 編輯器輔助 | code-inspector-plugin（按住 Shift 點擊定位源碼） | — |
| 包管理 | pnpm（Corepack 自動啟用） | ≥ 11.5.2 |
| Node | — | ≥ 16（推薦 20） |

> Vue Router 是 **hash 模式**（`createWebHashHistory`），部署時不需要伺服器 rewrite，但 URL 裡會帶 `#`。

---

## 倉庫目錄結構

### 頂層

```
GameFrameX.Admin.Unified.Vue/
├── src/                        # 業務源碼（99% 時間在這裡）
├── public/                     # 靜態資源
│   └── config/config.js        # Vite 啟動時生成的執行時組態
├── scripts/                    # 工程化腳本（gen-api、check-frontend 等）
├── openapi.json                # 後端 Swagger 導出，pnpm gen:api 的輸入
├── build/                      # Docker / Nginx / 部署腳本
├── Dockerfile                  # 多階段構建映像
├── .cnb.yml                    # CNB 雲原生構建 / 部署流水線
├── .env / .env.development / .env.production   # 環境變數
├── eslint.config.mjs           # ESLint 9 flat config
├── .prettierrc.json
├── tsconfig.json / tsconfig.typecheck.json
├── vite.config.ts              # Vite 組態
├── index.html                  # SPA 入口（注入 public/config/config.js）
├── package.json                # scripts / dependencies
├── pnpm-lock.yaml
├── pnpm-workspace.yaml
├── CHANGELOG.md
├── LICENSE     # 開源協議檔案
└── README.md                   # 本檔案
```

### src/ 詳解

```
src/
├── api/                          # 調後端的地方（※）
│   ├── _shared/                  # 自動生成的共享層（HttpClient + data-contracts）
│   ├── <module-camel>/           # 每個後端 tag 一個目錄，含 index.ts（Api class）+ types.ts
│   │   ├── index.ts              # 自動生成 + 末尾導出頂層函式
│   │   └── types.ts              # 僅本模組用到的 data-contracts
│   └── <業務手寫>.ts             # 極少數手寫擴展，命名避免與生成物衝突
│
│   當前已有模組（節選）：
│   systemAuth / systemUser / systemRole / systemMenu / systemConfig /
│   globalMenu / globalDictType / globalLanguage / globalPlatform /
│   gameAppReleaseVersion / gameAppAssetPackage / gameHomeEntity /
│   gameOrderEntity / gameMailEntity / gameRedeemCodeEntity /
│   platformCaptchaConfig / platformJwtConfig / platformPasswordPolicyConfig …
│
├── api-services/                 # 歷史遺留目錄，已清空，忽略
│
├── composables/                    # 組合式函式（※）
│   ├── useCrudTable.ts             表格分頁 / 排序 / loading 封裝
│   ├── useEditDialog.ts            新增 / 編輯彈窗（剝離審計欄位、isEnabled 預設）
│   ├── useTableColumnPersistence.ts  列寬 / 列順序 / 列顯隱本地持久化
│   ├── useCurrentApp.ts            當前應用（AppId）狀態 + 載入 / 切換
│   ├── useConfigAppScope.ts        應用作用域變更
│   ├── useRequiredFields.ts        必填欄位集合
│   ├── usePrimeTablePage.ts        表格分頁 hook
│   ├── useRouteAppIdQuerySync.ts   AppId query 與表格同步
│   ├── formResolver.ts             表單校驗規則適配
│   └── types/                      組合式函式的 TS 型別
│
├── components/                   # 通用元件
│   ├── crud-data-table/            PrimeVue DataTable 的 CRUD 封裝
│   ├── crud-form-fields/           表單欄位統一渲染
│   ├── base-edit-dialog/           新增 / 編輯彈窗基類
│   ├── search-form/                查詢欄，支援 authPrefix 自動加權限按鈕
│   ├── time-column/                表格列：long 秒自動格式化
│   ├── form-label / form-help-text 表單欄位標籤 / 說明文字
│   ├── app-confirm-dialog          全域確認彈窗
│   ├── config-app-scope-picker     應用作用域選擇器
│   ├── narrow-screen-hint          窄屏提示
│   ├── notice-bar                  通知條
│   ├── cropper / drag-verify / editor / icon-selector / reward-items-editor / svg-icon / table
│
├── directive/                    # 自訂指令
│   ├── authDirective.ts            v-auth — 權限隱藏
│   ├── customDirective.ts          v-waves / v-drag / v-reclick（防連點）
│   ├── blockLoading.ts             v-block-loading — 整塊 loading 蒙層
│   └── index.ts                    註冊入口
│
├── i18n/                         # 國際化
│   ├── lang/                       框架級（zh-cn / en / zh-tw / ja / ko）
│   └── pages/                      業務級，按模組子目錄
│       ├── system/ / client/ / game/ / home/ / login/ / menu/
│       ├── personal/ / layout/ / errors/ / formI18n/ / chart/
│       └── platform* / global*Config/
│
├── layout/                       # 佈局
│   ├── splitter/                   主佈局容器
│   ├── navMenu/                    左側 / 橫向選單
│   ├── navBars/                    頂欄（使用者 / 通知 / 全屏 / 主題）
│   ├── routerView/                 keep-alive 容器
│   ├── footer / logo / upgrade     其他佈局元素
│   └── index.vue                   佈局入口
│
├── router/                       # 路由（※）
│   ├── index.ts                    路由實例 + 守衛
│   ├── frontEnd.ts                 前端控制路由（兜底，目前用不到）
│   ├── backEnd.ts                  後端動態路由核心
│   ├── appWorkspaceRoutes.ts       應用工作台路由
│   ├── globalConfigRoutes.ts       全域組態路由
│   └── route.ts                    靜態 / 兜底路由（/login / 404 / 401）
│
├── stores/                       # Pinia 狀態
│   ├── themeConfig.ts              主題 / 佈局 / 暗色 / 浮水印 / globalI18n / isRequestRoutes
│   ├── userInfo.ts                 使用者資訊 + authBtnList + constList + dictList
│   ├── routesList.ts               選單樹
│   ├── tagsViewRoutes.ts           Tabs 欄
│   ├── keepAliveNames.ts           keep-alive 快取的 name 列表
│   └── index.ts                    Pinia 實例 + 持久化配接器
│
├── theme/                        # SCSS 主題
│   ├── z-index.scss                統一 z-index 階梯
│   ├── breakpoints.scss            斷點（--bp-lg = 1024）
│   ├── primevue-overrides.scss     PrimeVue token 覆蓋
│   └── common/transition.scss      通用過渡
│
├── types/                        # TS 型別宣告
│
├── utils/                        # 工具函式（保持純函式）
│   ├── request.ts                  手寫 API 用的 axios 實例
│   ├── axios-utils.ts              生成 API 用的 HttpClient + getAPI 單例快取
│   ├── http/shared.ts              ※ 共享攔截器（JWT / refresh / 錯誤碼 i18n）
│   ├── storage.ts                  Local / Session（鍵加 __NEXT_NAME__: 前綴）
│   ├── authFunction.ts             auth() / hasAnyAuth() 函式式權限
│   ├── message.ts                  showSuccess / showError / showConfirm（PrimeVue Toast）
│   ├── watermark.ts                浮水印
│   ├── isEnabled.ts                IsEnabled 歸一化與新增預設值
│   ├── rowVersion.ts               行版本讀取（相容 rowVersion / RowVersion）
│   ├── targetTenant.ts             跨租戶 header（X-Target-Tenant-Id）
│   ├── primevue-locale.ts          PrimeVue 元件按語言切換
│   ├── formatTime.ts               日期 / 時間格式化
│   ├── download.ts / exportExcel.ts  匯出
│   ├── sanitize.ts                 DOMPurify 封裝
│   ├── fontawesome.ts              FA 自動按需
│   ├── icons.ts                    FA 名稱映射
│   ├── build.ts                    CDN 組態
│   ├── appWorkspace.ts             ※ 應用作用域工具（resolveRouteScope 等）
│   ├── arrayOperation.ts / commonFunction.ts / base64Conver.ts / crudConfig.ts / getStyleSheets.ts / loading.ts / mitt.ts / other.ts / template-utils.ts
│   └── http/                       攔截器拆分
│
├── views/                        # 業務頁面
│   ├── system/                     系統管理（使用者 / 角色 / 選單 / 組織 / 租戶 / 字典 / 日誌 / 組態 / 通知 / 外掛 / 列印 / 區域 / 線上使用者 ……）
│   ├── game/                       遊戲管理（區服 / 商品 / 訂單 / 郵件 / 兌換碼 / 商城 / 公告 / 物品 / 道具 ……）
│   ├── client/                     客戶端管理（平台 / 版本 / 渠道 / 裝置 / 語言 / 資源包 / 開放介面 ……）
│   ├── appCenter/                  應用中心（appEntry / appSelector / appWorkspace / gameAppEntity 等）
│   ├── contentCenter / chart / chartCenter / chartCenter / opsCenter / trustCenter / automationCenter / goodsCenter / redeemCenter / webShopCenter / playerCenter
│   ├── home/                       首頁儀表板
│   ├── login/                      登入
│   ├── personal/                   個人中心
│   ├── error/                      404 / 401
│   └── global/                     全域路由容器
│
├── App.vue                       # 根元件
├── main.ts                       # ※ 應用入口
├── auto-imports.d.ts             # unplugin-auto-import 生成
└── components.d.ts               # unplugin-vue-components 生成
```

### scripts/ 詳解

```
scripts/
├── gen-api.mjs                   # pnpm gen:api — 用 swagger-typescript-api 程式化生成
├── post-gen.mjs                  # 生成產物拆分 + 重組到 src/api/<module>/
├── migrate-api-services.mjs      # 歷史遷移腳本
├── migrate-to-api-dir.mjs        # 把 api-services/ 合併到 api/
├── check-i18n-keys.mjs           # pnpm run lint:i18n — 5 語言鍵完整性
├── check-frontend-standards.mjs  # pnpm run check:frontend — 跨端規範門禁
│   ├─ modes: all / routes / permissions / api / api-query-get
│   │         / text / sys-abbreviation / text-baseline / self-test
│   ├─ routes         後端選單 component ↔ src/views/<file>
│   ├─ permissions    前端 v-auth 宣告 ↔ 後端種子 Btn
│   ├─ api            手寫 vs 生成 API 重疊 review
│   ├─ api-query-get  page/list/query 類介面禁止 GET 傳 params
│   ├─ text           changed files 硬編碼中文掃描
│   └─ sys-abbreviation C179 回歸：禁 Sys/sys 業務縮寫
├── smoke-test.mjs                # 冒煙測試
├── fix-collisions.mjs            # 衝突修復
└── i18n-text-baseline.json       # 硬編碼中文豁免基線（C151 歷史欠債）
```

---

## 快速開始

### 環境

- Node.js ≥ 16（推薦 20）
- pnpm ≥ 11.5.2（透過 `corepack enable` 自動啟用）

### 跑起來

```bash
git clone https://github.com/GameFrameX/Admin.git
cd Admin/GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # 預設 http://localhost:20800
```

打開瀏覽器 → 看到登入頁 → 找後端同學要帳號。

### 卡住了？

| 現象 | 大機率原因 |
|---|---|
| `pnpm: command not found` | 沒裝：`npm i -g pnpm@11.5.2` |
| 埠 20800 被占 | 改 `.env` 的 `VITE_PORT=9000`，重啟 dev |
| 瀏覽器白屏 | 看終端機最後幾行，Vite 會吐編譯錯誤 |
| 登入失敗 | 沒有後端 / 後端沒跑 / `.env.development` 的 `VITE_API_URL` 不對 |

### 環境變數

| 變數 | 說明 | 開發預設值 |
|---|---|---|
| `VITE_API_URL` | 後端 API 基址 | `http://127.0.0.1:20802` |
| `VITE_PORT` | dev 埠 | `20800` |
| `VITE_OPEN` | 是否自動開瀏覽器 | `false` |
| `VITE_OPEN_CDN` | 是否啟用 CDN 外置 | `false` |
| `VITE_PUBLIC_PATH` | 生產公共路徑 | 空 |

Vite 啟動時會把這些值寫入 `public/config/config.js` 的 `window.__env__`。Docker 部署時 `entrypoint.sh` 會根據容器環境變數重新生成這個檔案，**同一個映像可以靠環境變數切不同環境**。

---

## 它是怎麼工作的

### 啟動流程（`src/main.ts`）

`index.html` 載入 `public/config/config.js`（寫入 `window.__env__`）→ 載入 `src/main.ts` → 按順序掛載：

1. Pinia（含持久化外掛）
2. Router（hash 模式）
3. i18n（從 `src/i18n/**` 自動 glob）
5. PrimeVue + Aura 主題 + ToastService + ConfirmationService
6. `@vue-js-cron/light`、`vue-grid-layout`、`vue-signature-pad`、`vue3-tree-org`
7. 自訂指令（v-auth / v-waves / v-reclick / v-block-loading）

`App.vue` 再做四件事：監聽路由改 title、初始化暗色主題、注入浮水印、綁定 PrimeVue Locale、初始化 SignalR 訊息服務。

### 登入與選單註冊

打開瀏覽器看到登入頁 → 輸入帳號 → 前端發 `/api/systemAuth/login` → 後端校驗 + 簽發 JWT → 前端把 token 存 `localStorage` → 跳到 `/dashboard/home`。

路由守衛 `router.beforeEach`（`src/router/index.ts`）發現「我還沒有選單」，並行拉兩個介面：

- `/api/systemAuth/userInfo` → 使用者資訊 + `buttons` 權限陣列
- `/api/globalMenu/loginMenuTree` → 選單樹

選單大致長這樣（DevTools Network 裡看）：

```json
[
  { "path": "/dashboard",    "name": "dashboard",  "component": "/home/index" },
  { "path": "/system/user",  "name": "systemUser", "component": "/system/user/index" }
]
```

`backEnd.ts` 用 `import.meta.glob('../views/**/*.{vue,tsx}')` 把所有視圖檔案預載入到一張表，再用 `dynamicImport()` 把 `component` 字串映射成元件函式，最後 `router.addRoute()` 註冊。

**所以選單改了要後端重啟 + 前端重新登入才生效**。前端本身不寫選單列表。

按鈕權限類似：`<Button v-auth="'systemUser:add'">` 自訂指令去 `buttons` 陣列裡查，沒有就 `display: none` 藏起來。

### HTTP 攔截器（`src/utils/http/shared.ts`）

兩個 axios 實例共用同一份攔截器工廠：

- `src/utils/request.ts` 的 `service` —— 手寫 API 用，支援 GET params 序列化
- `src/utils/axios-utils.ts` 的 `sharedHttpClient` —— swagger 生成 API 用，自動加 `Accept-Language`

**請求攔截**（4 步）：

1. 自動加 `Authorization: Bearer <access-token>`；如 JWT 即將過期，同時加 `X-Authorization: Bearer <x-refresh-token>`，讓後端靜默換發
2. 自動加 `X-Target-Tenant-Id`（跨租戶切換時由 `utils/targetTenant.ts` 控制）
3. `request.ts` 額外把 GET 請求的 `data` 序列化為 query string（`tansParams`）
4. `axios-utils.ts` 額外加 `Accept-Language: <globalI18n>`

**回應攔截**（4 步）：

1. HTTP 401 → 清 token 並 reload（`clearAccessTokens`）
2. 回應頭帶回 `access-token` / `x-access-token` → 自動寫入 LocalStorage（token 靜默換發）
3. 業務碼 `code === 200` 直接放行；其它錯誤碼：
   - `errors.codes.<ERROR_CODE>` 命中 i18n → 顯示本地化文案
   - 命中 `VALIDATION.FAILED` → 顯示後端原始 message（保留欄位級錯誤）
   - 其它 → 顯示後端 `message`
4. 駝峰化（首字母大寫 → 小寫）+ 錯誤提示走 PrimeVue Toast

### 應用作用域（多租戶）

後端選單每項帶 `app_scope`（`platform` / `tenant` / `app`）與 `menu_context`。前端用 `meta.appScope` + `meta.menuContext` 標記：

| Scope | 含義 | 路由示例 |
|---|---|---|
| `platform` | 平台級（超管可見，跨租戶） | `/platform/*` |
| `tenant` | 租戶級（所有應用共有，無 appId query） | `/dashboard/*`、`/system/*` |
| `app` | 應用工作台內（必須帶 `appId` query） | `/gameCenter/*` |

跳轉邏輯在 `src/utils/appWorkspace.ts` 的 `decideTenantAppEntry()` / `resolveRouteScope()`，由 `router/index.ts` 的 `resolveTenantAppRedirect()` 在守衛裡觸發。

### Pinia 持久化

| Store | 儲存 | 關鍵欄位 |
|---|---|---|
| `themeConfig` | localStorage | 主題 / 佈局 / 暗色 / 浮水印 / globalI18n / `isRequestRoutes` |
| `userInfo` | sessionStorage（僅 `pick: ['constList', 'dictList']`） | 系統常量 + 字典（啟動慢，所以快取） |
| `routesList` | 不持久化 | 當前選單樹 |
| `tagsViewRoutes` | 不持久化 | Tabs 欄 |
| `keepAliveNames` | 不持久化 | keep-alive include |

所有鍵自動加 `__NEXT_NAME__:` 前綴（透過 `Local.setKey`），避免跨應用衝突。

---

## API 與後端約定

### 兩層 API

| 層 | 位置 | 來源 | 調用 |
|---|---|---|---|
| **生成 API** | `src/api/<module>/{index.ts,types.ts}` | swagger-typescript-api 程式化生成 | `getAPI(SomeApi).someMethod()` |
| **手寫 API** | `src/api/<業務>.ts` | 業務需要的擴展 / 封裝 | `request({ url, method, data })` |

生成 API 的調用範式：

```ts
import { getAPI } from '/@/utils/axios-utils'
import { SystemUser } from '/@/api/systemUser'

const api = getAPI(SystemUser)                       // 單例快取，多次調用無開銷
const res = await api.systemUserPagePost({ page: 1, pageSize: 20 })
const user = res.data.result.items[0]                 // 自動歸一化的 { code, result, ... }
```

手寫 API 的調用範式：

```ts
import request from '/@/utils/request'

export const pageX = (params: PageParams) =>
  request<Result<PageResult<X>>>({
    url: '/api/game/x/page',
    method: 'post',
    data: params,
  })
```

### URL 命名（動態 API 路由規則）

後端用自研 `IDynamicApiController`（由 `Web.Core/ApiRouting/AdminApiRouteRules` 實現）自動從 service 類名生成 URL：**去掉 `Service` / `AppService` / `Controller` 後綴 → 首字母小寫 → 加 `api/` 前綴**。

| 後端類名 | 前端 URL |
|---|---|
| `SystemAuthService` | `/api/systemAuth/...` |
| `GameAppService` | `/api/gameApp/...` |
| `SystemUserService` | `/api/systemUser/...` |

**前端 URL 嚴禁保留 `Service` 後綴**。這是新人 100% 會踩的坑。

### 錯誤碼 → i18n

後端穩定錯誤碼會自動映射到 `src/i18n/pages/errors/<lang>.ts` 的 `errors.codes.<CODE_WITH_UNDERSCORE>`：

- `AUTH.FORBIDDEN` → `errors.codes.AUTH_FORBIDDEN`
- `D0008` → `errors.codes.D0008`
- `VALIDATION.FAILED` → 透傳後端 message（保留欄位級錯誤）

新錯誤碼在 `errors/zh-cn.ts` 加對應 key 即可。

### 後端契約變更流程

1. 後端改 OpenAPI / Swagger
2. 導出新 `openapi.json` → 替換本倉庫根
3. `pnpm gen:api` → 檢查 `git diff src/api/`
4. 欄位重命名 / 移除 → 修前端調用處（TS 編譯會報錯）
5. `pnpm run check:frontend api` 跑一遍（看手寫與生成重疊）
6. `pnpm run quality` 必過

---

## 加一個新頁面

跟著做，做完你就掌握了 80% 的日常工作。我們加一個「通知公告」列表頁。

### 1. 建檔案

```
src/views/system/demo/
├── index.vue          # 列表頁
└── component/
    └── editDemo.vue   # 新增/編輯彈窗
```

### 2. 寫列表頁

`src/views/system/demo/index.vue` 的核心結構（不展示完整樣式）：

```vue
<template>
  <div class="p-3">
    <!-- 頂部：搜索 + 新增 -->
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-3 align-items-end">
          <InputText v-model="state.queryParams.title" />
          <Button v-auth="'demo:page'" @click="handleQuery">
            {{ t('common.button.search') }}
          </Button>
          <Button v-auth="'demo:add'" @click="editRef.openDialog()">
            {{ t('common.button.add') }}
          </Button>
        </div>
      </template>
    </Card>

    <!-- 表格 -->
    <Card class="mt-2">
      <template #content>
        <CrudDataTable
          :value="state.data"
          :totalRecords="state.tableParams.total"
          :rows="state.tableParams.pageSize"
          :first="(state.tableParams.page - 1) * state.tableParams.pageSize"
          @page="onPageChange"
          v-block-loading="state.loading"
        >
          <Column field="title" :header="t('common.label.title')" />
          <Column :header="t('common.table.action')">
            <template #body="{ data }">
              <Button link v-auth="'demo:update'" @click="editRef.openDialog(data)">
                {{ t('common.button.edit') }}
              </Button>
            </template>
          </Column>
        </CrudDataTable>
      </template>
    </Card>

    <EditDemo ref="editRef" @handle-query="handleQuery" />
  </div>
</template>

<script setup lang="ts" name="systemDemo">
import { onMounted, reactive, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { getAPI } from '/@/utils/axios-utils'
import { Demo as DemoApi } from '/@/api/demo'
import CrudDataTable from '/@/components/crud-data-table/index.vue'

const { t } = useI18n()
const editRef = ref()
const state = reactive({
  loading: false,
  data: [] as any[],
  queryParams: { title: '' },
  tableParams: { page: 1, pageSize: 10, total: 0 },
})
const handleQuery = async () => {
  state.loading = true
  try {
    const res = await getAPI(DemoApi).demoPagePost({ ...state.queryParams, ...state.tableParams })
    state.data = res.data.result?.items ?? []
    state.tableParams.total = res.data.result?.total ?? 0
  } finally { state.loading = false }
}
const onPageChange = (e: any) => {
  state.tableParams.page = e.page + 1
  state.tableParams.pageSize = e.rows
  handleQuery()
}
onMounted(handleQuery)
</script>
```

### 3. 寫彈窗

`src/views/system/demo/component/editDemo.vue`：

```vue
<template>
  <BaseEditDialog
    v-model:visible="state.isShowDialog"
    :default-form="() => ({ title: '', content: '' })"
    :add-api="(data) => getAPI(DemoApi).demoAddPost(data)"
    :update-api="(data) => getAPI(DemoApi).demoUpdatePut(data)"
    :rules="{ title: { required: true, message: '請輸入標題' } }"
    @handle-query="emit('handleQuery')"
  >
    <InputText v-model="state.ruleForm.title" />
    <InputText v-model="state.ruleForm.content" />
  </BaseEditDialog>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import { getAPI } from '/@/utils/axios-utils'
import { Demo as DemoApi } from '/@/api/demo'

const emit = defineEmits<{ (e: 'handleQuery'): void }>()
const state = reactive({ isShowDialog: false, ruleForm: {} as any })
</script>
```

`CrudDataTable` 和 `BaseEditDialog` 是項目自帶的輪子（`src/components/`），封裝了分頁 / loading / 操作列 / 新增編輯判斷 / 提交 loading，**別自己造輪子**。

### 4. 加 i18n（5 個語言必須齊全）

`src/i18n/pages/system/demo/zh-cn.ts`：

```ts
export default {
  title: '通知公告',
  form: { title: '標題', content: '內容' },
}
```

另外 4 個語言檔案**必須建**（哪怕只是機械複製）：

```
src/i18n/pages/system/demo/
├── zh-cn.ts
├── en.ts
├── zh-tw.ts
├── ja.ts
└── ko.ts
```

少了任何一個，`pnpm run lint:i18n` 會 fail。

### 5. 讓後端掛選單

程式碼寫完了但你還看不到頁面——選單是後端管的。告訴後端同學：

```
path:        /system/demo
name:        systemDemo
component:   /system/demo/index       ← 要和 src/views/ 裡路徑一致
title:       通知公告
titleCode:   system.demo.title        ← 要在 zh-cn.ts 裡存在
parent:      /system
```

後端 commit + 部署後，前端重新登入就能看到選單。

### 6. 提交

```bash
pnpm run quality      # 一鍵跑所有門禁
git add .
git commit -m "feat(system.demo): 新增通知公告列表 + 彈窗"
git push
```

`pnpm run quality` 掛了按錯誤修：i18n 缺 key 補翻譯、權限沒在選單種子找後端加、硬編碼中文走 i18n。

---

## 開發規範

> 這一節是**約束**——CI 會卡，違反就是 build fail。

### 命名約定

| 類別 | 風格 | 示例 |
|---|---|---|
| 視圖目錄 / 檔案 | kebab-case | `src/views/system/user/index.vue` |
| API 模組目錄 | camelCase（後端 swagger tag） | `src/api/systemUser/` |
| Pinia store 檔案 | camelCase | `themeConfig.ts` |
| Composables 檔案 | `useXxx` | `useEditDialog.ts` |
| 工具函式檔案 | camelCase | `authFunction.ts` |
| 權限字串 | `<resource>:<action>` | `systemUser:page`、`gameHomeEntity:query` |
| 路由 name | camelCase | `systemUser` |
| 路由 path | kebab-case | `/system/user` |
| i18n key | 點號分層 | `system.user.title`、`common.button.search` |
| 錯誤碼 key | 大寫下劃線 | `AUTH_FORBIDDEN` |

### 目錄約定

- 視圖 `<module>` 與後端選單 `component` 第一段一致，方便人腦 ↔ 路由 ↔ 源碼 三向定位
- 彈窗統一放 `views/<module>/component/`
- 單頁面專用元件放 `views/<module>/component/`，**不要**塞到 `src/components/`（那是復用池）
- API 模組不與生成物同名（`check:api` 會標出供人工 review）

### 權限規範

- 範本裡用 `v-auth="'xxx'"` / `v-auth="['x','y']"` / `v-auth-all`
- `v-if` / props 裡用 `auth('xxx')` 或 `hasAnyAuth('x','y')`（`src/utils/authFunction.ts`）
- 每個權限字串必須出現在後端選單種子的 `MenuTypeEnum.Btn` 條目裡，否則 `pnpm run check:permissions` fail
- 加新權限的流程：後端加種子條目 → 前端 `v-auth` 引用 → 跑 `check:permissions`

### i18n 規範

- **任何 i18n 鍵必須 5 個語言都存在**（`zh-cn / en / zh-tw / ja / ko`），`pnpm run lint:i18n` 自動檢查
- 範本和 `<script setup>` 中**嚴禁硬編碼中文**，`pnpm run check:frontend text` 掃 changed files
- 歷史欠債在 `scripts/i18n-text-baseline.json` 指紋豁免；遷移期可 `pnpm run check:frontend text-baseline` 增量基線
- 新錯誤碼在 `errors/zh-cn.ts` 加 `errors.codes.<CODE_WITH_UNDERSCORE>`

### HTTP 規範

- **page / list / query / search / filter 類介面禁止用 GET 傳 params / data**——`pnpm run check:frontend api-query-get` 會卡
- 後端生成 API 必須用 `xxxPagePost` / `xxxListPost` / `xxxQueryPost`
- 手寫 API 用 `request({ method: 'post', data: params })`
- **不要**在業務程式碼裡手動加 `Authorization` / `Accept-Language` / `X-Tenant-Id`——攔截器自動處理
- **不要**在回應裡手動 `showError`——攔截器自動處理業務碼

### 提交流程

```bash
pnpm run quality      # 必過：lint + typecheck + test:unit + build + check:frontend
git add .
git commit -m "<scope>(<module>): <subject>"   # 中文 / 英文都可，≤ 60 字
git push
```

> `pnpm run quality` 串起來的總閘，自 2026-10-20 起把 Vitest 單元/元件測試納入 lint 與 build 之間；
> 單元層回歸擋住就攔在 build 之前，省整合成本。新增純邏輯工具請在 `src/<dir>/<name>.test.ts` 同位補單測；
> 新增 Vue 元件請參照 `src/components/form-label/index.test.ts`（`mount` + 注入 provider + slot 文本斷言）。
> 開發期可跑 `pnpm run test:unit:watch` 走 watch UI。

- scope: `feat / fix / refactor / docs / style / test / chore / perf`
- module: `system.user` / `api` / `router` / `docs` / `i18n`…
- body 解釋動機 + 關鍵改動
- 分支：`feat/<module>-<desc>` / `fix/<module>-<desc>`

### 違規示例（不要這麼寫）

```ts
// ❌ axios.get('/api/xxx', { params }) — query 類介面禁止 GET
const res = await axios.get('/api/user/page', { params })

// ✅
const res = await request({ url: '/api/user/page', method: 'post', data })

// ❌ localStorage.setItem('themeConfig', ...) — 不會自動加前綴
window.localStorage.setItem('themeConfig', JSON.stringify({...}))

// ✅
import { Local } from '/@/utils/storage'
Local.set('themeConfig', {...})

// ❌ URL 帶 Service 後綴
const url = '/api/systemUserService/page'

// ✅
const url = '/api/systemUser/page'

// ❌ 範本硬編碼中文
<span>搜索</span>

// ✅
<span>{{ t('common.button.search') }}</span>

// ❌ 手寫 isShowDialog / openDialog / submit 三件套
defineExpose({ isShowDialog, openDialog, submit })

// ✅ 用 useEditDialog composable
const { isShowDialog, openDialog, submit } = useEditDialog({ ... })
```

---

## 常見問題與踩坑

### FAQ

**選單 / 權限 / 文案 為什麼要從後端來？**
因為前端不該知道「誰能看什麼」。後端根據角色算好回傳，前端只負責渲染。後端改了發版，前端不用動。

**JWT 是啥？怎麼自動續期？**
電影票。伺服器給你一張加密的票，請求頭帶 `Authorization: Bearer xxx`。過期前前端自動加 `X-Authorization` 讓後端靜悄悄換發。

**為什麼 i18n 要 5 個檔案？**
因為是多語言產品，CI 強制要求 `zh-cn / en / zh-tw / ja / ko` 都存在。少一個 fail。新寫的 key 可以先機械複製 zh-cn，翻譯慢慢補。

**`getAPI(...)` 和 `request(...)` 哪個用？**
- `getAPI(SomeApi).someMethod()` —— 自動生成的 API（`src/api/<module>/`）
- `request({ url, method })` —— 手寫 API（`src/api/login.ts` 這種）
兩種共用同一份 HTTP 攔截器，別自己造 axios。

**`v-auth` / `v-block-loading` / `v-reclick` 是啥？**
- `v-auth="'xxx'"` —— 沒權限 `display: none`
- `v-block-loading="loading"` —— 整塊加 loading 蒙層
- `v-reclick` —— 防按鈕連點
定義在 `src/directive/`，範本裡直接當 attribute 用。

**`src/api-services/` 是幹啥的？**
歷史遺留目錄，已廢棄。新生成 API 全部在 `src/api/<module>/`。

### 踩坑

**登入後白屏 / 選單出不來**
DevTools → Network 看 `/api/systemAuth/login` 是不是 200。不是 → 檢查 `VITE_API_URL` 和後端；是 → 看 `/userInfo` 和 `/loginMenuTree`，任一非 200 就會跳 `/login`。都 200 還白屏 → Console 看 JS 報錯，多半是 component 指向了不存在的 .vue。

**選單能進但頁面 404**
跑 `pnpm run check:routes`，多半是後端選單的 `component` 路徑和 `src/views/` 對不上。

**按鈕全看不到**
Network 看 `/userInfo` 回應裡的 `buttons` 欄位有沒有期待的字串；有但看不到 → `v-auth` 字串**完全一致**（區分大小寫）；都沒有 → 找後端同學，你的角色沒拿到這個權限。

**i18n 報 missing key**
跑 `pnpm run lint:i18n`，按報錯補 5 個語言檔案裡缺的 key。

**`check:frontend text` 全炸**
看是不是新增了硬編碼中文 → 走 i18n；否則是歷史欠債在遷移期 → 用 `pnpm run check:frontend text-baseline` 增量基線（謹慎）。

**Docker 部署後 API 地址還是舊的**
1. `docker exec -it <container> cat /usr/share/nginx/html/config/config.js` 看 `window.__env__.VITE_API_URL`
2. 是舊的 → 啟動時沒傳 `-e VITE_API_URL=...`
3. 是新值但瀏覽器是舊的 → Ctrl+F5 / Cmd+Shift+R 強刷
4. 還不行 → 容器是舊映像起的，`docker pull` 拉最新

**構建 OOM**
Docker Desktop → Preferences → Resources → Memory 調到 8G+，Swap 調到 4G+。構建命令已加 `NODE_OPTIONS=--max-old-space-size=4096`。

**401 後沒自動跳登入**
檢查 `localStorage.access-token` 是否過期；JWT payload `exp`；後端 `/api/systemAuth/refresh-token` 是否回傳新 token。

---

## 部署

### Docker（推薦）

> 單倉整合（2026-10-19）：Vue Dockerfile 的 build context 已上移為倉庫根，
> `build/default.conf` / `build/entrypoint.sh` 同步遷出至倉庫根 `build/`。注意在倉庫根執行構建。

```bash
docker build -t gameframex-admin-web:dev -f GameFrameX.Admin.Unified.Vue/Dockerfile .
docker run -d -p 80:80 gameframex-admin-web:dev                                          # 預設 API 地址
docker run -d -p 80:80 -e VITE_API_URL=https://api.example.com gameframex-admin-web:dev   # 自訂 API
```

容器有個小魔法：`build/entrypoint.sh` 啟動時讀環境變數重寫 `config.js`，所以**同一個映像可以靠環境變數切不同環境**。

### docker-build-push.sh 完整參數

```bash
./build/docker-build-push.sh --tag v2.4.33
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --tag v2.4.33
./build/docker-build-push.sh \
  --platforms linux/amd64,linux/arm64 \
  --tag v2.4.33
./build/docker-build-push.sh --no-push                # 只構建不推
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --username your_user \
  --password your_pass \
  --tag v2.4.33                                       # 私有倉庫認證
```

| 參數 | 說明 | 預設值 |
|---|---|---|
| `--registry URL` | 容器倉庫地址 | 空（Docker Hub） |
| `--namespace NAME` | 命名空間 / 使用者 | `alianblank` |
| `--name NAME` | 映像名稱 | `gameframex-admin-web` |
| `--tag TAG` | 映像標籤 | git tag 或 `分支-日期-SHA` |
| `--platforms LIST` | 目標平台 | `linux/amd64` |
| `--no-push` | 只構建不推 | — |
| `--username USER` | 倉庫使用者名 | — |
| `--password PASS` | 倉庫密碼 | — |

映像標籤自動生成規則：當前 commit 有 git tag → 用 tag；否則 → `分支名-日期-SHA`；同時打 `latest`。

### CNB 流水線

`main` / `develop` push 自動構建 + 部署測試環境；打 tag 同步打 `latest` + 自動部署。詳細規則見 `.cnb.yml`。

### 純靜態

`pnpm run build` 產物在 `GameFrameX.Admin.Unified.Vue/build/dist/`（`vite.config.ts` 的 `outDir` 仍為相對 cwd 的 `build/dist`，相對子目錄生效）。任何支援 SPA fallback（`try_files $uri /index.html`）的 HTTP 伺服器都能託管。Nginx 範本在倉庫根 `build/default.conf`（2026-10-19 單倉整合，已從上移的 Vue `build/` 遷出）。

### Docker Compose 示例

```yaml
services:
  admin-web:
    image: gameframex-admin-web
    ports: ["80:80"]
    environment:
      - VITE_API_URL=https://api.example.com
    restart: unless-stopped
```

---

## 命令速查

| 命令 | 幹嘛的 |
|---|---|
| `pnpm run dev` | 啟動開發伺服器 |
| `pnpm run build` | 生產構建（產物在 `GameFrameX.Admin.Unified.Vue/build/dist/`，vite `outDir` 相對 cwd） |
| `pnpm run lint` / `lint-fix` | ESLint 查 / 自動修 |
| `pnpm run format` / `format-fix` | Prettier 查 / 自動修 |
| `pnpm run typecheck` | tsc 型別檢查 |
| `pnpm run test:unit` / `test:unit:watch` / `test:unit:coverage` | Vitest 單元 / 元件單測（jsdom + @vue/test-utils，含 v8 覆蓋率） |
| `pnpm run test:e2e` / `test:e2e:headed` / `test:e2e:list` / `test:e2e:report` | Playwright 端到端（系統 Edge，套件串行） |
| `pnpm gen:api` | 從 `openapi.json` 重新生成前端 API 客戶端 |
| `pnpm run lint:i18n` | 檢查 5 語言 i18n 鍵完整性 |
| `pnpm run check:routes` | 後端選單 component 與前端視圖對齊 |
| `pnpm run check:permissions` | 前端權限宣告與後端種子對齊 |
| `pnpm run check:api` | 手寫與生成 API 重疊 review |
| `pnpm run check:text` | 掃描硬編碼中文（changed files fail） |
| `pnpm run check:frontend` | 上面所有 + Sys 縮寫回歸 |
| `pnpm run quality` | **一鍵跑所有門禁**，提交前必跑 |

`pnpm gen:api` 的前置：從後端導出 `openapi.json`——見下一節。

---

## 從後端導出 OpenAPI

`pnpm gen:api` 只認倉庫根的 `openapi.json`，但**前端不會自己生成它**，要從後端 Swagger 端點拉。

後端是 .NET + Swashbuckle，Swagger JSON 暴露在：

```
http://localhost:20802/swagger/web.api/swagger.json
```

> Swagger **預設只在 dev 環境開**。生產 / Docker 必須設 `Swagger__Enabled=true`（或 `Swagger:Enabled=true`）才會暴露這個端點——這是後端 `Startup.cs` 的開關行為，目的是生產預設不外洩文檔。

```bash
# 1. 起後端（任選一種讓 Swagger 可訪問）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
# 或
Swagger__Enabled=true dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &

# 2. 拉 swagger.json
curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o openapi.json
ls -lh openapi.json   # 應 ~3 MB

# 3. 跑前端生成
pnpm gen:api
git diff src/api/     # 看有沒有破壞性變更
```

Client / Hub API 也用同樣模式：`/swagger/client.api/swagger.json`（埠 20808）、`/swagger/hub.api/swagger.json`。前端 Admin 項目只用 Admin API，所以一般不用拉這兩個。

### 自動化腳本（可選）

團隊經常做這事可以寫個腳本（自己寫吧，我沒替你寫）。最小骨架：

```bash
#!/usr/bin/env bash
set -euo pipefail

BACKEND_HOST="${BACKEND_HOST:-http://localhost:20802}"
FRONTEND_REPO="${FRONTEND_REPO:-$(cd "$(dirname "$0")/../../GameFrameX.Admin.Unified.Vue" && pwd)}"

ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
BACKEND_PID=$!
trap "kill $BACKEND_PID 2>/dev/null || true" EXIT

for i in $(seq 1 30); do
  curl -fsSL "${BACKEND_HOST}/swagger/web.api/swagger.json" -o /dev/null && break
  sleep 1
done

curl -sSL "${BACKEND_HOST}/swagger/web.api/swagger.json" -o "${FRONTEND_REPO}/openapi.json"
echo "wrote ${FRONTEND_REPO}/openapi.json"
```

---

## 寫在最後

口訣：

> **選單來自後端，權限來自後端，國際化必須 5 語言，提交前跑 `pnpm run quality`。**

卡住了別死磕：先看踩坑，沒解決去看 git log，再不行問同事。

想看具體程式碼就跳：

- `src/router/index.ts` — 路由守衛
- `src/router/backEnd.ts` — 後端選單 → 前端路由
- `src/utils/http/shared.ts` — HTTP 攔截器
- `src/stores/userInfo.ts` — 使用者資訊 / 按鈕權限
- `src/composables/useEditDialog.ts` — 彈窗核心
- `src/utils/appWorkspace.ts` — 多租戶應用作用域
- `scripts/check-frontend-standards.mjs` — 跨端規範門禁

## 文檔與資源

- 項目文檔站：<https://gameframex.doc.alianblank.com>
- 項目總覽：[`../README.zh-TW.md`](../README.zh-TW.md)
- 後端運行手冊：[`../GameFrameX.Admin.Unified.Api/README.zh-TW.md`](../GameFrameX.Admin.Unified.Api/README.zh-TW.md)
- Issue 反饋：<https://github.com/GameFrameX/Admin/issues>

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

版本變更記錄見 [`CHANGELOG.md`](./CHANGELOG.md)。

## 開源協議

詳見 [LICENSE](./LICENSE) 檔案。

<!--
EN: See [LICENSE](./LICENSE) for license information.
zh-CN: 详见 [LICENSE](./LICENSE) 文件。
zh-TW: 詳見 [LICENSE](./LICENSE) 檔案。
ja: 詳しくは [LICENSE](./LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.
-->
