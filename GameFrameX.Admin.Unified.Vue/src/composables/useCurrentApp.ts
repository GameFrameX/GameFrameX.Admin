import { ref } from 'vue'
import type { LocationQuery } from 'vue-router'
import { tenantGameAppEntityListGet } from '../api/tenantGameAppEntity'
import type { AppWorkspaceId } from '../utils/appWorkspace'
import { normalizeRouteAppId } from '../utils/appWorkspace'
import { session } from '../utils/storage'

export interface AppOption {
  id: number | string
  name: string
  appName?: string
  isEnabled?: boolean
}

// 模块级单例：所有调用方共享同一份应用字典与当前应用 id（GFX-78 / C96）。
// 不引入 Pinia store；以 Vue 3 模块级 ref 为页面和 shell 共享当前应用上下文。
const appOptions = ref<AppOption[]>([])
let loaded = false

// 已选应用做 sessionStorage 级持久（HOME-001 方案 A，2026-08-31 拍板）：
// 刷新页面保持租户驾驶舱上下文；关标签页后重新选应用
const CURRENT_APP_SESSION_KEY = 'currentAppId'
const currentAppId = ref<AppWorkspaceId | undefined>(session.get(CURRENT_APP_SESSION_KEY) ?? undefined)

export function useCurrentApp() {
  const appName = (id: number | string | undefined | null): string => {
    if (id === undefined || id === null || id === '') {
      return ''
    }
    const found = appOptions.value.find((a) => String(a.id) === String(id))
    return found?.name ?? ''
  }

  // 幂等：已加载则跳过，避免每个调用页重复请求 tenantGameAppEntityListGet。
  const loadApps = async (force = false) => {
    if (loaded && !force) {
      return appOptions.value
    }
    const res = await tenantGameAppEntityListGet()
    appOptions.value = (res.data?.data ?? []) as unknown as AppOption[]
    loaded = true
    return appOptions.value
  }

  const reloadApps = () => loadApps(true)

  const resolveInitialAppId = (queryOrRoute: LocationQuery | { query?: LocationQuery }): AppWorkspaceId | undefined => {
    return normalizeRouteAppId(queryOrRoute)
  }

  const setCurrentApp = (id: AppWorkspaceId | undefined) => {
    currentAppId.value = id
    if (id === undefined) {
      session.remove(CURRENT_APP_SESSION_KEY)
    } else {
      session.set(CURRENT_APP_SESSION_KEY, String(id))
    }
  }

  const currentApp = () => {
    if (currentAppId.value === undefined) {
      return undefined
    }
    return appOptions.value.find((a) => String(a.id) === String(currentAppId.value))
  }

  // route.query.appId 优先（深链不被破坏），否则回落全局 currentAppId（跨页保持）。
  // 返回值供页面作为 queryParams.<appIdKey> 初值。
  const ensureCurrentAppFromQuery = (queryOrRoute: LocationQuery | { query?: LocationQuery }): AppWorkspaceId | undefined => {
    const fromQuery = resolveInitialAppId(queryOrRoute)
    const id = fromQuery ?? currentAppId.value
    currentAppId.value = id
    return id
  }

  return {
    appOptions,
    appName,
    loadApps,
    reloadApps,
    resolveInitialAppId,
    currentAppId,
    currentApp,
    setCurrentApp,
    ensureCurrentAppFromQuery,
  }
}
