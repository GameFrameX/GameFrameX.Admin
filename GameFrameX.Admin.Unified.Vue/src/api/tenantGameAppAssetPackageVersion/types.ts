/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse } from "../_shared/data-contracts";

/** 资源包版本增加输入参数 */
export interface AddGameAppAssetPackageVersionRequest {
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** 主键Id */
  id: string;
  /**
   * 渠道名称
   * @minLength 1
   */
  channel: string;
  /**
   * 平台
   * @minLength 1
   */
  platform: string;
  /**
   * 游戏版本
   * @minLength 1
   */
  appVersion: string;
  /**
   * 包名
   * @minLength 1
   */
  package: string;
  /**
   * 语言
   * @minLength 1
   */
  language: string;
  /**
   * 资源包名称
   * @minLength 1
   */
  assetPackageName: string;
  /**
   * 资源下载根路径
   * @minLength 1
   */
  rootPath: string;
  /**
   * 资源版本
   * @minLength 1
   */
  resourceVersion: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 机器码不能为空 */
  deviceGroupId: string;
}

/** 资源包版本输出参数 */
export interface GameAppAssetPackageVersionResponse {
  /** Id */
  id?: string;
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** 渠道 */
  channel?: string | null;
  /** 平台 */
  platform?: string | null;
  /** 游戏版本 */
  appVersion?: string | null;
  /** 包名 */
  package?: string | null;
  /** 语言 */
  language?: string | null;
  /** 资源包名称 */
  assetPackageName?: string | null;
  /** 资源下载根路径 */
  rootPath?: string | null;
  /** 资源版本 */
  resourceVersion?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 机器码分组id */
  deviceGroupId?: string | null;
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

export interface GameAppAssetPackageVersionResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppAssetPackageVersionResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 资源包版本更新输入参数 */
export interface GameAppAssetPackageVersionReturnByIdRequest {
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
  /** 记录id */
  resourceVersionHistoryId: string;
}

/** 资源包版本分页查询输入参数 */
export interface PageGameAppAssetPackageVersionRequest {
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
  channel?: string | null;
  platform?: string | null;
  appVersion?: string | null;
  package?: string | null;
  language?: string | null;
  /** 应用作用域筛选；null 不筛，0 表示租户默认 */
  appId?: string | null;
  /** 资源下载根路径 */
  rootPath?: string | null;
  /** 资源包名称 */
  assetPackageName?: string | null;
}

/** 资源包版本选择器输入参数 */
export interface PageGameAppAssetPackageVersionSelectRequest {
  searchKey?: string | null;
  appId?: string | null;
  /**
   * @format int32
   * @min 1
   * @max 1000
   */
  pageSize?: number;
  /** 分包Id */
  appPackageId?: string | null;
}

/** 资源包版本更新输入参数 */
export interface UpdateGameAppAssetPackageVersionRequest {
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
  /**
   * 资源版本
   * @minLength 1
   */
  resourceVersion: string;
  /**
   * 资源下载根路径
   * @minLength 1
   */
  rootPath: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 机器码分组id */
  deviceGroupId: string;
}

export interface TenantGameAppAssetPackageVersionAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 资源包版本输出参数 */
  data: GameAppAssetPackageVersionResponse;
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

export type TenantGameAppAssetPackageVersionDeletePostData = any;

export interface TenantGameAppAssetPackageVersionDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 资源包版本输出参数 */
  data: GameAppAssetPackageVersionResponse;
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

export interface TenantGameAppAssetPackageVersionPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppAssetPackageVersionResponseSqlSugarPagedList;
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

export type TenantGameAppAssetPackageVersionReturnPostData = any;

export interface TenantGameAppAssetPackageVersionSelectPostData {
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

export type TenantGameAppAssetPackageVersionUpdatePostData = any;
