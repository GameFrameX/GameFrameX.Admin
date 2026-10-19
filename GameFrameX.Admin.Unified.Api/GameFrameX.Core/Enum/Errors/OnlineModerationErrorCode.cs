namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Online 社交治理 Admin 侧错误码（段 317，见 <see cref="ErrorSegmentRegistry"/>；来源 change C350 / vault:C7）。
/// <para>
/// 与 <see cref="OnlineErrorCode"/>（32x 镜像段，专用于 Online 协议码的段级映射）不同：
/// 本枚举承载 Admin 侧自身的业务校验（处罚事实源状态、联动目标可达性），
/// 不映射 Online 协议码，不可与镜像段混用。
/// </para>
/// </summary>
public enum OnlineModerationErrorCode
{
    /// <summary>
    /// 处罚联动要求处罚处于生效中（Active）
    /// </summary>
    [ErrorText("处罚不是生效状态，无法联动 Online 运行时", "Penalty is not active; cannot link to Online runtime")]
    PenaltyNotActive = 31700001,

    /// <summary>
    /// 处罚动作无对应 Online 联动命令（RestrictTrade / RestrictMatch 仅落账）
    /// </summary>
    [ErrorText("该处罚动作无对应 Online 联动命令", "Penalty action has no corresponding Online link command")]
    PenaltyActionNotLinkable = 31700002,

    /// <summary>
    /// 应用下无区服可联动（联动命令需按区服解析 Online 管理 URL）
    /// </summary>
    [ErrorText("应用下没有可联动的区服", "No linkable game area under the application")]
    PenaltyLinkTargetMissing = 31700003,
}
