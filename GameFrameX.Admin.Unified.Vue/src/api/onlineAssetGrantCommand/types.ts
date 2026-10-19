/* eslint-disable */
/* tslint:disable */

/** Online 资产命令（发放/撤销）确认响应：透传 Online 首次执行结果（vault:C4 幂等语义——同幂等键重试返回首次结果，不产生第二笔变更）。 */
export interface OnlineAssetGrantAckResponse {
  /** 交易 Id（本次命令产生或首次命中的交易）。 */
  transactionId?: string | null;
  /** 幂等键（服务端按业务单号规则生成的回执）。 */
  idempotencyKey?: string | null;
  /** 交易状态（字符串透传：Succeeded / Failed / Reverted 等）。 */
  status?: string | null;
  /**
   * 执行时间（Unix 秒；幂等重试时为首次执行时间）。
   * @format int64
   */
  occurredAt?: number;
}

/** 人工发放明细行（货币或道具 + 数量）。 */
export interface OnlineAssetGrantItemRequest {
  /**
   * 货币或道具类型（如 gold / diamond / item:1001）。
   * @minLength 1
   * @maxLength 256
   */
  itemOrCurrencyType: string;
  /**
   * 数量（必须大于 0）。
   * @format int64
   * @min 1
   * @max 9223372036854776000
   */
  quantity?: number;
}

/**
 * Online 资产人工发放输入参数（高危；vault:C4「Admin 人工发放」六类来源之一）。
 *
 * 经 Online Grant API 执行，Admin 侧零直写；`Reason` 必填进审计；幂等键（业务单号）规则见命令服务。
 */
export interface OnlineAssetGrantRequest {
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
   * 目标玩家 Id。
   * @minLength 1
   * @maxLength 256
   */
  playerId: string;
  /** 发放明细（至少一条）。 */
  grantItems: OnlineAssetGrantItemRequest[];
  /**
   * 发放原因（必填，进审计；对应前端弹层理由输入）。
   * @minLength 1
   * @maxLength 512
   */
  reason: string;
  /**
   * 业务单号（可选；提供时以其为幂等键——同单号重试返回 Online 首次结果；未提供时服务端生成新单号，每次调用都是新业务意图）。
   * @maxLength 256
   */
  businessOrderNumber?: string | null;
}

/**
 * Online 资产人工撤销输入参数（高危；与 GameFrameX.Application.Game.Online.Asset.Dto.OnlineAssetGrantRequest 同构 + 目标 TransactionId）。
 *
 * 撤销的是一笔已生效交易的资产变更，经 Online Grant/Transaction API 执行，Admin 侧零直写。
 */
export interface OnlineAssetRevokeRequest {
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
   * 目标玩家 Id。
   * @minLength 1
   * @maxLength 256
   */
  playerId: string;
  /**
   * 被撤销的交易 Id（必填；从交易详情页发起撤销时携带）。
   * @minLength 1
   * @maxLength 256
   */
  transactionId: string;
  /**
   * 撤销原因（必填，进审计）。
   * @minLength 1
   * @maxLength 512
   */
  reason: string;
  /**
   * 业务单号（可选；语义同发放——提供时为幂等键，未提供时服务端生成）。
   * @maxLength 256
   */
  businessOrderNumber?: string | null;
}

export interface OnlineAssetGrantCommandGrantPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** Online 资产命令（发放/撤销）确认响应：透传 Online 首次执行结果（vault:C4 幂等语义——同幂等键重试返回首次结果，不产生第二笔变更）。 */
  data: OnlineAssetGrantAckResponse;
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

export interface OnlineAssetGrantCommandRevokePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** Online 资产命令（发放/撤销）确认响应：透传 Online 首次执行结果（vault:C4 幂等语义——同幂等键重试返回首次结果，不产生第二笔变更）。 */
  data: OnlineAssetGrantAckResponse;
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
