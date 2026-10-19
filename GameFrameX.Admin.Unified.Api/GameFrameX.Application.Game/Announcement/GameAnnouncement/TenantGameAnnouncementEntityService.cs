using System.ComponentModel;
using GameFrameX.Application.Game.Announcement.GameAnnouncement.Dto;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Entity.Game.Online;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Announcement.GameAnnouncement;

/// <summary>
/// 游戏公告服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameAnnouncementEntityService : GenericCrudService<Entity.Tenant.Game.Announcement.TenantGameAnnouncementEntity, AddTenantGameAnnouncementEntityRequest, UpdateTenantGameAnnouncementEntityRequest, TenantGameAnnouncementEntityResponse, PageTenantGameAnnouncementEntityRequest>
{
    private const string DefaultLanguage = "default";
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;
    private readonly IOnlineLiveOpsDispatchService _dispatchService;

    public TenantGameAnnouncementEntityService(
        SqlSugarRepository<Entity.Tenant.Game.Announcement.TenantGameAnnouncementEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        IOnlineLiveOpsDispatchService dispatchService) : base(repository)
    {
        _gameAppRepository = gameAppRepository;
        _dispatchService = dispatchService;
    }

    /// <summary>
    /// 构建分页查询条件（含日期范围过滤）
    /// </summary>
    protected override ISugarQueryable<TenantGameAnnouncementEntityResponse> BuildPageQuery(PageTenantGameAnnouncementEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏公告");
        var language = input.Language?.Trim();
        var hasLanguage = !string.IsNullOrWhiteSpace(language);
        var isDefaultLanguage = language == DefaultLanguage;
        var query = Repository.AsQueryable()
                              .WhereIF(hasLanguage && !isDefaultLanguage, u => u.Language == language)
                              .WhereIF(hasLanguage && isDefaultLanguage, u => u.Language == language || u.Language == null || u.Language == string.Empty)
                              .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                              .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Title.Contains(input.Title.Trim()))
                              .Where(u => u.AppId == appId)
                              .Select<TenantGameAnnouncementEntityResponse>()
            ;

        query = ApplyDateRangeFilter(query, input.StartTimeRange, "u.StartTime");
        query = ApplyDateRangeFilter(query, input.EndTimeRange, "u.EndTime");

        return query;
    }

    /// <summary>
    /// 新增前：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0），并标准化多语言字段。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameAnnouncementEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏公告");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        NormalizeInput(input);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属，并标准化多语言字段。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAnnouncementEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏公告");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        NormalizeInput(input);
    }

    private static void NormalizeInput(ITenantGameAnnouncementEntityRequest input)
    {
        input.Language = string.IsNullOrWhiteSpace(input.Language)
                             ? DefaultLanguage
                             : input.Language.Trim();
        input.TitleLocalizationKey = input.TitleLocalizationKey?.Trim() ?? string.Empty;
        input.ContentLocalizationKey = input.ContentLocalizationKey?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// 经 Online 统一下发当前公告（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（pushAnnouncement 通道），
    /// 来源类型固定注入 Announcement，存在性 / 应用归属 / 作用域校验在单一入口完成。
    /// </summary>
    [HttpPost("dispatchOnline")]
    [ApiDescriptionSettings(Name = "DispatchOnline")]
    [DisplayName("经Online统一下发")]
    public Task<OnlineLiveOpsDispatchResponse> DispatchOnlineAsync(OnlineLiveOpsSourceDispatchRequest input)
    {
        return _dispatchService.DispatchAsync(new OnlineLiveOpsDispatchRequest
        {
            Source = LiveOpsSourceKind.Announcement,
            BusinessKey = input.BusinessKey,
            AppId = input.AppId,
            ServerId = input.ServerId,
            SegmentId = input.SegmentId,
            Reason = input.Reason,
        });
    }
}
#pragma warning restore CS8602, CS8604, CS8629