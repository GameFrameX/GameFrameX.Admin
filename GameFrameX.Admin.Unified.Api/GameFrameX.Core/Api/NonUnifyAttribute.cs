namespace GameFrameX.Core.Api;

/// <summary>
/// 标记当前 action 返回原始响应，不经过 Admin 统一响应包装。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class NonUnifyAttribute : Attribute
{
}