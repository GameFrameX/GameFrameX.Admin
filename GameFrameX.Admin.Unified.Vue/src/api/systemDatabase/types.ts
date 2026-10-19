/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum DbObjectType {
  /** Table */
  Table = 0,
  /** View */
  View = 1,
  /** All */
  All = 2,
}

export interface DbColumnRequest {
  configId?: string | null;
  tableName?: string | null;
  dbColumnName?: string | null;
  dataType?: string | null;
  /** @format int32 */
  length?: number;
  columnDescription?: string | null;
  /** @format int32 */
  isNullable?: number;
  /** @format int32 */
  isIdentity?: number;
  /** @format int32 */
  isPrimarykey?: number;
  /** @format int32 */
  decimalDigits?: number;
}

export interface DbColumnResponse {
  tableName?: string | null;
  /** @format int32 */
  tableId?: number;
  dbColumnName?: string | null;
  propertyName?: string | null;
  dataType?: string | null;
  propertyType?: any | null;
  /** @format int32 */
  length?: number;
  columnDescription?: string | null;
  defaultValue?: string | null;
  isNullable?: boolean;
  isIdentity?: boolean;
  isPrimarykey?: boolean;
  value?: any | null;
  /** @format int32 */
  decimalDigits?: number;
  /** @format int32 */
  scale?: number;
  isArray?: boolean;
  isJson?: boolean;
  isUnsigned?: boolean | null;
  /** @format int32 */
  createTableFieldSort?: number;
}

export interface DbTableInfo {
  name?: string | null;
  description?: string | null;
  dbObjectType?: DbObjectType;
}

export interface DbTableRequest {
  configId?: string | null;
  tableName?: string | null;
  description?: string | null;
  dbColumnInfoList?: DbColumnRequest[] | null;
}

export interface DeleteDbColumnRequest {
  configId?: string | null;
  tableName?: string | null;
  dbColumnName?: string | null;
}

export interface DeleteDbTableRequest {
  configId?: string | null;
  tableName?: string | null;
}

export interface UpdateDbColumnRequest {
  configId?: string | null;
  tableName?: string | null;
  columnName?: string | null;
  oldColumnName?: string | null;
  description?: string | null;
}

export interface UpdateDbTableRequest {
  configId?: string | null;
  tableName?: string | null;
  oldTableName?: string | null;
  description?: string | null;
}

export type SystemDatabaseAddColumnPostData = any;

export type SystemDatabaseAddTablePostData = any;

export interface SystemDatabaseColumnListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: DbColumnResponse[];
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

export interface SystemDatabaseDbTypeListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: string[];
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

export type SystemDatabaseDeleteColumnPostData = any;

export type SystemDatabaseDeleteTablePostData = any;

export interface SystemDatabaseListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: string[];
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

export interface SystemDatabaseTableListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: DbTableInfo[];
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

export type SystemDatabaseUpdateColumnPostData = any;

export type SystemDatabaseUpdateTablePostData = any;
