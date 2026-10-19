/* eslint-disable */
/* tslint:disable */

/** 游戏支付配置新增输入参数 */
export interface AddGamePaymentConfigRequest {
  /** 游戏应用ID。管理端支付配置必须绑定具体应用，值必须大于 0。 */
  gameAppId: string;
  /** 分包ID。空表示应用默认配置。 */
  packageId?: string | null;
  /**
   * 支付渠道
   * @minLength 1
   */
  payChannel: string;
  /** 是否沙盒配置 */
  isSandbox?: boolean;
  /**
   * 支付配置JSON
   * @minLength 1
   */
  configJson: string;
  /** 备注 */
  remark?: string | null;
}

/** 游戏支付配置输出参数 */
export interface GamePaymentConfigResponse {
  /** Id */
  id?: string;
  /**
   * 租户 Id。wire 功能字段特例保留（非审计泄漏）：前端 buildNotifyUrl 以其为路径段拼接支付回调地址
   * （C65），SuperAdmin 视角每行租户不同，无法在客户端侧补全。
   */
  tenantId?: string;
  /** 应用ID。0表示租户默认配置。 */
  appId?: string;
  /** 游戏应用ID。空表示租户默认配置。 */
  gameAppId?: string | null;
  /** 分包ID。空表示应用默认配置。 */
  packageId?: string | null;
  /**
   * 游戏应用ID归一化值，空按0参与唯一性判断。
   * @format int64
   */
  gameAppIdNormalized?: number;
  /**
   * 分包ID归一化值，空按0参与唯一性判断。
   * @format int64
   */
  packageIdNormalized?: number;
  /** 支付渠道 */
  payChannel?: string | null;
  /** 是否沙盒配置 */
  isSandbox?: boolean;
  /** 支付配置JSON */
  configJson?: string | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 创建人 */
  createdName?: string | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人 */
  updatedName?: string | null;
  /** 行版本号（乐观锁） */
  rowVersion?: string | null;
  /** 备注 */
  remark?: string | null;
}

export interface GamePaymentConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GamePaymentConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏支付配置分页查询输入参数 */
export interface PageGamePaymentConfigRequest {
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
  appId?: string | null;
  isActive?: boolean | null;
  /** 游戏应用ID。支付配置为应用工作台页面，分页查询必须提供具体应用，值必须大于 0。 */
  gameAppId?: string | null;
  /** 分包ID */
  packageId?: string | null;
  /** 支付渠道 */
  payChannel?: string | null;
  /** 是否沙盒配置 */
  isSandbox?: boolean | null;
}

export interface PaymentConfigChannel {
  value?: string | null;
  label?: string | null;
}

export interface PaymentConfigField {
  key?: string | null;
  label?: string | null;
  type?: string | null;
  required?: boolean;
  help?: string | null;
  default?: any | null;
  options?: PaymentConfigOption[] | null;
}

export interface PaymentConfigOption {
  value?: string | null;
  label?: string | null;
}

export interface PaymentConfigSchema {
  channel?: string | null;
  label?: string | null;
  fields?: PaymentConfigField[] | null;
}

/** 游戏支付配置更新输入参数 */
export interface UpdateGamePaymentConfigRequest {
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
  /** 游戏应用ID。管理端支付配置必须绑定具体应用，值必须大于 0。 */
  gameAppId: string;
  /** 分包ID。空表示应用默认配置。 */
  packageId?: string | null;
  /**
   * 支付渠道
   * @minLength 1
   */
  payChannel: string;
  /** 是否沙盒配置 */
  isSandbox?: boolean;
  /**
   * 支付配置JSON。传 ****** 时保留原配置。
   * @minLength 1
   */
  configJson: string;
  /** 备注 */
  remark?: string | null;
}

export interface TenantGamePaymentConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏支付配置输出参数 */
  data: GamePaymentConfigResponse;
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

export interface TenantGamePaymentConfigEntityChannelsGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: PaymentConfigChannel[];
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

export type TenantGamePaymentConfigEntityDeletePostData = any;

export interface TenantGamePaymentConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏支付配置输出参数 */
  data: GamePaymentConfigResponse;
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

export interface TenantGamePaymentConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GamePaymentConfigResponseSqlSugarPagedList;
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

export interface TenantGamePaymentConfigEntitySchemaGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: PaymentConfigSchema;
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

export type TenantGamePaymentConfigEntityUpdatePostData = any;
