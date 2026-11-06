using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Message;

/// <summary>
/// 平台系统邮件用途。
/// <para>仅服务平台自身流程：平台安全、平台运维通知等；不依赖请求上下文，不得作为玩家邮件的静默 fallback。</para>
/// </summary>
[AdminEnum]
public enum SystemEmailPurpose
{
    /// <summary>
    /// 默认系统邮件配置（用途未指定时的兜底）。
    /// </summary>
    Default = 0,

    /// <summary>
    /// 平台安全通知（异常登录、密钥泄露告警等）。
    /// </summary>
    PlatformSecurity = 2,

    /// <summary>
    /// 平台运维通知（系统维护等）。
    /// </summary>
    PlatformOps = 3,

    /// <summary>
    /// 邮箱自助注册验证码（SaaS 平台注册流程）。
    /// </summary>
    RegisterVerificationCode = 4,
}