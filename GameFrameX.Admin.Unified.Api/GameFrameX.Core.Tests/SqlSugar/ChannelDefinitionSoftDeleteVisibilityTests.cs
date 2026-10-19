using GameFrameX.Application.System.ChannelDefinition;
using GameFrameX.Application.System.ChannelDefinition.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Tenant;
using GameFrameX.Entity.Tenant.Game.App;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GFX-919 / C388 回归测试：渠道定义管理查询不得显示软删行。
/// 根因：ChannelDefinitionManagementServiceBase.BuildPageQuery 用 .Filter(null, true) 绕过全部全局过滤器
/// （意图：管理列表显示禁用渠道 + 跨 scope 合并），连带禁用了 ISafeDeletedFilter，软删行穿透分页列表。
/// 复刻生产租户库连接装配（ClientApiTestDb 三组过滤器），Sqlite 租户库做服务级 PageAsync/AvailableAsync 端到端验证；
/// 修复契约（C67 FixPattern 同构）：Filter(null, true) 后必须手动补 u.IsDeleted == false，且禁用行仍须可见（可重新启用）。
/// </summary>
public sealed class ChannelDefinitionSoftDeleteVisibilityTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    public ChannelDefinitionSoftDeleteVisibilityTests()
    {
        _db.InitTenantTables(typeof(TenantChannelDefinitionEntity), typeof(TenantGameAppPackageEntity));
        _db.MainDb.CodeFirst.InitTables(typeof(GlobalChannelDefinitionEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task PageAsync_SoftDeletedChannelExcluded_DisabledChannelStillVisible()
    {
        SeedChannel(9101, "GFX_919_ACTIVE", isEnabled: true, isDeleted: false);
        SeedChannel(9102, "GFX_919_DISABLED", isEnabled: false, isDeleted: false);
        SeedChannel(9103, "GFX_919_SOFTDELETED", isEnabled: true, isDeleted: true);

        var service = CreateService();
        var page = await service.PageAsync(new PageChannelDefinitionRequest
        {
            Page = 1,
            PageSize = 10,
        });

        var codes = page.Items.Select(u => u.Code).ToList();
        Assert.Contains("GFX_919_ACTIVE", codes);
        // 禁用行必须仍可见：Filter(null, true) 的原意图（停用渠道可重新启用），修复不得破坏。
        Assert.Contains("GFX_919_DISABLED", codes);
        // 软删行不得出现在管理列表（GFX-919 缺陷主体）。
        Assert.DoesNotContain("GFX_919_SOFTDELETED", codes);
    }

    [Fact]
    public async Task AvailableAsync_SoftDeletedChannelExcluded()
    {
        SeedChannel(9111, "GFX_919_AVAILABLE", isEnabled: true, isDeleted: false);
        SeedChannel(9112, "GFX_919_SOFTDELETED", isEnabled: true, isDeleted: true);

        var service = CreateService();
        var channels = await service.AvailableAsync(new BaseSelectPageRequest());

        var codes = channels.Select(u => u.Code).ToList();
        Assert.Contains("GFX_919_AVAILABLE", codes);
        // 已删渠道不得出现在「可用渠道」下拉（同类连带禁用，删除后仍可被分包选用即缺陷链后半段）。
        Assert.DoesNotContain("GFX_919_SOFTDELETED", codes);
    }

    [Fact]
    public async Task GlobalSoftDeleteFilter_StillHidesSoftDeletedRow()
    {
        SeedChannel(9121, "GFX_919_ACTIVE", isEnabled: true, isDeleted: false);
        SeedChannel(9122, "GFX_919_SOFTDELETED", isEnabled: true, isDeleted: true);

        // 锁定过滤器装配真实生效：不绕过过滤器时软删行被 ISafeDeletedFilter 隐藏
        // （防止「过滤器未装上导致前两个测试恒绿」假阳性）。
        var rows = await _db.TenantDb.Queryable<TenantChannelDefinitionEntity>()
                            .Where(u => u.Code.StartsWith("GFX_919"))
                            .ToListAsync();
        var codes = rows.Select(u => u.Code).ToList();
        Assert.Contains("GFX_919_ACTIVE", codes);
        Assert.DoesNotContain("GFX_919_SOFTDELETED", codes);
    }

    private void SeedChannel(long id, string code, bool isEnabled, bool isDeleted)
    {
        _db.TenantDb.Insertable(new TenantChannelDefinitionEntity
        {
            Id = id,
            Code = code,
            Name = code.ToLowerInvariant(),
            Description = code.ToLowerInvariant(),
            Scope = ChannelScopeEnum.Tenant,
            TenantId = _db.TenantId,
            PlatformCode = "all",
            IsEnabled = isEnabled,
            IsDeleted = isDeleted,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommand();
    }

    private TenantChannelDefinitionService CreateService()
    {
        // 复刻 Admin host DI 形态：Repository<T> 按实体边界路由——租户表经 TenantDbProvider 走租户库
        // （生产过滤器 + TenantId 填充 AOP 已装配），全局表落主库连接。
        var currentUserContext = new CurrentUserContext(new HttpContextAccessor());
        currentUserContext.TenantId = _db.TenantId;
        return new TenantChannelDefinitionService(
            _db.Repository<TenantChannelDefinitionEntity>(),
            _db.Repository<GlobalChannelDefinitionEntity>(),
            _db.Repository<TenantGameAppPackageEntity>(),
            currentUserContext);
    }
}
