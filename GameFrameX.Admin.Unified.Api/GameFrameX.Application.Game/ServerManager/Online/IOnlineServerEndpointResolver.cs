namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// Online 管理 API 端点解析器（来源 change C339 / vault:C2 S1.7）。
/// <para>
/// 抽象隔离点：Online 管理 API 的前缀 / 路径形态以 vault S1.7 定稿为最终权威，
/// 定稿若与本首版实现（区服 <c>HttpManageUrl</c> + <c>OnlineAdminApiPrefix</c> + action）有出入，
/// 只调整 <see cref="OnlineServerEndpointResolver"/> 默认实现，调用方（OnlineServerClient 与后续业务服务）无感。
/// </para>
/// </summary>
public interface IOnlineServerEndpointResolver
{
    /// <summary>
    /// 按区服解析 Online 管理 URL：<c>{HttpManageUrl}/{OnlineAdminApiPrefix}/{action}</c>。
    /// </summary>
    /// <param name="area">目标区服（提供根地址与 Online 管理前缀）。</param>
    /// <param name="action">Online 管理 action 名。</param>
    /// <returns>绝对 URL。</returns>
    /// <exception cref="InvalidOperationException">根地址缺失/非法，或 Online 管理前缀为空（同 Gm/Control 守卫：空表示该区服未开放 Online 管理）。</exception>
    string Resolve(TenantGameAreaEntity area, string action);
}
