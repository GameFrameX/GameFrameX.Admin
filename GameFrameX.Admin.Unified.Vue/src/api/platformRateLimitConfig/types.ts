/* eslint-disable */
/* tslint:disable */

/** 平台限流配置输出。 */
export interface PlatformRateLimitConfigResponse {
  /** 是否启用全局 IP 固定窗口限流。 */
  isEnabled?: boolean;
  /**
   * 当前配置版本。
   * @format int64
   */
  version?: number;
  /**
   * 单个固定窗口允许的请求数。
   * @format int32
   */
  permitLimit?: number;
  /**
   * 固定窗口秒数。
   * @format int32
   */
  windowSeconds?: number;
  /**
   * 排队请求数量。
   * @format int32
   */
  queueLimit?: number;
  /**
   * 最近更新时间。
   * @format int64
   */
  updateTime?: number | null;
}

/** 平台限流配置更新输入。 */
export interface PlatformRateLimitConfigUpdateRequest {
  /** 是否启用全局 IP 固定窗口限流。 */
  isEnabled?: boolean;
  /**
   * 单个固定窗口允许的请求数。
   * @format int32
   * @min 1
   * @max 100000
   */
  permitLimit?: number;
  /**
   * 固定窗口秒数。
   * @format int32
   * @min 1
   * @max 3600
   */
  windowSeconds?: number;
  /**
   * 排队请求数量。
   * @format int32
   * @min 0
   * @max 100000
   */
  queueLimit?: number;
}

export interface PlatformRateLimitConfigDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台限流配置输出。 */
  data: PlatformRateLimitConfigResponse;
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

export type PlatformRateLimitConfigUpdatePostData = any;
