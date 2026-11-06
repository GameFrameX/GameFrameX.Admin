using GameFrameX.Application.NoticeCenter.GameRewardTemplate.Dto;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.NoticeCenter.GameRewardTemplate;

/// <summary>
/// 奖励模板服务。
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameRewardTemplateService
    : GenericCrudSelectService<Entity.Business.Game.GameRewardTemplateEntity,
        AddGameRewardTemplateEntityRequest, UpdateGameRewardTemplateEntityRequest,
        GameRewardTemplateResponse, PageGameRewardTemplateEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Business.Game.GameRewardTemplateGroupEntity> _groupRepository;

    public GameRewardTemplateService(
        SqlSugarRepository<Entity.Business.Game.GameRewardTemplateEntity> repository,
        SqlSugarRepository<Entity.Business.Game.GameRewardTemplateGroupEntity> groupRepository)
        : base(repository)
    {
        _groupRepository = groupRepository;
    }

    protected override ISugarQueryable<GameRewardTemplateResponse> BuildPageQuery(PageGameRewardTemplateEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u => u.Name.Contains(input.SearchKey.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.GroupId.HasValue, u => u.GroupId == input.GroupId.Value)
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .LeftJoin<Entity.Business.Game.GameRewardTemplateGroupEntity>((t, g) => t.GroupId == g.Id)
                         .Select((t, g) => new GameRewardTemplateResponse
                         {
                             Id = t.Id,
                             Name = t.Name,
                             Description = t.Description,
                             GroupId = t.GroupId,
                             GroupName = g.Name,
                             RewardItemsJson = t.RewardItemsJson,
                             SnapshotSchemaVersion = t.SnapshotSchemaVersion,
                             Sort = t.Sort,
                             IsEnabled = t.IsEnabled,
                             CreatedTime = t.CreatedTime,
                             UpdateTime = t.UpdateTime,
                             CreatedName = t.CreatedName,
                             UpdatedName = t.UpdatedName,
                             RowVersion = t.RowVersion,
                         });
    }

    /// <summary>
    /// 获取主库中启用的奖励模板。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var rows = await Repository.AsQueryable()
                                   .Where(u => u.IsDeleted == false)
                                   .Where(u => u.IsEnabled != false)
                                   .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => u.Name.Contains(searchKey))
                                   .OrderBy(u => u.Sort)
                                   .OrderBy(u => u.Id)
                                   .Take(pageSize)
                                   .ToListAsync();

        return rows.Select(u => new BaseSelectPageResponse { Id = u.Id, Value = u.Name, Label = u.Description, }).ToList();
    }

    /// <summary>
    /// 复制当前模板奖励快照。业务侧应保存返回的 JSON 快照，而不是运行时引用模板。
    /// </summary>
    [HttpPost("copySnapshot")]
    [ApiDescriptionSettings(Name = "CopySnapshot")]
    public async Task<GameRewardTemplateSnapshotResponse> CopySnapshotAsync(GameRewardTemplateSnapshotRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id && u.IsDeleted == false);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (entity.IsEnabled == false)
        {
            throw BusinessError.Oh("奖励模板已禁用，不能复制快照");
        }

        ValidateRewardItemsJson(entity.RewardItemsJson);
        return new GameRewardTemplateSnapshotResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            GroupId = entity.GroupId,
            RewardItemsJson = entity.RewardItemsJson,
            SnapshotSchemaVersion = entity.SnapshotSchemaVersion,
        };
    }

    protected override async Task OnAddingAsync(AddGameRewardTemplateEntityRequest input)
    {
        await ValidateGroupAsync(input.GroupId);
        ValidateRewardItemsJson(input.RewardItemsJson);
        input.SnapshotSchemaVersion = 1;
    }

    protected override async Task OnUpdatingAsync(UpdateGameRewardTemplateEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await ValidateGroupAsync(input.GroupId);
        ValidateRewardItemsJson(input.RewardItemsJson);
        input.SnapshotSchemaVersion = 1;
    }

    private static void ValidateRewardItemsJson(string rewardItemsJson)
    {
        if (string.IsNullOrWhiteSpace(rewardItemsJson))
        {
            throw BusinessError.Oh("奖励明细不能为空");
        }

        List<GamePropItem> items;
        try
        {
            items = JsonHelper.Deserialize<List<GamePropItem>>(rewardItemsJson);
        }
        catch (Exception exception)
        {
            throw BusinessError.Oh($"奖励明细格式错误：{exception.Message}");
        }

        if (items == null || items.Count == 0)
        {
            throw BusinessError.Oh("奖励明细不能为空");
        }

        if (items.Any(u => u.Count <= 0))
        {
            throw BusinessError.Oh("奖励数量必须大于0");
        }

        if (items.Any(u => u.Id <= 0 && u.ItemId <= 0))
        {
            throw BusinessError.Oh("奖励道具Id不能为空");
        }
    }

    private async Task ValidateGroupAsync(long groupId)
    {
        if (groupId <= 0)
        {
            throw BusinessError.Oh("奖励模板分组不能为空");
        }

        var group = await _groupRepository.GetFirstAsync(u => u.Id == groupId && u.IsEnabled != false);
        if (group == null)
        {
            throw BusinessError.Oh("奖励模板分组不存在或已禁用");
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629