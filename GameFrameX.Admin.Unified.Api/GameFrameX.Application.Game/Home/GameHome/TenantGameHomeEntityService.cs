using System.ComponentModel;
using GameFrameX.Application.Game.Home.GameHome.Dto;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.Game.Home.GameHome;

/// <summary>
/// 游戏账号服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class TenantGameHomeEntityService : IAdminApiController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITenantDbConnectionProvider _tenantDbConnectionProvider;

    public TenantGameHomeEntityService(IHttpClientFactory httpClientFactory, ITenantDbConnectionProvider tenantDbConnectionProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
    }

    /// <summary>
    /// 获取在线玩家信息
    /// </summary>
    /// <returns></returns>
    [HttpPost("getOnline")]
    [ApiDescriptionSettings(Name = "GetOnline")]
    [DisplayName("获取在线玩家信息")]
    public async Task<HttpJsonResultData<GameHomeOnlineResponse>> GetOnlineAsync()
    {
        // C175：从全局 GameServerOptions.HttpRootUrl 改为区服级聚合。仅统计启用且配置了管理地址的区服，
        // 每个区服按 Control 能力前缀调用 GetOnlinePlayer，求和返回全服在线人数。
        // C321：区服表是租户库表，经租户库连接路由查询；平台面（无租户上下文）返回明确业务错误而非裸 500。
        var tenantDb = ResolveTenantDb("首页在线人数");
        var total = await GameHomeOnlineCounter.GetTotalOnlineAsync(tenantDb, _httpClientFactory);
        return HttpJsonResultData<GameHomeOnlineResponse>.Success(new GameHomeOnlineResponse { OnlineCount = total, });
    }

    /// <summary>
    /// 首页运营驾驶舱聚合读数：在线人数、今日订单（待发货/退款中细分）、玩家总数/今日新增、区服计数（C321 / GFX-730）
    /// </summary>
    /// <returns>聚合计数结果</returns>
    [HttpPost("overview")]
    [ApiDescriptionSettings(Name = "Overview")]
    [DisplayName("首页运营驾驶舱聚合读数")]
    public async Task<HttpJsonResultData<GameHomeOverviewResponse>> OverviewAsync()
    {
        var tenantDb = ResolveTenantDb("首页运营驾驶舱聚合读数");

        // 今日 0 点（服务器本地时区，运营视角）转 Unix 秒；CreatedTime 审计列为 Unix 秒，同为绝对时间可直接比较
        var todayStart = new DateTimeOffset(DateTime.Now.Date).ToUnixTimeSeconds();

        // 订单读数：今日订单数 + 待办细分（待发货/退款中为全量待办视角，不限下单日期）
        var todayOrderCount = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameOrderEntity>()
                                           .Where(u => u.CreatedTime >= todayStart)
                                           .CountAsync();
        var pendingShipmentCount = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameOrderEntity>()
                                                .Where(u => u.Status == OrderStatusEnum.NotShipped)
                                                .CountAsync();
        var refundingCount = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameOrderEntity>()
                                           .Where(u => u.Status == OrderStatusEnum.InitiateRefund || u.Status == OrderStatusEnum.RefundAgreement)
                                           .CountAsync();

        // 玩家读数：总数 + 今日新增（CreatedTime 索引见实体 SugarIndex）
        var playerTotalCount = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameUserEntity>()
                                             .CountAsync();
        var playerTodayNewCount = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameUserEntity>()
                                                .Where(u => u.CreatedTime >= todayStart)
                                                .CountAsync();

        // 区服读数：按 ServerStatus 分组计数（全局 ISafeEnabledFilter 已隐藏禁用区服，AreaTotalCount 即启用区服总数）
        var areaStatusCounts = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameAreaEntity>()
                                             .GroupBy(u => u.ServerStatus)
                                             .Select(u => new { ServerStatus = u.ServerStatus, Count = SqlFunc.AggregateCount(u.Id), })
                                             .ToListAsync();
        var areaTotalCount = 0;
        var areaRunningCount = 0;
        var areaMaintenanceCount = 0;
        foreach (var row in areaStatusCounts)
        {
            areaTotalCount += row.Count;
            if (row.ServerStatus == GameServerStatus.Online)
            {
                areaRunningCount += row.Count;
            }
            else if (row.ServerStatus == GameServerStatus.Maintenance)
            {
                areaMaintenanceCount += row.Count;
            }
        }

        var onlineCount = await GameHomeOnlineCounter.GetTotalOnlineAsync(tenantDb, _httpClientFactory);

        return HttpJsonResultData<GameHomeOverviewResponse>.Success(new GameHomeOverviewResponse
        {
            OnlineCount = onlineCount,
            TodayOrderCount = todayOrderCount,
            PendingShipmentCount = pendingShipmentCount,
            RefundingCount = refundingCount,
            PlayerTotalCount = playerTotalCount,
            PlayerTodayNewCount = playerTodayNewCount,
            AreaTotalCount = areaTotalCount,
            AreaRunningCount = areaRunningCount,
            AreaMaintenanceCount = areaMaintenanceCount,
        });
    }

    /// <summary>
    /// 首页 24h 在线人数时序：最近 24 个整点桶回读采样表，桶内取最后一次采样，缺桶为 null（C321 / GFX-730）
    /// </summary>
    /// <returns>趋势点列表（按桶时间升序，共 24 个点）</returns>
    [HttpPost("onlineTrend")]
    [ApiDescriptionSettings(Name = "OnlineTrend")]
    [DisplayName("首页 24h 在线人数时序")]
    public async Task<HttpJsonResultData<GameHomeOnlineTrendResponse>> OnlineTrendAsync()
    {
        var tenantDb = ResolveTenantDb("首页在线趋势");

        // 当前整点桶（Unix 秒整点值与时区无关）往前共 24 个桶，含当前进行中桶
        var currentBucket = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 3600 * 3600;
        var earliestBucket = currentBucket - 23 * 3600;

        // 一次回读窗口内全部采样，内存中按桶归并取每桶最后采样
        var samples = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameHomeOnlineSampleEntity>()
                                    .Where(u => u.SampleTime >= earliestBucket && u.SampleTime < currentBucket + 3600)
                                    .OrderBy(u => u.SampleTime)
                                    .ToListAsync();
        var lastSampleByHour = new Dictionary<long, int>();
        foreach (var sample in samples)
        {
            // 15 分钟桶归并到所在整点桶：samples 已按时间升序，后写覆盖先写即「桶内取最后采样」
            var hourBucket = sample.SampleTime / 3600 * 3600;
            lastSampleByHour[hourBucket] = sample.OnlineCount;
        }

        var points = new List<GameHomeOnlineTrendPointResponse>();
        for (var bucket = earliestBucket; bucket <= currentBucket; bucket += 3600)
        {
            lastSampleByHour.TryGetValue(bucket, out var lastSample);
            points.Add(new GameHomeOnlineTrendPointResponse
            {
                BucketTime = bucket,
                OnlineCount = lastSampleByHour.ContainsKey(bucket) ? lastSample : null,
            });
        }

        return HttpJsonResultData<GameHomeOnlineTrendResponse>.Success(new GameHomeOnlineTrendResponse { Points = points, });
    }

    /// <summary>
    /// 解析当前请求的租户库连接。区服/订单/玩家/采样均为租户库表，平台面（SuperAdmin 无目标租户）主库无对应物理表，
    /// 返回明确业务错误而非让查询落到主库触发 42P01 裸 500（lesson：2026-07-11-superadminonly-tenant-coupling-d1016）。
    /// </summary>
    private ISqlSugarClient ResolveTenantDb(string operationName)
    {
        var tenantId = TenantOperationContext.GetEffectiveTenantId();
        if (!tenantId.HasValue)
        {
            throw BusinessError.Oh(GameHomeErrorCode.TenantContextRequired);
        }

        var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(tenantId.Value);
        if (tenantDb == null)
        {
            throw BusinessError.Oh(GameHomeErrorCode.TenantDbUnavailable);
        }

        return tenantDb;
    }
}
