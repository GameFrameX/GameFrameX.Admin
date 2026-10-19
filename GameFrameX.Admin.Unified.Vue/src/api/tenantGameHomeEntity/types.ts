/* eslint-disable */
/* tslint:disable */

/** 全服在线人数结果 */
export interface GameHomeOnlineResponse {
  /**
   * 全服在线人数（启用区服在线人数之和，区服不可达或未配置管理地址时按 0 计）
   * @format int32
   */
  onlineCount?: number;
}

export interface GameHomeOnlineResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 全服在线人数结果 */
  data?: GameHomeOnlineResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 首页在线人数时序单点（C321 / GFX-730） */
export interface GameHomeOnlineTrendPointResponse {
  /**
   * 整点桶时间（Unix 秒）
   * @format int64
   */
  bucketTime?: number;
  /**
   * 该小时内最后一次采样的在线人数；无采样数据时为 null
   * @format int32
   */
  onlineCount?: number | null;
}

/**
 * 首页 24h 在线人数时序（C321 / GFX-730）
 *
 * 最近 24 个整点桶（含当前进行中桶），桶内取该小时最后一次采样值；无采样数据的桶 OnlineCount 为 null（前端降级画 —）。
 */
export interface GameHomeOnlineTrendResponse {
  /** 趋势点列表（按桶时间升序，共 24 个点） */
  points?: GameHomeOnlineTrendPointResponse[] | null;
}

export interface GameHomeOnlineTrendResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /**
   * 首页 24h 在线人数时序（C321 / GFX-730）
   *
   * 最近 24 个整点桶（含当前进行中桶），桶内取该小时最后一次采样值；无采样数据的桶 OnlineCount 为 null（前端降级画 —）。
   */
  data?: GameHomeOnlineTrendResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/**
 * 首页运营驾驶舱聚合读数（C321 / GFX-730）
 *
 * 纯聚合计数输出，非 CRUD Response，无 Id/RowVersion（同 `GameHomeOnlineResponse` / `GameOrderSummaryResponse` 先例）。
 */
export interface GameHomeOverviewResponse {
  /**
   * 当前全服在线人数（启用区服在线人数之和，区服不可达或未配置管理地址时按 0 计）
   * @format int32
   */
  onlineCount?: number;
  /**
   * 今日订单数（当日 0 点起全部状态订单）
   * @format int32
   */
  todayOrderCount?: number;
  /**
   * 待发货订单数（全量待办视角，不限下单日期，状态=未发货）
   * @format int32
   */
  pendingShipmentCount?: number;
  /**
   * 退款中订单数（全量待办视角，状态=发起退款或同意退款）
   * @format int32
   */
  refundingCount?: number;
  /**
   * 玩家总数
   * @format int32
   */
  playerTotalCount?: number;
  /**
   * 今日新增玩家数（当日 0 点起创建）
   * @format int32
   */
  playerTodayNewCount?: number;
  /**
   * 区服总数
   * @format int32
   */
  areaTotalCount?: number;
  /**
   * 运行中区服数（ServerStatus=Online）
   * @format int32
   */
  areaRunningCount?: number;
  /**
   * 维护中区服数（ServerStatus=Maintenance）
   * @format int32
   */
  areaMaintenanceCount?: number;
}

export interface GameHomeOverviewResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /**
   * 首页运营驾驶舱聚合读数（C321 / GFX-730）
   *
   * 纯聚合计数输出，非 CRUD Response，无 Id/RowVersion（同 `GameHomeOnlineResponse` / `GameOrderSummaryResponse` 先例）。
   */
  data?: GameHomeOverviewResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export type TenantGameHomeEntityGetOnlinePostData =
  GameHomeOnlineResponseHttpJsonResultData;

export type TenantGameHomeEntityOnlineTrendPostData =
  GameHomeOnlineTrendResponseHttpJsonResultData;

export type TenantGameHomeEntityOverviewPostData =
  GameHomeOverviewResponseHttpJsonResultData;
