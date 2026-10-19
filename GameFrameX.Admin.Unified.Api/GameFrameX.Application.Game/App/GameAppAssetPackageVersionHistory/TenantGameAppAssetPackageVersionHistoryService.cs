using GameFrameX.Application.Game.App.GameAppAssetPackageVersionHistory.Dto;

namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersionHistory;

/// <summary>
/// 资源包版本记录服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppAssetPackageVersionHistoryService : GenericCrudService<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity, AddGameAppAssetPackageVersionHistoryRequest, BaseUpdateRequest, GameAppAssetPackageVersionHistoryResponse, PageGameAppAssetPackageVersionHistoryRequest>
{
    public TenantGameAppAssetPackageVersionHistoryService(SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppAssetPackageVersionHistoryResponse> BuildPageQuery(PageGameAppAssetPackageVersionHistoryRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(input.VersionId > 0, u => u.VersionId == input.VersionId)
                         .Select<GameAppAssetPackageVersionHistoryResponse>();
    }
}