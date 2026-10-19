import type { RouteRecordRaw } from 'vue-router'

/**
 * 全局配置页 fallback 路由（后端菜单 seed 未落地时启用，落地后自动让位动态路由）。
 *
 * 后端 C171（GFX-173）提供了日志配置 API、C173（GFX-177）提供了密码策略配置 API，但两者范围外均
 * 未 seed 菜单。本 fallback 让 SuperAdmin 可通过 hash 直达 `/platform/global/platformLoggingConfig`
 * 与 `/platform/global/platformPasswordPolicyConfig` 访问页面进行配置。后端 menu seed
 * （`route:platformLoggingConfig` / `route:platformPasswordPolicyConfig`）落地后，动态路由会先注册
 * 同 path，本函数检测到后跳过 fallback 注册，无需人工删代码；allowlist 条目同理由 admin-api-worker
 * 在 seed 落地后清理。
 *
 * 见 C178/GFX-174、C174/GFX-180。
 */
const globalConfigFallbackRoutes: RouteRecordRaw[] = [
  {
    path: '/platform/global/platformLoggingConfig',
    name: 'platformLoggingConfigFallback',
    component: () => import('/@/views/platform/global/platformLoggingConfig/index.vue'),
    meta: {
      title: 'platformLoggingConfig.title',
      isHide: true,
      isKeepAlive: false,
    },
  },
  {
    path: '/platform/global/platformPasswordPolicyConfig',
    name: 'platformPasswordPolicyConfigFallback',
    component: () => import('/@/views/platform/global/platformPasswordPolicyConfig/index.vue'),
    meta: {
      title: 'platformPasswordPolicyConfig.title',
      isHide: true,
      isKeepAlive: false,
    },
  },
]

/**
 * 注册全局配置 fallback 路由。仅当后端动态路由尚未注册同 path 时才注册，避免与后端
 * seed 菜单的动态路由冲突。
 */
export function registerGlobalConfigRoutes(router: {
  getRoutes: () => Array<{ path: string; name?: unknown }>;
  hasRoute: (name: string) => boolean;
  addRoute: (parentName: string, route: RouteRecordRaw) => void;
}) {
  const existingPaths = new Set(router.getRoutes().map((r) => r.path));
  for (const route of globalConfigFallbackRoutes) {
    if (existingPaths.has(route.path)) continue;
    const name = route.name as string;
    if (!router.hasRoute(name)) {
      router.addRoute('/', route);
    }
  }
}
