import { computed } from 'vue'
import { t } from '../i18n/index'
import { useCurrentApp } from './useCurrentApp'

export type ConfigScope = 'tenant' | 'app'

/** AppId=0 表示租户默认配置；AppId>0 表示应用覆盖（指向 TenantGameAppEntity.Id）。 */
export const TENANT_DEFAULT_APP_ID = 0

/**
 * 统一「租户默认 / 应用覆盖」配置作用域的交互逻辑。
 * 复用 useCurrentApp 拉取当前租户应用列表，避免重复请求与静态映射。
 */
export function useConfigAppScope() {
  const { appOptions, appName, loadApps } = useCurrentApp()

  const scopeOf = (appId: number | string | undefined | null): ConfigScope => {
    return Number(appId || 0) === 0 ? 'tenant' : 'app'
  }

  /** 租户默认 / 应用覆盖：{应用名称}（应用名缺失时回退为 App {id}）。 */
  const scopeLabel = (appId: number | string | undefined | null): string => {
    const n = Number(appId || 0)
    if (n === 0) {
      return t('gameBusiness.configScope.tenantDefault')
    }
    const name = appName(n) || appName(String(n))
    const override = t('gameBusiness.configScope.appOverride')
    return name ? `${override}：${name}` : `${override}：App ${n}`
  }

  /** 搜索下拉选项：租户默认 + 全部应用。value 为 number，清空即「全部」。 */
  const scopeSearchOptions = computed(() => {
    return [
      { label: t('gameBusiness.configScope.tenantDefault'), value: 0 },
      ...appOptions.value.map((a) => ({ label: a.name, value: Number(a.id) })),
    ]
  })

  return {
    appOptions,
    appName,
    loadApps,
    scopeOf,
    scopeLabel,
    scopeSearchOptions,
    TENANT_DEFAULT_APP_ID,
  }
}
