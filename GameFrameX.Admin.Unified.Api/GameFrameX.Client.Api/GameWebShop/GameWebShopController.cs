using GameFrameX.Client.Api.Services.PublicCallback;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using GameFrameX.Application.ServerCenter;
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
    /// 网页商城回调端点路由段（公开回调 {packageId} 可选路由模板，不含分包段）。
    /// </summary>
    private const string WebShopNotifyCallbackRoute = "api/GameWebShop/NotifyWebShop";

    /// <summary>
    /// 回调发货按订单主键加进程内信号量串行化（同 GameOrderShipService 的订单锁约定），
    /// 避免同订单并发重复回调重复发送邮件。
    /// </summary>
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> NotifyOrderLocks = new();

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
    [ProducesResponseType(200, Type = typeof(GameWebShopResponse))]
    public async Task<HttpJsonResultData<List<GameWebShopResponse>>> Get()
    {
        var list = await PRepository.AsQueryable().Where(g => g.IsUp).Select<GameWebShopResponse>().OrderByDescending(g => g.Weight).ToListAsync();
        return HttpJsonResultData<List<GameWebShopResponse>>.Success(list);
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
            // 下单前把配置地址规范化为能命中回调路由的完整地址：站点根地址自动补路由段（含当前分包段）、
            // 旧三段地址自动去段重建、无法识别的路径直接拒绝下单，避免网关回调打在无法命中的地址上导致不发货。
            var notifyUrl = PublicCallbackNotifyUrlBuilder.Build(systemConfigParams.NotifyUrl, WebShopNotifyCallbackRoute, PaymentOptionSettingProvider.GetCurrentPackageId(HttpContext));
            if (notifyUrl == null)
            {
                return GameApiResult.Error<TenantGameWebShopEntityBuyResponse>(AreaErrorCode.AreaCannotBuy, "支付回调地址配置无效，无法构造完整支付回调路由");
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
    /// 支付网关回调（zlpay notify）。信任链固定为：① 强制验签（签名不匹配一律拒绝且不发货）→ ② 按 out_trade_no 幂等查单
    /// → ③ 金额比对 → ④ 按订单快照（order.Content）解出发货商品 → ⑤ Unpaid→NotShipped 认领 → ⑥ 区服校验
    /// → ⑦ 邮件发货 → ⑧ 发货成功后迁移 Shipped 并累充。业务参数（RoleId/AreaId/ShopId/实付金额）不取自回调，
    /// 一律回读 Buy 预创建的订单快照；param 回显不解析（不可信输入不参与解析）。
    /// 仅 Unpaid / NotShipped 可进入发货，其余状态（已发货、退款中、已退款、已作废等）幂等返回成功；
    /// 发货失败（区服离线 / 邮件失败等）订单保留 NotShipped 可重试状态，后续回调可再次尝试发货。
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("webshop", "payment")]
    [HttpGet("{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyWebShop([FromQuery] TenantGameWebShopEntityBuyNotifyRequest data)
    {
        if (data.TradeStatus != "TRADE_SUCCESS")
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var systemConfig = await _systemConfigService.GetAsync(g => g.Code == "web_shop_params");
        var systemConfigParams = systemConfig == null ? null : JsonHelper.Deserialize<GameWebConfig>(systemConfig.Value);
        if (systemConfigParams == null)
        {
            await InsertFailedWebShopOrderAsync(data.OutTradeNo, 0, 0, 0, "订单服务器参数错误");
            return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
        }

        // ① 验签（强制）：以回传业务参数重建字典重算 MD5（key 复用 web_shop_params.Sign）。
        // 签名不匹配一律拒绝回调且不发货（公开回调可被任意调用，伪造签名不得换货）；expected/received 落日志便于排障。
        var expectedSign = CreateMd5Sign(BuildNotifySignParameters(data), systemConfigParams.Sign);
        if (!FixedTimeEqualsIgnoreCase(expectedSign, data.Sign))
        {
            LogHelper.Warning($"网页商城回调验签不匹配：out_trade_no={data.OutTradeNo} expected={expectedSign} received={data.Sign}");
            await InsertFailedWebShopOrderAsync(data.OutTradeNo, 0, 0, 0, "验签失败");
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.SignatureVerifyFailed, "签名验证失败");
        }

        // ② 幂等查单：out_trade_no 是 Buy 预创建订单的全局幂等键（雪花 ID），查无此单视为伪造/异常回调
        var order = await _gameOrderEntity.GetAsync(g => g.OrderId == data.OutTradeNo);
        if (order == null)
        {
            await InsertFailedWebShopOrderAsync(data.OutTradeNo, 0, 0, 0, "预创建订单不存在");
            return GameApiResult.Error<BaseOperationAckResponse>(OrderErrorCode.NotifyOrderNotFound, "回调订单不存在");
        }

        // 同订单回调按订单主键加进程内信号量串行化（同 GameOrderShipService 的订单锁约定），
        // 避免并发重复回调重复发送邮件；锁内重读订单后按状态守卫推进。
        var orderLock = NotifyOrderLocks.GetOrAdd(order.Id, _ => new SemaphoreSlim(1, 1));
        await orderLock.WaitAsync();
        try
        {
            order = await _gameOrderEntity.GetAsync(g => g.OrderId == data.OutTradeNo);

            // 状态守卫：仅 Unpaid（待认领）与 NotShipped（认领后待发货 / 失败重试）可进入发货；
            // 其余状态（Shipped 已发货、InitiateRefund/RefundAgreement 退款中、Refunded 已退款、
            // Voided 已作废、Completed 已完成等）按既定口径幂等返回成功，不发邮件、不累充
            //（同 GameOrderShipService 对 Refunded/Voided 等终态订单的守卫处理）。
            if (order.Status is not (OrderStatusEnum.Unpaid or OrderStatusEnum.NotShipped))
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            // ③ 金额比对：回传金额（分）必须与预创建实付金额一致，少付/多付/非正数均拒绝发货。
            // 用 decimal 直接比较（uint 隐式提升），避免不可信金额经 decimal→uint 强转溢出抛异常。
            if (data.Money <= 0 || data.Money * 100 != order.Amount)
            {
                await InsertFailedWebShopOrderAsync(data.OutTradeNo, long.TryParse(order.RoleId, out var snapshotRoleId) ? snapshotRoleId : 0, order.AreaId, order.Sku, "金额不符");
                return GameApiResult.Error<BaseOperationAckResponse>(OrderErrorCode.NotifyAmountMismatch, "支付金额与订单不符");
            }

            // ④ 发货商品一律取 Buy 预创建的订单快照（order.Content.RewardsItem），不回读商城商品当前状态：
            // 付款期间商品被修改或下架都不改变本单应发内容，下架也不再阻断发货。
            var roleId = long.TryParse(order.RoleId, out var parsedRoleId) ? parsedRoleId : 0;
            var areaId = order.AreaId;
            var orderSnapshot = string.IsNullOrWhiteSpace(order.Content) ? null : JsonHelper.Deserialize<PlayerCreateOrderResponse>(order.Content);
            var snapshotItems = orderSnapshot?.RewardsItem ?? new List<PropertyItemResponse>();
            var newList = new List<PropertyItemResponse>();
            foreach (var propertyItem in snapshotItems)
            {
                var gameGoodsEntity = await PRepository.AsSugarClient().Queryable<TenantGameGoodsEntity>().SingleAsync(m => m.Id == propertyItem.Id);
                if (gameGoodsEntity != null)
                {
                    propertyItem.Id = gameGoodsEntity.ConfigId;
                    newList.Add(propertyItem);
                }
            }

            var entity = new TenantGameWebShopOrderEntity
            {
                RoleId = roleId,
                AreaId = areaId,
                OrderId = data.OutTradeNo,
                GameShopId = order.Sku,
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

            // ⑤ 首次收款确认：仅首个回调完成 Unpaid→NotShipped 认领（条件更新，并发重复回调在此幂等返回）。
            // 认领后进入发货链路；此前发货失败的订单停留在 NotShipped，后续回调跳过认领直接重试发货。
            if (order.Status == OrderStatusEnum.Unpaid)
            {
                var claimedRows = await _gameOrderEntity.AsUpdateable()
                                                        .SetColumns(u => u.Status == OrderStatusEnum.NotShipped)
                                                        .Where(u => u.OrderId == data.OutTradeNo && u.Status == OrderStatusEnum.Unpaid)
                                                        .ExecuteCommandAsync();
                if (claimedRows != 1)
                {
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }
            }

            // ⑥ 区服一致性 / 在线校验：失败保留 NotShipped 可重试状态并返回错误（等待网关重试）
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

            // ⑦ 发送商品到邮件：失败落失败单并返回错误，订单保留 NotShipped（网关重试时重新发货，无需人工补发）
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
                    return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
                }
            }
            catch (Exception e)
            {
                //添加一个失败订单到网页商城
                entity.Reason = "游戏服务参数解析错误" + e.Message;
                await _webShopOrderService.InsertAsync(entity);
                return GameApiResult.Error<BaseOperationAckResponse>(AreaErrorCode.AreaCannotBuy, "当前状态不能购买,联系管理员进行补发");
            }

            // ⑧ 发货成功后完成状态迁移：NotShipped→Shipped 条件更新恰一迁移，累充随迁移恰一累加
            //（并发 / 重试下只有完成迁移的一方累充，已 Shipped 的订单在锁内幂等短路，不重复累充）。
            var shippedRows = await _gameOrderEntity.AsUpdateable()
                                                    .SetColumns(u => u.Status == OrderStatusEnum.Shipped)
                                                    .Where(u => u.OrderId == data.OutTradeNo && u.Status == OrderStatusEnum.NotShipped)
                                                    .ExecuteCommandAsync();
            if (shippedRows == 1)
            {
                // 数据库侧原子累加，避免并发下单时读改写丢失。
                var rechargeAmount = (int)order.Amount;
                await _roleService.AsUpdateable()
                                  .SetColumns(u => u.AllRecharge == (u.AllRecharge ?? 0) + rechargeAmount)
                                  .Where(u => u.Id == role.Id)
                                  .ExecuteCommandAsync();
            }

            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }
        finally
        {
            orderLock.Release();
        }
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
    /// 常量时间比较两个十六进制签名（忽略大小写）：统一小写转字节后用
    /// <see cref="CryptographicOperations.FixedTimeEquals"/> 比较，避免短路字符串比较
    /// 向定时攻击泄露匹配前缀长度；长度不等直接返回 false（仅泄露长度差异）。
    /// </summary>
    /// <param name="expectedSign">本地重算的期望签名（小写十六进制）。</param>
    /// <param name="receivedSign">回调回传签名，可为 null。</param>
    /// <returns>是否一致。</returns>
    private static bool FixedTimeEqualsIgnoreCase(string expectedSign, string? receivedSign)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSign.ToLowerInvariant());
        var receivedBytes = Encoding.UTF8.GetBytes((receivedSign ?? string.Empty).ToLowerInvariant());
        return expectedBytes.Length == receivedBytes.Length
               && CryptographicOperations.FixedTimeEquals(expectedBytes, receivedBytes);
    }

    /// <summary>
    /// Buy 校验全部通过后预创建 Unpaid 订单：快照 RoleId/AreaId/Sku/实付金额（分）与发货道具列表（PlayerCreateOrderResponse 形状）。
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
    /// 落网页商城失败单（保留排障语义）：验签失败/查无订单时无预创建快照可回读，RoleId/AreaId/GameShopId 以 0 填充。
    /// </summary>
    /// <param name="outTradeNo">回传订单号。</param>
    /// <param name="roleId">失败单角色 id（无快照时 0）。</param>
    /// <param name="areaId">失败单区服 id（无快照时 0）。</param>
    /// <param name="gameShopId">失败单商品 id（无快照时 0）。</param>
    /// <param name="reason">失败原因。</param>
    private async Task InsertFailedWebShopOrderAsync(string outTradeNo, long roleId, long areaId, long gameShopId, string reason)
    {
        await _webShopOrderService.InsertAsync(new TenantGameWebShopOrderEntity
        {
            RoleId = roleId,
            AreaId = areaId,
            OrderId = outTradeNo,
            GameShopId = gameShopId,
            Name = "",
            Reason = reason,
            Description = "",
        });
    }
}
