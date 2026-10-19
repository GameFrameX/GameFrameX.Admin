using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.ServerManager;

/// <summary>
/// 区服游戏服 Server HTTP 接口 URL 构造器（来源 change C150 §3.3 / C175 后端拆分）。
/// <para>所有 Admin / Client API 调游戏服 HTTP 的代码必须经此构造器，显式声明 <see cref="GameServerEndpointKindEnum"/> 能力类型，不得散落硬编码 <c>/game/api</c>。</para>
/// <para>规则：</para>
/// <list type="number">
/// <item><description><see cref="TenantGameAreaEntity.HttpManageUrl"/> 作为根地址，必须非空且为合法 http/https 绝对 URI，否则抛 <see cref="InvalidOperationException"/>。</description></item>
/// <item><description>按 <c>kind</c> 选择区服对应前缀；对应前缀为空时直接抛 <see cref="InvalidOperationException"/>，拒绝拼出根地址调用（空表示该区服未开放对应能力；C150 §3.2：空表示 Admin 不构造 GM/控制 URL）。</description></item>
/// <item><description>OnlineAdmin 前缀（默认 <c>online/admin</c>）空时同上抛异常拒绝构造（C339 / vault:C2 S1.7：空表示该区服未开放 Online 管理）。</description></item>
/// <item><description>前缀去除首尾 <c>/</c>，action 去除开头 <c>/</c>，拼接为 <c>{root}/{prefix}/{action}</c>。</description></item>
/// <item><description>构造器不判断用户是否把多个前缀填成相同字符串（运维配置自由，代码只保证能力类型语义分离）。</description></item>
/// </list>
/// </summary>
public static class GameServerEndpointUrlBuilder
{
    /// <summary>
    /// 默认前缀。仅允许出现在字段默认值、测试期望与 DTO 归一化中，不得散落到业务拼接。
    /// </summary>
    public const string DefaultApiPrefix = "game/api";

    /// <summary>
    /// 按 <c>kind</c> 构造区服游戏服 HTTP 调用 URL。
    /// </summary>
    /// <param name="area">目标区服，提供 <see cref="TenantGameAreaEntity.HttpManageUrl"/> 根地址与各能力前缀。</param>
    /// <param name="kind">接口能力类型；决定使用哪个前缀字段。</param>
    /// <param name="action">Server HTTP action 名（snake_case，如 <c>req_player_send_item</c>）。</param>
    /// <returns>形如 <c>{root}/{prefix}/{action}</c> 的绝对 URL。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="area"/> 为 null。</exception>
    /// <exception cref="InvalidOperationException">根地址缺失/非法，或对应能力前缀为空。</exception>
    public static string Build(TenantGameAreaEntity area, GameServerEndpointKindEnum kind, string action)
    {
        if (area == null)
        {
            throw new ArgumentNullException(nameof(area));
        }

        if (string.IsNullOrWhiteSpace(area.HttpManageUrl))
        {
            throw new InvalidOperationException("区服游戏服 HTTP 根地址（HttpManageUrl）未配置");
        }

        var root = area.HttpManageUrl.Trim();
        if (!Uri.TryCreate(root, UriKind.Absolute, out var rootUri)
            || (rootUri.Scheme != Uri.UriSchemeHttp && rootUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("区服游戏服 HTTP 根地址（HttpManageUrl）不是合法的 http/https 绝对地址");
        }

        var prefix = ResolvePrefix(area, kind);

        var normalizedRoot = root.TrimEnd('/');
        var normalizedPrefix = prefix.Trim('/');
        var normalizedAction = string.IsNullOrWhiteSpace(action) ? string.Empty : action.TrimStart('/');

        return normalizedPrefix.Length == 0
                   ? normalizedRoot + "/" + normalizedAction
                   : normalizedRoot + "/" + normalizedPrefix + "/" + normalizedAction;
    }

    /// <summary>
    /// 解析 <c>kind</c> 对应前缀；任一能力前缀为空即抛 <see cref="InvalidOperationException"/>，拒绝构造调用 URL。
    /// </summary>
    private static string ResolvePrefix(TenantGameAreaEntity area, GameServerEndpointKindEnum kind)
    {
        switch (kind)
        {
            case GameServerEndpointKindEnum.ClientDispatch:
            {
                if (string.IsNullOrWhiteSpace(area.ClientDispatchApiPrefix))
                {
                    throw new InvalidOperationException("区服未配置 ClientDispatch 能力前缀（ClientDispatchApiPrefix），拒绝构造 ClientDispatch 调用 URL");
                }

                return area.ClientDispatchApiPrefix;
            }

            case GameServerEndpointKindEnum.Fulfillment:
            {
                if (string.IsNullOrWhiteSpace(area.FulfillmentApiPrefix))
                {
                    throw new InvalidOperationException("区服未配置 Fulfillment 能力前缀（FulfillmentApiPrefix），拒绝构造 Fulfillment 调用 URL");
                }

                return area.FulfillmentApiPrefix;
            }

            case GameServerEndpointKindEnum.Gm:
            {
                if (string.IsNullOrWhiteSpace(area.GmApiPrefix))
                {
                    throw new InvalidOperationException("区服未配置 GM 能力前缀（GmApiPrefix），拒绝构造 GM 调用 URL");
                }

                return area.GmApiPrefix;
            }

            case GameServerEndpointKindEnum.Control:
            {
                if (string.IsNullOrWhiteSpace(area.ControlApiPrefix))
                {
                    throw new InvalidOperationException("区服未配置管理控制能力前缀（ControlApiPrefix），拒绝构造控制调用 URL");
                }

                return area.ControlApiPrefix;
            }

            case GameServerEndpointKindEnum.OnlineAdmin:
            {
                if (string.IsNullOrWhiteSpace(area.OnlineAdminApiPrefix))
                {
                    throw new InvalidOperationException("区服未配置 Online 平台管理能力前缀（OnlineAdminApiPrefix），拒绝构造 Online 管理调用 URL");
                }

                return area.OnlineAdminApiPrefix;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "未知的游戏服接口能力类型");
            }
        }
    }
}