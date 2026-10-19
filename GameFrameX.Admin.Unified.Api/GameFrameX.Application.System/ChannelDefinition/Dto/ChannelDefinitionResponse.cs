using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.ChannelDefinition.Dto;

/// <summary>
/// 渠道输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 GameFrameX.Entity.System.ChannelDefinition 及其基类链保持一致，
// 供 .Select&lt;ChannelDefinitionResponse&gt;() 自动投影与 Mapster 按名映射。
// ChannelDefinition 为抽象类（不映射物理表，C22 拆分），不满足 IBasePageResponse&lt;T&gt; 的 new() 约束，故保留非泛型 IBaseResponse。
public class ChannelDefinitionResponse : IBaseResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 稳定业务编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 渠道作用域
    /// </summary>
    public ChannelScopeEnum Scope { get; set; } = ChannelScopeEnum.Tenant;

    /// <summary>
    /// 适用平台编码
    /// </summary>
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
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 是否为默认
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}