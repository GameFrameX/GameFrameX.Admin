namespace GameFrameX.Application.Game.Home.GameHome.Dto;

/// <summary>
/// 首页运营驾驶舱聚合读数（C321 / GFX-730）
/// <para>纯聚合计数输出，非 CRUD Response，无 Id/RowVersion（同 <c>GameHomeOnlineResponse</c> / <c>GameOrderSummaryResponse</c> 先例）。</para>
/// </summary>
public class GameHomeOverviewResponse
{
    /// <summary>
    /// 当前全服在线人数（启用区服在线人数之和，区服不可达或未配置管理地址时按 0 计）
    /// </summary>
    public int OnlineCount { get; set; }

    /// <summary>
    /// 今日订单数（当日 0 点起全部状态订单）
    /// </summary>
    public int TodayOrderCount { get; set; }

    /// <summary>
    /// 待发货订单数（全量待办视角，不限下单日期，状态=未发货）
    /// </summary>
    public int PendingShipmentCount { get; set; }

    /// <summary>
    /// 退款中订单数（全量待办视角，状态=发起退款或同意退款）
    /// </summary>
    public int RefundingCount { get; set; }

    /// <summary>
    /// 玩家总数
    /// </summary>
    public int PlayerTotalCount { get; set; }

    /// <summary>
    /// 今日新增玩家数（当日 0 点起创建）
    /// </summary>
    public int PlayerTodayNewCount { get; set; }

    /// <summary>
    /// 区服总数
    /// </summary>
    public int AreaTotalCount { get; set; }

    /// <summary>
    /// 运行中区服数（ServerStatus=Online）
    /// </summary>
    public int AreaRunningCount { get; set; }

    /// <summary>
    /// 维护中区服数（ServerStatus=Maintenance）
    /// </summary>
    public int AreaMaintenanceCount { get; set; }
}
