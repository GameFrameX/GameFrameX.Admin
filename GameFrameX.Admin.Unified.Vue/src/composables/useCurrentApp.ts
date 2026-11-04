import { ref } from 'vue'
import type { LocationQuery } from 'vue-router'
// ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
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

  // 幂等桩：已加载则直接返回（原实现按需请求应用列表）。
  const loadApps = async (force = false) => {
    // ponytail: C400 已删除后端端点，此处只为保持 build 可用；C403 连页删除
    // 不发请求、不伪造数据：应用列表恒为空，与接口 404 后的运行时可见行为一致。
    void force
    loaded = true
    appOptions.value = []
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
