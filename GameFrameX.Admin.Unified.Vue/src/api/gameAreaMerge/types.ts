/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum GameAreaMergePlanStatus {
  /** 草稿 */
  Draft = 0,
  /** 已预演 */
  DryRunCompleted = 1,
  /** 已批准 */
  Approved = 2,
  /** 执行中 */
  Executing = 3,
  /** 已完成 */
  Completed = 4,
  /** 失败 */
  Failed = 5,
  /** 已取消 */
  Cancelled = 6,
}

/** 合服流程数据。 */
export interface ApproveGameAreaMergePlanRequest {
  id?: string;
  /** @minLength 1 */
  reason: string;
  reviewerOperatorId?: string;
}

export interface BasePageRequest {
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
}

/** 取消合服计划请求。 */
export interface CancelGameAreaMergePlanRequest {
  id?: string;
  /** @minLength 1 */
  reason: string;
}

/** 合服流程数据。 */
export interface CreateGameAreaMergePlanRequest {
  targetServerId?: string;
  sourceServerIds?: number[] | null;
  /** @minLength 1 */
  reason: string;
}

/** 合服流程数据。 */
export interface ExecuteGameAreaMergePlanRequest {
  id?: string;
  /** @minLength 1 */
  backupIdentifier: string;
  backupConfirmed?: boolean;
}

/** 合服计划详情，包含阶段报告快照。 */
export interface GameAreaMergePlanResponse {
  id?: string;
  rowVersion?: string | null;
  /** @format int64 */
  createdTime?: number;
  targetServerId?: string;
  sourceServerIds?: string | null;
  planId?: string | null;
  status?: GameAreaMergePlanStatus;
  dryRunReportSnapshot?: string | null;
  executionReportSnapshot?: string | null;
  applicantOperatorId?: string;
  reviewerOperatorId?: string | null;
  reason?: string | null;
  backupIdentifier?: string | null;
  backupConfirmed?: boolean;
  /** @format int64 */
  dryRunTime?: number | null;
  /** @format int64 */
  approvedTime?: number | null;
  /** @format int64 */
  executionStartedTime?: number | null;
  /** @format int64 */
  completedTime?: number | null;
  /** @format int64 */
  failedTime?: number | null;
  failureMessage?: string | null;
  cancelledByOperatorId?: string | null;
  /** @format int64 */
  cancelledTime?: number | null;
  cancellationReason?: string | null;
  cancellable?: boolean;
  executionReclaimable?: boolean;
}

export interface GameAreaMergePlanResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAreaMergePlanResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface GameAreaMergeAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergeApprovePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergeCancelPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergeDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergeDryRunPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergeExecutePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 合服计划详情，包含阶段报告快照。 */
  data: GameAreaMergePlanResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameAreaMergePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAreaMergePlanResponseSqlSugarPagedList;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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
