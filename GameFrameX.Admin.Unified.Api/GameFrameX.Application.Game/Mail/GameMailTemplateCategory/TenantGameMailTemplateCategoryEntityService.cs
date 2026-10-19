using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Mail.GameMailTemplateCategory.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Mail.GameMailTemplateCategory;

/// <summary>
/// 邮件模板分类服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameMailTemplateCategoryEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity,
        AddTenantGameMailTemplateCategoryEntityRequest, UpdateTenantGameMailTemplateCategoryEntityRequest,
        TenantGameMailTemplateCategoryEntityResponse, PageTenantGameMailTemplateCategoryEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameMailTemplateCategoryEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    protected override ISugarQueryable<TenantGameMailTemplateCategoryEntityResponse> BuildPageQuery(PageTenantGameMailTemplateCategoryEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "邮件模板分类");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameMailTemplateCategoryEntityResponse>();
    }

    /// <summary>
    /// 新增前：邮件模板分类属于应用内容，AppId 必须为当前应用。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameMailTemplateCategoryEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "邮件模板分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填），并按应用内容规则校验。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameMailTemplateCategoryEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "邮件模板分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }
}
#pragma warning restore CS8602, CS8604, CS8629