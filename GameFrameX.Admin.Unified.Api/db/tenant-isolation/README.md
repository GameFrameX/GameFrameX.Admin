# tenant-isolation —— 租户库完全隔离 P2 存量迁移（GFX-727 / C318）

三阶段方案（gfx-doc core-sqlsugar-module change 007）中的 P2 运维阶段。代码侧管线（`enc:v1:` 加密、provisioner 独立账号、建库自断）由 C317 / GFX-726 提供（已合并 main，PR #205）。

## 文件

| 文件 | 作用 |
|---|---|
| `01-provision-accounts.sql` | 建 `gfx_provisioner`（NOSUPERUSER+CREATEROLE+CREATEDB）；主库 `REVOKE CONNECT FROM PUBLIC` + 白名单 GRANT；P3 降权语句与 pg_hba 说明（注释段） |
| `02-migrate-existing-tenants.sh` | 以 `system_tenant_entity` 注册行驱动逐租户迁移：role 创建/密码重置 → 库内 `REASSIGN OWNED BY` → `ALTER DATABASE OWNER` → `REVOKE CONNECT FROM PUBLIC` → `enc:v1:` 加密回写 connection；支持 `--dry-run` |
| `03-verify-permission-matrix.sql` | 迁移后权限矩阵自检（四项全绿判定 + 孤儿库清单） |
| `encrypt-connection/` | dotnet 运维小工具，与 `TenantConnectionCrypto`（C317）同底层 `AesHelper`，输出 `enc:v1:` 密文；不注册进 slnx |

## 执行顺序（硬约束）

> 违反顺序的典型失败：旧镜像读不懂 `enc:v1:` 直连失败；旧推导连接（主库 gameframex 账号）被 REVOKE PUBLIC 阻断。

1. **前置**：三 host 已部署含 GFX-726（C317）解密代码的镜像，并配置共享密钥 `GFX_TENANT_CONN_ENC_KEY`（三 host 必须同值，否则跨 host 解密失败）。
2. `01-provision-accounts.sql`（superuser）——保存 provisioner 密码到部署密钥管理，配置 `DbConnection:ProvisioningConnectionString`。
3. 修改部署配置：`GFX_DEFAULT_TENANT_DB_TYPE=PostgreSQL` + `GFX_DEFAULT_TENANT_DB_CONNECTION_STRING=<02 输出的默认租户新串>`（先拿到串可先跑 02 的 `--dry-run` 拿不到密码——默认租户新串在 02 执行时输出，执行后立即配置）。
4. `02-migrate-existing-tenants.sh`（superuser + `GFX_TENANT_CONN_ENC_KEY`；先 `--dry-run` 过目）。
5. 三 host 滚动重启（释放连接缓存；本地 `docker compose up -d`，远程按部署方式）。
6. `03-verify-permission-matrix.sql` 自检四项全绿 + 租户业务读写回归（默认租户与业务租户各一条登录读写路径）。

## 本地 / 远程连接参数

| 环境 | 连接方式 |
|---|---|
| 本地 gameframex-postgres（PG 16，容器） | `docker exec -i gameframex-postgres psql -U gameframex -d ...`（01/03）或 `PGHOST=127.0.0.1 PGPORT=5432 ...`（02） |
| 远程 ***REMOVED-HOST-IP***（PG 18.6） | `PGHOST=***REMOVED-HOST-IP*** PGPORT=5432 PGUSER=gameframex PGPASSWORD=<superuser 密码>` —— **superuser 凭证由运维持有，本目录与仓库不保存** |

## 密钥与敏感信息

- `GFX_TENANT_CONN_ENC_KEY`：三 host 与迁移脚本共享；丢失后 `enc:v1:` 密文不可恢复（需重置密码重迁移）。
- 租户新密码为随机生成（`openssl rand -hex 24`），只出现在密文与 02 执行输出（默认租户明文串一次性输出），不落仓库。
- 孤儿库（如 `_c290` 残留、`gfx709_verify`）不在注册表，不迁移不自检，处置另行决定。

## 幂等性

02 任意步骤中断后重跑安全：role/密码重置幂等、`REASSIGN OWNED` 无对象时无操作、`UPDATE` 幂等。01 的 REVOKE/GRANT 天然幂等。

## P3：主库账号降权与 superuser 网络封锁（GFX-728 / C319）

| 文件 | 作用 |
|---|---|
| `04-demote-main-account.sh` | P3 收尾：postgres 兜底 superuser（缺失才建）→ 主库 OWNER 确认 → `ALTER ROLE gameframex NOCREATEROLE NOCREATEDB`（+`NOSUPERUSER`，见下）→ pg_hba 顶部注入 `host all postgres 0.0.0.0/0 reject`（+`::/0`，备份/校验/回滚一体）→ 自检矩阵；支持 `--dry-run` / `--verify-only` |

- **执行时机**：01–03 全绿、三 host 租户业务读写回归通过之后（P3 依赖 P2 完成）。
- **PG 硬限制**：bootstrap superuser（初始化角色 `oid=10`，容器 `POSTGRES_USER=gameframex` 形态即如此）不允许 `NOSUPERUSER`——脚本自动探测（`pg_roles.oid=10`），此时仅去除 CREATEROLE/CREATEDB 并输出 WARN；完整降权需部署形态改造（bootstrap 用独立运维账号）。执行前可 `SELECT rolname, oid FROM pg_roles WHERE oid = 10;` 确认。
- **pg_hba 写回后自动 `chown postgres:root + chmod 600`**（写回命令可能以 root 落盘，postgres 进程读不了会导致 reload 失败、容器重启起不来）。
- 远程执行 runbook 见脚本头注释（凭证就绪后 `PSQL_CMD="psql"` 直跑 SQL 段；pg_hba 段需目标机文件权限）。
