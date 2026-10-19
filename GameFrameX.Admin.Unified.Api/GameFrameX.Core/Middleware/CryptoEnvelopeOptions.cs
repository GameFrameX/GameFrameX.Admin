namespace GameFrameX.Core.Middleware;

/// <summary>
/// 加密信封配置（change C5 server-envelope-middleware），形状照 <see cref="SignatureValidationOptions"/>。
/// </summary>
public sealed class CryptoEnvelopeOptions
{
    /// <summary>
    /// 灰度总开关。默认关闭 = 中间件完全旁路（不解密不加密，现网零行为变化）；
    /// 打开后：请求带 X-Crypto: 1 即按信封解密（失败 400 + 60104/60105/60106），命中信封的响应加密回信封；
    /// 无 X-Crypto 头的请求原样放行（灰度窗口双形态——未注入会话密钥的旧客户端走明文）。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 测试租户 ID 列表：命中的租户即使 IsEnabled 打开也旁路信封（Swagger/联调明文可用，坑清单 #6）。
    /// </summary>
    public List<long> TestTenantIds { get; set; } = new();
}