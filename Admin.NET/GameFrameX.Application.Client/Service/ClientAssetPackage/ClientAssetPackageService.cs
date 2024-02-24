namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class ClientAssetPackageService : BaseSelectService<ClientAssetPackage>
{
    public ClientAssetPackageService(SqlSugarRepository<ClientAssetPackage> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询资源包
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientAssetPackageOutput>> Page(ClientAssetPackageInput input)
    {
        var query = Repository.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Name.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                .Select<ClientAssetPackageOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加资源包
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientAssetPackageInput input)
    {
        await InnerAdd(input);

        //var entity = input.Adapt<ClientAssetPackage>();
        //await Repository.InsertAsync(entity);
    }

    /// <summary>
    /// 删除资源包
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteClientAssetPackageInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity); //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新资源包
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientAssetPackageInput input)
    {
        await InnerUpdate(input);
        //var entity = input.Adapt<ClientAssetPackage>();
        //await Repository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取资源包
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<ClientAssetPackage> Get([FromQuery] QueryByIdClientAssetPackageInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取资源包列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientAssetPackageOutput>> List([FromQuery] ClientAssetPackageInput input)
    {
        return await Repository.AsQueryable().Select<ClientAssetPackageOutput>().ToListAsync();
    }
}