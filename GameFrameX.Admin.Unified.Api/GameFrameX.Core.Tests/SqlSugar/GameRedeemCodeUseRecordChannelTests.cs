using GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// BLA-32 / C71 回归测试：复刻 TenantGameRedeemCodeUseRecordEntityService.BuildPageQuery 的查询形状
/// （区服 + 兑换码(Code+AreaId) + 渠道 三重 LeftJoin，新增 input.ChannelId WhereIF 与 ChannelId/ChannelName 投影）。
/// 用 Sqlite 内存文件库 + ConfigureBaseFilters 做端到端查询验证。
/// </summary>
public sealed class GameRedeemCodeUseRecordChannelTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"redeemcode_channel_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameRedeemCodeUseRecordChannelTests()
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
            typeof(TenantGameRedeemCodeChannelEntity),
            typeof(TenantGameRedeemCodeEntity),
            typeof(TenantGameRedeemCodeUseRecordEntity));
    }

    /// <summary>
    /// 不传 ChannelId：返回全部使用记录，ChannelId/ChannelName 按关联兑换码正确投影（无渠道为 null）。
    /// </summary>
    [Fact]
    public async Task NoChannelFilter_ProjectsChannelIdAndNameFromJoinedCode()
    {
        await SeedAsync();

        long? channelId = null;
        var rows = await BuildQuery(channelId).ToListAsync();

        Assert.Equal(4, rows.Count);
        // CODE1(area1)→渠道A、CODE2(area1)→无渠道、CODE3(area2)→渠道B
        var code1 = rows.Single(r => r.Code == "CODE1" && r.AreaId == 1 && r.PlayerId == 1);
        Assert.Equal(10, code1.ChannelId);
        Assert.Equal("渠道A", code1.ChannelName);
        var code2 = rows.Single(r => r.Code == "CODE2" && r.AreaId == 1 && r.PlayerId == 2);
        Assert.Null(code2.ChannelId);
        Assert.Null(code2.ChannelName);
        var code3 = rows.Single(r => r.Code == "CODE3" && r.AreaId == 2 && r.PlayerId == 3);
        Assert.Equal(11, code3.ChannelId);
        Assert.Equal("渠道B", code3.ChannelName);
        // 同兑换码不同角色各自成行
        Assert.Equal(2, rows.Count(r => r.Code == "CODE1" && r.AreaId == 1));
    }

    /// <summary>
    /// 传 ChannelId：只返回该分组对应兑换码的使用记录。
    /// </summary>
    [Fact]
    public async Task ChannelIdFilter_ReturnsOnlyMatchingChannelRecords()
    {
        await SeedAsync();

        var rows = await BuildQuery(10).ToListAsync();

        // 仅 CODE1(area1) 属于渠道A(10)，两条使用记录（player1/player4）
        Assert.Equal(2, rows.Count);
        Assert.All(rows, r =>
        {
            Assert.Equal(10, r.ChannelId);
            Assert.Equal("渠道A", r.ChannelName);
            Assert.Equal("CODE1", r.Code);
        });
        // 不存在匹配渠道时返回空
        var empty = await BuildQuery(999).ToListAsync();
        Assert.Empty(empty);
    }

    /// <summary>
    /// 现有 AreaId 过滤不受新增 join 影响。
    /// </summary>
    [Fact]
    public async Task AreaIdFilter_StillWorksAlongsideChannelJoins()
    {
        await SeedAsync();

        var rows = await BuildQuery(null, 2).ToListAsync();

        Assert.Single(rows);
        Assert.Equal("CODE3", rows[0].Code);
        Assert.Equal(11, rows[0].ChannelId);
    }

    private ISugarQueryable<TenantGameRedeemCodeUseRecordEntityResponse> BuildQuery(long? channelId, long? areaId = null, string? code = null, long? playerId = null)
    {
        // 复刻 GameRedeemCodeUseRecordEntityService.BuildPageQuery 的 BLA-32 增量部分（兑换码 + 渠道 join、ChannelId 过滤与投影）。
        // 注：区服 LeftJoin 是 BuildPageQuery 既有逻辑（非 BLA-32 范围）且本测试不校验区服列，此处不复刻；
        // 同时可避免与 GameOrderShipServiceTests（不以 ConfigureBaseSettings 初始化、PascalCase 表名）共享 SqlSugar 静态实体缓存导致的 join 表名漂移。
        return _db.Queryable<TenantGameRedeemCodeUseRecordEntity>()
                  .WhereIF(areaId > 0, u => u.AreaId == areaId)
                  .WhereIF(!string.IsNullOrWhiteSpace(code), u => u.Code == code)
                  .WhereIF(playerId > 0, u => u.PlayerId == playerId)
                  .LeftJoin<TenantGameRedeemCodeEntity>((u, c) => u.Code == c.Code && u.AreaId == c.AreaId)
                  .LeftJoin<TenantGameRedeemCodeChannelEntity>((u, c, channel) => c.ChannelId == channel.Id)
                  .WhereIF(channelId > 0, (u, c, channel) => c.ChannelId == channelId)
                  .Select((u, c, channel) => new TenantGameRedeemCodeUseRecordEntityResponse
                  {
                      Id = u.Id,
                      AreaId = u.AreaId,
                      Code = u.Code,
                      PlayerId = u.PlayerId,
                      PlayerIdString = u.PlayerId.ToString(),
                      ChannelId = c.ChannelId,
                      ChannelName = channel.Name,
                  });
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nowDt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 注：区服表仅 InitTables 建表，不 seed：area 为 LEFT JOIN，测试不依赖区服列；
        // 区服实体含多个 NOT NULL 列，seed 会触发约束，故略过。

        await _db.Insertable(new TenantGameRedeemCodeChannelEntity
        {
            Id = 10, Name = "渠道A", IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeChannelEntity
        {
            Id = 11, Name = "渠道B", IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameRedeemCodeEntity
        {
            Id = 100, AreaId = 1, Code = "CODE1", ChannelId = 10, IsActive = true, UsageLimit = 100, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d1", Contents = "[]", IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeEntity
        {
            Id = 101, AreaId = 1, Code = "CODE2", ChannelId = null, IsActive = true, UsageLimit = 100, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d2", Contents = "[]", IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeEntity
        {
            Id = 102, AreaId = 2, Code = "CODE3", ChannelId = 11, IsActive = true, UsageLimit = 100, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d3", Contents = "[]", IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameRedeemCodeUseRecordEntity
        {
            Id = 1, AreaId = 1, Code = "CODE1", PlayerId = 1, IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeUseRecordEntity
        {
            Id = 2, AreaId = 1, Code = "CODE2", PlayerId = 2, IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeUseRecordEntity
        {
            Id = 3, AreaId = 2, Code = "CODE3", PlayerId = 3, IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeUseRecordEntity
        {
            Id = 4, AreaId = 1, Code = "CODE1", PlayerId = 4, IsDeleted = false, TenantId = null, CreatedTime = now,
        }).ExecuteCommandAsync();
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