/* eslint-disable */
/* tslint:disable */

import type { AccountTypeEnum } from "../_shared/data-contracts";

/** 图形验证码结果 */
export interface CaptchaResponse {
  /** 验证码唯一标识（登录提交时与验证码输入值配对校验） */
  id?: string | null;
  /** 验证码图片 Base64 内容 */
  img?: string | null;
}

export interface CaptchaResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 图形验证码结果 */
  data?: CaptchaResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 登录配置结果 */
export interface LoginConfigResponse {
  /** 是否启用二次验证 */
  secondVerEnabled?: boolean;
  /** 是否启用图形验证码 */
  captchaEnabled?: boolean;
  /** 是否启用邮箱自助注册 */
  emailRegisterEnabled?: boolean;
  /** 平台超管密码是否已完成初始化（false = 登录页需展示首装密码初始化向导） */
  superAdminPasswordConfigured?: boolean;
}

export interface LoginConfigResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 登录配置结果 */
  data?: LoginConfigResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 用户登录参数 */
export interface LoginRequest {
  /**
   * 账号
   * @minLength 2
   * @example "admin"
   */
  account: string;
  /**
   * 密码
   * @minLength 6
   * @example "123456"
   */
  password: string;
  /** 验证码Id */
  codeId?: string;
  /** 验证码 */
  code?: string | null;
  /** 租户Id。普通租户登录时使用，平台 SuperAdmin 可不传。 */
  tenantId?: string | null;
  /** 租户主机名。普通租户登录时可用 Host 解析租户。 */
  tenantHost?: string | null;
}

/** 用户登录结果 */
export interface LoginResponse {
  /** 令牌Token */
  accessToken?: string | null;
  /** 刷新Token */
  refreshToken?: string | null;
}

/** 用户登录信息 */
export interface LoginUserResponse {
  /** 用户id */
  id?: string;
  /** 账号名称 */
  account?: string | null;
  /** 真实姓名 */
  realName?: string | null;
  accountType?: AccountTypeEnum;
  /** 头像 */
  avatar?: string | null;
  /** 个人简介 */
  introduction?: string | null;
  /** 地址 */
  address?: string | null;
  /** 电子签名 */
  signature?: string | null;
  /** 机构Id */
  organizationId?: string;
  /** 机构名称 */
  organizationName?: string | null;
  /** 机构类型 */
  organizationType?: string | null;
  /** 职位名称 */
  posName?: string | null;
  /** 按钮权限集合 */
  buttons?: string[] | null;
}

/** 获取刷新Token结果 */
export interface RefreshTokenResponse {
  /** 刷新Token（一次性明文返回，仅本次响应可见，禁止落日志/缓存） */
  refreshToken?: string | null;
}

export interface RefreshTokenResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 获取刷新Token结果 */
  data?: RefreshTokenResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 邮箱自助注册参数 */
export interface RegisterByEmailRequest {
  /**
   * 注册邮箱（同时作为租管账号）
   * @format email
   * @minLength 1
   * @example "admin@example.com"
   */
  email: string;
  /**
   * 邮箱验证码
   * @minLength 6
   * @example "123456"
   */
  code: string;
  /**
   * 登录密码
   * @minLength 6
   * @example "123456"
   */
  password: string;
  /**
   * 租户名称
   * @minLength 2
   * @example "我的租户"
   */
  tenantName: string;
}

/** 发送邮箱自助注册验证码参数 */
export interface SendRegisterEmailCodeRequest {
  /**
   * 注册邮箱（同时作为租管账号）
   * @format email
   * @minLength 1
   * @example "admin@example.com"
   */
  email: string;
}

/** 平台超管首装密码初始化参数 */
export interface SetupSuperAdminPasswordRequest {
  /**
   * 登录密码
   * @minLength 6
   * @example "Abc12345"
   */
  password: string;
  /**
   * 确认密码
   * @minLength 6
   * @example "Abc12345"
   */
  confirmPassword: string;
}

/** 水印配置结果 */
export interface WatermarkConfigResponse {
  /** 是否启用页面水印 */
  watermarkEnabled?: boolean;
}

export interface WatermarkConfigResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 水印配置结果 */
  data?: WatermarkConfigResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export type SystemAuthCaptchaGetData = CaptchaResponseHttpJsonResultData;

export interface SystemAuthLoginPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 用户登录结果 */
  data: LoginResponse;
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

export type SystemAuthLoginConfigGetData =
  LoginConfigResponseHttpJsonResultData;

export type SystemAuthLogoutPostData = any;

export type SystemAuthRefreshTokenGetData =
  RefreshTokenResponseHttpJsonResultData;

export interface SystemAuthRegisterByEmailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 用户登录结果 */
  data: LoginResponse;
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

export type SystemAuthSendRegisterEmailCodePostData = any;

export interface SystemAuthSetupSuperAdminPasswordPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 用户登录结果 */
  data: LoginResponse;
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

export interface SystemAuthUserInfoGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 用户登录信息 */
  data: LoginUserResponse;
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

export type SystemAuthWatermarkConfigGetData =
  WatermarkConfigResponseHttpJsonResultData;

/** @format int32 */
export type SwaggerCheckUrlPostData = number;

export interface SwaggerSubmitUrlPostPayload {
  userName?: string;
  password?: string;
}

/** @format int32 */
export type SwaggerSubmitUrlPostData = number;
