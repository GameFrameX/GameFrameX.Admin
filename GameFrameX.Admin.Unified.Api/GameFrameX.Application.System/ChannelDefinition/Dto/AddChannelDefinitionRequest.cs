using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.ChannelDefinition.Dto;

/// <summary>
/// 渠道增加输入参数
/// </summary>
public class AddChannelDefinitionRequest : BaseIsDefaultNamedAddRequest
{
    /// <summary>
    /// 渠道编码
    /// </summary>
    [Required(ErrorMessage = "渠道编码不能为空")]
    [MaxLength(EntityStringLength.Length256, ErrorMessage = "渠道编码长度不能超过64")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 渠道作用域
    /// </summary>
    public ChannelScopeEnum Scope { get; set; } = ChannelScopeEnum.Tenant;

    /// <summary>
    /// 适用平台编码
    /// </summary>
    [MaxLength(EntityStringLength.Length256, ErrorMessage = "平台编码长度不能超过64")]
    public string PlatformCode { get; set; } = "all";

    /// <summary>
    /// 渠道类型
    /// </summary>
    public ChannelTypeEnum ChannelType { get; set; } = ChannelTypeEnum.Custom;

    /// <summary>
    /// 系统渠道是否锁定
    /// </summary>
    public bool IsSystemLocked { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; } = true;
}