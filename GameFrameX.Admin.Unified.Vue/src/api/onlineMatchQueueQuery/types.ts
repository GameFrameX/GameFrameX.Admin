/* eslint-disable */
/* tslint:disable */

/** 匹配队列总览查询输入参数（change C341 §3；AppId/ServerId 为 C339 作用域三元组硬前置）。 */
export interface OnlineMatchOverviewRequest {
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
  /**
   * 匹配模式过滤（null = 全部模式）。
   * @maxLength 256
   */
  mode?: string | null;
  /**
   * 地区过滤（null = 全部地区）。
   * @maxLength 256
   */
  region?: string | null;
}

/** 匹配队列总览响应（change C341 §3 A0 字段终版）。 */
export interface OnlineMatchOverviewResponse {
  /** 按 Mode/Region 维度的队列快照行。 */
  queues?: OnlineMatchQueueSnapshotResponse[] | null;
  /**
   * Online 侧快照时间（Unix 秒；每次拉取标注，前端展示用）。
   * @format int64
   */
  serverTime?: number;
  /** 快照说明（Admin 侧固定文案：运行态快照，非权威状态）。 */
  snapshotNote?: string | null;
}

/** 匹配队列快照行（按 Mode/Region 维度；运行态快照，Admin 不缓存为权威状态）。 */
export interface OnlineMatchQueueSnapshotResponse {
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

export interface OnlineMatchQueueQueryOverviewPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 匹配队列总览响应（change C341 §3 A0 字段终版）。 */
  data: OnlineMatchOverviewResponse;
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
