using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

// 仓储通过依赖注入解析，使用已注册的主库客户端。
// 锁定 open generic 注册可以解析不同实体仓储，避免恢复隐式连接构造。
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
        });
    }

    /// <summary>
    /// DI 解析平台表仓储：无作用域上下文（superadmin 语义）下不抛异常，Context 路由主库且查询可用。
    /// 对应 AddAsync 中 adminAccount 查重的修复路径。
    /// </summary>
    [Fact]
    public void DiResolution_PlatformTableRepository_RoutesMainDatabaseWithoutLegacyContext()
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

    private IServiceCollection BuildServices()
    {
        return new ServiceCollection()
               .AddSingleton<ISqlSugarClient>(_client)
               .AddScoped(typeof(SqlSugarRepository<>))
               .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor())
               .AddSingleton<ICache>(new FakeCache())
               .AddSingleton<IOptions<CacheOptions>>(Options.Create(new CacheOptions()))
               .AddSingleton<SystemCacheService>()
               ;
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