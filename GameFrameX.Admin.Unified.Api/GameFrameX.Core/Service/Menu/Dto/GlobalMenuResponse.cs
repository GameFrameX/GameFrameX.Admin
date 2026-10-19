using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Menu.Dto;

/// <summary>
/// 系统菜单输出参数
/// </summary>
public class GlobalMenuResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 业务键（种子菜单稳定身份，主键 Id 由其派生；管理端新增的菜单可为空）
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识（按钮绑定的 API 路由名 routeName，供端点目录引导 global_menu_endpoint 绑定）
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 菜单名称国际化Key
    /// </summary>
    public string? TitleCode { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    public string? OutLink { get; set; }

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
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 是否仅超管可见（种子元数据，DB 驱动授权读取）
    /// </summary>
    public bool SuperAdminOnly { get; set; }

    /// <summary>
    /// 租户默认角色同步标志（TenantMenuSyncRole 位枚举整数值）
    /// </summary>
    public int SyncRoles { get; set; }

    /// <summary>
    /// 超管专属菜单下允许租户只读访问
    /// </summary>
    public bool AllowTenantWhenSuperAdminOnly { get; set; }

    /// <summary>
    /// 应用作用域（Tenant 租户默认 / Platform 仅超管 / App 应用工作台）。显式声明菜单归属域，取代路由前缀推断。
    /// </summary>
    public MenuAppScopeEnum AppScope { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    public List<GlobalMenuResponse> Children { get; set; } = new();

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}