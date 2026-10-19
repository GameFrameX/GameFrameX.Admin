namespace GameFrameX.Core.Api;

/// <summary>
/// 标记需要暴露为 Admin API 控制器的服务类。
/// <para>服务路由由 Web.Core 的显式 MVC convention 生成，稳定规则为 <c>api/{serviceWithoutService}/{action}</c>。</para>
/// </summary>
public interface IAdminApiController
{
}