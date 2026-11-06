using GameFrameX.Application.Platform.TimeJob.GameTimeJob.Dto;

namespace GameFrameX.Application.Platform.TimeJob.GameTimeJob;

/// <summary>
/// 游戏定时任务服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameTimeJobService
    : GenericCrudSelectService<Entity.Business.Game.GameTimeJobEntity,
        AddGameTimeJobEntityRequest, UpdateGameTimeJobEntityRequest,
        GameTimeJobResponse, PageGameTimeJobEntityRequest>
{
    public GameTimeJobService(SqlSugarRepository<Entity.Business.Game.GameTimeJobEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 手动新增时由服务端固定任务来源。
    /// </summary>
    protected override Task OnAddingAsync(AddGameTimeJobEntityRequest input)
    {
        // 任务来源是系统标记非用户输入字段，手动新增固定 Manual，否则实体 NOT NULL 列 insert 500
        input.Source = "Manual";
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameTimeJobEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }

    protected override ISugarQueryable<GameTimeJobResponse> BuildPageQuery(PageGameTimeJobEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Select<GameTimeJobResponse>();
    }
}