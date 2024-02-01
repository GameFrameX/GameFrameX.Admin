namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏内横幅公告服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameBannerAnnouncementEntityService : BaseService<GameBannerAnnouncementEntity>
{
    public GameBannerAnnouncementEntityService(SqlSugarRepository<GameBannerAnnouncementEntity> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询游戏内横幅公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameBannerAnnouncementEntityOutput>> Page(GameBannerAnnouncementEntityInput input)
    {
        var query= Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Content.Contains(input.SearchKey.Trim())
            )
            .WhereIF(input.AreaId>0, u => u.AreaId == input.AreaId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Content), u => u.Content.Contains(input.Content.Trim()))
            .WhereIF(input.Priority>0, u => u.Priority == input.Priority)
            .WhereIF(input.Interval>0, u => u.Interval == input.Interval)
            .Select<GameBannerAnnouncementEntityOutput>()
;
        if(input.StartTimeRange != null && input.StartTimeRange.Count >0)
        {
            DateTime? start= input.StartTimeRange[0]; 
            query = query.WhereIF(start.HasValue, u => u.StartTime > start);
            if (input.StartTimeRange.Count >1 && input.StartTimeRange[1].HasValue)
            {
                var end = input.StartTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.StartTime < end);
            }
        } 
        if(input.EndTimeRange != null && input.EndTimeRange.Count >0)
        {
            DateTime? start= input.EndTimeRange[0]; 
            query = query.WhereIF(start.HasValue, u => u.EndTime > start);
            if (input.EndTimeRange.Count >1 && input.EndTimeRange[1].HasValue)
            {
                var end = input.EndTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.EndTime < end);
            }
        } 
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏内横幅公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameBannerAnnouncementEntityInput input)
    {
        await InnerAdd(input);
        
        //var entity = input.Adapt<GameBannerAnnouncementEntity>();
        //await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏内横幅公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteGameBannerAnnouncementEntityInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity);   //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏内横幅公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameBannerAnnouncementEntityInput input)
    {
        await InnerUpdate(input);
        //var entity = input.Adapt<GameBannerAnnouncementEntity>();
        //await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏内横幅公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameBannerAnnouncementEntity> Get([FromQuery] QueryByIdGameBannerAnnouncementEntityInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏内横幅公告列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameBannerAnnouncementEntityOutput>> List([FromQuery] GameBannerAnnouncementEntityInput input)
    {
        return await Repository.AsQueryable().Select<GameBannerAnnouncementEntityOutput>().ToListAsync();
    }



}

