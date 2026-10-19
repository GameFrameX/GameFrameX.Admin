using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Client.Api.GameRedeemCode.Dto;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Client.Api.GameRedeemCode;

/// <summary>
/// 兑换码控制器
/// </summary>
public class GameRedeemCodeController : BaseController<TenantGameRedeemCodeEntity>
{
    private readonly Repository<TenantGameRedeemCodeUseRecordEntity> _recordService;
    private readonly Repository<TenantGameRedeemCodeChannelEntity> _channelRepository;
    private readonly Repository<TenantGameRedeemCodeChannelRoleUsageEntity> _channelRoleUsageRepository;
    private readonly HttpClient _httpClientFactory;
    private readonly Repository<TenantGameAreaEntity> _areaRepository;

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="recordService"></param>
    /// <param name="channelRepository"></param>
    /// <param name="channelRoleUsageRepository"></param>
    /// <param name="areaService"></param>
    /// <param name="httpClientFactory"></param>
    public GameRedeemCodeController(
        SqlSugarRepository<TenantGameRedeemCodeEntity> service,
        Repository<TenantGameRedeemCodeUseRecordEntity> recordService,
        Repository<TenantGameRedeemCodeChannelEntity> channelRepository,
        Repository<TenantGameRedeemCodeChannelRoleUsageEntity> channelRoleUsageRepository,
        Repository<TenantGameAreaEntity> areaService,
        HttpClient httpClientFactory) : base(service)
    {
        _recordService = recordService;
        _channelRepository = channelRepository;
        _channelRoleUsageRepository = channelRoleUsageRepository;
        _httpClientFactory = httpClientFactory;
        _areaRepository = areaService;
    }

    /// <summary>
    /// 消费兑换码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseOperationAckResponse), 200)]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Consume([FromBody] GameRedeemCodeConsumeEntityRequest input)
    {
        // 应用作用域：AppId 只来自可信中间件上下文，不接受请求体覆盖（app-scope-backend-foundation.md §5.2）
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "游戏兑换码使用记录");

        var gameRedeemCodeEntity = await PRepository.GetAsync(m => m.Code == input.Code);
        if (gameRedeemCodeEntity == null)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeNotFound, "兑换码不存在");
        }

        // 兑换码必须属于当前应用，禁止跨应用兑换
        if (gameRedeemCodeEntity.AppId != appId)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeNotFound, "兑换码不存在");
        }

        if (!gameRedeemCodeEntity.IsActive)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeNotActive, "兑换码未激活");
        }

        if (gameRedeemCodeEntity.UsageLimit > 0 && gameRedeemCodeEntity.UsageLimit <= gameRedeemCodeEntity.UsedCount)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeLimit, "兑换码次数已用完");
        }

        if (!(gameRedeemCodeEntity.StartTime < TimerHelper.UnixTimeSeconds() && gameRedeemCodeEntity.EndTime > TimerHelper.UnixTimeSeconds()))
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeExpired, "兑换码已过期");
        }

        TenantGameAreaEntity gameAreaEntity;
        // 如果兑换码没有指定区服，则使用默认区服
        if (gameRedeemCodeEntity.AreaId <= 0)
        {
            // 获取传入区服信息
            gameAreaEntity = await _areaRepository.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(m => m.Id == input.AreaId);
            if (gameAreaEntity == null)
            {
                return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
            }
        }
        else
        {
            // 获取兑换码的指定区服信息
            gameAreaEntity = await _areaRepository.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).SingleAsync(m => m.Id == gameRedeemCodeEntity.AreaId);
            if (gameAreaEntity == null)
            {
                return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
            }
        }

        // 查询消费记录
        var hasExits = await _recordService.AsQueryable().AnyAsync(entity => entity.Code == gameRedeemCodeEntity.Code && entity.PlayerId == input.RoleId);

        if (hasExits)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeUsed, "兑换码已使用");
        }

        // 组级角色次数预检：兑换码归属渠道/分组时，按 (TenantId, AppId, ChannelId, RoleId) 维度校验每角色可用次数。
        // 客户端请求体不传 ChannelId，服务端从兑换码反推，禁止跨应用或已删除渠道兑换。
        var channelContext = await ResolveChannelRoleUsageAsync(appId, gameRedeemCodeEntity, input.RoleId);
        if (channelContext.Rejected)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeNotFound, "兑换码不存在");
        }

        if (channelContext.LimitReached)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeChannelRoleLimit, "兑换码分组次数已用完");
        }

        var propItems = JsonHelper.Deserialize<List<PropertyItemResponse>>(gameRedeemCodeEntity.Contents);
        foreach (var propertyItem in propItems)
        {
            propertyItem.Id = propertyItem.ItemId;
        }

        var reqPlayerSendItemRequest = new PlayerSendItemRequest
        {
            PlayerId = input.RoleId,
            Items = propItems,
        };

        // 和游戏服务器交互发送奖励
        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_send_item"), reqPlayerSendItemRequest);
        LogHelper.Info($"兑换码消费返回:{content}");
        var respPlayerSendItem = content.ToHttpJsonResult<PlayerSendItemResponse>();
        if (respPlayerSendItem.IsSuccess)
        {
            // 更新兑换码信息
            gameRedeemCodeEntity.UsedCount++;
            // 更新兑换码使用记录
            var gameRedeemCodeUseRecord = new TenantGameRedeemCodeUseRecordEntity
            {
                AppId = appId, // 事实数据应用作用域，来自可信中间件上下文
                AreaId = gameAreaEntity.Id,
                PlayerId = input.RoleId,
                Code = gameRedeemCodeEntity.Code,
            };
            var isSuccess = await _recordService.InsertAsync(gameRedeemCodeUseRecord);
            if (isSuccess)
            {
                await PRepository.AsUpdateable(gameRedeemCodeEntity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);

                // 组级角色次数累计：发奖成功后递增渠道角色使用累计，与使用记录/兑换码 UsedCount 同一成功路径写入。
                await AccumulateChannelRoleUsageAsync(channelContext, gameRedeemCodeEntity, input.RoleId);
            }

            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        // 兑换失败
        return GameApiResult.Error<BaseOperationAckResponse>(RedeemCodeErrorCode.RedeemCodeInvokeError, respPlayerSendItem.Message);
    }

    /// <summary>
    /// 解析兑换码归属渠道/分组并预检组级角色次数。客户端不传 ChannelId，服务端从兑换码反推。
    /// <para>无 ChannelId / 渠道未配置限制时返回 Neutral；渠道不存在或跨应用返回 Rejected；次数达上限返回 LimitReached。</para>
    /// </summary>
    private async Task<ChannelRoleUsageContext> ResolveChannelRoleUsageAsync(long appId, TenantGameRedeemCodeEntity code, long roleId)
    {
        if (!code.ChannelId.HasValue || code.ChannelId.Value <= 0)
        {
            return ChannelRoleUsageContext.Neutral();
        }

        var channel = await _channelRepository.GetSingleAsync(c => c.Id == code.ChannelId.Value);
        // 渠道不存在或跨应用：按配置异常安全拒绝（不向客户端泄露具体原因）
        if (channel == null || channel.AppId != appId)
        {
            return ChannelRoleUsageContext.AsRejected();
        }

        if (channel.RoleUsageLimit <= 0)
        {
            return ChannelRoleUsageContext.Neutral();
        }

        var existing = await _channelRoleUsageRepository.GetFirstAsync(u =>
                                                                           u.AppId == appId && u.ChannelId == channel.Id && u.RoleId == roleId);

        if (existing != null && existing.UsedCount >= channel.RoleUsageLimit)
        {
            return ChannelRoleUsageContext.AsLimitReached();
        }

        return ChannelRoleUsageContext.Tracked(channel, existing);
    }

    /// <summary>
    /// 发奖成功后递增组级角色使用累计。
    /// <para>事务边界：与使用记录插入、兑换码 UsedCount 递增同处发奖成功路径；累计写失败不回滚已发奖励，由日志显性化（与既有使用记录写失败同款风险）。</para>
    /// <para>并发兜底：累计表 ux_redeem_channel_role_usage 唯一约束保证 (TenantId, AppId, ChannelId, RoleId) 只有一行；递增走条件更新 WHERE UsedCount &lt; limit 避免超计数。</para>
    /// </summary>
    private async Task AccumulateChannelRoleUsageAsync(ChannelRoleUsageContext context, TenantGameRedeemCodeEntity code, long roleId)
    {
        if (!context.ShouldAccumulate)
        {
            return;
        }

        var now = TimerHelper.UnixTimeSeconds();
        if (context.Existing == null)
        {
            // 预检时该角色尚无累计行；唯一约束兜底并发插入，竞争失败方回落到条件递增。
            try
            {
                await _channelRoleUsageRepository.InsertAsync(new TenantGameRedeemCodeChannelRoleUsageEntity
                {
                    AppId = context.AppId,
                    ChannelId = context.ChannelId,
                    RoleId = roleId,
                    UsedCount = 1,
                    LastCode = code.Code,
                    LastUseTime = now,
                });
                return;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, $"兑换码渠道角色累计插入失败，回落条件递增。code={code.Code} channelId={context.ChannelId} roleId={roleId}");
            }
        }

        // 条件递增：仅当当前累计未达上限时 +1，避免并发竞争导致超计数
        var limit = context.RoleUsageLimit;
        await _channelRoleUsageRepository.AsUpdateable()
                                         .SetColumns(u => new TenantGameRedeemCodeChannelRoleUsageEntity
                                         {
                                             UsedCount = u.UsedCount + 1,
                                             LastCode = code.Code,
                                             LastUseTime = now,
                                         })
                                         .Where(u => u.AppId == context.AppId && u.ChannelId == context.ChannelId && u.RoleId == roleId && u.UsedCount < limit)
                                         .ExecuteCommandAsync();
    }

    /// <summary>
    /// 从 HttpContext.Items 读取长整型上下文值（租户 / 应用等）。
    /// </summary>
    private bool TryGetContextLong(string key, out long value)
    {
        value = 0;
        if (!HttpContext.Items.TryGetValue(key, out var item) || item == null)
        {
            return false;
        }

        return long.TryParse(item.ToString(), out value);
    }

    private struct ChannelRoleUsageContext
    {
        public bool Rejected { get; private set; }

        public bool LimitReached { get; private set; }

        public bool ShouldAccumulate { get; private set; }

        public long AppId { get; private set; }

        public long ChannelId { get; private set; }

        public int RoleUsageLimit { get; private set; }

        public TenantGameRedeemCodeChannelRoleUsageEntity? Existing { get; private set; }

        public static ChannelRoleUsageContext Neutral()
        {
            return default;
        }

        public static ChannelRoleUsageContext AsRejected()
        {
            return new ChannelRoleUsageContext { Rejected = true, };
        }

        public static ChannelRoleUsageContext AsLimitReached()
        {
            return new ChannelRoleUsageContext { LimitReached = true, };
        }

        public static ChannelRoleUsageContext Tracked(TenantGameRedeemCodeChannelEntity channel, TenantGameRedeemCodeChannelRoleUsageEntity? existing)
        {
            return new ChannelRoleUsageContext
            {
                ShouldAccumulate = true,
                AppId = channel.AppId,
                ChannelId = channel.Id,
                RoleUsageLimit = channel.RoleUsageLimit,
                Existing = existing,
            };
        }
    }
}