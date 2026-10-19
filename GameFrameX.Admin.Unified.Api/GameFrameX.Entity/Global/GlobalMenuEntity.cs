using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 系统菜单表
/// </summary>
[SugarTable(null, "系统菜单表")]
[GlobalTable]
[IncrementTable]
public sealed class GlobalMenuEntity : EntityBasic
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id", DefaultValue = "0")]
    public long Pid { get; set; }

    /// <summary>
    /// 菜单类型（1目录 2菜单 3按钮）
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单类型", DefaultValue = "0")]
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 业务键（种子菜单稳定身份，主键 Id 由其派生；管理端新增的菜单可为空）
    /// </summary>
    [SugarColumn(ColumnDescription = "业务键", Length = EntityStringLength.Length256)]
    public string? Key { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    [SugarColumn(ColumnDescription = "路由名称", Length = EntityStringLength.Length256)]
    public string? Name { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    [SugarColumn(ColumnDescription = "路由地址", Length = EntityStringLength.Length256)]
    public string? Path { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    [SugarColumn(ColumnDescription = "组件路径", Length = EntityStringLength.Length256)]
    public string? Component { get; set; }

    /// <summary>
    /// 重定向
    /// </summary>
    [SugarColumn(ColumnDescription = "重定向", Length = EntityStringLength.Length256)]
    public string? Redirect { get; set; }

    /// <summary>
    /// 权限标识（按钮绑定的 API 路由名 routeName，供端点目录引导 global_menu_endpoint 绑定）
    /// </summary>
    [SugarColumn(ColumnDescription = "权限标识", Length = EntityStringLength.Length256)]
    public string? Permission { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单名称", Length = EntityStringLength.Length256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 菜单名称国际化Key
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单名称国际化Key", Length = EntityStringLength.Length256)]
    public string? TitleCode { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnDescription = "图标", Length = EntityStringLength.Length256)]
    public string? Icon { get; set; }

    /// <summary>
    /// 是否内嵌
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内嵌", DefaultValue = "false")]
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    [SugarColumn(ColumnDescription = "外链链接", Length = EntityStringLength.Length512)]
    public string? OutLink { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [SugarColumn(ColumnDescription = "是否隐藏", DefaultValue = "false")]
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    [SugarColumn(ColumnDescription = "是否缓存", DefaultValue = "false")]
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    /// 是否固定
    /// </summary>
    [SugarColumn(ColumnDescription = "是否固定", DefaultValue = "false")]
    public bool IsAffix { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 是否仅超管可见（种子元数据，DB 驱动授权读取）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否仅超管可见", DefaultValue = "false")]
    public bool SuperAdminOnly { get; set; }

    /// <summary>
    /// 租户默认角色同步标志（TenantMenuSyncRole 位枚举整数值）
    /// </summary>
    [SugarColumn(ColumnDescription = "租户角色同步标志", DefaultValue = "0")]
    public int SyncRoles { get; set; }

    /// <summary>
    /// 超管专属菜单下允许租户只读访问
    /// </summary>
    [SugarColumn(ColumnDescription = "超管专属允许租户只读", DefaultValue = "false")]
    public bool AllowTenantWhenSuperAdminOnly { get; set; }

    /// <summary>
    /// 应用作用域（Tenant 租户默认 / Platform 仅超管 / App 应用工作台）。显式声明菜单归属域，取代路由前缀推断。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", DefaultValue = "0")]
    public MenuAppScopeEnum AppScope { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<GlobalMenuEntity> Children { get; set; } = new();
}