using GameFrameX.Web.Api.ApiRouting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameFrameX.Core.Tests.Swagger;

/// <summary>
/// C270：Swagger schema 契约与序列化层 long→string 契约对齐的行为锁定。
/// </summary>
public class LongIdentifierStringSchemaFilterTests
{
    private static OpenApiSchema ApplyFilter(IDictionary<string, IOpenApiSchema> properties)
    {
        var schema = new OpenApiSchema { Properties = new Dictionary<string, IOpenApiSchema>(properties), };
        new LongIdentifierStringSchemaFilter().Apply(schema, new SchemaFilterContext(typeof(object), schemaGenerator: null!, schemaRepository: new SchemaRepository()));
        return schema;
    }

    [Fact]
    public void Apply_ConvertsInt64IdentifierPropertiesToString()
    {
        var schema = ApplyFilter(new Dictionary<string, IOpenApiSchema>
        {
            ["id"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64", },
            ["tenantId"] = new OpenApiSchema { Type = JsonSchemaType.Integer | JsonSchemaType.Null, Format = "int64", },
        });

        Assert.Equal(JsonSchemaType.String, schema.Properties["id"].Type);
        Assert.Null(schema.Properties["id"].Format);
        // 可空标志保留（String | Null），仅数值位替换
        Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Properties["tenantId"].Type);
        Assert.Null(schema.Properties["tenantId"].Format);
    }

    [Fact]
    public void Apply_LeavesNonIdentifierOrNonInt64PropertiesUntouched()
    {
        var schema = ApplyFilter(new Dictionary<string, IOpenApiSchema>
        {
            // 非 Id 结尾的 int64（秒级时间戳）不动
            ["createdTime"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64", },
            // Id 结尾但非 int64（int32 / 本来就是 string 的 DTO）不动
            ["retryCountId"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", },
            ["labelId"] = new OpenApiSchema { Type = JsonSchemaType.String, },
            ["name"] = new OpenApiSchema { Type = JsonSchemaType.String, },
        });

        Assert.Equal(JsonSchemaType.Integer, schema.Properties["createdTime"].Type);
        Assert.Equal("int64", schema.Properties["createdTime"].Format);
        Assert.Equal(JsonSchemaType.Integer, schema.Properties["retryCountId"].Type);
        Assert.Equal(JsonSchemaType.String, schema.Properties["labelId"].Type);
        Assert.Equal(JsonSchemaType.String, schema.Properties["name"].Type);
    }

    [Fact]
    public void Apply_IgnoresSchemaWithoutProperties()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64", };

        // 不抛异常即为通过（空 Properties 分支）
        new LongIdentifierStringSchemaFilter().Apply(schema, new SchemaFilterContext(typeof(object), schemaGenerator: null!, schemaRepository: new SchemaRepository()));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
    }
}
