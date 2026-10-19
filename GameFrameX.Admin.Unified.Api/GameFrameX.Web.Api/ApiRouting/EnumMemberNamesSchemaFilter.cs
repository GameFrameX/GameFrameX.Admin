using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// Swashbuckle schema 过滤器：为整数枚举 schema 导出成员名与成员说明扩展（x-enum-varnames / x-enum-descriptions）。
/// </summary>
/// <remarks>
/// Swashbuckle 默认只输出 <c>enum: [0,1,2]</c> 数值，不带成员名；swagger-typescript-api 拿不到成员名时
/// 只能退化为 Value1/Value2 之类占位成员，前端无法按语义引用（历史上因此出现手写枚举副本绕行）。
/// 本过滤器按 schema.Enum 的数值顺序补齐 CLR 枚举成员名（<c>x-enum-varnames</c>，swagger-typescript-api
/// 生成命名枚举成员的约定扩展）与 <c>[Description]</c> 说明（<c>x-enum-descriptions</c>，生成成员注释）。
/// 仅影响 OpenAPI 文档元数据，不改运行时序列化；非枚举与空枚举 schema 不动。
/// </remarks>
public sealed class EnumMemberNamesSchemaFilter : ISchemaFilter
{
    private const string EnumVarNamesExtension = "x-enum-varnames";
    private const string EnumDescriptionsExtension = "x-enum-descriptions";

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Enum == null || schema.Enum.Count == 0 || context.Type is not { IsEnum: true })
        {
            return;
        }

        // 按数值建成员索引（别名成员同值取先声明者，与 XmlDocs 的 member id 顺序一致）
        var membersByValue = new Dictionary<long, MemberInfo>();
        foreach (var value in Enum.GetValuesAsUnderlyingType(context.Type))
        {
            var numeric = Convert.ToInt64(value);
            if (!membersByValue.ContainsKey(numeric))
            {
                var field = context.Type.GetField(Enum.GetName(context.Type, value!)!);
                if (field != null)
                {
                    membersByValue[numeric] = field;
                }
            }
        }

        if (membersByValue.Count == 0)
        {
            return;
        }

        var names = new JsonArray();
        var descriptions = new JsonArray();
        foreach (var enumValue in schema.Enum)
        {
            if (enumValue is not JsonValue jsonValue || !jsonValue.TryGetValue<long>(out var numeric))
            {
                // 非整数枚举（字符串枚举等）不在本过滤器职责内
                return;
            }

            if (!membersByValue.TryGetValue(numeric, out var field))
            {
                // schema 里出现 CLR 枚举没有的数值（理论不可达）：整组扩展不产出，避免 names 与 enum 错位
                return;
            }

            names.Add(field.Name);
            var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description;
            descriptions.Add(string.IsNullOrWhiteSpace(description) ? field.Name : description!);
        }

        // v2 里部分 schema（引用壳/内联生成路径）Extensions 可能为 null，且接口属性只读——只在具体类型上兜底
        if (schema.Extensions == null && schema is OpenApiSchema mutableSchema)
        {
            mutableSchema.Extensions = new Dictionary<string, IOpenApiExtension>();
        }

        if (schema.Extensions == null)
        {
            return;
        }

        schema.Extensions.Remove(EnumVarNamesExtension);
        schema.Extensions.Remove(EnumDescriptionsExtension);
        schema.Extensions.Add(EnumVarNamesExtension, new JsonNodeExtension(names));
        schema.Extensions.Add(EnumDescriptionsExtension, new JsonNodeExtension(descriptions));
    }
}
