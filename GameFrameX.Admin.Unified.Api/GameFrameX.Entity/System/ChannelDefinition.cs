using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.System;

public abstract class ChannelDefinition : EntityBasic, ISelectFilter
{
    /// <summary>
    /// 稳定业务编码
    /// </summary>
    [SugarColumn(ColumnDescription = "稳定业务编码", Length = EntityStringLength.Length256, IsNullable = true)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// 渠道作用域
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道作用域", DefaultValue = "0")]
    public ChannelScopeEnum Scope { get; set; } = ChannelScopeEnum.Tenant;

    /// <summary>
    /// 适用平台编码
    /// </summary>
    [SugarColumn(ColumnDescription = "适用平台编码", Length = EntityStringLength.Length256, IsNullable = true)]
    public string PlatformCode { get; set; } = "all";

    /// <summary>
    /// 渠道类型
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道类型", DefaultValue = "0")]
    public ChannelTypeEnum ChannelType { get; set; } = ChannelTypeEnum.Custom;

    /// <summary>
    /// 系统渠道是否锁定
    /// </summary>
    [SugarColumn(ColumnDescription = "系统渠道是否锁定", DefaultValue = "false")]
    public bool IsSystemLocked { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 是否为默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认")]
    public bool? IsDefault { get; set; }
}