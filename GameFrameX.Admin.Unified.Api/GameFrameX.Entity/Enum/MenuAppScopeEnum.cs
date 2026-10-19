namespace GameFrameX.Entity.Enum;

/// <summary>
/// 菜单应用作用域枚举：声明菜单归属的可见域，是 global_menu.app_scope 列的显式数据来源。
/// <para>Tenant=0 为枚举默认值，保证未标注的存量菜单安全落入租户 Shell；App 域菜单必须绑定具体 AppId &gt; 0。</para>
/// </summary>
[Description("菜单应用作用域枚举")]
[AdminEnum]
public enum MenuAppScopeEnum
{
    /// <summary>
    /// 租户 Shell（默认）：仅在租户工作台下可见
    /// </summary>
    [Description("租户")] Tenant = 0,

    /// <summary>
    /// 平台：仅 SuperAdmin 平台能力
    /// </summary>
    [Description("平台")] Platform = 1,

    /// <summary>
    /// 应用工作台：必须绑定具体 AppId &gt; 0
    /// </summary>
    [Description("应用")] App = 2,
}