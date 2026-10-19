// C124 / GFX-115 反作弊事件管理页枚举常量 + i18n key + severity map。
// 对齐后端 C116（GFX-114）枚举：GameFrameX.Core.Enum。

export const ANTI_CHEAT_PROVIDER = {
    TopOn: 1,
    GoogleAdMob: 2,
    UnityAds: 3,
    Custom: 99,
} as const;

export const ANTI_CHEAT_PROVIDER_OPTIONS = [
    { value: ANTI_CHEAT_PROVIDER.TopOn, label: 'message.gameBusiness.antiCheat.provider.topOn' },
    { value: ANTI_CHEAT_PROVIDER.GoogleAdMob, label: 'message.gameBusiness.antiCheat.provider.googleAdMob' },
    { value: ANTI_CHEAT_PROVIDER.UnityAds, label: 'message.gameBusiness.antiCheat.provider.unityAds' },
    { value: ANTI_CHEAT_PROVIDER.Custom, label: 'message.gameBusiness.antiCheat.provider.custom' },
];

export const RISK_LEVEL = {
    Low: 1,
    Medium: 2,
    High: 3,
    Critical: 4,
} as const;

export const RISK_LEVEL_OPTIONS = [
    { value: RISK_LEVEL.Low, label: 'message.gameBusiness.antiCheat.riskLevel.low' },
    { value: RISK_LEVEL.Medium, label: 'message.gameBusiness.antiCheat.riskLevel.medium' },
    { value: RISK_LEVEL.High, label: 'message.gameBusiness.antiCheat.riskLevel.high' },
    { value: RISK_LEVEL.Critical, label: 'message.gameBusiness.antiCheat.riskLevel.critical' },
];

export const ANTI_CHEAT_SUGGESTED_ACTION = {
    None: 1,
    Flag: 2,
    RestrictLogin: 3,
    Ban: 4,
} as const;

export const ANTI_CHEAT_SUGGESTED_ACTION_OPTIONS = [
    { value: ANTI_CHEAT_SUGGESTED_ACTION.None, label: 'message.gameBusiness.antiCheat.suggestedAction.none' },
    { value: ANTI_CHEAT_SUGGESTED_ACTION.Flag, label: 'message.gameBusiness.antiCheat.suggestedAction.flag' },
    { value: ANTI_CHEAT_SUGGESTED_ACTION.RestrictLogin, label: 'message.gameBusiness.antiCheat.suggestedAction.restrictLogin' },
    { value: ANTI_CHEAT_SUGGESTED_ACTION.Ban, label: 'message.gameBusiness.antiCheat.suggestedAction.ban' },
];

export const MANUAL_REVIEW_STATE = {
    Pending: 1,
    Approved: 2,
    Rejected: 3,
    Escalated: 4,
} as const;

export const MANUAL_REVIEW_STATE_OPTIONS = [
    { value: MANUAL_REVIEW_STATE.Pending, label: 'message.gameBusiness.antiCheat.reviewState.pending' },
    { value: MANUAL_REVIEW_STATE.Approved, label: 'message.gameBusiness.antiCheat.reviewState.approved' },
    { value: MANUAL_REVIEW_STATE.Rejected, label: 'message.gameBusiness.antiCheat.reviewState.rejected' },
    { value: MANUAL_REVIEW_STATE.Escalated, label: 'message.gameBusiness.antiCheat.reviewState.escalated' },
];

// 合法复核目标（Pending → Approved/Rejected/Escalated 单向迁移）。
export const REVIEW_TARGET_OPTIONS = [
    { value: MANUAL_REVIEW_STATE.Approved, label: 'message.gameBusiness.antiCheat.reviewState.approved' },
    { value: MANUAL_REVIEW_STATE.Rejected, label: 'message.gameBusiness.antiCheat.reviewState.rejected' },
    { value: MANUAL_REVIEW_STATE.Escalated, label: 'message.gameBusiness.antiCheat.reviewState.escalated' },
];

export const ANTI_CHEAT_TARGET_TYPE = {
    Account: 1,
    Role: 2,
    Device: 3,
    Ip: 4,
} as const;

export const ANTI_CHEAT_TARGET_TYPE_OPTIONS = [
    { value: ANTI_CHEAT_TARGET_TYPE.Account, label: 'message.gameBusiness.antiCheat.targetType.account' },
    { value: ANTI_CHEAT_TARGET_TYPE.Role, label: 'message.gameBusiness.antiCheat.targetType.role' },
    { value: ANTI_CHEAT_TARGET_TYPE.Device, label: 'message.gameBusiness.antiCheat.targetType.device' },
    { value: ANTI_CHEAT_TARGET_TYPE.Ip, label: 'message.gameBusiness.antiCheat.targetType.ip' },
];

// PrimeVue Tag severity 映射。
export const riskLevelSeverity = (value: number): 'info' | 'warn' | 'danger' | 'secondary' => {
    if (value === RISK_LEVEL.Critical) {
        return 'danger';
    }
    if (value === RISK_LEVEL.High) {
        return 'warn';
    }
    if (value === RISK_LEVEL.Medium) {
        return 'info';
    }
    return 'secondary';
};

export const reviewStateSeverity = (value: number): 'success' | 'danger' | 'warn' | 'secondary' => {
    if (value === MANUAL_REVIEW_STATE.Approved) {
        return 'success';
    }
    if (value === MANUAL_REVIEW_STATE.Rejected) {
        return 'danger';
    }
    if (value === MANUAL_REVIEW_STATE.Pending || value === MANUAL_REVIEW_STATE.Escalated) {
        return 'warn';
    }
    return 'secondary';
};

export const suggestedActionSeverity = (value: number): 'info' | 'warn' | 'danger' | 'secondary' => {
    if (value === ANTI_CHEAT_SUGGESTED_ACTION.Ban) {
        return 'danger';
    }
    if (value === ANTI_CHEAT_SUGGESTED_ACTION.RestrictLogin) {
        return 'warn';
    }
    if (value === ANTI_CHEAT_SUGGESTED_ACTION.Flag) {
        return 'info';
    }
    return 'secondary';
};
