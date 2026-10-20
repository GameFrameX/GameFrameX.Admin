using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.Tests.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.AppScope;

/// <summary>
/// <see cref="AppScopeQueryFilter.ApplyAppIdFilter{T}"/> 列名映射回归（issue #3）：
/// 成员选择器必须在调用方以具体实体 lambda 写出（<c>u =&gt; u.AppId</c>），经 helper 组合后 SQL 应与手写
/// <c>Where(u =&gt; u.AppId == 局部变量)</c> 同构（<c>"app_id" = @AppId0</c>）。
/// 历史缺陷：泛型方法内直写 <c>u.AppId</c> 绑定 <see cref="GameFrameX.Entity.Base.IAppScoped.AppId"/> 接口成员，
/// SqlSugar 查不到实体列映射按裸名折叠小写生成 <c>"appid"</c> → PostgreSQL 42703、密钥/邮件配置分页全 500。
/// <para>必须连真实 PostgreSQL（SQLite provider 表达式翻译与生产不一致，见 <see cref="PostgresTestDatabase"/> 头注）。</para>
/// </summary>
public class AppScopeQueryFilterMappingTests : IDisposable
{
    private readonly PostgresTestDatabase? _db;

    public AppScopeQueryFilterMappingTests()
    {
        PostgresTestDatabase.TryCreate(out var db, out _);
        _db = db;
    }

    [SkippableFact]
    public void ApplyAppIdFilter_WithSelector_GeneratesSnakeColumnEquality()
    {
        if (_db == null)
        {
            Skip.IfNot(false, "PostgreSQL unavailable");
            return;
        }

        var config = _db.BuildDbConfig("app-scope-filter", enableSeed: false);
        using var scope = new SqlSugarScope(config);
        var conn = scope.GetConnectionScope("app-scope-filter");
        conn.CodeFirst.InitTables(typeof(TenantSessionKeyEntity));

        var logs = new List<string>();
        conn.Aop.OnLogExecuting = (sql, _) => logs.Add(sql);

        // 对照：手写具体类型谓词（rollout 等正常服务形态）
        var controlAppId = 1300000000001L;
        conn.Queryable<TenantSessionKeyEntity>().Where(u => u.AppId == controlAppId).ToList();

        // 被测：helper 组合选择器
        conn.Queryable<TenantSessionKeyEntity>().ApplyAppIdFilter(1300000000001, u => u.AppId).ToList();

        Assert.Equal(2, logs.Count);
        var whereClauses = logs.Select(ExtractWhere).ToList();

        // 两者 WHERE 子句逐字同构（参数名一致）
        Assert.Equal(whereClauses[0], whereClauses[1]);

        // 命中蛇形实体列映射，不得出现接口裸名折叠的 "appid"
        Assert.Contains("\"app_id\" =", whereClauses[1], StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("appid\"", whereClauses[1].Replace("\"app_id\"", string.Empty), StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public void ApplyAppIdFilter_NullAppId_AppliesNoAppFilter()
    {
        if (_db == null)
        {
            Skip.IfNot(false, "PostgreSQL unavailable");
            return;
        }

        var config = _db.BuildDbConfig("app-scope-filter-null", enableSeed: false);
        using var scope = new SqlSugarScope(config);
        var conn = scope.GetConnectionScope("app-scope-filter-null");
        conn.CodeFirst.InitTables(typeof(TenantSessionKeyEntity));

        var logs = new List<string>();
        conn.Aop.OnLogExecuting = (sql, _) => logs.Add(sql);

        conn.Queryable<TenantSessionKeyEntity>().ApplyAppIdFilter(null, u => u.AppId).ToList();

        // null 三态语义：不附加应用过滤条件
        Assert.DoesNotContain("app_id", ExtractWhere(logs.Single()), StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose() => _db?.Dispose();

    private static string ExtractWhere(string sql)
    {
        var index = sql.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);
        return index < 0 ? string.Empty : sql[index..];
    }
}
