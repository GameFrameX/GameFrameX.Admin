using GameFrameX.Web.Api.Handlers;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class ApiEndpointCatalogSyncTests
{
    [Fact]
    public void StartAsync_ReloadsMenuSeedCatalogAfterEndpointSyncBeforeCacheWarmup()
    {
        var sourceFile = FindSourceFile(nameof(ApiEndpointCatalogSync));
        var source = File.ReadAllText(sourceFile.FullName);
        var reloadCatalog = source.IndexOf("MenuSeedCatalog.LoadFromDb(menuRepository.Context);", StringComparison.Ordinal);
        var warmCache = source.IndexOf("await WarmEndpointPermissionCacheAsync(endpointRepository, bindingRepository, cache);", StringComparison.Ordinal);

        Assert.True(reloadCatalog >= 0);
        Assert.True(warmCache >= 0);
        Assert.True(reloadCatalog < warmCache);
    }

    [Fact]
    public void BindingMigration_CoversParameterizedRouteNames()
    {
        var sourceFile = FindSourceFile(nameof(ApiEndpointCatalogSync));
        var source = File.ReadAllText(sourceFile.FullName);

        Assert.Contains("routePath + \"/{\"", source, StringComparison.Ordinal);
        Assert.Contains("StartsWith(parameterizedPrefix", source, StringComparison.Ordinal);
    }

    [Fact]
    public void StartupSync_DoesNotRewriteTenantRoleMenuGrants()
    {
        var sourceFile = FindSourceFile(nameof(ApiEndpointCatalogSync));
        var source = File.ReadAllText(sourceFile.FullName);

        Assert.DoesNotContain("TenantRoleMenuEntity", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SyncTenantDefaultRoleMenusAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildRoleMenus", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ReconcileEndpoints_ClearsQueryFilterAndRevivesSoftDeletedRows()
    {
        var sourceFile = FindSourceFile(nameof(ApiEndpointCatalogSync));
        var source = File.ReadAllText(sourceFile.FullName);

        // 对账必须临时清除查询过滤器（ISafeDeletedFilter/ISafeEnabledFilter）：否则活端点匹配不到
        // 被软删除/禁用的同键物理行 → 走插入分支 → 撞全表唯一约束 ux_global_api_endpoint_method_route，
        // 事务回滚导致启动 fail-fast。repository.Context 前缀限定到对账方法本身（menuRepository/bindingRepository 另有清过滤器）。
        Assert.Contains("repository.Context.QueryFilter.ClearAndBackup()", source, StringComparison.Ordinal);
        Assert.Contains("repository.Context.QueryFilter.Restore()", source, StringComparison.Ordinal);

        // 匹配到软删除行必须复活：否则运行期缓存查询（带过滤器）仍看不到该端点，授权漏口。
        Assert.Contains("if (row.IsDeleted == true)", source, StringComparison.Ordinal);
        Assert.Contains("row.IsDeleted = false;", source, StringComparison.Ordinal);
    }

    private static FileInfo FindSourceFile(string typeName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidates = Directory.EnumerateFiles(directory.FullName, typeName + ".cs", SearchOption.AllDirectories)
                                      .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                      .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                      .Select(u => new FileInfo(u))
                                      .ToArray();
            if (candidates.Length == 1)
            {
                return candidates[0];
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"找不到 {typeName}.cs。");
    }
}