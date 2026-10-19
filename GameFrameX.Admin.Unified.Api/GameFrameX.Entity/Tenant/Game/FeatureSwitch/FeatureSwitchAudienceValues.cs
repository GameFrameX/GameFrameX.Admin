namespace GameFrameX.Entity.Tenant.Game.FeatureSwitch;

/// <summary>
/// 开关注册表消费方标记（<see cref="TenantGameAppFeatureSwitchEntity.Audience"/> 列）的取值域。
/// <para>定义在实体层：列默认值、Admin 写入校验与投放解析的行过滤必须共用同一取值域，避免字面量漂移。</para>
/// </summary>
public static class FeatureSwitchAudienceValues
{
    /// <summary>
    /// 仅客户端通道下发
    /// </summary>
    public const string Client = "client";

    /// <summary>
    /// 仅游戏服务器通道下发
    /// </summary>
    public const string Server = "server";

    /// <summary>
    /// 客户端与游戏服务器通道均下发（列默认值）
    /// </summary>
    public const string Both = "both";

    /// <summary>
    /// 判断取值是否属于本取值域。
    /// </summary>
    /// <param name="value">待校验的消费方标记。</param>
    /// <returns>合法返回 true，否则返回 false。</returns>
    public static bool IsValid(string? value)
    {
        return value == Client || value == Server || value == Both;
    }
}
