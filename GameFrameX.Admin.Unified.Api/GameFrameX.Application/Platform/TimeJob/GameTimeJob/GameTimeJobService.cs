using GameFrameX.Application.Platform.TimeJob.GameTimeJob.Dto;

namespace GameFrameX.Application.Platform.TimeJob.GameTimeJob;

/// <summary>
/// 游戏定时任务服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameTimeJobEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameTimeJobEntity,
        AddTenantGameTimeJobEntityRequest, UpdateTenantGameTimeJobEntityRequest,
        TenantGameTimeJobEntityResponse, PageTenantGameTimeJobEntityRequest>
{
    public TenantGameTimeJobEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameTimeJobEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 手动新增时由服务端固定任务来源。
    /// </summary>
    protected override Task OnAddingAsync(AddTenantGameTimeJobEntityRequest input)
    {
        // 任务来源是系统标记非用户输入字段，手动新增固定 Manual，否则实体 NOT NULL 列 insert 500
        input.Source = "Manual";
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameTimeJobEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }

    protected override ISugarQueryable<TenantGameTimeJobEntityResponse> BuildPageQuery(PageTenantGameTimeJobEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<TenantGameTimeJobEntityResponse>();
    }
}