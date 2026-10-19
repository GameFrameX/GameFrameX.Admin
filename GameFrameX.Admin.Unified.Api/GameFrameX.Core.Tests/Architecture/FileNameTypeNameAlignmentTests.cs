using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C285（GFX-694）：文件名 = 主类型名守护测试。全仓 .cs 文件的主（首个）类型名必须等于文件基名。
// 背景：Application.Game 服务类统一加 Tenant 前缀重构后文件未同步 git mv（C210 等），2026-08-22 命名审查盘点 36 处不一致，本测试防回潮。
// 豁免三类既有约定（豁免规则本身即规范，无静态例外清单，双向断言：新增违规失败）：
//   E1 partial 分片 TypeName.Suffix.cs——文件声明同名 partial 类型（GlobalMenuSeedData.*.cs 13 个、GameOrderController.{AliPay,ApplePay,GooglePay,Xslloa}.cs 4 个、GooglePlayApiManager.UploadSku.cs）；
//   E2 泛型 OfT 后缀——文件名 TypeNameOfT.cs 声明泛型 TypeName<T>（IBaseRequestOfT / IConfigurableOptionsOfT / BaseControllerOfT 3 处）；
//   E3 无类型声明文件（top-level 语句 Program.cs）与生成文件（*.g.cs，如 TenantDefaultTemplateDefinitions.g.cs）。
public sealed class FileNameTypeNameAlignmentTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly Regex TypeDeclarationRegex = new(
        @"^[ \t]*(?:public|internal|private|protected)?[ \t]*((?:(?:sealed|abstract|static|partial|file)[ \t]+)*)(class|interface|enum|struct|record)[ \t]+(\w+)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    [Fact]
    public void AllCsFiles_FileNameMatchesPrimaryTypeName()
    {
        var violations = new List<string>();
        foreach (var file in EnumerateSourceFiles())
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.EndsWith(".g", StringComparison.Ordinal))
            {
                continue; // E3 生成文件
            }

            var primary = ReadPrimaryTypeDeclaration(file);
            if (primary is null)
            {
                continue; // E3 无类型声明（top-level Program.cs 等）
            }

            if (fileName == primary.TypeName)
            {
                continue;
            }

            if (fileName == primary.TypeName + "OfT")
            {
                continue; // E2 泛型 TypeName<T> → TypeNameOfT.cs
            }

            if (fileName.Contains('.', StringComparison.Ordinal) && fileName.Split('.')[0] == primary.TypeName && primary.IsPartial)
            {
                continue; // E1 partial 分片 TypeName.Suffix.cs
            }

            violations.Add($"{ToRelative(file)} | 主类型 {primary.TypeName}");
        }

        Assert.True(
            violations.Count == 0,
            "文件名必须等于主类型名（git mv 对齐，禁止只改类名不改文件名；partial 分片 TypeName.Suffix.cs / 泛型 OfT / 生成文件 .g.cs / 无类型声明文件除外）违规清单:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static PrimaryTypeDeclaration? ReadPrimaryTypeDeclaration(string file)
    {
        var source = File.ReadAllText(file);
        var match = TypeDeclarationRegex.Match(source);
        if (!match.Success)
        {
            return null;
        }

        var isPartial = match.Groups[1].Value.Contains("partial", StringComparison.Ordinal);
        return new PrimaryTypeDeclaration(match.Groups[3].Value, isPartial);
    }

    private static IEnumerable<string> EnumerateSourceFiles()
    {
        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (IsBuildOutput(file) || file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            yield return file;
        }
    }

    private static bool IsBuildOutput(string file)
    {
        return file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static string ToRelative(string file)
    {
        return Path.GetRelativePath(RepositoryRoot, file).Replace(Path.DirectorySeparatorChar, '/');
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

    private sealed record PrimaryTypeDeclaration(string TypeName, bool IsPartial);
}
