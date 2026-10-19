-- 2026-09-05 tenantAdCallbackConfigEntity → tenantAdvertisementCallbackConfigEntity 全名化迁移
-- 背景：C302/C309 简写归零政策；后端控制器/端点已是长名（短名端点 is_orphan=t），DB 按钮 Key 仍为短名，
--       前端 authPrefix 长名导致按钮全隐藏。本迁移把 DB 侧 Key/Id/path/component 统一为长名。
-- Id 均按种子约定重算：StableSeedId.Menu(Key)、StableSeedId.RoleMenu(roleCode, menuKey) = SHA256(scope+":"+key) 前 8 字节小端取正。
-- 执行后需重启 Admin 后端（KeyUserMenu/KeyUserButton/端点权限映射缓存不随直插失效）。
-- 适用库：主库（gameframex_admin）+ 全部租户库（gameframex_admin_tenant_*）。

-- ============ 主库部分 ============
BEGIN;

-- 1. 端点绑定引用迁移（menu_id 旧→新）
UPDATE global_menu_endpoint_entity SET menu_id=6860242852540439506 WHERE menu_id=5110847056985098658;
UPDATE global_menu_endpoint_entity SET menu_id=6940948928971539290 WHERE menu_id=3352876220291316396;
UPDATE global_menu_endpoint_entity SET menu_id=5792012472829487979 WHERE menu_id=7073279304846390967;
UPDATE global_menu_endpoint_entity SET menu_id=716803222343382802  WHERE menu_id=5547964472362383467;
UPDATE global_menu_endpoint_entity SET menu_id=8324335229950723480 WHERE menu_id=1612748001776889309;
UPDATE global_menu_endpoint_entity SET menu_id=2572327799805046998 WHERE menu_id=4456159242109866994;
UPDATE global_menu_endpoint_entity SET menu_id=8968603540301201000 WHERE menu_id=580973458478905738;
UPDATE global_menu_endpoint_entity SET menu_id=4250504171958338993 WHERE menu_id=3089105995661482754;

-- 2. 删除短名死端点（is_orphan=t）的绑定
DELETE FROM global_menu_endpoint_entity
WHERE endpoint_id IN (SELECT id FROM global_api_endpoint_entity WHERE route_path LIKE 'api/tenantadcallbackconfigentity/%');

-- 3. 页面菜单行（Id/Key/path/component 长名化）
UPDATE global_menu_entity SET id=6860242852540439506,
  key='route:tenantAdvertisementCallbackConfigEntity',
  path='/appCenter/tenantAdvertisementCallbackConfigEntity',
  component='/appCenter/tenantAdvertisementCallbackConfigEntity/index'
WHERE id=5110847056985098658;

-- 4. 七个按钮行（Id/Key/permission 长名化，pid 指向新页面 Id）
UPDATE global_menu_entity SET id=6940948928971539290, key='tenantAdvertisementCallbackConfigEntity:add',      permission='tenantAdvertisementCallbackConfigEntity:add',      pid=6860242852540439506 WHERE id=3352876220291316396;
UPDATE global_menu_entity SET id=5792012472829487979, key='tenantAdvertisementCallbackConfigEntity:delete',   permission='tenantAdvertisementCallbackConfigEntity:delete',   pid=6860242852540439506 WHERE id=7073279304846390967;
UPDATE global_menu_entity SET id=716803222343382802,  key='tenantAdvertisementCallbackConfigEntity:detail',   permission='tenantAdvertisementCallbackConfigEntity:detail',   pid=6860242852540439506 WHERE id=5547964472362383467;
UPDATE global_menu_entity SET id=8324335229950723480, key='tenantAdvertisementCallbackConfigEntity:page',     permission='tenantAdvertisementCallbackConfigEntity:page',     pid=6860242852540439506 WHERE id=1612748001776889309;
UPDATE global_menu_entity SET id=2572327799805046998, key='tenantAdvertisementCallbackConfigEntity:providers',permission='tenantAdvertisementCallbackConfigEntity:providers',pid=6860242852540439506 WHERE id=4456159242109866994;
UPDATE global_menu_entity SET id=8968603540301201000, key='tenantAdvertisementCallbackConfigEntity:schema',   permission='tenantAdvertisementCallbackConfigEntity:schema',   pid=6860242852540439506 WHERE id=580973458478905738;
UPDATE global_menu_entity SET id=4250504171958338993, key='tenantAdvertisementCallbackConfigEntity:update',   permission='tenantAdvertisementCallbackConfigEntity:update',   pid=6860242852540439506 WHERE id=3089105995661482754;

COMMIT;

-- ============ 租户库部分（每个 gameframex_admin_tenant_* 库各执行一次）============
-- 授权行 menu_id 迁移 + 主键 id 按新 menuKey 与角色 code 重算（保持种子同构，防幂等收敛漂移）
WITH map(old_id, new_id, new_key) AS (VALUES
  (5110847056985098658::bigint, 6860242852540439506::bigint, 'route:tenantAdvertisementCallbackConfigEntity'),
  (3352876220291316396::bigint, 6940948928971539290::bigint, 'tenantAdvertisementCallbackConfigEntity:add'),
  (7073279304846390967::bigint, 5792012472829487979::bigint, 'tenantAdvertisementCallbackConfigEntity:delete'),
  (5547964472362383467::bigint, 716803222343382802::bigint,  'tenantAdvertisementCallbackConfigEntity:detail'),
  (1612748001776889309::bigint, 8324335229950723480::bigint, 'tenantAdvertisementCallbackConfigEntity:page'),
  (4456159242109866994::bigint, 2572327799805046998::bigint, 'tenantAdvertisementCallbackConfigEntity:providers'),
  (580973458478905738::bigint,  8968603540301201000::bigint, 'tenantAdvertisementCallbackConfigEntity:schema'),
  (3089105995661482754::bigint, 4250504171958338993::bigint, 'tenantAdvertisementCallbackConfigEntity:update')
),
calc AS (
  SELECT t.id AS old_pk, map.new_id,
    ('x' || (SELECT substr(hx.x,15,2)||substr(hx.x,13,2)||substr(hx.x,11,2)||substr(hx.x,9,2)
             ||substr(hx.x,7,2)||substr(hx.x,5,2)||substr(hx.x,3,2)||substr(hx.x,1,2)
     FROM (SELECT substr(encode(sha256(convert_to('role-menu:' || r.code || '|' || map.new_key, 'utf8')),'hex'),1,16) AS x) hx
    ))::bit(64)::bigint & 9223372036854775807 AS new_pk
  FROM tenant_role_menu_entity t
  JOIN map ON map.old_id = t.menu_id
  JOIN tenant_role_entity r ON r.id = t.role_id
)
UPDATE tenant_role_menu_entity SET id = calc.new_pk, menu_id = calc.new_id
FROM calc WHERE tenant_role_menu_entity.id = calc.old_pk;

-- ============ 配套代码改动（与 SQL 同批生效，新环境由种子自动建长名，无需本 SQL）============
-- 后端仓：
--   GameFrameX.Application/SeedData/System/GlobalMenuSeedData.AppCenter.cs   种子 Key/Path/Component/TitleCode 长名化（Id 自动随 StableSeedId.Menu 派生）
--   GameFrameX.Core.Tests/System/TenantAdminMenuDefaultsTests.cs             守护期望清单长名化
--   GameFrameX.Core.Tests/System/GlobalMenuSeedDataStageTwoPathMigrationTests.cs 同上
-- 前端仓：
--   views/appCenter/tenantAdCallbackConfigEntity/ → tenantAdvertisementCallbackConfigEntity/（git mv）
--   index.vue authPrefix、i18n/pages/menu/*.ts（5 语言）menu key 同步长名化
-- 验证：Core.Tests 1323 passed / check-permission-keys / check-i18n-keys 全绿
