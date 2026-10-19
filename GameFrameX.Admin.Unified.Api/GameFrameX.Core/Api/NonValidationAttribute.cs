namespace GameFrameX.Core.Api;

/// <summary>
/// 标记当前接口跳过自动模型验证。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class NonValidationAttribute : Attribute
{
}