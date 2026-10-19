export const APP_CREATE_GUIDE_PATH = '/appCenter/appEntry'
export const APP_SELECTION_PATH = '/appCenter/appSelector'
export const APP_WORKSPACE_PATH = '/appCenter/appWorkspace'
export const APP_MANAGE_PATH = '/tenant/appCenter/gameAppEntity'

export interface AppWorkspaceOption {
  id?: number | string | null
  name?: string | null
  appName?: string | null
  isEnabled?: boolean | null
}

export interface AppEntryTarget {
  path: string
  query?: Record<string, string | number>
}

export type AppWorkspaceId = number | string
export type RouteScope = 'platform' | 'tenant' | 'app'
export type MenuContext = 'tenantShell' | 'appWorkspace'

/** 携带作用域元数据的路由最小形状：作用域只来自 meta，不再从路径前缀推断。 */
type ScopedRoute = { meta?: { appScope?: RouteScope; menuContext?: MenuContext } } | undefined | null

const toId = (value: unknown): AppWorkspaceId | undefined => {
  if (value === undefined || value === null || value === '') {
    return undefined
  }
  const text = String(value).trim()
  if (!/^[1-9]\d*$/.test(text)) {
    return undefined
  }
  const id = Number(text)
  return Number.isSafeInteger(id) ? id : text
}

export function normalizeAppId(value: unknown): AppWorkspaceId | undefined {
  return toId(Array.isArray(value) ? value[0] : value)
}

export function normalizeRouteAppId(value: unknown): AppWorkspaceId | undefined {
  const source = value && typeof value === 'object' && 'query' in value
    ? (value as { query?: Record<string, unknown> }).query
    : value
  return normalizeAppId((source as Record<string, unknown> | undefined)?.appId)
}

export function visibleApps(apps: AppWorkspaceOption[] = []): AppWorkspaceOption[] {
  return apps.filter((app) => app && app.isEnabled !== false && toId(app.id) !== undefined)
}

export function decideTenantAppEntry(apps: AppWorkspaceOption[] = []): AppEntryTarget {
  const candidates = visibleApps(apps)
  if (candidates.length === 0) {
    return { path: APP_CREATE_GUIDE_PATH }
  }
  // 不论应用数量，首次进入一律先展示应用列表，由用户显式选择后进入工作台
  return { path: APP_SELECTION_PATH }
}

/**
 * 路由应用作用域：直接读 meta.appScope，缺省 tenant。
 * 作用域是后端 global_menu.app_scope 的显式数据，前端不再用路径前缀推断。
 */
export function resolveRouteScope(route: ScopedRoute): RouteScope {
  return route?.meta?.appScope ?? 'tenant'
}

/**
 * 路由菜单上下文：meta.menuContext 优先，否则由 meta.appScope 派生。
 * 平台域（appScope=platform）不切换上下文：侧边栏在平台管理子页下保持完整菜单。
 */
export function resolveMenuContext(route: ScopedRoute): MenuContext {
  const ctx = route?.meta?.menuContext
  if (ctx === 'tenantShell' || ctx === 'appWorkspace') {
    return ctx
  }
  const scope = route?.meta?.appScope
  if (scope === 'app') {
    return 'appWorkspace'
  }
  return 'tenantShell'
}

/** 是否应用工作台域路由（导航需绑定具体 AppId）。 */
export function isAppScopedRoute(route: ScopedRoute): boolean {
  return resolveRouteScope(route) === 'app'
}

/** 导航到该路由时是否需要在 query 里携带 appId。 */
export function shouldCarryAppId(route: ScopedRoute): boolean {
  return resolveRouteScope(route) === 'app'
}

export function isAppWorkspaceRoutePath(path: string): boolean {
  return path === APP_CREATE_GUIDE_PATH || path === APP_SELECTION_PATH || path === APP_WORKSPACE_PATH
}

function getRouteContext(route: any): MenuContext {
  return resolveMenuContext(route)
}

export function filterRoutesByMenuContext(routes: any[] = [], context: MenuContext): any[] {
  return routes
    .filter((item: any) => typeof item?.path === 'string' && item.path.length > 0)
    .filter((item: any) => !item?.meta?.isHide)
    .map((item: any) => {
      const next = { ...item }
      if (next.children) {
        next.children = filterRoutesByMenuContext(next.children, context)
      }
      return next
    })
    .filter((item: any) => {
      const itemContext = getRouteContext(item)
      return itemContext === context || (item.children?.length ?? 0) > 0
    })
}
