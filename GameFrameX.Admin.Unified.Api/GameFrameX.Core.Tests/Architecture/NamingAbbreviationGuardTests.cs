using System.Reflection;
using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.Architecture;

// C309（GFX-713）命名禁简写守护测试（政策收口）。B佬 2026-08-22 D1 决策：全系统不允许简写；
// 豁免与基线是临时措施，随清理批次收缩，基线清零后删除快照文件本身（不留永久基线）。
// 规范单一事实来源：gfx-doc/agent-handbook/naming-convention.md（白名单与本测试常量两处一致，改动须双向同步）。
// 三层词表（优先级从高到低）：
//   1. 第三方品牌名强制豁免（标识符含品牌字符串即豁免，先于禁止词判断——AdMob* 不因 Ad 词命中）；
//   2. GFX/Gfx 前缀豁免；
//   3. 业务简写禁止表（含复数形式），按 PascalCase/camelCase 词边界分词后整词匹配（Dictionary 是完整词，不误伤 Dict）。
// 通用技术缩写（Id/Url/Api/Db/Sms/Auth/Config/Info/Jwt/Http/Ip/Dns）不在禁止表内即天然允许，
// 白名单登记于 naming-convention.md 与下方 AllowedTechnicalAbbreviations（供文档一致性核对）。
// 守护范围：类型 / 方法 / 属性 / 字段（含枚举成员）标识符；方法参数与局部变量不在本测试范围。
// 扫描方式沿用 AsyncSuffixContractTests 先例：反射枚举 BaseDirectory 的 GameFrameX*.dll（注释与 #if false 不参与编译，天然免疫）。
// 守护范围锁定 slnx 产线程序集（RequiredAssemblies）；GameFrameX.Foundation.* 为外部 NuGet 包，不在治理范围。
// 存量冻结于同目录 NamingAbbreviationBaseline.baseline.txt，只减不增（新增违规失败 + 基线死条目失败，防僵尸口径参照
// ThirdPartyContractNamingGuardTests）。
public sealed class NamingAbbreviationGuardTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    /// <summary>业务简写禁止表（D1 政策）：分词后整词命中即违规。含复数形式。</summary>
    private static readonly HashSet<string> ProhibitedAbbreviations = new(StringComparer.Ordinal)
    {
        "Ad", "Ads",
        "Org", "Orgs",
        "Dict", "Dicts",
        "Btn", "Btns",
        "Nav", "Navs",
        "Img", "Imgs",
        "Msg", "Msgs",
        "Param", "Params",
        "Rep", "Reps",
    };

    /// <summary>通用技术缩写白名单：允许使用（不在禁止表内天然放行，此处登记与 gfx-doc naming-convention.md 保持一致）。</summary>
    private static readonly HashSet<string> AllowedTechnicalAbbreviations = new(StringComparer.Ordinal)
    {
        "Id", "Url", "Api", "Db", "Sms", "Auth", "Config", "Info", "Jwt", "Http", "Ip", "Dns",
    };

    /// <summary>第三方品牌名强制豁免：标识符包含任一品牌字符串即整体豁免（AdMob 含 Ad 词、UnityAds 含 Ads 词）。</summary>
    private static readonly string[] ThirdPartyBrandExemptions =
    {
        "AdMob", "Pangle", "UnityAds", "TopOn", "IP2Region", "Xsolla", "hiprint",
    };

    /// <summary>标识符分词：PascalCase / camelCase / 去掉前导下划线 / 全大写缩写段 / 数字段。</summary>
    private static readonly Regex WordSplitRegex = new(@"[A-Z]+(?=[A-Z][a-z])|[A-Z][a-z0-9]*|[a-z]+|[0-9]+", RegexOptions.Compiled);

    [Fact]
    public void Identifiers_ContainNoProhibitedAbbreviations_BeyondBaseline()
    {
        var baseline = LoadBaseline();
        var violations = CollectViolations();
        var additions = violations.Keys.Where(key => !baseline.Contains(key)).Order(StringComparer.Ordinal).ToList();

        Assert.True(
            additions.Count == 0,
            "新增禁简写违规（D1 政策：全系统不允许简写；存量冻结于 NamingAbbreviationBaseline.baseline.txt 只减不增）。"
            + "若为合理存量请勿加基线——先在 gfx-doc naming-convention.md 登记豁免理由；新增违规清单：" + Environment.NewLine
            + string.Join(Environment.NewLine, additions));
    }

    [Fact]
    public void Baseline_ContainsNoDeadEntries()
    {
        var baseline = LoadBaseline();
        var violations = CollectViolations();
        var deadEntries = baseline.Where(entry => !violations.ContainsKey(entry)).Order(StringComparer.Ordinal).ToList();

        Assert.True(
            deadEntries.Count == 0,
            "基线存在死条目（对应标识符已清理或改名）——随清理批次收缩基线，删除以下行；基线清零后删除快照文件本身：" + Environment.NewLine
            + string.Join(Environment.NewLine, deadEntries));
    }

    [Fact]
    public void AssemblyCoverage_CoversAllSlnxProjects()
    {
        // 双向锁定：slnx 产线项目必须全部可扫（防引用链丢失），在场的 GameFrameX*.dll 必须登记或为已知外部包
        // （防新工程脱离守护）。GameFrameX.Foundation.* 为外部 NuGet 包（4.0.0），非本仓代码不在治理范围。
        var present = new HashSet<string>(Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll")
            .Select(Path.GetFileNameWithoutExtension), StringComparer.Ordinal);
        var missing = RequiredAssemblies.Where(name => !present.Contains(name)).Order(StringComparer.Ordinal).ToList();
        var unregistered = present
            .Where(name => !RequiredAssemblies.Contains(name) && !name.StartsWith("GameFrameX.Foundation.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal).ToList();

        Assert.True(
            missing.Count == 0,
            "以下 slnx 项目未出现在测试输出目录（脱离命名守护覆盖），请补 GameFrameX.Core.Tests.csproj 的 ProjectReference：" + Environment.NewLine
            + string.Join(Environment.NewLine, missing));
        Assert.True(
            unregistered.Count == 0,
            "以下程序集在场但未登记（新工程须加入 RequiredAssemblies；新外部包前缀须在本测试登记豁免规则）：" + Environment.NewLine
            + string.Join(Environment.NewLine, unregistered));
    }

    [Fact]
    public void Tokenizer_DetectsDemoProhibitedAndSparesLegitimateNames()
    {
        // 验收演示（内置自检）：含 Btn 简写的标识符必须被检出。
        Assert.Equal(new[] { "Btn" }, FindProhibitedWords("BtnDemoWidget"));
        Assert.Equal(new[] { "Btn" }, FindProhibitedWords("GetOwnBtnPermListAsync"));

        // 完整词不误伤。
        Assert.Empty(FindProhibitedWords("Dictionary"));
        Assert.Empty(FindProhibitedWords("TenantAdvertisementCallbackConfigEntity"));
        Assert.Empty(FindProhibitedWords("ButtonPermissionList"));

        // 通用技术缩写白名单放行。
        Assert.Empty(FindProhibitedWords("DbConfigWithJwtAndHttpAndIpAndDnsAndUrlAndApiAndSmsAndAuthAndInfo"));

        // 复数形式同样命中。
        Assert.Equal(new[] { "Params" }, FindProhibitedWords("ServerParams"));

        // 第三方品牌名豁免优先于禁止词（AdMob 含 Ad、UnityAds 含 Ads）。
        Assert.Empty(FindProhibitedWords("AdMobCallbackRequest"));
        Assert.Empty(FindProhibitedWords("AdvertisementUnityAdsCallbackRequest"));

        // camelCase 私有字段带下划线前缀。
        Assert.Equal(new[] { "Msg" }, FindProhibitedWords("_logMsg"));

        // 白名单与禁止表互斥（两处词表若漂移直接失败，保证与 gfx-doc 一致性口径）。
        Assert.Empty(ProhibitedAbbreviations.Intersect(AllowedTechnicalAbbreviations));
    }

    /// <summary>分词并返回命中的禁止简写词（无命中返回空）。品牌豁免与 GFX 前缀豁免优先。</summary>
    private static string[] FindProhibitedWords(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return Array.Empty<string>();
        }

        foreach (var brand in ThirdPartyBrandExemptions)
        {
            if (identifier.Contains(brand, StringComparison.Ordinal))
            {
                return Array.Empty<string>();
            }
        }

        if (identifier.StartsWith("GFX", StringComparison.Ordinal) || identifier.StartsWith("Gfx", StringComparison.Ordinal))
        {
            return Array.Empty<string>();
        }

        var trimmed = identifier.TrimStart('_');
        return WordSplitRegex.Matches(trimmed)
            .Where(match => ProhibitedAbbreviations.Contains(match.Value))
            .Select(match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>反射枚举全部 GameFrameX*.dll 的类型/方法/属性/字段标识符，返回违规字典（key = 基线行格式）。</summary>
    private static SortedDictionary<string, string> CollectViolations()
    {
        var violations = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var assemblyPath in EnumerateInScopeAssemblies())
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyPath));
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (IsSyntheticName(type.Name))
                {
                    continue;
                }

                RecordViolation(violations, assemblyName, type.FullName ?? type.Name, "Type", type.Name);

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (method.IsSpecialName || IsSyntheticName(method.Name))
                    {
                        continue; // get_/set_/op_ 等由属性自身与运算符覆盖；编译器生成名跳过
                    }

                    RecordViolation(violations, assemblyName, type.FullName ?? type.Name, "Method", method.Name);
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    RecordViolation(violations, assemblyName, type.FullName ?? type.Name, "Property", property.Name);
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (IsSyntheticName(field.Name))
                    {
                        continue; // <Prop>k__BackingField 等编译器生成字段
                    }

                    RecordViolation(violations, assemblyName, type.FullName ?? type.Name, "Field", field.Name);
                }
            }
        }

        return violations;
    }

    private static void RecordViolation(SortedDictionary<string, string> violations, string assemblyName, string typeName, string kind, string identifier)
    {
        var words = FindProhibitedWords(identifier);
        if (words.Length > 0)
        {
            violations[assemblyName + "|" + typeName + "|" + kind + "|" + identifier] = string.Join("/", words);
        }
    }

    private static bool IsSyntheticName(string name)
    {
        return name.Contains('<') || name.Contains('>');
    }

    /// <summary>读取基线快照（# 注释与空行跳过），行格式：Assembly|TypeFullName|Kind|Identifier（命中词存于行尾注释）。</summary>
    private static HashSet<string> LoadBaseline()
    {
        var path = Path.Combine(RepositoryRoot, "GameFrameX.Core.Tests", "Architecture", "NamingAbbreviationBaseline.baseline.txt");
        Assert.True(File.Exists(path), "基线快照缺失：" + path);
        var entries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var entry = line;
            var commentIndex = entry.IndexOf(" #", StringComparison.Ordinal);
            if (commentIndex >= 0)
            {
                entry = entry[..commentIndex].TrimEnd();
            }

            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>slnx 全部产线项目（AssemblyCoverage_CoversAllSlnxProjects 锚点；新增工程须同步登记）。</summary>
    private static readonly string[] RequiredAssemblies =
    {
        "GameFrameX.Advertisement.Abstract",
        "GameFrameX.Advertisement.AdMob",
        "GameFrameX.Advertisement.Pangle",
        "GameFrameX.Advertisement.TopOn",
        "GameFrameX.Advertisement.UnityAds",
        "GameFrameX.Email.Abstract",
        "GameFrameX.Email.Smtp",
        "GameFrameX.Email.TencentSes",
        "GameFrameX.Application",
        "GameFrameX.Application.Game",
        "GameFrameX.Application.System",
        "GameFrameX.Client.Api",
        "GameFrameX.Core",
        "GameFrameX.Core.BaseInputOutput",
        "GameFrameX.Core.Extension",
        "GameFrameX.Core.SqlSugar",
        "GameFrameX.Core.Tests",
        "GameFrameX.Core.Utility",
        "GameFrameX.Entity",
        "GameFrameX.Hub.Api",
        "GameFrameX.Web.Api",
        "GameFrameX.ObjectStorage.Abstract",
        "GameFrameX.ObjectStorage.AliyunOss",
        "GameFrameX.ObjectStorage.Local",
        "GameFrameX.ObjectStorage.QiniuKodo",
        "GameFrameX.ObjectStorage.TencentCos",
        "GameFrameX.Payment.Abstract",
        "GameFrameX.Payment.AliPay",
        "GameFrameX.Payment.Apple",
        "GameFrameX.Payment.Apple.AppStoreServer",
        "GameFrameX.Payment.Google",
        "GameFrameX.Payment.Wechat",
        "GameFrameX.Payment.Xsolla",
        "GameFrameX.Sms.Abstract",
        "GameFrameX.Sms.Aliyun",
        "GameFrameX.Wechat.Abstract",
        "GameFrameX.Wechat.Open",
    };

    /// <summary>守护范围 = slnx 产线程序集（RequiredAssemblies）；GameFrameX.Foundation.* 外部 NuGet 包不在治理范围。</summary>
    private static IEnumerable<string> EnumerateInScopeAssemblies()
    {
        var required = new HashSet<string>(RequiredAssemblies, StringComparer.Ordinal);
        foreach (var assemblyPath in Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll").Order(StringComparer.Ordinal))
        {
            if (required.Contains(Path.GetFileNameWithoutExtension(assemblyPath)))
            {
                yield return assemblyPath;
            }
        }
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
