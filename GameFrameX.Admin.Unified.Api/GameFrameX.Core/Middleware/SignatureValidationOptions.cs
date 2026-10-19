namespace GameFrameX.Core.Middleware;

/// <summary>
/// 传输签名验证配置（C1 hmac-foundation-migration 落地；C2 sig-keyring-server 调整启用语义）。
/// </summary>
public sealed class SignatureValidationOptions
{
    /// <summary>
    /// 灰度总开关（C2 起语义降级）。默认关闭 = 中间件完全旁路，现网零行为变化；
    /// 打开后验签失败按 App 级 RiskEnforcementMode 分流（FailClosed 拒绝 / AuditOnly、FailOpen 记录放行），
    /// v2 密钥环验签同时可用。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// v1 常量签名密钥。默认值与客户端 NetLib.ApiServerHttpRequest.SignatureSecret 同源
    /// （C1 决策：密钥维持常量模式）。C2 起仅为 v1 过渡期回退路径，App 级 v2 密钥环
    /// （TenantAppSignatureKeyEntity）是正式密钥来源；C3 全量切换后本常量退役。
    /// </summary>
    public string Secret { get; set; } = "iwyWDopevzWv7zyriHi3DDtuwpKMOzfFPq0lkNITV44cPGD_";

    /// <summary>
    /// 时间戳容忍窗口（秒），双侧对称。
    /// </summary>
    public int TimestampToleranceSeconds { get; set; } = 300;

    /// <summary>
    /// 防重放记账 TTL（秒），不小于 2 × 时间窗。
    /// </summary>
    public int ReplayTtlSeconds { get; set; } = 600;
}