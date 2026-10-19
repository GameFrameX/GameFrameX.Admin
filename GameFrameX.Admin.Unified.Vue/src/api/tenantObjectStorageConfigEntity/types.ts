/* eslint-disable */
/* tslint:disable */

/** 新增租户对象存储配置入参。 */
export interface AddTenantObjectStorageConfigRequest {
  /** 存储提供商名称（local / aliyun / tencent-cos / qiniu）。 */
  provider?: string | null;
  /** 对象键路径模板。 */
  pathTemplate?: string | null;
  /**
   * 单文件最大字节数。
   * @format int64
   */
  maxFileSizeBytes?: number;
  /** 允许的内容类型 JSON 数组。 */
  allowedContentTypesJson?: string | null;
  /** 是否启用 MD5 去重。 */
  enableMd5?: boolean;
  /** 提供商配置 JSON，敏感字段可用 ****** 表示沿用旧值（更新场景）。 */
  configJson?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface ObjectStorageConfigField {
  key?: string | null;
  label?: string | null;
  type?: string | null;
  required?: boolean;
  help?: string | null;
  default?: any | null;
  options?: string[] | null;
  sensitive?: boolean;
}

export interface ObjectStorageConfigSchema {
  provider?: string | null;
  label?: string | null;
  fields?: ObjectStorageConfigField[] | null;
}

export interface ObjectStorageProviderItem {
  value?: string | null;
  label?: string | null;
}

/** 租户对象存储配置分页查询入参。 */
export interface PageTenantObjectStorageConfigRequest {
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
  /** 按提供商过滤。 */
  provider?: string | null;
  /** 按启用状态过滤。 */
  isEnabled?: boolean | null;
}

/** 租户对象存储配置列表项。 */
export interface TenantObjectStorageConfigResponse {
  id?: string;
  /** 存储提供商名称。 */
  provider?: string | null;
  /** 对象键路径模板。 */
  pathTemplate?: string | null;
  /**
   * 单文件最大字节数。
   * @format int64
   */
  maxFileSizeBytes?: number;
  /** 允许的内容类型 JSON 数组。 */
  allowedContentTypesJson?: string | null;
  /** 是否启用 MD5 去重。 */
  enableMd5?: boolean;
  /** 提供商配置 JSON（列表场景整体脱敏为 ******）。 */
  configJson?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
  /**
   * 创建时间。
   * @format int64
   */
  createdTime?: number;
  /** 行版本（乐观锁） */
  rowVersion?: string | null;
}

export interface TenantObjectStorageConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantObjectStorageConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 更新租户对象存储配置入参。Provider 必须与既有记录一致。 */
export interface UpdateTenantObjectStorageConfigRequest {
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
  /** 存储提供商名称（必须等于既有记录的 Provider）。 */
  provider?: string | null;
  /** 对象键路径模板。 */
  pathTemplate?: string | null;
  /**
   * 单文件最大字节数。
   * @format int64
   */
  maxFileSizeBytes?: number;
  /** 允许的内容类型 JSON 数组。 */
  allowedContentTypesJson?: string | null;
  /** 是否启用 MD5 去重。 */
  enableMd5?: boolean;
  /** 提供商配置 JSON，敏感字段为 ****** 时沿用既有值。 */
  configJson?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface TenantObjectStorageConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户对象存储配置列表项。 */
  data: TenantObjectStorageConfigResponse;
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

export type TenantObjectStorageConfigEntityDeletePostData = any;

export interface TenantObjectStorageConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户对象存储配置列表项。 */
  data: TenantObjectStorageConfigResponse;
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

export interface TenantObjectStorageConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantObjectStorageConfigResponseSqlSugarPagedList;
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

export interface TenantObjectStorageConfigEntityProvidersGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: ObjectStorageProviderItem[];
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

export interface TenantObjectStorageConfigEntitySchemaGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: ObjectStorageConfigSchema;
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

export interface TenantObjectStorageConfigEntityTestConnectionGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: boolean;
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

export type TenantObjectStorageConfigEntityUpdatePostData = any;
