using System.Security.Claims;
using System.Threading;
using GameFrameX.Application.System.Logging;
using GameFrameX.Application.System.Logging.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Log;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.Logging;

/// <summary>
/// 日志分页 searchKey 模糊过滤守护（C327 / GFX-737）：BaseLogService.QueryPagedAsync 按各日志服务声明的
/// <c>SearchableTextColumns</c> 多列 OR Contains 模糊匹配；空 searchKey 不变；与时间范围条件 AND 叠加。
/// </summary>
public sealed class LogPageSearchKeyFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"gfx_log_searchkey_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;
    private long _visitId;
    private long _operationId;
    private long _exceptionId;
    private long _diffId;

    public LogPageSearchKeyFilterTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(SystemLogVisitEntity),
            typeof(TenantLogVisitEntity),
            typeof(SystemLogOperationEntity),
            typeof(TenantLogOperationEntity),
            typeof(SystemLogExceptionEntity),
            typeof(TenantLogExceptionEntity),
            typeof(SystemLogDiffEntity),
            typeof(TenantLogDiffEntity));
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task VisitLog_EmptySearchKey_BehavesLikeBaseline()
    {
        await SeedVisitAsync("登录页", "alice", "10.0.0.1", "上海");
        await SeedVisitAsync("控制台", "bob", "10.0.0.2", "北京");

        var service = BuildVisitService();
        var page = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, });

        Assert.Equal(2, page.Total);
    }

    [Fact]
    public async Task VisitLog_SearchKey_MatchesMultipleColumns_OrRelation()
    {
        await SeedVisitAsync("登录页", "alice", "10.0.0.1", "上海");
        await SeedVisitAsync("控制台", "bob", "10.0.0.2", "北京");
        await SeedVisitAsync("仪表盘", "carol", "192.168.1.5", "深圳");

        var service = BuildVisitService();

        // 命中 ActionName="控制台"
        var byAction = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "控制台", });
        Assert.Single(byAction.Items);
        Assert.Equal("bob", byAction.Items.First().Account);

        // 命中 RemoteIp="192.168.1.5"
        var byIp = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "192.168", });
        Assert.Single(byIp.Items);
        Assert.Equal("carol", byIp.Items.First().Account);

        // 命中 Account="alice"
        var byAccount = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "alice", });
        Assert.Single(byAccount.Items);
        Assert.Equal("alice", byAccount.Items.First().Account);

        // 不命中任何列
        var none = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "不存在的关键字", });
        Assert.Empty(none.Items);
    }

    [Fact]
    public async Task VisitLog_SearchKey_TrimsWhitespace()
    {
        await SeedVisitAsync("登录页", "alice", "10.0.0.1", "上海");

        var service = BuildVisitService();
        var page = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "   alice   ", });

        Assert.Single(page.Items);
        Assert.Equal("alice", page.Items.First().Account);
    }

    [Fact]
    public async Task OperationLog_SearchKey_MatchesRequestUrlAndMessage()
    {
        await SeedOperationAsync("GET", "/api/tenantUser/page", "user list", "ok");
        await SeedOperationAsync("POST", "/api/tenantUser/add", "user create", "ok");
        await SeedOperationAsync("GET", "/api/globalDictionary/page", "dict list", "ok");

        var service = BuildOperationService();

        // 命中 RequestUrl
        var byUrl = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "tenantUser", });
        Assert.Equal(2, byUrl.Total);

        // 命中 Message
        var byMessage = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "dict list", });
        Assert.Single(byMessage.Items);
    }

    [Fact]
    public async Task ExceptionLog_SearchKey_MatchesSameColumns()
    {
        await SeedExceptionAsync("GET", "/api/gameOrder/submit", "biz error: order amount invalid");
        await SeedExceptionAsync("POST", "/api/gamePayment/recharge", "stack overflow at GamePaymentService");

        var service = BuildExceptionService();

        var byMessage = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "stack overflow", });
        Assert.Single(byMessage.Items);

        var byUrl = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "submit", });
        Assert.Single(byUrl.Items);
    }

    [Fact]
    public async Task DiffLog_SearchKey_MatchesDiffTypeAndSql_ExcludesBigString()
    {
        await SeedDiffAsync(diffType: "Insert", sql: "INSERT INTO foo VALUES (1)", beforeData: "{\"old\":1}");
        await SeedDiffAsync(diffType: "Update", sql: "UPDATE foo SET a=1", beforeData: "contains-keyword-in-beforedata");

        var service = BuildDiffService();

        // 命中 DiffType
        var byDiffType = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "Insert", });
        Assert.Single(byDiffType.Items);

        // 命中 Sql
        var bySql = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "UPDATE", });
        Assert.Single(bySql.Items);

        // 不命中 BigString（BeforeData 即使包含关键字也不应返回，排除验证）
        var notMatched = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 20, SearchKey = "beforedata", });
        Assert.Empty(notMatched.Items);
    }

    [Fact]
    public async Task SearchKey_CombinesWithTimeRange_AsAndCondition()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new SystemLogVisitEntity
        {
            Id = Interlocked.Increment(ref _visitId),
            DisplayTitle = "old hit", Account = "alice", ActionName = "X", CreatedTime = now - 10_000,
        }).ExecuteCommandAsync();
        await _db.Insertable(new SystemLogVisitEntity
        {
            Id = Interlocked.Increment(ref _visitId),
            DisplayTitle = "new hit", Account = "alice", ActionName = "Y", CreatedTime = now + 10_000,
        }).ExecuteCommandAsync();
        await _db.Insertable(new SystemLogVisitEntity
        {
            Id = Interlocked.Increment(ref _visitId),
            DisplayTitle = "old miss", Account = "bob", ActionName = "Z", CreatedTime = now - 10_000,
        }).ExecuteCommandAsync();

        var service = BuildVisitService();
        // searchKey="hit" + 时间范围(只保留最近) => 仅 new hit
        var page = await service.PageAsync(new PageLogRequest
        {
            Page = 1,
            PageSize = 20,
            SearchKey = "hit",
            StartTime = now,
            EndTime = now + 86_400,
        });

        Assert.Single(page.Items);
        Assert.Equal("alice", page.Items.First().Account);
        Assert.Equal("Y", page.Items.First().ActionName);
    }

    // ---------- helpers ----------

    private async Task SeedVisitAsync(string displayTitle, string account, string remoteIp, string location)
    {
        var id = Interlocked.Increment(ref _visitId);
        await _db.Insertable(new SystemLogVisitEntity
        {
            Id = id,
            DisplayTitle = displayTitle,
            Account = account,
            RealName = account,
            ActionName = displayTitle,
            RemoteIp = remoteIp,
            Location = location,
            Browser = "Chrome",
            Os = "Linux",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
    }

    private async Task SeedOperationAsync(string httpMethod, string requestUrl, string message, string status)
    {
        var id = Interlocked.Increment(ref _operationId);
        await _db.Insertable(new SystemLogOperationEntity
        {
            Id = id,
            HttpMethod = httpMethod,
            RequestUrl = requestUrl,
            Message = message,
            Status = status,
            Account = "tester",
            ActionName = "test",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
    }

    private async Task SeedExceptionAsync(string httpMethod, string requestUrl, string message)
    {
        var id = Interlocked.Increment(ref _exceptionId);
        await _db.Insertable(new SystemLogExceptionEntity
        {
            Id = id,
            HttpMethod = httpMethod,
            RequestUrl = requestUrl,
            Message = message,
            Exception = message,
            Account = "tester",
            ActionName = "test",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
    }

    private async Task SeedDiffAsync(string diffType, string sql, string beforeData)
    {
        var id = Interlocked.Increment(ref _diffId);
        await _db.Insertable(new SystemLogDiffEntity
        {
            Id = id,
            DiffType = diffType,
            Sql = sql,
            BeforeData = beforeData,
            AfterData = "after",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
    }

    private LogVisitService BuildVisitService()
    {
        var systemRepo = new SqlSugarRepository<SystemLogVisitEntity> { Context = _db };
        var tenantRepo = new SqlSugarRepository<TenantLogVisitEntity> { Context = _db };
        return new LogVisitService(systemRepo, tenantRepo, BuildSuperAdminContext());
    }

    private LogOperationService BuildOperationService()
    {
        var systemRepo = new SqlSugarRepository<SystemLogOperationEntity> { Context = _db };
        var tenantRepo = new SqlSugarRepository<TenantLogOperationEntity> { Context = _db };
        return new LogOperationService(systemRepo, tenantRepo, BuildSuperAdminContext());
    }

    private LogExceptionService BuildExceptionService()
    {
        var systemRepo = new SqlSugarRepository<SystemLogExceptionEntity> { Context = _db };
        var tenantRepo = new SqlSugarRepository<TenantLogExceptionEntity> { Context = _db };
        return new LogExceptionService(systemRepo, tenantRepo, BuildSuperAdminContext());
    }

    private LogDiffService BuildDiffService()
    {
        var systemRepo = new SqlSugarRepository<SystemLogDiffEntity> { Context = _db };
        var tenantRepo = new SqlSugarRepository<TenantLogDiffEntity> { Context = _db };
        return new LogDiffService(systemRepo, tenantRepo, BuildSuperAdminContext());
    }

    /// <summary>
    /// 构造 SuperAdmin CurrentUserContext，让 BaseLogService 走系统日志（system_log_*）主库路径。
    /// </summary>
    private static CurrentUserContext BuildSuperAdminContext()
    {
        var claims = new[]
        {
            new Claim(ClaimConst.AccountType, ((int)AccountTypeEnum.SuperAdmin).ToString()),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal, };
        return new CurrentUserContext(new HttpContextAccessor { HttpContext = httpContext, });
    }
}
