/* eslint-disable */
/* tslint:disable */

/** 平台日志配置输出。ES !:ElasticSearchServerUris 经掩码处理，不回显明文 URI/凭据。 */
export interface PlatformLoggingConfigResponse {
  file?: PlatformLoggingFileResponse;
  database?: PlatformLoggingDatabaseResponse;
  /** ES 日志输出。ServerUris 元素统一掩码为 `"***"`，仅保留数量，不回显明文。 */
  elasticSearch?: PlatformLoggingElasticSearchResponse;
  monitor?: PlatformLoggingMonitorResponse;
  /**
   * 最近更新时间（Unix 秒）。
   * @format int64
   */
  updateTime?: number | null;
}

/**
 * 平台日志配置更新输入。
 * ES !:ElasticSearchServerUris：若全部为掩码占位 `"***"` 视为「未变更」，保留库内原值；
 * 含非占位项则按非占位项替换；空列表视为清空。
 */
export interface PlatformLoggingConfigUpdateRequest {
  file?: PlatformLoggingFileUpdateRequest;
  database?: PlatformLoggingDatabaseUpdateRequest;
  elasticSearch?: PlatformLoggingElasticSearchUpdateRequest;
  monitor?: PlatformLoggingMonitorUpdateRequest;
}

export interface PlatformLoggingDatabaseResponse {
  isEnabled?: boolean;
  minimumLevel?: string | null;
}

export interface PlatformLoggingDatabaseUpdateRequest {
  isEnabled?: boolean;
  minimumLevel?: string | null;
}

/** ES 日志输出。ServerUris 元素统一掩码为 `"***"`，仅保留数量，不回显明文。 */
export interface PlatformLoggingElasticSearchResponse {
  isEnabled?: boolean;
  serverUris?: string[] | null;
  defaultIndex?: string | null;
}

export interface PlatformLoggingElasticSearchUpdateRequest {
  isEnabled?: boolean;
  serverUris?: string[] | null;
  defaultIndex?: string | null;
}

export interface PlatformLoggingFileResponse {
  isEnabled?: boolean;
  fileName?: string | null;
  append?: boolean;
  /** @format int64 */
  fileSizeLimitBytes?: number;
  /** @format int32 */
  maxRollingFiles?: number;
}

export interface PlatformLoggingFileUpdateRequest {
  isEnabled?: boolean;
  /**
   * @min 1
   * @max 4096
   */
  fileName?: string | null;
  append?: boolean;
  /**
   * @format int64
   * @min 1024
   * @max 1073741824
   */
  fileSizeLimitBytes?: number;
  /**
   * @format int32
   * @min 1
   * @max 365
   */
  maxRollingFiles?: number;
}

export interface PlatformLoggingMonitorResponse {
  globalEnabled?: boolean;
  bahLogLevel?: string | null;
  withReturnValue?: boolean;
  /** @format int32 */
  returnValueThreshold?: number;
  jsonBehavior?: string | null;
  jsonIndented?: boolean;
  useUtcTimestamp?: boolean;
}

export interface PlatformLoggingMonitorUpdateRequest {
  globalEnabled?: boolean;
  bahLogLevel?: string | null;
  withReturnValue?: boolean;
  /**
   * @format int32
   * @min 0
   * @max 100000
   */
  returnValueThreshold?: number;
  jsonBehavior?: string | null;
  jsonIndented?: boolean;
  useUtcTimestamp?: boolean;
}

export interface PlatformLoggingConfigDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台日志配置输出。ES !:ElasticSearchServerUris 经掩码处理，不回显明文 URI/凭据。 */
  data: PlatformLoggingConfigResponse;
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

export type PlatformLoggingConfigUpdatePostData = any;
