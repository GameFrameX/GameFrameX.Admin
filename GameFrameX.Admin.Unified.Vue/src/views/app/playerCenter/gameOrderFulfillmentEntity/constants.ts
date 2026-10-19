// 权益发货账本前端枚举常量（对齐后端 FulfillmentTypeEnum / FulfillmentStatusEnum）
// label 通过 i18nKey 在组件内动态解析（t()），避免模块加载时固化 locale。

/** 发放类型（FulfillmentTypeEnum） */
export const FULFILLMENT_TYPE_VALUES = {
  Purchase: 1,
  Refund: 2,
  Compensate: 3,
} as const

/** 发放状态（FulfillmentStatusEnum） */
export const FULFILLMENT_STATUS_VALUES = {
  Pending: 1,
  Success: 2,
  Failed: 3,
  Voided: 4,
} as const

/** 发放类型下拉选项（i18nKey 对应 message.gameBusiness.orderFulfillment.*） */
export const FULFILLMENT_TYPE_OPTIONS = [
  { value: FULFILLMENT_TYPE_VALUES.Purchase, i18nKey: 'typePurchase' },
  { value: FULFILLMENT_TYPE_VALUES.Refund, i18nKey: 'typeRefund' },
  { value: FULFILLMENT_TYPE_VALUES.Compensate, i18nKey: 'typeCompensate' },
]

/** 发放状态下拉选项（i18nKey 对应 message.gameBusiness.orderFulfillment.*） */
export const FULFILLMENT_STATUS_OPTIONS = [
  { value: FULFILLMENT_STATUS_VALUES.Pending, i18nKey: 'statusPending' },
  { value: FULFILLMENT_STATUS_VALUES.Success, i18nKey: 'statusSuccess' },
  { value: FULFILLMENT_STATUS_VALUES.Failed, i18nKey: 'statusFailed' },
  { value: FULFILLMENT_STATUS_VALUES.Voided, i18nKey: 'statusVoided' },
]
