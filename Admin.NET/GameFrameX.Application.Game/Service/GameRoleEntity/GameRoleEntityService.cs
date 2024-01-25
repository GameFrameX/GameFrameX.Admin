using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Game;
/// <summary>
/// 游戏角色服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameRoleEntityService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<GameRoleEntity> _rep;
    public GameRoleEntityService(SqlSugarRepository<GameRoleEntity> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameRoleEntityOutput>> Page(GameRoleEntityInput input)
    {
        var query= _rep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                u.Name.Contains(input.SearchKey.Trim())
            )
            .WhereIF(input.UserId>0, u => u.UserId == input.UserId)
            .WhereIF(input.AreaId>0, u => u.AreaId == input.AreaId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
            .WhereIF(input.Level>0, u => u.Level == input.Level)
            .Select<GameRoleEntityOutput>()
;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameRoleEntityInput input)
    {
        var entity = input.Adapt<GameRoleEntity>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteGameRoleEntityInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameRoleEntityInput input)
    {
        var entity = input.Adapt<GameRoleEntity>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameRoleEntity> Get([FromQuery] QueryByIdGameRoleEntityInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏角色列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameRoleEntityOutput>> List([FromQuery] GameRoleEntityInput input)
    {
        return await _rep.AsQueryable().Select<GameRoleEntityOutput>().ToListAsync();
    }





}

