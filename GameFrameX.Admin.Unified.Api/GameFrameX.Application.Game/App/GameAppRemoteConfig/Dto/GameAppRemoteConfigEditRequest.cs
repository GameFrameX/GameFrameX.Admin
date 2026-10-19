namespace GameFrameX.Application.Game.App.GameAppRemoteConfig.Dto;

/// <summary>
/// 远程配置管理新增/更新共有输入参数
/// </summary>
public class GameAppRemoteConfigEditRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域。管理端客户端远程配置必须从应用工作台维护，值必须大于 0。
    /// </summary>
    public virtual long? AppId { get; set; }

    /// <summary>
    /// Channel
    /// </summary>
    public virtual string? Channel { get; set; }

    /// <summary>
    /// Content
    /// </summary>
    public virtual string? Content { get; set; }

    /// <summary>
    /// AotCodeList
    /// </summary>
    public virtual string? AotCodeList { get; set; }

    /// <summary>
    /// CheckResourceUrl
    /// </summary>
    public virtual string? CheckResourceUrl { get; set; }

    /// <summary>
    /// CheckAppVersionUrl
    /// </summary>
    public virtual string? CheckAppVersionUrl { get; set; }

    /// <summary>
    /// Platform
    /// </summary>
    public virtual string? Platform { get; set; }

    /// <summary>
    /// AppVersion
    /// </summary>
    public virtual string? AppVersion { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    public virtual string? Package { get; set; }

    /// <summary>
    /// Language
    /// </summary>
    public virtual string? Language { get; set; }

    /// <summary>
    /// 模式内容映射 JSON
    /// </summary>
    public virtual string? ModeContents { get; set; }

    /// <summary>
    /// 当前激活模式编码
    /// </summary>
    public virtual string? ActiveModeCode { get; set; }

    /// <summary>
    /// 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
    /// </summary>
    public virtual int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
    /// </summary>
    public virtual int RolloutPercent { get; set; } = 100;

    /// <summary>
    /// 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。
    /// </summary>
    public virtual string? DeviceWhitelistJson { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public virtual string? Description { get; set; }
}