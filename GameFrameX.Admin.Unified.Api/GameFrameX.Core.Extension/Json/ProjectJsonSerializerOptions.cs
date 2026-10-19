using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace GameFrameX.Core.Extension.Json;

/// <summary>
/// Admin Host 统一 JSON 选项。
/// </summary>
public static class ProjectJsonSerializerOptions
{
    /// <summary>
    /// 创建已应用 Admin JSON 契约的选项实例。
    /// </summary>
    public static JsonSerializerOptions CreateDefault()
    {
        var options = new JsonSerializerOptions();
        Configure(options);
        return options;
    }

    /// <summary>
    /// 配置保持既有 Admin JSON 契约的 System.Text.Json 选项。
    /// </summary>
    public static void Configure(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.PropertyNamingPolicy = null;
        options.DictionaryKeyPolicy = null;
        options.PropertyNameCaseInsensitive = true;
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        ApplyLongIdentifierStringContract(options);
        options.Converters.Add(new LongListJsonConverter());
        options.Converters.Add(new LocalDateTimeJsonConverter());
    }

    /// <summary>
    /// 仅应用"Id 结尾 long 字符串化"契约（写出为字符串、可从字符串读回），不动其他选项。
    /// 供 Hub/Client host 在保持自身 JSON 选项（如 PascalCase）的前提下复用 Admin 同一规则（C270）。
    /// </summary>
    /// <remarks>整体替换 TypeInfoResolver；调用方若已有自定义 resolver 需自行合并（当前无此场景）。</remarks>
    public static void ApplyLongIdentifierStringContract(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.TypeInfoResolver = CreateTypeInfoResolver();
    }

    private static IJsonTypeInfoResolver CreateTypeInfoResolver()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(static typeInfo =>
        {
            foreach (var property in typeInfo.Properties)
            {
                if (IsIdentifierProperty(property))
                {
                    property.NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString;
                }
            }
        });

        return resolver;
    }

    private static bool IsIdentifierProperty(JsonPropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return type == typeof(long) && IsLongIdentifierName(property.Name);
    }

    /// <summary>
    /// 标识属性名判定：以 "Id" 结尾（忽略大小写）或乐观锁版本号 RowVersion。序列化契约与 Swagger schema 契约
    /// （GameFrameX.Web.Core 的 LongIdentifierStringSchemaFilter）共用此判定，防止两份规则漂移。
    /// </summary>
    public static bool IsLongIdentifierName(string propertyName)
    {
        return propertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
               // RowVersion 由 SqlSugar ExecuteCommandWithOptLockAsync 生成雪花 long（同样超 2^53）：
               // 不字符串化会被 JS 舍入，前端写回比对必然失配（乐观锁恒 409）。
               || string.Equals(propertyName, "RowVersion", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <see cref="List{T}"/> 元素字符串化（写出字符串、宽容读回数字/字符串）。
    /// <para>Id 结尾属性契约只覆盖对象属性；ownMenuList / roleMenuIdList 等雪花 id 列表接口
    /// 裸数字输出会被 JS JSON.parse 舍入（超出 2^53），前端与字符串 id 匹配必然失配。</para>
    /// </summary>
    private sealed class LongListJsonConverter : JsonConverter<List<long>>
    {
        public override List<long> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<long>();
            }

            var list = new List<long>();
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"期望 long 数组，实际 {reader.TokenType}");
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(reader.TokenType == JsonTokenType.String
                             ? long.Parse(reader.GetString()!, CultureInfo.InvariantCulture)
                             : reader.GetInt64());
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<long> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteStringValue(item.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteEndArray();
        }
    }

    private sealed class LocalDateTimeJsonConverter : JsonConverter<DateTime>
    {
        private const string Format = "yyyy-MM-dd HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            {
                return parsed;
            }

            return DateTime.Parse(value!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var localValue = value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
            writer.WriteStringValue(localValue.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}