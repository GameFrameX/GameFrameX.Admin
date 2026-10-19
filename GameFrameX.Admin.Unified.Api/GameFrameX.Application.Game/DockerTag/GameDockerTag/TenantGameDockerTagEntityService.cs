using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.DockerTag.GameDockerTag.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.DockerTag.GameDockerTag;

/// <summary>
/// 游戏镜像服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameDockerTagEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameDockerTagEntity,
        AddTenantGameDockerTagEntityRequest, UpdateTenantGameDockerTagEntityRequest,
        TenantGameDockerTagEntityResponse, PageTenantGameDockerTagEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameDockerTagEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameDockerTagEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameDockerTagEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏镜像标签");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameDockerTagEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏镜像标签");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    protected override ISugarQueryable<TenantGameDockerTagEntityResponse> BuildPageQuery(PageTenantGameDockerTagEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏镜像标签");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameDockerTagEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629