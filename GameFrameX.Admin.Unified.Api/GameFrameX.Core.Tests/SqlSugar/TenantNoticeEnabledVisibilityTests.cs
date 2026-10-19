using GameFrameX.Application.System.Tenant.Notice;
using GameFrameX.Application.System.Tenant.Notice.Dto;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GFX-915 / C385 回归测试：通知公告新建后必须对创建人可见。
/// 根因：InitNoticeInfo 不设置 IsEnabled，EntityBase.IsEnabled 无初始化器，
/// NOT NULL 列把 null 序列化为 false 落库，被租户库连接装配的 ISafeEnabledFilter
/// 全局过滤（IsEnabled == null || IsEnabled == true）隐藏，列表恒为空。
/// 复刻生产租户库连接装配（ConfigureTenantConnection 三组过滤器），Sqlite 租户库做服务级端到端验证。
/// </summary>
public sealed class TenantNoticeEnabledVisibilityTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    public TenantNoticeEnabledVisibilityTests()
    {
        _db.InitTenantTables(typeof(TenantNoticeEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task AddNoticeAsync_NewNotice_VisibleUnderGlobalEnabledFilter()
    {
        var service = CreateService();

        using (TenantOperationContext.UseTenant(_db.TenantId))
        {
            await service.AddNoticeAsync(new AddNoticeRequest
            {
                Title = "GFX-915 回归通知",
                Content = "新建后创建人必须可见",
                Type = NoticeTypeEnum.NOTICE,
            });
        }

        // 生产 PageAsync 查询形状：租户库连接 + 全局 ISafeEnabledFilter（未绕过过滤器）。
        var visibleRows = await _db.TenantDb.Queryable<TenantNoticeEntity>().ToListAsync();
        Assert.Single(visibleRows);

        // 修复契约（方言无关锚点）：落库行启用位必须是 true，而非 null/false。
        var physicalRow = await _db.TenantDb.Queryable<TenantNoticeEntity>().Filter(null, true).FirstAsync();
        Assert.True(physicalRow.IsEnabled);
    }

    [Fact]
    public async Task GlobalEnabledFilter_StillHidesExplicitlyDisabledNotice()
    {
        // 锁定过滤器装配真实生效：显式禁用行仍被隐藏（防止「过滤器未装上导致上一测试恒绿」假阳性）。
        await _db.TenantDb.Insertable(new TenantNoticeEntity
        {
            Title = "显式禁用通知",
            Content = "不应出现在全局过滤查询中",
            Type = NoticeTypeEnum.NOTICE,
            IsEnabled = false,
            IsDeleted = false,
            TenantId = _db.TenantId,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();

        var rows = await _db.TenantDb.Queryable<TenantNoticeEntity>().ToListAsync();
        Assert.Empty(rows);
    }

    private TenantNoticeService CreateService()
    {
        // 复刻 Admin host DI 形态：GetTenantRepository 经 IServiceProvider 解析 SqlSugarRepository<T>，
        // Repository 构造按租户上下文路由到租户库连接（生产过滤器 + TenantId 填充 AOP 已装配）。
        var repository = new Repository<TenantNoticeEntity>(_db.MainDb, _db.TenantDbProvider, _db.TenantIdResolver);
        var services = new ServiceCollection();
        services.AddSingleton<SqlSugarRepository<TenantNoticeEntity>>(repository);
        return new TenantNoticeService(
            new CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()),
            services.BuildServiceProvider());
    }
}
