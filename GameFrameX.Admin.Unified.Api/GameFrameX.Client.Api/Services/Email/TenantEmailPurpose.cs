namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件用途。
/// <para>面向玩家 / 租户品牌发信；必须依赖当前租户上下文，缺配置时返回明确失败，不允许改用系统邮件。</para>
/// </summary>
public enum TenantEmailPurpose
{
    /// <summary>
    /// 通配租户业务邮件配置（<c>Purpose=0</c>）。
    /// </summary>
    Default = 0,

    /// <summary>
    /// 玩家注册验证码。
    /// </summary>
    PlayerRegisterCode = 1,

    /// <summary>
    /// 玩家找回密码验证码。
    /// </summary>
    PlayerPasswordReset = 2,

    /// <summary>
    /// 玩家运营触达。
    /// </summary>
    PlayerEngagement = 3,
}