using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Const;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

// S3/C401 断链修复点
namespace GameFrameX.Core.Tests.ServerManager.Online;

// S3/C401 断链修复点
public sealed class OnlineScopeContextFactoryTests : IDisposable
{
    private const long TenantA = 1001;
    private const long App100 = 100;
    private const long App200 = 200;
    private const long Area1001 = 2001; // 归属 App100
    private const long Area2001 = 3001; // 归属 App200（构造跨应用区服场景）

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_scope_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineScopeContextFactoryTests()
    {
        // 专属 ConfigId（C339）：SqlSugar 实体信息（含表名映射）按连接配置静态缓存且进程级共享；
        // 不设 ConfigId 会与裸配置 fixture（AdvancedQueryAreaFilterTests / TenantGameOrderShipServiceTests，
        // PascalCase 表名）互抢默认缓存键，后到者 InitTables no-op → insert 报 no such table
        //（AdvancedQueryAreaFilterTests ctor 注释记录的同类坑，ClientApi/Home 系靠租户 ConfigId 天然隔离）。
        // 专属键经 List 构造保留 ConfigId（同 ISqlSugarClient 范式），静态缓存彻底隔离。
        var config = new ConnectionConfig
        {
            ConfigId = "online-scope",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-scope");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAreaEntity),
            typeof(TenantLogOperationEntity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateAsync_RejectsMissingServer(long serverId)
    {
        var factory = new OnlineScopeContextFactory(Repository<TenantGameAreaEntity>());
        var exception = await Assert.ThrowsAsync<BusinessException>(() => factory.CreateAsync(serverId));
        Assert.Equal(32200005, exception.Code);
    }

    [Fact]
    public async Task CreateAsync_RejectsNonexistentServer()
    {
        var factory = new OnlineScopeContextFactory(Repository<TenantGameAreaEntity>());
        var exception = await Assert.ThrowsAsync<BusinessException>(() => factory.CreateAsync(9999));
        Assert.Equal(32200001, exception.Code);
    }

    [Fact]
    public async Task CreateAsync_ReturnsExistingServerWithoutUserContext()
    {
        await SeedAsync();
        var factory = new OnlineScopeContextFactory(Repository<TenantGameAreaEntity>());
        var scope = await factory.CreateAsync(Area1001, "unit-test");
        Assert.Equal(Area1001, scope.ServerId);
        Assert.Equal("unit-test", scope.RequestSummary);
    }

    /// <summary>
    /// 审计承载断言：拒绝异常（镜像数字码 + 本地化文案 + errors JSON）可完整写入租户操作日志并查回往返
    /// （MonitorLogging 在 action 层自动落库；此处锁定 TenantLogOperationEntity 表对 322 段拒绝留痕的承载能力）。
    /// </summary>
    [Fact]
    public async Task RejectedScope_IsAuditableInTenantOperationLog()
    {
        await SeedAsync();

        var factory = new OnlineScopeContextFactory(Repository<TenantGameAreaEntity>());

        // 本地化文案断言依赖请求语言，显式固定 zh-CN（与 ErrorCodeContractTests 的 BusinessException 行为测试同姿势）。
        var originalCulture = CultureInfo.CurrentUICulture;
        string persistedMessage;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("zh-CN");
            var exception = await Assert.ThrowsAsync<BusinessException>(() => factory.CreateAsync(9999));
            Assert.Equal(32200001, exception.Code);

            var log = new TenantLogOperationEntity
            {
                Message = exception.Message,
                Exception = exception.ToString(),
                RequestUrl = "/api/online/scope/test",
                CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                IsDeleted = false,
            };
            await _db.Insertable(log).ExecuteCommandAsync();

            var persisted = await _db.Queryable<TenantLogOperationEntity>().FirstAsync(u => u.Message == exception.Message);
            Assert.NotNull(persisted);
            persistedMessage = persisted.Message;
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }

        Assert.Contains("Online 作用域越界", persistedMessage); // ErrorLocalizer 本地化文案完整落库往返
        Assert.Contains("9999", persistedMessage); // errors JSON 透出的越界 appId 可检索
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(NewArea(Area1001, App100, now)).ExecuteCommandAsync();
        await _db.Insertable(NewArea(Area2001, App200, now)).ExecuteCommandAsync();
    }

    private static TenantGameAreaEntity NewArea(long id, long gameAppId, long now)
    {
        return new TenantGameAreaEntity
        {
            Id = id,
            Name = $"area-{id}",
            Description = string.Empty,
            GameAppId = gameAppId,
            HttpManageUrl = "https://game-server.test",
            TcpSocketUrl = "tcp://game-server.test",
            WebSocketUrl = "wss://game-server.test",
            ServerVersion = "1.0.0",
            ServerType = "default",
            LocalizationKey = "area.key",
            AllowedPlayerProfile = "{}",
            ServerEvents = "[]",
            AccessControls = "{}",
            ServerContainerId = string.Empty,
            ServerParams = "{}",
            AppVersion = "1.0.0",
            GmApiPrefix = "gm/api",
            ControlApiPrefix = "ctl",
            OnlineAdminApiPrefix = "online/admin",
            CreatedTime = now,
            IsDeleted = false,
        };
    }

    public void Dispose()
    {
        _scopeOwner.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
