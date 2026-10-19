namespace GameFrameX.Core.Tests.Architecture;

// C341 / GFX-753（vault:C5 S4.9）：Online 匹配观测域零持久化守护测试。
// Admin 定位为只读观测方：观测数据是运行态快照，Service 层禁止 Repository 注入、禁止 SqlSugar 客户端持有、
// 禁止 Entity 引用（不落队列/Ticket 镜像表或缓存）。文本级源扫描 Application.Game/Online/Match/ 全部 .cs；
// 双断言：目录非空（防目录改名后扫描扑空静默绿）+ 禁用模式零命中。
// root 定位沿用 AdminEndpointGoldenSnapshotTests.GetRepositoryRoot 模式（向上找 slnx）。
public sealed class OnlineObservatoryNoPersistenceTests
{
    private static readonly string MatchSourceDirectory = Path.Combine(
        GetRepositoryRoot(),
        Path.Combine("GameFrameX.Application.Game", "Online", "Match"));

    // 禁用模式：仓储注入 / SqlSugar 客户端 / 表特性 / Entity 命名空间引用（含 global using 形态的整行排除判定）
    private static readonly string[] ForbiddenPatterns =
    {
        "SqlSugarRepository<",
        "ISqlSugarClient",
        "SqlSugarClient",
        "[SugarTable]",
        "using GameFrameX.Entity",
    };

    [Fact]
    public void MatchObservatoryDirectory_ExistsAndHasSources()
    {
        Assert.True(Directory.Exists(MatchSourceDirectory), "Application.Game/Online/Match/ 目录不存在（被改名或删除须回 change 评审）");

        var sourceFiles = Directory.EnumerateFiles(MatchSourceDirectory, "*.cs", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(sourceFiles);
    }

    [Fact]
    public void MatchObservatorySources_HaveNoPersistenceReferences()
    {
        var offenders = new List<string>();
        foreach (var path in Directory.EnumerateFiles(MatchSourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var content = File.ReadAllText(path);
            foreach (var pattern in ForbiddenPatterns)
            {
                if (content.Contains(pattern, StringComparison.Ordinal))
                {
                    offenders.Add(Path.GetRelativePath(GetRepositoryRoot(), path) + " 含禁用模式 \"" + pattern + "\"");
                }
            }
        }

        Assert.Empty(offenders);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find GameFrameX.Admin.Unified.slnx.");
    }
}
