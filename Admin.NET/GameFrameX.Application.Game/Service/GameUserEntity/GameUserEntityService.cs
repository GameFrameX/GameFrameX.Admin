using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Furion.FriendlyException;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Game;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏账号服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GameUserEntityService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<GameUserEntity> _rep;

    public GameUserEntityService(SqlSugarRepository<GameUserEntity> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<GameUserEntityOutput>> Page(GameUserEntityInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.UserName.Contains(input.SearchKey.Trim())
                    || u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(input.Id > 0, u => u.Id == input.Id)
                .WhereIF(!string.IsNullOrWhiteSpace(input.UserName), u => u.UserName.Contains(input.UserName.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<GameUserEntityOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddGameUserEntityInput input)
    {
        var entity = input.Adapt<GameUserEntity>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteGameUserEntityInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateGameUserEntityInput input)
    {
        var entity = input.Adapt<GameUserEntity>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<GameUserEntity> Get([FromQuery] QueryByIdGameUserEntityInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏账号列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<GameUserEntityOutput>> List([FromQuery] GameUserEntityInput input)
    {
        return await _rep.AsQueryable().Select<GameUserEntityOutput>().ToListAsync();
    }
}