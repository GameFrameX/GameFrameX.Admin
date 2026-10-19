/* eslint-disable */
/* tslint:disable */

/**
 * 异常 Ticket 检索类别（Admin 侧自有检索枚举，vault 无此概念；change C341 §3 A0）。
 *
 * 定义在 Application.Game（非 Core.Enum），不触碰 EnumContractTests SHA-256 契约锁。
 * @format int32
 */
export enum AbnormalKind {
  /** LongWaiting */
  LongWaiting = 1,
  /** RepeatedCancel */
  RepeatedCancel = 2,
  /** Orphan */
  Orphan = 3,
}

/** 异常 Ticket 分页响应（change C341 §3 A0 字段终版：Items/NextCursor/HasMore + ServerTime）。 */
export interface OnlineMatchAbnormalTicketPageResponse {
  /** 当前页异常 Ticket 行（已做展示键映射）。 */
  items?: OnlineMatchAbnormalTicketResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
  /**
   * Online 侧快照时间（Unix 秒；每次拉取标注）。
   * @format int64
   */
  serverTime?: number;
}

/** 异常 Ticket 行（Admin 展示形态 = Online 原值 + 展示键映射产物，经 `OnlineMatchTicketDisplayMap` 集中生成）。 */
export interface OnlineMatchAbnormalTicketResponse {
  /** Ticket Id。 */
  ticketId?: string | null;
  /** 所属 Party Id（无队伍时为 null）。 */
  partyId?: string | null;
  /** 匹配模式。 */
  mode?: string | null;
  /** 地区。 */
  region?: string | null;
  /** Ticket 状态（Online 原值）。 */
  status?: string | null;
  /** Ticket 状态展示键（onlineMatch.ticketStatus.<小写原值>；未知原值兜底 onlineMatch.ticketStatus.unknown）。 */
  statusDisplayKey?: string | null;
  /**
   * 创建时间（Unix 秒）。
   * @format int64
   */
  createdAt?: number;
  /**
   * 过期时间（Unix 秒；未设置为 null）。
   * @format int64
   */
  expiresAt?: number | null;
  /**
   * 已等待秒数（LongWaiting 类别主看字段；未提供为 null）。
   * @format int64
   */
  waitingSeconds?: number | null;
  /**
   * 累计取消次数（RepeatedCancel 类别主看字段；未提供为 null）。
   * @format int64
   */
  cancelCount?: number | null;
}

/** 匹配失败原因分页响应（change C341 §3 A0 字段终版：Items/ReasonSummary/NextCursor/HasMore + ServerTime）。 */
export interface OnlineMatchFailurePageResponse {
  /** 当前页失败明细行（已做展示键映射）。 */
  items?: OnlineMatchFailureResponse[] | null;
  /** 按原因码聚合的摘要行（Online 原值透传）。 */
  reasonSummary?: OnlineMatchFailureReasonSummaryResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
  /**
   * Online 侧快照时间（Unix 秒；每次拉取标注）。
   * @format int64
   */
  serverTime?: number;
}

/** 匹配失败原因聚合摘要行（按原因码计数，Online 原值透传）。 */
export interface OnlineMatchFailureReasonSummaryResponse {
  /** 失败原因码（Online 原值）。 */
  reasonCode?: string | null;
  /**
   * 时间窗内该原因的失败次数。
   * @format int64
   */
  count?: number;
}

/** 匹配失败明细行（Admin 展示形态 = Online 原值 + 展示键映射产物，经 `OnlineMatchTicketDisplayMap` 集中生成）。 */
export interface OnlineMatchFailureResponse {
  /** Ticket Id。 */
  ticketId?: string | null;
  /** 所属 Party Id（无队伍时为 null）。 */
  partyId?: string | null;
  /** 失败原因码（Online 原值）。 */
  reasonCode?: string | null;
  /** 失败原因展示键（onlineMatch.failureReason.<小写原值>；未知原值兜底 onlineMatch.failureReason.unknown）。 */
  reasonDisplayKey?: string | null;
  /** 失败诊断原文（仅排查展示）。 */
  reasonText?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
}

/** 异常 Ticket 分页查询输入参数（change C341 §3；按异常类别检索，Cursor 翻页透传 Online）。 */
export interface PageOnlineMatchAbnormalTicketRequest {
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
   * 异常 Ticket 检索类别（Admin 侧自有检索枚举，vault 无此概念；change C341 §3 A0）。
   *
   * 定义在 Application.Game（非 Core.Enum），不触碰 EnumContractTests SHA-256 契约锁。
   */
  abnormalKind: AbnormalKind;
  /** 游标（上一页返回的 NextCursor；首页不传）。 */
  cursor?: string | null;
  /**
   * 页大小（1～100，默认 20）。
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
}

/** 匹配失败原因分页查询输入参数（change C341 §3；Cursor 翻页透传 Online）。 */
export interface PageOnlineMatchFailureRequest {
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
   * 失败原因码过滤（Online 原值：Timeout/CancelRace/RateLimited/DuplicateTicket；null = 不过滤）。
   * @maxLength 256
   */
  reasonCode?: string | null;
  /**
   * 发生时间窗起点（Unix 秒；null = 不限）。
   * @format int64
   */
  startTime?: number | null;
  /**
   * 发生时间窗终点（Unix 秒；null = 不限）。
   * @format int64
   */
  endTime?: number | null;
  /** 游标（上一页返回的 NextCursor；首页不传）。 */
  cursor?: string | null;
  /**
   * 页大小（1～100，默认 20）。
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
}

export interface OnlineMatchTicketQueryAbnormalPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 异常 Ticket 分页响应（change C341 §3 A0 字段终版：Items/NextCursor/HasMore + ServerTime）。 */
  data: OnlineMatchAbnormalTicketPageResponse;
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

export interface OnlineMatchTicketQueryFailurePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 匹配失败原因分页响应（change C341 §3 A0 字段终版：Items/ReasonSummary/NextCursor/HasMore + ServerTime）。 */
  data: OnlineMatchFailurePageResponse;
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
