using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Client.Api.Integrations.Payment.Dto;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 游戏订单控制器
/// </summary>
public partial class GameOrderController : BaseController<TenantGameOrderEntity>
{
    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly Repository<TenantGameRoleEntity> _gameRoleService;
    private readonly HttpClient _httpClient;
    private readonly TenantGameOrderShipService _shipService;
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
    public GameOrderController(SqlSugarRepository<TenantGameOrderEntity> service,
        IClientApiTenantOptionsProvider optionsProvider,
        Repository<TenantGameRoleEntity> gameRoleService,
        HttpClient httpClient,
        TenantGameOrderShipService shipService,
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
    [ProducesResponseType(typeof(HttpJsonResultData<AddTenantGameOrderEntityResponse>), 200)]
    public async Task<HttpJsonResultData<AddTenantGameOrderEntityResponse>> Create([FromBody] CreateTenantGameOrderEntityRequest input)
    {
        // 应用作用域：AppId 只来自可信中间件上下文，不接受请求体覆盖（app-scope-backend-foundation.md §5.2）
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "游戏订单");

        var gameAreaEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).Where(u => u.Id == input.AreaId).FirstAsync();
        if (gameAreaEntity == null)
        {
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
        }

        var gameRoleEntity = await PRepository.AsSugarClient().Queryable<TenantGameRoleEntity>().Where(u => u.Id == input.RoleId).FirstAsync();
        if (gameRoleEntity == null)
        {
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        // 调用游戏服务器验证商品ID相关
        var reqPlayerCreateOrderResult = await _httpClient.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.ClientDispatch, "req_player_create_order"), new { RoleId = input.RoleId, ProductId = input.Sku, });
        var resultData = reqPlayerCreateOrderResult.ToHttpJsonResult<PlayerCreateOrderResponse>();
        LogHelper.Info($"req_player_create_order ==> {JsonHelper.Serialize(resultData)}");

        if (resultData.IsSuccess == false)
        {
            return HttpJsonResultData<AddTenantGameOrderEntityResponse>.Error(resultData.Code, resultData.Message);
        }

        // 判断是否需要支付
        if (resultData.Data.IsNeedPay == false)
        {
            // 不需要支付，直接返回
            var addTenantGameOrderEntityOutput = new AddTenantGameOrderEntityResponse
            {
                RoleId = input.RoleId,
                Sku = input.Sku,
                IsNeedPay = false,
            };
            return HttpJsonResultData<AddTenantGameOrderEntityResponse>.Success(addTenantGameOrderEntityOutput);
        }

        var entity = input.Adapt<TenantGameOrderEntity>();
        entity.Id = SnowFlakeIdHelper.Instance.NextId();
        entity.AppId = appId; // 事实数据应用作用域，来自可信中间件上下文
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
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "不支持的支付渠道");
        }

        // 获取支付渠道处理器
        var paymentHandler = _paymentChannelFactory.GetHandler(input.PayChannel);
        if (paymentHandler == null)
        {
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "支付渠道处理器未找到");
        }

        // 获取支付配置
        var optionSetting = _paymentChannelFactory.GetOptionSetting(input.PayChannel);
        if (optionSetting == null)
        {
            return GameApiResult.Error<AddTenantGameOrderEntityResponse>(PaymentErrorCode.PayChannelNotSupported, "支付渠道配置未找到");
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
            return HttpJsonResultData<AddTenantGameOrderEntityResponse>.Fail(paymentResult.ErrorMessage);
        }

        var orderEntity = paymentResult.OrderEntity;
        entity.OrderId = orderEntity.OrderId;
        // 保存订单到数据库
        var updatedEntity = await PRepository.InsertReturnEntityAsync(entity);
        // 注意：累充(AllRecharge)不在下单时递增，改为在发货确认支付成功后递增（见 TenantGameOrderShipService.ShipAsync），避免下单刷 VIP 等级
        // 创建输出对象
        var output = updatedEntity.Adapt<AddTenantGameOrderEntityResponse>();

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

        return HttpJsonResultData<AddTenantGameOrderEntityResponse>.Success(output);
    }

    /// <summary>
    /// 发货
    /// </summary>
    /// <param name="gameOrderEntity">订单信息</param>
    /// <param name="isRecharge">是否是充值发货</param>
    /// <param name="isSandBox">是否是沙盒充值</param>
    /// <returns></returns>
    private async Task<HttpJsonResultData<BaseOperationAckResponse>> SendOutGoods(TenantGameOrderEntity gameOrderEntity, bool isRecharge = false, bool isSandBox = false)
    {
        return await _shipService.ShipAsync(gameOrderEntity.Id, isRecharge, isSandBox);
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
}