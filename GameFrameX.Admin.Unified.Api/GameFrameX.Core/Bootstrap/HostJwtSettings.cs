namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 静态 JWT 配置件（C261，GFX-668）：自 Hub Program.cs 内嵌 private JwtSettings 类提升的全库唯一定义。
/// <para>Admin host 已迁移动态平台 JWT（<c>PlatformJwtRuntimeProvider</c> 每请求从主库读取参数），
/// 不再持有静态 JWTSettings；使用本配置件的 host（当前为 Hub）需保证配置值与主库平台 JWT 配置一致，
/// 以使 Admin 登录签发的 Token 可直接访问。绑定配置节固定为 <c>JWTSettings</c>。</para>
/// </summary>
public sealed class HostJwtSettings
{
    /// <summary>
    /// 签名密钥
    /// </summary>
    public string IssuerSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// 签发方
    /// </summary>
    public string ValidIssuer { get; set; } = string.Empty;

    /// <summary>
    /// 签收方
    /// </summary>
    public string ValidAudience { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证签名密钥
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; }

    /// <summary>
    /// 是否验证签发方
    /// </summary>
    public bool ValidateIssuer { get; set; }

    /// <summary>
    /// 是否验证签收方
    /// </summary>
    public bool ValidateAudience { get; set; }

    /// <summary>
    /// 是否验证过期时间
    /// </summary>
    public bool ValidateLifetime { get; set; }

    /// <summary>
    /// 过期时间容错值（秒）
    /// </summary>
    public long ClockSkew { get; set; }
}