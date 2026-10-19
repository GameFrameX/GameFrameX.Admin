using GameFrameX.Core.Bootstrap;

namespace GameFrameX.Core.Tests.Architecture;

// C261（GFX-668）：host Program.cs 启动样板收敛守护测试。
// 收敛前 Client/Hub/Game 各持一份逐字重复的启动样板（日志 LogOptions+Loki 标签 / 雪花 / CORS AllowAnyOrigin /
// AddSwaggerService / HealthChecks live+ready 注册与三端点映射 / Hub 内嵌 JwtSettings 手写 TokenValidationParameters）。
// C268 后 Game.Api 已并入 Client.Api，守护范围剩 Client/Hub 两 host。
// 收敛后唯一权威实现在 GameFrameX.Core.Bootstrap；各 host Program.cs 只允许 host 特有装配。
// 双向断言：
//   B1 各 host Program.cs 源码不再出现样板标记（防回潮：新样板副本直接失败）；
//   B2 各 host Program.cs 均已接线共享扩展（防遗漏：收敛后新增 host 忘记接线时失败）；
//   B3 共享扩展落在 Core（全库唯一定义，host 无同名副本回潮）。
public sealed class HostBootstrapContractTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string[] HostProgramFiles =
    {
        "GameFrameX.Client.Api/Program.cs",
        "GameFrameX.Hub.Api/Program.cs",
    };

    // 样板标记：任一出现在三 host Program.cs 即视为内联样板回潮
    private static readonly string[] BootstrapBoilerplateMarkers =
    {
        "AddSwaggerService",
        "AllowAnyOrigin",
        "GrafanaLokiLabels.Add",
        "MapHealthChecks(",
        "TokenValidationParameters",
        "SnowFlakeIdHelper.WorkId",
        "AddHealthChecks()",
        "PropertyNamingPolicy = null",
    };

    [Theory]
    [InlineData("GameFrameX.Client.Api/Program.cs")]
    [InlineData("GameFrameX.Hub.Api/Program.cs")]
    public void HostProgram_NoBootstrapBoilerplateMarkers(string relativeProgramFile)
    {
        var source = ReadHostProgram(relativeProgramFile);

        foreach (var marker in BootstrapBoilerplateMarkers)
        {
            Assert.False(source.Contains(marker, StringComparison.Ordinal),
                         $"{relativeProgramFile} 出现启动样板标记 \"{marker}\"：C261 已将启动样板收敛至 GameFrameX.Core/Bootstrap，" +
                         $"host Program.cs 只保留 host 特有装配，请改用共享扩展而非内联副本。");
        }
    }

    [Theory]
    [InlineData("GameFrameX.Client.Api/Program.cs")]
    [InlineData("GameFrameX.Hub.Api/Program.cs")]
    public void HostProgram_WiredToSharedBootstrap(string relativeProgramFile)
    {
        var source = ReadHostProgram(relativeProgramFile);

        Assert.Contains("UseHostBootstrapLogging", source, StringComparison.Ordinal);
        Assert.Contains("AddHostCors", source, StringComparison.Ordinal);
        Assert.Contains("UseSnowFlake", source, StringComparison.Ordinal);
        Assert.Contains("AddHostControllers", source, StringComparison.Ordinal);
        Assert.Contains("AddHostHealthChecks", source, StringComparison.Ordinal);
        Assert.Contains("IsHostSwaggerEnabled", source, StringComparison.Ordinal);
        Assert.Contains("UseHostSwaggerUi", source, StringComparison.Ordinal);
        Assert.Contains("UseHostCors", source, StringComparison.Ordinal);
        Assert.Contains("MapHostHealthChecks", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HostMvcJson_WritesIdentifierLongsAsString()
    {
        // C270：Hub/Client 的 *Id long 必须挂 Admin 同一字符串化契约，防 JS 侧雪花 Id 超 2^53 丢精度
        var sourcePath = Path.Combine(RepositoryRoot, "GameFrameX.Core", "Bootstrap", "HostBootstrapMvcExtensions.cs");
        Assert.True(File.Exists(sourcePath), "未找到 GameFrameX.Core/Bootstrap/HostBootstrapMvcExtensions.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("ApplyLongIdentifierStringContract", source, StringComparison.Ordinal);
        Assert.Contains("PropertyNamingPolicy = null", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HubProgram_UsesSharedHostJwtBearer_NotInlineJwtSettings()
    {
        var source = ReadHostProgram("GameFrameX.Hub.Api/Program.cs");

        Assert.Contains("AddHostJwtBearer", source, StringComparison.Ordinal);
        // Hub 收敛前内嵌 private sealed class JwtSettings（C261 提升为 Core 的 HostJwtSettings，全库唯一定义）
        Assert.DoesNotContain("class JwtSettings", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HostBootstrapExtensions_SingleDefinition_LivesInCore()
    {
        var bootstrapAssembly = typeof(HostBootstrapSnowflake).Assembly;
        Assert.Equal("GameFrameX.Core", bootstrapAssembly.GetName().Name);

        // 各 host 程序集内不得再出现同名启动扩展类型副本（防 C260 之前的 host 副本模式回潮）
        var hostAssemblies = new[]
        {
            global::System.Reflection.Assembly.Load("GameFrameX.Client.Api"),
            global::System.Reflection.Assembly.Load("GameFrameX.Hub.Api"),
        };
        var bootstrapTypeNames = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(HostBootstrapLoggingExtensions),
            nameof(HostBootstrapCorsExtensions),
            nameof(HostBootstrapSwaggerExtensions),
            nameof(HostBootstrapHealthChecksExtensions),
            nameof(HostBootstrapMvcExtensions),
            nameof(HostBootstrapJwtExtensions),
            nameof(HostBootstrapSnowflake),
            nameof(HostJwtSettings),
        };

        var duplicates = hostAssemblies
                         .SelectMany(a => a.GetTypes())
                         .Where(t => bootstrapTypeNames.Contains(t.Name))
                         .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void WebApiStartup_UsesSharedHealthChecks()
    {
        var startupPath = Path.Combine(RepositoryRoot, "GameFrameX.Web.Api", "Startup.cs");
        Assert.True(File.Exists(startupPath), "未找到 GameFrameX.Web.Api/Startup.cs");
        var source = File.ReadAllText(startupPath);

        // Admin 与三 host 共用注册/端点映射（C261 第 4 份重复同步收敛），须带 allowAnonymous（全局鉴权下探针必须匿名）
        Assert.Contains("AddHostHealthChecks", source, StringComparison.Ordinal);
        Assert.Contains("MapHostHealthChecks(allowAnonymous: true)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("endpoints.MapHealthChecks(", source, StringComparison.Ordinal);
    }

    private static string ReadHostProgram(string relativePath)
    {
        var fullPath = Path.Combine(RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath), "未找到 " + relativePath);
        return File.ReadAllText(fullPath);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }
}