using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C276（GFX-682）守护测试：生产代码禁止调用 SimpleClient.ChangeRepository<T>() 无参重载解析
// SqlSugarRepository<T>。SqlSugar 5.1.4.216 该重载对带参构造仓储必抛 MissingMethodException
// （详见 SqlSugarRepositoryDiResolutionTests 根因锁定），替代路径是
// IServiceProvider.GetRequiredService<SqlSugarRepository<T>>() 走 DI open generic 解析。
// 零例外：C276 已全量替换 8 处调用点，新增调用一律不允许（带 IServiceProvider 参数的 DI 重载如需启用须先更新本测试）。
public sealed class SqlSugarRepositoryChangeRepositoryGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string[] ProductionDirectories =
    {
        "GameFrameX.Core",
        "GameFrameX.Core.SqlSugar",
        "GameFrameX.Core.BaseInputOutput",
        "GameFrameX.Application",
        "GameFrameX.Application.System",
        "GameFrameX.Application.Game",
        "GameFrameX.Web.Api",
        "GameFrameX.Client.Api",
        "GameFrameX.Hub.Api",
        "GameFrameX.Entity",
    };

    [Fact]
    public void ProductionCode_DoesNotUse_ParameterlessChangeRepositoryForSqlSugarRepository()
    {
        var offenders = new List<string>();

        foreach (var directory in ProductionDirectories)
        {
            var path = Path.Combine(RepositoryRoot, directory);
            if (!Directory.Exists(path))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                if (IsBuildOutput(file))
                {
                    continue;
                }

                var source = StripComments(File.ReadAllText(file));
                if (source.Contains("ChangeRepository<SqlSugarRepository<", StringComparison.Ordinal))
                {
                    offenders.Add(Path.GetRelativePath(RepositoryRoot, file).Replace('\\', '/'));
                }
            }
        }

        Assert.True(offenders.Count == 0,
                    "生产代码发现 ChangeRepository<SqlSugarRepository<...> 调用（SqlSugar 5.1.4.216 无参重载必抛 MissingMethodException，GFX-682）。"
                    + "请改用 _serviceProvider.GetRequiredService<SqlSugarRepository<T>>()（先例：GlobalMenuService）。命中文件："
                    + string.Join(", ", offenders));
    }

    private static string StripComments(string source)
    {
        source = Regex.Replace(source, @"//[^\n]*", string.Empty);
        source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return source;
    }

    private static bool IsBuildOutput(string file)
    {
        return file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
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