namespace GameFrameX.Core.Const;

/// <summary>
/// 缓存相关常量
/// </summary>
public sealed class CacheConst
{
    /// <summary>
    /// 用户缓存
    /// </summary>
    public const string KeyUser = "cache:user:";

    /// <summary>
    /// 用户已授权菜单Id集合缓存（来自 TenantRoleMenuEntity，含目录/菜单/按钮，Id 化）
    /// </summary>
    public const string KeyUserMenu = "cache:user_menu:";

    /// <summary>
    /// 用户权限缓存（按钮集合）
    /// </summary>
    public const string KeyUserButton = "cache:user_button:";

    /// <summary>
    /// 用户机构缓存
    /// </summary>
    public const string KeyUserOrganization = "cache:user_org:";

    /// <summary>
    /// 角色最大数据范围缓存
    /// </summary>
    public const string KeyRoleMaxDataScope = "cache:role_max_data_scope:";

    /// <summary>
    /// 在线用户缓存
    /// </summary>
    public const string KeyUserOnline = "cache:user_online:";

    /// <summary>
    /// 图形验证码缓存
    /// </summary>
    public const string KeyVerCode = "cache:ver_code:";

    /// <summary>
    /// 租户缓存
    /// </summary>
    public const string KeyTenant = "cache:tenant";

    /// <summary>
    /// 常量下拉框
    /// </summary>
    public const string KeyConst = "cache:const:";

    /// <summary>
    /// 所有缓存关键字集合
    /// </summary>
    public const string KeyAll = "cache:keys";

    /// <summary>
    /// SqlSugar二级缓存
    /// </summary>
    public const string SqlSugar = "cache:sqlsugar:";

    /// <summary>
    /// 开放接口身份缓存
    /// </summary>
    public const string KeyOpenAccess = "cache:open_access:";

    /// <summary>
    /// 开放接口身份随机数缓存
    /// </summary>
    public const string KeyOpenAccessNonce = "cache:open_access_nonce:";

    /// <summary>
    /// API 端点→绑定按钮 MenuId 集合缓存（U6/C42，全局单份，按端点 Id 索引）。
    /// </summary>
    public const string KeyEndpointButtons = "cache:endpoint_buttons:";

    /// <summary>
    /// (HttpMethod,RoutePath)→ApiEndpoint 行缓存（U6/C42，JwtHandler 请求期查端点身份用）。
    /// </summary>
    public const string KeyEndpointByRoute = "cache:endpoint_by_route";
}