using GameFrameX.Application.ServerCenter.GameDockerTag.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.ServerCenter.GameDockerTag;

/// <summary>
/// 游戏镜像服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameDockerTagEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameDockerTagEntity,
        AddTenantGameDockerTagEntityRequest, UpdateTenantGameDockerTagEntityRequest,
        TenantGameDockerTagEntityResponse, PageTenantGameDockerTagEntityRequest>
{

    public TenantGameDockerTagEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameDockerTagEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameDockerTagEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

    }

    protected override ISugarQueryable<TenantGameDockerTagEntityResponse> BuildPageQuery(PageTenantGameDockerTagEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<TenantGameDockerTagEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629