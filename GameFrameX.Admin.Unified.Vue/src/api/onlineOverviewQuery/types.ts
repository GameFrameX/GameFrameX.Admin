/* eslint-disable */
/* tslint:disable */

/** 总览队列概况行（按 Mode/Region 维度；字段与 C341 匹配队列快照对齐，运行态数据不缓存）。 */
export interface OnlineOverviewQueueSummaryResponse {
  /** 匹配模式。 */
  mode?: string | null;
  /** 地区。 */
  region?: string | null;
  /**
   * 队列深度（当前排队 Ticket 数）。
   * @format int64
   */
  queueDepth?: number;
  /**
   * 平均等待秒数。
   * @format int64
   */
  averageWaitSeconds?: number;
  /**
   * 每分钟吞吐（最近时间窗匹配完成数）。
   * @format int64
   */
  throughputPerMinute?: number;
}

/** 在线总览快照查询输入参数（作用域必填；change C344 / vault:C9 S8.1）。 */
export interface OnlineOverviewRequest {
  /**
   * 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
}

/** 在线总览快照响应（Admin 出参；change C344 / vault:C9 S8.1）。 */
export interface OnlineOverviewResponse {
  /**
   * 在线玩家数（玩家侧连接口径，X6：不含管理员连接）。
   * @format int64
   */
  onlinePlayerCount?: number;
  /**
   * 活跃 Session 数。
   * @format int64
   */
  sessionCount?: number;
  /**
   * 进行中对局数。
   * @format int64
   */
  matchCount?: number;
  /**
   * 重连率（最近时间窗重连占比，0～1）。
   * @format double
   */
  reconnectRate?: number;
  /** 匹配队列概况（按 Mode/Region 维度）。 */
  queueSummaries?: OnlineOverviewQueueSummaryResponse[] | null;
  /**
   * Online 侧快照时间（Unix 秒）。
   * @format int64
   */
  serverTime?: number;
  /** 快照口径说明（运行态快照非权威状态，前端不得缓存）。 */
  snapshotNote?: string | null;
}

export interface OnlineOverviewQuerySnapshotPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 在线总览快照响应（Admin 出参；change C344 / vault:C9 S8.1）。 */
  data: OnlineOverviewResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系（C265）下恒为 null，数字业务错误码承载在 code 字段 */
  errorCode: string | null;
  /** 响应类型（success/warning/error） */
  type: string | null;
  /**
   * Unix 时间戳（秒）
   * @format int64
   */
  time: number;
  extras: object | null;
}
