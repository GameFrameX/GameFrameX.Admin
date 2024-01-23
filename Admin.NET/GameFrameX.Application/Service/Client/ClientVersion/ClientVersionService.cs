using GameFrameX.Application.Const;
using GameFrameX.Application.Service.Client.ClientVersion.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.Service.Client.ClientVersion;

/// <summary>
/// 版本管理服务
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class ClientVersionService : GetSelectBaseService<Entity.Client.ClientVersion>
{
    private readonly SqlSugarRepository<Entity.Client.ClientVersion> _rep;

    public ClientVersionService(SqlSugarRepository<Entity.Client.ClientVersion> rep) : base(rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientVersionOutput>> Page(ClientVersionInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientVersionOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientVersionInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientVersion>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientVersionInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientVersionInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientVersion>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientVersion> Get([FromQuery] QueryByIdClientVersionInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取版本管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientVersionOutput>> List([FromQuery] ClientVersionInput input)
    {
        return await _rep.AsQueryable().Select<ClientVersionOutput>().ToListAsync();
    }
}