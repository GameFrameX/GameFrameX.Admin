using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C292 / GFX-698：第三方契约命名治理守护（issue 任务 2 的豁免登记 + 防扩散）。
// 四规则：
//   R1 全仓活代码公开属性名禁止 snake_case（含下划线）——第三方 wire 名只能经 [JsonPropertyName] 镜像；
//   R2 全仓活代码公开属性名禁止纯小写单词——仅允许登记的 LegacyViolations（自家业务 DTO 待后续治理）；
//   R3 SCREAMING_SNAKE 枚举成员仅允许 Apple AppStoreServer Models 目录（Apple 官方模型风格镜像）；
//   R4 中文枚举成员仅允许 WechatReturnCodeEnum（Senparc 移植的微信返回码原文）。
// 双向断言：新增违规失败（防扩散）；豁免/例外清单出现死条目也失败（防僵尸，参照 DtoNamingConventionTests 口径）。
// 活代码定义：剥离 /* */ 块注释与 // 行注释后仍存在的声明（Xsolla/Google 镜像类内有大量注释形态的旧字段，保留不动）。
public sealed class ThirdPartyContractNamingGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly Regex PropertyDeclarationRegex = new(
        @"public\s+(?:virtual\s+)?[A-Za-z_][\w<>?,\.\[\]\s:]*?\s(?<name>[a-z][a-z0-9]*(?:_[a-z0-9]+)*)\s*\{\s*get",
        RegexOptions.Compiled);

    private static readonly Regex EnumMemberRegex = new(
        @"^[ \t]*(?<name>[A-Za-z_\p{IsCJKUnifiedIdeographs}][\w\p{IsCJKUnifiedIdeographs}]*)\s*[=,]",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>R2 例外：自家业务 DTO 的存量纯小写属性（非第三方契约，改名涉及 Admin wire 评审，待独立 change 治理）。
    /// GameCommodity 四项 status 已随 C292/#187 PascalCase 化清零（main 既有僵尸条目，C289 顺手删除）。</summary>
    private static readonly HashSet<string> LowercaseWordLegacyViolations = new(StringComparer.Ordinal)
    {
    };

    /// <summary>R1 豁免：双 wire 变体镜像——camelCase 与 snake_case 双份属性共存，Pascal 化会产生仅差大小写的同名成员。</summary>
    private static readonly HashSet<string> SnakeCaseExemptions = new(StringComparer.Ordinal)
    {
        "GameFrameX.Client.Api/Integrations/Payment/Dto/Google/GoogleWebHookInputMessage.cs|message_id",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/Google/GoogleWebHookInputMessage.cs|publish_time",
    };

    /// <summary>R3 豁免目录：Apple App Store Server API 官方模型镜像（枚举成员 SCREAMING_SNAKE 跟随 Apple 官方定义）。</summary>
    private const string AppleModelsDirectory = "Payment/GameFrameX.Payment.Apple.AppStoreServer/Models";

    /// <summary>R4 豁免类型：Senparc 移植的微信返回码枚举，成员名为微信官方中文描述。</summary>
    private const string WechatReturnCodeEnumFile = "GameFrameX.Entity/Enum/WechatReturnCodeEnum.cs";

    /// <summary>第三方契约镜像登记表（C292 任务 1 治理对象）：文件必须存在且属性已全量 PascalCase（防回潮）。</summary>
    private static readonly string[] ThirdPartyMirrorRegistry =
    {
        "Payment/GameFrameX.Payment.Xsolla/Order/OrderManager.cs",
        "Payment/GameFrameX.Payment.Xsolla/Reports/Transactions/Search.cs",
        "GameFrameX.Client.Api/GameWebShop/Dto/TenantGameWebShopEntityBuyNotifyRequest.cs",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/XsollaWebHookOrderPaidRequest.cs",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/XsollaWebHookRequest.cs",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/XsollaWebHookUserValidationRequest.cs",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/Google/GoogleWebHookRequest.cs",
        "GameFrameX.Client.Api/Integrations/Payment/Dto/Google/GoogleWebHookInputMessage.cs",
        "GameFrameX.Client.Api/Integrations/Advertisement/Dto/AdMobCallbackRequest.cs",
        "GameFrameX.Client.Api/Integrations/Advertisement/Dto/AdvertisementUnityAdsCallbackRequest.cs",
        "GameFrameX.Client.Api/GrafanaLoki/GrafanaLokiAnalyticsEntry.cs",
        "GameFrameX.Client.Api/GrafanaLoki/GrafanaLokiAnalyticsData.cs",
        "GameFrameX.Client.Api/GrafanaLoki/GrafanaLokiAnalyticsDataStreamsItem.cs",
        "GameFrameX.Hub.Api/Dto/WebHook/TapdWebHookData.cs",
        "Email/GameFrameX.Email.TencentSes/SendEmailTemplateData.cs",
    };

    [Fact]
    public void AllSource_Properties_AreNotSnakeCase()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sourceFile in EnumerateLiveCode())
        {
            var relativePath = sourceFile.RelativePath;
            var code = sourceFile.Code;
            foreach (var name in PropertyDeclarationRegex.Matches(code).Select(match => match.Groups["name"].Value))
            {
                if (!name.Contains('_', StringComparison.Ordinal))
                {
                    continue;
                }

                actual.Add(relativePath + "|" + name);
            }
        }

        AssertNoNewAndNoStale(actual, SnakeCaseExemptions, "R1: 活代码公开属性名禁止 snake_case；第三方 wire 名必须以 [JsonPropertyName(\"...\")] 镜像、属性名用 PascalCase（C292）。");
    }

    [Fact]
    public void AllSource_Properties_AreNotLowercaseWord()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sourceFile in EnumerateLiveCode())
        {
            var relativePath = sourceFile.RelativePath;
            var code = sourceFile.Code;
            foreach (var match in PropertyDeclarationRegex.Matches(code))
            {
                var name = ((Match)match).Groups["name"].Value;
                if (name.Contains('_', StringComparison.Ordinal))
                {
                    continue;
                }

                actual.Add(relativePath + "|" + name);
            }
        }

        AssertNoNewAndNoStale(actual, LowercaseWordLegacyViolations, "R2: 活代码公开属性名禁止纯小写单词（PascalCase 化 + 特性锁 wire，参照 C292）；自家存量例外须在 LowercaseWordLegacyViolations 登记。");
    }

    [Fact]
    public void ScreamingSnakeEnumMembers_OnlyInAppleModelsMirror()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sourceFile in EnumerateLiveCode())
        {
            var relativePath = sourceFile.RelativePath;
            var code = sourceFile.Code;
            if (relativePath.StartsWith(AppleModelsDirectory + "/", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var name in EnumMemberRegex.Matches(code).Select(match => match.Groups["name"].Value))
            {
                if (name.Length > 1 && name.All(character => char.IsUpper(character) || char.IsDigit(character) || character == '_') && name.Contains('_', StringComparison.Ordinal))
                {
                    actual.Add(relativePath + "|" + name);
                }
            }
        }

        AssertNoNewAndStale(actual, "R3: SCREAMING_SNAKE 枚举成员仅允许 Apple AppStoreServer Models 镜像目录（" + AppleModelsDirectory + "），其余域新增即违规。");
    }

    [Fact]
    public void ChineseEnumMembers_OnlyInWechatReturnCodeEnum()
    {
        var actual = new HashSet<string>(StringComparer.Ordinal);
        foreach (var sourceFile in EnumerateLiveCode())
        {
            var relativePath = sourceFile.RelativePath;
            var code = sourceFile.Code;
            if (relativePath == WechatReturnCodeEnumFile)
            {
                continue;
            }

            foreach (var name in EnumMemberRegex.Matches(code).Select(match => match.Groups["name"].Value))
            {
                if (name.Any(char.IsLetter) && name.Any(character => character >= '一' && character <= '鿿'))
                {
                    actual.Add(relativePath + "|" + name);
                }
            }
        }

        AssertNoNewAndStale(actual, "R4: 中文枚举成员仅允许 WechatReturnCodeEnum（Senparc 移植，" + WechatReturnCodeEnumFile + "）。");
    }

    [Fact]
    public void ThirdPartyMirrorRegistry_FilesExistAndPropertiesArePascalCase()
    {
        var failures = new List<string>();
        foreach (var relativePath in ThirdPartyMirrorRegistry)
        {
            var fullPath = Path.Combine(RepositoryRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                failures.Add($"镜像登记条目文件不存在（僵尸登记）：{relativePath}");
                continue;
            }

            var code = StripComments(File.ReadAllText(fullPath));
            foreach (var name in PropertyDeclarationRegex.Matches(code).Select(match => match.Groups["name"].Value))
            {
                var exemptionKey = relativePath + "|" + name;
                if (SnakeCaseExemptions.Contains(exemptionKey))
                {
                    continue;
                }

                failures.Add($"镜像类属性回潮为小写命名：{relativePath}|{name}");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static void AssertNoNewAndStale(HashSet<string> actual, string ruleDescription)
    {
        Assert.True(actual.Count == 0, ruleDescription + " 违规项:" + Environment.NewLine + string.Join(Environment.NewLine, actual.Order(StringComparer.Ordinal)));
    }

    private static void AssertNoNewAndNoStale(HashSet<string> actual, HashSet<string> expected, string ruleDescription)
    {
        var newViolations = actual.Except(expected).Order(StringComparer.Ordinal).ToList();
        var staleExceptions = expected.Except(actual).Order(StringComparer.Ordinal).ToList();
        Assert.True(newViolations.Count == 0, ruleDescription + " 新增违规:" + Environment.NewLine + string.Join(Environment.NewLine, newViolations));
        Assert.True(staleExceptions.Count == 0, ruleDescription + " 例外清单死条目（存量清理后须同步删除）:" + Environment.NewLine + string.Join(Environment.NewLine, staleExceptions));
    }

    private static IEnumerable<SourceFile> EnumerateLiveCode()
    {
        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(RepositoryRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            if (IsBuildOutputPath(relativePath))
            {
                continue;
            }

            yield return new SourceFile(relativePath, StripComments(File.ReadAllText(file)));
        }
    }

    private static string StripComments(string source)
    {
        source = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        source = Regex.Replace(source, @"//[^\n]*", string.Empty);
        return source;
    }

    private static bool IsBuildOutputPath(string relativePath)
    {
        return relativePath.Contains("/bin/", StringComparison.Ordinal)
               || relativePath.Contains("/obj/", StringComparison.Ordinal)
               || relativePath.Contains("/.git/", StringComparison.Ordinal);
    }

    /// <summary>活代码扫描单元：相对路径 + 剥注释后的源码。</summary>
    private sealed record SourceFile(string RelativePath, string Code);

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
