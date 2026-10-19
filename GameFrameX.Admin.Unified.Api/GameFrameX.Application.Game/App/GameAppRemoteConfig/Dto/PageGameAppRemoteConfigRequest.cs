namespace GameFrameX.Application.Game.App.GameAppRemoteConfig.Dto;

/// <summary>
/// 远程配置管理分页查询输入参数
/// </summary>
public class PageGameAppRemoteConfigRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// 应用作用域筛选。客户端远程配置是应用工作台页面，分页查询必须提供具体应用，值必须大于 0。
    /// </summary>
    public long? AppId { get; set; }

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