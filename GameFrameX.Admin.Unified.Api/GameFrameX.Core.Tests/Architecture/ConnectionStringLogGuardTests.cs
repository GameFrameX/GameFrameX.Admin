using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C317（GFX-726）：连接串日志泄漏守护。
// change：gfx-doc/dev/tenant/changes/C317-gfx-726-tenant-db-isolation-hardening/change.md
// SqlSugarClientConfig 曾把含密码的主库连接配置 JsonSerializer.Serialize 后 Console.WriteLine 进日志，
// 本守护防止同类回潮：SqlSugarClientConfig.cs 不得出现任何控制台打印与连接配置序列化。
public sealed class ConnectionStringLogGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();
    private static readonly string ConfigSourcePath = "GameFrameX.Core.SqlSugar/SqlSugarClientConfig.cs";

    private static readonly Regex ConsoleWritePattern = new(@"Console\.(Write|WriteLine)\s*\(", RegexOptions.Compiled);
    private static readonly Regex SerializePattern = new(@"JsonSerializer\.Serialize\s*\(\s*dbConfig", RegexOptions.Compiled);

    [Fact]
    public void SqlSugarClientConfig_DoesNotPrintOrSerializeConnectionConfig()
    {
        var source = File.ReadAllText(Path.Combine(RepositoryRoot, ConfigSourcePath));

        Assert.False(ConsoleWritePattern.IsMatch(source), "SqlSugarClientConfig 不得向控制台打印（历史上曾借此泄漏含密码连接串）。");
        Assert.False(SerializePattern.IsMatch(source), "SqlSugarClientConfig 不得序列化连接配置对象。");
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
