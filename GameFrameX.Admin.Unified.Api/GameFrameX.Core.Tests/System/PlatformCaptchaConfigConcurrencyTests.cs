using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 验证 <see cref="PlatformCaptchaConfigService.UpdateAsync"/> 所依赖的乐观并发条件更新机制：
/// 以读取时的完整 SystemConfigEntity.Value（含版本）作为比较基准执行条件更新，
/// 受影响行数为 0 即表示期间存在并发写入，是服务重试循环判定版本冲突的依据。
/// </summary>
public class PlatformCaptchaConfigConcurrencyTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"captcha_cas_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public PlatformCaptchaConfigConcurrencyTests()
    {
        _db = new SqlSugarScope(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
        _db.CodeFirst.InitTables<SystemConfigEntity>();
    }

    [Fact]
    public async Task CasUpdate_WhenValueMatches_AppliesAndReturnsOne()
    {
        var row = await SeedRowAsync(1);

        var affected = await CasUpdateAsync(row.Id, row.Value, SerializeWithVersion(2));

        Assert.Equal(1, affected);
        var after = await _db.Queryable<SystemConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal(SerializeWithVersion(2), after.Value);
    }

    [Fact]
    public async Task CasUpdate_WhenValueStale_ReturnsZeroAndLeavesRow()
    {
        var row = await SeedRowAsync(1);

        // 并发写入：另一请求先把 Value 改为 version=2
        await _db.Updateable<SystemConfigEntity>()
                 .SetColumns(u => new SystemConfigEntity { Value = SerializeWithVersion(2), UpdateTime = NowSeconds(), })
                 .Where(u => u.Id == row.Id)
                 .ExecuteCommandAsync();

        // 用过期基准（version=1 的旧 Value）做 CAS，应 0 行受影响
        var affected = await CasUpdateAsync(row.Id, row.Value, SerializeWithVersion(3));

        Assert.Equal(0, affected);
        var after = await _db.Queryable<SystemConfigEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal(SerializeWithVersion(2), after.Value);
    }

    [Fact]
    public async Task RetryLoop_WithConcurrentWriter_StillProducesMonotonicVersion()
    {
        var seeded = await SeedRowAsync(5);
        var written = new List<long>();

        // 模拟服务重试循环：每次重读当前 Value，重算 version+1，CAS 写入；
        // 在第 1 次保存后插入一次并发写入，验证后续 CAS 能重读新值并继续严格递增。
        for (var i = 0; i < 3; i++)
        {
            var attempt = 0;
            while (true)
            {
                var current = await _db.Queryable<SystemConfigEntity>().FirstAsync(u => u.Id == seeded.Id);
#pragma warning disable CS8604 // 测试上下文：seeded 行 Value 由 SerializeWithVersion 写入，断言非空
                var settings = PlatformCaptchaConfigProvider.Deserialize(current.Value);
#pragma warning restore CS8604
                var nextVersion = Math.Max(settings.Version, 0) + 1;
                var newValue = SerializeWithVersion(nextVersion);

                var affected = await CasUpdateAsync(seeded.Id, current.Value, newValue);
                if (affected > 0)
                {
                    written.Add(nextVersion);
                    break;
                }

                if (++attempt >= 8)
                {
                    throw new InvalidOperationException("CAS 重试耗尽");
                }
            }

            if (i == 0)
            {
                await _db.Updateable<SystemConfigEntity>()
                         .SetColumns(u => new SystemConfigEntity { Value = SerializeWithVersion(99), UpdateTime = NowSeconds(), })
                         .Where(u => u.Id == seeded.Id)
                         .ExecuteCommandAsync();
            }
        }

        // 三次保存均成功，版本严格单调递增（并发写入的 99 会被下一次重读覆盖为更高的递增值）
        Assert.Equal(new long[] { 6, 100, 101, }, written);
    }

    private async Task<SystemConfigEntity> SeedRowAsync(long version)
    {
        var row = new SystemConfigEntity
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Name = "平台图形验证码配置",
            Code = PlatformCaptchaConfigConstants.ConfigCode,
            Value = SerializeWithVersion(version),
            SysFlag = YesNoEnum.Y,
            OrderNo = 20,
            GroupCode = "Security",
            Remark = "平台私有登录图形验证码配置",
            CreatedTime = NowSeconds(),
            TenantId = 0,
            CreatedId = 0,
            CreatedName = "test",
            UpdatedId = 0,
            UpdatedName = "test",
            UpdateTime = NowSeconds(),
            UpdateCount = 0,
            IsDeleted = false,
            IsEnabled = true,
            RowVersion = 0,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
        };
        await _db.Insertable(row).ExecuteCommandAsync();
        return await _db.Queryable<SystemConfigEntity>().FirstAsync(u => u.Id == row.Id);
    }

    // 与 PlatformCaptchaConfigService.UpdateAsync 一致的条件更新形态：SET ... WHERE Id=@id AND Value=@originalValue
    private Task<int> CasUpdateAsync(long id, string? originalValue, string? nextValue)
    {
        return _db.Updateable<SystemConfigEntity>()
                  .SetColumns(u => new SystemConfigEntity
                  {
                      Name = "平台图形验证码配置",
                      Value = nextValue,
                      SysFlag = YesNoEnum.Y,
                      GroupCode = "Security",
                      Remark = "平台私有登录图形验证码配置",
                      UpdateTime = NowSeconds(),
                  })
                  .Where(u => u.Id == id && u.Value == originalValue)
                  .ExecuteCommandAsync();
    }

    private static string SerializeWithVersion(long version)
    {
        return PlatformCaptchaConfigProvider.Serialize(new PlatformCaptchaSettings { Version = version, });
    }

    private static long NowSeconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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