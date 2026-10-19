namespace GameFrameX.Core.Filter;

/// <summary>
/// MVC 全局过滤器注册入口（Client / Hub 共享，C236 收敛）。
/// <para>三 host 统一契约语义：注册请求/响应日志两件套；Client 特有的
/// <c>PenaltyLoginBlockedExceptionFilter</c>（处罚事实源登录阻断）由 Client.Api 自行追加，不进共享层。</para>
/// </summary>
public static class MvcFilterApp
{
    /// <summary>
    /// 注册全局过滤器（请求日志 + 响应日志）。
    /// </summary>
    /// <param name="filterCollection"></param>
    public static void Init(FilterCollection filterCollection)
    {
        filterCollection.Add<GlobalRequestHandlerFilter>();
        // 全局数据格式统一处理
        filterCollection.Add<GlobalResponseHandlerFilter>();
    }
}