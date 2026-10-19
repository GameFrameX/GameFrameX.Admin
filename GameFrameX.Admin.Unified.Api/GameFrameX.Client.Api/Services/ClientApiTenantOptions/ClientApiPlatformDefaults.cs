namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// Client API 平台默认策略常量（SuperAdmin 视角的非秘密默认值与安全上限）。
/// <para>仅覆盖白名单内的非秘密默认策略：访问令牌有效期、Game 登录票据有效期、首次签发自动建 key 开关。</para>
/// <para>Game 登录票据有效期以固定到期时间表达（TicketExpireTime）；0 表示永不过期，非 0 值超出平台上限时按平台默认值 / 上限归一化。</para>
/// <para>签名密钥、邮件凭据、广告 key 等秘密材料不在此处，必须按租户从数据库读取。</para>
/// </summary>
public static class ClientApiPlatformDefaults
{
    /// <summary>
    /// 访问令牌有效期默认值（分钟）。1440 分钟 = 24 小时。
    /// </summary>
    public const int ClientApiTokenLifetimeMinutesDefault = 1440;

    /// <summary>
    /// 访问令牌有效期上限（分钟）。10080 分钟 = 7 天，防止租户配置过长有效期。
    /// </summary>
    public const int ClientApiTokenLifetimeMinutesMax = 10080;

    /// <summary>
    /// Game 登录票据默认有效期（秒）。60 秒保持短票据默认安全水位。
    /// </summary>
    public const int GameLoginTicketLifetimeSecondsDefault = 60;

    /// <summary>
    /// Game 登录票据最大有效期（秒）。2592000 秒 = 30 天。
    /// </summary>
    public const int GameLoginTicketLifetimeSecondsMax = 2592000;

    /// <summary>
    /// 首次签发自动创建租户应用级 RSA key 的默认行为。
    /// </summary>
    public const bool GameLoginTicketKeyAutoCreateDefault = true;

    /// <summary>
    /// Game 登录票据固定签发者。
    /// </summary>
    public const string GameLoginTicketIssuer = "gameframex-client-api";

    /// <summary>
    /// Game 登录票据固定受众。
    /// </summary>
    public const string GameLoginTicketAudience = "game-server";

    /// <summary>
    /// 将访问令牌有效期钳制到平台上限内的安全值。
    /// </summary>
    /// <param name="configured">租户配置值；&lt;=0 表示未配置，使用默认值。</param>
    /// <returns>钳制后的有效期分钟数，至少 1 分钟。</returns>
    public static int ClampTokenLifetimeMinutes(int configured)
    {
        if (configured <= 0)
        {
            return ClientApiTokenLifetimeMinutesDefault;
        }

        return Math.Min(Math.Max(configured, 1), ClientApiTokenLifetimeMinutesMax);
    }

    /// <summary>
    /// 将 Game 登录票据到期时间归一化到平台允许范围内。
    /// </summary>
    /// <param name="configuredExpireTime">租户配置的 Unix 秒到期时间；0 表示永不过期，正数小于等于当前时间表示已过期。</param>
    /// <param name="now">当前 Unix 秒。</param>
    /// <returns>归一化后的 Unix 秒到期时间。</returns>
    public static long NormalizeGameLoginTicketExpireTime(long configuredExpireTime, long now)
    {
        if (configuredExpireTime == 0)
        {
            return 0;
        }

        var defaultExpireTime = now + GameLoginTicketLifetimeSecondsDefault;
        if (configuredExpireTime < 0 || configuredExpireTime <= now)
        {
            return defaultExpireTime;
        }

        var maxExpireTime = now + GameLoginTicketLifetimeSecondsMax;
        return Math.Min(configuredExpireTime, maxExpireTime);
    }
}