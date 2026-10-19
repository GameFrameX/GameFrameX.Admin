using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;

namespace GameFrameX.Application.Game.TimeJob.GameTimeJob;

/// <summary>
/// 游戏定时任务服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameTimeJobEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameTimeJobEntity,
        AddTenantGameTimeJobEntityRequest, UpdateTenantGameTimeJobEntityRequest,
        TenantGameTimeJobEntityResponse, PageTenantGameTimeJobEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameTimeJobEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameTimeJobEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameTimeJobEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "定时任务");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        // 任务来源是系统标记非用户输入字段，手动新增固定 Manual，否则实体 NOT NULL 列 insert 500
        input.Source = "Manual";
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameTimeJobEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "定时任务");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    protected override ISugarQueryable<TenantGameTimeJobEntityResponse> BuildPageQuery(PageTenantGameTimeJobEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "定时任务");
        return Repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<TenantGameTimeJobEntityResponse>();
    }
}