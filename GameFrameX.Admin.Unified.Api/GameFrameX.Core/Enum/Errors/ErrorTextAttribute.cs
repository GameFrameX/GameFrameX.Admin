namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 错误码双语文案特性：挂在模块错误码枚举成员上，错误码数值与中英文文案同处一行声明，
/// 构成 <see cref="Localization.Errors.ErrorLocalizer"/> 的单一事实来源（不存在枚举与外部码表两处事实）。
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ErrorTextAttribute : Attribute
{
    public ErrorTextAttribute(string zhCN, string enUS)
    {
        ZhCN = zhCN;
        EnUS = enUS;
    }

    /// <summary>
    /// 中文文案（zh-CN，默认文化）。
    /// </summary>
    public string ZhCN { get; }

    /// <summary>
    /// 英文文案（en-US）。
    /// </summary>
    public string EnUS { get; }
}