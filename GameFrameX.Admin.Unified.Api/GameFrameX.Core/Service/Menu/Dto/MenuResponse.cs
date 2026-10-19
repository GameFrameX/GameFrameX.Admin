using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Menu.Dto;

/// <summary>
/// 系统菜单返回结果
/// </summary>
public class MenuResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 业务键（种子菜单稳定身份，前端按 Key 关联）
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（0目录 1菜单 2按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 路由地址
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// 组件路径
    /// </summary>
    public string Component { get; set; } = null!;

    /// <summary>
    /// 权限标识
    /// </summary>
    public string Permission { get; set; } = null!;

    /// <summary>
    /// 重定向
    /// </summary>
    public string Redirect { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 菜单Meta
    /// </summary>
    public SystemMenuMetaResponse Meta { get; set; } = null!;

    /// <summary>
    /// 菜单子项
    /// </summary>
    public List<MenuResponse> Children { get; set; } = null!;
}