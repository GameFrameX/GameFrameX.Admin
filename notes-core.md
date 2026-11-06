# C406 修复轮 notes-core（F3 首个管理员种子引导）

> 本文件由修复轮 F3（C 执行）新建：仓库中此前不存在 notes-core.md，本节为首个条目。

## F3 首个管理员种子引导

### 基线

- 第 1 步同步集成分支：`git merge feature/GFX-1232`（30cbf141），无冲突。
- 新基线测试：**1723 passed / 0 failed / 6 skipped**（GameFrameX.Core.Tests，SQLite + PG 可达性跳过口径不变）。

### 设计

- 背景：集成轮发现新装库只种了超管，产品内没有任何路径能创建第一个业务管理员（超管菜单里没有账号/角色管理，种子里也没有 `sys_admin` 角色与授权）。B佬 2026-11-06 裁定「种子引导首个管理员」。
- `InitialAdminBootstrap.RunAsync`（GameFrameX.Core/Bootstrap/InitialAdminBootstrap.cs）三步：
  1. `CommonConst.SysAdminRole`（`sys_admin`）角色不存在则创建（雪花 Id 显式取号）；
  2. 授予全部非超管种子菜单：口径与 `RoleMenuService.ExcludeSuperAdminOnlyMenusAsync` 完全一致——`MenuSeedCatalog.AllDefinitions` 全量 Id，剔除 `SuperAdminOnlySubtreeMenuIds` 整棵超管专属子树，但 `AdminReadableSuperAdminOnlyMenuIds`（`AllowAdminWhenSuperAdminOnly` 可读选择器）照常授予；
  3. `GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT` + `GAMEFRAMEX_ADMIN_INITIAL_PASSWORD` 成对设置且 `UserEntity` 表（含软删行）为空时，创建该账号：Argon2id（Foundation `PasswordHashHelper`，经 `CryptogramUtility.HashPassword`，与现有用户服务一致）、`AccountType=SystemAdmin`、`IsEnabled=true`，绑定 `sys_admin`。
- 执行时机：仅 Web.Api。`AddSqlSugar`（旧库守卫 → 建表 → 种子 → `MenuSeedCatalog.LoadFromDb`）在装配期同步完成；`InitialAdminStartupBootstrap`（IHostedService）注册在 `PlatformPasswordPolicyStartupLoader` 之后，保证密码强度静态属性就位后再做 fail-closed 校验。Client/Hub 不装配。
- **只补不删的理由**：角色-菜单授权是运维数据而非权威种子（global_menu 行才是权威种子，upsert 纠正）。若启动期做「删多余授权」的收敛，运营在角色管理里对 `sys_admin` 手工收窄菜单（最小权限）后一重启就会被种子冲回全量，等于剥夺了收窄能力；且 `RoleMenuService.GrantRoleMenuAsync` 的正常路径本来就是「保存运营选择」。因此补授语义是：库里缺的种子菜单补上（含新版本新增的种子菜单），已有的授权（无论来源）一律保留。副作用：若运营主动删掉某条种子菜单的授权，该菜单不会因重启恢复——这符合「授权归运营管」的边界。
- 校验与日志：
  - 密码不满足 `CryptogramUtility.StrongPassword` 策略时抛 `InvalidOperationException` 终止启动（fail-closed），错误信息只含变量名与策略提示，不含密码；
  - 只设一个变量 → 警告「必须成对设置」；
  - 都没设且库无业务管理员 → 警告「尚未初始化业务管理员：设置这两个变量并重启」（变量名写入警告便于照抄）；
  - 账号已存在却设了变量 → 警告「被忽略：不重建账号、不覆盖密码」；
  - 所有输出走 Console（与 `LegacySchemaStartupGuard` / `SqlSugarSetup` 启动期先例一致），任何日志不含口令。
- 命名差异登记（参照 D2.3 先例）：新变量用全拼前缀 `GAMEFRAMEX_ADMIN_INITIAL_*`，与既有 `GFX_SUPERADMIN_INITIAL_PASSWORD` 的缩写前缀不一致；按「新增命名用全拼」规则执行，存量 `GFX_*` 变量保留不改。

### 改动的文件

| 文件 | 改动 |
|---|---|
| `GameFrameX.Core/Bootstrap/InitialAdminBootstrap.cs` | 新增：引导核心逻辑（静态类，可被测试与 Web.Api 共用） |
| `GameFrameX.Web.Api/Handlers/InitialAdminStartupBootstrap.cs` | 新增：IHostedService 装配 |
| `GameFrameX.Web.Api/Startup.cs` | 注册 HostedService（密码策略加载之后） |
| `GameFrameX.Core.Tests/Bootstrap/InitialAdminBootstrapTests.cs` | 新增 5 个用例 |
| `docker-compose.yml` / `docker-compose.admin.yml` | web-api/admin-web environment 透传两个变量（`${VAR:-}`，无默认口令） |
| `.env.example` | 新增成对变量注释段（不写默认口令） |
| `GameFrameX.Admin.Unified.Api/README.zh-CN.md` | 部署章新增「首次部署：超管密码与首个业务管理员」小节 |
| `GameFrameX.Admin.Unified.Vue/README.zh-CN.md` | 快速开始说明首装账号来源（两变量各自用途） |

其他语言 README（en/ja/ko/zh-TW）本轮未同步，留给集成轮统一处理。

### 环境变量

- `GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT`：首个业务管理员账号名。
- `GAMEFRAMEX_ADMIN_INITIAL_PASSWORD`：首个业务管理员初始密码（须满足平台密码强度策略）。

### 测试清单（InitialAdminBootstrapTests，5 例）

1. `FirstInstallWithEnv_CreatesSingleAdminWithNonSuperAdminMenus`：新库 + 双变量 → 1 个账号、Argon2id 可验、`AccountType=SystemAdmin`、绑定 `sys_admin`；角色菜单 = 全部非超管种子菜单（超管专属子树命中数为 0，可读选择器除外）。
2. `SecondRun_IdempotentAndOnlyAddsMissingSeedMenus`：二次启动账号仍 1 个、密码不覆盖；删一条种子授权后重启补回；手工额外授权（非种子菜单 987654321）保留不删。
3. `WithoutEnv_CreatesNoAccountAndWarns`：不设变量 → 不建账号、输出「尚未初始化业务管理员」警告（含变量名）。
4. `PartialEnv_WarnsAndCreatesNoAccount`：单设账号变量 → 警告「必须成对设置」，不建账号。
5. `WeakPassword_FailsClosedWithoutLeakingPassword`：开启强度策略 + 弱密码 → 抛 `InvalidOperationException`（含变量名、不含密码明文），账号未创建。

登录链路说明：密码可验性 + 授权集合在单测层证明；HTTP 登录拿菜单由 e2e（管理员会话）覆盖——种子账号与 e2e 凭证对齐后现有 `auth.setup.e2e.ts` 即走真实登录。超管首装向导行为不变：`SuperAdminFirstInstallPasswordTests` 全绿。

### 闸门结果

- 契约 §5 闸门：全量 `dotnet test` **1728 passed / 0 failed / 6 skipped**（= 基线 1723 + 新增 5；D13 阻断测试随套件保持绿）。
- 每项改动原子 commit：`d7e5897a`（代码+测试）、`ab744a55`（compose/env/README）、本节随 `docs(notes)` 提交，最终 HEAD 以 worker_done 摘要为准。

### 给集成轮的接线说明

- 隔离栈（端口段 21120–21129）在 compose/.env 中设置：
  - `GAMEFRAMEX_ADMIN_INITIAL_ACCOUNT` = e2e 的 `E2E_ACCOUNT`
  - `GAMEFRAMEX_ADMIN_INITIAL_PASSWORD` = e2e 的 `E2E_PASSWORD`
- 设置后新装库启动即引导出首个业务管理员，e2e 不再需要直接写库建 admin（`admin-menu-fixture` 之外的写库兜底可移除，具体以 e2e 套件现状为准）。
- 注意：账号只在「库里没有任何业务管理员（含软删）」时创建；隔离栈每次重建数据卷才等价于新装，复用卷时变量会被忽略并告警，属预期。
