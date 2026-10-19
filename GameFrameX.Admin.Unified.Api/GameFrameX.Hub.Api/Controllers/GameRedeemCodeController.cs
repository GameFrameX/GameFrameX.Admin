using Microsoft.AspNetCore.Authorization;
using GameFrameX.Core.Service;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Hub.Api.Dto.GameRedeemCode;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 物品道具控制器
/// </summary>
[Authorize]
public class GameRedeemCodeController : BaseController<TenantGameRedeemCodeEntity>
{
    private readonly Repository<TenantGameRedeemCodeChannelEntity> _gameRedeemCodeChannelRepository;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    /// <param name="gameRedeemCodeChannelEntity"></param>
    public GameRedeemCodeController(Repository<TenantGameRedeemCodeEntity> service, Repository<TenantGameRedeemCodeChannelEntity> gameRedeemCodeChannelEntity) : base(service)
    {
        _gameRedeemCodeChannelRepository = gameRedeemCodeChannelEntity;
    }

    /// <summary>
    /// 添加兑换码
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(HttpJsonResultData<BaseOperationAckResponse>), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Add([FromBody] TenantGameRedeemCodeEntityByAddRequest input)
    {
        try
        {
            var gameGoodsCatalogueEntity = await _gameRedeemCodeChannelRepository.GetFirstAsync(m => m.Id == input.ChannelId);

            if (gameGoodsCatalogueEntity == null)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("渠道Id不存在=>" + input.ChannelId);
            }

            var gameRedeemCodeEntity = await PRepository.GetFirstAsync(m => m.Code == input.Code);
            if (gameRedeemCodeEntity != null)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("兑换码已经存在=>" + input.Code);
            }

            gameRedeemCodeEntity = input.Adapt<TenantGameRedeemCodeEntity>();

            var gamePropItemEntities = input.Items;

            var newList = new List<GamePropItem>();
            foreach (var attachmentItem in gamePropItemEntities)
            {
                var gameGoodsEntity = await PRepository.AsSugarClient().Queryable<TenantGameGoodsEntity>().SingleAsync(m => m.Id == attachmentItem.Id);
                if (gameGoodsEntity != null)
                {
                    attachmentItem.ItemId = gameGoodsEntity.ConfigId;
                    newList.Add(attachmentItem);
                }
            }

            gameRedeemCodeEntity.Contents = JsonHelper.Serialize(newList);
            gameRedeemCodeEntity.IsActive = true;
            gameRedeemCodeEntity.Channel = await _gameRedeemCodeChannelRepository.AsSugarClient().Queryable<TenantGameRedeemCodeChannelEntity>().FirstAsync(m => m.Id == input.ChannelId);
            var isSuccess = await PRepository.InsertAsync(gameRedeemCodeEntity);
            if (isSuccess)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            return HttpJsonResultData<BaseOperationAckResponse>.Fail("添加兑换码失败");
        }
        catch (Exception ex)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("添加兑换码失败: " + ex.Message);
        }
    }
}