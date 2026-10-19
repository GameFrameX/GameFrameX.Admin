using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Area.GameAreaClassify.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Area.GameAreaClassify;

/// <summary>
/// 游戏大区分类服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameAreaClassifyEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameAreaClassifyEntity,
        AddTenantGameAreaClassifyEntityRequest, UpdateTenantGameAreaClassifyEntityRequest,
        TenantGameAreaClassifyEntityResponse, PageTenantGameAreaClassifyEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameAreaClassifyEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaClassifyEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    protected override ISugarQueryable<TenantGameAreaClassifyEntityResponse> BuildPageQuery(PageTenantGameAreaClassifyEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "区服分类");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameAreaClassifyEntityResponse>();
    }

    /// <summary>
    /// 新增前：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameAreaClassifyEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "区服分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// <para>DockerPassword 已不在 Output 回显（C228 敏感字段剔除），编辑弹窗回传空值时保留 DB 原值，避免误清空。</para>
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAreaClassifyEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        if (string.IsNullOrWhiteSpace(input.DockerPassword))
        {
            input.DockerPassword = existing.DockerPassword;
        }

        AppScopeRules.EnsureFactAppId(input.AppId, "区服分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }
}
#pragma warning restore CS8602, CS8604, CS8629