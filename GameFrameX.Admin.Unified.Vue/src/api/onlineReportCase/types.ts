/* eslint-disable */
/* tslint:disable */

import type { OnlinePenaltyLinkResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum OnlineReportDecisionKind {
  /** 驳回 */
  Dismiss = 1,
  /** 警告 */
  Warn = 2,
  /** 禁言 */
  Mute = 3,
  /** 封禁 */
  Ban = 4,
  /** 转人工处罚 */
  ForwardPenalty = 5,
}

/** @format int32 */
export enum OnlineReportCaseStatusKind {
  /** 待处理 */
  Pending = 1,
  /** 处理中 */
  Handling = 2,
  /** 已处理 */
  Resolved = 3,
  /** 已驳回 */
  Rejected = 4,
}

/** 举报案件条目（vault:C7 举报证据链全字段透传：举报人/被举报人/场景/MatchId/ChatMessageId/原因/证据/状态/处理结果）。 */
export interface OnlineReportCaseItemResponse {
  /** 案件 Id（Online 侧标识，字符串形态）。 */
  caseId?: string | null;
  /** 应用作用域。 */
  appId?: string;
  /** 区服作用域。 */
  serverId?: string;
  /** 举报人玩家 Id。 */
  reporterPlayerId?: string | null;
  /** 被举报人玩家 Id。 */
  reportedPlayerId?: string | null;
  /** 举报场景。 */
  scene?: string | null;
  /** 关联对局 Id（证据链字段；null = 非对局内举报）。 */
  matchId?: string | null;
  /** 关联聊天消息 Id（证据链字段；null = 非聊天举报）。 */
  chatMessageId?: string | null;
  /** 举报原因。 */
  reason?: string | null;
  /** 证据（Online 侧证据载荷引用；null = 无证据）。 */
  evidence?: string | null;
  status?: OnlineReportCaseStatusKind;
  /** 处理结果摘要（已处置案件的非空；含联动中间态描述）。 */
  handleResult?: string | null;
  /**
   * 举报时间（Unix 秒）。
   * @format int64
   */
  createdAt?: number;
  /**
   * 最近更新时间（Unix 秒）。
   * @format int64
   */
  updatedAt?: number;
}

/** 举报案件分页响应（Cursor + HasMore，透传 Online 游标语义）。 */
export interface OnlineReportCasePageResponse {
  /** 当前页案件条目。 */
  items?: OnlineReportCaseItemResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
}

/** 举报案件处置输入参数（处置流：事实源写入 → 联动命令 → 当事人通知，三步状态落审计）。 */
export interface OnlineReportHandleRequest {
  /**
   * 应用作用域（案件所在应用，经 OnlineScopeContextFactory 校验租户归属）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 区服作用域（案件所在区服，经 OnlineScopeContextFactory 校验租户与应用归属）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  /**
   * 案件 Id（Online 侧标识）。
   * @minLength 1
   */
  caseId: string;
  /**
   * 被举报玩家 Id（Mute / Ban 时写入事实源 TargetId；案件数据 Admin 不落镜像，由调用方随案件行携带）。
   * @minLength 1
   * @maxLength 256
   */
  reportedPlayerId: string;
  decision: OnlineReportDecisionKind;
  /**
   * 处置原因（必填，进入事实源 Reason 与审计）。
   * @minLength 1
   * @maxLength 2048
   */
  reason: string;
  /**
   * 处罚时长（分钟）。Decision 为 Mute / Ban 时必填且大于 0；其余决策忽略。
   * @format int32
   * @min 1
   * @max 2147483647
   */
  durationMinutes?: number | null;
}

/** 举报案件处置响应（三步结果聚合：事实源写入 → 联动命令 → 当事人通知）。 */
export interface OnlineReportHandleResponse {
  /** 案件 Id。 */
  caseId?: string | null;
  decision?: OnlineReportDecisionKind;
  /** 落账的处罚事实源行 Id（仅 Mute / Ban；其余决策为 null）。 */
  penaltyId?: string | null;
  /** 处罚联动命令聚合响应（按联动目标区服集逐区服执行后的聚合结果）。 */
  linkCommand?: OnlinePenaltyLinkResponse;
  /**
   * 案件状态是否已回写 Online（false 时见 CaseUpdateFailureReason；案件主状态仍以此回写为准，
   * Admin 不落镜像，回写失败须人工重查重处）。
   */
  caseUpdated?: boolean;
  /** 案件状态回写失败原因（回写成功时为 null）。 */
  caseUpdateFailureReason?: string | null;
  /** 当事人通知是否送达（通知失败不阻断处置，原因见 NotifiedFailureReason）。 */
  playerNotified?: boolean;
  /** 通知失败原因（送达时为 null）。 */
  notifiedFailureReason?: string | null;
  /**
   * 处置完成时间（Unix 秒）。
   * @format int64
   */
  handledAt?: number;
}

/** 举报案件分页查询输入参数（案件数据实时透传 Online，Admin 不落镜像；作用域必填，与消息审计页一致）。 */
export interface PageOnlineReportCaseRequest {
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
  status?: OnlineReportCaseStatusKind;
  /** 举报人玩家 Id 过滤（null = 不过滤）。 */
  reporterPlayerId?: string | null;
  /**
   * 举报时间窗起点（Unix 秒；null = 不限）。
   * @format int64
   */
  startTime?: number | null;
  /**
   * 举报时间窗终点（Unix 秒；null = 不限）。
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

export interface OnlineReportCaseHandlePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 举报案件处置响应（三步结果聚合：事实源写入 → 联动命令 → 当事人通知）。 */
  data: OnlineReportHandleResponse;
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

export interface OnlineReportCasePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 举报案件分页响应（Cursor + HasMore，透传 Online 游标语义）。 */
  data: OnlineReportCasePageResponse;
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
