using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Localization.Errors;

namespace GameFrameX.Core.Tests.Architecture;

// C263 / GFX-669：统一错误码体系 T1 契约守护。
// 权威依据：gfx-doc/decisions/2026-08-20-unified-error-code-segment-system.md（ADR 分段表）
// 与 gfx-doc/dev/global-config/changes/C262-unified-error-code-system/change.md（父 change）。
// 五类断言：
//   U1 全局数值唯一（0 保留给信封成功码，任何成员不得为 0；8 位取值域）
//   S1 每成员前 3 位段命中 ErrorSegmentRegistry 且 EnumTypeName 与所属模块 enum 匹配（含 pending 段不可承载成员）
//   S2 登记表自洽：段首位落在所属大域区间；已实现段必须存在对应 enum 且成员全部落在本段
//   L1 全部枚举成员挂载非空 [ErrorText]（中英双语文案声明即数据，单一事实来源）
//   C1 源码扫描禁 (XxxErrorCode)数字 强转字面量（杜绝魔法数字；扫描排除本测试文件自身与 bin/obj）
//   R1 旧枚举防回潮（C264 / T2）：三 host Result/ResultCode.cs 不复存在，全仓无旧 using / 裸 ResultCode 成员引用
// 另含 BusinessException 新构造行为测试（message 缺省走 ErrorLocalizer + int Code 出站）。
public sealed class ErrorCodeContractTests
{
    private const string ErrorsNamespace = "GameFrameX.Core.Enum.Errors";

    private static string RepositoryRoot
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find GameFrameX.Admin.Unified.slnx.");
        }
    }

    private static List<Type> ErrorCodeEnums
    {
        get
        {
            return typeof(ErrorSegmentRegistry).Assembly
                                               .GetTypes()
                                               .Where(t => t.IsEnum && t.Namespace == ErrorsNamespace && t.Name.EndsWith("ErrorCode", StringComparison.Ordinal))
                                               .OrderBy(t => t.Name, StringComparer.Ordinal)
                                               .ToList();
        }
    }

    /// <summary>
    /// 错误码成员描述（枚举类型 + 成员名 + 数值；嵌套类型遵守 one-type-per-file 例外）。
    /// </summary>
    private sealed class ErrorCodeMember
    {
        public ErrorCodeMember(Type enumType, string member, int value)
        {
            EnumType = enumType;
            Member = member;
            Value = value;
        }

        public Type EnumType { get; }

        public string Member { get; }

        public int Value { get; }
    }

    private static List<ErrorCodeMember> AllMembers()
    {
        var members = new List<ErrorCodeMember>();
        foreach (var enumType in ErrorCodeEnums)
        {
            foreach (var value in global::System.Enum.GetValues(enumType).Cast<object>())
            {
                members.Add(new ErrorCodeMember(enumType, value.ToString() ?? string.Empty, Convert.ToInt32(value, CultureInfo.InvariantCulture)));
            }
        }

        return members;
    }

    // U1：全局数值唯一；0 保留给信封成功码；取值域为 8 位（10000000-99999999）。
    [Fact]
    public void AllErrorCodes_AreGloballyUnique_AndZeroReserved()
    {
        var members = AllMembers();
        Assert.NotEmpty(members);

        var duplicates = members.GroupBy(m => m.Value).Where(g => g.Count() > 1).ToList();
        Assert.True(duplicates.Count == 0,
                    "跨模块错误码数值重复：\n" + string.Join("\n", duplicates.Select(g =>
                                                                              $"{g.Key} <- {string.Join(", ", g.Select(m => $"{m.EnumType.Name}.{m.Member}"))}")));

        foreach (var member in members)
        {
            Assert.NotEqual(0, member.Value);
            Assert.InRange(member.Value, 10000000, 99999999);
        }
    }

    // S1：每成员前 3 位命中段登记，且登记条目 EnumTypeName 与所属枚举名一致；pending 段不可承载成员。
    [Fact]
    public void EveryMember_SegmentIsRegistered_AndMatchesModuleEnum()
    {
        var problems = new List<string>();
        foreach (var member in AllMembers())
        {
            var segment = member.Value / 100000;
            var entry = ErrorSegmentRegistry.Find(segment);
            if (entry is null)
            {
                problems.Add($"{member.EnumType.Name}.{member.Member}={member.Value}：段 {segment} 未在 ErrorSegmentRegistry 登记（新模块必须先登记段再写码）");
                continue;
            }

            if (entry.PendingImplementation)
            {
                problems.Add($"{member.EnumType.Name}.{member.Member}={member.Value}：段 {segment} 登记为 PendingImplementation（{entry.EnumTypeName} 成员平移归 C262 T3），本阶段不得承载成员");
            }

            if (entry.EnumTypeName != member.EnumType.Name)
            {
                problems.Add($"{member.EnumType.Name}.{member.Member}={member.Value}：段 {segment} 登记模块为 {entry.EnumTypeName}，与枚举名 {member.EnumType.Name} 不匹配");
            }
        }

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    // S2：登记表自洽 —— 段首位落在所属大域区间；已实现段必须有对应 enum 且其成员全部落在该枚举登记的段集合内。
    // 段集合按 EnumTypeName 分组（C339 起 OnlineErrorCode 一枚举跨 320～327 八段为合法形态；枚举成员仍不得漂出自己登记的段集合）。
    [Fact]
    public void RegistryEntries_AreConsistent_WithDomainAndImplementedEnums()
    {
        var problems = new List<string>();
        var enumTypesByName = ErrorCodeEnums.ToDictionary(t => t.Name, t => t, StringComparer.Ordinal);

        foreach (var entry in ErrorSegmentRegistry.Entries)
        {
            Assert.InRange(entry.Segment, 0, 999);
            Assert.True(ErrorSegmentRegistry.IsLeadingDigitInDomain(entry.Segment, entry.Domain),
                        $"段 {entry.Segment}（{entry.Module}）首位不在 {entry.Domain} 域区间");

            if (entry.EnumTypeName is null || entry.PendingImplementation)
            {
                continue;
            }

            if (!enumTypesByName.TryGetValue(entry.EnumTypeName, out var enumType))
            {
                problems.Add($"段 {entry.Segment} 登记 EnumTypeName={entry.EnumTypeName} 且非 pending，但对应枚举不存在");
            }
        }

        var segmentsByEnum = ErrorSegmentRegistry.Entries
                                                 .Where(e => e.EnumTypeName is not null && !e.PendingImplementation)
                                                 .GroupBy(e => e.EnumTypeName!, StringComparer.Ordinal)
                                                 .ToDictionary(g => g.Key, g => g.Select(e => e.Segment).ToHashSet(), StringComparer.Ordinal);
        foreach (var pair in segmentsByEnum)
        {
            var enumType = enumTypesByName[pair.Key];
            foreach (var value in global::System.Enum.GetValues(enumType).Cast<object>())
            {
                var numeric = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                if (!pair.Value.Contains(numeric / 100000))
                {
                    problems.Add($"{enumType.Name}.{value}={numeric} 不在该枚举登记的段集合（{string.Join("/", segmentsByEnum[pair.Key].Order())}）内");
                }
            }
        }

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    // L1：全部枚举成员挂载非空 [ErrorText]（中英双语文案声明即数据，杜绝枚举与外部码表两处事实漂移）。
    [Fact]
    public void EveryMember_HasNonEmptyErrorText()
    {
        var problems = new List<string>();
        foreach (var member in AllMembers())
        {
            var field = member.EnumType.GetField(member.Member);
            var text = field?.GetCustomAttribute<ErrorTextAttribute>();
            if (text is null)
            {
                problems.Add($"{member.EnumType.Name}.{member.Member}={member.Value}：缺少 [ErrorText] 中英双语文案");
            }
            else if (string.IsNullOrWhiteSpace(text.ZhCN) || string.IsNullOrWhiteSpace(text.EnUS))
            {
                problems.Add($"{member.EnumType.Name}.{member.Member}={member.Value}：[ErrorText] 中文或英文文案为空");
            }
        }

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    // C1：源码扫描禁 (XxxErrorCode)数字 强转字面量（魔法数字护栏；旧三 host ResultCode 强转已随 C264 切换消灭，不在本断言范围）。
    [Fact]
    public void SourceCode_HasNoErrorCodeCastLiterals()
    {
        var pattern = new Regex(@"\(\s*[A-Za-z]+ErrorCode\s*\)\s*-?\d");
        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            var normalized = file.Replace(Path.DirectorySeparatorChar, '/');
            if (normalized.Contains("/bin/") || normalized.Contains("/obj/") ||
                normalized.Contains("/.menu-test/") || normalized.EndsWith("ErrorCodeContractTests.cs", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var line in File.ReadAllLines(file))
            {
                if (pattern.IsMatch(line))
                {
                    violations.Add($"{normalized}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0, "发现 (XxxErrorCode)数字 强转字面量（禁止魔法数字，须直接引用枚举成员）：\n" + string.Join("\n", violations));
    }

    // R1：旧三 host 错误码枚举防回潮（C264 / GFX-670 T2）——旧文件不得复现，全仓不得残留旧引用。
    [Fact]
    public void LegacyHostResultCodeEnums_DoNotExist()
    {
        string[] legacyFiles =
        {
            "GameFrameX.Client.Api/Result/ResultCode.cs",
            "GameFrameX.Hub.Api/Result/ResultCode.cs",
            "GameFrameX.Game.Api/Result/ResultCode.cs",
            // C265（GFX-671）：Admin 旧 D 码体系三文件防回潮
            "GameFrameX.Core.Enum/ErrorCodeEnum.cs",
            "GameFrameX.Core.Enum/ErrorCodeTypeAttribute.cs",
            "GameFrameX.Core.Enum/ErrorCodeItemMetadataAttribute.cs",
        };

        foreach (var relative in legacyFiles)
        {
            Assert.False(File.Exists(Path.Combine(RepositoryRoot, relative)), $"旧三 host 错误码枚举文件不得复现：{relative}（C264 已切换为 Core 模块 enum + GameApiResult 包装）");
        }

        // 全仓不得残留旧引用：using（含别名）或裸 ResultCode 成员引用（排除本测试文件自身与 bin/obj）
        var usingPattern = new Regex(@"using\s+(?:\w+\s*=\s*)?GameFrameX\.(?:Client|Hub|Game)\.Api\.Result\s*;");
        var memberPattern = new Regex(@"\b(?:Client|Hub)?ResultCode\.");
        // C265：Admin 旧 D 码体系标识符（枚举/特性/出站 string 方法）全仓防回潮
        var adminLegacyPattern = new Regex(@"\b(?:ErrorCodeEnum|ErrorCodeTypeAttribute|ErrorCodeItemMetadataAttribute|ErrorCodeToString|ResolveErrorCode)\b");
        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(RepositoryRoot, "*.cs", SearchOption.AllDirectories))
        {
            var normalized = file.Replace(Path.DirectorySeparatorChar, '/');
            if (normalized.Contains("/bin/") || normalized.Contains("/obj/") ||
                normalized.Contains("/.menu-test/") || normalized.EndsWith("ErrorCodeContractTests.cs", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var line in File.ReadAllLines(file))
            {
                if (usingPattern.IsMatch(line) || memberPattern.IsMatch(line) || adminLegacyPattern.IsMatch(line))
                {
                    violations.Add($"{normalized}: {line.Trim()}");
                }
            }
        }

        Assert.True(violations.Count == 0, "发现旧错误码体系残留引用（C264 三 host ResultCode / C265 Admin ErrorCodeEnum 系已统一为 Core 模块 enum）：\n" + string.Join("\n", violations));
    }

    // E1：码表导出——每次测试运行把全部枚举成员的 [ErrorText] 双语文案合成仓库根 error-codes.json（数字 + 中英），
    // 供前端 locale（C262 T4）与 Unity 客户端消费；产物与 ErrorTextAttribute 单一事实来源天然同源（同一张表序列化落盘）。
    [Fact]
    public void Export_ErrorCodesJson_InSyncWithErrorTextAttributes()
    {
        var zh = ErrorLocalizer.GetCultureTable(ErrorLocalizer.DefaultCulture);
        var en = ErrorLocalizer.GetCultureTable(ErrorLocalizer.EnglishCulture);

        // 双表同源于同一 ErrorText 集合（条目数一致为导出输入自洽的最小断言）
        Assert.NotEmpty(zh);
        Assert.Equal(zh.Count, en.Count);
        Assert.True(zh.Keys.All(k => en.ContainsKey(k)));

        var export = new SortedDictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var pair in zh)
        {
            export[pair.Key] = new Dictionary<string, string>
            {
                [ErrorLocalizer.DefaultCulture] = pair.Value,
                [ErrorLocalizer.EnglishCulture] = en[pair.Key],
            };
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        var json = JsonSerializer.Serialize(export, options);
        File.WriteAllText(Path.Combine(RepositoryRoot, "error-codes.json"), json + Environment.NewLine);

        Assert.Equal(zh.Count, export.Count);
        Assert.NotEmpty(export);
    }

    // E2：Core.Extension 乐观锁冲突码 int 常量与 CommonErrorCode.ConcurrencyConflict 同源锁定
    //（Core.Extension 不引用 GameFrameX.Core，反向会成环；见 OptimisticConcurrencyMiddleware.ConflictErrorCode 注释）。
    [Fact]
    public void OptimisticConcurrencyErrorCode_MatchesCommonErrorCodeConcurrencyConflict()
    {
        Assert.Equal(Convert.ToInt32(CommonErrorCode.ConcurrencyConflict, CultureInfo.InvariantCulture), GameFrameX.Core.Extension.OptimisticConcurrencyMiddleware.ConflictErrorCode);
    }

    // 框架行为：Enum 构造 message 缺省走 ErrorLocalizer（默认中文；请求语言英文走 en-US），Code 出站数值。
    [Fact]
    public void BusinessException_EnumConstructor_ResolvesLocalizedMessageAndCode()
    {
        var original = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(ErrorLocalizer.DefaultCulture);
            var zhException = new BusinessException(AccountErrorCode.AccountOrPasswordError);
            Assert.Equal(60100001, zhException.Code);
            Assert.Equal("账号或密码错误", zhException.Message);
            Assert.Equal(AccountErrorCode.AccountOrPasswordError, Assert.IsType<AccountErrorCode>(zhException.ErrorCode));

            CultureInfo.CurrentUICulture = new CultureInfo(ErrorLocalizer.EnglishCulture);
            var enException = new BusinessException(AuthErrorCode.SignatureVerifyFailed);
            Assert.Equal(20100005, enException.Code);
            Assert.Equal("Signature verification failed", enException.Message);

            var explicitException = new BusinessException(PaymentErrorCode.PayChannelNotSupported, "显式消息优先");
            Assert.Equal(70400001, explicitException.Code);
            Assert.Equal("显式消息优先", explicitException.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = original;
        }
    }
}