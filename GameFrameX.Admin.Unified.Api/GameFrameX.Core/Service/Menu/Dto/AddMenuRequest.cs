using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Service.Menu.Dto;

/// <summary>
/// 新增/编辑菜单输入参数。
/// 显式声明前端表单可填字段，不再继承 <see cref="GlobalMenuEntity"/>，避免种子安全元数据（SuperAdminOnly、SyncRoles、AllowTenantWhenSuperAdminOnly、Key）与子节点导航属性被前端批量赋值。
/// </summary>
public class AddMenuRequest
{
    /// <summary>
    /// 主键Id（新增为 0，编辑由前端回填）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 业务键（按钮类型用于唯一性校验；管理端新增可为空）
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 菜单名称
    /// </summary>
    [Required(ErrorMessage = "菜单名称不能为空")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 菜单名称国际化Key
    /// </summary>
    public string TitleCode { get; set; } = null!;

    /// <summary>
    /// 路由名称
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
    /// 重定向
    /// </summary>
    public string Redirect { get; set; } = null!;

    /// <summary>
    /// 权限标识
    /// </summary>
    public string Permission { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 外链链接
    /// </summary>
    public string OutLink { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    /// 是否固定
    /// </summary>
    public bool IsAffix { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}