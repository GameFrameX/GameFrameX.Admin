namespace GameFrameX.Entity.Enum;

/// <summary>
/// 渠道类型
/// </summary>
[Description("渠道类型")]
[AdminEnum]
public enum ChannelTypeEnum
{
    /// <summary>
    /// 官方渠道
    /// </summary>
    [Description("官方")] Official = 1,

    /// <summary>
    /// 应用商店渠道
    /// </summary>
    [Description("商店")] Store = 2,

    /// <summary>
    /// 广告投放渠道
    /// </summary>
    [Description("广告")] Ad = 3,

    /// <summary>
    /// 自定义渠道
    /// </summary>
    [Description("自定义")] Custom = 4,
}