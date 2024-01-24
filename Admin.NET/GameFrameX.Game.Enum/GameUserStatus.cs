namespace GameFrameX.Game.Enum;

/// <summary>
/// 游戏玩家账号状态
/// </summary>
public enum GameUserStatus
{
    /// <summary>
    /// 新建
    /// 账号刚被创建，尚未完成所有激活流程或游戏初始设定。
    /// </summary>
    New,

    /// <summary>
    /// 待验证
    /// 账号已创建，但玩家需要完成邮箱验证、手机验证或其他形式的验证才能激活账号。
    /// </summary>
    PendingVerification,

    /// <summary>
    /// 活跃
    /// 账号处于正常使用状态，玩家可以自由登录和玩游戏。
    /// </summary>
    Active,

    /// <summary>
    /// 封禁/禁止登录
    /// 由于违反游戏规定（如作弊、滥用、违规行为等），账号被暂时或永久封禁。
    /// </summary>
    Banned,

    /// <summary>
    /// 暂停
    /// 账号因特定原因（如玩家的临时请求、支付问题等）被临时锁定，不能登录。
    /// </summary>
    Suspended,

    /// <summary>
    /// 已删除/停用
    /// 账号已经被玩家请求删除或者由于长期不活跃而被系统停用。
    /// </summary>
    Deleted,

    /// <summary>
    /// 安全风险
    /// 账号疑似被盗或出现安全问题，暂时被锁定，直到玩家通过安全验证
    /// </summary>
    Compromised,

    /// <summary>
    /// 审核中
    /// 账号因为可疑行为或举报正在被官方审核，可能暂时无法正常使用
    /// </summary>
    UnderReview,

    /// <summary>
    /// 限制
    /// 账号的某些功能被限制，比如不能发言、交易等，通常是因为不良行为导致的临时惩罚。
    /// </summary>
    Limited,
}