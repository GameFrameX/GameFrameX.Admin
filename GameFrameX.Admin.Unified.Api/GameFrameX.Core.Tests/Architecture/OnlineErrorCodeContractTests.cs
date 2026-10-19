using System.Globalization;
using System.Reflection;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Localization.Errors;

namespace GameFrameX.Core.Tests.Architecture;

// C339 / GFX-750：Online 横切契约八段镜像错误码三方对照守护。
// 权威依据：vault gfx-doc/dev/online-platform/spec.md L105 八段分段定义（协议段 Nxxx ↔ Admin 镜像段 32(N-1)），
// 镜像成员详表快照 = gfx-doc/dev/online-platform/contracts/error-code-table.md（vault 详表落盘前由本仓首版拟定，
// 经 2026-09-13 项目负责人评审，落盘后 vault 表为唯一权威，本测试内嵌快照须随 vault 同步，漂移即红）。
// 三方对照（任何一侧改动不同步另两侧即失败）：
//   A OnlineErrorCode 枚举（[ErrorText] 声明即数据）
//   B ErrorLocalizer 运行时码表（GetCultureTable 现算，error-codes.json 导出同源）
//   C vault 表快照（本测试内嵌常量；八段分段表 + 成员详表）
public sealed class OnlineErrorCodeContractTests
{
    /// <summary>
    /// vault 八段分段表快照行（嵌套类型遵守 one-type-per-file 例外）。
    /// </summary>
    private sealed class VaultSegmentRow
    {
        public VaultSegmentRow(int protocolSegment, int mirrorSegment, string segmentName)
        {
            ProtocolSegment = protocolSegment;
            MirrorSegment = mirrorSegment;
            SegmentName = segmentName;
        }

        public int ProtocolSegment { get; }

        public int MirrorSegment { get; }

        public string SegmentName { get; }
    }

    /// <summary>
    /// vault 成员详表快照行。
    /// </summary>
    private sealed class VaultMemberRow
    {
        public VaultMemberRow(string member, int value)
        {
            Member = member;
            Value = value;
        }

        public string Member { get; }

        public int Value { get; }
    }

    /// <summary>
    /// 枚举成员描述（成员名 + 8 位镜像码）。
    /// </summary>
    private sealed class MirrorMemberRow
    {
        public MirrorMemberRow(string member, int value)
        {
            Member = member;
            Value = value;
        }

        public string Member { get; }

        public int Value { get; }
    }

    /// <summary>
    /// vault 八段分段表快照；「协议段 Nxxx ↔ 镜像段 32(N-1)」结构关系由断言锁定。
    /// </summary>
    private static readonly VaultSegmentRow[] VaultSegmentTable =
    {
        new VaultSegmentRow(1000, 320, "系统与网络"),
        new VaultSegmentRow(2000, 321, "鉴权与 Session"),
        new VaultSegmentRow(3000, 322, "租户、App、Server 作用域"),
        new VaultSegmentRow(4000, 323, "参数与资源"),
        new VaultSegmentRow(5000, 324, "业务状态"),
        new VaultSegmentRow(6000, 325, "幂等与冲突"),
        new VaultSegmentRow(7000, 326, "限流、安全和风控"),
        new VaultSegmentRow(8000, 327, "临时失败与可重试"),
    };

    /// <summary>
    /// vault 成员详表快照（error-code-table.md 镜像；成员名 + 8 位镜像码逐条对照，多 / 少 / 改数值均红）。
    /// </summary>
    private static readonly VaultMemberRow[] VaultMemberTable =
    {
        // 段 320 ← 协议段 1xxx
        new VaultMemberRow("InternalError", 32000001),
        new VaultMemberRow("NetworkTimeout", 32000002),
        new VaultMemberRow("NetworkError", 32000003),
        // 段 321 ← 协议段 2xxx
        new VaultMemberRow("TokenExpired", 32100001),
        new VaultMemberRow("TokenRevoked", 32100002),
        new VaultMemberRow("SessionInvalid", 32100003),
        // 段 322 ← 协议段 3xxx
        new VaultMemberRow("ScopeDenied", 32200001),
        new VaultMemberRow("CrossTenantDenied", 32200002),
        new VaultMemberRow("CrossAppDenied", 32200003),
        new VaultMemberRow("ServerScopeDenied", 32200004),
        new VaultMemberRow("ScopeMissing", 32200005),
        // 段 323 ← 协议段 4xxx
        new VaultMemberRow("ParameterInvalid", 32300001),
        new VaultMemberRow("ResourceNotFound", 32300002),
        // 段 324 ← 协议段 5xxx
        new VaultMemberRow("StateNotReady", 32400001),
        new VaultMemberRow("StateEnded", 32400002),
        new VaultMemberRow("StateOperationForbidden", 32400003),
        // 段 325 ← 协议段 6xxx
        new VaultMemberRow("DuplicateRequest", 32500001),
        new VaultMemberRow("VersionConflict", 32500002),
        // 段 326 ← 协议段 7xxx
        new VaultMemberRow("RateLimitExceeded", 32600001),
        new VaultMemberRow("RiskControlRejected", 32600002),
        new VaultMemberRow("AccountBanned", 32600003),
        // 段 327 ← 协议段 8xxx
        new VaultMemberRow("ServiceBusy", 32700001),
        new VaultMemberRow("DependencyUnavailable", 32700002),
    };

    private static List<MirrorMemberRow> EnumMembers()
    {
        var members = new List<MirrorMemberRow>();
        foreach (var value in global::System.Enum.GetValues(typeof(OnlineErrorCode)).Cast<object>())
        {
            members.Add(new MirrorMemberRow(value.ToString() ?? string.Empty, Convert.ToInt32(value, CultureInfo.InvariantCulture)));
        }

        return members;
    }

    // A ↔ C（分段表）：枚举实际承载的镜像段集合与 vault 八段表一一对应，协议段 N ↔ 镜像段 32(N-1) 结构关系自洽。
    [Fact]
    public void MirrorSegments_MatchVaultSegmentTable()
    {
        var actualSegments = EnumMembers().Select(m => m.Value / 100000).Distinct().OrderBy(s => s).ToList();
        var expectedSegments = VaultSegmentTable.Select(t => t.MirrorSegment).OrderBy(s => s).ToList();

        Assert.Equal(expectedSegments, actualSegments);

        foreach (var row in VaultSegmentTable)
        {
            // 「协议段 Nxxx ↔ 镜像段 32(N-1)」结构断言：1xxx→320、8xxx→327，关系不可手改。
            Assert.Equal(row.ProtocolSegment / 1000 - 1, row.MirrorSegment - 320);
            Assert.NotEmpty(row.SegmentName);
        }
    }

    // A ↔ C（成员详表）：成员名 + 数值逐条对照 vault 快照；多出、缺失、数值漂移均失败。
    [Fact]
    public void Members_MatchVaultMemberTable()
    {
        var actual = EnumMembers().OrderBy(m => m.Value).ToList();
        var expected = VaultMemberTable.OrderBy(t => t.Value).ToList();

        Assert.Equal(expected.Count, actual.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].Member, actual[i].Member);
            Assert.Equal(expected[i].Value, actual[i].Value);
        }
    }

    // A ↔ B：ErrorLocalizer 运行时码表（zh-CN / en-US）覆盖全部镜像成员，且文案与 [ErrorText] 声明逐字一致。
    [Fact]
    public void LocalizerTable_CoversAllMirrorMembers_WithErrorTextContent()
    {
        var zh = ErrorLocalizer.GetCultureTable(ErrorLocalizer.DefaultCulture);
        var en = ErrorLocalizer.GetCultureTable(ErrorLocalizer.EnglishCulture);

        var problems = new List<string>();
        foreach (var field in typeof(OnlineErrorCode).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = Convert.ToInt32(field.GetValue(null), CultureInfo.InvariantCulture);
            var text = field.GetCustomAttribute<ErrorTextAttribute>();
            Assert.NotNull(text);

            var key = value.ToString(CultureInfo.InvariantCulture);
            if (!zh.TryGetValue(key, out var zhText) || zhText != text!.ZhCN)
            {
                problems.Add($"{field.Name}={value}：zh-CN 码表缺失或文案与 [ErrorText] 不一致（表={zh.GetValueOrDefault(key) ?? "<缺失>"}，声明={text.ZhCN}）");
            }

            if (!en.TryGetValue(key, out var enText) || enText != text.EnUS)
            {
                problems.Add($"{field.Name}={value}：en-US 码表缺失或文案与 [ErrorText] 不一致（表={en.GetValueOrDefault(key) ?? "<缺失>"}，声明={text.EnUS}）");
            }
        }

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    // 八段自洽：镜像段 320～327 连续无缺；每段首成员 = 段号 × 100000 + 1；段内序号连续递增无跳号（空段即红）。
    [Fact]
    public void MirrorSegments_AreDense_AndMemberNumbersSequentialWithinSegment()
    {
        var members = EnumMembers();

        for (var segment = 320; segment <= 327; segment++)
        {
            var inSegment = members.Where(m => m.Value / 100000 == segment).OrderBy(m => m.Value).ToList();
            Assert.NotEmpty(inSegment);

            for (var i = 0; i < inSegment.Count; i++)
            {
                Assert.Equal(segment * 100000 + 1 + i, inSegment[i].Value);
            }
        }

        // 八段之外不承载任何成员（对照 VaultSegmentTable 的段集合，防第九段悄悄出现）。
        Assert.Equal(8, VaultSegmentTable.Length);
        Assert.All(members, m => Assert.InRange(m.Value / 100000, 320, 327));
    }
}
