namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本记录服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class ClientAssetPackageVersionHistoryService : BaseService<ClientAssetPackageVersionHistory>
{
    public ClientAssetPackageVersionHistoryService(SqlSugarRepository<ClientAssetPackageVersionHistory> rep) : base(rep)
    {
    }

    /// <summary>
    /// 分页查询资源包版本记录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<ClientAssetPackageVersionHistoryOutput>> Page(ClientAssetPackageVersionHistoryInput input)
    {
        var query= Repository.AsQueryable()
            .WhereIF(input.VersionId>0, u => u.VersionId == input.VersionId)
            .Select<ClientAssetPackageVersionHistoryOutput>()
;
        query = query.OrderBuilder(input, "", "CreateTime");
        
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
}

