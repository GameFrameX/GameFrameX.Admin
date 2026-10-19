import type { RouteRecordRaw } from 'vue-router'
import {
  APP_CREATE_GUIDE_PATH,
  APP_SELECTION_PATH,
  APP_WORKSPACE_PATH,
} from '../utils/appWorkspace'

const appWorkspaceRoutes: RouteRecordRaw[] = [
  {
    // APP-驾驶舱：租户选定应用后的运营首页（三层驾驶舱，menuContext=appWorkspace
    // 使顶栏应用切换器可见；appScope=tenant 使其不要求 query.appId）
    path: '/dashboard/appHome',
    name: 'appHome',
    component: () => import('/@/views/tenant/appHome/AppHomeBoard.vue'),
    meta: {
      title: 'message.appHome.pageTitle',
      isHide: true,
      isKeepAlive: false,
      menuContext: 'appWorkspace',
      appScope: 'tenant',
    },
  },
  {
    path: APP_CREATE_GUIDE_PATH,
    name: 'appEntry',
    component: () => import('/@/views/tenant/appCenter/appEntry/index.vue'),
    meta: {
      title: 'gameBusiness.appWorkspace.createFirstApp',
      isHide: true,
      isKeepAlive: false,
      menuContext: 'tenantShell',
      appScope: 'tenant',
    },
  },
  {
    path: APP_SELECTION_PATH,
    name: 'appSelector',
    component: () => import('/@/views/tenant/appCenter/appEntry/index.vue'),
    meta: {
      title: 'gameBusiness.appWorkspace.selectApp',
      isHide: true,
      isKeepAlive: false,
      menuContext: 'tenantShell',
      appScope: 'tenant',
    },
  },
  {
    path: APP_WORKSPACE_PATH,
    name: 'appWorkspace',
    component: () => import('/@/views/app/appCenter/appWorkspace/index.vue'),
    meta: {
      title: 'gameBusiness.appWorkspace.title',
      isHide: true,
      isKeepAlive: false,
      menuContext: 'appWorkspace',
      appScope: 'app',
    },
  },
  {
    // 凭证中心操作台（C375 fallback 路由；后端菜单 seed 在 C376 落地前启用）。
    // 由 registerAppWorkspaceRoutes 检测到动态路由已注册同名 path 时跳过注册，避免与 C376 冲突。
    path: '/app/appCenter/credentialCenter',
    name: 'credentialCenter',
    component: () => import('/@/views/app/appCenter/credentialCenter/index.vue'),
    meta: {
      title: 'gameBusiness.credentialCenter.title',
      isHide: true,
      isKeepAlive: false,
      menuContext: 'appWorkspace',
      appScope: 'app',
    },
  },
]

export function registerAppWorkspaceRoutes(router: { hasRoute: (name: string) => boolean; addRoute: (parentName: string, route: RouteRecordRaw) => void }) {
  for (const route of appWorkspaceRoutes) {
    if (!router.hasRoute(route.name as string)) {
      router.addRoute('/', route)
    }
  }
}
