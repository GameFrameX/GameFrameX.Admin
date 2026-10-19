using System.Reflection;

namespace GameFrameX.Web.Api.ApiRouting;

public static class AdminOpenApiGroups
{
    /// <summary>
    /// 本地 Web.Api 进程的唯一 OpenApi 文档名。收敛业务模块分组后，所有 IAdminApiController 都归入此文档，
    /// 模块分类改由 Swagger Tag（左侧菜单）承担。三个 Api 项目（Web/Hub/Client）各自独立 Swagger，互不聚合。
    /// </summary>
    public const string Web = "web.api";

    /// <summary>
    /// 本地 Web.Api 的 SwaggerDoc 元数据。Title 取宿主进程入口程序集名（与程序集名称保持一致，
    /// 程序集改名后自动跟随）；Swashbuckle 约定 swagger.json 路径为 swagger/{name}/swagger.json。
    /// </summary>
    public static AdminOpenApiGroup WebGroup { get; } = new(
        Web,
        Assembly.GetEntryAssembly()?.GetName().Name ?? Web,
        "swagger/" + Web + "/swagger.json");
}