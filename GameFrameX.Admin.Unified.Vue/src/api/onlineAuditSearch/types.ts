/* eslint-disable */
/* tslint:disable */

/**
 * 统一审计行来源标识（change C344 / vault:C9 S8.2；三来源口径见 OnlineAuditSourceMap）。
 * @format int32
 */
export enum OnlineAuditSourceKind {
  /** AdminOperation */
  AdminOperation = 1,
  /** OnlineAuditEvent */
  OnlineAuditEvent = 2,
  /** DomainCommandRecord */
  DomainCommandRecord = 3,
}

/**
 * 统一审计检索行（三来源五维统一字段；change C344 / vault:C9 S8.2）。
 *
 * 字段映射唯一来源是 OnlineAuditSourceMap——禁止在 Service 内散落第二套映射。
 */
export interface OnlineAuditSearchEntryResponse {
  /** 统一审计行来源标识（change C344 / vault:C9 S8.2；三来源口径见 OnlineAuditSourceMap）。 */
  source?: OnlineAuditSourceKind;
  /** 操作者标识（Admin 操作日志 = 账号；Online 审计 = 操作者 Id；域命令 = 操作者 Id/账号）。 */
  operatorIdentifier?: string | null;
  /** 目标对象标识（玩家 Id 等；提取不到时为 null，不伪造）。 */
  targetPlayerIdentifier?: string | null;
  /** 应用作用域（提取不到时为 null）。 */
  appId?: string | null;
  /** 区服作用域（提取不到时为 null）。 */
  serverId?: string | null;
  /** 命令类型（各来源原值：Controller.Action / Online CommandType / 域命令 ActionType）。 */
  commandType?: string | null;
  /**
   * 发生时间（Unix 秒，三来源统一排序键）。
   * @format int64
   */
  occurredAt?: number;
  /** 摘要（操作内容/原因概述）。 */
  summary?: string | null;
  /** 关联 Id（可观测关联键；无则为 null）。 */
  correlationId?: string | null;
}

/**
 * 统一审计检索分页输入参数（五维过滤：操作者/对象/作用域/命令类型/时间窗；change C344 / vault:C9 S8.2）。
 *
 * 作用域维度（AppId/ServerId）在此是可选过滤而非调用前置：本地两来源（Admin 操作日志 / 域命令记录）
 * 支持无作用域全量检索；Online 审计事件来源仅在 AppId + ServerId 均提供时参与聚合
 * （C339 作用域三元组是每次 Online 调用硬前置），未锚定时该来源静默跳过（三来源口径见 OnlineAuditSourceMap）。
 */
export interface PageOnlineAuditSearchRequest {
  /**
   * 操作者过滤（Admin 操作日志按账号/姓名匹配，Online/域命令按操作者 Id；null = 不过滤）。
   * @maxLength 256
   */
  operator?: string | null;
  /**
   * 目标对象过滤（玩家 Id 等；null = 不过滤。Admin 操作日志来源经 RequestParam 提取尽力匹配，提取不到的行不命中）。
   * @maxLength 256
   */
  targetPlayerId?: string | null;
  /**
   * 应用作用域过滤（null = 全部应用；Online 审计事件来源要求与 ServerId 同时提供才参与聚合）。
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string | null;
  /**
   * 区服作用域过滤（null = 全部区服；Online 审计事件来源要求与 AppId 同时提供才参与聚合）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId?: string | null;
  /**
   * 命令类型过滤（substring 匹配，如 penalty / grant / rollout；null = 不过滤）。
   * @maxLength 256
   */
  commandKind?: string | null;
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
  /**
   * 游标（上一页返回的 NextCursor = 末行 OccurredAt Unix 秒的时间倒序 keyset；首页不传）。
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

/** 统一审计检索分页响应（Admin 出参；change C344 / vault:C9 S8.2）。 */
export interface PageOnlineAuditSearchResponse {
  /** 统一审计行（三来源归并，按 OccurredAt 倒序）。 */
  items?: OnlineAuditSearchEntryResponse[] | null;
  /** 下一页游标（= 末行 OccurredAt Unix 秒的时间倒序 keyset；null = 没有更多）。 */
  nextCursor?: string | null;
  /** 是否还有更多（任一来源仍有剩余即为 true）。 */
  hasMore?: boolean;
}

export interface OnlineAuditSearchPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 统一审计检索分页响应（Admin 出参；change C344 / vault:C9 S8.2）。 */
  data: PageOnlineAuditSearchResponse;
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
