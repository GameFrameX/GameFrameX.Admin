using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

// C276（GFX-682）：SimpleClient.ChangeRepository<T>() 无参重载在仓储存在带参 public 构造时，
// 用 Activator.CreateInstance(type, new string[1]) 寻找 (string) 单参构造，对本项目带
// (ISqlSugarClient, TenantDbConnectionProvider) 双参构造的 SqlSugarRepository<T> 必抛
// MissingMethodException（AddTenantAsync 报 "Constructor on type ... not found" 的根因）。
// 本测试锁定替代路径：经 DI open generic（AddScoped(typeof(SqlSugarRepository<>))）解析目标仓储。
public sealed class SqlSugarRepositoryDiResolutionTests : IDisposable
{
    private readonly SqlSugarScope _client;
    private readonly List<string> _dbPaths = new();

    public SqlSugarRepositoryDiResolutionTests()
    {
        SqlSugarCache.Configure(new FakeCache());
        var mainConfig = CreateConfig();
        _client = new SqlSugarScope(new List<ConnectionConfig> { mainConfig, });
        SqlSugarSetup.InitDatabase(_client, mainConfig, new[]
        {
            typeof(SystemUserEntity),
            typeof(SystemTenantEntity),
        });
    }

    /// <summary>
    /// DI 解析平台表仓储：无租户上下文（superadmin 语义）下不抛异常，Context 路由主库且查询可用。
    /// 对应 AddTenantAsync 中 adminAccount 查重的修复路径。
    /// </summary>
    [Fact]
    public void DiResolution_PlatformTableRepository_RoutesMainDatabase_WithoutTenantContext()
    {
        var services = BuildServices();

        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var systemUserRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemUserEntity>>();

            Assert.NotNull(systemUserRepository.Context);
            Assert.Equal(SqlSugarConst.MainConfigId, systemUserRepository.Context.CurrentConnectionConfig.ConfigId.ToString());
            // 建表后查询可用（空表 Count == 0），证明连接落在主库的 system_user 物理表。
            Assert.Equal(0, systemUserRepository.AsQueryable().Count());
        }
    }

    /// <summary>
    /// 根因锁定：SqlSugar 5.1.4.216 的 ChangeRepository 无参重载对带参构造仓储抛 MissingMethodException。
    /// 若 SqlSugar 升级后本测试失败，说明官方已修复该行为，可重新评估 C276 的禁用约束并同步更新守护测试。
    /// </summary>
    [Fact]
    public void ChangeRepository_ParameterlessOverload_ThrowsMissingMethodException()
    {
#pragma warning disable CS8604 // SystemTenantEntity 是平台表，构造不实际使用 tenantDbConnectionProvider
        var sourceRepository = new SqlSugarRepository<SystemTenantEntity>(_client, null!);
#pragma warning restore CS8604

        Assert.Throws<MissingMethodException>(() => sourceRepository.ChangeRepository<SqlSugarRepository<SystemUserEntity>>());
    }

    private IServiceCollection BuildServices()
    {
        return new ServiceCollection()
               .AddSingleton<ISqlSugarClient>(_client)
               .AddScoped(typeof(SqlSugarRepository<>))
               .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor())
               .AddSingleton<ISqlSugarTenantIdResolver>(new DelegateSqlSugarTenantIdResolver(() => null))
               .AddSingleton<ICache>(new FakeCache())
               .AddSingleton<IOptions<CacheOptions>>(Options.Create(new CacheOptions()))
               .AddSingleton<IOptions<DbConnectionOptions>>(Options.Create(new DbConnectionOptions()))
               .AddSingleton<SystemCacheService>()
               .AddTransient<TenantDbConnectionProvider>();
    }

    private DbConnectionConfig CreateConfig()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_c276_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }

    public void Dispose()
    {
        _client.Dispose();
        foreach (var dbPath in _dbPaths)
        {
            try
            {
                File.Delete(dbPath);
            }
            catch (IOException)
            {
                // 临时文件清理失败不影响测试结果
            }
        }
    }
}