using GameFrameX.Client.Api.GameUserReward.Dto;
using GameFrameX.Client.Api.GameUserReward.Models;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserReward;

/// <summary>
/// 游戏玩家奖励控制器（C8 奖励域：BLGetUserRewards / BLReceiveRewards1 自建）
/// <para>路由：POST /api/GameUserReward/{Action}（继承 BaseController 基类路由）。</para>
/// </summary>
public class GameUserRewardController : BaseController
{
    private readonly Repository<TenantGameUserRewardEntity> _rewardRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="rewardRepository">奖励记录仓储</param>
    public GameUserRewardController(Repository<TenantGameUserRewardEntity> rewardRepository)
    {
        _rewardRepository = rewardRepository;
    }

    /// <summary>
    /// 查询奖励记录状态（原 BLGetUserRewards；响应无 rcode，空数组合法）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GetUserRewardsResponse>> GetUserRewards([FromBody] GetUserRewardsRequest request)
    {
        // 只返回服务端已存在的 key（契约：查不到的 key 不返回元素，空数组合法）
        var entities = await _rewardRepository.GetListAsync(x => x.UserId == request.UserId && request.RewardKeys.Contains(x.RewardKey));
        var response = new GetUserRewardsResponse();
        foreach (var entity in entities)
        {
            response.Rewards.Add(new GameUserRewardInfo
            {
                Key = entity.RewardKey,
                Content = entity.Content,
                Status = entity.Status,
            });
        }

        return HttpJsonResultData<GetUserRewardsResponse>.Success(response);
    }

    /// <summary>
    /// 领取奖励（原 BLReceiveRewards1；Rcode 仅允许 0/1/2，token 同重放同结果）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<ReceiveRewardsResponse>> ReceiveRewards([FromBody] ReceiveRewardsRequest request)
    {
        var entity = await _rewardRepository.GetFirstAsync(x => x.UserId == request.UserId && x.RewardKey == request.Key);
        if (entity == null)
        {
            // 无记录（check_exist=0 问卷/mail 允许创建；check_exist=1 的 op_cp 记录自建模型下也无预建路径，
            // 统一创建 + rcode 0——客户端发奖用本地任务配置，不受服务端 content 影响）
            entity = new TenantGameUserRewardEntity
            {
                UserId = request.UserId,
                RewardKey = request.Key,
                Status = 1,
                LastToken = request.Token,
                ReceivedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Name = request.Key,
            };
            await _rewardRepository.InsertAsync(entity);
            return HttpJsonResultData<ReceiveRewardsResponse>.Success(new ReceiveRewardsResponse { Rcode = 0, });
        }

        if (entity.Status == 1)
        {
            // token 幂等：同 token 重放返回首次结果 0（客户端响应丢失重试时补发本地奖励；
            // 返回 1 会让客户端跳过发奖造成丢奖）；换新 token 才是真正的重复领取 → 1
            var rcode = entity.LastToken == request.Token ? 0 : 1;
            return HttpJsonResultData<ReceiveRewardsResponse>.Success(new ReceiveRewardsResponse { Rcode = rcode, });
        }

        entity.Status = 1;
        entity.LastToken = request.Token;
        entity.ReceivedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _rewardRepository.UpdateAsync(entity);
        return HttpJsonResultData<ReceiveRewardsResponse>.Success(new ReceiveRewardsResponse { Rcode = 0, });
    }
}