using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Normalization;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Order.GameOrderFulfillment;

/// <summary>
/// 权益发货账本 Admin 查询/管理服务（C11-A / C120，承接 C113 账本事实源）。
/// <para>只读：PageAsync（继承基类）/ GetAsync 详情（继承基类）/ ExportAsync 导出当前过滤列表（上限 1000 行）。</para>
/// <para>高风险动作：RefundAsync / CompensateAsync / ResendAsync，全部委托 TenantGameOrderShipService（C113 已落地的账本写入 + 操作人审计 + 幂等链路）。</para>
/// <para>事实数据铁律：OnAddingAsync / OnUpdatingAsync 抛错关闭默认 AddAsync/UpdateAsync 端点，无 DeleteAsync 端点。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameOrderFulfillmentEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGameOrderFulfillmentEntity,
        FulfillmentActionRequest,
        UpdateTenantGameOrderFulfillmentEntityRequest,
        TenantGameOrderFulfillmentEntityResponse,
        PageTenantGameOrderFulfillmentEntityRequest>
{
    private readonly TenantGameOrderShipService _shipService;
    private readonly TenantGameOrderChannelRefundService _channelRefundService;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameOrderEntity> _orderRepository;
    private readonly CurrentUserContext _currentUserContext;

    public TenantGameOrderFulfillmentEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameOrderFulfillmentEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameOrderEntity> orderRepository,
        TenantGameOrderShipService shipService,
        TenantGameOrderChannelRefundService channelRefundService,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _orderRepository = orderRepository;
        _shipService = shipService;
        _channelRefundService = channelRefundService;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 账本由发货链路产生（C113），禁止管理后台直接创建。
    /// </summary>
    protected override Task OnAddingAsync(FulfillmentActionRequest input)
    {
        throw new Exception("权益发货账本由发货链路产生，不支持通过管理后台直接创建");
    }

    /// <summary>
    /// 账本为事实数据，禁止管理后台修改。
    /// </summary>
    protected override Task OnUpdatingAsync(UpdateTenantGameOrderFulfillmentEntityRequest input)
    {
        throw new Exception("权益发货账本为事实数据，不支持修改");
    }

    /// <summary>
    /// 账本分页查询过滤（字段穷举见 change C120 §3）。
    /// </summary>
    protected override ISugarQueryable<TenantGameOrderFulfillmentEntityResponse> BuildPageQuery(PageTenantGameOrderFulfillmentEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "权益发货账本");
        return Repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(input.OrderId.HasValue, u => u.OrderId == input.OrderId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.RoleId), u => u.RoleId == input.RoleId)
                         .WhereIF(input.AreaId.HasValue, u => u.AreaId == input.AreaId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.PayChannel), u => u.PayChannel.Contains(input.PayChannel.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.ProviderTransactionId), u => u.ProviderTransactionId == input.ProviderTransactionId)
                         .WhereIF(input.Type.HasValue, u => u.Type == input.Type.Value)
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .WhereIF(input.Sku.HasValue, u => u.Sku == input.Sku.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.AccountId), u => u.AccountId == input.AccountId)
                         .WhereIF(input.OperatorId.HasValue, u => u.OperatorId == input.OperatorId.Value)
                         .WhereIF(input.CreateTimeRange != null && input.CreateTimeRange.Count == 2 && input.CreateTimeRange[0] < input.CreateTimeRange[1],
                                  u => u.CreatedTime >= input.CreateTimeRange[0]
                                       && u.CreatedTime <= input.CreateTimeRange[1])
                         .Select<TenantGameOrderFulfillmentEntityResponse>();
    }

    /// <summary>
    /// 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Export")]
    public async Task<List<TenantGameOrderFulfillmentEntityResponse>> ExportAsync(PageTenantGameOrderFulfillmentEntityRequest input)
    {
        return await BuildPageQuery(input).Take(1000).ToListAsync();
    }

    /// <summary>
    /// 退款扣除（高风险）。C310 / GFX-718 起两阶段：已支付订单先渠道退款（按 PayChannel 路由 IRefundManager——
    /// 渠道失败 / 不支持主动退款 / 配置缺失抛统一错误码 PaymentErrorCode 段 704，本地账本不动），
    /// 渠道成功后委托 TenantGameOrderShipService.RefundAsync（C113）：按订单状态走作废 / 扣除路径，账本落 RefundAsync 类型 + 状态 + FailureReason。
    /// Unpaid 订单未收款，跳过渠道直接本地作废。
    /// <para>C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Refund")]
    public async Task<FulfillmentActionResponse> RefundAsync(FulfillmentActionRequest input)
    {
        // C310：Unpaid 未收款无需渠道退款，直接本地作废；其余状态先渠道退款成功再扣本地账本
        var order = await GetOrderForChannelRefundAsync(input.Id);
        if (order.Status != OrderStatusEnum.Unpaid)
        {
            await _channelRefundService.RefundByOrderAsync(order, input.Reason);
        }

        var result = await _shipService.RefundAsync(order.Id, input.Reason);
        EnsureActionSucceeded(result);
        return new FulfillmentActionResponse { OrderId = order.Id, Type = FulfillmentTypeEnum.Refund, };
    }

    /// <summary>
    /// 人工补偿/补发（高风险）。委托 TenantGameOrderShipService.CompensateAsync（C113）：按 Purchase 链路重发，账本落 CompensateAsync 类型 + OperatorId 审计。
    /// <para>C223：Admin result 统一对象契约——失败抛 BusinessException 走 AdminResultFilter 统一错误路径，不再透传 HttpJsonResultData 双重信封。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Compensate")]
    public async Task<FulfillmentActionResponse> CompensateAsync(FulfillmentActionRequest input)
    {
        var reason = string.IsNullOrWhiteSpace(input.Reason) ? "人工补偿" : input.Reason;
        var result = await _shipService.CompensateAsync(input.Id, reason, _currentUserContext.UserId);
        EnsureActionSucceeded(result);
        return new FulfillmentActionResponse { OrderId = input.Id, Type = FulfillmentTypeEnum.Compensate, };
    }

    /// <summary>
    /// 重新发货（高风险）。收敛到 CompensateAsync（不新增发放类型枚举，C113 已锁定 Purchase/RefundAsync/CompensateAsync 三类）。
    /// <para>C223：Admin result 统一对象契约，同 CompensateAsync。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Resend")]
    public async Task<FulfillmentActionResponse> ResendAsync(FulfillmentActionRequest input)
    {
        var reason = string.IsNullOrWhiteSpace(input.Reason) ? "重新发货" : input.Reason;
        var result = await _shipService.CompensateAsync(input.Id, reason, _currentUserContext.UserId);
        EnsureActionSucceeded(result);
        return new FulfillmentActionResponse { OrderId = input.Id, Type = FulfillmentTypeEnum.Compensate, };
    }

    /// <summary>
    /// 解析退款动作对应的订单（C310）：input.Id 兼容订单 Id（TenantGameOrderShipService 既有口径）与账本行 Id（账本页动作语义）两种传参，
    /// 后者按账本行 OrderId 映射回订单实体。
    /// </summary>
    private async Task<Entity.Tenant.Game.TenantGameOrderEntity> GetOrderForChannelRefundAsync(long inputId)
    {
        var order = await _orderRepository.GetFirstAsync(u => u.Id == inputId);
        if (order != null)
        {
            return order;
        }

        var fulfillment = await Repository.GetFirstAsync(u => u.Id == inputId);
        if (fulfillment == null || fulfillment.OrderId <= 0)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        order = await _orderRepository.GetFirstAsync(u => u.Id == fulfillment.OrderId);
        return order ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
    }

    /// <summary>
    /// 转译 TenantGameOrderShipService 的信封结果：失败抛 BusinessException（AdminResultFilter 统一包装非 0 响应码），
    /// 成功无返回。C227 起 ShipService 泛型为 BaseOperationAckResponse（data 恒空，仅承载成败与消息），转译仍收敛在本服务边界内。
    /// </summary>
    private static void EnsureActionSucceeded(HttpJsonResultData<BaseOperationAckResponse> result)
    {
        if (!result.IsSuccess)
        {
            // ship service 的 Fail 消息已自带动作前缀（如「退款扣除失败: …」），原样透传给 Admin 统一错误路径
            throw BusinessError.Oh(result.Message ?? "操作失败");
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629