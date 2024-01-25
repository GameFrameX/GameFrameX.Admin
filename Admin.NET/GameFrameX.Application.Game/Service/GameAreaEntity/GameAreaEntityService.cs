namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏区服服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameAreaEntityService : BaseService<GameAreaEntity>
{
    public GameAreaEntityService(SqlSugarRepository<GameAreaEntity> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameAreaEntityOutput>> Page(GameAreaEntityInput input)
    {
        var query= Repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Name.Contains(input.SearchKey.Trim())
                || u.Region.Contains(input.SearchKey.Trim())
                || u.Address.Contains(input.SearchKey.Trim())
                || u.Version.Contains(input.SearchKey.Trim())
                || u.ServerType.Contains(input.SearchKey.Trim())
                || u.Language.Contains(input.SearchKey.Trim())
            )
            .WhereIF(input.IsDefault.HasValue, u => u.IsDefault == input.IsDefault)
            .WhereIF(input.IsEnable.HasValue, u => u.IsEnable == input.IsEnable)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Region), u => u.Region.Contains(input.Region.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Address), u => u.Address.Contains(input.Address.Trim()))
            .WhereIF(input.Port>0, u => u.Port == input.Port)
            .WhereIF(input.Capacity>0, u => u.Capacity == input.Capacity)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Version), u => u.Version.Contains(input.Version.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ServerType), u => u.ServerType.Contains(input.ServerType.Trim()))
            .Select<GameAreaEntityOutput>()
;
        if(input.CreationDateRange != null && input.CreationDateRange.Count >0)
        {
            DateTime? start= input.CreationDateRange[0]; 
            query = query.WhereIF(start.HasValue, u => u.CreationDate > start);
            if (input.CreationDateRange.Count >1 && input.CreationDateRange[1].HasValue)
            {
                var end = input.CreationDateRange[1].Value.AddDays(1);
                query = query.Where(u => u.CreationDate < end);
            }
        } 
        if(input.LastMaintenanceDateRange != null && input.LastMaintenanceDateRange.Count >0)
        {
            DateTime? start= input.LastMaintenanceDateRange[0]; 
            query = query.WhereIF(start.HasValue, u => u.LastMaintenanceDate > start);
            if (input.LastMaintenanceDateRange.Count >1 && input.LastMaintenanceDateRange[1].HasValue)
            {
                var end = input.LastMaintenanceDateRange[1].Value.AddDays(1);
                query = query.Where(u => u.LastMaintenanceDate < end);
            }
        } 
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameAreaEntityInput input)
    {
        await InnerAdd(input);
        
        //var entity = input.Adapt<GameAreaEntity>();
        //await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteGameAreaEntityInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity);   //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameAreaEntityInput input)
    {
        await InnerUpdate(input);
        //var entity = input.Adapt<GameAreaEntity>();
        //await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏区服
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameAreaEntity> Get([FromQuery] QueryByIdGameAreaEntityInput input)
    {
        return await InnerGet(input);
    }

    /// <summary>
    /// 获取游戏区服列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameAreaEntityOutput>> List([FromQuery] GameAreaEntityInput input)
    {
        return await Repository.AsQueryable().Select<GameAreaEntityOutput>().ToListAsync();
    }



}

