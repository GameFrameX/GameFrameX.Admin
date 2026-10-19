using GameFrameX.Core.Extension.Json;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Provider;

/// <summary>
/// Admin 统一响应契约工厂。
/// </summary>
public static class AdminResultFactory
{
    /// <summary>
    /// 创建 Admin 统一成功响应对象（code=0，业务数据装 data）。
    /// </summary>
    public static HttpJsonResultData<object> Create(object? data = null)
    {
        return HttpJsonResultData<object>.Success(data!);
    }

    /// <summary>
    /// 创建 Admin 统一失败响应对象。
    /// </summary>
    /// <param name="statusCode">HTTP 状态语义响应码（400/401/403/404/409/500），与 Foundation 常量取值对齐。</param>
    /// <param name="errors">错误信息；非字符串对象（如 ModelState 字典）序列化为 JSON 文本。</param>
    /// <param name="code">统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core.Enum.Errors）；缺省回退为 statusCode。</param>
    public static HttpJsonResultData<object> CreateError(int statusCode, object? errors = null, int? code = null)
    {
        return new HttpJsonResultData<object>
        {
            Code = code ?? statusCode,
            Message = errors is null or string ? errors + "" : ProjectJsonSerializer.Default.Serialize(errors),
        };
    }
}