namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class ClientAssetPackageVersionService : BaseService<ClientAssetPackageVersion>
{
    protected readonly SqlSugarRepository<ClientAssetPackageVersionHistory> RepositoryAssetPackageVersionHistory;

    public ClientAssetPackageVersionService(SqlSugarRepository<ClientAssetPackageVersion> rep, SqlSugarRepository<ClientAssetPackageVersionHistory> rep2) : base(rep)
    {
        RepositoryAssetPackageVersionHistory = rep2;
    }

    /// <summary>
    /// 分页查询资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientAssetPackageVersionOutput>> Page(ClientAssetPackageVersionInput input)
    {
        var query = Repository.AsQueryable()
                .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                    u.Channel.Contains(input.SearchKey.Trim())
                    || u.Platform.Contains(input.SearchKey.Trim())
                    || u.AppVersion.Contains(input.SearchKey.Trim())
                    || u.Package.Contains(input.SearchKey.Trim())
                    || u.Language.Contains(input.SearchKey.Trim())
                    || u.AssetPackageName.Contains(input.SearchKey.Trim())
                )
                .WhereIF(!string.IsNullOrWhiteSpace(input.Channel), u => u.Channel.Contains(input.Channel.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Platform), u => u.Platform.Contains(input.Platform.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.AppVersion), u => u.AppVersion.Contains(input.AppVersion.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Package), u => u.Package.Contains(input.Package.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.Language), u => u.Language.Contains(input.Language.Trim()))
                .WhereIF(!string.IsNullOrWhiteSpace(input.AssetPackageName), u => u.AssetPackageName.Contains(input.AssetPackageName.Trim()))
                .Select<ClientAssetPackageVersionOutput>()
            ;
        query = query.OrderBuilder(input, "", "CreateTime");
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddClientAssetPackageVersionInput input)
    {
        var result = await InnerAdd(input);
        var historyInput = new AddClientAssetPackageVersionHistoryInput();
        var entity = historyInput.Adapt<ClientAssetPackageVersionHistory>();
        entity.CreateUser = result.CreateUser;
        entity.CreateTime = result.CreateTime;
        entity.CreateUserId = result.CreateUserId;
        entity.CreateUserName = result.CreateUserName;
        entity.Version = input.Version;
        entity.OldVersion = string.Empty;
        entity.VersionId = result.Id;
        await RepositoryAssetPackageVersionHistory.InsertAsync(entity);
    }

    /// <summary>
    /// 删除资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DisableDelete")]
    public async Task DisableDelete(DeleteClientAssetPackageVersionInput input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await Repository.FakeDeleteAsync(entity); //假删除
        //await Repository.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 更新资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateClientAssetPackageVersionInput input)
    {
        var after = await Repository.GetByIdAsync(input.Id);

        var result = await InnerUpdate(input);

        if (input.Version == after.Version)
        {
            return;
        }

        var historyInput = new AddClientAssetPackageVersionHistoryInput();
        var entity = historyInput.Adapt<ClientAssetPackageVersionHistory>();
        entity.CreateUser = result.CreateUser;
        entity.CreateTime = result.CreateTime;
        entity.CreateUserId = result.CreateUserId;
        entity.CreateUserName = result.CreateUserName;
        entity.Version = input.Version;
        entity.OldVersion = after.Version;
        entity.VersionId = input.Id;
        await RepositoryAssetPackageVersionHistory.InsertAsync(entity);
    }

    /// <summary>
    /// 获取资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<ClientAssetPackageVersion> Get([FromQuery] QueryByIdClientAssetPackageVersionInput input)
    {
        return await InnerGet(input);
        //return await Repository.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取资源包版本列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<ClientAssetPackageVersionOutput>> List([FromQuery] ClientAssetPackageVersionInput input)
    {
        return await Repository.AsQueryable().Select<ClientAssetPackageVersionOutput>().ToListAsync();
    }
}