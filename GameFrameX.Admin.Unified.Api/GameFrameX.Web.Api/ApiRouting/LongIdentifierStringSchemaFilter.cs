using GameFrameX.Core.Extension.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// Swashbuckle schema 过滤器：让 OpenAPI 契约与 ProjectJsonSerializerOptions 的 long→string 运行时契约一致（C270）。
/// </summary>
/// <remarks>
/// 序列化层对"属性名以 Id 结尾的 long"启用 <c>NumberHandling.WriteAsString</c>（响应输出字符串，雪花 Id 防 JS 超 2^53 丢精度），
/// 但 Swashbuckle 不感知 NumberHandling，schema 仍声明 integer/int64，前端生成 number 类型，诱导 <c>Number(id)</c> 回转
/// 导致精度损坏。本过滤器把命中属性的 schema 同步改为 string；名字判定与序列化层同源
/// （<see cref="ProjectJsonSerializerOptions.IsLongIdentifierName"/>），杜绝两份规则漂移。
/// schema 属性名 casing（camelCase/PascalCase）不影响判定：忽略大小写。
/// 非 Id 结尾的 int64（如秒级时间戳，2^53 内安全）与非 int64 属性不动。
/// </remarks>
public sealed class LongIdentifierStringSchemaFilter : ISchemaFilter
{
    private const string Int64Format = "int64";

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || schema.Properties.Count == 0)
        {
            return;
        }

        foreach (var property in schema.Properties)
        {
            if (IsInt64(property.Value) && ProjectJsonSerializerOptions.IsLongIdentifierName(property.Key))
            {
                // IOpenApiSchema 接口的 Type/Format 只读；标量属性为内联 OpenApiSchema 实例（复杂类型才走 $ref 引用）
                if (property.Value is not OpenApiSchema mutable)
                {
                    continue;
                }

                // 保留可空组合标志（JsonSchemaType.String | JsonSchemaType.Null），仅替换数值类型位
                mutable.Type = JsonSchemaType.String | (mutable.Type!.Value & JsonSchemaType.Null);
                mutable.Format = null;
            }
        }
    }

    /// <summary>
    /// int64 判定需兼容可空组合（JsonSchemaType.Integer | JsonSchemaType.Null）。
    /// </summary>
    private static bool IsInt64(IOpenApiSchema schema)
    {
        return schema.Type.HasValue
               && (schema.Type.Value & JsonSchemaType.Integer) == JsonSchemaType.Integer
               && string.Equals(schema.Format, Int64Format, StringComparison.OrdinalIgnoreCase);
    }
}
