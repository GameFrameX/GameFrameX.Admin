namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 错误码段登记条目，构成 <see cref="ErrorSegmentRegistry"/> 可执行分段表的一行。
/// </summary>
public sealed class ErrorSegment
{
    public ErrorSegment(int segment, ErrorDomain domain, string module, string? enumTypeName, bool pendingImplementation, string description)
    {
        Segment = segment;
        Domain = domain;
        Module = module;
        EnumTypeName = enumTypeName;
        PendingImplementation = pendingImplementation;
        Description = description;
    }

    /// <summary>
    /// 段号 = 8 位错误码的前 3 位（域首位 + 2 位模块号）；0 表示信封保留段。
    /// </summary>
    public int Segment { get; }

    /// <summary>
    /// 所属大域。
    /// </summary>
    public ErrorDomain Domain { get; }

    /// <summary>
    /// 模块登记名（如 System / Auth / Account / SystemUser）。
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// 对应模块错误码 enum 类型名；null 表示本段不挂枚举（信封保留 / 预留段）。
    /// </summary>
    public string? EnumTypeName { get; }

    /// <summary>
    /// 段已登记但枚举成员由后续 change 平移（Admin 业务域 3xx-5xx 当前未建 enum，见 C262 T3）。
    /// </summary>
    public bool PendingImplementation { get; }

    /// <summary>
    /// 段语义描述。
    /// </summary>
    public string Description { get; }
}