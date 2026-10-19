namespace GameFrameX.Application.Game.App.GameAppRemoteConfigKey.Dto;

/// <summary>
/// 远程配置内容键定义分页查询输入参数
/// </summary>
public class PageGameAppRemoteConfigKeyRequest : BasePageRequest
{
    /// <summary>
    /// JSON key 名称
    /// </summary>
    public string? KeyName { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 值类型：string、bool、long
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}