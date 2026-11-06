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
    /// 业务自定义渠道
    /// </summary>
    [Description("业务")] Business = 2,
}