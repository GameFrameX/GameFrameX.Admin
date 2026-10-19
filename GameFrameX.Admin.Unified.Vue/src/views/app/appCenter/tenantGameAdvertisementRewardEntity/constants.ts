// 广告奖励发放账本前端枚举常量（对齐后端 AdRewardStatusEnum：Pending/Success/Failed 三态）
// label 通过 i18nKey 在组件内动态解析（t()），避免模块加载时固化 locale。

/** 发放状态（AdRewardStatusEnum，严格锁定三态） */
export const ADVERTISEMENT_REWARD_STATUS_VALUES = {
  Pending: 1,
  Success: 2,
  Failed: 3,
} as const

/** 发放状态下拉选项（i18nKey 对应 message.gameBusiness.advertisementReward.*） */
export const ADVERTISEMENT_REWARD_STATUS_OPTIONS = [
  { value: ADVERTISEMENT_REWARD_STATUS_VALUES.Pending, i18nKey: 'statusPending' },
  { value: ADVERTISEMENT_REWARD_STATUS_VALUES.Success, i18nKey: 'statusSuccess' },
  { value: ADVERTISEMENT_REWARD_STATUS_VALUES.Failed, i18nKey: 'statusFailed' },
]

/** Tag severity 按状态：Pending=warn / Success=success / Failed=danger */
export const adRewardStatusSeverity = (v: number): string | undefined => {
  const map: Record<number, string> = {
    [ADVERTISEMENT_REWARD_STATUS_VALUES.Pending]: 'warn',
    [ADVERTISEMENT_REWARD_STATUS_VALUES.Success]: 'success',
    [ADVERTISEMENT_REWARD_STATUS_VALUES.Failed]: 'danger',
  }
  return map[v] ?? undefined
}

/**
 * Provider 兜底选项（标准化值来自后端 AdCallbackConfigSchemaProvider.NormalizeProvider：
 * TopOn / GoogleAdMob / UnityAds）。运行期 index.vue 优先复用 tenantAdvertisementCallbackConfigEntity
 * 的 /providers 端点拉取真实 Provider 列表；此常量仅在端点不可用时兜底。
 */
export const ADVERTISEMENT_REWARD_PROVIDER_FALLBACK_OPTIONS = [
  { label: 'TopOn', value: 'TopOn' },
  { label: 'GoogleAdMob', value: 'GoogleAdMob' },
  { label: 'UnityAds', value: 'UnityAds' },
]
