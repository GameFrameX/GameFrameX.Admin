using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.ChannelDefinition.Dto;

/// <summary>
/// 渠道分页查询输入参数
/// </summary>
public class PageChannelDefinitionRequest : BasePageRequest
{
    /// <summary>
    /// 渠道编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 渠道作用域
    /// </summary>
    public ChannelScopeEnum? Scope { get; set; }

    /// <summary>
    /// 适用平台编码
    /// </summary>
    public string PlatformCode { get; set; } = string.Empty;

    /// <summary>
    /// 渠道类型
    /// </summary>
    public ChannelTypeEnum? ChannelType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}