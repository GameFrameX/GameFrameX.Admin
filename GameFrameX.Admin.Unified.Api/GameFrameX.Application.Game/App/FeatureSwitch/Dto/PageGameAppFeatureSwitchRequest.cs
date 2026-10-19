namespace GameFrameX.Application.Game.App.FeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关管理分页查询输入参数
/// </summary>
public class PageGameAppFeatureSwitchRequest : BasePageRequest
{
    /// <summary>
    /// 应用作用域筛选。给正值时只看该应用行；留空或 0 时只看租户默认层行
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 开关键模糊筛选
    /// </summary>
    public string? SwitchKey { get; set; }

    /// <summary>
    /// 启停筛选
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 消费方标记精确筛选：client / server / both
    /// </summary>
    public string? Audience { get; set; }
}
