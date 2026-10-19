/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 游戏大区分类增加输入参数 */
export interface AddTenantGameAreaClassifyEntityRequest {
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 本地化Key */
  localizationKey?: string | null;
  /** 大区服务器 HTTP管理器URL */
  httpManagerUrl?: string | null;
  /** 镜像仓库地址 */
  dockerRegister?: string | null;
  /** 仓库用户名 */
  dockerUserName?: string | null;
  /** 仓库密码 */
  dockerPassword?: string | null;
  /** 容器网络名 */
  containerNetName?: string | null;
  /** 容器网络子网 */
  containerSubnet?: string | null;
  /** 容器网络网关 */
  containerGateway?: string | null;
  /** 镜像源 */
  imageName?: string | null;
}

/** 游戏大区分类分页查询输入参数 */
export interface PageTenantGameAreaClassifyEntityRequest {
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
  /** 应用Id */
  appId?: string | null;
  /** 名称 */
  name?: string | null;
}

/** 游戏大区分类输出参数 */
export interface TenantGameAreaClassifyEntityResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 应用Id。运营内容应用作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 本地化Key */
  localizationKey?: string | null;
  /** 大区服务器 HTTP管理器URL */
  httpManagerUrl?: string | null;
  /** 镜像仓库地址 */
  dockerRegister?: string | null;
  /** 镜像名称 */
  imageName?: string | null;
  /** 仓库用户名 */
  dockerUserName?: string | null;
  /** 容器网络名 */
  containerNetName?: string | null;
  /** 容器网络子网 */
  containerSubnet?: string | null;
  /** 容器网络网关 */
  containerGateway?: string | null;
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

export interface TenantGameAreaClassifyEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAreaClassifyEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏大区分类更新输入参数 */
export interface UpdateTenantGameAreaClassifyEntityRequest {
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
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /** 本地化Key */
  localizationKey?: string | null;
  /** 大区服务器 HTTP管理器URL */
  httpManagerUrl?: string | null;
  /** 镜像仓库地址 */
  dockerRegister?: string | null;
  /** 仓库用户名 */
  dockerUserName?: string | null;
  /** 仓库密码 */
  dockerPassword?: string | null;
  /** 容器网络名 */
  containerNetName?: string | null;
  /** 容器网络子网 */
  containerSubnet?: string | null;
  /** 容器网络网关 */
  containerGateway?: string | null;
  /** 镜像源 */
  imageName?: string | null;
}

export interface TenantGameAreaClassifyEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏大区分类输出参数 */
  data: TenantGameAreaClassifyEntityResponse;
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

export type TenantGameAreaClassifyEntityDeletePostData = any;

export interface TenantGameAreaClassifyEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏大区分类输出参数 */
  data: TenantGameAreaClassifyEntityResponse;
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

export interface TenantGameAreaClassifyEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAreaClassifyEntityResponseSqlSugarPagedList;
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

export interface TenantGameAreaClassifyEntitySelectPostData {
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

export interface TenantGameAreaClassifyEntitySelectStringPostData {
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

export type TenantGameAreaClassifyEntityUpdatePostData = any;
