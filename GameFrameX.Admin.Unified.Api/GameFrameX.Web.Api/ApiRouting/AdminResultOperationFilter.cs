using GameFrameX.Core.Api;
using GameFrameX.Foundation.Http.Normalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// Swashbuckle 操作过滤器：让 OpenAPI schema 与 AdminResultFilter 的运行时包装保持一致。
/// </summary>
/// <remarks>
/// controller 方法签名返回业务数据 T，但 <c>AdminResultFilter</c> 在运行时把响应包成
/// <c>HttpJsonResultData&lt;T&gt;</c>（三 host 统一信封）。Swashbuckle 默认只看方法签名，生成的 schema 是 T，
/// 与实际响应脱节，导致前端按 openapi 生成的客户端类型错误（schema 丢了 code/data 等字段）。
/// 本过滤器对每个 operation 的成功响应 schema 同步包一层 HttpJsonResultData 结构；
/// action 已显式返回 <c>HttpJsonResultData&lt;Output&gt;</c> 时透传（schema 由签名天然生成，防双重包装，
/// 与 AdminResultFilter 的 IHttpJsonResult 透传分支对应）；
/// 并为缺少 operationId 的接口补全（path 段 + HttpMethod），与 AdminResultFilter.ShouldSkip
/// 保持一致的跳过规则（[NonUnify]/[NonAction]）。
/// </remarks>
public sealed class AdminResultOperationFilter : IOperationFilter
{
    private const string CodeField = "code";
    private const string MessageField = "message";
    private const string DataField = "data";
    private const string TrackIdField = "trackId";
    private const string ErrorCodeField = "errorCode";
    private const string TypeField = "type";
    private const string TimeField = "time";
    private const string ExtrasField = "extras";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // 即使跳过包装，也要补全 operationId（前端生成方法名依赖它）
        if (ShouldSkip(context))
        {
            EnsureOperationId(operation, context);
            return;
        }

        if (!ReturnsHttpJsonResult(context))
        {
            WrapSuccessResponses(operation);
        }

        EnsureOperationId(operation, context);
    }

    private static bool ShouldSkip(OperationFilterContext context)
    {
        var methodInfo = context.MethodInfo;
        if (methodInfo == null)
        {
            return true;
        }

        if (methodInfo.GetCustomAttributes(true).OfType<NonActionAttribute>().Any())
        {
            return true;
        }

        if (methodInfo.GetCustomAttributes(true).OfType<NonUnifyAttribute>().Any())
        {
            return true;
        }

        var declaringType = methodInfo.DeclaringType;
        if (declaringType != null && declaringType.GetCustomAttributes(true).OfType<NonUnifyAttribute>().Any())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// action 显式返回统一信封（如 Task&lt;HttpJsonResultData&lt;Output&gt;&gt;）时无需再包装 schema。
    /// </summary>
    private static bool ReturnsHttpJsonResult(OperationFilterContext context)
    {
        var returnType = context.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            returnType = returnType.GetGenericArguments()[0];
        }

        return typeof(IHttpJsonResult).IsAssignableFrom(returnType);
    }

    private static void WrapSuccessResponses(OpenApiOperation operation)
    {
        // operation.Responses 的 key 即 HTTP 状态码（"200"/"201"/...），2.0 不再在 response 上暴露 StatusCode。
        if (operation.Responses == null)
        {
            return;
        }

        foreach (var pair in operation.Responses)
        {
            if (!IsSuccessStatus(pair.Key))
            {
                continue;
            }

            if (pair.Value?.Content == null)
            {
                continue;
            }

            foreach (var mediaType in pair.Value.Content.Values)
            {
                mediaType.Schema = WrapWithHttpJsonResult(mediaType.Schema);
            }
        }
    }

    private static bool IsSuccessStatus(string statusCode)
    {
        return !string.IsNullOrEmpty(statusCode)
               && statusCode.Length == 3
               && statusCode[0] == '2';
    }

    private static OpenApiSchema WrapWithHttpJsonResult(IOpenApiSchema? dataSchema)
    {
        // HttpJsonResultData<T> 字段集与 AdminResultProvider 包装产物、三 host wire 一致：
        // code/message/data/time 必有；trackId 因 [JsonIgnore(Condition = Never)] 恒输出（null 保留）；
        // errorCode/type/extras 为 null 时 wire 仍输出 null（Admin host 序列化未启用 WhenWritingNull）。
        // data 字段保留原始 schema（可能是 $ref 或内联），其余字段为固定结构。
        // Microsoft.OpenApi 2.0：Type 用 JsonSchemaType 枚举，可空类型用 Type | JsonSchemaType.Null 组合。
        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                [CodeField] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", Description = "响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）", },
                [MessageField] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "错误或提示信息", },
                [DataField] = dataSchema ?? new OpenApiSchema(),
                [TrackIdField] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "链路追踪标识，未启用追踪时为 null", },
                [ErrorCodeField] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段", },
                [TypeField] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Description = "响应类型（success/warning/error）", },
                [TimeField] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64", Description = "Unix 时间戳（秒）", },
                [ExtrasField] = new OpenApiSchema { Type = JsonSchemaType.Object | JsonSchemaType.Null, },
            },
            Required = new HashSet<string>
            {
                CodeField,
                MessageField,
                DataField,
                TrackIdField,
                ErrorCodeField,
                TypeField,
                TimeField,
                ExtrasField,
            },
        };
    }

    private static void EnsureOperationId(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.IsNullOrWhiteSpace(operation.OperationId))
        {
            return;
        }

        operation.OperationId = BuildOperationId(context);
    }

    /// <summary>
    /// 生成稳定的 operationId：去掉 api 前缀与路由参数段，各段首字母大写拼接，末尾追加 HttpMethod。
    /// 示例：GET api/globalMenu/list -&gt; GlobalMenuListGet；POST api/globalMenu/add -&gt; GlobalMenuAddPost。
    /// 追加 HttpMethod 是为了区分同一资源路径下不同谓词的接口，保证 operationId 全局唯一。
    /// </summary>
    private static string BuildOperationId(OperationFilterContext context)
    {
        var relativePath = context.ApiDescription?.RelativePath ?? string.Empty;
        var httpMethod = context.ApiDescription?.HttpMethod ?? "get";

        var pathPart = relativePath.Split('?')[0];
        var segments = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries)
                               .Where(segment => !IsApiPrefix(segment) && !IsRouteParameter(segment))
                               .Select(ToPascalCase);

        return string.Concat(segments) + ToPascalCase(httpMethod.ToLowerInvariant());
    }

    private static bool IsApiPrefix(string segment)
    {
        return string.Equals(segment, "api", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRouteParameter(string segment)
    {
        return segment.StartsWith("{", StringComparison.Ordinal);
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }
}