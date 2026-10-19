namespace GameFrameX.Core.Tests.Architecture;

// Online 资产域（change C340 / GFX-752 / vault:C4）三条红线守护：
// ① Online/Asset 源码零数据面——资产事实全部以 Online Runtime 为唯一权威，Admin 侧不落任何资产镜像；
//    查询/命令一律经 IOnlineServerClient 透传，域内源码不得出现任何 SqlSugar 仓储/客户端/写路径 API；
// ② 发放/撤销命令 action 唯一入口——grant_asset / revoke_asset 两个 action 常量只允许出现在
//    OnlineAssetGrantCommandService（高危权限码 onlineAssetGrantCommand:grant/:revoke 与之配套）；
// ③ 查询 action 归位——query_wallet_ledger / query_transaction_detail / query_asset_audit 只允许出现在各自查询 Service。
public sealed class OnlineAssetCommandWhitelistTests
{
    private static readonly string RepositoryRoot = GetRepositoryRoot();

    private static readonly string AssetSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game", "Online", "Asset");

    private static readonly string ApplicationGameSourceDirectory = Path.Combine(
        RepositoryRoot, "GameFrameX.Application.Game");

    /// <summary>SqlSugar 数据面标识符：Online/Asset 源码出现任意一个即违反「Admin 侧零资产数据面」边界（X3 红线）。</summary>
    private static readonly string[] DataPlaneTokens =
    {
        ".Insertable(", ".Updateable(", ".Deleteable(", ".Saveable(",
        "InsertWithDiffLog", "InsertWithIdentity", ".InsertAsync(", ".UpdateAsync(", ".DeleteAsync(",
        ".Insert(", ".Update(", ".ExecuteCommand",
        "ISqlSugarClient", "SqlSugarRepository",
    };

    /// <summary>资产命令 action 常量（高危白名单：新增命令必须回 vault 评审并同步本测试）。</summary>
    private static readonly string[] CommandActionTokens = { "grant_asset", "revoke_asset", };

    /// <summary>查询 action → 唯一允许出现的源文件相对路径。</summary>
    private static readonly (string Action, string RelativeFile)[] QueryActionWhitelist =
    {
        ("query_wallet_ledger", "GameFrameX.Application.Game/Online/Asset/OnlineAssetLedgerQueryService.cs"),
        ("query_transaction_detail", "GameFrameX.Application.Game/Online/Asset/OnlineAssetTransactionQueryService.cs"),
        ("query_asset_audit", "GameFrameX.Application.Game/Online/Asset/OnlineAssetAuditQueryService.cs"),
    };

    [Fact]
    public void AssetSource_HasNoDataPlaneAccess()
    {
        Assert.True(Directory.Exists(AssetSourceDirectory), "Online/Asset 源码目录不存在:" + AssetSourceDirectory);

        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(AssetSourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(file);
            foreach (var token in DataPlaneTokens)
            {
                if (source.Contains(token, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → {token}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "Online/Asset 源码出现 SqlSugar 数据面访问（资产以 Online Runtime 为唯一权威，Admin 侧零镜像零直写；" +
            "查询/命令一律经 IOnlineServerClient 透传）。违规点:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void CommandActions_AreOnlyInvokedFromGrantCommandService()
    {
        var allowedFile = Path.Combine(AssetSourceDirectory, "OnlineAssetGrantCommandService.cs");
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(ApplicationGameSourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (string.Equals(file, allowedFile, StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            foreach (var token in CommandActionTokens)
            {
                if (source.Contains(token, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → {token}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "Online 资产命令（grant_asset/revoke_asset）出现于 OnlineAssetGrantCommandService 之外。" +
            "人工发放/撤销的唯一入口是 OnlineAssetGrantCommandService（高危权限码与之配套），不得绕过直调 Online 命令接口。" +
            "违规点:" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void QueryActions_AreOnlyInvokedFromTheirOwnQueryServices()
    {
        var offenders = new List<string>();
        foreach (var pair in QueryActionWhitelist)
        {
            var allowedFile = Path.Combine(RepositoryRoot, pair.RelativeFile.Replace('/', Path.DirectorySeparatorChar));
            foreach (var file in Directory.EnumerateFiles(ApplicationGameSourceDirectory, "*.cs", SearchOption.AllDirectories))
            {
                if (string.Equals(file, allowedFile, StringComparison.Ordinal))
                {
                    continue;
                }

                if (File.ReadAllText(file).Contains(pair.Action, StringComparison.Ordinal))
                {
                    offenders.Add($"{Path.GetRelativePath(RepositoryRoot, file)} → {pair.Action}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "资产查询 action 出现于其归属查询 Service 之外（查询端点与 §3 端点表一一对应，不得旁路复用）。" +
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
