// C124 / GFX-115 处罚生效状态查询页枚举常量 + i18n key + severity map + 运行时生效状态计算。
// 对齐后端 C115（GFX-112）枚举：GameFrameX.Core.Enum。

export const PENALTY_TARGET_TYPE = {
    Account: 1,
    Role: 2,
    Device: 3,
    Ip: 4,
} as const;

export const PENALTY_TARGET_TYPE_OPTIONS = [
    {value: PENALTY_TARGET_TYPE.Account, label: 'message.gameBusiness.penaltyEnforcement.targetType.account'},
    {value: PENALTY_TARGET_TYPE.Role, label: 'message.gameBusiness.penaltyEnforcement.targetType.role'},
    {value: PENALTY_TARGET_TYPE.Device, label: 'message.gameBusiness.penaltyEnforcement.targetType.device'},
    {value: PENALTY_TARGET_TYPE.Ip, label: 'message.gameBusiness.penaltyEnforcement.targetType.ip'},
];

export const PENALTY_ACTION_TYPE = {
    Ban: 1,
    RestrictLogin: 2,
    Mute: 3,
    RestrictTrade: 4,
    RestrictMatch: 5,
} as const;

export const PENALTY_ACTION_TYPE_OPTIONS = [
    {value: PENALTY_ACTION_TYPE.Ban, label: 'message.gameBusiness.penaltyEnforcement.actionType.ban'},
    {value: PENALTY_ACTION_TYPE.RestrictLogin, label: 'message.gameBusiness.penaltyEnforcement.actionType.restrictLogin'},
    {value: PENALTY_ACTION_TYPE.Mute, label: 'message.gameBusiness.penaltyEnforcement.actionType.mute'},
    {value: PENALTY_ACTION_TYPE.RestrictTrade, label: 'message.gameBusiness.penaltyEnforcement.actionType.restrictTrade'},
    {value: PENALTY_ACTION_TYPE.RestrictMatch, label: 'message.gameBusiness.penaltyEnforcement.actionType.restrictMatch'},
];

export const PENALTY_STATUS = {
    Active: 1,
    Revoked: 2,
    Expired: 3,
} as const;

export const PENALTY_STATUS_OPTIONS = [
    {value: PENALTY_STATUS.Active, label: 'message.gameBusiness.penaltyEnforcement.status.active'},
    {value: PENALTY_STATUS.Revoked, label: 'message.gameBusiness.penaltyEnforcement.status.revoked'},
    {value: PENALTY_STATUS.Expired, label: 'message.gameBusiness.penaltyEnforcement.status.expired'},
];

// 运行时生效状态（前端计算，非后端枚举）。对齐 C115 §3.3 Enforcement 命中查询铁律：
// Status=Active AND (ExpiresAt IS NULL OR ExpiresAt>now) → 命中（生效中）。
export const RUNTIME_STATUS = {
    Effective: 'effective',
    Expired: 'expired',
    Revoked: 'revoked',
} as const;

export const runtimeStatusSeverity = (status: string): 'success' | 'warn' | 'danger' => {
    if (status === RUNTIME_STATUS.Effective) {
        return 'success';
    }
    if (status === RUNTIME_STATUS.Expired) {
        return 'warn';
    }
    return 'danger';
};

// 运行时生效状态计算：前端镜像后端 Enforcement 读取时判定。
export const computeRuntimeStatus = (row: any): string => {
    if (!row) {
        return RUNTIME_STATUS.Expired;
    }
    if (row.status === PENALTY_STATUS.Revoked) {
        return RUNTIME_STATUS.Revoked;
    }
    if (row.status === PENALTY_STATUS.Expired) {
        return RUNTIME_STATUS.Expired;
    }
    if (row.status === PENALTY_STATUS.Active) {
        if (!row.expireTime) {
            return RUNTIME_STATUS.Effective;
        }
        const nowSeconds = Math.floor(Date.now() / 1000);
        return row.expireTime > 0 && row.expireTime > nowSeconds ? RUNTIME_STATUS.Effective : RUNTIME_STATUS.Expired;
    }
    return RUNTIME_STATUS.Expired;
};

export const actionTypeSeverity = (value: number): 'danger' | 'warn' | 'info' | 'secondary' => {
    if (value === PENALTY_ACTION_TYPE.Ban || value === PENALTY_ACTION_TYPE.RestrictLogin) {
        return 'danger';
    }
    if (value === PENALTY_ACTION_TYPE.Mute) {
        return 'warn';
    }
    if (value === PENALTY_ACTION_TYPE.RestrictTrade || value === PENALTY_ACTION_TYPE.RestrictMatch) {
        return 'info';
    }
    return 'secondary';
};

export const statusSeverity = (value: number): 'success' | 'danger' | 'warn' | 'secondary' => {
    if (value === PENALTY_STATUS.Active) {
        return 'success';
    }
    if (value === PENALTY_STATUS.Revoked) {
        return 'danger';
    }
    if (value === PENALTY_STATUS.Expired) {
        return 'warn';
    }
    return 'secondary';
};
