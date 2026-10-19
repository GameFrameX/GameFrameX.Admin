/* eslint-disable */
/* tslint:disable */

/** 租户广告回调配置新增输入参数 */
export interface AddTenantAdvertisementCallbackConfigRequest {
  /** 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * 广告平台 Provider。当前首批配置支持 `TopOn`、`GoogleAdMob`、`Pangle`、`UnityAds`。
   * @minLength 1
   */
  provider: string;
  /** 广告平台后台配置的回调 URL。 */
  callbackUrl?: string | null;
  /** 签名密钥或共享密钥。TopOn 对应 sec_key，Pangle 对应 security key，Unity Ads 对应 secret。 */
  signingSecret?: string | null;
  /** 验签公钥或验证材料。Google AdMob 可记录 key_id=base64key。 */
  verificationKeys?: string | null;
  /**
   * 广告回调动态配置 JSON。字段由 Provider schema 决定。
   * @minLength 1
   */
  configJson: string;
  /** 参数作用说明 JSON。 */
  parameterPurposeJson?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface AdvertisementCallbackConfigField {
  key?: string | null;
  label?: string | null;
  type?: string | null;
  required?: boolean;
  help?: string | null;
  default?: any | null;
  sensitive?: boolean;
  options?: AdvertisementCallbackConfigOption[] | null;
}

export interface AdvertisementCallbackConfigOption {
  value?: string | null;
  label?: string | null;
}

export interface AdvertisementCallbackConfigSchema {
  provider?: string | null;
  label?: string | null;
  fields?: AdvertisementCallbackConfigField[] | null;
}

export interface AdvertisementCallbackProviderItem {
  value?: string | null;
  label?: string | null;
}

/** 租户广告回调验签配置分页查询输入参数 */
export interface PageTenantAdvertisementCallbackConfigRequest {
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
  /** 广告平台 Provider。 */
  provider?: string | null;
}

/** 租户广告回调配置输出参数（敏感字段已脱敏） */
export interface TenantAdvertisementCallbackConfigResponse {
  /** Id */
  id?: string;
  /** 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /** 广告平台 Provider。当前首批配置支持 TopOn、GoogleAdMob、Pangle、UnityAds。 */
  provider?: string | null;
  /** 广告平台后台配置的回调 URL。 */
  callbackUrl?: string | null;
  /** 签名密钥或共享密钥（敏感字段，API 输出脱敏，日志禁止明文）。 */
  signingSecret?: string | null;
  /** 验签公钥或验证材料（敏感字段，API 输出脱敏，日志禁止明文）。 */
  verificationKeys?: string | null;
  /** 广告回调动态配置 JSON。字段由 Provider schema 决定。 */
  configJson?: string | null;
  /** 参数作用说明 JSON。记录该 Provider 的核心参数如何映射到用户、交易、奖励、签名、审计等用途。 */
  parameterPurposeJson?: string | null;
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

export interface TenantAdvertisementCallbackConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantAdvertisementCallbackConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 租户广告回调配置更新输入参数。
 *
 * 敏感字段提交 `******` 时表示保留原值。
 */
export interface UpdateTenantAdvertisementCallbackConfigRequest {
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
  /** 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * 广告平台 Provider。当前首批配置支持 `TopOn`、`GoogleAdMob`、`Pangle`、`UnityAds`。
   * @minLength 1
   */
  provider: string;
  /** 广告平台后台配置的回调 URL。 */
  callbackUrl?: string | null;
  /** 签名密钥或共享密钥。传 `******` 时保留原值。 */
  signingSecret?: string | null;
  /** 验签公钥或验证材料。传 `******` 时保留原值。 */
  verificationKeys?: string | null;
  /**
   * 广告回调动态配置 JSON。字段由 Provider schema 决定；敏感字段传 `******` 时保留原值。
   * @minLength 1
   */
  configJson: string;
  /** 参数作用说明 JSON。 */
  parameterPurposeJson?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface TenantAdvertisementCallbackConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户广告回调配置输出参数（敏感字段已脱敏） */
  data: TenantAdvertisementCallbackConfigResponse;
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

export type TenantAdvertisementCallbackConfigEntityDeletePostData = any;

export interface TenantAdvertisementCallbackConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户广告回调配置输出参数（敏感字段已脱敏） */
  data: TenantAdvertisementCallbackConfigResponse;
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

export interface TenantAdvertisementCallbackConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantAdvertisementCallbackConfigResponseSqlSugarPagedList;
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

export interface TenantAdvertisementCallbackConfigEntityProvidersGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: AdvertisementCallbackProviderItem[];
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

export interface TenantAdvertisementCallbackConfigEntitySchemaGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: AdvertisementCallbackConfigSchema;
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

export type TenantAdvertisementCallbackConfigEntityUpdatePostData = any;
