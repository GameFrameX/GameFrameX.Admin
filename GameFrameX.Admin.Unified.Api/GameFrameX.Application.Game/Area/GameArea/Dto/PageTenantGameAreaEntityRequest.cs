using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Area.GameArea.Dto;

/// <summary>
/// 游戏区服分页查询输入参数
/// </summary>
public class PageTenantGameAreaEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 游戏应用Id（可选过滤）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 是否是默认
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 是否推荐
    /// </summary>
    public bool? IsRecommend { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public GameLanguage? Language { get; set; }
}