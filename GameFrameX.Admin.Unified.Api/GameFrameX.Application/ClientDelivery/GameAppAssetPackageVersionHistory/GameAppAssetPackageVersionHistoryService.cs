using GameFrameX.Application.ClientDelivery.GameAppAssetPackageVersionHistory.Dto;

namespace GameFrameX.Application.ClientDelivery.GameAppAssetPackageVersionHistory;

/// <summary>
/// 资源包版本记录服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class GameAppAssetPackageVersionHistoryService : GenericCrudService<Entity.Business.Game.Release.GameAppAssetPackageVersionHistoryEntity, AddGameAppAssetPackageVersionHistoryRequest, BaseUpdateRequest, GameAppAssetPackageVersionHistoryResponse, PageGameAppAssetPackageVersionHistoryRequest>
{
    public GameAppAssetPackageVersionHistoryService(SqlSugarRepository<Entity.Business.Game.Release.GameAppAssetPackageVersionHistoryEntity> repository) : base(repository)
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