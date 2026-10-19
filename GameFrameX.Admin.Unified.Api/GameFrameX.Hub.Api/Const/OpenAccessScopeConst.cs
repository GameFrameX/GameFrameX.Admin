namespace GameFrameX.Hub.Api.Const;

/// <summary>
/// 应用开放访问凭证接口范围（scope）取值常量。
/// <para>scope 值为路由前缀（不含 <c>/api/</c>），由 <c>TenantGameAppOpenAccessAuthMiddleware</c>
/// 按前缀匹配放行；凭证配置入口按自由字符串录入，本常量集只收敛 Hub.Api 已提供的机器端点取值域。</para>
/// </summary>
public static class OpenAccessScopeConst
{
    /// <summary>
    /// 功能开关服务器同步 scope：命中 <c>/api/FeatureSwitchSync</c> 路由前缀（C347）。
    /// </summary>
    public const string FeatureSwitchSync = "FeatureSwitchSync";
}
