namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// Online Runtime 管理 API HTTP 客户端（来源 change C339 / vault:C2 S1.7；typed client 注册见 Web.Api Startup）。
/// <para>
/// 契约：所有调用必须携带 <see cref="OnlineScopeContext"/>（由 <see cref="OnlineScopeContextFactory"/> 构造，
/// TenantId 鉴权强制注入、AppId/ServerId 已过租户归属校验）；响应经 Foundation 信封解包后，
/// Online 业务码非 0 时按协议段映射 <see cref="OnlineErrorCode"/> 镜像码抛 <c>BusinessError</c>（原始协议码随 errors 透出），
/// 调用方拿到的要么是业务数据、要么是异常，不存在「静默失败」路径。
/// </para>
/// </summary>
public interface IOnlineServerClient
{
    /// <summary>
    /// POST 调用 Online 管理 action（作用域三元组并入请求体）。
    /// </summary>
    /// <typeparam name="TResponse">业务响应数据类型。</typeparam>
    /// <param name="action">Online 管理 action 名。</param>
    /// <param name="request">业务请求对象（不含作用域字段，作用域由 scope 注入）。</param>
    /// <param name="scope">作用域上下文（Factory 构造产物）。</param>
    /// <returns>Online 响应 Data 载荷（Code=0 时）。</returns>
    /// <exception cref="BusinessException">Online 业务码非 0（映射 OnlineErrorCode 镜像码）或作用域区服不可达/未配置。</exception>
    Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class;

    /// <summary>
    /// GET 调用 Online 管理 action（作用域三元组作为查询参数）。
    /// </summary>
    /// <typeparam name="TResponse">业务响应数据类型。</typeparam>
    /// <param name="action">Online 管理 action 名。</param>
    /// <param name="scope">作用域上下文（Factory 构造产物）。</param>
    /// <returns>Online 响应 Data 载荷（Code=0 时）。</returns>
    /// <exception cref="BusinessException">Online 业务码非 0（映射 OnlineErrorCode 镜像码）或作用域区服不可达/未配置。</exception>
    Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class;
}
