/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 机器码增加输入参数 */
export interface AddTenantGameAppDeviceRequest {
  description?: string | null;
  createdName?: string | null;
  updatedName?: string | null;
  /**
   * 应用 Id。机器码归属具体应用，必须 > 0；同一租户下不同应用的机器码彼此独立。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 设备唯一标识
   * @minLength 1
   */
  deviceUniqueIdentifier: string;
}

/** 机器码分页查询输入参数 */
export interface TenantGameAppDeviceRequest {
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
  /** 应用 Id（必填过滤）。设备码为应用级资源，PageAsync 必须按当前应用收口。 */
  appId?: string | null;
  /** 设备分组 Id（可选过滤）：按设备-分组关系过滤属于该分组的设备。 */
  groupId?: string | null;
}

/** 机器码输出参数 */
export interface TenantGameAppDeviceResponse {
  /** Id */
  id?: string;
  /** 应用 ID。机器码归属具体应用，必须 > 0；同一租户下不同应用的机器码彼此独立，禁止 AppId=0 的租户默认机器码。 */
  appId?: string;
  /** 机器码 */
  deviceUniqueIdentifier?: string | null;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
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
}

export interface TenantGameAppDeviceResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAppDeviceResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 机器码更新输入参数 */
export interface UpdateTenantGameAppDeviceRequest {
  name?: string | null;
  description?: string | null;
  createdName?: string | null;
  updatedName?: string | null;
  /** Id */
  id: string;
  /**
   * 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
  /** 应用 Id。不可变（服务端从既有实体回填）；声明于 DTO 仅为契约对称，前端无需传入。 */
  appId?: string;
  /**
   * 设备唯一标识
   * @minLength 1
   */
  deviceUniqueIdentifier: string;
}

export interface TenantGameAppDeviceAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 机器码输出参数 */
  data: TenantGameAppDeviceResponse;
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

export type TenantGameAppDeviceDeletePostData = any;

export interface TenantGameAppDeviceDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 机器码输出参数 */
  data: TenantGameAppDeviceResponse;
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

export interface TenantGameAppDevicePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAppDeviceResponseSqlSugarPagedList;
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

export interface TenantGameAppDeviceSelectPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageResponse[];
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

export interface TenantGameAppDeviceSelectStringPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageStringResponse[];
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

export type TenantGameAppDeviceUpdatePostData = any;
