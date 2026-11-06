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
- `全局配置` is SuperAdmin-only; admin-facing business features belong under admin-visible business menus. `ServerId` is the only operational scope.
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
- Perform real interactions (form login, navigate by real menu), prove the page's real API fired (route recorder), then assert the DOM renders the expected data (account, server selection, the row/field under test).
- Evidence lands automatically under `GameFrameX.Admin.Unified.Vue/e2e/.e2e/`: per-test artifacts in `results/<test>/` (explicit `shot(page, 'label')` screenshots + trace.zip on failure), plus `report.json` and the HTML report in `report/` (`pnpm run test:e2e:report`). Use the `shot()` helper from `e2e/tests/helpers.ts` for explicit proof shots.

**What is NOT sufficient on its own:**
- `dotnet build` / `dotnet test` passing.
- Raw HTTP (`curl`/`fetch`) hitting the API and checking the JSON.
- Direct SQL `SELECT` on the DB.
These are allowed as supplementary checks, but a user-visible change still requires the browser-level assertion above. Backend-only/internal-path changes (no UI surface) may stop at compile + API/DB checks.

**Existing harness:**
- `GameFrameX.Admin.Unified.Vue/e2e/` — Playwright regression suite using system Edge. The app has one shell: ordinary admin sees nine roots (总览、区服、内容运营、在线运维、玩家、商业化、C 端交付、审计、系统); superadmin sees only 总览 and 系统, including protected system groups. `auth.setup.e2e.ts` / `superadmin.setup.e2e.ts` create separate storage states through real form login; `admin-stack.ts` holds the isolated-stack guard and response-envelope readers; the admin account and its `sys_admin` non-superadmin menu grants now come from the first-install seed bootstrap (`GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT`/`_PASSWORD`), so no active role/grant API call is made by the suite. The merge reviewer account is created through the real 账号管理 UI and its password reset through the row action menu. Page navigation uses rendered sidebar DOM and real clicks through `helpers.ts` `navigateByMenu`; login lands on 运营首页 (`/dashboard/home`). `modules/single-shell.e2e.ts` has one case per root/session (11 total), with `actions: true` (C404: 区服组计数含开服/合服两个向导流程页，通用 CRUD 探针经 `skipLeaves` 跳过，写路径由 `game-area-provision.e2e.ts` / `game-area-merge.e2e.ts` 专属用例覆盖); `modules/menu-inventory.e2e.ts` asserts the exact nine/two root sets. `module-coverage.ts` records page requests and sweeps every leaf: search/reset payloads, real Select/AutoComplete choices, details, edit echo, delete confirmation/cancel, and add-dialog validation. Per-leaf coverage is written to annotations. `scheduler-retention.e2e.ts` covers 任务调度内置作业可见/手动执行运行态、超期日志留存消失与定时任务 admin 增删改（C404）。 `crud-roundtrips.e2e.ts` has 20 entity roundtrips (create → edit-submit/echo → delete) plus the F2 ad-callback package-binding closure, with exceptions documented in its header and fields filled in control order. `server-selector.e2e.ts` covers global/local server selection, request payload and DOM, reload persistence, and delayed-response isolation. Other files cover login failure, landing/logout, appearance, log pages/search/export, and both directions of the admin/superadmin visibility boundary. `client-api-protocol.e2e.ts` proves the C406 single-game Client.Api protocol (X-Proto-Ver 200 / missing 400 / legacy headers ignored); `e2e-integration-checks.e2e.ts` carries the C406 acceptance assertions (login form fields, renamed pages + renamed APIs, wire hygiene across pages, log columns without TenantId, single-packageId callback URL shape, Hub machine-channel HMAC with legacy headers ignored, bootstrapped-admin first login with nine roots, ad-callback package selection/echo with the single-{packageId} URL form, and cross-package key rejection with missing-packageId 404). Run `pnpm run test:e2e tests/<file>.e2e.ts`, optionally `-g "<组名>"`; update the CRUD matrix for new CRUD pages and the module/inventory expectations for new menus. Independent static checks are `pnpm exec tsc --noEmit -p e2e/tsconfig.json` and `pnpm exec eslint -c e2e/eslint.config.mjs e2e/tests`.
- `GameFrameX.Admin.Unified.Vue/src/**/*.test.ts` — unit / component suite (Vitest 3 + jsdom + @vue/test-utils, since 2026-10-20; covers pure-logic utilities like `cursorPager` / `commandForm` / `notifyUrl` / `pageServer` / `http/shared.resolveResponseMessage`, plus a component convention reference at `src/components/form-label/index.test.ts`). Run via `pnpm run test:unit` (CI) / `test:unit:watch` (dev watch) / `test:unit:coverage` (v8 coverage). New pure-logic helpers: write a colocated `*.test.ts` next to the source; new Vue components: mirror `form-label/index.test.ts` (`mount` + `injected provider` + slot text).
When a user-visible change is reported done without browser evidence, treat the report as incomplete and run the browser check before claiming success.

## Issue / Task Creation
When creating GitHub issues or Linear tasks for this project:

- **Never put a priority prefix in titles.** Express priority only via `P0`/`P1`/`P2`/`P3` labels (GitHub) plus the native Linear priority field (`P0` -> `urgent`, `P1` -> `high`, `P2` -> `medium`, `P3` -> `low`).
- Put scope at the start of the title, e.g. `后端：修复应用配置作用域` or `前端：修复应用配置页面`. For work that genuinely spans both sides as one unit, use `全栈` or `前后端`.
- If the target repository does not have the priority label yet, create it before creating the issue.
- If the user specifies a priority, use it. Otherwise default ordinary feature work to `P2`; data boundary, permission, security, cross-frontend/backend contract, or user-visible production-impact work defaults to `P1`.
- Automation triage keeps a fallback query for open tasks missing priority metadata, such as GitHub `is:issue is:open -label:P0 -label:P1 -label:P2 -label:P3`, fixing metadata before scheduling execution.

经确认后写入：待办 → `gfx-doc/backlog.md`；关键决策 → `gfx-doc/decisions/`；经验教训 → `gfx-doc/lessons/`。
