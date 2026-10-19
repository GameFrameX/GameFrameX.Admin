using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.Core.Tests.ObjectStorage;

/// <summary>
/// 对象存储 Adapter 契约测试：AdapterFactory 解析 + Context 构建 + IObjectStorageAdapter fake seam。
/// 对应 change.md §5 验证标准「Adapter 契约测试：用可控 SDK seam/fake 验证参数和异常语义」。
/// 不发起真实云请求；LocalAdapter 真实 IO 与云连通留集成/浏览器 e2e。
/// </summary>
public class ObjectStorageAdapterContractTests
{
#pragma warning disable CS8625 // 测试意图：pathTemplate=null 模拟未配置路径模板，生产签名 string? 与本测试约定一致
    private readonly IObjectStorageAdapterFactory _factory = new DefaultObjectStorageAdapterFactory();

    [Theory]
    [InlineData("local")]
    [InlineData("aliyun")]
    [InlineData("tencent-cos")]
    [InlineData("qiniu")]
    public void TryResolve_ReturnsAdapterForFourProviders(string provider)
    {
        Assert.True(_factory.TryResolve(provider, out var adapter));
        Assert.NotNull(adapter);
    }

    [Theory]
    [InlineData("LOCAL")]
    [InlineData("Aliyun")]
    [InlineData("TENCENT-COS")]
    [InlineData("Qiniu")]
    public void TryResolve_IsCaseInsensitive(string provider)
    {
        Assert.True(_factory.TryResolve(provider, out var adapter));
        Assert.NotNull(adapter);
    }

    [Theory]
    [InlineData("aws-s3")]
    [InlineData("")]
    [InlineData("unknown")]
    public void TryResolve_ReturnsFalseForUnknown(string provider)
    {
        Assert.False(_factory.TryResolve(provider, out var adapter));
        Assert.Null(adapter);
    }

    [Fact]
    public void Resolve_ReturnsAdapterForKnownProvider()
    {
        var adapter = _factory.Resolve("local");
        Assert.NotNull(adapter);
    }

    [Fact]
    public void Resolve_ThrowsNotSupportedExceptionForUnknown()
    {
        Assert.Throws<NotSupportedException>(() => _factory.Resolve("aws-s3"));
    }

    [Fact]
    public void BuildContext_ParsesBucketForAliyun()
    {
        var configJson = "{\"Endpoint\":\"oss-cn-hangzhou.aliyuncs.com\",\"BucketName\":\"my-bucket\",\"AccessKeyId\":\"kid\",\"AccessKeySecret\":\"sec\"}";

        var context = _factory.BuildContext("aliyun", configJson, "Upload/{yyyy}/{MM}/{dd}");

        Assert.Equal("aliyun", context.Provider);
        Assert.Equal("my-bucket", context.Bucket);
        Assert.Equal("Upload/{yyyy}/{MM}/{dd}", context.PathTemplate);
        Assert.NotNull(context.Options);
    }

    [Fact]
    public void BuildContext_ParsesBucketForTencent()
    {
        var configJson = "{\"Region\":\"ap-guangzhou\",\"Bucket\":\"t-1250000000\",\"SecretId\":\"sid\",\"SecretKey\":\"sk\"}";

        var context = _factory.BuildContext("tencent-cos", configJson, null);

        Assert.Equal("t-1250000000", context.Bucket);
    }

    [Fact]
    public void BuildContext_PropagatesPublicBaseUrlWhenProvided()
    {
        var configJson = "{\"Region\":\"ap-guangzhou\",\"Bucket\":\"t-1\",\"SecretId\":\"sid\",\"SecretKey\":\"sk\",\"PublicBaseUrl\":\"https://cdn.example.com\"}";

        var context = _factory.BuildContext("tencent-cos", configJson, null);

        Assert.Equal("https://cdn.example.com", context.PublicBaseUrl);
    }

    [Fact]
    public void BuildContext_ThrowsForUnknownProvider()
    {
        Assert.Throws<ArgumentException>(() => _factory.BuildContext("aws-s3", "{}", null));
    }

    [Fact]
    public void BuildContext_RawConfigJsonPreservedForDiagnostics()
    {
        var configJson = "{\"Bucket\":\"t-1\",\"SecretId\":\"sid\",\"SecretKey\":\"sk\",\"Region\":\"ap-guangzhou\"}";

        var context = _factory.BuildContext("tencent-cos", configJson, null);

        Assert.Equal(configJson, context.RawConfigJson);
    }

    // ---------- IObjectStorageAdapter fake seam：验证接口契约可被可控实现替换 ----------

    [Fact]
    public async Task FakeAdapter_PutRecordsObjectKeyAndContent()
    {
        var fake = new FakeObjectStorageAdapter();
        var context = _factory.BuildContext("local", "", null);
        var payload = new byte[] { 1, 2, 3, };

        using (var stream = new MemoryStream(payload))
        {
            await fake.PutAsync(context, stream, "Upload/a.bin", "application/octet-stream", default);
        }

        Assert.Equal("Upload/a.bin", fake.LastPutObjectKey);
        Assert.Equal("application/octet-stream", fake.LastPutContentType);
        Assert.Equal(payload.Length, fake.LastPutBytes);
    }

    [Fact]
    public async Task FakeAdapter_OpenReadReturnsWhatWasPut()
    {
        var fake = new FakeObjectStorageAdapter();
        fake.Stored["Upload/a.bin"] = new byte[] { 9, 8, 7, };
        var context = _factory.BuildContext("local", "", null);

        using (var stream = await fake.OpenReadAsync(context, "Upload/a.bin", default))
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                Assert.Equal(new byte[] { 9, 8, 7, }, ms.ToArray());
            }
        }
    }

    [Fact]
    public async Task FakeAdapter_DeleteIsIdempotent()
    {
        var fake = new FakeObjectStorageAdapter();
        var context = _factory.BuildContext("local", "", null);

        await fake.DeleteAsync(context, "missing.bin", default);
        Assert.Equal(1, fake.DeleteCalls);
    }

    [Fact]
    public async Task FakeAdapter_TestConnectionReturnsBool()
    {
        var fake = new FakeObjectStorageAdapter { ConnectionResult = true, };
        var context = _factory.BuildContext("local", "", null);

        Assert.True(await fake.TestConnectionAsync(context, default));
    }

    [Fact]
    public void FakeAdapter_BuildPublicUrlReturnsString()
    {
        var fake = new FakeObjectStorageAdapter();
        var context = _factory.BuildContext("local", "", null);

        var url = fake.BuildPublicUrl(context, "Upload/a.bin");
        Assert.False(string.IsNullOrWhiteSpace(url));
    }

    /// <summary>
    /// 可控 IObjectStorageAdapter 实现：作为测试 seam 验证调用方与 Adapter 之间的契约，
    /// 同时也是"新增 Provider 不依赖中心 switch"的反例佐证——Adapter 只需实现接口即可被替换。
    /// </summary>
    private sealed class FakeObjectStorageAdapter : IObjectStorageAdapter
    {
        public Dictionary<string, byte[]> Stored { get; } = new();
        public string LastPutObjectKey { get; private set; } = null!;
        public string LastPutContentType { get; private set; } = null!;
        public int LastPutBytes { get; private set; }
        public int DeleteCalls { get; private set; }
        public bool ConnectionResult { get; set; } = true;

        public Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
        {
            LastPutObjectKey = objectKey;
            LastPutContentType = contentType;
            using (var ms = new MemoryStream())
            {
                content.CopyTo(ms);
                LastPutBytes = (int)ms.Length;
                Stored[objectKey] = ms.ToArray();
            }

            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
        {
            if (!Stored.TryGetValue(objectKey, out var bytes))
            {
                throw new FileNotFoundException("对象不存在：" + objectKey);
            }

            return Task.FromResult<Stream>(new MemoryStream(bytes));
        }

        public Task DeleteAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
        {
            DeleteCalls++;
            Stored.Remove(objectKey);
            return Task.CompletedTask;
        }

        public Task<bool> TestConnectionAsync(ObjectStorageAdapterContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ConnectionResult);
        }

        public string BuildPublicUrl(ObjectStorageAdapterContext context, string objectKey)
        {
            return "https://fake.example.com/" + (objectKey ?? string.Empty).TrimStart('/');
        }
    }
#pragma warning restore CS8625
}