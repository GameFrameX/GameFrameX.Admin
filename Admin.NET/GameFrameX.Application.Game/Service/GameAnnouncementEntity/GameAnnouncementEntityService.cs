namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏公告服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameAnnouncementEntityService : BaseService<GameAnnouncementEntity>
{
    public GameAnnouncementEntityService(SqlSugarRepository<GameAnnouncementEntity> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询游戏公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameAnnouncementEntityOutput>> Page(GameAnnouncementEntityInput input)
    {
        var query= Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Title.Contains(input.SearchKey.Trim())
                || u.Content.Contains(input.SearchKey.Trim())
            )
            .WhereIF(input.AreaId>0, u => u.AreaId == input.AreaId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Title.Contains(input.Title.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Content), u => u.Content.Contains(input.Content.Trim()))
            .WhereIF(input.Priority>0, u => u.Priority == input.Priority)
            .Select<GameAnnouncementEntityOutput>()
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
    /// 增加游戏公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameAnnouncementEntityInput input)
    {
        await InnerAdd(input);
        
        //var entity = input.Adapt<GameAnnouncementEntity>();
        //await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteGameAnnouncementEntityInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity);   //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameAnnouncementEntityInput input)
    {
        await InnerUpdate(input);
        //var entity = input.Adapt<GameAnnouncementEntity>();
        //await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameAnnouncementEntity> Get([FromQuery] QueryByIdGameAnnouncementEntityInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏公告列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameAnnouncementEntityOutput>> List([FromQuery] GameAnnouncementEntityInput input)
    {
        return await Repository.AsQueryable().Select<GameAnnouncementEntityOutput>().ToListAsync();
    }



}

