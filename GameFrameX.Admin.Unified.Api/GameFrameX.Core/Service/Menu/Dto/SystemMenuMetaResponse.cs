namespace GameFrameX.Core.Service.Menu.Dto;

/// <summary>
/// 菜单Meta配置
/// </summary>
public class SystemMenuMetaResponse
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 标题国际化Key
    /// </summary>
    public string TitleCode { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 是否内嵌
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链链接
    /// </summary>
    public string IsLink { get; set; } = null!;

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public bool IsAffix { get; set; }

    /// <summary>
    /// 菜单上下文：tenantShell / appWorkspace。
    /// <para>tenantShell 是默认菜单（平台域页面同在此上下文，侧边栏保持完整菜单）；appWorkspace 只在当前应用工作台中展示。</para>
    /// </summary>
    public string MenuContext { get; set; } = null!;

    /// <summary>
    /// 菜单应用作用域：platform / tenant / app。
    /// <para>platform 只允许 SuperAdmin 平台能力；tenant 只在租户 Shell 下可见；app 必须绑定具体 AppId > 0。</para>
    /// </summary>
    public string AppScope { get; set; } = null!;
}