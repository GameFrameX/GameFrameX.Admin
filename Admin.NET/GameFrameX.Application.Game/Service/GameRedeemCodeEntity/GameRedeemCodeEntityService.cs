namespace GameFrameX.Application.Game;

/// <summary>
/// 兑换码服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameRedeemCodeEntityService : BaseService<GameRedeemCodeEntity>
{
    public GameRedeemCodeEntityService(SqlSugarRepository<GameRedeemCodeEntity> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameRedeemCodeEntityOutput>> Page(GameRedeemCodeEntityInput input)
    {
        var query= Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Code.Contains(input.SearchKey.Trim())
            )
            .WhereIF(input.AreaId>0, u => u.AreaId == input.AreaId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code.Trim()))
            .WhereIF(input.UsedCount>0, u => u.UsedCount == input.UsedCount)
            .Select<GameRedeemCodeEntityOutput>()
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
    /// 增加兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameRedeemCodeEntityInput input)
    {
        await InnerAdd(input);
        
        //var entity = input.Adapt<GameRedeemCodeEntity>();
        //await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteGameRedeemCodeEntityInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity);   //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameRedeemCodeEntityInput input)
    {
        await InnerUpdate(input);
        //var entity = input.Adapt<GameRedeemCodeEntity>();
        //await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameRedeemCodeEntity> Get([FromQuery] QueryByIdGameRedeemCodeEntityInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取兑换码列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameRedeemCodeEntityOutput>> List([FromQuery] GameRedeemCodeEntityInput input)
    {
        return await Repository.AsQueryable().Select<GameRedeemCodeEntityOutput>().ToListAsync();
    }



}

