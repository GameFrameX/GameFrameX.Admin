namespace GameFrameX.Entity.Enum;

/// <summary>
/// 渠道作用域
/// </summary>
[Description("渠道作用域")]
[AdminEnum]
public enum ChannelScopeEnum
{
    /// <summary>
    /// 系统标准渠道
    /// </summary>
    [Description("系统")] System = 1,

    /// <summary>
    /// 租户自定义渠道
    /// </summary>
    [Description("租户")] Tenant = 2,
}