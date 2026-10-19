/**
 * 支付配置页「复制回调地址」URL 拼装纯函数。
 *
 * 契约来源：C298 gfx704-api-url-case-alignment（GFX-704）+ spec `## Payment Config Copy Notify URL`。
 * 必须与后端 `GameOrderController` 公开 provider 回调路由一致：
 *   `/api/gameOrder/{action}/{tenantId}/{appId}/{packageId?}`
 * 其中 `{action}` 为 lowerCamelCase（`notifyApple` 等）。后端为 `BaseController` token 路由
 * `api/[controller]/[action]`，ASP.NET Core literal 路由段匹配大小写不敏感（C298 已实测：
 * 小写 / PascalCase 回调 URL 均命中路由），故前端统一输出小写 camel 规范形态，
 * 不要求与后端 action 方法名逐字符一致。公开回调只从 URL 路由段解析租户上下文，
 * 不得退化回旧 `/client/gameOrder/...` 或 `?tenantId=&appId=&packageId=` query 形式。
 *
 * 本文件不依赖任何 `/@/` 别名或 Vue 运行时，可被 node:test 直接加载（Node 24 原生 type stripping）。
 */

/** 已知支付渠道 → 后端 GameOrderController 公开回调 action 的 lowerCamelCase URL 段。 */
export const NOTIFY_ACTION_MAP: Record<string, string> = {
  apple: 'notifyApple',
  google: 'notifyGoogle',
  xsolla: 'notifyXsolla',
  alipay: 'notifyAliPay',
};

/**
 * 把渠道标识首字母大写（lower-case remainder 保留原样），用于未知渠道 fallback。
 * 空字符串返回空串，调用方负责兜底。
 */
export const capitalizeChannel = (channel: string): string => {
  if (!channel) return '';
  return channel.charAt(0).toUpperCase() + channel.slice(1);
};

/**
 * 解析渠道对应的 Notify action URL 段。已知渠道走映射；未知渠道 fallback `notify${capitalize(channel)}`，
 * 不破坏已知渠道映射。
 */
export const buildNotifyAction = (channel: string): string => {
  const known = NOTIFY_ACTION_MAP[channel];
  if (known) return known;
  return `notify${capitalizeChannel(channel)}`;
};

/** 复制回调地址行数据的最小契约（与列表行字段对齐，宽松可选）。 */
export interface NotifyUrlRow {
  payChannel?: string;
  tenantId?: number | string;
  gameAppId?: number | string;
  appId?: number | string;
  packageId?: number | string;
}

/**
 * 拼接公开回调地址 URL。
 * - `tenantId` / `appId` 缺失时回退占位 `{tenantId}` / `{appId}`（与 C65 既有行为一致，便于人工识别未补全的行）。
 * - `packageId` 存在（truthy）时作为路径段追加；缺失/为 0 时不出现（与原 index.vue `if (row.packageId)` 一致）。
 * - 输出恒为路由段格式，永不包含 query 串或 `/client/gameOrder/` 前缀。
 */
export const buildNotifyUrl = (row: NotifyUrlRow): string => {
  const tenantId = row.tenantId ?? '{tenantId}';
  const appId = row.gameAppId ?? row.appId ?? '{appId}';
  const action = buildNotifyAction(String(row.payChannel ?? ''));
  let url = `/api/gameOrder/${action}/${tenantId}/${appId}`;
  if (row.packageId) {
    url += `/${row.packageId}`;
  }
  return url;
};
