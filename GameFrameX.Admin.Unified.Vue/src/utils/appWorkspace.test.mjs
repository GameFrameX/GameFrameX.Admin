import { test } from 'node:test'
import assert from 'node:assert/strict'
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
} from './appWorkspace.ts'

test('decideTenantAppEntry routes tenants by app count', () => {
  assert.equal(decideTenantAppEntry([]).path, APP_CREATE_GUIDE_PATH)
  // 产品决策：不论应用数量，首次进入一律先展示应用选择器（见 appWorkspace.ts decideTenantAppEntry 注释）
  assert.equal(decideTenantAppEntry([{ id: 7, name: 'A' }]).path, APP_SELECTION_PATH)
  assert.equal(decideTenantAppEntry([{ id: 7, name: 'A' }, { id: 8, name: 'B' }]).path, APP_SELECTION_PATH)
})

test('normalizeAppId preserves Snowflake ids that exceed JavaScript safe integers', () => {
  const snowflake = '2599183429848999758'
  assert.equal(normalizeAppId(snowflake), snowflake)
})

test('normalizeRouteAppId accepts either route query or the route object used by existing pages', () => {
  const snowflake = '2599183429848999758'
  assert.equal(normalizeRouteAppId({ appId: snowflake }), snowflake)
  assert.equal(normalizeRouteAppId({ query: { appId: snowflake } }), snowflake)
})

test('app-scoped routes resolve to app workspace context', () => {
  const route = { meta: { appScope: 'app', menuContext: 'appWorkspace' } }
  assert.equal(resolveRouteScope(route), 'app')
  assert.equal(isAppScopedRoute(route), true)
  assert.equal(shouldCarryAppId(route), true)
  assert.equal(resolveMenuContext(route), 'appWorkspace')
})

test('tenant routes stay in tenant shell context', () => {
  const route = { meta: { appScope: 'tenant' } }
  assert.equal(resolveRouteScope(route), 'tenant')
  assert.equal(isAppScopedRoute(route), false)
  assert.equal(shouldCarryAppId(route), false)
  assert.equal(resolveMenuContext(route), 'tenantShell')
})

test('platform routes resolve to platform context', () => {
  assert.equal(resolveMenuContext({ meta: { appScope: 'platform' } }), 'platform')
  assert.equal(resolveMenuContext({ meta: { menuContext: 'platform' } }), 'platform')
})

test('routes without scope meta default to tenant shell', () => {
  assert.equal(resolveRouteScope({}), 'tenant')
  assert.equal(resolveMenuContext(undefined), 'tenantShell')
  assert.equal(isAppScopedRoute(null), false)
})

test('filterRoutesByMenuContext ignores button-only children when selecting shell menus', () => {
  const routes = [
    {
      path: '/appCenter',
      meta: { menuContext: 'tenantShell', isHide: false },
      children: [
        {
          path: '/appCenter/gameAppEntity',
          meta: { menuContext: 'tenantShell', isHide: false },
          children: [
            { path: '', meta: { menuContext: 'tenantShell', isHide: false } },
          ],
        },
        {
          path: '/appCenter/resource',
          meta: { menuContext: 'appWorkspace', isHide: false },
          children: [
            { path: '', meta: { menuContext: 'tenantShell', isHide: false } },
          ],
        },
      ],
    },
  ]

  const tenantShellRoutes = filterRoutesByMenuContext(routes, 'tenantShell')
  const appWorkspaceRoutes = filterRoutesByMenuContext(routes, 'appWorkspace')

  assert.deepEqual(tenantShellRoutes.map((item) => item.path), ['/appCenter'])
  assert.deepEqual(tenantShellRoutes[0].children.map((item) => item.path), ['/appCenter/gameAppEntity'])
  assert.deepEqual(appWorkspaceRoutes.map((item) => item.path), ['/appCenter'])
  assert.deepEqual(appWorkspaceRoutes[0].children.map((item) => item.path), ['/appCenter/resource'])
})
