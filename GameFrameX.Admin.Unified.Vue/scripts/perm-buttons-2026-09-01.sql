-- 2026-09-01 补显式权限按钮（对账脚本 check-permission-keys.mjs 首跑发现的存量欠账）
-- 按钮 Id = StableSeedId.Menu(Key) = SHA256("menu:"+key) 前 8 字节小端 & long.MaxValue（与种子体系同构）
-- 端点绑定为追加（保留 permission:auto:* 兜底绑定，收紧语义不变）
-- 执行后需重启 Admin 后端（全局按钮/菜单缓存 KeyUserButton+0、端点权限映射不随直插失效）
-- 适用库：主库（gameframex_admin）

INSERT INTO global_menu_entity
  (id, pid, type, key, permission, title, is_iframe, is_hide, is_keep_alive, is_affix,
   order_no, super_admin_only, sync_roles, allow_tenant_when_super_admin_only, app_scope,
   tenant_id, created_time, is_deleted, is_enabled, row_version)
VALUES
  (6237617357109286359, 4272713015995144348, 3, 'systemDatabase:updatetable',  'systemDatabase:updatetable',  '编辑表',   false, false, true, false, 100, true, 0, false, 0, 0, extract(epoch from now())::bigint, false, true, 0),
  (7465144620076053409, 4272713015995144348, 3, 'systemDatabase:deletetable', 'systemDatabase:deletetable', '删除表',   false, false, true, false, 100, true, 0, false, 0, 0, extract(epoch from now())::bigint, false, true, 0),
  (8279821677540379012, 5034912420112633389, 3, 'systemConfig:batchDelete',   'systemConfig:batchDelete',   '批量删除', false, false, true, false, 100, true, 0, false, 0, 0, extract(epoch from now())::bigint, false, true, 0);

INSERT INTO global_menu_endpoint_entity (id, menu_id, endpoint_id, is_auto, tenant_id, created_time, is_deleted, is_enabled, row_version)
VALUES
  (220231790542065666, 6237617357109286359, 216998930158845952, false, 0, extract(epoch from now())::bigint, false, true, 0),
  (220231790542065667, 7465144620076053409, 216998930154651648, false, 0, extract(epoch from now())::bigint, false, true, 0),
  (220231790542065668, 8279821677540379012, 216998930452447232, false, 0, extract(epoch from now())::bigint, false, true, 0);
