/* eslint-disable */
/* tslint:disable */

/**
 * 资产审计行（命令类型 + 操作者 + 目标玩家 + 时间；作用域 App/Server 由查询作用域隐含）。
 * 线缆与 Admin 响应共用形态（审计行无脱敏面）。
 */
export interface OnlineAssetAuditEntryResponse {
  /** 关联交易 Id（经其反查交易详情与账本行）。 */
  transactionId?: string | null;
  /** 命令类型（grant / revoke 等）。 */
  commandType?: string | null;
  /** 操作者 Id。 */
  operatorId?: string | null;
  /** 目标玩家 Id。 */
  playerId?: string | null;
  /** 原因（进审计的操作理由原文）。 */
  reason?: string | null;
  /** 业务单号。 */
  businessOrderNumber?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
}

/** 资产审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。 */
export interface OnlineAssetAuditPageResponse {
  /** 当前页审计行。 */
  items?: OnlineAssetAuditEntryResponse[] | null;
  /** 下一页游标（null = 没有更多数据）。 */
  nextCursor?: string | null;
  /** 是否还有更多数据。 */
  hasMore?: boolean;
}

/**
 * 资产审计分页查询输入参数（操作者/时间/作用域/命令类型 + 目标玩家；Cursor 翻页，vault:C2 S1.2）。
 *
 * AppId/ServerId 必填（EXECUTE 期回写：C339 作用域三元组为每次 Online 调用硬前置，审计查询无跨区服聚合端点）。
 */
export interface PageOnlineAssetAuditRequest {
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
   * 操作者 Id 过滤（null = 不过滤）。
   * @maxLength 256
   */
  operatorId?: string | null;
  /**
   * 目标玩家 Id 过滤（null = 不过滤）。
   * @maxLength 256
   */
  playerId?: string | null;
  /**
   * 命令类型过滤（grant / revoke 等；null = 不过滤）。
   * @maxLength 256
   */
  commandType?: string | null;
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

export interface OnlineAssetAuditQueryPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 资产审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。 */
  data: OnlineAssetAuditPageResponse;
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
