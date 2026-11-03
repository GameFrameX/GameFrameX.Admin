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

    public TenantGameAreaClassifyEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaClassifyEntity> repository)
        : base(repository)
    {
    }

    protected override ISugarQueryable<TenantGameAreaClassifyEntityResponse> BuildPageQuery(PageTenantGameAreaClassifyEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<TenantGameAreaClassifyEntityResponse>();
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// <para>DockerPassword 已不在 Output 回显（C228 敏感字段剔除），编辑弹窗回传空值时保留 DB 原值，避免误清空。</para>
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAreaClassifyEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (string.IsNullOrWhiteSpace(input.DockerPassword))
        {
            input.DockerPassword = existing.DockerPassword;
        }

    }
}
#pragma warning restore CS8602, CS8604, CS8629