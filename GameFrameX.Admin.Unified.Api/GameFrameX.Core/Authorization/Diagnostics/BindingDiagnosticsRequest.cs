namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 诊断引擎输入。所有集合允许为空（按空集处理，不抛异常）。
/// </summary>
public sealed class BindingDiagnosticsRequest
{
    /// <summary>活端点集合。</summary>
    public IList<LiveEndpointSnapshot> LiveEndpoints { get; set; } = new List<LiveEndpointSnapshot>();

    /// <summary>DB 端点行集合。</summary>
    public IList<EndpointRowSnapshot> EndpointRows { get; set; } = new List<EndpointRowSnapshot>();

    /// <summary>绑定行集合。</summary>
    public IList<BindingSnapshot> Bindings { get; set; } = new List<BindingSnapshot>();

    /// <summary>菜单/按钮集合。</summary>
    public IList<MenuSnapshot> Menus { get; set; } = new List<MenuSnapshot>();
}