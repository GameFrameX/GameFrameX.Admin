namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 协议域协议解析错误码（段 202，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum ProtocolErrorCode
{
    /// <summary>
    /// 不是枚举类型
    /// </summary>
    [ErrorText("不是枚举类型", "Not an enum type")]
    NoEnumType = 20200001,
}