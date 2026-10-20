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

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761
<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

---

The frontend project of the GameFrameX game management platform, built on Vue 3 + TypeScript + Vite + PrimeVue.

> This project is forked from [vue-next-admin](https://gitee.com/lyt-top/vue-next-admin) and reworked for GameFrameX's multi-tenant / app-scope model. Current version ![version](https://img.shields.io/github/package-json/v/GameFrameX/Admin?filename=GameFrameX.Admin.Unified.Vue/package.json) (sourced from package.json, kept in sync automatically).

## Table of Contents

- [Project Overview](#project-overview)
- [Tech Stack](#tech-stack)
- [Repository Directory Structure](#repository-directory-structure)
- [Quick Start](#quick-start)
- [How It Works](#how-it-works)
- [API and Backend Conventions](#api-and-backend-conventions)
- [Adding a New Page](#adding-a-new-page)
- [Development Standards](#development-standards)
- [FAQ and Pitfalls](#faq-and-pitfalls)
- [Deployment](#deployment)
- [Command Cheat Sheet](#command-cheat-sheet)
- [Exporting OpenAPI from the Backend](#exporting-openapi-from-the-backend)
- [Final Words](#final-words)
- [Documentation & Resources](#documentation--resources)
- [Changelog](#changelog)

---

## Project Overview

This is the frontend written for the **GameFrameX game management platform**. Operations, GMs, customer service, and DevOps staff spend every day in the browser editing menus, adding users, reviewing orders, pushing releases, and reading logs — **that is exactly what this project is for**.

The backend is written in .NET, in the repository [`GameFrameX.Admin.Unified.Api`](../GameFrameX.Admin.Unified.Api); the database defaults to PostgreSQL. The frontend uses mainstream staples like Vue 3 + Vite + PrimeVue + Pinia + Vue Router + vue-i18n + Axios — no exotic tricks.

**The three things most worth knowing**:

1. **Menus, permissions, and copy are all controlled by the backend**. The frontend does not define menus; to add one, add it in the backend menu management. The frontend only renders the menu tree returned by the backend.
2. **Backend dynamic routing**. After login, the frontend calls `/api/globalMenu/loginMenuTree` to fetch the menu, uses the `component` field to find the corresponding `.vue` file in `src/views/`, and finally registers it with `router.addRoute()`. So adding a page file is enough — no need to touch the router files.
3. **Automatic JWT renewal**. The token obtained at login is stored in `localStorage`; before it expires the frontend sends an `X-Authorization` header so the backend quietly reissues it — one login keeps you working for a long time.

---

## Tech Stack

| Dimension | Choice | Version |
|---|---|---|
| Framework | Vue 3 (Composition API + `<script setup lang="ts">`) | 3.5.x |
| Build | Vite | 6.x |
| UI component library | PrimeVue (Aura theme) + PrimeFlex + PrimeIcons | 4.5.x |
| State management | Pinia + pinia-plugin-persistedstate | 3.x |
| Routing | Vue Router (hash mode, backend dynamic control) | 4.x |
| Language | TypeScript (strict) | 5.3.x |
| Internationalization | vue-i18n (5 languages) | 11.x |
| Charts | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| Realtime | @microsoft/signalr | 10.x |
| Rich text | wangEditor + editor-for-vue | 5.x |
| Code editor | Monaco Editor | 0.44.x |
| HTTP | Axios + in-house interceptors | 1.x |
| Unit testing | Vitest (jsdom + @vue/test-utils) | 3.x |
| End-to-end testing | Playwright (system Edge) | 1.64.x |
| Icons | FontAwesome 7 (vue-fontawesome + automatic on-demand) | 7.x |
| Forms | @primevue/forms + in-house formResolver | 4.5.x |
| Editor helper | code-inspector-plugin (hold Shift and click to jump to source) | — |
| Package manager | pnpm (auto-enabled via Corepack) | ≥ 11.5.2 |
| Node | — | ≥ 16 (20 recommended) |

> Vue Router uses **hash mode** (`createWebHashHistory`), so deployment needs no server-side rewrite, but URLs carry a `#`.

---

## Repository Directory Structure

### Top level

```
GameFrameX.Admin.Unified.Vue/
├── src/                        # Business source code (99% of your time is here)
├── public/                     # Static assets
│   └── config/config.js        # Runtime config generated by Vite at startup
├── scripts/                    # Engineering scripts (gen-api, check-frontend, etc.)
├── openapi.json                # Backend Swagger export, input of pnpm gen:api
├── build/                      # Docker / Nginx / deployment scripts
├── Dockerfile                  # Multi-stage build image
├── .cnb.yml                    # CNB cloud-native build / deploy pipeline
├── .env / .env.development / .env.production   # Environment variables
├── eslint.config.mjs           # ESLint 9 flat config
├── .prettierrc.json
├── tsconfig.json / tsconfig.typecheck.json
├── vite.config.ts              # Vite config
├── index.html                  # SPA entry (injects public/config/config.js)
├── package.json                # scripts / dependencies
├── pnpm-lock.yaml
├── pnpm-workspace.yaml
├── CHANGELOG.md
├── LICENSE     # Open source license file
└── README.md                   # This file
```

### src/ in detail

```
src/
├── api/                          # Where backend calls live (※)
│   ├── _shared/                  # Auto-generated shared layer (HttpClient + data-contracts)
│   ├── <module-camel>/           # One directory per backend tag, containing index.ts (Api class) + types.ts
│   │   ├── index.ts              # Auto-generated + top-level functions exported at the end
│   │   └── types.ts              # data-contracts used only by this module
│   └── <handwritten>.ts          # Rare handwritten extensions, named to avoid clashing with generated code
│
│   Existing modules (excerpt):
│   systemAuth / systemUser / systemRole / systemMenu / systemConfig /
│   globalMenu / globalDictType / globalLanguage / globalPlatform /
│   gameAppReleaseVersion / gameAppAssetPackage / gameHomeEntity /
│   gameOrderEntity / gameMailEntity / gameRedeemCodeEntity /
│   platformCaptchaConfig / platformJwtConfig / platformPasswordPolicyConfig …
│
├── api-services/                 # Legacy directory, already emptied, ignore it
│
├── composables/                    # Composables (※)
│   ├── useCrudTable.ts             Table pagination / sorting / loading wrapper
│   ├── useEditDialog.ts            Add / edit dialog (strips audit fields, isEnabled default)
│   ├── useTableColumnPersistence.ts  Local persistence of column width / order / visibility
│   ├── useCurrentApp.ts            Current app (AppId) state + load / switch
│   ├── useConfigAppScope.ts        App-scope changes
│   ├── useRequiredFields.ts        Required-field sets
│   ├── usePrimeTablePage.ts        Table pagination hook
│   ├── useRouteAppIdQuerySync.ts   Syncs AppId query with the table
│   ├── formResolver.ts             Form validation rule adapter
│   └── types/                      TS types for composables
│
├── components/                   # Shared components
│   ├── crud-data-table/            CRUD wrapper over PrimeVue DataTable
│   ├── crud-form-fields/           Unified form field rendering
│   ├── base-edit-dialog/           Base for add / edit dialogs
│   ├── search-form/                Search bar, supports authPrefix to auto-add permission buttons
│   ├── time-column/                Table column: auto-formats long seconds
│   ├── form-label / form-help-text Form field label / help text
│   ├── app-confirm-dialog          Global confirm dialog
│   ├── config-app-scope-picker     App-scope picker
│   ├── narrow-screen-hint          Narrow-screen hint
│   ├── notice-bar                  Notice bar
│   ├── cropper / drag-verify / editor / icon-selector / reward-items-editor / svg-icon / table
│
├── directive/                    # Custom directives
│   ├── authDirective.ts            v-auth — permission-based hiding
│   ├── customDirective.ts          v-waves / v-drag / v-reclick (anti double-click)
│   ├── blockLoading.ts             v-block-loading — full-block loading overlay
│   └── index.ts                    Registration entry
│
├── i18n/                         # Internationalization
│   ├── lang/                       Framework level (zh-cn / en / zh-tw / ja / ko)
│   └── pages/                      Business level, per-module subdirectories
│       ├── system/ / client/ / game/ / home/ / login/ / menu/
│       ├── personal/ / layout/ / errors/ / formI18n/ / chart/
│       └── platform* / global*Config/
│
├── layout/                       # Layout
│   ├── splitter/                   Main layout container
│   ├── navMenu/                    Left / horizontal menus
│   ├── navBars/                    Top bar (user / notifications / fullscreen / theme)
│   ├── routerView/                 keep-alive container
│   ├── footer / logo / upgrade     Other layout elements
│   └── index.vue                   Layout entry
│
├── router/                       # Routing (※)
│   ├── index.ts                    Router instance + guards
│   ├── frontEnd.ts                 Frontend-controlled routes (fallback, currently unused)
│   ├── backEnd.ts                  Core of backend dynamic routing
│   ├── appWorkspaceRoutes.ts       App workspace routes
│   ├── globalConfigRoutes.ts       Global config routes
│   └── route.ts                    Static / fallback routes (/login / 404 / 401)
│
├── stores/                       # Pinia state
│   ├── themeConfig.ts              Theme / layout / dark mode / watermark / globalI18n / isRequestRoutes
│   ├── userInfo.ts                 User info + authBtnList + constList + dictList
│   ├── routesList.ts               Menu tree
│   ├── tagsViewRoutes.ts           Tabs bar
│   ├── keepAliveNames.ts           List of names cached by keep-alive
│   └── index.ts                    Pinia instance + persistence adapter
│
├── theme/                        # SCSS themes
│   ├── z-index.scss                Unified z-index scale
│   ├── breakpoints.scss            Breakpoints (--bp-lg = 1024)
│   ├── primevue-overrides.scss     PrimeVue token overrides
│   └── common/transition.scss      Common transitions
│
├── types/                        # TS type declarations
│
├── utils/                        # Utility functions (keep them pure)
│   ├── request.ts                  axios instance for handwritten APIs
│   ├── axios-utils.ts              HttpClient for generated APIs + getAPI singleton cache
│   ├── http/shared.ts              ※ Shared interceptors (JWT / refresh / error-code i18n)
│   ├── storage.ts                  Local / Session (keys prefixed with __NEXT_NAME__:)
│   ├── authFunction.ts             Functional permissions: auth() / hasAnyAuth()
│   ├── message.ts                  showSuccess / showError / showConfirm (PrimeVue Toast)
│   ├── watermark.ts                Watermark
│   ├── isEnabled.ts                IsEnabled normalization and add defaults
│   ├── rowVersion.ts               Row version reading (supports rowVersion / RowVersion)
│   ├── targetTenant.ts             Cross-tenant header (X-Target-Tenant-Id)
│   ├── primevue-locale.ts          PrimeVue components switch by language
│   ├── formatTime.ts               Date / time formatting
│   ├── download.ts / exportExcel.ts  Exports
│   ├── sanitize.ts                 DOMPurify wrapper
│   ├── fontawesome.ts              FA automatic on-demand loading
│   ├── icons.ts                    FA name mapping
│   ├── build.ts                    CDN config
│   ├── appWorkspace.ts             ※ App-scope utilities (resolveRouteScope, etc.)
│   ├── arrayOperation.ts / commonFunction.ts / base64Conver.ts / crudConfig.ts / getStyleSheets.ts / loading.ts / mitt.ts / other.ts / template-utils.ts
│   └── http/                       Interceptor split
│
├── views/                        # Business pages
│   ├── system/                     System management (users / roles / menus / orgs / tenants / dictionaries / logs / config / notifications / plugins / printing / regions / online users …)
│   ├── game/                       Game management (zones / goods / orders / mail / redeem codes / shop / announcements / items / props …)
│   ├── client/                     Client management (platforms / versions / channels / devices / languages / asset packages / open APIs …)
│   ├── appCenter/                  App center (appEntry / appSelector / appWorkspace / gameAppEntity, etc.)
│   ├── contentCenter / chart / chartCenter / chartCenter / opsCenter / trustCenter / automationCenter / goodsCenter / redeemCenter / webShopCenter / playerCenter
│   ├── home/                       Home dashboard
│   ├── login/                      Login
│   ├── personal/                   Personal center
│   ├── error/                      404 / 401
│   └── global/                     Global route container
│
├── App.vue                       # Root component
├── main.ts                       # ※ App entry
├── auto-imports.d.ts             # Generated by unplugin-auto-import
└── components.d.ts               # Generated by unplugin-vue-components
```

### scripts/ in detail

```
scripts/
├── gen-api.mjs                   # pnpm gen:api — programmatic generation with swagger-typescript-api
├── post-gen.mjs                  # Splits + reassembles generated output into src/api/<module>/
├── migrate-api-services.mjs      # Historical migration script
├── migrate-to-api-dir.mjs        # Merges api-services/ into api/
├── check-i18n-keys.mjs           # pnpm run lint:i18n — 5-language key completeness
├── check-frontend-standards.mjs  # pnpm run check:frontend — cross-end standards gate
│   ├─ modes: all / routes / permissions / api / api-query-get
│   │         / text / sys-abbreviation / text-baseline / self-test
│   ├─ routes         backend menu component ↔ src/views/<file>
│   ├─ permissions    frontend v-auth declarations ↔ backend seed Btn
│   ├─ api            handwritten vs generated API overlap review
│   ├─ api-query-get  page/list/query style endpoints forbid GET with params
│   ├─ text           scans changed files for hardcoded Chinese
│   └─ sys-abbreviation C179 regression: no Sys/sys business abbreviations
├── smoke-test.mjs                # Smoke tests
├── fix-collisions.mjs            # Collision fixes
├── frontend-standards-allowlist.json   # Standards exemptions list
└── i18n-text-baseline.json       # Hardcoded-Chinese exemption baseline (C151 legacy debt)
```

---

## Quick Start

### Environment

- Node.js ≥ 16 (20 recommended)
- pnpm ≥ 11.5.2 (auto-enabled via `corepack enable`)

### Run it

```bash
git clone https://github.com/GameFrameX/Admin.git
cd Admin/GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # defaults to http://localhost:20800
```

Open the browser → see the login page → ask a backend colleague for an account.

### Stuck?

| Symptom | Most likely cause |
|---|---|
| `pnpm: command not found` | Not installed: `npm i -g pnpm@11.5.2` |
| Port 20800 occupied | Change `VITE_PORT=9000` in `.env`, restart dev |
| Blank browser page | Check the last lines of the terminal; Vite prints compile errors |
| Login fails | No backend / backend not running / wrong `VITE_API_URL` in `.env.development` |

### Environment variables

| Variable | Description | Dev default |
|---|---|---|
| `VITE_API_URL` | Backend API base URL | `http://127.0.0.1:20802` |
| `VITE_PORT` | Dev port | `20800` |
| `VITE_OPEN` | Whether to open the browser automatically | `false` |
| `VITE_OPEN_CDN` | Whether to enable external CDN | `false` |
| `VITE_PUBLIC_PATH` | Production public path | empty |

At startup Vite writes these values into `window.__env__` in `public/config/config.js`. In Docker deployments, `entrypoint.sh` regenerates this file from container environment variables, so **the same image can switch environments via environment variables**.

---

## How It Works

### Startup flow (`src/main.ts`)

`index.html` loads `public/config/config.js` (writing `window.__env__`) → loads `src/main.ts` → mounts in order:

1. Pinia (with persistence plugin)
2. Router (hash mode)
3. i18n (auto-globbed from `src/i18n/**`)
5. PrimeVue + Aura theme + ToastService + ConfirmationService
6. `@vue-js-cron/light`, `vue-grid-layout`, `vue-signature-pad`, `vue3-tree-org`
7. Custom directives (v-auth / v-waves / v-reclick / v-block-loading)

`App.vue` then does four things: watches route changes to set the title, initializes the dark theme, injects the watermark, binds the PrimeVue locale, and initializes the SignalR messaging service.

### Login and menu registration

Open the browser, see the login page → enter credentials → the frontend calls `/api/systemAuth/login` → the backend validates and issues a JWT → the frontend stores the token in `localStorage` → redirects to `/dashboard/home`.

The route guard `router.beforeEach` (`src/router/index.ts`) notices "I have no menus yet" and fetches two endpoints in parallel:

- `/api/systemAuth/userInfo` → user info + the `buttons` permission array
- `/api/globalMenu/loginMenuTree` → the menu tree

The menu roughly looks like this (see it in DevTools Network):

```json
[
  { "path": "/dashboard",    "name": "dashboard",  "component": "/home/index" },
  { "path": "/system/user",  "name": "systemUser", "component": "/system/user/index" }
]
```

`backEnd.ts` uses `import.meta.glob('../views/**/*.{vue,tsx}')` to preload all view files into a table, `dynamicImport()` maps the `component` string to a component function, and finally `router.addRoute()` registers it.

**So menu changes take effect only after a backend restart + a fresh frontend login**. The frontend itself holds no menu list.

Button permissions work similarly: for `<Button v-auth="'systemUser:add'">`, the custom directive looks it up in the `buttons` array; if absent, the element is hidden with `display: none`.

### HTTP interceptors (`src/utils/http/shared.ts`)

Two axios instances share the same interceptor factory:

- `service` in `src/utils/request.ts` — for handwritten APIs, supports GET params serialization
- `sharedHttpClient` in `src/utils/axios-utils.ts` — for swagger-generated APIs, automatically adds `Accept-Language`

**Request interception** (4 steps):

1. Automatically adds `Authorization: Bearer <access-token>`; if the JWT is about to expire, also adds `X-Authorization: Bearer <x-refresh-token>` so the backend silently reissues it
2. Automatically adds `X-Target-Tenant-Id` (controlled by `utils/targetTenant.ts` on cross-tenant switches)
3. `request.ts` additionally serializes `data` of GET requests into a query string (`tansParams`)
4. `axios-utils.ts` additionally adds `Accept-Language: <globalI18n>`

**Response interception** (4 steps):

1. HTTP 401 → clears tokens and reloads (`clearAccessTokens`)
2. Response headers carrying `access-token` / `x-access-token` → automatically written to LocalStorage (silent token renewal)
3. Business code `code === 200` passes through directly; other error codes:
   - `errors.codes.<ERROR_CODE>` hits i18n → show the localized message
   - hits `VALIDATION.FAILED` → show the backend's original message (preserving field-level errors)
   - otherwise → show the backend `message`
4. Camelization (initial uppercase → lowercase) + error toasts via PrimeVue Toast

### App scope (multi-tenancy)

Each backend menu item carries `app_scope` (`platform` / `tenant` / `app`) and `menu_context`. The frontend marks them with `meta.appScope` + `meta.menuContext`:

| Scope | Meaning | Route example |
|---|---|---|
| `platform` | Platform level (visible to super admins, cross-tenant) | `/platform/*` |
| `tenant` | Tenant level (shared by all apps, no appId query) | `/dashboard/*`, `/system/*` |
| `app` | Inside an app workspace (must carry the `appId` query) | `/gameCenter/*` |

The redirect logic lives in `decideTenantAppEntry()` / `resolveRouteScope()` in `src/utils/appWorkspace.ts`, triggered inside the guard by `resolveTenantAppRedirect()` in `router/index.ts`.

### Pinia persistence

| Store | Storage | Key fields |
|---|---|---|
| `themeConfig` | localStorage | Theme / layout / dark mode / watermark / globalI18n / `isRequestRoutes` |
| `userInfo` | sessionStorage (only `pick: ['constList', 'dictList']`) | System constants + dictionaries (slow at startup, hence cached) |
| `routesList` | Not persisted | Current menu tree |
| `tagsViewRoutes` | Not persisted | Tabs bar |
| `keepAliveNames` | Not persisted | keep-alive include |

All keys automatically get the `__NEXT_NAME__:` prefix (via `Local.setKey`) to avoid cross-app conflicts.

---

## API and Backend Conventions

### Two API layers

| Layer | Location | Origin | Invocation |
|---|---|---|---|
| **Generated APIs** | `src/api/<module>/{index.ts,types.ts}` | programmatic swagger-typescript-api generation | `getAPI(SomeApi).someMethod()` |
| **Handwritten APIs** | `src/api/<business>.ts` | extensions / wrappers the business needs | `request({ url, method, data })` |

Calling pattern for generated APIs:

```ts
import { getAPI } from '/@/utils/axios-utils'
import { SystemUser } from '/@/api/systemUser'

const api = getAPI(SystemUser)                       // singleton cache, repeated calls cost nothing
const res = await api.systemUserPagePost({ page: 1, pageSize: 20 })
const user = res.data.result.items[0]                 // automatically normalized { code, result, ... }
```

Calling pattern for handwritten APIs:

```ts
import request from '/@/utils/request'

export const pageX = (params: PageParams) =>
  request<Result<PageResult<X>>>({
    url: '/api/game/x/page',
    method: 'post',
    data: params,
  })
```

### URL naming (dynamic API routing rules)

The backend uses its own `IDynamicApiController` (implemented by `Web.Core/ApiRouting/AdminApiRouteRules`) to generate URLs from service class names: **strip the `Service` / `AppService` / `Controller` suffix → lowercase the first letter → prefix with `api/`**.

| Backend class name | Frontend URL |
|---|---|
| `SystemAuthService` | `/api/systemAuth/...` |
| `GameAppService` | `/api/gameApp/...` |
| `SystemUserService` | `/api/systemUser/...` |

**Frontend URLs must never keep the `Service` suffix**. This is a pitfall 100% of newcomers step into.

### Error codes → i18n

Stable backend error codes map automatically to `errors.codes.<CODE_WITH_UNDERSCORE>` in `src/i18n/pages/errors/<lang>.ts`:

- `AUTH.FORBIDDEN` → `errors.codes.AUTH_FORBIDDEN`
- `D0008` → `errors.codes.D0008`
- `VALIDATION.FAILED` → passes the backend message through (preserving field-level errors)

For a new error code, just add the corresponding key in `errors/zh-cn.ts`.

### Backend contract change workflow

1. Backend changes the OpenAPI / Swagger
2. Export a new `openapi.json` → replace the one at this repository root
3. `pnpm gen:api` → review `git diff src/api/`
4. Field renames / removals → fix the frontend call sites (the TS compiler will error)
5. Run `pnpm run check:frontend api` (reviews handwritten vs generated overlap)
6. `pnpm run quality` must pass

---

## Adding a New Page

Follow along; once done you will have mastered 80% of the daily work. Let's add an "Announcements" list page.

### 1. Create the files

```
src/views/system/demo/
├── index.vue          # List page
└── component/
    └── editDemo.vue   # Add/edit dialog
```

### 2. Write the list page

The core structure of `src/views/system/demo/index.vue` (full styles omitted):

```vue
<template>
  <div class="p-3">
    <!-- Top: search + add -->
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

    <!-- Table -->
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

### 3. Write the dialog

`src/views/system/demo/component/editDemo.vue`:

```vue
<template>
  <BaseEditDialog
    v-model:visible="state.isShowDialog"
    :default-form="() => ({ title: '', content: '' })"
    :add-api="(data) => getAPI(DemoApi).demoAddPost(data)"
    :update-api="(data) => getAPI(DemoApi).demoUpdatePut(data)"
    :rules="{ title: { required: true, message: '请输入标题' } }"
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

`CrudDataTable` and `BaseEditDialog` are in-house wheels shipped with the project (`src/components/`); they encapsulate pagination / loading / action column / add-vs-edit branching / submit loading — **do not reinvent them**.

### 4. Add i18n (all 5 languages required)

`src/i18n/pages/system/demo/zh-cn.ts`:

```ts
export default {
  title: '通知公告',
  form: { title: '标题', content: '内容' },
}
```

The other 4 language files **must also be created** (even if just mechanically copied):

```
src/i18n/pages/system/demo/
├── zh-cn.ts
├── en.ts
├── zh-tw.ts
├── ja.ts
└── ko.ts
```

If any one is missing, `pnpm run lint:i18n` fails.

### 5. Have the backend attach the menu

The code is done but you still can't see the page — menus belong to the backend. Tell your backend colleague:

```
path:        /system/demo
name:        systemDemo
component:   /system/demo/index       ← must match the path under src/views/
title:       通知公告
titleCode:   system.demo.title        ← must exist in zh-cn.ts
parent:      /system
```

After the backend commits and deploys, log in again on the frontend and the menu appears.

### 6. Commit

```bash
pnpm run quality      # one command to run all gates
git add .
git commit -m "feat(system.demo): add announcements list + dialog"
git push
```

If `pnpm run quality` fails, fix per the errors: missing i18n keys → add translations; permissions not in the menu seed → ask the backend to add; hardcoded Chinese → move to i18n.

---

## Development Standards

> This section is **binding** — CI enforces it; violations mean build failure.

### Naming conventions

| Category | Style | Example |
|---|---|---|
| View directories / files | kebab-case | `src/views/system/user/index.vue` |
| API module directories | camelCase (backend swagger tag) | `src/api/systemUser/` |
| Pinia store files | camelCase | `themeConfig.ts` |
| Composables files | `useXxx` | `useEditDialog.ts` |
| Utility function files | camelCase | `authFunction.ts` |
| Permission strings | `<resource>:<action>` | `systemUser:page`, `gameHomeEntity:query` |
| Route names | camelCase | `systemUser` |
| Route paths | kebab-case | `/system/user` |
| i18n keys | dot-hierarchical | `system.user.title`, `common.button.search` |
| Error code keys | uppercase with underscores | `AUTH_FORBIDDEN` |

### Directory conventions

- A view `<module>` matches the first segment of the backend menu `component`, making human ↔ route ↔ source three-way lookup easy
- Dialogs live uniformly in `views/<module>/component/`
- Single-page dedicated components go in `views/<module>/component/`; **do not** stuff them into `src/components/` (that is the reuse pool)
- API modules must not share a name with generated output (`check:api` flags them for manual review)

### Permission standards

- In templates use `v-auth="'xxx'"` / `v-auth="['x','y']"` / `v-auth-all`
- In `v-if` / props use `auth('xxx')` or `hasAnyAuth('x','y')` (`src/utils/authFunction.ts`)
- Every permission string must appear in a `MenuTypeEnum.Btn` entry of the backend menu seed, otherwise `pnpm run check:permissions` fails
- Workflow for new permissions: backend adds the seed entry → frontend references it via `v-auth` → run `check:permissions`

### i18n standards

- **Every i18n key must exist in all 5 languages** (`zh-cn / en / zh-tw / ja / ko`); `pnpm run lint:i18n` checks automatically
- **Hardcoded Chinese is strictly forbidden** in templates and `<script setup>`; `pnpm run check:frontend text` scans changed files
- Legacy debt is exempted by fingerprint in `scripts/i18n-text-baseline.json`; during the migration you can extend the baseline incrementally via `pnpm run check:frontend text-baseline`
- For new error codes, add `errors.codes.<CODE_WITH_UNDERSCORE>` in `errors/zh-cn.ts`

### HTTP standards

- **page / list / query / search / filter style endpoints must not pass params / data via GET** — `pnpm run check:frontend api-query-get` will block it
- Backend-generated APIs must use `xxxPagePost` / `xxxListPost` / `xxxQueryPost`
- Handwritten APIs use `request({ method: 'post', data: params })`
- **Do not** manually add `Authorization` / `Accept-Language` / `X-Tenant-Id` in business code — the interceptors handle it automatically
- **Do not** manually call `showError` on responses — the interceptors handle business codes automatically

### Commit workflow

```bash
pnpm run quality      # must pass: lint + typecheck + test:unit + build + check:frontend
git add .
git commit -m "<scope>(<module>): <subject>"   # Chinese / English both fine, ≤ 60 chars
git push
```

> `pnpm run quality` chains everything as the master gate; since 2026-10-20 it runs Vitest unit/component tests between lint and build;
> unit-level regressions are blocked before build, saving integration cost. For new pure-logic utilities, add a colocated test in `src/<dir>/<name>.test.ts`;
> for new Vue components, follow `src/components/form-label/index.test.ts` (`mount` + injected providers + slot text assertions).
> During development you can run `pnpm run test:unit:watch` for the watch UI.

- scope: `feat / fix / refactor / docs / style / test / chore / perf`
- module: `system.user` / `api` / `router` / `docs` / `i18n`…
- body explains motivation + key changes
- branches: `feat/<module>-<desc>` / `fix/<module>-<desc>`

### Violation examples (do not write like this)

```ts
// ❌ axios.get('/api/xxx', { params }) — query-style endpoints forbid GET
const res = await axios.get('/api/user/page', { params })

// ✅
const res = await request({ url: '/api/user/page', method: 'post', data })

// ❌ localStorage.setItem('themeConfig', ...) — no automatic prefix
window.localStorage.setItem('themeConfig', JSON.stringify({...}))

// ✅
import { Local } from '/@/utils/storage'
Local.set('themeConfig', {...})

// ❌ URL keeps the Service suffix
const url = '/api/systemUserService/page'

// ✅
const url = '/api/systemUser/page'

// ❌ hardcoded Chinese in templates
<span>搜索</span>

// ✅
<span>{{ t('common.button.search') }}</span>

// ❌ handwriting the isShowDialog / openDialog / submit trio
defineExpose({ isShowDialog, openDialog, submit })

// ✅ use the useEditDialog composable
const { isShowDialog, openDialog, submit } = useEditDialog({ ... })
```

---

## FAQ and Pitfalls

### FAQ

**Why do menus / permissions / copy come from the backend?**
Because the frontend should not know "who can see what". The backend computes them by role and returns them; the frontend only renders. The backend ships a release and the frontend needs no change.

**What is a JWT? How does auto-renewal work?**
A movie ticket. The server hands you an encrypted ticket; requests carry `Authorization: Bearer xxx`. Before expiry the frontend automatically adds `X-Authorization` so the backend quietly reissues it.

**Why does i18n need 5 files?**
Because this is a multilingual product; CI enforces that `zh-cn / en / zh-tw / ja / ko` all exist. One missing means failure. New keys can be mechanically copied from zh-cn first; translations can catch up later.

**`getAPI(...)` or `request(...)` — which one?**
- `getAPI(SomeApi).someMethod()` — auto-generated APIs (`src/api/<module>/`)
- `request({ url, method })` — handwritten APIs (like `src/api/login.ts`)
Both share the same HTTP interceptors; do not roll your own axios.

**What are `v-auth` / `v-block-loading` / `v-reclick`?**
- `v-auth="'xxx'"` — no permission means `display: none`
- `v-block-loading="loading"` — adds a loading overlay over the whole block
- `v-reclick` — prevents button double-clicks
Defined in `src/directive/`; use them directly as attributes in templates.

**What is `src/api-services/` for?**
A legacy directory, deprecated. All newly generated APIs live in `src/api/<module>/`.

### Pitfalls

**Blank screen after login / menu never appears**
DevTools → Network: is `/api/systemAuth/login` a 200? If not → check `VITE_API_URL` and the backend; if yes → check `/userInfo` and `/loginMenuTree`; any non-200 redirects to `/login`. If both are 200 yet still blank → check Console for JS errors; most likely the component points to a nonexistent .vue.

**Menu opens but the page 404s**
Run `pnpm run check:routes`; most likely the backend menu's `component` path doesn't match `src/views/`.

**All buttons invisible**
Check whether the `buttons` field of the `/userInfo` response contains the expected strings; present but invisible → the `v-auth` string must match **exactly** (case-sensitive); absent entirely → talk to the backend; your role didn't get that permission.

**i18n reports a missing key**
Run `pnpm run lint:i18n` and add the missing keys across the 5 language files per the errors.

**`check:frontend text` explodes everywhere**
Check whether you added hardcoded Chinese → move it to i18n; otherwise it's legacy debt during the migration → extend the baseline incrementally with `pnpm run check:frontend text-baseline` (use with care).

**API address is still old after Docker deployment**
1. `docker exec -it <container> cat /usr/share/nginx/html/config/config.js` to inspect `window.__env__.VITE_API_URL`
2. Old → `-e VITE_API_URL=...` was not passed at startup
3. New value but old in the browser → hard refresh with Ctrl+F5 / Cmd+Shift+R
4. Still failing → the container was started from an old image; `docker pull` the latest

**Build OOMs**
Docker Desktop → Preferences → Resources → Memory to 8G+, Swap to 4G+. The build command already sets `NODE_OPTIONS=--max-old-space-size=4096`.

**No automatic redirect to login after 401**
Check whether `localStorage.access-token` has expired; the JWT payload `exp`; whether the backend `/api/systemAuth/refresh-token` returns a new token.

---

## Deployment

### Docker (recommended)

> Single-repository consolidation (2026-10-19): the Vue Dockerfile's build context has moved up to the repository root,
> and `build/default.conf` / `build/entrypoint.sh` were relocated to the repository-root `build/`. Run the build from the repository root.

```bash
docker build -t gameframex-admin-web:dev -f GameFrameX.Admin.Unified.Vue/Dockerfile .
docker run -d -p 80:80 gameframex-admin-web:dev                                          # default API address
docker run -d -p 80:80 -e VITE_API_URL=https://api.example.com gameframex-admin-web:dev   # custom API
```

The container has a small trick: `build/entrypoint.sh` rewrites `config.js` from environment variables at startup, so **the same image can switch environments via environment variables**.

### Full parameters of docker-build-push.sh

```bash
./build/docker-build-push.sh --tag v2.4.33
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --tag v2.4.33
./build/docker-build-push.sh \
  --platforms linux/amd64,linux/arm64 \
  --tag v2.4.33
./build/docker-build-push.sh --no-push                # build only, no push
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --username your_user \
  --password your_pass \
  --tag v2.4.33                                       # private registry auth
```

| Parameter | Description | Default |
|---|---|---|
| `--registry URL` | Container registry URL | empty (Docker Hub) |
| `--namespace NAME` | Namespace / user | `alianblank` |
| `--name NAME` | Image name | `gameframex-admin-web` |
| `--tag TAG` | Image tag | git tag or `branch-date-SHA` |
| `--platforms LIST` | Target platforms | `linux/amd64` |
| `--no-push` | Build only, no push | — |
| `--username USER` | Registry username | — |
| `--password PASS` | Registry password | — |

Image tag auto-generation rule: if the current commit has a git tag → use the tag; otherwise → `branchname-date-SHA`; `latest` is tagged as well.

### CNB pipeline

Pushes to `main` / `develop` build + deploy to the test environment automatically; tagging also tags `latest` + auto-deploys. See `.cnb.yml` for the detailed rules.

### Pure static

The `pnpm run build` output lands in `GameFrameX.Admin.Unified.Vue/build/dist/` (the `outDir` in `vite.config.ts` remains `build/dist` relative to cwd, effective for the subdirectory). Any HTTP server supporting SPA fallback (`try_files $uri /index.html`) can host it. The Nginx template is at the repository-root `build/default.conf` (moved out of the relocated Vue `build/` in the 2026-10-19 single-repo consolidation).

### Docker Compose example

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

## Command Cheat Sheet

| Command | What it does |
|---|---|
| `pnpm run dev` | Start the dev server |
| `pnpm run build` | Production build (output in `GameFrameX.Admin.Unified.Vue/build/dist/`, vite `outDir` relative to cwd) |
| `pnpm run lint` / `lint-fix` | ESLint check / auto-fix |
| `pnpm run format` / `format-fix` | Prettier check / auto-fix |
| `pnpm run typecheck` | tsc type checking |
| `pnpm run test:unit` / `test:unit:watch` / `test:unit:coverage` | Vitest unit / component tests (jsdom + @vue/test-utils, with v8 coverage) |
| `pnpm run test:e2e` / `test:e2e:headed` / `test:e2e:list` / `test:e2e:report` | Playwright end-to-end (system Edge, suites run serially) |
| `pnpm gen:api` | Regenerate the frontend API client from `openapi.json` |
| `pnpm run lint:i18n` | Check 5-language i18n key completeness |
| `pnpm run check:routes` | Align backend menu components with frontend views |
| `pnpm run check:permissions` | Align frontend permission declarations with backend seeds |
| `pnpm run check:api` | Review handwritten vs generated API overlap |
| `pnpm run check:text` | Scan for hardcoded Chinese (changed files fail) |
| `pnpm run check:frontend` | All of the above + Sys abbreviation regression |
| `pnpm run quality` | **Run all gates in one command**; must run before committing |

Prerequisite for `pnpm gen:api`: export `openapi.json` from the backend — see the next section.

---

## Exporting OpenAPI from the Backend

`pnpm gen:api` only reads the `openapi.json` at the repository root, but **the frontend does not generate it itself** — you pull it from the backend's Swagger endpoint.

The backend is .NET + Swashbuckle; the Swagger JSON is exposed at:

```
http://localhost:20802/swagger/web.api/swagger.json
```

> Swagger **is on only in the dev environment by default**. In production / Docker you must set `Swagger__Enabled=true` (or `Swagger:Enabled=true`) to expose this endpoint — this is the switch behavior in the backend's `Startup.cs`, so that documentation is not leaked in production by default.

```bash
# 1. Start the backend (either way makes Swagger reachable)
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
# or
Swagger__Enabled=true dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &

# 2. Pull swagger.json
curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o openapi.json
ls -lh openapi.json   # should be ~3 MB

# 3. Run the frontend generation
pnpm gen:api
git diff src/api/     # check for breaking changes
```

The Client / Hub APIs follow the same pattern: `/swagger/client.api/swagger.json` (port 20808) and `/swagger/hub.api/swagger.json`. The frontend Admin project uses only the Admin API, so you generally don't need those two.

### Automation script (optional)

If your team does this often, write a script (write it yourself; I didn't write it for you). Minimal skeleton:

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

## Final Words

The mantra:

> **Menus come from the backend, permissions come from the backend, internationalization needs all 5 languages, run `pnpm run quality` before committing.**

When stuck, don't grind endlessly: check the pitfalls first, then git log if unresolved, and finally ask a colleague.

To read concrete code, jump to:

- `src/router/index.ts` — route guards
- `src/router/backEnd.ts` — backend menu → frontend routes
- `src/utils/http/shared.ts` — HTTP interceptors
- `src/stores/userInfo.ts` — user info / button permissions
- `src/composables/useEditDialog.ts` — dialog core
- `src/utils/appWorkspace.ts` — multi-tenant app scope
- `scripts/check-frontend-standards.mjs` — cross-end standards gate

## Documentation & Resources

- Project documentation site: <https://gameframex.doc.alianblank.com>
- Project overview: [`../README.md`](../README.md)
- Backend operations manual: [`../GameFrameX.Admin.Unified.Api/README.md`](../GameFrameX.Admin.Unified.Api/README.md)
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

See [CHANGELOG.md](./CHANGELOG.md) for the version history.

## License

See [LICENSE](./LICENSE) for license information.

<!--
EN: See [LICENSE](./LICENSE) for license information.
zh-CN: 详见 [LICENSE](./LICENSE) 文件。
zh-TW: 詳見 [LICENSE](./LICENSE) 檔案。
ja: 詳しくは [LICENSE](./LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.
-->
