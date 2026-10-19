import { PENALTY_TARGET_TYPE_OPTIONS, PENALTY_TARGET_TYPE_VALUES, type EnumOption, findEnumOption } from '../gameReportEntity/constants'

export { PENALTY_TARGET_TYPE_OPTIONS, PENALTY_TARGET_TYPE_VALUES, type EnumOption, findEnumOption }

// 处罚动作 PenaltyActionTypeEnum
export const PENALTY_ACTION_TYPE_OPTIONS: EnumOption[] = [
  { value: 1, labelKey: 'message.gameBusiness.trustSafety.penalty.action.ban', severity: 'danger' },
  { value: 2, labelKey: 'message.gameBusiness.trustSafety.penalty.action.restrictLogin', severity: 'danger' },
  { value: 3, labelKey: 'message.gameBusiness.trustSafety.penalty.action.mute', severity: 'warn' },
  { value: 4, labelKey: 'message.gameBusiness.trustSafety.penalty.action.restrictTrade', severity: 'warn' },
  { value: 5, labelKey: 'message.gameBusiness.trustSafety.penalty.action.restrictMatch', severity: 'warn' },
]

// 处罚状态 PenaltyStatusEnum
export const PENALTY_STATUS_OPTIONS: EnumOption[] = [
  { value: 1, labelKey: 'message.gameBusiness.trustSafety.penalty.statusOption.active', severity: 'danger' },
  { value: 2, labelKey: 'message.gameBusiness.trustSafety.penalty.statusOption.revoked', severity: 'secondary' },
  { value: 3, labelKey: 'message.gameBusiness.trustSafety.penalty.statusOption.expired', severity: 'warn' },
]
