using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C267：传输签名规范统一守护测试。规范正文：gfx-doc/dev/signature/spec.md（唯一权威 spec）。
// 四条守护（防回潮）：
//   G1 HmacSignatureHelper 全部符号清零（规范 B 旧算法核心已随文件删除，不得复活）
//   G2 new HMACSHA256/HMACSHA1 构造仅允许 CanonicalRequestBuilder（全仓唯一 HMAC 实现）；
//      豁免：SignatureValidationMiddlewareTests（W 向量既有签名 helper，历史事实源）。
//      扫描范围为 GameFrameX.* 顶层产品目录 —— ObjectStorage/ 下腾讯云 COS v5 协议要求的
//      HMAC-SHA1（ObjectStorageSignHelper）按 C267 期外声明豁免，不在扫描域内。
//   G3 SignatureAuthentication* 整条 OpenAccess authentication scheme 链路清零（8 文件已删）
//   G4 禁止字符串序数比较签名（serverSign ==/!= sign 形态；字节级 FixedTimeEquals 是唯一比较方式）
public sealed class HmacSignatureGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    // G2 豁免：唯一实现 + 向量测试 helper
    private static readonly HashSet<string> HmacConstructorExemptions = new(StringComparer.Ordinal)
    {
        "GameFrameX.Core/SignatureAuth/CanonicalRequestBuilder.cs",
        "GameFrameX.Core.Tests/ClientApi/SignatureValidationMiddlewareTests.cs",
    };

    // 守护测试自身需要书写被守护的关键字（G1/G3），自引用豁免
    private const string GuardSelfPath = "GameFrameX.Core.Tests/Architecture/HmacSignatureGuardTests.cs";

    [Fact]
    public void G1_HmacSignatureHelper_AllReferencesRemoved()
    {
        var offenders = EnumerateProductSources()
                        .Where(f => f != GuardSelfPath)
                        .Where(f => File.ReadAllText(Path.Combine(RepositoryRoot, f)).Contains("HmacSignatureHelper"))
                        .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void G2_HmacConstructor_OnlyAllowedInCanonicalRequestBuilder()
    {
        var pattern = new Regex(@"new\s+HMACSHA(256|1)?\s*\(", RegexOptions.Compiled);
        var offenders = EnumerateProductSources()
                        .Where(f => !HmacConstructorExemptions.Contains(f))
                        .Where(f => pattern.IsMatch(File.ReadAllText(Path.Combine(RepositoryRoot, f))))
                        .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void G3_SignatureAuthenticationScheme_FullyRemoved()
    {
        var offenders = EnumerateProductSources()
                        .Where(f => f != GuardSelfPath)
                        .Where(f => File.ReadAllText(Path.Combine(RepositoryRoot, f)).Contains("SignatureAuthentication"))
                        .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public void G4_StringOrdinalSignatureCompare_Forbidden()
    {
        var pattern = new Regex(@"serverSign\s*(==|!=)\s*sign", RegexOptions.Compiled);
        var offenders = EnumerateProductSources()
                        .Where(f => pattern.IsMatch(File.ReadAllText(Path.Combine(RepositoryRoot, f))))
                        .ToList();
        Assert.Empty(offenders);
    }

    /// <summary>
    /// 枚举 GameFrameX.* 顶层产品目录下的全部 .cs 源文件（相对路径，/ 分隔；排除 obj/bin 构建产物）。
    /// </summary>
    private static IEnumerable<string> EnumerateProductSources()
    {
        var root = new DirectoryInfo(RepositoryRoot);
        foreach (var projectDir in root.EnumerateDirectories("GameFrameX.*"))
        {
            foreach (var file in projectDir.EnumerateFiles("*.cs", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(RepositoryRoot, file.FullName).Replace(Path.DirectorySeparatorChar, '/');
                var segments = relative.Split('/');
                if (segments.Contains("obj") || segments.Contains("bin"))
                {
                    continue;
                }

                yield return relative;
            }
        }
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