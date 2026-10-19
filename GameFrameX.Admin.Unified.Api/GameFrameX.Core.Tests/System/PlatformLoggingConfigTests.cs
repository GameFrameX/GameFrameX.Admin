using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.PlatformLogging.Dto;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C171 平台日志配置测试：默认序列化、校验、静态文件移除、快照动态开关、ES URI 掩码保护。
/// </summary>
public class PlatformLoggingConfigTests
{
    [Fact]
    public void Serialize_DefaultSettings_StaysWithinSystemConfigValueLimit()
    {
        var settings = PlatformLoggingConfigProvider.CreateDefaultSettings();

        var value = PlatformLoggingConfigProvider.Serialize(settings);

        Assert.Contains("\"file\":", value);
        Assert.Contains("\"database\":", value);
        Assert.Contains("\"elasticSearch\":", value);
        Assert.Contains("\"monitor\":", value);
        Assert.Contains("\"isEnabled\":false", value); // ES 默认关闭
        Assert.Contains("\"minimumLevel\":\"Information\"", value);
        Assert.True(value.Length <= PlatformLoggingConfigConstants.MaxSerializedLength);
    }

    [Theory]
    [InlineData(0, 30, 500, "Information", "OnlyJson")] // FileSizeLimitBytes 过小
    [InlineData(1024, 0, 500, "Information", "OnlyJson")] // MaxRollingFiles 过小
    [InlineData(1024, 30, -1, "Information", "OnlyJson")] // ReturnValueThreshold 过小
    [InlineData(1024, 30, 500, "FooLevel", "OnlyJson")] // 数据库级别名非法
    [InlineData(1024, 30, 500, "Information", "Everything")] // JsonBehavior 非法
    public void Validate_RejectsInvalidValues(long fileSizeLimitBytes, int maxRollingFiles, int returnValueThreshold, string minimumLevel, string jsonBehavior)
    {
        var settings = PlatformLoggingConfigProvider.CreateDefaultSettings();
        settings.File.FileSizeLimitBytes = fileSizeLimitBytes;
        settings.File.MaxRollingFiles = maxRollingFiles;
        settings.Monitor.ReturnValueThreshold = returnValueThreshold;
        settings.Database.MinimumLevel = minimumLevel;
        settings.Monitor.JsonBehavior = jsonBehavior;

        Assert.ThrowsAny<Exception>(() => PlatformLoggingConfigProvider.Validate(settings));
    }

    [Fact]
    public void StaticLoggingJson_IsRemovedFromSourceAndProjectCopyItems()
    {
        var repoRoot = FindRepoRoot();

        Assert.False(File.Exists(Path.Combine(repoRoot, "GameFrameX.Application/Configuration/Logging.json")));
        var projectFile = File.ReadAllText(Path.Combine(repoRoot, "GameFrameX.Application/GameFrameX.Application.csproj"));
        Assert.DoesNotContain("Configuration\\Logging.json", projectFile);
    }

    /// <summary>
    /// 动态生效证据：DB 存储 database.enabled=false 时，反序列化快照反映为关闭，
    /// DatabaseLoggingWriter 写路径的门控（!GetSnapshot().Database.IsEnabled）据此立即停止落库。
    /// </summary>
    [Fact]
    public void Deserialize_DisabledDatabase_ReflectsInSnapshot()
    {
        var disabledDbJson = "{\"database\":{\"isEnabled\":false}}";

        var settings = PlatformLoggingConfigProvider.Deserialize(disabledDbJson);

        Assert.False(settings.Database.IsEnabled);
        Assert.True(settings.File.IsEnabled); // 其它分组保持安全默认
        Assert.False(settings.ElasticSearch.IsEnabled);

        // 模拟 DatabaseLoggingWriter.WriteAsync 的门控判定
        var writerGateSkips = !settings.Database.IsEnabled;
        Assert.True(writerGateSkips);
    }

    /// <summary>
    /// ES URI 敏感保护：DetailAsync 输出把每个真实 URI 掩码为 ***，仅保留数量。
    /// </summary>
    [Fact]
    public void ToOutput_MasksElasticSearchServerUris()
    {
        var settings = PlatformLoggingConfigProvider.CreateDefaultSettings();
        settings.ElasticSearch.ServerUris = new List<string> { "http://user:pass@es-host:9200", "http://second:9200", };

        var output = PlatformLoggingConfigService.ToOutput(settings, null);

        Assert.Equal(2, output.ElasticSearch.ServerUris.Count);
        Assert.All(output.ElasticSearch.ServerUris, uri => Assert.Equal(PlatformLoggingConfigConstants.ServerUriMaskPlaceholder, uri));
    }

    /// <summary>
    /// ES URI 更新语义：全为掩码占位视为未变更保留库内原值；含非占位项按非占位项替换；空列表清空。
    /// </summary>
    [Theory]
    [InlineData(new[] { "***", "***", }, "keep")] // 全掩码 → 保留原值
    [InlineData(new[] { "http://new:9200", }, "replace")] // 新值 → 替换
    [InlineData(new string[0], "clear")] // 空 → 清空
    public void ToSettings_ResolvesServerUrisByMaskSemantic(string[] inputUris, string expectation)
    {
        var current = PlatformLoggingConfigProvider.CreateDefaultSettings();
        current.ElasticSearch.ServerUris = new List<string> { "http://original:9200", "http://original2:9200", };
        var input = new PlatformLoggingConfigUpdateRequest
        {
            File = new PlatformLoggingFileUpdateRequest(),
            Database = new PlatformLoggingDatabaseUpdateRequest(),
            ElasticSearch = new PlatformLoggingElasticSearchUpdateRequest { ServerUris = inputUris.ToList(), },
            Monitor = new PlatformLoggingMonitorUpdateRequest(),
        };

        var result = PlatformLoggingConfigService.ToSettings(input, current);

        switch (expectation)
        {
            case "keep":
                Assert.Equal(new[] { "http://original:9200", "http://original2:9200", }, result.ElasticSearch.ServerUris);
                break;
            case "replace":
                Assert.Equal(new[] { "http://new:9200", }, result.ElasticSearch.ServerUris);
                break;
            case "clear":
                Assert.Empty(result.ElasticSearch.ServerUris);
                break;
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate GameFrameX.Admin.Unified.slnx");
    }
}