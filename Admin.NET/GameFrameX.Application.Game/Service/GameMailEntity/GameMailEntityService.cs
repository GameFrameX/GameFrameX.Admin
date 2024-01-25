namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏邮件服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameMailEntityService : BaseService<GameMailEntity>
{
    public GameMailEntityService(SqlSugarRepository<GameMailEntity> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameMailEntityOutput>> Page(GameMailEntityInput input)
    {
        var query = Repository.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Subject.Contains(input.SearchKey.Trim())
                    || u.Body.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Subject), u => u.Subject.Contains(input.Subject.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Body), u => u.Body.Contains(input.Body.Trim()))
                .Select<GameMailEntityOutput>()
            ;
        if (input.SendDateRange != null && input.SendDateRange.Count > 0)
        {
            DateTime? start = input.SendDateRange[0];
            query = query.WhereIF(start.HasValue, u => u.SendDate > start);
            if (input.SendDateRange.Count > 1 && input.SendDateRange[1].HasValue)
            {
                var end = input.SendDateRange[1].Value.AddDays(1);
                query = query.Where(u => u.SendDate < end);
            }
        }

        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameMailEntityInput input)
    {
        // await InnerAdd(input);
        var entity = input.Adapt<GameMailEntity>();
        entity.AreaIdList = string.Join(",", input.AreaIdList);
        await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteGameMailEntityInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity); //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameMailEntityInput input)
    {
        // await InnerUpdate(input);
        var entity = input.Adapt<GameMailEntity>();
        await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏邮件
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameMailEntity> Get([FromQuery] QueryByIdGameMailEntityInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏邮件列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameMailEntityOutput>> List([FromQuery] GameMailEntityInput input)
    {
        return await Repository.AsQueryable().Select<GameMailEntityOutput>().ToListAsync();
    }
}