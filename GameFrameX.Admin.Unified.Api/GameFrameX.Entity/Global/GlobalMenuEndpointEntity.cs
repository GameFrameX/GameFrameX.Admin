using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 菜单按钮 ↔ API 端点 绑定表（U6/C42）。“菜单侧把按钮和接口绑定”落在此表。多对多，OR 语义。
/// </summary>
[SugarTable(null, "菜单按钮接口绑定表")]
[SugarIndex("ux_global_menu_endpoint_menu_endpoint", nameof(MenuId), OrderByType.Asc, nameof(EndpointId), OrderByType.Asc, true)]
[GlobalTable]
[IncrementTable]
public class GlobalMenuEndpointEntity : EntityBasic
{
    /// <summary>
    /// 菜单按钮 Id（global_menu.Id，Type=Button 行）。
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单按钮Id", DefaultValue = "0")]
    public long MenuId { get; set; }

    /// <summary>
    /// API 端点 Id（api_endpoint.Id）。
    /// </summary>
    [SugarColumn(ColumnDescription = "API端点Id", DefaultValue = "0")]
    public long EndpointId { get; set; }

    /// <summary>
    /// 是否启动扫描器自动挂的隐藏按钮绑定；管理员显式绑定后自动行可被清理。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否自动绑定", DefaultValue = "false")]
    public bool IsAuto { get; set; }
}