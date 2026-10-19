namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 绑定行快照（来自 <c>global_menu_endpoint</c>）。
/// </summary>
public sealed class BindingSnapshot
{
    /// <summary>菜单按钮 Id。</summary>
    public long MenuId { get; set; }

    /// <summary>端点 Id。</summary>
    public long EndpointId { get; set; }

    /// <summary>是否启动扫描器自动挂的隐藏按钮绑定。</summary>
    public bool IsAuto { get; set; }
}