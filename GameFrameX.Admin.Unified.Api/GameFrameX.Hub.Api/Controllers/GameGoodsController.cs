using GameFrameX.Core.Service;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Hub.Api.Dto.GameGoodsEntity;
using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 物品道具控制器
/// </summary>
[Authorize]
public class GameGoodsController : BaseController<TenantGameGoodsEntity>
{
    private readonly Repository<TenantGameGoodsCatalogueEntity> _gameGoodsRepository;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    /// <param name="gameGoodsCatalogueEntity"></param>
    public GameGoodsController(Repository<TenantGameGoodsEntity> service, Repository<TenantGameGoodsCatalogueEntity> gameGoodsCatalogueEntity) : base(service)
    {
        _gameGoodsRepository = gameGoodsCatalogueEntity;
    }

    /// <summary>
    /// 添加或更新道具信息
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(HttpJsonResultData<BaseOperationAckResponse>), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> AddOrUpdate([FromBody] AddOrUpdateTenantGameGoodsEntityRequest addOrUpdateTenantGameGoodsEntityInput)
    {
        var gameGoodsCatalogueEntity = await _gameGoodsRepository.GetFirstAsync(m => m.Id == addOrUpdateTenantGameGoodsEntityInput.ClassifyId);

        if (gameGoodsCatalogueEntity == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("分类Id不存在=>" + addOrUpdateTenantGameGoodsEntityInput.ClassifyId);
        }

        var gameGoodsEntity = await PRepository.GetFirstAsync(m => m.ConfigId == addOrUpdateTenantGameGoodsEntityInput.ConfigId);
        if (gameGoodsEntity == null)
        {
            gameGoodsEntity = addOrUpdateTenantGameGoodsEntityInput.Adapt<TenantGameGoodsEntity>();
            await PRepository.InsertAsync(gameGoodsEntity);
        }
        else
        {
            var id = gameGoodsEntity.Id;
            gameGoodsEntity = addOrUpdateTenantGameGoodsEntityInput.Adapt(gameGoodsEntity);
            gameGoodsEntity.Id = id;
            await PRepository.UpdateWithOptimisticLockAsync(gameGoodsEntity);
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }
}