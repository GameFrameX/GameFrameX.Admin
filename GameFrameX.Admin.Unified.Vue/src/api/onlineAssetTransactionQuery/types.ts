/* eslint-disable */
/* tslint:disable */

/**
 * 资产交易详情响应（Transaction/Grant 全字段：TransactionId、IdempotencyKey、状态、来源、原因、业务单号、操作者、Grant 明细、时间；
 * vault:C4 交易不变量——每笔变更可追溯，VC-3.14 Admin 展示半边）。
 */
export interface OnlineAssetTransactionDetailResponse {
  /** 交易 Id。 */
  transactionId?: string | null;
  /** 幂等键（业务单号承载；同键重试返回首次结果）。 */
  idempotencyKey?: string | null;
  /** 交易状态（字符串透传 Online 状态机：Succeeded / Failed / Reverted 等，Admin 不解释状态机只展示）。 */
  status?: string | null;
  /** 来源（vault:C4 六类）。 */
  source?: string | null;
  /** 原因（进审计的操作理由原文）。 */
  reason?: string | null;
  /** 业务单号。 */
  businessOrderNumber?: string | null;
  /** 操作者 Id。 */
  operatorId?: string | null;
  /** 目标玩家 Id。 */
  playerId?: string | null;
  /**
   * 发生时间（Unix 秒）。
   * @format int64
   */
  occurredAt?: number;
  /** Grant 明细（本次交易的资产维度变更列表）。 */
  grantItems?: OnlineAssetTransactionGrantItemResponse[] | null;
}

/** 交易详情内的 Grant 明细行（Item/道具 或 Currency/货币、数量、前后值）。 */
export interface OnlineAssetTransactionGrantItemResponse {
  /** 货币或道具类型。 */
  itemOrCurrencyType?: string | null;
  /**
   * 数量（发放为正、撤销为负）。
   * @format int64
   */
  quantity?: number;
  /**
   * 变更前值。
   * @format int64
   */
  beforeValue?: number;
  /**
   * 变更后值。
   * @format int64
   */
  afterValue?: number;
}

export interface OnlineAssetTransactionQueryDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 资产交易详情响应（Transaction/Grant 全字段：TransactionId、IdempotencyKey、状态、来源、原因、业务单号、操作者、Grant 明细、时间；
   * vault:C4 交易不变量——每笔变更可追溯，VC-3.14 Admin 展示半边）。
   */
  data: OnlineAssetTransactionDetailResponse;
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
