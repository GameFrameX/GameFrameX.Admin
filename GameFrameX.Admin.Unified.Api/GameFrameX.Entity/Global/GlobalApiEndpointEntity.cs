using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

/// <summary>
/// API 端点权限表（U6/C42）。
/// <para>每个 ASP.NET Core MVC 发现的 Admin API 端点一行，即一个按钮级权限点。Id 为 DB 代理主键（稳定身份，不由路径派生）；
/// RoutePath 为规范化路由（可变，仅用于启动对账），<b>不</b>作为授权令牌。授权令牌是按钮 MenuId。</para>
/// </summary>
[SugarTable(null, "API端点权限表")]
[SugarIndex("ux_global_api_endpoint_method_route", nameof(HttpMethod), OrderByType.Asc, nameof(RoutePath), OrderByType.Asc, true)]
[GlobalTable]
[IncrementTable]
public class GlobalApiEndpointEntity : EntityBasic
{
    /// <summary>
    /// HTTP 方法（大写），对账键的一半。
    /// </summary>
    [SugarColumn(ColumnDescription = "HTTP方法", Length = EntityStringLength.Length256)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// 规范化路由，例如 api/systemUser/page（去首尾 /、小写、保证 api/ 前缀）。对账键的另一半。<b>可变，非授权令牌。</b>
    /// </summary>
    [SugarColumn(ColumnDescription = "规范化路由", Length = EntityStringLength.Length512)]
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// 显示名（来自 [ApiDescriptionSettings(Name)] 或方法名），便于管理端展示。
    /// </summary>
    [SugarColumn(ColumnDescription = "显示名", Length = EntityStringLength.Length256)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 控制器/服务类名，用于孤儿诊断。
    /// </summary>
    [SugarColumn(ColumnDescription = "控制器", Length = EntityStringLength.Length512)]
    public string? Controller { get; set; }

    /// <summary>
    /// Action 方法名，用于孤儿诊断。
    /// </summary>
    [SugarColumn(ColumnDescription = "Action方法", Length = EntityStringLength.Length512)]
    public string? Action { get; set; }

    /// <summary>
    /// 是否公开（[AllowAnonymous] 端点）。JwtHandler 据此在查端点前短路放行。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否公开AllowAnonymous", DefaultValue = "false")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// 是否仅需登录态（标注 <c>[AuthenticatedOnly]</c> 特性的端点）。仅需有效 JWT，不要按钮绑定。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否仅需登录态", DefaultValue = "false")]
    public bool IsAuthenticatedOnly { get; set; }

    /// <summary>
    /// 是否孤儿（DB 有行但本次扫描无活端点匹配）。孤儿自动 IsEnabled=false。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否孤儿", DefaultValue = "false")]
    public bool IsOrphan { get; set; }

    /// <summary>
    /// 首次发现时间戳。
    /// </summary>
    [SugarColumn(ColumnDescription = "首次发现时间", DefaultValue = "0")]
    public long FirstSeenTime { get; set; }

    /// <summary>
    /// 最近发现时间戳。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近发现时间", DefaultValue = "0")]
    public long LastSeenTime { get; set; }
}