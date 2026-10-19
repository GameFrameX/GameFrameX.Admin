import { findEnumOption, type EnumOption } from '../gameReportEntity/constants'

export { findEnumOption, type EnumOption }

// 申诉状态 AppealStatusEnum
export const APPEAL_STATUS_OPTIONS: EnumOption[] = [
  { value: 1, labelKey: 'message.gameBusiness.trustSafety.appeal.statusOption.pending', severity: 'warn' },
  { value: 2, labelKey: 'message.gameBusiness.trustSafety.appeal.statusOption.approved', severity: 'success' },
  { value: 3, labelKey: 'message.gameBusiness.trustSafety.appeal.statusOption.rejected', severity: 'danger' },
]
