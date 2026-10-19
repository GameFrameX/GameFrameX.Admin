/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum RiskLevelEnum {
  /** 低风险 */
  Low = 1,
  /** 中风险 */
  Medium = 2,
  /** 高风险 */
  High = 3,
  /** 严重风险 */
  Critical = 4,
}

/** @format int32 */
export enum ManualReviewStateEnum {
  /** 待复核 */
  Pending = 1,
  /** 复核通过 */
  Approved = 2,
  /** 复核驳回 */
  Rejected = 3,
  /** 升级处理 */
  Escalated = 4,
}

/** @format int32 */
export enum AntiCheatTargetTypeEnum {
  /** 账号 */
  Account = 1,
  /** 角色 */
  Role = 2,
  /** 设备 */
  Device = 3,
  /** IP */
  Ip = 4,
}

/** @format int32 */
export enum AntiCheatSuggestedActionEnum {
  /** 无建议 */
  None = 1,
  /** 标记观察 */
  Flag = 2,
  /** 建议限制登录 */
  RestrictLogin = 3,
  /** 建议封禁 */
  Ban = 4,
}

/** @format int32 */
export enum AntiCheatProviderEnum {
  /** TopOn */
  TopOn = 1,
  /** GoogleAdMob */
  GoogleAdMob = 2,
  /** UnityAds */
  UnityAds = 3,
  /** 自定义 */
  Custom = 99,
}

/** 反作弊事件上报输入（SubmitAsync 接入）。不含复核相关字段——上报一律落 Pending，复核由 ReviewAsync 动作单独驱动。 */
export interface AddTenantGameAntiCheatEventRequest {
  /** 应用 Id（事实数据应用作用域，跟随上报来源应用）。 */
  appId: string;
  provider?: AntiCheatProviderEnum;
  /** Provider 名称。Provider=Custom 时必填；其它可空。 */
  providerName?: string | null;
  /** Provider 侧事件/交易 Id。可空。 */
  providerEventId?: string | null;
  targetType?: AntiCheatTargetTypeEnum;
  /**
   * 指向对象标识。
   * @minLength 1
   */
  targetId: string;
  riskLevel?: RiskLevelEnum;
  /**
   * 证据摘要（脱敏后的 Provider 原文摘要）。
   * @minLength 1
   */
  evidenceSummary: string;
  suggestedAction?: AntiCheatSuggestedActionEnum;
  /** 建议补充说明。可空。 */
  suggestedActionNote?: string | null;
  /**
   * 事件发生时间（Provider 上报）。
   * @format int64
   */
  occurredTime?: number;
  /**
   * 幂等键。建议组成：{Provider}:{ProviderEventId}:{TargetType}:{TargetId}:{OccurredTime:O}。
   * @minLength 1
   */
  idempotencyKey: string;
}

/** 反作弊事件分页查询输入。 */
export interface PageTenantGameAntiCheatEventRequest {
  searchKey?: string | null;
  /**
   * @format int32
   * @min 1
   * @max 2147483647
   */
  page?: number;
  /**
   * @format int32
   * @min 1
   * @max 100
   */
  pageSize?: number;
  orderBy?: string | null;
  isDescending?: boolean;
  /** 应用 Id。应用级反作弊事件分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
  provider?: AntiCheatProviderEnum;
  riskLevel?: RiskLevelEnum;
  manualReviewState?: ManualReviewStateEnum;
  targetType?: AntiCheatTargetTypeEnum;
  /** 指向对象标识过滤（精确匹配）。可空。 */
  targetId?: string | null;
}

/** 反作弊事件输出投影。 */
export interface TenantGameAntiCheatEventResponse {
  id?: string;
  tenantId?: string;
  appId?: string;
  provider?: AntiCheatProviderEnum;
  providerName?: string | null;
  providerEventId?: string | null;
  targetType?: AntiCheatTargetTypeEnum;
  targetId?: string | null;
  riskLevel?: RiskLevelEnum;
  evidenceSummary?: string | null;
  suggestedAction?: AntiCheatSuggestedActionEnum;
  suggestedActionNote?: string | null;
  manualReviewState?: ManualReviewStateEnum;
  reviewerId?: string | null;
  /** @format int64 */
  reviewedTime?: number | null;
  reviewDecision?: string | null;
  reviewNote?: string | null;
  /** @format int64 */
  occurredTime?: number;
  idempotencyKey?: string | null;
  rowVersion?: string | null;
  /** @format int64 */
  createdTime?: number | null;
  /** @format int64 */
  updateTime?: number | null;
}

export interface TenantGameAntiCheatEventResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAntiCheatEventResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 反作弊事件人工复核输入（ReviewAsync 动作）。仅允许在 Pending 状态下迁移到 Approved/Rejected/Escalated。 */
export interface UpdateTenantGameAntiCheatEventReviewRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
  manualReviewState: ManualReviewStateEnum;
  /** 复核备注。可空。 */
  reviewNote?: string | null;
}

export type TenantGameAntiCheatEventEntityDeletePostData = any;

export interface TenantGameAntiCheatEventEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 反作弊事件输出投影。 */
  data: TenantGameAntiCheatEventResponse;
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

export interface TenantGameAntiCheatEventEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAntiCheatEventResponseSqlSugarPagedList;
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

export type TenantGameAntiCheatEventEntityReviewPostData = any;

export interface TenantGameAntiCheatEventEntitySubmitPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 反作弊事件输出投影。 */
  data: TenantGameAntiCheatEventResponse;
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
