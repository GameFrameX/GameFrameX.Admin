namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏交易域兑换码错误码（段 703，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum RedeemCodeErrorCode
{
    /// <summary>
    /// 兑换码已过期
    /// </summary>
    [ErrorText("兑换码已过期", "Redeem code has expired")]
    RedeemCodeExpired = 70300001,

    /// <summary>
    /// 兑换码次数已用完
    /// </summary>
    [ErrorText("兑换码次数已用完", "Redeem code usage limit reached")]
    RedeemCodeLimit = 70300002,

    /// <summary>
    /// 兑换码未激活
    /// </summary>
    [ErrorText("兑换码未激活", "Redeem code is not activated")]
    RedeemCodeNotActive = 70300003,

    /// <summary>
    /// 兑换码已使用
    /// </summary>
    [ErrorText("兑换码已使用", "Redeem code has already been used")]
    RedeemCodeUsed = 70300004,

    /// <summary>
    /// 兑换码不存在
    /// </summary>
    [ErrorText("兑换码不存在", "Redeem code not found")]
    RedeemCodeNotFound = 70300005,

    /// <summary>
    /// 兑换码调用错误
    /// </summary>
    [ErrorText("兑换码调用错误", "Redeem code invocation error")]
    RedeemCodeInvokeError = 70300006,

    /// <summary>
    /// 兑换码分组每角色次数已用完
    /// </summary>
    [ErrorText("兑换码分组每角色次数已用完", "Per-role usage limit of this redeem code group reached")]
    RedeemCodeChannelRoleLimit = 70300007,
}