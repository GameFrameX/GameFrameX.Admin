/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum GameUserStatus {
  /** 新建 */
  New = 0,
  /** 待验证 */
  PendingVerification = 1,
  /** 活跃 */
  Active = 2,
  /** 封禁 */
  Banned = 3,
  /** 暂停 */
  Suspended = 4,
  /** 已删除 */
  Deleted = 5,
  /** 安全风险 */
  Compromised = 6,
  /** 审核中 */
  UnderReview = 7,
  /** 限制 */
  Limited = 8,
}

/** 根据邮箱添加账号 */
export interface AddTenantGameUserEntityByMailRequest {
  /** 账号 */
  userName?: string | null;
  /** 密码 */
  password?: string | null;
  /** 描述 */
  description?: string | null;
  /** 创建人 */
  createdName?: string | null;
  /** 最后更新人 */
  updatedName?: string | null;
  /**
   * 账号
   * @minLength 1
   */
  email: string;
  /**
   * 密码
   * @minLength 1
   */
  emailPassword: string;
  /** 昵称 */
  name?: string | null;
  status: GameUserStatus;
}

/** 游戏账号删除输入参数 */
export interface DeleteTenantGameUserEntityRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 当前应用Id。封禁/解封前校验账号在当前应用下存在角色归属。 */
  appId?: string | null;
}

/** 游戏账号分页查询输入参数 */
export interface PageTenantGameUserEntityRequest {
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
  /** 当前应用Id。游戏账号本身是租户级账号，后台应用工作台通过角色归属派生过滤账号。 */
  appId?: string | null;
  /** Id */
  id?: string | null;
  /** 账号 */
  userName?: string | null;
  status?: GameUserStatus;
  /** 昵称 */
  name?: string | null;
}

/** 游戏账号输出参数 */
export interface TenantGameUserEntityResponse {
  /** Id */
  id?: string;
  /** 账号 */
  userName?: string | null;
  status?: GameUserStatus;
  /** 昵称 */
  name?: string | null;
  /** 描述 */
  description?: string | null;
  /** 创建人 */
  createdName?: string | null;
  /** 最后更新人 */
  updatedName?: string | null;
  /** 创建时间 */
  createTime?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 行版本（乐观锁） */
  rowVersion?: string | null;
}

export interface TenantGameUserEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameUserEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏账号更新输入参数 */
export interface UpdateTenantGameUserEntityRequest {
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
  userName?: string | null;
  password?: string | null;
  status?: GameUserStatus;
  description?: string | null;
}

export interface TenantGameUserEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏账号输出参数 */
  data: TenantGameUserEntityResponse;
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

export interface TenantGameUserEntityAddByMailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏账号输出参数 */
  data: TenantGameUserEntityResponse;
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

export type TenantGameUserEntityBanPostData = any;

export type TenantGameUserEntityDeletePostData = any;

export interface TenantGameUserEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏账号输出参数 */
  data: TenantGameUserEntityResponse;
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

export interface TenantGameUserEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameUserEntityResponseSqlSugarPagedList;
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

export interface TenantGameUserEntitySelectPostData {
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

export interface TenantGameUserEntitySelectStringPostData {
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

export type TenantGameUserEntityUnsealPostData = any;

export type TenantGameUserEntityUpdatePostData = any;
