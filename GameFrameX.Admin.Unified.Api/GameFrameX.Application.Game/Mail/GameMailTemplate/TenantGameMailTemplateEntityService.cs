using GameFrameX.Application.Game.Mail.GameMailTemplate.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Mail.GameMailTemplate;

/// <summary>
/// 邮件模板服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameMailTemplateEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameMailTemplateEntity,
        AddTenantGameMailTemplateEntityRequest, UpdateTenantGameMailTemplateEntityRequest,
        TenantGameMailTemplateEntityResponse, PageTenantGameMailTemplateEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity> _categoryRepository;

    public TenantGameMailTemplateEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameMailTemplateEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity> categoryRepository)
        : base(repository)
    {
        _categoryRepository = categoryRepository;
    }

    protected override ISugarQueryable<TenantGameMailTemplateEntityResponse> BuildPageQuery(PageTenantGameMailTemplateEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u => u.Name.Contains(input.SearchKey.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.CategoryId.HasValue, u => u.CategoryId == input.CategoryId.Value)
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .LeftJoin<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity>((t, c) => t.CategoryId == c.Id)
                         .Select((t, c) => new TenantGameMailTemplateEntityResponse
                         {
                             Id = t.Id,
                             Name = t.Name,
                             Description = t.Description,
                             CategoryId = t.CategoryId,
                             CategoryIdName = c.Name,
                             TitleTemplate = t.TitleTemplate,
                             SubTitleTemplate = t.SubTitleTemplate,
                             BodyTemplate = t.BodyTemplate,
                             DefaultAttachmentsJson = t.DefaultAttachmentsJson,
                             DefaultMailType = t.DefaultMailType,
                             DefaultExpirationDays = t.DefaultExpirationDays,
                             Sort = t.Sort,
                             IsEnabled = t.IsEnabled,
                             CreatedTime = t.CreatedTime,
                             UpdateTime = t.UpdateTime,
                             CreatedName = t.CreatedName,
                             UpdatedName = t.UpdatedName,
                             // 手写投影漏版本号：响应 RowVersion 恒 null → 前端编辑提交 rowVersion=null →
                             // 乐观锁以 null 参数比较版本列（Postgres bigint = text）500。左连接投影必须显式带上。
                             RowVersion = t.RowVersion,
                         });
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameMailTemplateEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629