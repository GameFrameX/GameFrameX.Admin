// 举报/处罚/申诉 共用：处罚对象类型枚举（PenaltyTargetTypeEnum）
// 对齐后端 GameFrameX.Core.Enum.PenaltyTargetTypeEnum
export const PENALTY_TARGET_TYPE_VALUES = {
  Account: 1,
  Role: 2,
  Device: 3,
  Ip: 4,
} as const

export interface EnumOption {
  value: number
  labelKey: string
  severity: 'success' | 'warn' | 'info' | 'danger' | 'secondary' | 'contrast'
}

export const PENALTY_TARGET_TYPE_OPTIONS: EnumOption[] = [
  { value: PENALTY_TARGET_TYPE_VALUES.Account, labelKey: 'message.gameBusiness.trustSafety.targetType.account', severity: 'info' },
  { value: PENALTY_TARGET_TYPE_VALUES.Role, labelKey: 'message.gameBusiness.trustSafety.targetType.role', severity: 'info' },
  { value: PENALTY_TARGET_TYPE_VALUES.Device, labelKey: 'message.gameBusiness.trustSafety.targetType.device', severity: 'info' },
  { value: PENALTY_TARGET_TYPE_VALUES.Ip, labelKey: 'message.gameBusiness.trustSafety.targetType.ip', severity: 'info' },
]

export const findEnumOption = (options: EnumOption[], value: number | undefined | null): EnumOption | undefined => {
  if (value === undefined || value === null) {
    return undefined
  }
  return options.find((o) => o.value === value)
}

// 举报类型 ReportTypeEnum
export const REPORT_TYPE_OPTIONS: EnumOption[] = [
  { value: 1, labelKey: 'message.gameBusiness.trustSafety.report.type.cheat', severity: 'danger' },
  { value: 2, labelKey: 'message.gameBusiness.trustSafety.report.type.abuse', severity: 'warn' },
  { value: 3, labelKey: 'message.gameBusiness.trustSafety.report.type.illegal', severity: 'danger' },
  { value: 4, labelKey: 'message.gameBusiness.trustSafety.report.type.spam', severity: 'warn' },
  { value: 5, labelKey: 'message.gameBusiness.trustSafety.report.type.other', severity: 'secondary' },
]

// 举报状态 ReportStatusEnum
export const REPORT_STATUS_OPTIONS: EnumOption[] = [
  { value: 1, labelKey: 'message.gameBusiness.trustSafety.report.statusOption.pending', severity: 'warn' },
  { value: 2, labelKey: 'message.gameBusiness.trustSafety.report.statusOption.investigating', severity: 'info' },
  { value: 3, labelKey: 'message.gameBusiness.trustSafety.report.statusOption.confirmed', severity: 'danger' },
  { value: 4, labelKey: 'message.gameBusiness.trustSafety.report.statusOption.dismissed', severity: 'secondary' },
]
