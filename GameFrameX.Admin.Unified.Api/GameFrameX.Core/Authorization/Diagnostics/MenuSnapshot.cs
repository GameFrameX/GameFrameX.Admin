namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 菜单/按钮行快照（来自 <c>global_menu</c>）。
/// </summary>
public sealed class MenuSnapshot
{
    /// <summary>菜单/按钮主键 Id。</summary>
    public long Id { get; set; }

    /// <summary>菜单类型（1目录 2菜单 3按钮）。</summary>
    public int Type { get; set; }

    /// <summary>权限标识（按钮绑定的 routeName，可空）。</summary>
    public string? Permission { get; set; }

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; }

    /// <summary>是否软删。</summary>
    public bool IsDeleted { get; set; }
}