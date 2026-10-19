using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim() 实际安全
namespace GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity;

/// <summary>
/// 广告奖励发放账本 Admin 查询服务（C114 / GFX-110 事实源的管理面跟进，MENU-P2-03 欠账清理）。
/// <para>只读：PageAsync（继承基类）/ GetAsync 详情（继承基类）/ ExportAsync 导出当前过滤列表（上限 1000 行，文件化由前端处理）。</para>
/// <para>事实数据铁律：OnAddingAsync / OnUpdatingAsync 抛错关闭默认 AddAsync/UpdateAsync 端点，无 DeleteAsync 端点。</para>
/// <para>高风险动作 RetryAsync / MarkProcessedAsync 涉及 C114 幂等状态机（同 IdempotencyKey 复用账本行从 Failed 翻回），
/// 留待单独 change 落地；前端对应按钮已由 tenantGameAdRewardEntity:retry / :markProcessed 权限码门控，未种码即隐藏。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameAdvertisementRewardEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGameAdvertisementRewardEntity,
        AddTenantGameAdvertisementRewardEntityRequest, UpdateTenantGameAdvertisementRewardEntityRequest,
        TenantGameAdvertisementRewardEntityResponse, PageTenantGameAdvertisementRewardEntityRequest>
{
    public TenantGameAdvertisementRewardEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAdvertisementRewardEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 账本由广告服务端回调链路产生（Client.Api GameAdvertisementRewardService），禁止管理后台直接创建。
    /// </summary>
    protected override Task OnAddingAsync(AddTenantGameAdvertisementRewardEntityRequest input)
    {
        throw new Exception("广告奖励账本由广告回调链路产生，不支持通过管理后台直接创建");
    }

    /// <summary>
    /// 账本为事实数据，禁止管理后台修改。
    /// </summary>
    protected override Task OnUpdatingAsync(UpdateTenantGameAdvertisementRewardEntityRequest input)
    {
        throw new Exception("广告奖励账本为事实数据，不支持修改");
    }

    /// <summary>
    /// 账本分页查询过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGameAdvertisementRewardEntityResponse> BuildPageQuery(PageTenantGameAdvertisementRewardEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "广告奖励账本");
        return Repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Provider), u => u.Provider == input.Provider)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.TransactionId), u => u.TransactionId == input.TransactionId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.RoleId), u => u.RoleId == input.RoleId)
                         .WhereIF(input.UserId.HasValue, u => u.UserId == input.UserId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.AdvertisementUnit), u => u.AdvertisementUnit.Contains(input.AdvertisementUnit.Trim()))
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .WhereIF(input.AreaId.HasValue, u => u.AreaId == input.AreaId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => u.TransactionId.Contains(input.SearchKey.Trim()) || u.RoleId.Contains(input.SearchKey.Trim()))
                         .WhereIF(input.CreateTimeRange != null && input.CreateTimeRange.Count == 2 && input.CreateTimeRange[0] < input.CreateTimeRange[1],
                                  u => u.CreatedTime >= input.CreateTimeRange[0]
                                       && u.CreatedTime <= input.CreateTimeRange[1])
                         .Select<TenantGameAdvertisementRewardEntityResponse>();
    }

    /// <summary>
    /// 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Export)]
    [ApiDescriptionSettings(Name = "Export")]
    [DisplayName("导出广告奖励账本")]
    public async Task<List<TenantGameAdvertisementRewardEntityResponse>> ExportAsync(PageTenantGameAdvertisementRewardEntityRequest input)
    {
        return await BuildPageQuery(input).Take(1000).ToListAsync();
    }
}
#pragma warning restore CS8602, CS8604, CS8629
