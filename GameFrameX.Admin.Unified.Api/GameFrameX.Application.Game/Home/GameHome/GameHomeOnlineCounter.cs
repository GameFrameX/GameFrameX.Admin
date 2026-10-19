using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Home.GameHome;

/// <summary>
/// 全服在线人数聚合器（C321 / GFX-730）
/// <para>「启用且配置管理地址的区服，按 Control 能力前缀调用 GetOnlinePlayer 求和」逻辑的唯一实现，
/// 供 <c>TenantGameHomeEntityService</c>（实时读数）与 <c>GameHomeOnlineSampleJob</c>（采样落表）复用，
/// 语义继承 C175：区服不可达 / 未配置 ControlApiPrefix 时按 0 计。</para>
/// </summary>
public static class GameHomeOnlineCounter
{
    /// <summary>
    /// 按指定租户库连接聚合全服在线人数。无可用区服时返回 0。
    /// </summary>
    /// <param name="tenantDb">租户业务库连接（必须已路由到目标租户库）。</param>
    /// <param name="httpClientFactory">HTTP 客户端工厂。</param>
    /// <returns>启用区服在线人数之和。</returns>
    public static async Task<int> GetTotalOnlineAsync(ISqlSugarClient tenantDb, IHttpClientFactory httpClientFactory)
    {
        var areas = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameAreaEntity>()
                                  .Where(u => u.IsEnabled == true && u.HttpManageUrl != null && u.HttpManageUrl != "")
                                  .ToListAsync();

        if (areas == null || areas.Count == 0)
        {
            // ponytail: 首页只需要在线人数摘要；无可用区服时降级为 0。
            return 0;
        }

        var client = httpClientFactory.CreateClient();
        var total = 0;
        foreach (var area in areas)
        {
            total += await GetAreaOnlineAsync(client, area);
        }

        return total;
    }

    /// <summary>
    /// 查询单个区服在线人数。Control 能力前缀为空（构造器抛异常）或调用异常时降级为 0，
    /// 与历史「全局地址缺失 / 游戏服不可达降级 0」语义一致（来源 change C175）。
    /// </summary>
    private static async Task<int> GetAreaOnlineAsync(HttpClient client, Entity.Tenant.Game.TenantGameAreaEntity area)
    {
        string url;
        try
        {
            url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Control, "GetOnlinePlayer");
        }
        catch (InvalidOperationException)
        {
            // 区服未配置 ControlApiPrefix：跳过该区服，按 0 计（运维配 ControlApiPrefix=game/api 即恢复）。
            return 0;
        }

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return int.TryParse(body?.Trim(), out var n) ? n : 0;
        }
        catch (HttpRequestException)
        {
            // ponytail: 游戏服不可达时降级为 0，后续多区服状态页应展示逐服错误。
            return 0;
        }
    }
}
