-- 03-verify-permission-matrix.sql
-- GameFrameX Admin 租户库完全隔离 · P2 迁移后权限矩阵自检（GFX-727 / C318）
-- 对应 issue 验收第 1/2 条：平台侧任何账号无法操作租户库 + connection 无明文残留。
--
-- 执行方式（主库）：
--   本地：docker exec -i gameframex-postgres psql -U gameframex -d gameframex_admin \
--           -f db/tenant-isolation/03-verify-permission-matrix.sql
--   远程：PGHOST=... PGPASSWORD=... psql -d gameframex_admin -f db/tenant-isolation/03-verify-permission-matrix.sql
--
-- 判定：四项检查的「违规行数」全部为 0 即全绿；任一非 0 输出明细行。

\set QUIET on
\pset footer off

\echo '== 权限矩阵自检（GFX-727 / C318）=='
\echo '注册租户数（基准）：'
SELECT count(*) AS registered_tenants FROM system_tenant_entity WHERE is_deleted = false;

\echo ''
\echo '[1/4] 注册租户库的 CONNECT 授权残留（期望 0 行；口径见下）：'
\echo '  · ACL 层：dataacl 中不存在授予 gameframex 或 PUBLIC 的 CONNECT（REVOKE FROM PUBLIC 的直接证据）；'
\echo '  · 探针层：非 superuser 的 gfx_provisioner 对注册租户库无 CONNECT（真实反映 ACL 生效性）。'
\echo '  注：gameframex 本身为 superuser，PG 语义下 superuser 绕过一切 ACL（has_database_privilege 恒真）；'
\echo '  其完全阻断以 P3 主库账号降权后达成，P2 阶段以上述 ACL + 非 superuser 探针为机器可检口径。'
WITH registered AS (
    SELECT 'gameframex_admin_tenant_' || id::text AS db_name FROM system_tenant_entity WHERE is_deleted = false
)
SELECT d.datname AS acl_violation_db, a.grantee_role, a.privilege_type
FROM pg_database d
JOIN registered r ON r.db_name = d.datname
CROSS JOIN LATERAL (
    SELECT COALESCE(pg_get_userbyid(x.grantee), 'PUBLIC（grantee=0）') AS grantee_role, x.privilege_type
    FROM aclexplode(COALESCE(d.datacl, acldefault('d', d.datdba))) x
) a
WHERE a.grantee_role IN ('gameframex', 'PUBLIC（grantee=0）')
  AND a.privilege_type = 'CONNECT'
UNION ALL
SELECT d.datname, 'gfx_provisioner(probe)', 'CONNECT'
FROM pg_database d
JOIN registered r ON r.db_name = d.datname
WHERE has_database_privilege('gfx_provisioner', d.datname, 'CONNECT');

\echo '[2/4] gfx_provisioner 对租户 role 存在 membership（期望 0 行）：'
SELECT r.rolname AS tenant_role, m.rolname AS member
FROM pg_auth_members am
JOIN pg_roles r ON r.oid = am.roleid
JOIN pg_roles m ON m.oid = am.member
WHERE m.rolname = 'gfx_provisioner'
  AND r.rolname LIKE 'gameframex_admin_tenant_user_%';

\echo '[3/4] 注册租户库 owner 不是各自独立账号（期望 0 行）：'
WITH registered AS (
    SELECT 'gameframex_admin_tenant_' || id::text AS db_name,
           'gameframex_admin_tenant_user_' || id::text AS user_name
    FROM system_tenant_entity WHERE is_deleted = false
)
SELECT d.datname, pg_get_userbyid(d.datdba) AS current_owner
FROM pg_database d
JOIN registered r ON r.db_name = d.datname
WHERE pg_get_userbyid(d.datdba) <> r.user_name;

\echo '[4/4] system_tenant_entity.connection 存在非 enc:v1: 残留（期望 0 行）：'
SELECT id, left(COALESCE(connection, '<NULL>'), 20) AS connection_head
FROM system_tenant_entity
WHERE is_deleted = false
  AND (connection IS NULL OR connection NOT LIKE 'enc:v1:%');

\echo ''
\echo '[附] 孤儿库清单（gameframex_admin_tenant_% 形态但未注册；信息项，不在迁移范围）：'
SELECT d.datname, pg_get_userbyid(d.datdba) AS owner
FROM pg_database d
WHERE d.datname LIKE 'gameframex_admin_tenant_%'
  AND d.datname NOT IN (
      SELECT 'gameframex_admin_tenant_' || id::text FROM system_tenant_entity WHERE is_deleted = false
  )
ORDER BY d.datname;

\echo '== 自检结束：上述四项明细区均无输出行 = 全绿 =='
