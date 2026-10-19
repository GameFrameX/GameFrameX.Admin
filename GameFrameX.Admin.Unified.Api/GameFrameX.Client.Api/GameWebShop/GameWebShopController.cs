using GameFrameX.Client.Api.Services.PublicCallback;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.GameUserEmail.Dto;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Extensions;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;

namespace GameFrameX.Client.Api.GameWebShop;

/// <summary>
/// 网页商城
/// </summary>
public class GameWebShopController : BaseController<TenantGameWebShopEntity>
{
    private readonly HttpClient _httpClientFactory;

    private readonly Repository<TenantGameRoleEntity> _roleService;
    private readonly Repository<TenantGameAreaEntity> _areaService;
    private readonly Repository<TenantGameWebShopOrderEntity> _webShopOrderService;
    private readonly Repository<TenantGameOrderEntity> _gameOrderEntity;
    private readonly Repository<SystemConfigEntity> _systemConfigService;

    /// <summary>
    /// </summary>
    /// <param name="service">网页商城服务</param>
    /// <param name="httpClientFactory"></param>
    /// <param name="roleService">角色服务</param>
    /// <param name="areaService">区服服务</param>
    /// <param name="webShopOrderService">网页商城订单</param>
    /// <param name="gameOrderEntity"></param>
    /// <param name="systemConfigService"></param>
    public GameWebShopController(
        HttpClient httpClientFactory,
        SqlSugarRepository<TenantGameWebShopEntity> service,
        Repository<TenantGameRoleEntity> roleService,
        Repository<TenantGameAreaEntity> areaService,
        Repository<TenantGameWebShopOrderEntity> webShopOrderService,
        Repository<TenantGameOrderEntity> gameOrderEntity,
        Repository<SystemConfigEntity> systemConfigService) : base(service)
    {
        _httpClientFactory = httpClientFactory;
        _roleService = roleService;
        _areaService = areaService;
        _webShopOrderService = webShopOrderService;
        _gameOrderEntity = gameOrderEntity;
        _systemConfigService = systemConfigService;
    }

    /// <summary>
    /// 获取网页商城商品
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(TenantGameWebShopEntityResponse))]
    public async Task<HttpJsonResultData<List<TenantGameWebShopEntityResponse>>> Get()
    {
        var list = await PRepository.AsQueryable().Where(g => g.IsUp).Select<TenantGameWebShopEntityResponse>().OrderByDescending(g => g.Weight).ToListAsync();
        return HttpJsonResultData<List<TenantGameWebShopEntityResponse>>.Success(list);
    }


    /// <summary>
    /// 下单一个商品
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(TenantGameWebShopEntityBuyResponse))]
    public async Task<HttpJsonResultData<TenantGameWebShopEntityBuyResponse>> Buy([FromBody] TenantGameWebShopEntityBuyRequest input)
    {
        var role = await _roleService.GetByIdAsync(input.RoleId);
        //角色不存在
        if (role == null)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        //区服不一致
        if (role.AreaId != input.AreaId)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(AreaErrorCode.AreaNotFound, "区服不一致");
        }

        var gameAreaEntity = await _areaService.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(u => u.Id == input.AreaId);

        if (gameAreaEntity.IsNull() || gameAreaEntity.ServerStatus == GameServerStatus.Unknown
                                    || gameAreaEntity.ServerStatus == GameServerStatus.Offline
                                    || gameAreaEntity.ServerStatus == GameServerStatus.Maintenance)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买");
        }

        //查询商品
        var shop = await PRepository.GetAsync(g => g.Id == input.Id && g.IsUp);
        if (shop == null)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, "商品不存在");
        }

        //判断支付方式
        if (input.Type == "")
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, "支付方式不正确");
        }

        var param = new Dictionary<string, string>();
        param["RoleId"] = input.RoleId.ToString();
        param["AreaId"] = input.AreaId.ToString();
        param["ShopId"] = input.Id.ToString();

        var postParams = new Dictionary<string, string>();
        var systemConfig = await _systemConfigService.GetAsync(g => g.Code == "web_shop_params");
        if (systemConfig == null)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(AreaErrorCode.AreaCannotBuy, "订单服务器参数错误");
        }

        try
        {
            var systemConfigParams = JsonHelper.Deserialize<GameWebConfig>(systemConfig.Value);
            if (!decimal.TryParse(shop.RealPrice, out var realPrice))
            {
                return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, "商品价格不正确");
            }

            // out_trade_no 用 Foundation 雪花 ID 作全局幂等键（替代时间戳+随机数，回调按其查重防重放）
            var outTradeNo = SnowFlakeIdHelper.Instance.NextId().ToString();
            postParams["pid"] = systemConfigParams.Pid;
            postParams["type"] = input.Type;
            postParams["out_trade_no"] = outTradeNo;
            var notifyUrl = BuildWebShopNotifyUrl(systemConfigParams.NotifyUrl, role.IsSandbox ?? false);
            if (string.IsNullOrWhiteSpace(notifyUrl))
            {
                return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(AreaErrorCode.AreaCannotBuy, "回调地址缺少租户或应用上下文");
            }

            postParams["notify_url"] = notifyUrl;
            postParams["return_url"] = systemConfigParams.ReturnUrl;
            postParams["param"] = JsonHelper.Serialize(param);
            postParams["name"] = shop.Name;
            postParams["money"] = shop.RealPrice;
            postParams["sign_type"] = "MD5";
            postParams["sign"] = CreateMd5Sign(postParams, systemConfigParams.Sign);

            // 预创建 Unpaid 订单落参数快照，回调只信此单；预创建失败则不向网关提交
            if (!await InsertPreCreatedOrderAsync(input, role, shop, outTradeNo, realPrice))
            {
                return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, "预创建订单失败");
            }

            // 发送POST请求
            var formContent = new FormUrlEncodedContent(postParams);
            var response = await _httpClientFactory.PostAsync("https://zlpay.zilancreative.com/submit.php", formContent);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var regex = new Regex(@"window\.location\.replace\(['""](.+?)['""]\)");
                var match = regex.Match(responseContent);
                if (match.Success)
                {
                    var output = new TenantGameWebShopEntityBuyResponse();
                    output.Html = "https://zlpay.zilancreative.com" + match.Groups[1].Value;
                    output.OrderId = postParams["out_trade_no"];
                    return HttpJsonResultData<TenantGameWebShopEntityBuyResponse>.Success(output);
                }

                return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, responseContent);
            }

            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, JsonHelper.Serialize(response));
        }
        catch (Exception e)
        {
            return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(OrderErrorCode.WebShopNotFound, JsonHelper.Serialize(e));
        }
    }

    /// <summary>
    /// 支付网关回调（zlpay notify）。信任链固定为：① 验签（VerifySignMode log/enforce 灰度）→ ② 按 out_trade_no 幂等查单
    /// → ③ 金额比对 → ④ Unpaid→Shipped 迁移发货。业务参数（RoleId/AreaId/ShopId/实付金额）不取自回调，
    /// 一律回读 Buy 预创建的订单快照；param 回显不解析（不可信输入不参与解析）。
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("webshop", "payment")]
    [HttpGet("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyWebShop([FromQuery] TenantGameWebShopEntityBuyNotifyRequest data)
    {
        // 公开回调端点：appId 由 PublicCallbackTenantResolutionMiddleware 从路由 {appId} 解析并写入 AppIdKey
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "网页商城订单");

        if (data.TradeStatus != "TRADE_SUCCESS")
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var systemConfig = await _systemConfigService.GetAsync(g => g.Code == "web_shop_params");
        var systemConfigParams = systemConfig == null ? null : JsonHelper.Deserialize<GameWebConfig>(systemConfig.Value);
        if (systemConfigParams == null)
        {
            await InsertFailedWebShopOrderAsync(appId, data.OutTradeNo, 0, 0, 0, "订单服务器参数错误");
            return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
        }

        // ① 验签：以回传业务参数重建字典重算 MD5（key 复用 web_shop_params.Sign）。
        // log 模式仅记录 expected/received 不拒绝（灰度观测 zlpay notify 签名口径）；enforce 模式验签失败直接拒发。
        var expectedSign = CreateMd5Sign(BuildNotifySignParameters(data), systemConfigParams.Sign);
        if (!string.Equals(expectedSign, data.Sign, StringComparison.OrdinalIgnoreCase))
        {
            LogHelper.Warning($"网页商城回调验签不匹配：out_trade_no={data.OutTradeNo} expected={expectedSign} received={data.Sign}");
            if (IsEnforceVerifySignMode(systemConfigParams))
            {
                await InsertFailedWebShopOrderAsync(appId, data.OutTradeNo, 0, 0, 0, "验签失败");
                return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.SignatureVerifyFailed, "签名验证失败");
            }
        }

        // ② 幂等查单：out_trade_no 是 Buy 预创建订单的全局幂等键（雪花 ID），查无此单视为伪造/异常回调
        var order = await _gameOrderEntity.GetAsync(g => g.OrderId == data.OutTradeNo);
        if (order == null)
        {
            await InsertFailedWebShopOrderAsync(appId, data.OutTradeNo, 0, 0, 0, "预创建订单不存在");
            return GameApiResult.Error<BaseOperationAckResponse>(OrderErrorCode.NotifyOrderNotFound, "回调订单不存在");
        }

        if (order.Status == OrderStatusEnum.Shipped)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        // ③ 金额比对：回传金额（分）必须与预创建实付金额一致，少付/多付/非正数均拒绝发货。
        // 用 decimal 直接比较（uint 隐式提升），避免不可信金额经 decimal→uint 强转溢出抛异常。
        if (data.Money <= 0 || data.Money * 100 != order.Amount)
        {
            await InsertFailedWebShopOrderAsync(appId, data.OutTradeNo, long.TryParse(order.RoleId, out var snapshotRoleId) ? snapshotRoleId : 0, order.AreaId, order.Sku, "金额不符");
            return GameApiResult.Error<BaseOperationAckResponse>(OrderErrorCode.NotifyAmountMismatch, "支付金额与订单不符");
        }

        // ④ Unpaid→Shipped 状态迁移并执行现有发货链路；同单重复回调在上方幂等检查直接返回，仅发货一次
        var roleId = long.TryParse(order.RoleId, out var parsedRoleId) ? parsedRoleId : 0;
        var areaId = order.AreaId;
        var shopId = order.Sku;
        var entity = new TenantGameWebShopOrderEntity
        {
            AppId = appId, // 事实数据应用作用域，来自公开回调路由 {appId}
            RoleId = roleId,
            AreaId = areaId,
            OrderId = data.OutTradeNo,
            GameShopId = shopId,
            Name = "",
            Reason = "",
            Description = "",
        };
        var role = await _roleService.GetByIdAsync(roleId);
        //角色不存在
        if (role == null)
        {
            //添加一个失败订单到网页商城
            entity.Reason = "角色不存在";
            await _webShopOrderService.InsertAsync(entity);
            return GameApiResult.Error<BaseOperationAckResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        var shop = await PRepository.GetAsync(g => g.Id == shopId && g.IsUp);

        if (shop == null)
        {
            //添加一个失败订单到网页商城

            entity.Reason = "商品不存在或下架";
            await _webShopOrderService.InsertAsync(entity);
            return GameApiResult.Error<BaseOperationAckResponse>(OrderErrorCode.WebShopNotFound, "商品不存在");
        }

        var list = JsonHelper.Deserialize<List<PropertyItemResponse>>(shop.ItemJson);
        var newList = new List<PropertyItemResponse>();
        foreach (var propertyItem in list)
        {
            var gameGoodsEntity = await PRepository.AsSugarClient().Queryable<TenantGameGoodsEntity>().SingleAsync(m => m.Id == propertyItem.Id);
            if (gameGoodsEntity != null)
            {
                propertyItem.Id = gameGoodsEntity.ConfigId;
                newList.Add(propertyItem);
            }
        }

        // 收款确认后原子迁移 Unpaid→Shipped：条件更新（仅 Unpaid 可迁），并发重复回调在此串行化，
        // 抢不到迁移权（影响行数!=1）说明另一请求已处理，按幂等直接返回 success；发货链路继续使用上方回读的订单快照。
        var shippedRows = await _gameOrderEntity.AsUpdateable()
                                                .SetColumns(u => u.Status == OrderStatusEnum.Shipped)
                                                .Where(u => u.OrderId == data.OutTradeNo && u.Status == OrderStatusEnum.Unpaid)
                                                .ExecuteCommandAsync();
        if (shippedRows != 1)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        // 数据库侧原子累加，避免并发下单时读改写丢失。
        var rechargeAmount = (int)order.Amount;
        await _roleService.AsUpdateable()
                          .SetColumns(u => u.AllRecharge == (u.AllRecharge ?? 0) + rechargeAmount)
                          .Where(u => u.Id == role.Id)
                          .ExecuteCommandAsync();
        //区服不一致
        if (role.AreaId != areaId)
        {
            //添加一个失败订单到网页商城

            entity.Reason = "区服不一致";
            await _webShopOrderService.InsertAsync(entity);
            return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaNotFound, "区服不一致");
        }

        //获取区服
        var gameAreaEntity = await _areaService.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(u => u.Id == areaId);
        if (gameAreaEntity.IsNull()
            || gameAreaEntity.ServerStatus == GameServerStatus.Unknown
            || gameAreaEntity.ServerStatus == GameServerStatus.Offline
            || gameAreaEntity.ServerStatus == GameServerStatus.Maintenance)
        {
            //添加一个失败订单到网页商城

            entity.Reason = "服务器状态不在线";
            await _webShopOrderService.InsertAsync(entity);
            return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
        }
        //发送商品到邮件

        try
        {
            var reqPostMailRequest = new GameWebShpBuySendMailRequest
            {
                Roles = new List<long> { roleId, },
                Title = systemConfigParams.WebShopTitle,
                SubTitle = systemConfigParams.WebShopSubTitle,
                Content = systemConfigParams.WebShopContent,
                PostType = false,
                ExpirationTime = TimerHelper.UnixTimeSeconds() + 30 * 24 * 60 * 60,
                IsTranslate = true,
                Items = newList,
            };
            var result = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "post_mail"), reqPostMailRequest);
            var resultData = result.ToHttpJsonResult<GameWebShpBuySendMailResponse>();
            if (!resultData.IsSuccess)
            {
                entity.Reason = JsonHelper.SerializeFormat(resultData);
                await _webShopOrderService.InsertAsync(entity);
            }
        }
        catch (Exception e)
        {
            //添加一个失败订单到网页商城
            entity.Reason = "游戏服务参数解析错误" + e.Message;
            await _webShopOrderService.InsertAsync(entity);
            return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 生成签名
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string CreateMd5Sign(Dictionary<string, string> parameters, string key)
    {
        var filtered = parameters
                       .Where(p => p.Value != null
                                   && p.Value.ToString() != ""
                                   && p.Key != "sign"
                                   && p.Key != "sign_type")
                       .OrderBy(p => p.Key, StringComparer.Ordinal) // ASCII 升序
                       .Select(p => $"{p.Key}={p.Value}");

        var queryString = string.Join("&", filtered);
        var raw = queryString + key;

        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// Buy 校验全部通过后预创建 Unpaid 订单：快照 RoleId/AreaId/Sku/AppId/实付金额（分）与发货道具列表（PlayerCreateOrderResponse 形状）。
    /// 回调按 out_trade_no 回读此单完成状态迁移，业务参数不取自回调；预创建失败则 Buy 报错且不向网关提交。
    /// </summary>
    /// <param name="input">下单参数（RoleId/AreaId/商品 Id）。</param>
    /// <param name="role">已校验的角色（AccountId/IsSandbox 来源）。</param>
    /// <param name="shop">已校验的商城商品（道具列表来源）。</param>
    /// <param name="outTradeNo">雪花 ID 幂等键。</param>
    /// <param name="realPrice">实付金额（元，已 TryParse 校验）。</param>
    /// <returns>预创建是否成功。</returns>
    private async Task<bool> InsertPreCreatedOrderAsync(TenantGameWebShopEntityBuyRequest input, TenantGameRoleEntity role, TenantGameWebShopEntity shop, string outTradeNo, decimal realPrice)
    {
        // notify_url 构造已确保租户/应用上下文存在，此处缺失属于不可达防御路径，按预创建失败处理
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return false;
        }

        var snapshot = new PlayerCreateOrderResponse
        {
            RoleId = input.RoleId,
            Sku = input.Id,
            Amount = (uint)(realPrice * 100),
            IsNeedPay = true,
            RewardsItem = JsonHelper.Deserialize<List<PropertyItemResponse>>(shop.ItemJson) ?? new List<PropertyItemResponse>(),
        };
        var order = new TenantGameOrderEntity
        {
            AppId = appId,
            Sku = input.Id,
            AreaId = input.AreaId,
            OrderId = outTradeNo,
            RoleId = input.RoleId.ToString(),
            Status = OrderStatusEnum.Unpaid,
            Amount = (uint)(realPrice * 100),
            Content = JsonHelper.Serialize(snapshot),
            AccountId = role.UserId.ToString(),
            Channel = "聚支付平台",
            PayChannel = "网页支付",
            Platform = "web",
            IsSandbox = role.IsSandbox ?? false,
            AdventureSuffix = string.Empty,
        };
        return await _gameOrderEntity.InsertAsync(order);
    }

    /// <summary>
    /// 重建 zlpay 回传业务参数字典用于验签：排除 sign/sign_type 与空值（由 CreateMd5Sign 统一过滤），
    /// money 用回传 decimal 的 invariant 无尾零格式转字符串。
    /// </summary>
    /// <param name="data">网关回传参数。</param>
    /// <returns>参与 MD5 签名的业务参数字典。</returns>
    private static Dictionary<string, string> BuildNotifySignParameters(TenantGameWebShopEntityBuyNotifyRequest data)
    {
        return new Dictionary<string, string>
        {
            { "pid", data.Pid.ToString(CultureInfo.InvariantCulture) },
            { "trade_no", data.TradeNo },
            { "out_trade_no", data.OutTradeNo },
            { "type", data.Type },
            { "name", data.Name },
            { "money", data.Money.ToString(CultureInfo.InvariantCulture) },
            { "trade_status", data.TradeStatus },
            { "param", data.Param },
        };
    }

    /// <summary>
    /// 验签灰度模式判定：仅 enforce 白名单值返回 true，其余（缺省 log / 非白名单取值）一律按 log 灰度处理，
    /// 避免口径未确认时误拒真实收款。
    /// </summary>
    /// <param name="config">网页商城配置。</param>
    /// <returns>是否为 enforce 强制验签模式。</returns>
    private static bool IsEnforceVerifySignMode(GameWebConfig config)
    {
        return string.Equals(config.VerifySignMode, "enforce", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 落网页商城失败单（保留排障语义）：验签失败/查无订单时无预创建快照可回读，RoleId/AreaId/GameShopId 以 0 填充。
    /// </summary>
    /// <param name="appId">公开回调路由解析出的事实 appId。</param>
    /// <param name="outTradeNo">回传订单号。</param>
    /// <param name="roleId">失败单角色 id（无快照时 0）。</param>
    /// <param name="areaId">失败单区服 id（无快照时 0）。</param>
    /// <param name="gameShopId">失败单商品 id（无快照时 0）。</param>
    /// <param name="reason">失败原因。</param>
    private async Task InsertFailedWebShopOrderAsync(long appId, string outTradeNo, long roleId, long areaId, long gameShopId, string reason)
    {
        await _webShopOrderService.InsertAsync(new TenantGameWebShopOrderEntity
        {
            AppId = appId,
            RoleId = roleId,
            AreaId = areaId,
            OrderId = outTradeNo,
            GameShopId = gameShopId,
            Name = "",
            Reason = reason,
            Description = "",
        });
    }

    private string? BuildWebShopNotifyUrl(string notifyUrl, bool isSandbox)
    {
        if (!TryGetContextLong(TenantContextAccessor.TenantIdKey, out var tenantId) ||
            !TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return null;
        }

        var routeContext = new PublicCallbackRouteContext(
            tenantId,
            appId,
            "webshop",
            "payment",
            TryGetContextLong(PaymentOptionSettingProvider.PackageIdKey, out var packageId) ? packageId : null);
        return PublicCallbackRouteResolver.AppendRouteParameters(notifyUrl, routeContext);
    }

    private bool TryGetContextLong(string key, out long value)
    {
        value = 0;
        return HttpContext.Items.TryGetValue(key, out var item) &&
               item != null &&
               long.TryParse(item.ToString(), out value) &&
               value > 0;
    }
}