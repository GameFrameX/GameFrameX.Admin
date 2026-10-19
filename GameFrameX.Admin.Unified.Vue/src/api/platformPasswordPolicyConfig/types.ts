/* eslint-disable */
/* tslint:disable */

/** 平台密码策略配置输出（C173）。仅暴露密码强度策略字段，绝不回显 Cryptogram 密钥（CryptoType/PublicKey/PrivateKey）。 */
export interface PlatformPasswordPolicyConfigResponse {
  /** 是否开启密码强度验证。 */
  strongPassword?: boolean;
  /** 密码强度验证正则表达式。 */
  passwordStrengthValidation?: string | null;
  /** 密码强度验证失败提示。 */
  passwordStrengthValidationMsg?: string | null;
  /**
   * 最近更新时间（Unix 秒）。
   * @format int64
   */
  updateTime?: number | null;
}

/** 平台密码策略配置更新输入（SuperAdmin-only）。 */
export interface PlatformPasswordPolicyConfigUpdateRequest {
  /** 是否开启密码强度验证。 */
  strongPassword?: boolean;
  /**
   * 密码强度验证正则表达式（必须可编译）。
   * @min 1
   * @max 1024
   */
  passwordStrengthValidation?: string | null;
  /**
   * 密码强度验证失败提示。
   * @min 1
   * @max 1024
   */
  passwordStrengthValidationMsg?: string | null;
}

export interface PlatformPasswordPolicyConfigDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台密码策略配置输出（C173）。仅暴露密码强度策略字段，绝不回显 Cryptogram 密钥（CryptoType/PublicKey/PrivateKey）。 */
  data: PlatformPasswordPolicyConfigResponse;
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

export type PlatformPasswordPolicyConfigUpdatePostData = any;
