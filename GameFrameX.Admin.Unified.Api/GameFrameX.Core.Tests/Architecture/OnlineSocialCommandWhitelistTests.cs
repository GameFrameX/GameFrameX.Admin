namespace GameFrameX.Core.Tests.Architecture;

// Online 社交治理（change C350 / GFX-754 / vault:C7）三条红线守护：
// ① 处罚联动命令枚举严格三值（Kick / Ban / Mute，X3 红线）——新增命令必须回 vault 评审并同步本测试；
// ② Online/Social 源码零直写——事实源（TenantGamePenaltyEntity）只经 TenantGamePenaltyEntityService.AddAsync 写入，
//    Social 域自身只读（只读 Repository 允许；任何写入 API 调用或 ISqlSugarClient 依赖直接失败）；
// ③ Online 显式命令调用点唯一——kick_player / revoke_token / mute_player 三个 action 常量与
//    OnlineLinkCommandAckData 联动命令响应类型只允许出现在 OnlinePenaltyLinkCommandService（处置流与处罚页同步均经其公共入口）。
public sealed class OnlineSocialCommandWhitelistTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string SocialSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game", "Online", "Social");

    private static readonly string ApplicationGameSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game");

    /// <summary>SqlSugar 写路径 API 标识符：Online/Social 源码出现任意一个即违反「事实源只经 EntityService 写入」边界。</summary>
    private static readonly string[] WriteApiTokens =
    {
        ".Insertable(", ".Updateable(", ".Deleteable(", ".Saveable(",
        "InsertWithDiffLog", "InsertWithIdentity", ".InsertAsync(", ".UpdateAsync(", ".DeleteAsync(",
        ".Insert(", ".Update(", ".ExecuteCommand",
    };

    /// <summary>Online 显式命令 action 常量（X3 红线白名单：新增即破坏命令面收敛）。</summary>
    private static readonly string[] OnlineCommandActionTokens = { "kick_player", "revoke_token", "mute_player", };

    [Fact]
    public void PenaltyCommandKind_IsLockedToKickBanMute()
    {
        var enumType = typeof(GameFrameX.Entity.Enum.OnlinePenaltyCommandKind);
        var expected = new Dictionary<GameFrameX.Entity.Enum.OnlinePenaltyCommandKind, int>
        {
            { GameFrameX.Entity.Enum.OnlinePenaltyCommandKind.Kick, 1 },
            { GameFrameX.Entity.Enum.OnlinePenaltyCommandKind.Ban, 2 },
            { GameFrameX.Entity.Enum.OnlinePenaltyCommandKind.Mute, 3 },
        };

        var actualNames = global::System.Enum.GetNames(enumType).Order(StringComparer.Ordinal).ToList();
        var expectedNames = expected.Keys.Select(u => u.ToString()).Order(StringComparer.Ordinal).ToList();
        Assert.True(actualNames.SequenceEqual(expectedNames),
            "OnlinePenaltyCommandKind 必须严格为 Kick/Ban/Mute 三值（X3 红线），实际:" + string.Join(",", actualNames));

        foreach (var pair in expected)
        {
            Assert.Equal(pair.Value, Convert.ToInt32(global::System.Enum.Parse(enumType, pair.Key.ToString())));
        }
    }

    [Fact]
    public void SocialSource_HasNoDirectWriteApiOrSqlSugarClientDependency()
    {
        Assert.True(Directory.Exists(SocialSourceDirectory), "Online/Social 源码目录不存在:" + SocialSourceDirectory);

        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(SocialSourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            foreach (var token in WriteApiTokens)
            {
                if (source.Contains(token, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → {token}");
                }
            }

            if (source.Contains("ISqlSugarClient", StringComparison.Ordinal))
            {
                offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → ISqlSugarClient");
            }
        }

        Assert.True(offenders.Count == 0,
            "Online/Social 源码出现直写 API 或 ISqlSugarClient 依赖（处罚事实源只经 TenantGamePenaltyEntityService.AddAsync 写入；" +
            "Social 域只读，只读 Repository 允许）。违规点:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void OnlineCommandActions_AreOnlyInvokedFromPenaltyLinkCommandService()
    {
        var allowedFile = Path.Combine(SocialSourceDirectory, "OnlinePenaltyLinkCommandService.cs");
        // 线缆契约类型自身定义文件：定义合法（类型必须落盘），只禁止「定义之外的使用」。
        var ackDataContractFile = Path.Combine(SocialSourceDirectory, "Dto", "OnlineLinkCommandAckData.cs");
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(ApplicationGameSourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (string.Equals(file, allowedFile, StringComparison.Ordinal) || string.Equals(file, ackDataContractFile, StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            foreach (var token in OnlineCommandActionTokens)
            {
                if (source.Contains(token, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → {token}");
                }
            }

            if (source.Contains("OnlineLinkCommandAckData", StringComparison.Ordinal))
            {
                offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → OnlineLinkCommandAckData");
            }
        }

        Assert.True(offenders.Count == 0,
            "Online 显式命令（kick_player/revoke_token/mute_player 或 OnlineLinkCommandAckData）出现于 OnlinePenaltyLinkCommandService 之外。" +
            "联动命令调用点唯一：处置流与处罚页同步一律经 OnlinePenaltyLinkCommandService.ExecuteLinkAsync，不得绕过直调 Online 命令接口。" +
            "违规点:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent!;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }
}
