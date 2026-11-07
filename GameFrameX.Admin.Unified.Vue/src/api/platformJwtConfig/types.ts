/* eslint-disable */
/* tslint:disable */

/** 平台 JWT 配置输出。 */
export interface PlatformJwtConfigResponse {
  /** 签名密钥掩码。 */
  issuerSigningKey?: string | null;
  /** 是否已配置签名密钥。 */
  hasSigningKey?: boolean;
  /** 签发方。 */
  validIssuer?: string | null;
  /** 签收方。 */
  validAudience?: string | null;
  /**
   * 过期时间容错秒数。
   * @format int64
   */
  clockSkew?: number;
  /** 签名算法。 */
  algorithm?: string | null;
  /** 验证签名密钥。 */
  validateIssuerSigningKey?: boolean;
  /** 验证签发方。 */
  validateIssuer?: boolean;
  /** 验证签收方。 */
  validateAudience?: boolean;
  /** 验证生命周期。 */
  validateLifetime?: boolean;
  /**
   * 当前全局令牌版本。
   * @format int64
   */
  tokenVersion?: number;
  /**
   * 最近更新时间。
   * @format int64
   */
  updateTime?: number | null;
}

/** 平台 JWT 配置更新输入。 */
export interface PlatformJwtConfigUpdateRequest {
  /** 签名密钥。传入 ****** 表示保留当前密钥。 */
  issuerSigningKey?: string | null;
  /**
   * 签发方。
   * @minLength 1
   */
  validIssuer: string;
  /**
   * 签收方。
   * @minLength 1
   */
  validAudience: string;
  /**
   * 过期时间容错秒数。
   * @format int64
   * @min 0
   * @max 300
   */
  clockSkew?: number;
}

export interface PlatformJwtConfigDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台 JWT 配置输出。 */
  data: PlatformJwtConfigResponse;
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

export type PlatformJwtConfigUpdatePostData = any;
