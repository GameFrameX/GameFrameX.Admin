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
    /// 菜单上下文：console。
    /// </summary>
    public string MenuContext { get; set; } = null!;

}