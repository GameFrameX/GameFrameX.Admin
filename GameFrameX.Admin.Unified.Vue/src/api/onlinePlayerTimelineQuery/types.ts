/* eslint-disable */
/* tslint:disable */

/**
 * 玩家时间线事件分组（严格六值，来源 change C344 / vault:C9 S8.1）。
 *
 * 仅作为查询侧过滤参数：按业务域收窄事件范围（如只看处罚链）。响应行内的 `EventType`/`Group`
 * 均为 Online 事件信封原值 string 透传——Admin 不做事件枚举映射、不推断分组归属（缺事件即空槽，X6 口径红线）。
 * @format int32
 */
export enum OnlinePlayerTimelineEventGroup {
  /** Session */
  Session = 1,
  /** Match */
  Match = 2,
  /** Asset */
  Asset = 3,
  /** Social */
  Social = 4,
  /** Penalty */
  Penalty = 5,
  /** LiveOps */
  LiveOps = 6,
}

/**
 * 玩家时间线事件行（Online 事件信封原值透传；change C344 / vault:C9 S8.1）。
 *
 * 全部字段来自 Online 事件信封——Admin 不构造事件、不推断归属、不补齐空位（缺事件即空槽，由前端呈现）。
 */
export interface OnlinePlayerTimelineEntryResponse {
  /** 事件 Id（Online 信封 EventId）。 */
  eventId?: string | null;
  /** 事件类型（Online 原值，如 session.login / match.ticket.created）。 */
  eventType?: string | null;
  /** 事件分组（Online 原值；与请求侧 OnlinePlayerTimelineEventGroup 枚举语义对齐，未知值前端兜底展示）。 */
  group?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
  /** 事件来源模块（Online 信封 Source）。 */
  source?: string | null;
  /** 关联 Id（跨模块串联键，vault:C9 可观测关联键约定）。 */
  correlationId?: string | null;
  /** 载荷摘要（Online 侧裁剪后的 Payload 摘要，非完整载荷）。 */
  payloadSummary?: string | null;
}

/** 玩家时间线查询输入参数（change C344 / vault:C9 S8.1；Cursor 翻页，vault:C2 S1.2）。 */
export interface OnlinePlayerTimelineRequest {
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
   * 目标玩家 Id（必填）。
   * @minLength 1
   * @maxLength 256
   */
  playerId: string;
  /**
   * 玩家时间线事件分组（严格六值，来源 change C344 / vault:C9 S8.1）。
   *
   * 仅作为查询侧过滤参数：按业务域收窄事件范围（如只看处罚链）。响应行内的 `EventType`/`Group`
   * 均为 Online 事件信封原值 string 透传——Admin 不做事件枚举映射、不推断分组归属（缺事件即空槽，X6 口径红线）。
   */
  eventGroup?: OnlinePlayerTimelineEventGroup;
  /**
   * 时间窗起点（Unix 秒；null = 不限）。
   * @format int64
   */
  startTime?: number | null;
  /**
   * 时间窗终点（Unix 秒；null = 不限）。
   * @format int64
   */
  endTime?: number | null;
  /**
   * 游标（上一页返回的 NextCursor；首页不传）。
   * @maxLength 256
   */
  cursor?: string | null;
  /**
   * 页大小（1～100，默认 20）。
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
}

/** 玩家时间线查询响应（Admin 出参；change C344 / vault:C9 S8.1）。 */
export interface OnlinePlayerTimelineResponse {
  /** 目标玩家 Id（回显查询锚点）。 */
  playerId?: string | null;
  /** 事件行（Online 事件信封原值透传；缺事件即空槽，Admin 不做本地推断补齐）。 */
  events?: OnlinePlayerTimelineEntryResponse[] | null;
  /** 下一页游标（null = 没有更多）。 */
  nextCursor?: string | null;
  /** 是否还有更多事件。 */
  hasMore?: boolean;
}

export interface OnlinePlayerTimelineQueryQueryPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 玩家时间线查询响应（Admin 出参；change C344 / vault:C9 S8.1）。 */
  data: OnlinePlayerTimelineResponse;
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
