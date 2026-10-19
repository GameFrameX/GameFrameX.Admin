/* eslint-disable */
/* tslint:disable */

import type { GlobalDictionaryDataResponse, GlobalDictionaryTypeEntity } from "../_shared/data-contracts";

export interface AddDictionaryDataRequest {
  /** 主键Id */
  id: string;
  /** 创建人Id */
  createdId?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /**
   * 创建人姓名
   * @maxLength 512
   */
  createdName?: string | null;
  /**
   * 更新次数
   * @format int32
   */
  updateCount?: number | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人Id */
  updatedId?: string | null;
  /**
   * 更新人姓名
   * @maxLength 512
   */
  updatedName?: string | null;
  /** 软删除标记,true:删除,false:未删除,null:未设置(未删除) */
  isDeleted?: boolean | null;
  /**
   * 删除时间
   * @format int64
   */
  deleteTime?: number | null;
  /** 删除人Id */
  deletedId?: string | null;
  /**
   * 删除人姓名
   * @maxLength 512
   */
  deletedName?: string | null;
  /** 行版本号（用于乐观锁） */
  rowVersion?: string | null;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  /** 租户Id */
  tenantId?: string | null;
  dictionaryTypeId?: string;
  dictionaryType?: GlobalDictionaryTypeEntity;
  value?: string | null;
  code?: string | null;
  tagType?: string | null;
  styleSetting?: string | null;
  classSetting?: string | null;
  /** @format int32 */
  orderNo?: number;
  remark?: string | null;
  extData?: string | null;
}

export interface DeleteDictionaryDataRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface DictionaryDataRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 是否启用 */
  isEnabled?: boolean;
}

export interface GlobalDictionaryDataResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GlobalDictionaryDataResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface PageDictionaryDataRequest {
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
  /** 字典类型Id */
  dictionaryTypeId?: string;
  /** 值 */
  value?: string | null;
  /** 编码 */
  code?: string | null;
}

export interface UpdateDictionaryDataRequest {
  /** 主键Id */
  id: string;
  /** 创建人Id */
  createdId?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /**
   * 创建人姓名
   * @maxLength 512
   */
  createdName?: string | null;
  /**
   * 更新次数
   * @format int32
   */
  updateCount?: number | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新人Id */
  updatedId?: string | null;
  /**
   * 更新人姓名
   * @maxLength 512
   */
  updatedName?: string | null;
  /** 软删除标记,true:删除,false:未删除,null:未设置(未删除) */
  isDeleted?: boolean | null;
  /**
   * 删除时间
   * @format int64
   */
  deleteTime?: number | null;
  /** 删除人Id */
  deletedId?: string | null;
  /**
   * 删除人姓名
   * @maxLength 512
   */
  deletedName?: string | null;
  /** 行版本号（用于乐观锁） */
  rowVersion?: string | null;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  /** 租户Id */
  tenantId?: string | null;
  dictionaryTypeId?: string;
  dictionaryType?: GlobalDictionaryTypeEntity;
  value?: string | null;
  code?: string | null;
  tagType?: string | null;
  styleSetting?: string | null;
  classSetting?: string | null;
  /** @format int32 */
  orderNo?: number;
  remark?: string | null;
  extData?: string | null;
}

export type GlobalDictionaryDataAddPostData = any;

export interface GlobalDictionaryDataDataListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GlobalDictionaryDataResponse[];
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

export interface GlobalDictionaryDataDataListGet2Data {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GlobalDictionaryDataResponse[];
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

export type GlobalDictionaryDataDeletePostData = any;

export interface GlobalDictionaryDataDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 系统字典值输出参数 */
  data: GlobalDictionaryDataResponse;
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

export interface GlobalDictionaryDataListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GlobalDictionaryDataResponse[];
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

export interface GlobalDictionaryDataPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GlobalDictionaryDataResponseSqlSugarPagedList;
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

export type GlobalDictionaryDataSetStatusPostData = any;

export type GlobalDictionaryDataUpdatePostData = any;
