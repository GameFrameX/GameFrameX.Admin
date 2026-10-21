# AGENTS

GameFrameX Admin is handled as one integrated front-end/back-end project. This file is the single root instruction entry for Codex, Claude Code, and other agents that read `AGENTS.md`.

## Code Roots

- `GameFrameX.Admin.Unified.Api/` — .NET backend
- `GameFrameX.Admin.Unified.Vue/` — Vue frontend

## Required Reading

- Before reading or changing code, first read `gfx-doc/agent-handbook/INDEX.md`.
- For business state/rules, read `gfx-doc/state/INDEX.md`.
- For active specs/plans/reviews, read `gfx-doc/dev/index.md`.

## Core Commands

Build / run / 前端脚本见 `gfx-doc/agent-handbook/INDEX.md` 的 Build & Run;命令只在 handbook 一处维护,避免双份漂移。

## Non-Negotiables

- Treat the repo as one integrated project. Do not recreate separate Api/Vue document systems.
- Root `gfx-doc/` is the only code-side document system.
- Keep `AGENTS.md`, `CLAUDE.md`, and `gfx-doc` as root-level entry points only.
- Use SqlSugar patterns in backend work; do not introduce Entity Framework.
- Preserve tenant boundaries: `全局配置` is SuperAdmin-only; tenant features belong under tenant-visible business menus.
- Prefer existing local patterns and generated API clients before adding new abstractions.
- Any change affecting user-visible behavior MUST be verified by real browser end-to-end (see **Verification Standard** below). Compile-only or API-only (curl/fetch) verification is NOT acceptable as the sole evidence for such changes.

## Verification Standard

**Scope — what triggers real-browser verification:**
Any change a user can see or click must be verified in a real browser before reporting done:
- Frontend pages, components, routes, menus, permissions visible in the UI.
- Backend changes whose effect is observed through the UI: logs/audit/observability pages, menus, reports, exports, any API consumed by a page.
- Log/monitor/audit write paths (e.g. `DatabaseLoggingWriter`): the row must be confirmed rendered on its frontend page, not only present in the DB.

**What counts as real-browser verification:**
- Drive the actual Vue app (default dev server `http://127.0.0.1:20800`, hash router) with the e2e suite under `GameFrameX.Admin.Unified.Vue/e2e/` (Playwright, `channel: 'msedge'` launches system Edge — no manual CDP/browser step; run `pnpm run test:e2e` from the Vue root).
- Perform real interactions (form login with the right tenant, navigate by real menu), prove the page's real API fired (route recorder), then assert the DOM renders the expected data (account, tenant, the row/field under test).
- Evidence lands automatically under `GameFrameX.Admin.Unified.Vue/e2e/.e2e/`: per-test artifacts in `results/<test>/` (explicit `shot(page, 'label')` screenshots + trace.zip on failure), plus `report.json` and the HTML report in `report/` (`pnpm run test:e2e:report`). Use the `shot()` helper from `e2e/tests/helpers.ts` for explicit proof shots.

**What is NOT sufficient on its own:**
- `dotnet build` / `dotnet test` passing.
- Raw HTTP (`curl`/`fetch`) hitting the API and checking the JSON.
- Direct SQL `SELECT` on the DB.
These are allowed as supplementary checks, but a user-visible change still requires the browser-level assertion above. Backend-only/internal-path changes (no UI surface) may stop at compile + API/DB checks.

**Existing harness:**
- `GameFrameX.Admin.Unified.Vue/e2e/` — the e2e regression suite (Playwright since 2026-10-19, migrated from tester-army/e2e; real-click-only since 2026-10-20: no menu-tree API reads, no direct hash teleporting — menu leaves are enumerated from the rendered sidebar DOM and reached by real clicks, via `e2e/tests/helpers.ts` `navigateByMenu` / `gotoAppSelector` / `enterWorkspace`; the only sanctioned API use is passive request recording/mocking to prove the page fired its own calls); per-module coverage reorganized 2026-10-20 (C395) with a real-form **action layer**: dual-session auth via storageState setup projects (`admin`/`superadmin`), login failure path, landing/logout, log pages + search filter, tenant-boundary, appearance; **module coverage** under `e2e/tests/modules/` (`module-coverage.ts` harness: `installApiRecorder` with request-body snapshots / `renderedMenuGroups` / `sweepGroup`) — one test per sidebar root group on both highest-permission surfaces: SuperAdmin platform shell (`platform-shell.e2e.ts`: 平台管理/全局配置/开发工具/帮助文档/工作台), tenant admin global shell (`tenant-global-shell.e2e.ts`: 工作台/应用中心/系统管理/日志管理), app workspace 10 centers (`app-workspace-modules.e2e.ts`), plus root-group inventory guards (`menu-inventory.e2e.ts`) asserting the rendered group set equals the menu seed. Each module test (`actions: true`) sweeps every leaf via real clicks with 404 sampling plus the action layer: search keyword typed into the form → asserted present in the request payload → reset clears the input; search dropdowns (PrimeVue Select AND AutoComplete) really picked → new query asserted; row-level 详情 open/close, 编辑 open → echo assertion + first dropdown really picked (never saved), 删除 → confirm dialog must appear → cancel → row still present (a delete that fires without confirmation is a defect); add-dialog open/cancel; zero passive-API anomalies (per-leaf coverage lands in test annotations). Data-level roundtrips (create → edit-submit with echo assertion → updated row → delete → gone) live in `crud-roundtrips.e2e.ts` — dialog fields filled by control order (text/select/date/number/json/skip/cascade/editor), matrix header documents the audited exception list with issue links (42 CRUD pages total). `pnpm run test:e2e tests/<file>.e2e.ts` for a single file; a single module: append `-g "<组名>"`. New CRUD pages: add an entry to `crud-roundtrips.e2e.ts` ENTITIES; new menu groups/leaves: update the corresponding module file's expectations + `menu-inventory.e2e.ts`.
- `GameFrameX.Admin.Unified.Vue/src/**/*.test.ts` — unit / component suite (Vitest 3 + jsdom + @vue/test-utils, since 2026-10-20; covers pure-logic utilities like `cursorPager` / `commandForm` / `notifyUrl` / `appWorkspace` / `http/shared.resolveResponseMessage`, plus a component convention reference at `src/components/form-label/index.test.ts`). Run via `pnpm run test:unit` (CI) / `test:unit:watch` (dev watch) / `test:unit:coverage` (v8 coverage). New pure-logic helpers: write a colocated `*.test.ts` next to the source; new Vue components: mirror `form-label/index.test.ts` (`mount` + `injected provider` + slot text).
When a user-visible change is reported done without browser evidence, treat the report as incomplete and run the browser check before claiming success.

## Issue / Task Creation
When creating GitHub issues or Linear tasks for this project:

- **Never put a priority prefix in titles.** Express priority only via `P0`/`P1`/`P2`/`P3` labels (GitHub) plus the native Linear priority field (`P0` -> `urgent`, `P1` -> `high`, `P2` -> `medium`, `P3` -> `low`).
- Put scope at the start of the title, e.g. `后端：修复应用配置作用域` or `前端：修复应用配置页面`. For work that genuinely spans both sides as one unit, use `全栈` or `前后端`.
- If the target repository does not have the priority label yet, create it before creating the issue.
- If the user specifies a priority, use it. Otherwise default ordinary feature work to `P2`; data boundary, permission, security, cross-frontend/backend contract, or user-visible production-impact work defaults to `P1`.
- Automation triage keeps a fallback query for open tasks missing priority metadata, such as GitHub `is:issue is:open -label:P0 -label:P1 -label:P2 -label:P3`, fixing metadata before scheduling execution.

经确认后写入：待办 → `gfx-doc/backlog.md`；关键决策 → `gfx-doc/decisions/`；经验教训 → `gfx-doc/lessons/`。
