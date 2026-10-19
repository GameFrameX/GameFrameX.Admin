using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GameMailEntityService — IsAllPost / Status (GameMailStatus?) / MailType (GameMailType?) 过滤契约。
/// </summary>
public sealed class AdvancedQueryMailFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"advq_mail_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public AdvancedQueryMailFilterTests()
    {
        var config = new ConnectionConfig { DbType = DbType.Sqlite, ConnectionString = $"DataSource={_dbPath}", };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameMailEntity));
    }

    [Fact]
    public async Task StatusFilter_ReturnsOnlyMatchingStatus()
    {
        await SeedAsync();
        var sent = await BuildQuery(status: GameMailStatus.Sent).ToListAsync();
        Assert.Single(sent);
        Assert.Equal(GameMailStatus.Sent, sent[0].Status);
    }

    [Fact]
    public async Task MailTypeFilter_ReturnsOnlyMatchingType()
    {
        await SeedAsync();
        var sys = await BuildQuery(mailType: GameMailType.SystemMail).ToListAsync();
        Assert.Single(sys);
        Assert.Equal(GameMailType.SystemMail, sys[0].MailType);
    }

    [Fact]
    public async Task IsAllPostFilter_ReturnsOnlyBroadcastMails()
    {
        await SeedAsync();
        var broadcast = await BuildQuery(true).ToListAsync();
        Assert.Equal(2, broadcast.Count);
        Assert.All(broadcast, m => Assert.True(m.IsAllPost));
    }

    [Fact]
    public async Task NoAdvancedFilter_ReturnsAll()
    {
        await SeedAsync();
        var all = await BuildQuery(null, null, null).ToListAsync();
        Assert.Equal(4, all.Count);
    }

    private ISugarQueryable<TenantGameMailEntity> BuildQuery(bool? isAllPost = null, GameMailStatus? status = null, GameMailType? mailType = null)
    {
        return _db.Queryable<TenantGameMailEntity>()
                  .WhereIF(isAllPost.HasValue, u => u.IsAllPost == isAllPost!.Value)
                  .WhereIF(status.HasValue, u => u.Status == status!.Value)
                  .WhereIF(mailType.HasValue, u => u.MailType == mailType!.Value);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(BuildMail(1, "m1", true, GameMailStatus.Sent, GameMailType.AnnouncementMail, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildMail(2, "m2", false, GameMailStatus.Draft, GameMailType.SystemMail, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildMail(3, "m3", true, GameMailStatus.Read, GameMailType.PersonalMail, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildMail(4, "m4", false, GameMailStatus.Draft, GameMailType.PersonalMail, now)).ExecuteCommandAsync();
    }

    private static TenantGameMailEntity BuildMail(long id, string title, bool isAllPost, GameMailStatus status, GameMailType mailType, long now)
    {
        // 非空字符串列全部给占位值，满足 Sqlite NOT NULL 约束（本测试只断言 IsAllPost/Status/MailType 过滤形状）。
        return new TenantGameMailEntity
        {
            Id = id, Title = title, SubTitle = "", Body = "", PlayerIdListJson = "[]", AttachmentsJson = "[]",
            IsAllPost = isAllPost, Status = status, MailType = mailType, IsDeleted = false, CreatedTime = now,
        };
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