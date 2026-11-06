using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Application.ServerCenter;
using GameFrameX.Application.CommerceCenter.GameOrder;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Client.Api.Integrations.Payment.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiOptions;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 游戏订单控制器
/// </summary>
public partial class GameOrderController : BaseController<GameOrderEntity>
{
    private readonly IClientApiOptionsProvider _optionsProvider;
    private readonly Repository<GameRoleEntity> _gameRoleService;
    private readonly HttpClient _httpClient;
    private readonly GameOrderShipService _shipService;
    private readonly IPaymentChannelFactory _paymentChannelFactory;
    private readonly IGooglePubSubPushAuthenticator _googlePubSubPushAuthenticator;
    private readonly IGooglePlayPurchaseService _googlePlayPurchaseService;

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="optionsProvider"></param>
    /// <param name="gameRoleService"></param>
    /// <param name="httpClient"></param>
    /// <param name="shipService"></param>
    /// <param name="paymentChannelFactory"></param>
    /// <param name="googlePubSubPushAuthenticator"></param>
    /// <param name="googlePlayPurchaseService"></param>
    public GameOrderController(SqlSugarRepository<GameOrderEntity> service,
        IClientApiOptionsProvider optionsProvider,
        Repository<GameRoleEntity> gameRoleService,
        HttpClient httpClient,
        GameOrderShipService shipService,
        IPaymentChannelFactory paymentChannelFactory,
        IGooglePubSubPushAuthenticator googlePubSubPushAuthenticator,
        IGooglePlayPurchaseService googlePlayPurchaseService
    ) : base(service)
    {
        _optionsProvider = optionsProvider;
        _gameRoleService = gameRoleService;
        _httpClient = httpClient;
        _shipService = shipService;
        _paymentChannelFactory = paymentChannelFactory;
        _googlePubSubPushAuthenticator = googlePubSubPushAuthenticator;
        _googlePlayPurchaseService = googlePlayPurchaseService;
    }

    /// <summary>
    /// 增加商品订单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(HttpJsonResultData<AddGameOrderEntityResponse>), 200)]
    public async Task<HttpJsonResultData<AddGameOrderEntityResponse>> Create([FromBody] CreateGameOrderEntityRequest input)
    {
        var gameAreaEntity = await PRepository.AsSugarClient().Queryable<GameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).Where(u => u.Id == input.AreaId).FirstAsync();
        if (gameAreaEntity == null)
        {
            return GameApiResult.Error<AddGameOrderEntityResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
        }

        var gameRoleEntity = await PRepository.AsSugarClient().Queryable<GameRoleEntity>().Where(u => u.Id == input.RoleId).FirstAsync();
        if (gameRoleEntity == null)
        {
            return GameApiResult.Error<AddGameOrderEntityResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        // 调用游戏服务器验证商品ID相关
        var reqPlayerCreateOrderResult = await _httpClient.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.ClientDispatch, "req_player_create_order"), new { RoleId = input.RoleId, ProductId = input.Sku, });
        var resultData = reqPlayerCreateOrderResult.ToHttpJsonResult<PlayerCreateOrderResponse>();
        LogHelper.Info($"req_player_create_order ==> {JsonHelper.Serialize(resultData)}");

        if (resultData.IsSuccess == false)
        {
            return HttpJsonResultData<AddGameOrderEntityResponse>.Error(resultData.Code, resultData.Message);
        }

        // 判断是否需要支付
        if (resultData.Data.IsNeedPay == false)
        {
            // 不需要支付，直接返回
            var addGameOrderEntityOutput = new AddGameOrderEntityResponse
            {
                RoleId = input.RoleId,
                Sku = input.Sku,
                IsNeedPay = false,
            };
            return HttpJsonResultData<AddGameOrderEntityResponse>.Success(addGameOrderEntityOutput);
        }

        var entity = input.Adapt<GameOrderEntity>();
        entity.Id = SnowFlakeIdHelper.Instance.NextId();

        entity.PayChannel = input.PayChannel;
        entity.Amount = resultData.Data.Amount;
        entity.AccountId = gameRoleEntity.UserId.ToString();
        entity.RoleRegisterTime = gameRoleEntity.CreatedTime > 0 ? gameRoleEntity.CreatedTime : (long?)null;
        entity.AdventureSuffix = resultData.Data.AdventureSuffix;
        entity.Content = JsonHelper.Serialize(resultData.Data);
        // 创建成功
        entity.Status = OrderStatusEnum.Unpaid;
        entity.IsSandbox = gameRoleEntity.IsSandbox ?? false;
        HttpContext.Items[PaymentOptionSettingProvider.PaymentIsSandboxKey] = entity.IsSandbox.Value;

        // 检查是否支持该支付渠道
        if (!_paymentChannelFactory.IsSupported(input.PayChannel))
        {
            return GameApiResult.Error<AddGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "不支持的支付渠道");
        }

        // 获取支付渠道处理器
        var paymentHandler = _paymentChannelFactory.GetHandler(input.PayChannel);
        if (paymentHandler == null)
        {
            return GameApiResult.Error<AddGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "支付渠道处理器未找到");
        }

        // 获取支付配置
        IPaymentOptionSetting optionSetting;
        try
        {
            optionSetting = _paymentChannelFactory.GetOptionSetting(input.PayChannel);
        }
        catch (InvalidOperationException exception)
        {
            // S3/C401 断链修复点：把回调路由不完整的拒单原因返回给客户端。
            return GameApiResult.Error<AddGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, exception.Message);
        }
        if (optionSetting == null)
        {
            return GameApiResult.Error<AddGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "支付渠道配置未找到");
        }

        var entityBase = new GameOrderBase
        {
            Sku = entity.Sku.ToString(),
            AreaId = entity.AreaId.ToString(),
            RoleId = entity.RoleId,
            ClientIp = input.ClientIp,
            Country = input.Country,
            OrderId = entity.Id.ToString(),
            Amount = entity.Amount,
            IsSandbox = entity.IsSandbox ?? false,
        };
        // 使用策略模式处理订单创建
        var paymentResult = await paymentHandler.CreateOrderAsync(entityBase, input, gameRoleEntity, _httpClient, optionSetting);
        if (!paymentResult.IsSuccess)
        {
            return HttpJsonResultData<AddGameOrderEntityResponse>.Fail(paymentResult.ErrorMessage);
        }

        var orderEntity = paymentResult.OrderEntity;
        entity.OrderId = orderEntity.OrderId;
        // 保存订单到数据库
        var updatedEntity = await PRepository.InsertReturnEntityAsync(entity);
        // 注意：累充(AllRecharge)不在下单时递增，改为在发货确认支付成功后递增（见 GameOrderShipService.ShipAsync），避免下单刷 VIP 等级
        // 创建输出对象
        var output = updatedEntity.Adapt<AddGameOrderEntityResponse>();

        // 如果支付结果包含额外信息，则合并
        if (paymentResult.Output != null)
        {
            var paymentOutput = paymentResult.Output;
            if (paymentOutput?.Token != null)
            {
                output.Token = paymentOutput.Token;
            }

            if (paymentOutput?.OrderId != null)
            {
                output.OrderId = paymentOutput.OrderId;
            }
        }

        output.RoleId = input.RoleId;
        output.Sku = input.Sku;
        output.PriceId = resultData.Data.PriceId;
        output.IsNeedPay = true;
        // 沙盒环境下，直接发货
        if (entity.IsSandbox == true)
        {
            var orderPolicy = await _optionsProvider.GetOrderPolicyAsync();
            if (orderPolicy.EnableSandBoxRecharge)
            {
                output.IsNeedPay = false;
                entity.Status = OrderStatusEnum.NotShipped;
                await SendOutGoods(entity, true, true);
            }
        }

        return HttpJsonResultData<AddGameOrderEntityResponse>.Success(output);
    }

    /// <summary>
    /// 发货
    /// </summary>
    /// <param name="gameOrderEntity">订单信息</param>
    /// <param name="isRecharge">是否是充值发货</param>
    /// <param name="isSandBox">是否是沙盒充值</param>
    /// <returns></returns>
    private async Task<HttpJsonResultData<BaseOperationAckResponse>> SendOutGoods(GameOrderEntity gameOrderEntity, bool isRecharge = false, bool isSandBox = false)
    {
        return await _shipService.ShipAsync(gameOrderEntity.Id, isRecharge, isSandBox);
    }

}