using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C231 / U10：DTO 命名规范契约测试。规范正文：gfx-doc/decisions/2026-08-19-unified-dto-suffix-request-response.md（C237 推翻 2026-08-17 边界×后缀矩阵后的单套规则）。
// 五规则：R1 Dto 目录文件名=主类型名；R2 全仓单一后缀规则（输入 Request / 输出 Response，Admin/Client/Hub/Game 四 host 一致）；R3 Core.BaseInputOutput 公共类 Base 前缀；R4 禁 Req/Resp 冗余前缀；R5 Payment 渠道工程与 Client.Api/Services 禁 Result/Info/Item/Params 后缀（C290，GFX-697）。
// R2 例外：C232/C233/C237 后曾长期零例外；2026-08-27 c303 决策（RemoteConfig 命名统一）明确 LocationResponse -> Location 映射，按规范决策更新例外清单登记 RemoteConfigLocation 一条（见 LegacyViolations）。
// 第三方 webhook payload 镜像（Xsolla 嵌套 snake_case 结构、Hub.Api WebHook 目录、Google 镜像目录）按决策文档豁免清单豁免，不是例外。
// R5 的 LegacyViolations 条目不是待清理违规，而是 Apple/Xsolla 官方 JSON 镜像类的显式豁免登记（C290）：类型名跟随第三方原始命名，双向断言同样适用（镜像类删除后须移除豁免条目）。
// 双向断言：新增违规失败（防回潮）；例外清单出现死条目也失败（防僵尸例外，参照 C118 baseline 清零经验）。
public sealed class DtoNamingConventionTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly Regex TypeDeclarationRegex = new(
        @"^[ \t]*(?:public|internal|private|protected)?[ \t]*(?:sealed[ \t]+|abstract[ \t]+|static[ \t]+|partial[ \t]+|file[ \t]+)*(class|interface|enum|struct|record)[ \t]+(\w+)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex RedundantPrefixRegex = new(@"^(?:Req\w*Request|Resp\w*Response)$", RegexOptions.Compiled);

    private static readonly HashSet<string> LegacyViolations = new(StringComparer.Ordinal)
    {
        // R5 官方 JSON 镜像豁免（C290）：Apple verifyReceipt / App Store Server API 与 Xsolla webhook / payment/token
        // API 的官方字段镜像类，类型名跟随第三方原始命名（snake_case 属性同源），不套自家 Request/Response 后缀。
        "R5|Payment/GameFrameX.Payment.Apple/Models/Receipt/AppleReceiptInfo.cs|AppleReceiptInfo",
        "R5|Payment/GameFrameX.Payment.Apple/Models/Receipt/AppleReceiptVerificationInfo.cs|AppleReceiptVerificationInfo",
        "R5|Payment/GameFrameX.Payment.Apple/Models/Verification/AppleVerificationTransactionInfo.cs|AppleVerificationTransactionInfo",
        "R5|Payment/GameFrameX.Payment.Xsolla/Models/XsollaTransactionItem.cs|XsollaTransactionItem",
        "R5|Payment/GameFrameX.Payment.Xsolla/Models/XsollaVerifyReceiptItem.cs|XsollaVerifyReceiptItem",
        "R5|Payment/GameFrameX.Payment.Xsolla/Order/OrderManager.cs|OrderParamsCreateItem",
        // R2 决策级例外（2026-08-27 c303 RemoteConfig 命名统一，GFX-719/C311）：决策映射表明确
        // 旧名 LocationResponse 后缀类型 -> RemoteConfigLocation（RemoteConfigResponse 的嵌套 IP 地理位置类型，
        // wire JSON 形态不变），依据 gfx-doc/decisions/2026-08-27-c303-remote-config-naming-unification.md。
        "R2|GameFrameX.Client.Api/RemoteConfig/Dto/RemoteConfigLocation.cs|RemoteConfigLocation",
    };

    // R5 扫描根目录（C290，GFX-697）：Payment 渠道封装工程（Apple.AppStoreServer 官方 SDK 镜像工程整体不扫）
    // 与 Client.Api/Services 服务层。输出类后缀统一 Response、输入统一 Request，禁 Result/Info/Item/Params 混用。
    private static readonly string[] PaymentSuffixRuleDirectories =
    {
        "Payment/GameFrameX.Payment.Abstract",
        "Payment/GameFrameX.Payment.Apple",
        "Payment/GameFrameX.Payment.Xsolla",
        "Payment/GameFrameX.Payment.AliPay",
        "Payment/GameFrameX.Payment.Google",
        "GameFrameX.Client.Api/Services",
    };

    [Fact]
    public void DtoFiles_FileNameMatchesPrimaryTypeName()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in EnumerateDtoSourceFiles())
        {
            var types = ReadTypeDeclarations(file);
            if (types.Count == 0)
            {
                continue;
            }

            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName != types[0].TypeName)
            {
                actual.Add($"R1|{ToRelative(file)}|{types[0].TypeName}");
            }
        }

        AssertNoNewAndNoStale("R1", actual, "Dto 目录文件名必须等于文件内主类型名（git mv 对齐文件名，参照 C229 一类型一文件规则）。");
    }

    [Fact]
    public void DtoTypes_SuffixMatchesBoundaryMatrix()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in EnumerateDtoSourceFiles())
        {
            var relativePath = ToRelative(file);
            foreach (var declaration in ReadTypeDeclarations(file))
            {
                if (IsSuffixRuleExempt(relativePath, declaration.Kind, declaration.TypeName))
                {
                    continue;
                }

                var matched = declaration.TypeName.EndsWith("Request", StringComparison.Ordinal)
                              || declaration.TypeName.EndsWith("Response", StringComparison.Ordinal);

                if (!matched)
                {
                    actual.Add($"R2|{relativePath}|{declaration.TypeName}");
                }
            }
        }

        AssertNoNewAndNoStale("R2", actual, "类型名后缀必须符合全仓单一规则（2026-08-19 决策，C237）：输入 XxxRequest、输出 XxxResponse，Admin/Client/Hub/Game 四 host 一致。");
    }

    [Fact]
    public void BaseInputOutputTypes_UseBasePrefix()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        var projectDirectory = Path.Combine(RepositoryRoot, "GameFrameX.Core.BaseInputOutput");
        foreach (var file in Directory.EnumerateFiles(projectDirectory, "*.cs"))
        {
            foreach (var declaration in ReadTypeDeclarations(file))
            {
                if (declaration.Kind == "enum")
                {
                    continue;
                }

                if (declaration.Kind == "interface")
                {
                    if (!declaration.TypeName.StartsWith("IBase", StringComparison.Ordinal) && !declaration.TypeName.StartsWith("ICrud", StringComparison.Ordinal))
                    {
                        actual.Add($"R3|{ToRelative(file)}|{declaration.TypeName}");
                    }

                    continue;
                }

                if (!declaration.TypeName.StartsWith("Base", StringComparison.Ordinal))
                {
                    actual.Add($"R3|{ToRelative(file)}|{declaration.TypeName}");
                }
            }
        }

        AssertNoNewAndNoStale("R3", actual, "Core.BaseInputOutput 公共类必须 Base 前缀（接口 IBase*/ICrud*），禁止 XxxBaseRequest 后缀式。");
    }

    [Fact]
    public void AllTypes_NoRedundantReqRespPrefix()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in EnumerateAllSourceFiles())
        {
            foreach (var declaration in ReadTypeDeclarations(file))
            {
                if (RedundantPrefixRegex.IsMatch(declaration.TypeName))
                {
                    actual.Add($"R4|{ToRelative(file)}|{declaration.TypeName}");
                }
            }
        }

        AssertNoNewAndNoStale("R4", actual, "类型名禁止 Req/Resp 前缀与 Request/Response 后缀双重冗余（去掉前缀即可）。");
    }

    [Fact]
    public void PaymentAndClientServicesTypes_NoLegacyOutputSuffixes()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var relativeDirectory in PaymentSuffixRuleDirectories)
        {
            var directory = Path.Combine(RepositoryRoot, relativeDirectory);
            Assert.True(Directory.Exists(directory), "R5 扫描目录不存在（工程改名/迁移后必须同步更新 PaymentSuffixRuleDirectories）:" + relativeDirectory);
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                if (IsBuildOutput(file))
                {
                    continue;
                }

                foreach (var declaration in ReadTypeDeclarations(file))
                {
                    if (declaration.TypeName.EndsWith("Result", StringComparison.Ordinal)
                        || declaration.TypeName.EndsWith("Info", StringComparison.Ordinal)
                        || declaration.TypeName.EndsWith("Item", StringComparison.Ordinal)
                        || declaration.TypeName.EndsWith("Params", StringComparison.Ordinal))
                    {
                        actual.Add($"R5|{ToRelative(file)}|{declaration.TypeName}");
                    }
                }
            }
        }

        AssertNoNewAndNoStale("R5", actual, "Payment 渠道工程与 Client.Api/Services 的数据类禁止 Result/Info/Item/Params 后缀（C290）：输出类统一 Response、输入类统一 Request；官方 JSON 镜像类在 LegacyViolations 显式登记豁免。");
    }

    private static void AssertNoNewAndNoStale(string ruleId, HashSet<string> actual, string ruleDescription)
    {
        var expectedForRule = new HashSet<string>(LegacyViolations.Where(item => item.StartsWith(ruleId + "|", StringComparison.Ordinal)), StringComparer.Ordinal);
        var newViolations = actual.Except(expectedForRule).Order(StringComparer.Ordinal).ToList();
        var staleExceptions = expectedForRule.Except(actual).Order(StringComparer.Ordinal).ToList();

        Assert.True(
            newViolations.Count == 0,
            ruleDescription + " 新增违规（修复命名或按规范决策更新例外清单）:" + Environment.NewLine + string.Join(Environment.NewLine, newViolations));
        Assert.True(
            staleExceptions.Count == 0,
            ruleId + " 例外清单存在已不存在于代码库的死条目（存量清理后必须同步删除对应例外项）:" + Environment.NewLine + string.Join(Environment.NewLine, staleExceptions));
    }

    private static bool IsSuffixRuleExempt(string relativePath, string kind, string typeName)
    {
        if (kind == "enum")
        {
            return true;
        }

        if (typeName.EndsWith("Attribute", StringComparison.Ordinal))
        {
            return true;
        }

        if (relativePath.Contains("/Integrations/Payment/Dto/Google/", StringComparison.Ordinal))
        {
            return true;
        }

        // 第三方 webhook payload 镜像（C233）：Xsolla 嵌套结构名跟随 Xsolla JSON 对象名（含 snake_case）、
        // Tapd WebHook 目录同理，不套自家后缀（决策文档 §二矩阵「第三方 SDK 镜像跟随第三方原始命名」）。
        if (relativePath.Contains("/Integrations/Payment/Dto/XsollaWebHook", StringComparison.Ordinal)
            || relativePath.Contains("Hub.Api/Dto/WebHook/", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static List<TypeDeclaration> ReadTypeDeclarations(string file)
    {
        var declarations = new List<TypeDeclaration>();
        var source = File.ReadAllText(file);
        foreach (Match match in TypeDeclarationRegex.Matches(source))
        {
            declarations.Add(new TypeDeclaration(match.Groups[1].Value, match.Groups[2].Value));
        }

        return declarations;
    }

    private static IEnumerable<string> EnumerateDtoSourceFiles()
    {
        foreach (var directory in EnumerateDirectoriesByName(RepositoryRoot, "Dto"))
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                if (IsBuildOutput(file))
                {
                    continue;
                }

                yield return file;
            }
        }
    }

    private static IEnumerable<string> EnumerateAllSourceFiles()
    {
        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (IsBuildOutput(file))
            {
                continue;
            }

            yield return file;
        }
    }

    private static IEnumerable<string> EnumerateDirectoriesByName(string root, string name)
    {
        var pending = new Queue<string>();
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            var directories = Directory.GetDirectories(current);
            foreach (var directory in directories)
            {
                if (IsBuildOutputPath(directory))
                {
                    continue;
                }

                if (Path.GetFileName(directory) == name)
                {
                    yield return directory;
                }
                else
                {
                    pending.Enqueue(directory);
                }
            }
        }
    }

    private static bool IsBuildOutput(string file)
    {
        return file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
               || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
    }

    private static bool IsBuildOutputPath(string directory)
    {
        var name = Path.GetFileName(directory);
        return name is "bin" or "obj" or ".git";
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

    private sealed record TypeDeclaration(string Kind, string TypeName);
}