using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C406 D13 阻断测试（GFX-1232）：租户与 AppId 作用域全面物理清除后，防止残留回潮。
// 依据：parallel-contract.md §1 D12-D13、§7 R2/R3/R4 与 evidence-audit/residual-scan.py 同口径。
//
// 扫描范围：
// 1. GameFrameX.Admin.Unified.Api/**（生产代码、实体、数据库模型、测试源码、slnx/csproj/props）；
// 2. GameFrameX.Admin.Unified.Vue/src/** 与 openapi.json；
// 3. 运行时配置：.env.example、docker-compose*.yml、appsettings*.json、Database.json、launchSettings.json。
//
// 白名单机制：本目录 LegacyScopeResidualWhitelist.tsv，每行 path_glob<TAB>token<TAB>reason；
// 命中必须同时满足路径 glob 与 token 全字匹配（R2-a：按 token 精确匹配，不允许整行或整目录放行）。
// 只允许 D2.5 第三方原生名称、D7 断代说明、R3-a 游戏服协议镜像、D12 旧字段/旧路径负向测试（R2-b）。
//
// R4：除文件内容外，还扫描 git 跟踪文件路径——路径含 tenant 即失败（白名单除外）。
// R2-c：正则先匹配最长 token（GameAppId 先于 AppId），GameApp* 其它产品名不命中本正则。
public sealed class LegacyScopeResidualGuardTests
{
    private static readonly Regex ResidualTokenRegex = new(
        "AsTenant|ITenant|TenantTable|GameAppId|gameAppId|AppId|appId|app_id|tenant_|Tenant|tenant",
        RegexOptions.Compiled);

    [Fact]
    public void InScopeSources_ContainNoUnwhitelistedTenantOrAppIdResiduals()
    {
        var repositoryRoot = GetGitRepositoryRoot();
        var whitelist = LoadWhitelist(GetWhitelistPath(repositoryRoot));
        var violations = new List<string>();

        foreach (var file in EnumerateInScopeFiles(repositoryRoot))
        {
            var relativePath = Path.GetRelativePath(repositoryRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            var lines = File.ReadAllLines(file);
            for (var index = 0; index < lines.Length; index++)
            {
                foreach (Match match in ResidualTokenRegex.Matches(lines[index]))
                {
                    if (!IsWhitelisted(relativePath, match.Value, whitelist))
                    {
                        violations.Add($"{relativePath}:{index + 1}: {match.Value} :: {lines[index].Trim()}");
                    }
                }
            }
        }

        Assert.True(violations.Count == 0, "发现未登记的 Tenant/AppId 残留：\n" + string.Join("\n", violations));
    }

    [Fact]
    public void TrackedFilePaths_ContainNoTenantSegment()
    {
        var repositoryRoot = GetGitRepositoryRoot();
        var offenders = ListTrackedPaths(repositoryRoot)
            .Where(path => path.ToLowerInvariant().Contains("tenant"))
            .ToList();

        Assert.True(offenders.Count == 0, "git 跟踪文件路径仍含 tenant（R4）：\n" + string.Join("\n", offenders));
    }

    [Fact]
    public void WhitelistEntries_HavePathTokenAndReason()
    {
        var entries = LoadWhitelist(GetWhitelistPath(GetGitRepositoryRoot()));
        Assert.NotEmpty(entries);
        Assert.All(entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.PathGlob));
            Assert.False(string.IsNullOrWhiteSpace(entry.TokenPattern));
            Assert.False(string.IsNullOrWhiteSpace(entry.Reason));
        });
    }

    private static IEnumerable<string> EnumerateInScopeFiles(string root)
    {
        var excluded = new[] { ".git", ".codegraph", "bin", "obj", "node_modules", "dist", "coverage" };
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".ts", ".tsx", ".vue", ".js", ".mjs", ".json", ".yml", ".yaml", ".props", ".targets",
            ".csproj", ".slnx",
        };
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                              .Any(part => excluded.Contains(part, StringComparer.Ordinal))
                          && (extensions.Contains(Path.GetExtension(path))
                              || Path.GetFileName(path).StartsWith(".env", StringComparison.Ordinal)))
            .Where(path =>
            {
                var relative = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
                return relative.StartsWith("GameFrameX.Admin.Unified.Api/", StringComparison.Ordinal)
                    || relative.StartsWith("GameFrameX.Admin.Unified.Vue/src/", StringComparison.Ordinal)
                    || relative.Equals("GameFrameX.Admin.Unified.Vue/openapi.json", StringComparison.Ordinal)
                    || relative.Equals(".env.example", StringComparison.Ordinal)
                    || relative.StartsWith("docker-compose", StringComparison.Ordinal)
                    || relative.Contains("appsettings", StringComparison.OrdinalIgnoreCase)
                    || relative.EndsWith("Database.json", StringComparison.Ordinal)
                    || relative.EndsWith("launchSettings.json", StringComparison.Ordinal);
            });
    }

    private static IReadOnlyList<string> ListTrackedPaths(string repositoryRoot)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "ls-files",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        using (var process = Process.Start(startInfo)!)
        {
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Assert.True(process.ExitCode == 0, "git ls-files 执行失败，退出码 " + process.ExitCode);
            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }

    private static IReadOnlyList<WhitelistEntry> LoadWhitelist(string path)
    {
        return File.ReadAllLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
            .Select(line => line.Split('\t', 3))
            .Select(fields =>
            {
                Assert.Equal(3, fields.Length);
                return new WhitelistEntry(fields[0], fields[1], fields[2]);
            })
            .ToList();
    }

    private static bool IsWhitelisted(string relativePath, string token, IReadOnlyList<WhitelistEntry> whitelist)
    {
        return whitelist.Any(entry =>
            GlobMatch(relativePath, entry.PathGlob)
            && Regex.IsMatch(token, "^(?:" + entry.TokenPattern + ")$", RegexOptions.IgnoreCase));
    }

    private static bool GlobMatch(string value, string pattern)
    {
        var regex = "^" + Regex.Escape(pattern)
            .Replace("\\*\\*/", "(?:.*/)?")
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*") + "$";
        return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
    }

    private static string GetGitRepositoryRoot()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse --show-toplevel",
            WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        using (var process = Process.Start(startInfo)!)
        {
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            Assert.True(process.ExitCode == 0 && output.Length > 0, "git rev-parse --show-toplevel 执行失败");
            return output;
        }
    }

    private static string GetWhitelistPath(string repositoryRoot)
    {
        var localPath = Path.Combine(repositoryRoot, "GameFrameX.Admin.Unified.Api", "GameFrameX.Core.Tests", "Architecture", "LegacyScopeResidualWhitelist.tsv");
        return File.Exists(localPath) ? localPath : throw new FileNotFoundException("C406 residual whitelist missing", localPath);
    }

    /// <summary>
    /// 白名单行（嵌套类型遵守 one-type-per-file 例外）。
    /// </summary>
    private sealed record WhitelistEntry(string PathGlob, string TokenPattern, string Reason);
}
