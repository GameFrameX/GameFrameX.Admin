using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Announcement.GameBannerAnnouncement.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Announcement.GameBannerAnnouncement;

/// <summary>
/// 游戏内横幅公告服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameBannerAnnouncementEntityService : GenericCrudService<Entity.Tenant.Game.Announcement.TenantGameBannerAnnouncementEntity, AddTenantGameBannerAnnouncementEntityRequest, UpdateTenantGameBannerAnnouncementEntityRequest, TenantGameBannerAnnouncementEntityResponse, PageTenantGameBannerAnnouncementEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameBannerAnnouncementEntityService(
        SqlSugarRepository<Entity.Tenant.Game.Announcement.TenantGameBannerAnnouncementEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository) : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 构建分页查询条件（含日期范围过滤）
    /// </summary>
    protected override ISugarQueryable<TenantGameBannerAnnouncementEntityResponse> BuildPageQuery(PageTenantGameBannerAnnouncementEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏横幅公告");
        var query = Repository.AsQueryable()
                              .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                              .WhereIF(!string.IsNullOrWhiteSpace(input.Content), u => u.Content.Contains(input.Content.Trim()))
                              .WhereIF(input.Priority > 0, u => u.Priority == input.Priority)
                              .WhereIF(input.Interval > 0, u => u.Interval == input.Interval)
                              .WhereIF(input.RepeatCount > 0, u => u.RepeatCount == input.RepeatCount)
                              .Where(u => u.AppId == appId)
                              .WhereIF(input.IsActive.HasValue, u => u.IsActive == input.IsActive.Value)
                              .Select<TenantGameBannerAnnouncementEntityResponse>()
            ;

        query = ApplyDateRangeFilter(query, input.StartTimeRange, "u.StartTime");
        query = ApplyDateRangeFilter(query, input.EndTimeRange, "u.EndTime");

        return query;
    }

    /// <summary>
    /// 新增前：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameBannerAnnouncementEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏横幅公告");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        // 实体列 NOT NULL 且前端无此字段：无本地化 key 语义 = 直接展示 Content，兜底空串
        input.ContentLocalizationKey ??= string.Empty;
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameBannerAnnouncementEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏横幅公告");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }
}
#pragma warning restore CS8602, CS8604, CS8629