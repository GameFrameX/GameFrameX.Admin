namespace GameFrameX.Core.Tests.Architecture;

// C344 / GFX-760（vault:C9 S8.1/S8.2）Online 控制台四条红线守护：
// ① X6 口径：Online/Console 全域源码 + OnlineOverviewQueryService 依赖闭包零引用管理员连接链路
//    （OnlineUserHub / TenantOnlineUserService / TenantOnlineUserEntity / GameHomeOnlineCounter）——
//    玩家在线数据唯一来源是 Online 玩家侧查询 API，总览不得并入管理员浏览器连接，也不得复用
//    首页驾驶舱 24h 在线时序（GameHomeOnlineCounter，C321 产物，口径不同源）；
// ② 受控命令枚举严格四值（CancelTicket / EndAbnormalMatch / SuspendResumePlayer / MatchIsolation）——
//    新增命令必须回 vault 评审并同步本测试；
// ③ 复用矩阵无平行实现：Online/Console 源码零出现既有命令 token（踢下线/吊销Token/禁言/发放/撤销资产），
//    这些命令由 C350 OnlinePenaltyLinkCommandService 与 C340 OnlineAssetGrantCommandService 既有端点承载；
// ④ 四新命令 action 名只允许出现在 OnlineControlledCommandService（单一实现点，全域文本扫描）。
public sealed class OnlineOverviewIsolationTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string ConsoleSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game", "Online", "Console");

    private static readonly string ApplicationGameSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game");

    /// <summary>X6 红线：管理员连接链路标识（总览玩家口径不得混入的四个类型名）。</summary>
    private static readonly string[] AdministratorConnectionTokens =
    {
        "OnlineUserHub", "TenantOnlineUserService", "TenantOnlineUserEntity", "GameHomeOnlineCounter",
    };

    /// <summary>既有受控命令 token（复用矩阵：Console 域出现任意一个即构成平行实现）。</summary>
    private static readonly string[] LegacyCommandTokens =
    {
        "kick_player", "revoke_token", "mute_player", "grant_asset", "revoke_asset",
    };

    /// <summary>四新命令 action 名（只允许出现在 OnlineControlledCommandService.cs）。</summary>
    private static readonly string[] NewControlledCommandTokens =
    {
        "cancel_match_ticket", "end_abnormal_match", "suspend_resume_player", "match_isolation",
    };

    [Fact]
    public void ConsoleDirectory_ExistsAndHasSources()
    {
        Assert.True(Directory.Exists(ConsoleSourceDirectory), "Application.Game/Online/Console/ 目录不存在（被改名或删除须回 change 评审）");

        var sourceFiles = Directory.EnumerateFiles(ConsoleSourceDirectory, "*.cs", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(sourceFiles);
    }

    [Fact]
    public void ConsoleSources_HaveNoAdministratorConnectionReferences()
    {
        var offenders = ScanFilesForTokens(
            Directory.EnumerateFiles(ConsoleSourceDirectory, "*.cs", SearchOption.AllDirectories),
            AdministratorConnectionTokens);
        Assert.True(offenders.Count == 0,
            "Online/Console 源码出现管理员连接链路引用（X6：玩家在线数据唯一来源是 Online 玩家侧查询 API，" +
            "总览不得计入 OnlineUserHub/TenantOnlineUserService 管理员连接，也不得复用 GameHomeOnlineCounter 驾驶舱口径）。违规点:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void OverviewService_DependencyClosure_HasNoAdministratorConnectionReferences()
    {
        // 依赖闭包：OnlineOverviewQueryService 自身 + 其构造函数参数类型（及参数类型自身的构造参数，
        // 限 Application.Game 程序集、visited 防环）。Foundation/Core/Entity 层框架类型不进闭包——
        // X6 只关心 Admin 业务层是否绕道管理员链路取数。
        var overviewServiceType = typeof(GameFrameX.Application.Game.Online.Console.OnlineOverviewQueryService);
        var closure = new HashSet<Type> { overviewServiceType, };
        var pending = new Queue<Type>(overviewServiceType.GetConstructors().SelectMany(u => u.GetParameters()).Select(u => u.ParameterType));

        while (pending.Count > 0)
        {
            var parameterType = pending.Dequeue();
            var elementType = parameterType.IsArray ? parameterType.GetElementType()! : parameterType;
            if (elementType.IsGenericType)
            {
                elementType = elementType.GetGenericArguments()[0];
            }

            if (!closure.Add(elementType))
            {
                continue;
            }

            if (elementType.Assembly != typeof(GameFrameX.Application.Game.Online.Console.OnlineOverviewQueryService).Assembly)
            {
                continue;
            }

            foreach (var constructor in elementType.GetConstructors())
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    pending.Enqueue(parameter.ParameterType);
                }
            }
        }

        var offenders = new List<string>();
        foreach (var closedType in closure)
        {
            if (closedType.IsInterface || closedType.IsEnum || !closedType.Name.Contains('.'))
            {
                // 接口/枚举无构造依赖，其定义文件随具体类型/使用方文件一并扫描；这里只扫有源文件的类型。
            }

            var sourceFile = LocateTypeSourceFile(closedType);
            if (sourceFile == null)
            {
                continue;
            }

            offenders.AddRange(ScanFileForTokens(sourceFile, AdministratorConnectionTokens, closedType.Name));
        }

        Assert.True(offenders.Count == 0,
            "OnlineOverviewQueryService 依赖闭包内出现管理员连接链路引用（X6）。违规点:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void ControlledCommandKind_IsLockedToFourValues()
    {
        var enumType = typeof(GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind);
        var expected = new Dictionary<GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind, int>
        {
            { GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind.CancelTicket, 1 },
            { GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind.EndAbnormalMatch, 2 },
            { GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind.SuspendResumePlayer, 3 },
            { GameFrameX.Application.Game.Online.Console.Dto.OnlineControlledCommandKind.MatchIsolation, 4 },
        };

        var actualNames = global::System.Enum.GetNames(enumType).Order(StringComparer.Ordinal).ToList();
        var expectedNames = expected.Keys.Select(u => u.ToString()).Order(StringComparer.Ordinal).ToList();
        Assert.True(actualNames.SequenceEqual(expectedNames),
            "OnlineControlledCommandKind 必须严格为 CancelTicket/EndAbnormalMatch/SuspendResumePlayer/MatchIsolation 四值" +
            "（复用矩阵：踢下线/封禁/禁言/发放/撤销资产由 C350/C340 既有端点承载，不得并入本枚举），实际:" + string.Join(",", actualNames));

        foreach (var pair in expected)
        {
            Assert.Equal(pair.Value, Convert.ToInt32(global::System.Enum.Parse(enumType, pair.Key.ToString())));
        }
    }

    [Fact]
    public void ConsoleSources_HaveNoLegacyCommandParallelImplementation()
    {
        var offenders = ScanFilesForTokens(
            Directory.EnumerateFiles(ConsoleSourceDirectory, "*.cs", SearchOption.AllDirectories),
            LegacyCommandTokens);
        Assert.True(offenders.Count == 0,
            "Online/Console 源码出现既有受控命令 token（复用矩阵禁平行实现：踢下线/封禁/禁言经 C350 OnlinePenaltyLinkCommandService，" +
            "发放/撤销资产经 C340 OnlineAssetGrantCommandService，控制台面板直调既有端点，本域不得重新实现）。违规点:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void NewControlledCommandActions_AreOnlyInvokedFromControlledCommandService()
    {
        var allowedFile = Path.Combine(ConsoleSourceDirectory, "OnlineControlledCommandService.cs");
        var offenders = ScanFilesForTokens(
            Directory.EnumerateFiles(ApplicationGameSourceDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(u => !string.Equals(u, allowedFile, StringComparison.Ordinal)),
            NewControlledCommandTokens);
        Assert.True(offenders.Count == 0,
            "四新命令 action 名出现于 OnlineControlledCommandService 之外（单一实现点，新增调用方须复用该 Service）。违规点:" +
            Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    private static List<string> ScanFilesForTokens(IEnumerable<string> files, string[] tokens)
    {
        var offenders = new List<string>();
        foreach (var file in files)
        {
            offenders.AddRange(ScanFileForTokens(file, tokens, null));
        }

        return offenders;
    }

    private static List<string> ScanFileForTokens(string file, string[] tokens, string? typeName)
    {
        var offenders = new List<string>();
        var source = File.ReadAllText(file);
        foreach (var token in tokens)
        {
            if (source.Contains(token, StringComparison.Ordinal))
            {
                var location = typeName ?? Path.GetRelativePath(RepositoryRoot, file);
                offenders.Add($"{location} → {token}");
            }
        }

        return offenders;
    }

    /// <summary>
    /// 在 Application.Game 目录下按「类型名.cs」定位类型的源文件（Dto/ 目录一类型一文件约定）。
    /// 定位不到（嵌套类型/编译生成/其他程序集）返回 null，由调用方跳过。
    /// </summary>
    private static string? LocateTypeSourceFile(Type type)
    {
        if (type.IsInterface || type.IsEnum || type.IsPrimitive || type == typeof(string) || type == typeof(object))
        {
            return null;
        }

        var candidates = Directory.EnumerateFiles(ApplicationGameSourceDirectory, type.Name + ".cs", SearchOption.AllDirectories).ToList();
        return candidates.Count == 1 ? candidates[0] : null;
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
