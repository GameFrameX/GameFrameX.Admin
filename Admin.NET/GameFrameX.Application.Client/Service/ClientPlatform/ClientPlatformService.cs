using GameFrameX.Application.Base.Service;
using GameFrameX.Application.Client.Client.ClientPlatform.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.Client.Client.ClientPlatform;

/// <summary>
/// 平台管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class ClientPlatformService : BaseSelectService<Entity.Client.ClientPlatform>
{
    private readonly SqlSugarRepository<Entity.Client.ClientPlatform> _rep;

    public ClientPlatformService(SqlSugarRepository<Entity.Client.ClientPlatform> rep) : base(rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询平台管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientPlatformOutput>> Page(ClientPlatformInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientPlatformOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加平台管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientPlatformInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientPlatform>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除平台管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientPlatformInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新平台管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientPlatformInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientPlatform>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取平台管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientPlatform> Get([FromQuery] QueryByIdClientPlatformInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取平台管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientPlatformOutput>> List([FromQuery] ClientPlatformInput input)
    {
        return await _rep.AsQueryable().Select<ClientPlatformOutput>().ToListAsync();
    }
}