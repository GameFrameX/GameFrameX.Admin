using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.ChannelDefinition.Dto;

/// <summary>
/// 可用渠道选择输出
/// </summary>
public class AvailableChannelDefinitionResponse
{
    /// <summary>
    /// 渠道Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 渠道编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 渠道作用域
    /// </summary>
    public ChannelScopeEnum Scope { get; set; }

    /// <summary>
    /// 渠道类型
    /// </summary>
    public ChannelTypeEnum ChannelType { get; set; }

    /// <summary>
    /// 适用平台编码
    /// </summary>
    public string PlatformCode { get; set; } = string.Empty;
}