using GameFrameX.Application.Client.Client.ClientGlobalInfo.Dto;
using GameFrameX.Core.Extension;

namespace GameFrameX.Application.Client.Client.ClientGlobalInfo;

/// <summary>
/// 全局信息管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class ClientGlobalInfoService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<Entity.Client.ClientGlobalInfo> _rep;

    public ClientGlobalInfoService(SqlSugarRepository<Entity.Client.ClientGlobalInfo> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询全局信息管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientGlobalInfoOutput>> Page(ClientGlobalInfoInput input)
    {
        var query = _rep.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Channel.Contains(input.SearchKey.Trim())
                    || u.Content.Contains(input.SearchKey.Trim())
                    || u.CheckResourceUrl.Contains(input.SearchKey.Trim())
                    || u.CheckAppVersionUrl.Contains(input.SearchKey.Trim())
                    || u.Platform.Contains(input.SearchKey.Trim())
                    || u.Version.Contains(input.SearchKey.Trim())
                    || u.Package.Contains(input.SearchKey.Trim())
                    || u.Language.Contains(input.SearchKey.Trim())
                    || u.CreateUserName.Contains(input.SearchKey.Trim())
                    || u.UpdateUserName.Contains(input.SearchKey.Trim())
                    || u.Description.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Channel), u => u.Channel.Contains(input.Channel.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Content), u => u.Content.Contains(input.Content.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.CheckResourceUrl), u => u.CheckResourceUrl.Contains(input.CheckResourceUrl.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.CheckAppVersionUrl), u => u.CheckAppVersionUrl.Contains(input.CheckAppVersionUrl.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Platform), u => u.Platform.Contains(input.Platform.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Version), u => u.Version.Contains(input.Version.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Package), u => u.Package.Contains(input.Package.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Language), u => u.Language.Contains(input.Language.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
                .Select<ClientGlobalInfoOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加全局信息管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientGlobalInfoInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientGlobalInfo>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除全局信息管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteClientGlobalInfoInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity); //假删除
        //await _rep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新全局信息管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientGlobalInfoInput input)
    {
        var entity = input.Adapt<Entity.Client.ClientGlobalInfo>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取全局信息管理
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.Client.ClientGlobalInfo> Get([FromQuery] QueryByIdClientGlobalInfoInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取全局信息管理列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientGlobalInfoOutput>> List([FromQuery] ClientGlobalInfoInput input)
    {
        return await _rep.AsQueryable().Select<ClientGlobalInfoOutput>().ToListAsync();
    }
}