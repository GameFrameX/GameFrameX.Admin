import { test, expect } from 'vitest'
import {
  APP_CREATE_GUIDE_PATH,
  APP_SELECTION_PATH,
  filterRoutesByMenuContext,
  decideTenantAppEntry,
  isAppScopedRoute,
  normalizeAppId,
  normalizeRouteAppId,
  resolveMenuContext,
  resolveRouteScope,
  shouldCarryAppId,
  type RouteScope,
  type MenuContext,
} from './appWorkspace'

// 路由 meta 字段类型需要保留字面量（RouteScope = 'platform' | 'tenant' | 'app'）。
// 用 helper 在 helper 内部打 as const / 显式类型，保证 resolveRouteScope 等函数收到窄类型。
type RouteShape = { meta: { appScope: RouteScope; menuContext?: MenuContext } }

// filterRoutesByMenuContext 接受 any[] 输入，但子节点含 meta 必须按 ScopedRoute 收窄。
// 测试用本地类型表达最小路由形状，让 .map(item => item.path) 不退化成 any。
interface AppRouteNode {
	path: string
	meta?: { menuContext?: 'tenantShell' | 'appWorkspace'; isHide?: boolean }
	children: AppRouteNode[]
}

const route = (appScope: RouteScope, menuContext?: MenuContext): RouteShape =>
	({ meta: menuContext === undefined ? { appScope } : { appScope, menuContext } })

test('decideTenantAppEntry routes tenants by app count', () => {
	expect(decideTenantAppEntry([]).path).toBe(APP_CREATE_GUIDE_PATH)
	// 产品决策：不论应用数量，首次进入一律先展示应用选择器（见 appWorkspace.ts decideTenantAppEntry 注释）
	expect(decideTenantAppEntry([{ id: 7, name: 'A' }]).path).toBe(APP_SELECTION_PATH)
	expect(decideTenantAppEntry([{ id: 7, name: 'A' }, { id: 8, name: 'B' }]).path).toBe(APP_SELECTION_PATH)
})

test('normalizeAppId preserves Snowflake ids that exceed JavaScript safe integers', () => {
	const snowflake = '2599183429848999758'
	expect(normalizeAppId(snowflake)).toBe(snowflake)
})

test('normalizeRouteAppId accepts either route query or the route object used by existing pages', () => {
	const snowflake = '2599183429848999758'
	expect(normalizeRouteAppId({ appId: snowflake })).toBe(snowflake)
	expect(normalizeRouteAppId({ query: { appId: snowflake } })).toBe(snowflake)
})

test('app-scoped routes resolve to app workspace context', () => {
	const r = route('app', 'appWorkspace')
	expect(resolveRouteScope(r)).toBe('app')
	expect(isAppScopedRoute(r)).toBe(true)
	expect(shouldCarryAppId(r)).toBe(true)
	expect(resolveMenuContext(r)).toBe('appWorkspace')
})

test('tenant routes stay in tenant shell context', () => {
	const r = route('tenant')
	expect(resolveRouteScope(r)).toBe('tenant')
	expect(isAppScopedRoute(r)).toBe(false)
	expect(shouldCarryAppId(r)).toBe(false)
	expect(resolveMenuContext(r)).toBe('tenantShell')
})

test('platform routes stay in tenant shell context (platform scope does not switch menu context)', () => {
	// 平台域（appScope=platform）不切换菜单上下文（见 appWorkspace.ts resolveMenuContext 注释）：
	// 侧边栏在平台管理子页下保持完整菜单。MenuContext 类型本身只允许 'tenantShell' | 'appWorkspace'。
	expect(resolveMenuContext(route('platform'))).toBe('tenantShell')
	// 显式 menuContext 优先：appWorkspace 直接返回
	expect(resolveMenuContext({ meta: { menuContext: 'appWorkspace' as const } })).toBe('appWorkspace')
})

test('routes without scope meta default to tenant shell', () => {
	expect(resolveRouteScope({})).toBe('tenant')
	expect(resolveMenuContext(undefined)).toBe('tenantShell')
	expect(isAppScopedRoute(null)).toBe(false)
})

test('filterRoutesByMenuContext ignores button-only children when selecting shell menus', () => {
	const routes: AppRouteNode[] = [
		{
			path: '/appCenter',
			meta: { menuContext: 'tenantShell' as const, isHide: false },
			children: [
				{
					path: '/appCenter/gameAppEntity',
					meta: { menuContext: 'tenantShell' as const, isHide: false },
					children: [
						{ path: '', meta: { menuContext: 'tenantShell' as const, isHide: false }, children: [] },
					],
				},
				{
					path: '/appCenter/resource',
					meta: { menuContext: 'appWorkspace' as const, isHide: false },
					children: [
						{ path: '', meta: { menuContext: 'tenantShell' as const, isHide: false }, children: [] },
					],
				},
			],
		},
	]

	// filterRoutesByMenuContext 的签名是 (any[], MenuContext): any[]——故意不锁型，
	// 测试侧用 AppRouteNode[] 收窄：输入仍是 AppRouteNode[]，cast 是事实声明，非外部输入。
	const tenantShellRoutes = filterRoutesByMenuContext(routes, 'tenantShell') as AppRouteNode[]
	const appWorkspaceRoutes = filterRoutesByMenuContext(routes, 'appWorkspace') as AppRouteNode[]

	expect(tenantShellRoutes.map((item) => item.path)).toEqual(['/appCenter'])
	expect(tenantShellRoutes[0].children.map((item) => item.path)).toEqual(['/appCenter/gameAppEntity'])
	expect(appWorkspaceRoutes.map((item) => item.path)).toEqual(['/appCenter'])
	expect(appWorkspaceRoutes[0].children.map((item) => item.path)).toEqual(['/appCenter/resource'])
})

