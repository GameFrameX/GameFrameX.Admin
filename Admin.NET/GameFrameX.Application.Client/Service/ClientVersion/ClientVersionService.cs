using GameFrameX.Application.Base.Service;
using GameFrameX.Application.Client.Client.ClientVersion.Dto;
using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Client.Client.ClientVersion;

/// <summary>
/// 版本管理服务
/// </summary>
[ApiDescriptionSettings( Order = 100)]
public class ClientVersionService : BaseSelectService<Entity.Client.ClientVersion>
{

    public ClientVersionService(SqlSugarRepository<Entity.Client.ClientVersion> rep) : base(rep)
    {
       
    }

    /// <summary>
    /// 分页查询版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientVersionPageOutput>> Page(ClientVersionInput input)
    {
        var query = Repository.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientVersionPageOutput>()
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
        await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除版本管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(BaseDeleteClientVersionInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity); //假删除
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
        await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
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
        return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取版本管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientVersionPageOutput>> List([FromQuery] ClientVersionInput input)
    {
        return await Repository.AsQueryable().Select<ClientVersionPageOutput>().ToListAsync();
    }
}