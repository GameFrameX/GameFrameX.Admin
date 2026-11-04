namespace GameFrameX.Application.ClientDelivery.GameAppRemoteConfig.Dto;

/// <summary>
/// 远程配置管理分页查询输入参数
/// </summary>
public class PageGameAppRemoteConfigRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// Content
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// CheckResourceUrl
    /// </summary>
    public string? CheckResourceUrl { get; set; }

    /// <summary>
    /// CheckAppVersionUrl
    /// </summary>
    public string? CheckAppVersionUrl { get; set; }

    /// <summary>
    /// CreatedName
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// UpdatedName
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}