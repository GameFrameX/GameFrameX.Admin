-- 01-provision-accounts.sql
-- GameFrameX Admin 租户库完全隔离 · P2 存量迁移第一步：平台账号准备（GFX-727 / C318）
--
-- 作用：
--   1. 创建建库专用账号 gfx_provisioner（NOSUPERUSER + CREATEROLE + CREATEDB，见 C317 spec：
--      普通租户建库的管理连接 DbConnection:ProvisioningConnectionString 指向该账号，缺省回落主库串为降级模式）。
--   2. 主库收回 PUBLIC 的 CONNECT 并建白名单（平台 gameframex 账号 + gfx_provisioner）。
--   3. （供 P3 使用，本脚本不执行）主库平台 superuser 账号降权语句，注释段给出。
--   4. pg_hba.conf 变更说明，注释段给出。
--
-- 执行方式（psql 客户端，用平台 superuser 连接任意库，建议 postgres 维护库）：
--   本地：docker exec -i gameframex-postgres psql -U gameframex -d postgres \
--           -v provisioner_password="$(openssl rand -hex 24)" -v main_database="gameframex_admin" \
--           < db/tenant-isolation/01-provision-accounts.sql
--   远程：PGHOST=***REMOVED-HOST-IP*** PGUSER=gameframex PGPASSWORD='<superuser 密码>' psql -d postgres \
--           -v provisioner_password="$(openssl rand -hex 24)" -v main_database="gameframex_admin" \
--           -f db/tenant-isolation/01-provision-accounts.sql
--
-- 幂等性：role 已存在时不重复创建、不覆盖密码（输出提示由人工决定是否 ALTER）；
--         主库 REVOKE/GRANT 天然幂等，可重复执行。
-- 敏感提示：provisioner 密码只出现在 psql 变量与进程列表，请自行保存到部署密钥管理
--           （compose/部署侧配置 DbConnection:ProvisioningConnectionString）。

-- ============================================================
-- 1. gfx_provisioner 建号（存在则跳过）
-- ============================================================
SELECT 'CREATE ROLE gfx_provisioner WITH LOGIN NOSUPERUSER CREATEROLE CREATEDB PASSWORD ' || quote_literal(:'provisioner_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'gfx_provisioner')\gexec

-- 已存在时的密码重置语句（默认注释，需要时手工去掉注释执行）：
-- ALTER ROLE gfx_provisioner WITH PASSWORD :'provisioner_password';

-- ============================================================
-- 2. 主库 CONNECT 白名单：收回 PUBLIC，仅平台账号与 provisioner 可连
--    （三 host 主库连接均使用平台 gameframex 账号；owner 本身天然保有 CONNECT，GRANT 为显式白名单）
-- ============================================================
REVOKE CONNECT ON DATABASE :main_database FROM PUBLIC;
GRANT CONNECT ON DATABASE :main_database TO gameframex;
GRANT CONNECT ON DATABASE :main_database TO gfx_provisioner;

-- 执行前请确认没有其它业务 role 需要访问主库；如有，请一并 GRANT，避免 REVOKE PUBLIC 后被断连。

-- ============================================================
-- 3.（供 P3 使用）主库平台账号降权语句 —— 本阶段不执行
-- ------------------------------------------------------------
-- P3 目标：平台侧运行时账号不再以 superuser 运行。语句模板：
--   CREATE ROLE gameframex_platform_runtime WITH LOGIN NOSUPERUSER; -- 占位示意，实际由 P3 任务定名
--   REVOKE gameframex FROM gameframex;           -- 若存在 membership
--   -- 并将三 host GFX_MAIN_DB_CONNECTION_STRING 切换到降权账号后再执行下列对象权限收敛：
--   -- REVOKE ALL ON ALL TABLES IN SCHEMA public FROM gameframex; 等
-- 在 P3 完成前，平台 gameframex 保持 superuser（REASSIGN OWNED 等迁移操作依赖它）。
-- ============================================================

-- ============================================================
-- 4. pg_hba.conf 变更说明 —— 本脚本不改文件，仅说明
-- ------------------------------------------------------------
-- Docker 官方 postgres 镜像默认 host all all all scram-sha-256（或 trust），
--   gfx_provisioner 与租户 role 的密码登录无需额外条目。
-- 自管 PG 实例若 pg_hba.conf 采用「按库/按用户白名单」策略，需要补：
--   host gameframex_admin_tenant_<id> gameframex_admin_tenant_user_<id> <应用网段> scram-sha-256
--   host postgres                    gfx_provisioner                    <应用网段> scram-sha-256
-- 修改后执行 SELECT pg_reload_conf(); 生效（无需重启实例）。
-- ============================================================

-- 完成自检：gfx_provisioner 属性应显示 Super=f, CreateRole=t, CreateDB=t
SELECT rolname, rolsuper, rolcreaterole, rolcreatedb FROM pg_roles WHERE rolname IN ('gameframex', 'gfx_provisioner') ORDER BY rolname;
