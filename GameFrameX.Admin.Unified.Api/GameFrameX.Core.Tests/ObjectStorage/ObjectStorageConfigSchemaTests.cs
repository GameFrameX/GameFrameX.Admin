using System.Reflection;
using GameFrameX.ObjectStorage.Abstract;
using GameFrameX.ObjectStorage.AliyunOss;
using GameFrameX.ObjectStorage.Local;
using GameFrameX.ObjectStorage.QiniuKodo;
using GameFrameX.ObjectStorage.TencentCos;

namespace GameFrameX.Core.Tests.ObjectStorage;

/// <summary>
/// 对象存储配置契约层防漂移测试：Registry / Schema / Normalizer / Parser。
/// 对应 change.md §5 验证标准「Registry 测试 / 配置测试」可离线验证部分。
/// 不依赖 DB、历史框架 运行时与云凭证。
/// </summary>
public class ObjectStorageConfigSchemaTests
{
#pragma warning disable CS8625 // 测试意图：TryGet(null)/Normalize(...,null oldJson) 验证未知 Provider 与缺旧配置的降级路径
    private readonly ObjectStorageConfigSchemaProvider _provider = new();

    public static IEnumerable<object[]> ProviderCases()
    {
        yield return new object[] { "local", typeof(LocalObjectStorageOptionSetting), };
        yield return new object[] { "aliyun", typeof(AliyunOssOptionSetting), };
        yield return new object[] { "tencent-cos", typeof(TencentCosOptionSetting), };
        yield return new object[] { "qiniu", typeof(QiniuKodoOptionSetting), };
    }

    // ---------- Registry 防漂移 ----------

    [Fact]
    public void Registry_RegistersAllFourProviders()
    {
        var names = ObjectStorageProviderRegistry.List().Select(b => b.Provider).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(new[] { "aliyun", "local", "qiniu", "tencent-cos", }, names);
    }

    [Theory]
    [MemberData(nameof(ProviderCases))]
    public void Registry_BindsOptionType(string provider, Type optionType)
    {
        Assert.True(ObjectStorageProviderRegistry.TryGet(provider, out var binding));
        Assert.Equal(optionType, binding.OptionType);
    }

    [Fact]
    public void Registry_TryGetIsCaseInsensitive()
    {
        Assert.True(ObjectStorageProviderRegistry.TryGet("ALIYUN", out var upper));
        Assert.Equal("aliyun", upper.Provider);
        Assert.True(ObjectStorageProviderRegistry.TryGet("Tencent-COS", out _));
    }

    [Fact]
    public void Registry_TryGetReturnsFalseForUnknownAndEmpty()
    {
        Assert.False(ObjectStorageProviderRegistry.TryGet("aws-s3", out var binding));
        Assert.Null(binding);
        Assert.False(ObjectStorageProviderRegistry.TryGet("", out _));
        Assert.False(ObjectStorageProviderRegistry.TryGet(null, out _));
    }

    // ---------- Schema ----------

    [Theory]
    [MemberData(nameof(ProviderCases))]
    public void Schema_FieldsMatchOptionWritableProperties(string provider, Type optionType)
    {
        var schema = _provider.GetSchema(provider);
        Assert.NotNull(schema);

        var expected = optionType
                       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                       .Select(p => p.Name)
                       .OrderBy(n => n, StringComparer.Ordinal)
                       .ToList();
        var actual = schema.Fields.Select(f => f.Key).OrderBy(n => n, StringComparer.Ordinal).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Schema_LocalHasNoConfigurableFields()
    {
        var schema = _provider.GetSchema("local");
        Assert.NotNull(schema);
        Assert.Empty(schema.Fields);
    }

    [Fact]
    public void Schema_MarksCloudSecretKeysSensitive()
    {
        Assert.True(Field(_provider.GetSchema("aliyun"), "AccessKeySecret").Sensitive);
        Assert.True(Field(_provider.GetSchema("tencent-cos"), "SecretKey").Sensitive);
        Assert.True(Field(_provider.GetSchema("qiniu"), "SecretKey").Sensitive);
    }

    [Fact]
    public void Schema_NonSecretFieldsAreNotSensitive()
    {
        Assert.False(Field(_provider.GetSchema("aliyun"), "Endpoint").Sensitive);
        Assert.False(Field(_provider.GetSchema("tencent-cos"), "Bucket").Sensitive);
        Assert.False(Field(_provider.GetSchema("qiniu"), "AccessKey").Sensitive);
    }

    [Fact]
    public void Schema_ResolveTypeMapsSecretAndText()
    {
        Assert.Equal("secret", Field(_provider.GetSchema("aliyun"), "AccessKeySecret").Type);
        Assert.Equal("text", Field(_provider.GetSchema("aliyun"), "Endpoint").Type);
        Assert.Equal("text", Field(_provider.GetSchema("tencent-cos"), "Region").Type);
    }

    [Fact]
    public void Schema_RequiredFlagsMatchAttribute()
    {
        Assert.True(Field(_provider.GetSchema("aliyun"), "Endpoint").Required);
        Assert.True(Field(_provider.GetSchema("aliyun"), "BucketName").Required);
        Assert.False(Field(_provider.GetSchema("aliyun"), "PublicBaseUrl").Required);
        Assert.True(Field(_provider.GetSchema("qiniu"), "PublicBaseUrl").Required);
    }

    [Fact]
    public void Schema_GetSchemaReturnsNullForUnknown()
    {
        Assert.Null(_provider.GetSchema("aws-s3"));
    }

    [Fact]
    public void Schema_GetSchemaIsCaseInsensitive()
    {
        Assert.NotNull(_provider.GetSchema("QINIU"));
        Assert.NotNull(_provider.GetSchema("Local"));
    }

    [Fact]
    public void Providers_ReturnsAllFourSortedByValue()
    {
        var providers = _provider.GetProviders();
        var values = providers.Select(p => p.Value).ToList();
        Assert.Equal(4, values.Count);
        Assert.Equal(new[] { "aliyun", "local", "qiniu", "tencent-cos", }, values);
    }

    // ---------- Normalizer：白名单 / 必填 / ****** 保留 / 字段脱敏 ----------

    [Fact]
    public void Normalize_DropsUnknownKeys()
    {
        var input = "{\"Endpoint\":\"oss-cn-hangzhou.aliyuncs.com\",\"BucketName\":\"b\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"sec\",\"evil\":\"hacked\"}";

        var normalized = ObjectStorageConfigNormalizer.Normalize(_provider, "aliyun", input, null);

        Assert.Contains("\"AccessKeySecret\":\"sec\"", normalized);
        Assert.DoesNotContain("evil", normalized);
    }

    [Fact]
    public void Normalize_RejectsMissingRequired()
    {
        Assert.Throws<ArgumentException>(() => ObjectStorageConfigNormalizer.Normalize(_provider, "aliyun", "{}", null));
    }

    [Fact]
    public void Normalize_PreservesSecretFromOldWhenMaskSubmitted()
    {
        var oldJson = "{\"Endpoint\":\"oss\",\"BucketName\":\"b\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"real-secret\"}";
        var input = "{\"Endpoint\":\"oss\",\"BucketName\":\"b\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"******\"}";

        var normalized = ObjectStorageConfigNormalizer.Normalize(_provider, "aliyun", input, oldJson);

        Assert.Contains("\"AccessKeySecret\":\"real-secret\"", normalized);
        Assert.DoesNotContain("\"AccessKeySecret\":\"******\"", normalized);
    }

    [Fact]
    public void Normalize_RejectsMaskedRequiredSecretWhenNoOld()
    {
        // 提交 ****** 但系统无旧值可保留 → 必填校验失败
        var input = "{\"Endpoint\":\"oss\",\"BucketName\":\"b\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"******\"}";
        Assert.Throws<ArgumentException>(() => ObjectStorageConfigNormalizer.Normalize(_provider, "aliyun", input, null));
    }

    [Fact]
    public void Normalize_KeepsPlainNonSensitiveValues()
    {
        var input = "{\"Endpoint\":\"oss-cn-hangzhou.aliyuncs.com\",\"BucketName\":\"mybucket\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"plain-secret\",\"PublicBaseUrl\":\"https://cdn.example.com\"}";

        var normalized = ObjectStorageConfigNormalizer.Normalize(_provider, "aliyun", input, null);

        Assert.Contains("\"Endpoint\":\"oss-cn-hangzhou.aliyuncs.com\"", normalized);
        Assert.Contains("\"BucketName\":\"mybucket\"", normalized);
        Assert.Contains("\"AccessKeySecret\":\"plain-secret\"", normalized);
        Assert.Contains("\"PublicBaseUrl\":\"https://cdn.example.com\"", normalized);
    }

    [Fact]
    public void Normalize_RejectsUnknownProvider()
    {
        Assert.Throws<ArgumentException>(() => ObjectStorageConfigNormalizer.Normalize(_provider, "aws-s3", "{}", null));
    }

    [Fact]
    public void Mask_ReturnsPlaceholder()
    {
        Assert.Equal(ObjectStorageConfigMask.Placeholder, ObjectStorageConfigNormalizer.Mask());
    }

    [Fact]
    public void MaskFields_ReplacesOnlySensitiveNonEmpty()
    {
        var json = "{\"Endpoint\":\"oss\",\"BucketName\":\"b\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"real-secret\"}";

        var masked = ObjectStorageConfigNormalizer.MaskFields(_provider, "aliyun", json);

        Assert.Contains("\"AccessKeySecret\":\"******\"", masked);
        Assert.Contains("\"Endpoint\":\"oss\"", masked);
        Assert.Contains("\"BucketName\":\"b\"", masked);
        Assert.DoesNotContain("\"AccessKeySecret\":\"real-secret\"", masked);
    }

    [Fact]
    public void MaskFields_KeepsNonSensitiveValuesUntouched()
    {
        var json = "{\"Region\":\"ap-guangzhou\",\"Bucket\":\"t-123\",\"SecretId\":\"sid\",\"SecretKey\":\"sk\"}";

        var masked = ObjectStorageConfigNormalizer.MaskFields(_provider, "tencent-cos", json);

        Assert.Contains("\"SecretKey\":\"******\"", masked);
        Assert.Contains("\"Region\":\"ap-guangzhou\"", masked);
        Assert.Contains("\"SecretId\":\"sid\"", masked);
    }

    [Fact]
    public void MaskFields_HandlesEmptyAndWhitespaceJson()
    {
        Assert.Equal("", ObjectStorageConfigNormalizer.MaskFields(_provider, "aliyun", ""));
        Assert.Equal("   ", ObjectStorageConfigNormalizer.MaskFields(_provider, "aliyun", "   "));
    }

    // ---------- Parser：Bucket / PublicBaseUrl 反射 ----------

    [Fact]
    public void Parser_GetBucketReadsBucketNameForAliyun()
    {
        var option = ObjectStorageConfigParser.ParseOptions("aliyun", "{\"BucketName\":\"ali-bucket\"}");
        Assert.Equal("ali-bucket", ObjectStorageConfigParser.GetBucket(option));
    }

    [Fact]
    public void Parser_GetBucketReadsBucketForTencent()
    {
        var option = ObjectStorageConfigParser.ParseOptions("tencent-cos", "{\"Bucket\":\"tencent-123\"}");
        Assert.Equal("tencent-123", ObjectStorageConfigParser.GetBucket(option));
    }

    [Fact]
    public void Parser_GetPublicBaseUrlReadsPublicBaseUrl()
    {
        var option = ObjectStorageConfigParser.ParseOptions("qiniu", "{\"PublicBaseUrl\":\"https://cdn.example.com\"}");
        Assert.Equal("https://cdn.example.com", ObjectStorageConfigParser.GetPublicBaseUrl(option));
    }

    [Fact]
    public void Parser_ParseOptionsIsCaseInsensitive()
    {
        var option = (AliyunOssOptionSetting)ObjectStorageConfigParser.ParseOptions("aliyun", "{\"bucketname\":\"lower-bucket\"}");
        Assert.Equal("lower-bucket", option.BucketName);
    }

    [Fact]
    public void Parser_ParseOptionsReturnsDefaultForEmptyJson()
    {
        var option = ObjectStorageConfigParser.ParseOptions("local", "");
        Assert.NotNull(option);
        Assert.IsType<LocalObjectStorageOptionSetting>(option);
    }

    [Fact]
    public void Parser_ParseOptionsThrowsForUnknownProvider()
    {
        Assert.Throws<ArgumentException>(() => ObjectStorageConfigParser.ParseOptions("aws-s3", "{}"));
    }

    private static ObjectStorageConfigField Field(ObjectStorageConfigSchema schema, string key)
    {
        return schema.Fields.First(f => f.Key == key);
    }
#pragma warning restore CS8625
}