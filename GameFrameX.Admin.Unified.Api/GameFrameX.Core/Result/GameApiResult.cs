using System.Globalization;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Result;

/// <summary>
/// 三 host（Client / Hub / Game）共享的统一错误出口包装（C264，ADR 2026-08-20 统一错误码体系）。
/// </summary>
/// <remarks>
/// Foundation 信封 <see cref="HttpJsonResultData{T}"/> 的 <c>Error(int, string)</c> 为 NuGet 包签名，无法收口枚举；
/// 本包装以 <see cref="global::System.Enum"/> 为唯一入口，裸 int / 魔法数字无法编译通过，错误码语义由
/// <c>GameFrameX.Core.Enum.Errors</c> 模块枚举全局唯一承载。
/// 成功路径（code=0）不经本包装，直接走 Foundation <c>Success</c>（信封契约）。
/// </remarks>
public static class GameApiResult
{
    /// <summary>
    /// 以模块错误码枚举创建失败信封（错误码值取枚举 8 位数字码，Convert.ToInt32 出站）。
    /// </summary>
    /// <typeparam name="T">信封 data 载荷类型（与 action 返回类型一致）。</typeparam>
    /// <param name="code">模块错误码枚举成员（见 <see cref="GameFrameX.Core.Enum.Errors.ErrorSegmentRegistry"/>）。</param>
    /// <param name="message">返回给调用方的错误消息。</param>
    public static HttpJsonResultData<T> Error<T>(global::System.Enum code, string message)
    {
        return HttpJsonResultData<T>.Error(Convert.ToInt32(code, CultureInfo.InvariantCulture), message);
    }
}