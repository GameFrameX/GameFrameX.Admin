using GameFrameX.Application.NoticeCenter.GameMailTemplateCategory.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.NoticeCenter.GameMailTemplateCategory;

/// <summary>
/// 邮件模板分类服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameMailTemplateCategoryService
    : GenericCrudSelectService<Entity.Business.Game.GameMailTemplateCategoryEntity,
        AddGameMailTemplateCategoryEntityRequest, UpdateGameMailTemplateCategoryEntityRequest,
        GameMailTemplateCategoryResponse, PageGameMailTemplateCategoryEntityRequest>
{

    public GameMailTemplateCategoryService(
        SqlSugarRepository<Entity.Business.Game.GameMailTemplateCategoryEntity> repository)
        : base(repository)
    {
    }

    protected override ISugarQueryable<GameMailTemplateCategoryResponse> BuildPageQuery(PageGameMailTemplateCategoryEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .Select<GameMailTemplateCategoryResponse>();
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameMailTemplateCategoryEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629