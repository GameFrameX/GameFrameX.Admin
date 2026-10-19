namespace GameFrameX.Application.Game.App.GameAppRemoteConfigKey.Dto;

/// <summary>
/// 远程配置内容键定义基础输入参数
/// </summary>
public class GameAppRemoteConfigKeyBaseRequest : BaseAddRequest
{
    /// <summary>
    /// JSON key 名称
    /// </summary>
    public virtual string? KeyName { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public virtual string? DisplayName { get; set; }

    /// <summary>
    /// 值类型：string、bool、long
    /// </summary>
    [Required(ErrorMessage = "ValueType不能为空")]
    public string? ValueType { get; set; } = "string";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; } = true;
}