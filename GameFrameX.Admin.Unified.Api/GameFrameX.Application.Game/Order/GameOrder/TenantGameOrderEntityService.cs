using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Order.GameOrder.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Order.GameOrder;

/// <summary>
/// 游戏支付订单服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameOrderEntityService : GenericCrudService<Entity.Tenant.Game.TenantGameOrderEntity, AddTenantGameOrderEntityRequest, UpdateTenantGameOrderEntityRequest, TenantGameOrderEntityResponse, PageTenantGameOrderEntityRequest>
{
    private readonly TenantGameOrderShipService _shipService;
    private readonly CurrentUserContext _currentUserContext;

    public TenantGameOrderEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameOrderEntity> repository, TenantGameOrderShipService shipService, CurrentUserContext currentUserContext) : base(repository)
    {
        _shipService = shipService;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 新增前：游戏订单由客户端下单 / 支付回调链路产生，事实数据 AppId 必须来自可信写入链路；
    /// 禁止管理后台直接创建订单，避免写入 AppId=0 违反事实数据应用作用域铁律。
    /// </summary>
    protected override Task OnAddingAsync(AddTenantGameOrderEntityRequest input)
    {
        throw new Exception("游戏订单由客户端下单链路产生，不支持通过管理后台直接创建");
    }

    protected override ISugarQueryable<TenantGameOrderEntityResponse> BuildPageQuery(PageTenantGameOrderEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏订单");
        return Repository.AsQueryable()
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.RoleId), u => u.RoleId == input.RoleId)
                         .WhereIF(input.Amount > 0, u => u.Amount == input.Amount)
                         .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.OrderId), u => u.OrderId == input.OrderId)
                         .WhereIF(input.Sku > 0, u => u.Sku == input.Sku)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.PayChannel), u => u.PayChannel.Contains(input.PayChannel.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Channel), u => u.Channel.Contains(input.Channel.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Platform), u => u.Platform.Contains(input.Platform.Trim()))
                         .WhereIF(input.IsSandbox.HasValue, u => u.IsSandbox == input.IsSandbox)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.AccountId), u => u.AccountId == input.AccountId)
                         .WhereIF(input.CreateTimeRange != null && input.CreateTimeRange.Count == 2 && input.CreateTimeRange[0] < input.CreateTimeRange[1],
                                  u => u.CreatedTime >= input.CreateTimeRange[0]
                                       && u.CreatedTime <= input.CreateTimeRange[1])
                         .Select<TenantGameOrderEntityResponse>();
    }

    /// <summary>
    /// 订单统计汇总
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Summary")]
    public async Task<GameOrderSummaryResponse> SummaryAsync(PageTenantGameOrderEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏订单");
        var baseQuery = Repository.AsQueryable()
                                  .WhereIF(input.Status.HasValue, u => u.Status == input.Status)
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.RoleId), u => u.RoleId == input.RoleId)
                                  .WhereIF(input.Amount > 0, u => u.Amount == input.Amount)
                                  .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                                  .Where(u => u.AppId == appId)
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.OrderId), u => u.OrderId == input.OrderId)
                                  .WhereIF(input.Sku > 0, u => u.Sku == input.Sku)
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.PayChannel), u => u.PayChannel.Contains(input.PayChannel.Trim()))
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.Channel), u => u.Channel.Contains(input.Channel.Trim()))
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.Platform), u => u.Platform.Contains(input.Platform.Trim()))
                                  .WhereIF(input.IsSandbox.HasValue, u => u.IsSandbox == input.IsSandbox)
                                  .WhereIF(!string.IsNullOrWhiteSpace(input.AccountId), u => u.AccountId == input.AccountId)
                                  .WhereIF(input.CreateTimeRange != null && input.CreateTimeRange.Count == 2 && input.CreateTimeRange[0] < input.CreateTimeRange[1],
                                           u => u.CreatedTime >= input.CreateTimeRange[0]
                                                && u.CreatedTime <= input.CreateTimeRange[1]);

        var paidStatuses = new[] { OrderStatusEnum.NotShipped, OrderStatusEnum.Shipped, OrderStatusEnum.InitiateRefund, OrderStatusEnum.RefundRefused, OrderStatusEnum.Completed, };

        var totalStats = await baseQuery
                               .Select(u => new AggregateRow { Amount = SqlFunc.AggregateSum(u.Amount), Count = SqlFunc.AggregateCount(u.Id), })
                               .FirstAsync();

        var paidQuery = baseQuery.Where(u => paidStatuses.Contains(u.Status));
        var paidStats = await paidQuery
                              .Select(u => new AggregateRow { Amount = SqlFunc.AggregateSum(u.Amount), Count = SqlFunc.AggregateCount(u.Id), })
                              .FirstAsync();

        var channelBreakdown = await baseQuery.GroupBy(u => u.PayChannel)
                                              .Select(u => new GameOrderChannelBreakdownResponse
                                              {
                                                  PayChannel = u.PayChannel,
                                                  Amount = SqlFunc.AggregateSum(u.Amount),
                                                  Count = SqlFunc.AggregateCount(u.Id),
                                              })
                                              .ToListAsync();

        var statusBreakdown = await baseQuery.GroupBy(u => u.Status)
                                             .Select(u => new GameOrderStatusBreakdownResponse
                                             {
                                                 Status = u.Status,
                                                 Amount = SqlFunc.AggregateSum(u.Amount),
                                                 Count = SqlFunc.AggregateCount(u.Id),
                                             })
                                             .ToListAsync();

        return new GameOrderSummaryResponse
        {
            TotalAmount = totalStats?.Amount ?? 0,
            TotalCount = totalStats?.Count ?? 0,
            PaidAmount = paidStats?.Amount ?? 0,
            PaidCount = paidStats?.Count ?? 0,
            PayChannelBreakdown = channelBreakdown,
            StatusBreakdown = statusBreakdown,
        };
    }

    /// <summary>
    /// 游戏支付订单人工补发（高风险）。走 <see cref="TenantGameOrderShipService.CompensateAsync"/> 审计路径：
    /// 必须带操作人（当前登录 Admin），独立幂等键 + OperatorId 落账，不改动订单状态、不重复累加角色累充。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Send")]
    public async Task SendAsync(AddTenantGameOrderEntityRequest input)
    {
        await _shipService.CompensateAsync(input.Id, "人工补发", _currentUserContext.UserId);
    }

    private class AggregateRow
    {
        public long? Amount { get; set; }
        public int? Count { get; set; }
    }
}
#pragma warning restore CS8602, CS8604, CS8629