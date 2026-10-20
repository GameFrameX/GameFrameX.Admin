<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />

# GameFrameX Admin Web

[![License](https://img.shields.io/badge/license-blue.svg)](./LICENSE)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Admin.Unified.Vue)](https://github.com/GameFrameX/GameFrameX.Admin.Unified.Vue/releases)
[![Documentation](https://img.shields.io/badge/docs-gameframex.doc.alianblank.com-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

</div>

---

GameFrameX 游戏管理平台的前端工程，基于 Vue 3 + TypeScript + Vite + PrimeVue 构建。

> 本项目 fork 自 [vue-next-admin](https://gitee.com/lyt-top/vue-next-admin)，按 GameFrameX 的多租户 / 应用作用域模型做了改造。当前版本 `v2.4.33`。

## 目录

- [这是干什么的](#这是干什么的)
- [技术栈](#技术栈)
- [仓库目录结构](#仓库目录结构)
- [快速开始](#快速开始)
- [它是怎么工作的](#它是怎么工作的)
- [API 与后端约定](#api-与后端约定)
- [加一个新页面](#加一个新页面)
- [开发规范](#开发规范)
- [常见问题与踩坑](#常见问题与踩坑)
- [部署](#部署)
- [命令速查](#命令速查)
- [从后端导出 OpenAPI](#从后端导出-openapi)
- [写在最后](#写在最后)

---

## 这是干什么的

这是给 **GameFrameX 游戏管理平台**写的前端。运营、GM、客服、运维每天在浏览器里改菜单、加用户、看订单、推版本、看日志——**这个工程就是干这个的**。

后端是 .NET 写的，仓库在 [`GameFrameX.Admin.Unified.Api`](../GameFrameX.Admin.Unified.Api)；数据库默认 PostgreSQL。前端用的是 Vue 3 + Vite + PrimeVue + Pinia + Vue Router + vue-i18n + Axios 这些大路货，没有奇技淫巧。

**最值得知道的三件事**：

1. **菜单、权限、文案 全部由后端控制**。前端不写菜单，加菜单要去后端菜单管理里加；前端只负责渲染后端返回的菜单树。
2. **后端动态路由**。登录后前端会调 `/api/globalMenu/loginMenuTree` 拿菜单，再用 `component` 字段去 `src/views/` 找对应 `.vue` 文件，最后用 `router.addRoute()` 注册。所以加页面文件就够了，不用动路由文件。
3. **JWT 自动续期**。登录拿到的 token 存在 `localStorage`，过期前前端会在请求头里塞 `X-Authorization` 让后端静悄悄换发，所以你登录一次能爽很久。

---

## 技术栈

| 维度 | 选型 | 版本 |
|---|---|---|
| 框架 | Vue 3（Composition API + `<script setup lang="ts">`） | 3.5.x |
| 构建 | Vite | 6.x |
| UI 组件库 | PrimeVue（Aura 主题）+ PrimeFlex + PrimeIcons | 4.5.x |
| 状态管理 | Pinia + pinia-plugin-persistedstate | 3.x |
| 路由 | Vue Router（hash 模式，后端动态控制） | 4.x |
| 语言 | TypeScript（strict） | 5.3.x |
| 国际化 | vue-i18n（5 语言） | 11.x |
| 图表 | ECharts + echarts-gl + echarts-wordcloud | 6.x |
| 实时通信 | @microsoft/signalr | 10.x |
| 富文本 | wangEditor + editor-for-vue | 5.x |
| 代码编辑器 | Monaco Editor | 0.44.x |
| HTTP | Axios + 自研拦截器 | 1.x |
| 单元测试 | Vitest（jsdom + @vue/test-utils） | 3.x |
| 端到端测试 | Playwright（系统 Edge） | 1.64.x |
| 图标 | FontAwesome 7（vue-fontawesome + 自动按需） | 7.x |
| 表单 | @primevue/forms + 自研 formResolver | 4.5.x |
| 编辑器辅助 | code-inspector-plugin（按住 Shift 点击定位源码） | — |
| 包管理 | pnpm（Corepack 自动激活） | ≥ 11.5.2 |
| Node | — | ≥ 16（推荐 20） |

> Vue Router 是 **hash 模式**（`createWebHashHistory`），部署时不需要服务端 rewrite，但 URL 里会带 `#`。

---

## 仓库目录结构

### 顶层

```
GameFrameX.Admin.Unified.Vue/
├── src/                        # 业务源码（99% 时间在这里）
├── public/                     # 静态资源
│   └── config/config.js        # Vite 启动时生成的运行时配置
├── scripts/                    # 工程化脚本（gen-api、check-frontend 等）
├── openapi.json                # 后端 Swagger 导出，pnpm gen:api 的输入
├── build/                      # Docker / Nginx / 部署脚本
├── Dockerfile                  # 多阶段构建镜像
├── .cnb.yml                    # CNB 云原生构建 / 部署流水线
├── .env / .env.development / .env.production   # 环境变量
├── eslint.config.mjs           # ESLint 9 flat config
├── .prettierrc.json
├── tsconfig.json / tsconfig.typecheck.json
├── vite.config.ts              # Vite 配置
├── index.html                  # SPA 入口（注入 public/config/config.js）
├── package.json                # scripts / dependencies
├── pnpm-lock.yaml
├── pnpm-workspace.yaml
├── CHANGELOG.md
├── LICENSE                     # MIT
└── README.md                   # 本文件
```

### src/ 详解

```
src/
├── api/                          # 调后端的地方（⭐）
│   ├── _shared/                  # 自动生成的共享层（HttpClient + data-contracts）
│   ├── <module-camel>/           # 每个后端 tag 一个目录，含 index.ts（Api class）+ types.ts
│   │   ├── index.ts              # 自动生成 + 末尾导出顶层函数
│   │   └── types.ts              # 仅本模块用到的 data-contracts
│   └── <业务手写>.ts             # 极少数手写扩展，命名避免与生成物冲突
│
│   当前已有模块（节选）：
│   systemAuth / systemUser / systemRole / systemMenu / systemConfig /
│   globalMenu / globalDictType / globalLanguage / globalPlatform /
│   gameAppReleaseVersion / gameAppAssetPackage / gameHomeEntity /
│   gameOrderEntity / gameMailEntity / gameRedeemCodeEntity /
│   platformCaptchaConfig / platformJwtConfig / platformPasswordPolicyConfig …
│
├── api-services/                 # 历史遗留目录，已清空，忽略
│
├── composables/                    # 组合式函数（⭐）
│   ├── useCrudTable.ts             表格分页 / 排序 / loading 封装
│   ├── useEditDialog.ts            新增 / 编辑弹窗（剥离审计字段、isEnabled 默认）
│   ├── useTableColumnPersistence.ts  列宽 / 列顺序 / 列显隐本地持久化
│   ├── useCurrentApp.ts            当前应用（AppId）状态 + 加载 / 切换
│   ├── useConfigAppScope.ts        应用作用域变更
│   ├── useRequiredFields.ts        必填字段集合
│   ├── usePrimeTablePage.ts        表格分页 hook
│   ├── useRouteAppIdQuerySync.ts   AppId query 与表格同步
│   ├── formResolver.ts             表单校验规则适配
│   └── types/                      组合式函数的 TS 类型
│
├── components/                   # 通用组件
│   ├── crud-data-table/            PrimeVue DataTable 的 CRUD 封装
│   ├── crud-form-fields/           表单字段统一渲染
│   ├── base-edit-dialog/           新增 / 编辑弹窗基类
│   ├── search-form/                查询栏，支持 authPrefix 自动加权限按钮
│   ├── time-column/                表格列：long 秒自动格式化
│   ├── form-label / form-help-text 表单字段标签 / 帮助文字
│   ├── app-confirm-dialog          全局确认弹窗
│   ├── config-app-scope-picker     应用作用域选择器
│   ├── narrow-screen-hint          窄屏提示
│   ├── notice-bar                  通知条
│   ├── cropper / drag-verify / editor / icon-selector / reward-items-editor / svg-icon / table
│
├── directive/                    # 自定义指令
│   ├── authDirective.ts            v-auth — 权限隐藏
│   ├── customDirective.ts          v-waves / v-drag / v-reclick（防连点）
│   ├── blockLoading.ts             v-block-loading — 整块 loading 蒙层
│   └── index.ts                    注册入口
│
├── i18n/                         # 国际化
│   ├── lang/                       框架级（zh-cn / en / zh-tw / ja / ko）
│   └── pages/                      业务级，按模块子目录
│       ├── system/ / client/ / game/ / home/ / login/ / menu/
│       ├── personal/ / layout/ / errors/ / formI18n/ / chart/
│       └── platform* / global*Config/
│
├── layout/                       # 布局
│   ├── splitter/                   主布局容器
│   ├── navMenu/                    左侧 / 横向菜单
│   ├── navBars/                    顶栏（用户 / 通知 / 全屏 / 主题）
│   ├── routerView/                 keep-alive 容器
│   ├── footer / logo / upgrade     其他布局元素
│   └── index.vue                   布局入口
│
├── router/                       # 路由（⭐）
│   ├── index.ts                    路由实例 + 守卫
│   ├── frontEnd.ts                 前端控制路由（兜底，目前用不到）
│   ├── backEnd.ts                  后端动态路由核心
│   ├── appWorkspaceRoutes.ts       应用工作台路由
│   ├── globalConfigRoutes.ts       全局配置路由
│   └── route.ts                    静态 / 兜底路由（/login / 404 / 401）
│
├── stores/                       # Pinia 状态
│   ├── themeConfig.ts              主题 / 布局 / 暗色 / 水印 / globalI18n / isRequestRoutes
│   ├── userInfo.ts                 用户信息 + authBtnList + constList + dictList
│   ├── routesList.ts               菜单树
│   ├── tagsViewRoutes.ts           Tabs 栏
│   ├── keepAliveNames.ts           keep-alive 缓存的 name 列表
│   └── index.ts                    Pinia 实例 + 持久化适配器
│
├── theme/                        # SCSS 主题
│   ├── z-index.scss                统一 z-index 阶梯
│   ├── breakpoints.scss            断点（--bp-lg = 1024）
│   ├── primevue-overrides.scss     PrimeVue token 覆盖
│   └── common/transition.scss      通用过渡
│
├── types/                        # TS 类型声明
│
├── utils/                        # 工具函数（保持纯函数）
│   ├── request.ts                  手写 API 用的 axios 实例
│   ├── axios-utils.ts              生成 API 用的 HttpClient + getAPI 单例缓存
│   ├── http/shared.ts              ⭐ 共享拦截器（JWT / refresh / 错误码 i18n）
│   ├── storage.ts                  Local / Session（键加 __NEXT_NAME__: 前缀）
│   ├── authFunction.ts             auth() / hasAnyAuth() 函数式权限
│   ├── message.ts                  showSuccess / showError / showConfirm（PrimeVue Toast）
│   ├── watermark.ts                水印
│   ├── isEnabled.ts                IsEnabled 归一化与新增默认值
│   ├── rowVersion.ts               行版本读取（兼容 rowVersion / RowVersion）
│   ├── targetTenant.ts             跨租户 header（X-Target-Tenant-Id）
│   ├── primevue-locale.ts          PrimeVue 组件按语言切换
│   ├── formatTime.ts               日期 / 时间格式化
│   ├── download.ts / exportExcel.ts  导出
│   ├── sanitize.ts                 DOMPurify 封装
│   ├── fontawesome.ts              FA 自动按需
│   ├── icons.ts                    FA 名称映射
│   ├── build.ts                    CDN 配置
│   ├── appWorkspace.ts             ⭐ 应用作用域工具（resolveRouteScope 等）
│   ├── arrayOperation.ts / commonFunction.ts / base64Conver.ts / crudConfig.ts / getStyleSheets.ts / loading.ts / mitt.ts / other.ts / template-utils.ts
│   └── http/                       拦截器拆分
│
├── views/                        # 业务页面
│   ├── system/                     系统管理（用户 / 角色 / 菜单 / 组织 / 租户 / 字典 / 日志 / 配置 / 通知 / 插件 / 打印 / 区域 / 在线用户 ……）
│   ├── game/                       游戏管理（区服 / 商品 / 订单 / 邮件 / 兑换码 / 商城 / 公告 / 物品 / 道具 ……）
│   ├── client/                     客户端管理（平台 / 版本 / 渠道 / 设备 / 语言 / 资源包 / 开放接口 ……）
│   ├── appCenter/                  应用中心（appEntry / appSelector / appWorkspace / gameAppEntity 等）
│   ├── contentCenter / chart / chartCenter / chartCenter / opsCenter / trustCenter / automationCenter / goodsCenter / redeemCenter / webShopCenter / playerCenter
│   ├── home/                       首页仪表盘
│   ├── login/                      登录
│   ├── personal/                   个人中心
│   ├── error/                      404 / 401
│   └── global/                     全局路由容器
│
├── App.vue                       # 根组件
├── main.ts                       # ⭐ 应用入口
├── auto-imports.d.ts             # unplugin-auto-import 生成
└── components.d.ts               # unplugin-vue-components 生成
```

### scripts/ 详解

```
scripts/
├── gen-api.mjs                   # pnpm gen:api — 用 swagger-typescript-api 程序化生成
├── post-gen.mjs                  # 生成产物拆分 + 重组到 src/api/<module>/
├── migrate-api-services.mjs      # 历史迁移脚本
├── migrate-to-api-dir.mjs        # 把 api-services/ 合并到 api/
├── check-i18n-keys.mjs           # pnpm run lint:i18n — 5 语言键完整性
├── check-frontend-standards.mjs  # pnpm run check:frontend — 跨端规范门禁
│   ├─ modes: all / routes / permissions / api / api-query-get
│   │         / text / sys-abbreviation / text-baseline / self-test
│   ├─ routes         后端菜单 component ↔ src/views/<file>
│   ├─ permissions    前端 v-auth 声明 ↔ 后端种子 Btn
│   ├─ api            手写 vs 生成 API 重叠 review
│   ├─ api-query-get  page/list/query 类接口禁止 GET 传 params
│   ├─ text           changed files 硬编码中文扫描
│   └─ sys-abbreviation C179 回归：禁 Sys/sys 业务缩写
├── smoke-test.mjs                # 冒烟测试
├── fix-collisions.mjs            # 碰撞修复
├── frontend-standards-allowlist.json   # 规范豁免清单
└── i18n-text-baseline.json       # 硬编码中文豁免基线（C151 历史欠债）
```

---

## 快速开始

### 环境

- Node.js ≥ 16（推荐 20）
- pnpm ≥ 11.5.2（通过 `corepack enable` 自动激活）

### 跑起来

```bash
git clone <仓库地址>
cd GameFrameX.Admin.Unified.Vue
corepack enable
pnpm install
pnpm run dev    # 默认 http://localhost:20800
```

打开浏览器 → 看到登录页 → 找后端同学要账号。

### 卡住了？

| 现象 | 大概率原因 |
|---|---|
| `pnpm: command not found` | 没装：`npm i -g pnpm@11.5.2` |
| 端口 20800 被占 | 改 `.env` 的 `VITE_PORT=9000`，重启 dev |
| 浏览器白屏 | 看终端最后几行，Vite 会吐编译错误 |
| 登录失败 | 没有后端 / 后端没跑 / `.env.development` 的 `VITE_API_URL` 不对 |

### 环境变量

| 变量 | 说明 | 开发默认值 |
|---|---|---|
| `VITE_API_URL` | 后端 API 基址 | `http://127.0.0.1:20802` |
| `VITE_PORT` | dev 端口 | `20800` |
| `VITE_OPEN` | 是否自动开浏览器 | `false` |
| `VITE_OPEN_CDN` | 是否启用 CDN 外置 | `false` |
| `VITE_PUBLIC_PATH` | 生产公共路径 | 空 |

Vite 启动时会把这些值写入 `public/config/config.js` 的 `window.__env__`。Docker 部署时 `entrypoint.sh` 会根据容器环境变量重新生成这个文件，**同一个镜像可以靠环境变量切不同环境**。

---

## 它是怎么工作的

### 启动流程（`src/main.ts`）

`index.html` 加载 `public/config/config.js`（写入 `window.__env__`）→ 加载 `src/main.ts` → 按顺序挂载：

1. Pinia（含持久化插件）
2. Router（hash 模式）
3. i18n（从 `src/i18n/**` 自动 glob）
5. PrimeVue + Aura 主题 + ToastService + ConfirmationService
6. `@vue-js-cron/light`、`vue-grid-layout`、`vue-signature-pad`、`vue3-tree-org`
7. 自定义指令（v-auth / v-waves / v-reclick / v-block-loading）

`App.vue` 再做四件事：监听路由改 title、初始化暗色主题、注入水印、绑定 PrimeVue Locale、初始化 SignalR 消息服务。

### 登录与菜单注册

打开浏览器看到登录页 → 输入账号 → 前端发 `/api/systemAuth/login` → 后端校验 + 签发 JWT → 前端把 token 存 `localStorage` → 跳到 `/dashboard/home`。

路由守卫 `router.beforeEach`（`src/router/index.ts`）发现"我还没有菜单"，并行拉两个接口：

- `/api/systemAuth/userInfo` → 用户信息 + `buttons` 权限数组
- `/api/globalMenu/loginMenuTree` → 菜单树

菜单大致长这样（DevTools Network 里看）：

```json
[
  { "path": "/dashboard",    "name": "dashboard",  "component": "/home/index" },
  { "path": "/system/user",  "name": "systemUser", "component": "/system/user/index" }
]
```

`backEnd.ts` 用 `import.meta.glob('../views/**/*.{vue,tsx}')` 把所有视图文件预加载到一张表，再用 `dynamicImport()` 把 `component` 字符串映射成组件函数，最后 `router.addRoute()` 注册。

**所以菜单改了要后端重启 + 前端重新登录才生效**。前端本身不写菜单列表。

按钮权限类似：`<Button v-auth="'systemUser:add'">` 自定义指令去 `buttons` 数组里查，没有就 `display: none` 藏起来。

### HTTP 拦截器（`src/utils/http/shared.ts`）

两个 axios 实例共用同一份拦截器工厂：

- `src/utils/request.ts` 的 `service` —— 手写 API 用，支持 GET params 序列化
- `src/utils/axios-utils.ts` 的 `sharedHttpClient` —— swagger 生成 API 用，自动加 `Accept-Language`

**请求拦截**（4 步）：

1. 自动加 `Authorization: Bearer <access-token>`；如 JWT 即将过期，同时加 `X-Authorization: Bearer <x-refresh-token>`，让后端静默换发
2. 自动加 `X-Target-Tenant-Id`（跨租户切换时由 `utils/targetTenant.ts` 控制）
3. `request.ts` 额外把 GET 请求的 `data` 序列化为 query string（`tansParams`）
4. `axios-utils.ts` 额外加 `Accept-Language: <globalI18n>`

**响应拦截**（4 步）：

1. HTTP 401 → 清 token 并 reload（`clearAccessTokens`）
2. 响应头带回 `access-token` / `x-access-token` → 自动写入 LocalStorage（token 静默换发）
3. 业务码 `code === 200` 直接放行；其它错误码：
   - `errors.codes.<ERROR_CODE>` 命中 i18n → 显示本地化文案
   - 命中 `VALIDATION.FAILED` → 显示后端原始 message（保留字段级错误）
   - 其它 → 显示后端 `message`
4. 驼峰化（首字母大写 → 小写）+ 错误提示走 PrimeVue Toast

### 应用作用域（多租户）

后端菜单每项带 `app_scope`（`platform` / `tenant` / `app`）与 `menu_context`。前端用 `meta.appScope` + `meta.menuContext` 标记：

| Scope | 含义 | 路由示例 |
|---|---|---|
| `platform` | 平台级（超管可见，跨租户） | `/platform/*` |
| `tenant` | 租户级（所有应用共有，无 appId query） | `/dashboard/*`、`/system/*` |
| `app` | 应用工作台内（必须带 `appId` query） | `/gameCenter/*` |

跳转逻辑在 `src/utils/appWorkspace.ts` 的 `decideTenantAppEntry()` / `resolveRouteScope()`，由 `router/index.ts` 的 `resolveTenantAppRedirect()` 在守卫里触发。

### Pinia 持久化

| Store | 存储 | 关键字段 |
|---|---|---|
| `themeConfig` | localStorage | 主题 / 布局 / 暗色 / 水印 / globalI18n / `isRequestRoutes` |
| `userInfo` | sessionStorage（仅 `pick: ['constList', 'dictList']`） | 系统常量 + 字典（启动慢，所以缓存） |
| `routesList` | 不持久化 | 当前菜单树 |
| `tagsViewRoutes` | 不持久化 | Tabs 栏 |
| `keepAliveNames` | 不持久化 | keep-alive include |

所有键自动加 `__NEXT_NAME__:` 前缀（通过 `Local.setKey`），避免跨应用冲突。

---

## API 与后端约定

### 两层 API

| 层 | 位置 | 来源 | 调用 |
|---|---|---|---|
| **生成 API** | `src/api/<module>/{index.ts,types.ts}` | swagger-typescript-api 程序化生成 | `getAPI(SomeApi).someMethod()` |
| **手写 API** | `src/api/<业务>.ts` | 业务需要的扩展 / 封装 | `request({ url, method, data })` |

生成 API 的调用范式：

```ts
import { getAPI } from '/@/utils/axios-utils'
import { SystemUser } from '/@/api/systemUser'

const api = getAPI(SystemUser)                       // 单例缓存，多次调用无开销
const res = await api.systemUserPagePost({ page: 1, pageSize: 20 })
const user = res.data.result.items[0]                 // 自动归一化的 { code, result, ... }
```

手写 API 的调用范式：

```ts
import request from '/@/utils/request'

export const pageX = (params: PageParams) =>
  request<Result<PageResult<X>>>({
    url: '/api/game/x/page',
    method: 'post',
    data: params,
  })
```

### URL 命名（动态 API 路由规则）

后端用自研 `IDynamicApiController`（由 `Web.Core/ApiRouting/AdminApiRouteRules` 实现）自动从 service 类名生成 URL：**去掉 `Service` / `AppService` / `Controller` 后缀 → 首字母小写 → 加 `api/` 前缀**。

| 后端类名 | 前端 URL |
|---|---|
| `SystemAuthService` | `/api/systemAuth/...` |
| `GameAppService` | `/api/gameApp/...` |
| `SystemUserService` | `/api/systemUser/...` |

**前端 URL 严禁保留 `Service` 后缀**。这是新人 100% 会踩的坑。

### 错误码 → i18n

后端稳定错误码会自动映射到 `src/i18n/pages/errors/<lang>.ts` 的 `errors.codes.<CODE_WITH_UNDERSCORE>`：

- `AUTH.FORBIDDEN` → `errors.codes.AUTH_FORBIDDEN`
- `D0008` → `errors.codes.D0008`
- `VALIDATION.FAILED` → 透传后端 message（保留字段级错误）

新错误码在 `errors/zh-cn.ts` 加对应 key 即可。

### 后端契约变更流程

1. 后端改 OpenAPI / Swagger
2. 导出新 `openapi.json` → 替换本仓库根
3. `pnpm gen:api` → 检查 `git diff src/api/`
4. 字段重命名 / 移除 → 修前端调用处（TS 编译会报错）
5. `pnpm run check:frontend api` 跑一遍（看手写与生成重叠）
6. `pnpm run quality` 必过

---

## 加一个新页面

跟着做，做完你就掌握了 80% 的日常工作。我们加一个「通知公告」列表页。

### 1. 建文件

```
src/views/system/demo/
├── index.vue          # 列表页
└── component/
    └── editDemo.vue   # 新增/编辑弹窗
```

### 2. 写列表页

`src/views/system/demo/index.vue` 的核心结构（不展示完整样式）：

```vue
<template>
  <div class="p-3">
    <!-- 顶部：搜索 + 新增 -->
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

### 3. 写弹窗

`src/views/system/demo/component/editDemo.vue`：

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

`CrudDataTable` 和 `BaseEditDialog` 是项目自带的轮子（`src/components/`），封装了分页 / loading / 操作列 / 新增编辑判断 / 提交 loading，**别自己造轮子**。

### 4. 加 i18n（5 个语言必须齐全）

`src/i18n/pages/system/demo/zh-cn.ts`：

```ts
export default {
  title: '通知公告',
  form: { title: '标题', content: '内容' },
}
```

另外 4 个语言文件**必须建**（哪怕只是机械复制）：

```
src/i18n/pages/system/demo/
├── zh-cn.ts
├── en.ts
├── zh-tw.ts
├── ja.ts
└── ko.ts
```

少了任何一个，`pnpm run lint:i18n` 会 fail。

### 5. 让后端挂菜单

代码写完了但你还看不到页面——菜单是后端管的。告诉后端同学：

```
path:        /system/demo
name:        systemDemo
component:   /system/demo/index       ← 要和 src/views/ 里路径一致
title:       通知公告
titleCode:   system.demo.title        ← 要在 zh-cn.ts 里存在
parent:      /system
```

后端 commit + 部署后，前端重新登录就能看到菜单。

### 6. 提交

```bash
pnpm run quality      # 一键跑所有门禁
git add .
git commit -m "feat(system.demo): 新增通知公告列表 + 弹窗"
git push
```

`pnpm run quality` 挂了按错误修：i18n 缺 key 补翻译、权限没在菜单种子找后端加、硬编码中文走 i18n。

---

## 开发规范

> 这一节是**约束**——CI 会卡，违反就是 build fail。

### 命名约定

| 类别 | 风格 | 示例 |
|---|---|---|
| 视图目录 / 文件 | kebab-case | `src/views/system/user/index.vue` |
| API 模块目录 | camelCase（后端 swagger tag） | `src/api/systemUser/` |
| Pinia store 文件 | camelCase | `themeConfig.ts` |
| Composables 文件 | `useXxx` | `useEditDialog.ts` |
| 工具函数文件 | camelCase | `authFunction.ts` |
| 权限字符串 | `<resource>:<action>` | `systemUser:page`、`gameHomeEntity:query` |
| 路由 name | camelCase | `systemUser` |
| 路由 path | kebab-case | `/system/user` |
| i18n key | 点号分层 | `system.user.title`、`common.button.search` |
| 错误码 key | 大写下划线 | `AUTH_FORBIDDEN` |

### 目录约定

- 视图 `<module>` 与后端菜单 `component` 第一段一致，方便人脑 ↔ 路由 ↔ 源码 三向定位
- 弹窗统一放 `views/<module>/component/`
- 单页面专用组件放 `views/<module>/component/`，**不要**塞到 `src/components/`（那是复用池）
- API 模块不与生成物同名（`check:api` 会标出供人工 review）

### 权限规范

- 模板里用 `v-auth="'xxx'"` / `v-auth="['x','y']"` / `v-auth-all`
- `v-if` / props 里用 `auth('xxx')` 或 `hasAnyAuth('x','y')`（`src/utils/authFunction.ts`）
- 每个权限字符串必须出现在后端菜单种子的 `MenuTypeEnum.Btn` 条目里，否则 `pnpm run check:permissions` fail
- 加新权限的流程：后端加种子条目 → 前端 `v-auth` 引用 → 跑 `check:permissions`

### i18n 规范

- **任何 i18n 键必须 5 个语言都存在**（`zh-cn / en / zh-tw / ja / ko`），`pnpm run lint:i18n` 自动检查
- 模板和 `<script setup>` 中**严禁硬编码中文**，`pnpm run check:frontend text` 扫 changed files
- 历史欠债在 `scripts/i18n-text-baseline.json` 指纹豁免；迁移期可 `pnpm run check:frontend text-baseline` 增量基线
- 新错误码在 `errors/zh-cn.ts` 加 `errors.codes.<CODE_WITH_UNDERSCORE>`

### HTTP 规范

- **page / list / query / search / filter 类接口禁止用 GET 传 params / data**——`pnpm run check:frontend api-query-get` 会卡
- 后端生成 API 必须用 `xxxPagePost` / `xxxListPost` / `xxxQueryPost`
- 手写 API 用 `request({ method: 'post', data: params })`
- **不要**在业务代码里手动加 `Authorization` / `Accept-Language` / `X-Tenant-Id`——拦截器自动处理
- **不要**在响应里手动 `showError`——拦截器自动处理业务码

### 提交流程

```bash
pnpm run quality      # 必过：lint + typecheck + test:unit + build + check:frontend
git add .
git commit -m "<scope>(<module>): <subject>"   # 中文 / 英文都可，≤ 60 字
git push
```

> `pnpm run quality` 串起来的总闸，自 2026-10-20 起把 Vitest 单元/组件测试纳入 lint 与 build 之间；
> 单元层回归挡住就拦在 build 之前，省集成成本。新增纯逻辑工具请在 `src/<dir>/<name>.test.ts` 同位补单测；
> 新增 Vue 组件请参照 `src/components/form-label/index.test.ts`（`mount` + 注入 provider + slot 文本断言）。
> 开发期可跑 `pnpm run test:unit:watch` 走 watch UI。

- scope: `feat / fix / refactor / docs / style / test / chore / perf`
- module: `system.user` / `api` / `router` / `docs` / `i18n`…
- body 解释动机 + 关键改动
- 分支：`feat/<module>-<desc>` / `fix/<module>-<desc>`

### 违规示例（不要这么写）

```ts
// ❌ axios.get('/api/xxx', { params }) — query 类接口禁止 GET
const res = await axios.get('/api/user/page', { params })

// ✅
const res = await request({ url: '/api/user/page', method: 'post', data })

// ❌ localStorage.setItem('themeConfig', ...) — 不会自动加前缀
window.localStorage.setItem('themeConfig', JSON.stringify({...}))

// ✅
import { Local } from '/@/utils/storage'
Local.set('themeConfig', {...})

// ❌ URL 带 Service 后缀
const url = '/api/systemUserService/page'

// ✅
const url = '/api/systemUser/page'

// ❌ 模板硬编码中文
<span>搜索</span>

// ✅
<span>{{ t('common.button.search') }}</span>

// ❌ 手写 isShowDialog / openDialog / submit 三件套
defineExpose({ isShowDialog, openDialog, submit })

// ✅ 用 useEditDialog composable
const { isShowDialog, openDialog, submit } = useEditDialog({ ... })
```

---

## 常见问题与踩坑

### FAQ

**菜单 / 权限 / 文案 为什么要从后端来？**
因为前端不该知道"谁能看什么"。后端根据角色算好返回，前端只负责渲染。后端改了发版，前端不用动。

**JWT 是啥？怎么自动续期？**
电影票。服务器给你一张加密的票，请求头带 `Authorization: Bearer xxx`。过期前前端自动加 `X-Authorization` 让后端静悄悄换发。

**为什么 i18n 要 5 个文件？**
因为是多语言产品，CI 强制要求 `zh-cn / en / zh-tw / ja / ko` 都存在。少一个 fail。新写的 key 可以先机械复制 zh-cn，翻译慢慢补。

**`getAPI(...)` 和 `request(...)` 哪个用？**
- `getAPI(SomeApi).someMethod()` —— 自动生成的 API（`src/api/<module>/`）
- `request({ url, method })` —— 手写 API（`src/api/login.ts` 这种）
两种共用同一份 HTTP 拦截器，别自己造 axios。

**`v-auth` / `v-block-loading` / `v-reclick` 是啥？**
- `v-auth="'xxx'"` —— 没权限 `display: none`
- `v-block-loading="loading"` —— 整块加 loading 蒙层
- `v-reclick` —— 防按钮连点
定义在 `src/directive/`，模板里直接当 attribute 用。

**`src/api-services/` 是干啥的？**
历史遗留目录，已废弃。新生成 API 全部在 `src/api/<module>/`。

### 踩坑

**登录后白屏 / 菜单出不来**
DevTools → Network 看 `/api/systemAuth/login` 是不是 200。不是 → 检查 `VITE_API_URL` 和后端；是 → 看 `/userInfo` 和 `/loginMenuTree`，任一非 200 就会跳 `/login`。都 200 还白屏 → Console 看 JS 报错，多半是 component 指向了不存在的 .vue。

**菜单能进但页面 404**
跑 `pnpm run check:routes`，多半是后端菜单的 `component` 路径和 `src/views/` 对不上。

**按钮全看不到**
Network 看 `/userInfo` 响应里的 `buttons` 字段有没有期待的字符串；有但看不到 → `v-auth` 字符串**完全一致**（区分大小写）；都没有 → 找后端同学，你的角色没拿到这个权限。

**i18n 报 missing key**
跑 `pnpm run lint:i18n`，按报错补 5 个语言文件里缺的 key。

**`check:frontend text` 全炸**
看是不是新增了硬编码中文 → 走 i18n；否则是历史欠债在迁移期 → 用 `pnpm run check:frontend text-baseline` 增量基线（谨慎）。

**Docker 部署后 API 地址还是旧的**
1. `docker exec -it <container> cat /usr/share/nginx/html/config/config.js` 看 `window.__env__.VITE_API_URL`
2. 是旧的 → 启动时没传 `-e VITE_API_URL=...`
3. 是新值但浏览器是旧的 → Ctrl+F5 / Cmd+Shift+R 强刷
4. 还不行 → 容器是旧镜像起的，`docker pull` 拉最新

**构建 OOM**
Docker Desktop → Preferences → Resources → Memory 调到 8G+，Swap 调到 4G+。构建命令已加 `NODE_OPTIONS=--max-old-space-size=4096`。

**401 后没自动跳登录**
检查 `localStorage.access-token` 是否过期；JWT payload `exp`；后端 `/api/systemAuth/refresh-token` 是否返回新 token。

---

## 部署

### Docker（推荐）

> 单仓整合（2026-10-19）：Vue Dockerfile 的 build context 已上移为仓库根，
> `build/default.conf` / `build/entrypoint.sh` 同步迁出至仓库根 `build/`。注意在仓库根执行构建。

```bash
docker build -t gameframex-admin-web:dev -f GameFrameX.Admin.Unified.Vue/Dockerfile .
docker run -d -p 80:80 gameframex-admin-web:dev                                          # 默认 API 地址
docker run -d -p 80:80 -e VITE_API_URL=https://api.example.com gameframex-admin-web:dev   # 自定义 API
```

容器有个小魔法：`build/entrypoint.sh` 启动时读环境变量重写 `config.js`，所以**同一个镜像可以靠环境变量切不同环境**。

### docker-build-push.sh 完整参数

```bash
./build/docker-build-push.sh --tag v2.4.33
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --tag v2.4.33
./build/docker-build-push.sh \
  --platforms linux/amd64,linux/arm64 \
  --tag v2.4.33
./build/docker-build-push.sh --no-push                # 只构建不推
./build/docker-build-push.sh \
  --registry registry.cn-hangzhou.aliyuncs.com \
  --namespace myns \
  --username your_user \
  --password your_pass \
  --tag v2.4.33                                       # 私有仓库认证
```

| 参数 | 说明 | 默认值 |
|---|---|---|
| `--registry URL` | 容器仓库地址 | 空（Docker Hub） |
| `--namespace NAME` | 命名空间 / 用户 | `alianblank` |
| `--name NAME` | 镜像名称 | `gameframex-admin-web` |
| `--tag TAG` | 镜像标签 | git tag 或 `分支-日期-SHA` |
| `--platforms LIST` | 目标平台 | `linux/amd64` |
| `--no-push` | 只构建不推 | — |
| `--username USER` | 仓库用户名 | — |
| `--password PASS` | 仓库密码 | — |

镜像标签自动生成规则：当前 commit 有 git tag → 用 tag；否则 → `分支名-日期-SHA`；同时打 `latest`。

### CNB 流水线

`main` / `develop` push 自动构建 + 部署测试环境；打 tag 同步打 `latest` + 自动部署。详细规则见 `.cnb.yml`。

### 纯静态

`pnpm run build` 产物在 `GameFrameX.Admin.Unified.Vue/build/dist/`（`vite.config.ts` 的 `outDir` 仍为相对 cwd 的 `build/dist`，相对子目录生效）。任何支持 SPA fallback（`try_files $uri /index.html`）的 HTTP 服务器都能托管。Nginx 模板在仓库根 `build/default.conf`（2026-10-19 单仓整合，已从上移的 Vue `build/` 迁出）。

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

| 命令 | 干嘛的 |
|---|---|
| `pnpm run dev` | 启动开发服务器 |
| `pnpm run build` | 生产构建（产物在 `GameFrameX.Admin.Unified.Vue/build/dist/`，vite `outDir` 相对 cwd） |
| `pnpm run lint` / `lint-fix` | ESLint 查 / 自动修 |
| `pnpm run format` / `format-fix` | Prettier 查 / 自动修 |
| `pnpm run typecheck` | tsc 类型检查 |
| `pnpm run test:unit` / `test:unit:watch` / `test:unit:coverage` | Vitest 单元 / 组件单测（jsdom + @vue/test-utils，含 v8 覆盖率） |
| `pnpm run test:e2e` / `test:e2e:headed` / `test:e2e:list` / `test:e2e:report` | Playwright 端到端（系统 Edge，套件串行） |
| `pnpm gen:api` | 从 `openapi.json` 重新生成前端 API 客户端 |
| `pnpm run lint:i18n` | 检查 5 语言 i18n 键完整性 |
| `pnpm run check:routes` | 后端菜单 component 与前端视图对齐 |
| `pnpm run check:permissions` | 前端权限声明与后端种子对齐 |
| `pnpm run check:api` | 手写与生成 API 重叠 review |
| `pnpm run check:text` | 扫描硬编码中文（changed files fail） |
| `pnpm run check:frontend` | 上面所有 + Sys 缩写回归 |
| `pnpm run quality` | **一键跑所有门禁**，提交前必跑 |

`pnpm gen:api` 的前置：从后端导出 `openapi.json`——见下一节。

---

## 从后端导出 OpenAPI

`pnpm gen:api` 只认仓库根的 `openapi.json`，但**前端不会自己生成它**，要从后端 Swagger 端点拉。

后端是 .NET + Swashbuckle，Swagger JSON 暴露在：

```
http://localhost:20802/swagger/web.api/swagger.json
```

> Swagger **默认只在 dev 环境开**。生产 / Docker 必须设 `Swagger__Enabled=true`（或 `Swagger:Enabled=true`）才会暴露这个端点——这是后端 `Startup.cs` 的开关行为，目的是生产默认不外泄文档。

```bash
# 1. 起后端（任选一种让 Swagger 可访问）
ASPNETCORE_ENVIRONMENT=Development dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &
# 或
Swagger__Enabled=true dotnet run --project ../GameFrameX.Admin.Unified.Api/GameFrameX.Web.Api &

# 2. 拉 swagger.json
curl -sSL http://localhost:20802/swagger/web.api/swagger.json -o openapi.json
ls -lh openapi.json   # 应 ~3 MB

# 3. 跑前端生成
pnpm gen:api
git diff src/api/     # 看有没有破坏性变更
```

Client / Hub API 也用同样模式：`/swagger/client.api/swagger.json`（端口 20808）、`/swagger/hub.api/swagger.json`。前端 Admin 项目只用 Admin API，所以一般不用拉这两个。

### 自动化脚本（可选）

团队经常做这事可以写个脚本（自己写吧，我没替你写）。最小骨架：

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

## 写在最后

口诀：

> **菜单来自后端，权限来自后端，国际化必须 5 语言，提交前跑 `pnpm run quality`。**

卡住了别死磕：先看踩坑，没解决去看 git log，再不行问同事。

想看具体代码就跳：

- `src/router/index.ts` — 路由守卫
- `src/router/backEnd.ts` — 后端菜单 → 前端路由
- `src/utils/http/shared.ts` — HTTP 拦截器
- `src/stores/userInfo.ts` — 用户信息 / 按钮权限
- `src/composables/useEditDialog.ts` — 弹窗核心
- `src/utils/appWorkspace.ts` — 多租户应用作用域
- `scripts/check-frontend-standards.mjs` — 跨端规范门禁

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

详见 [LICENSE](./LICENSE) 文件。

<!--
EN: See [LICENSE](./LICENSE) for license information.
zh-CN: 详见 [LICENSE](./LICENSE) 文件。
zh-TW: 詳見 [LICENSE](./LICENSE) 檔案。
ja: 詳しくは [LICENSE](./LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](./LICENSE) 파일을 참조하세요.
-->