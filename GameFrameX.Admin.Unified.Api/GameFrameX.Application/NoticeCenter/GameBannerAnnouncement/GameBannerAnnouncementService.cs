using GameFrameX.Application.NoticeCenter.GameBannerAnnouncement.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.NoticeCenter.GameBannerAnnouncement;

/// <summary>
/// 游戏内横幅公告服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameBannerAnnouncementService : GenericCrudService<Entity.Business.Game.Announcement.GameBannerAnnouncementEntity, AddGameBannerAnnouncementEntityRequest, UpdateGameBannerAnnouncementEntityRequest, GameBannerAnnouncementResponse, PageGameBannerAnnouncementEntityRequest>
{
    public GameBannerAnnouncementService(
        SqlSugarRepository<Entity.Business.Game.Announcement.GameBannerAnnouncementEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件（含日期范围过滤）
    /// </summary>
    protected override ISugarQueryable<GameBannerAnnouncementResponse> BuildPageQuery(PageGameBannerAnnouncementEntityRequest input)
    {
        var query = Repository.AsQueryable()
                              .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                              .WhereIF(!string.IsNullOrWhiteSpace(input.Content), u => u.Content.Contains(input.Content.Trim()))
                              .WhereIF(input.Priority > 0, u => u.Priority == input.Priority)
                              .WhereIF(input.Interval > 0, u => u.Interval == input.Interval)
                              .WhereIF(input.RepeatCount > 0, u => u.RepeatCount == input.RepeatCount)
                              .WhereIF(input.IsActive.HasValue, u => u.IsActive == input.IsActive.Value)
                              .Select<GameBannerAnnouncementResponse>()
            ;

        query = ApplyDateRangeFilter(query, input.StartTimeRange, "u.StartTime");
        query = ApplyDateRangeFilter(query, input.EndTimeRange, "u.EndTime");

        return query;
    }

    /// <summary>
    /// 新增前初始化本地化字段。
    /// </summary>
    protected override Task OnAddingAsync(AddGameBannerAnnouncementEntityRequest input)
    {
        // 实体列 NOT NULL 且前端无此字段：无本地化 key 语义 = 直接展示 Content，兜底空串
        input.ContentLocalizationKey ??= string.Empty;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameBannerAnnouncementEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629