using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameFrameX.Application.Game.App.GameAppServerConfig;
using GameFrameX.Application.Game.App.GameAppServerConfig.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.ServerConfig;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// C314 游戏运营配置管理服务测试。
/// <para>覆盖：md5 版本号构造保证（改内容必变、同内容幂等）、非法 JSON 拒绝、
/// 同应用键唯一（跨应用允许同键）、分页按应用收口（AppId=null 基线行不出现在管理面）、键级启停读写。</para>
/// </summary>
public sealed class TenantGameAppServerConfigServiceTests : IDisposable
{
    private const long App100 = 100;
    private const long App200 = 200;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"game_server_config_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public TenantGameAppServerConfigServiceTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppServerConfigEntity));
    }

    /// <summary>
    /// 新增时 md5 由服务端对归一化 Config 重算，前端传入的 Md5 被忽略。
    /// </summary>
    [Fact]
    public async Task Add_ComputesMd5FromNormalizedConfig_IgnoresClientInput()
    {
        var service = CreateService();
        var row = await service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "frame",
            Config = "{ \"list\": [1, 2] }", // 带空白，归一化后应与紧凑形同 md5
            Md5 = "attacker-forged-md5",
            Enabled = true,
        });

        Assert.Equal(Md5Of("{\"list\":[1,2]}"), row.Md5);
        var stored = await _db.Queryable<TenantGameAppServerConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal("{\"list\":[1,2]}", stored.Config); // 归一化原文落库
        Assert.Equal(Md5Of(stored.Config), stored.Md5);
    }

    /// <summary>
    /// 非法 JSON（无法解析）新增被拒。
    /// </summary>
    [Fact]
    public async Task Add_RejectsInvalidJson()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "badge",
            Config = "not-a-json{",
        }));
    }

    /// <summary>
    /// 同应用下配置键唯一：重复键新增被拒。
    /// </summary>
    [Fact]
    public async Task Add_RejectsDuplicateKeyInSameApp()
    {
        var service = CreateService();
        await InsertAsync(App100, "frame", "{}");
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "frame",
            Config = "{}",
        }));
    }

    /// <summary>
    /// 跨应用允许同键（应用行互不干扰），且分页按当前应用收口、AppId=null 基线行不出现在管理面。
    /// </summary>
    [Fact]
    public async Task Page_ReturnsOnlyCurrentAppRows_ExcludesBaselineRows()
    {
        await InsertAsync(App100, "frame", "{}");
        await InsertAsync(App200, "frame", "{}");
        await InsertAsync(null, "bakeOut", "{}"); // 种子基线行

        var service = CreateService();
        var page = await service.PageAsync(new PageGameAppServerConfigRequest { AppId = App100, });

        var row = Assert.Single(page.Items);
        Assert.Equal("frame", row.Key);
        Assert.Equal(App100, row.AppId);
    }

    /// <summary>
    /// 更新改内容时 md5 随之重算（构造保证「内容变 md5 必变」）。
    /// </summary>
    [Fact]
    public async Task Update_ChangedConfig_RecomputesMd5()
    {
        var service = CreateService();
        var row = await service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "notify",
            Config = "{\"throttle\":1}",
        });

        await service.UpdateAsync(new UpdateGameAppServerConfigRequest
        {
            Id = row.Id,
            RowVersion = row.RowVersion,
            Key = "notify",
            Config = "{\"throttle\":2}",
            Enabled = true,
        });

        var stored = await _db.Queryable<TenantGameAppServerConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.NotEqual(row.Md5, stored.Md5);
        Assert.Equal(Md5Of(stored.Config), stored.Md5);
    }

    /// <summary>
    /// 同内容重复保存 md5 不变（幂等），且内容归一化后空白差异不引起 md5 抖动。
    /// </summary>
    [Fact]
    public async Task Update_SameContent_KeepsMd5()
    {
        var service = CreateService();
        var row = await service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "general_conf",
            Config = "{\"flag\":true}",
        });

        await service.UpdateAsync(new UpdateGameAppServerConfigRequest
        {
            Id = row.Id,
            RowVersion = row.RowVersion,
            Key = "general_conf",
            Config = "{ \"flag\" : true }", // 语义相同，仅空白不同
            Enabled = true,
        });

        var stored = await _db.Queryable<TenantGameAppServerConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal(row.Md5, stored.Md5);
    }

    /// <summary>
    /// 键级启停可编辑回读（Enabled=false 登录下发侧消费，见 C315）。
    /// </summary>
    [Fact]
    public async Task Update_TogglesEnabled()
    {
        var service = CreateService();
        var row = await service.AddAsync(new AddGameAppServerConfigRequest
        {
            AppId = App100,
            Key = "bakeOut",
            Config = "{}",
            Enabled = true,
        });

        await service.UpdateAsync(new UpdateGameAppServerConfigRequest
        {
            Id = row.Id,
            RowVersion = row.RowVersion,
            Key = "bakeOut",
            Config = "{}",
            Enabled = false,
        });

        var stored = await _db.Queryable<TenantGameAppServerConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.False(stored.Enabled);
    }

    /// <summary>
    /// Page 不带 AppId（null）被守卫拒绝（Nullable 守卫须显式判 null，lesson 2026-08-11）。
    /// </summary>
    [Fact]
    public async Task Page_WithoutAppId_IsRejected()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageGameAppServerConfigRequest()));
    }

    private TenantGameAppServerConfigService CreateService()
    {
        return new TenantGameAppServerConfigService(new SqlSugarRepository<TenantGameAppServerConfigEntity> { Context = _db, });
    }

    private long _nextSeedId = 900000;

    private async Task InsertAsync(long? appId, string key, string config)
    {
        await _db.Insertable(new TenantGameAppServerConfigEntity
        {
            Id = _nextSeedId++,
            AppId = appId,
            Key = key,
            Config = config,
            Md5 = Md5Of(config),
            Enabled = true,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 复刻服务端 md5 版本号算法（归一化字符串的 MD5 hex 小写），用于断言期望值
    /// </summary>
    private static string Md5Of(string normalizedConfig)
    {
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(normalizedConfig))).ToLowerInvariant();
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
