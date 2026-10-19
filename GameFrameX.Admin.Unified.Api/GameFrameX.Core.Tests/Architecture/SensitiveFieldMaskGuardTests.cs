using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C369（GFX-828）：敏感字段脱敏基建统一守护。
// change：gfx-doc/dev/system-management/changes/C369-sensitive-field-mask-foundation/change.md
// SensitiveFieldMask（GameFrameX.Core/Service）是全仓 "******" 脱敏占位契约的唯一实现。
// 历史上曾有 4 处并行副本 + 多处内联字面量，每个新凭证实体都要重新手接一遍，漏接即泄露。
// 本守护防止回潮：Core 与测试工程之外不得再定义同语义字符串常量或出现 "******" 字面量赋值，
// 检测限定赋值表达式所在行（跳过 // 与 /// 注释行），XML 文档注释中的占位示例不受影响。
public sealed class SensitiveFieldMaskGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly Regex MaskLiteralPattern = new("\"\\*{6}\"", RegexOptions.Compiled);

    /// <summary>
    /// 豁免目录（相对仓库根，以 / 结尾）：
    /// <para>- GameFrameX.Core/：唯一实现的家（SensitiveFieldMask.MaskValue、PlatformJwtConfigConstants.MaskedSigningKey）；</para>
    /// <para>- GameFrameX.Core.Tests/：测试断言本身需要字面量；</para>
    /// <para>- ObjectStorage/GameFrameX.ObjectStorage.Abstract/：零依赖抽象层，禁止引用 Core，
    /// 其 ObjectStorageConfigMask.Placeholder 与 MaskValue 值一致由人工评审保证。</para>
    /// </summary>
    private static readonly string[] ExcludedDirectoryPrefixes =
    {
        "GameFrameX.Core/",
        "GameFrameX.Core.Tests/",
        "ObjectStorage/GameFrameX.ObjectStorage.Abstract/",
    };

    private static readonly string[] ExcludedDirectoryNameFragments = { "/bin/", "/obj/", "/.git/", "/.menu-test/", "/node_modules/" };

    [Fact]
    public void NoMaskLiteralOutsideCoreAndTests()
    {
        var violations = new List<string>();
        var sourceFiles = Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories)
                                   .Select(path => Path.GetRelativePath(RepositoryRoot, path).Replace('\\', '/'))
                                   .Where(IsGuardedSourceFile)
                                   .OrderBy(path => path, StringComparer.Ordinal)
                                   .ToList();

        foreach (var relativePath in sourceFiles)
        {
            var lines = File.ReadAllLines(Path.Combine(RepositoryRoot, relativePath));
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.TrimStart().StartsWith("//"))
                {
                    continue;
                }

                if (MaskLiteralPattern.IsMatch(line))
                {
                    violations.Add($"{relativePath}:{i + 1}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Core 与测试工程之外发现 \"******\" 字面量，敏感字段脱敏必须统一走 SensitiveFieldMask（C369 唯一收口）：\n" +
            string.Join("\n", violations));
    }

    [Fact]
    public void Detector_DetectsMaskLiteralInSample()
    {
        // 检测器自检：防止守护正则失效导致主测试恒绿（RED 能力证明）。
        Assert.Matches(MaskLiteralPattern, "        private const string MaskValue = \"******\";");
        Assert.Matches(MaskLiteralPattern, "            item.Secret = \"******\";");
        Assert.DoesNotMatch(MaskLiteralPattern, "        /// 敏感字段输出脱敏 <c>******</c>；注释豁免。");
    }

    private static bool IsGuardedSourceFile(string relativePath)
    {
        if (ExcludedDirectoryNameFragments.Any(fragment => relativePath.Contains(fragment, StringComparison.Ordinal)))
        {
            return false;
        }

        return !ExcludedDirectoryPrefixes.Any(prefix => relativePath.StartsWith(prefix, StringComparison.Ordinal));
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
