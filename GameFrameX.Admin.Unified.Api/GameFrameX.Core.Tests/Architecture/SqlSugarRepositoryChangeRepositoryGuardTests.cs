using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// 生产仓储必须经依赖注入解析，避免无参 ChangeRepository 绕过作用域和连接上下文约束。
// 替代路径为 IServiceProvider.GetRequiredService<SqlSugarRepository<T>>()。
public sealed class SqlSugarRepositoryChangeRepositoryGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string[] ProductionDirectories =
    {
        "GameFrameX.Core",
        "GameFrameX.Core.SqlSugar",
        "GameFrameX.Core.BaseInputOutput",
        "GameFrameX.Application",
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
                    "生产代码发现 ChangeRepository<SqlSugarRepository<...> 调用（禁止绕过依赖注入和连接上下文约束）。"
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