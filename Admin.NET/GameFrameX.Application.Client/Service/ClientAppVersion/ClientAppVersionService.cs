using GameFrameX.Application.Client.Client.ClientAppVersion.Dto;
using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Client.Client.ClientAppVersion;

/// <summary>
/// 游戏版本管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class ClientAppVersionService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<Entity.Client.ClientAppVersion> _rep;

    public ClientAppVersionService(SqlSugarRepository<Entity.Client.ClientAppVersion> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询游戏版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientAppVersionOutput>> Page(ClientAppVersionInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Description.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Channel), u => u.Channel == input.Channel)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Platform), u => u.Platform == input.Platform)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Version), u => u.Version == input.Version)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Package), u => u.Package == input.Package)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Language), u => u.Language == input.Language)
                .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
                .Select<ClientAppVersionOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加游戏版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientAppVersionInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientAppVersion>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除游戏版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientAppVersionInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新游戏版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientAppVersionInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientAppVersion>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取游戏版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientAppVersion> Get([FromQuery] QueryByIdClientAppVersionInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取游戏版本管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientAppVersionOutput>> List([FromQuery] ClientAppVersionInput input)
    {
        return await _rep.AsQueryable().Select<ClientAppVersionOutput>().ToListAsync();
    }
}