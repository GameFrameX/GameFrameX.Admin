/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 游戏角色增加输入参数 */
export interface AddTenantGameRoleEntityRequest {
  /**
   * 应用Id。事实数据应用作用域，必须 > 0。
   *
   * 过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。
   */
  appId: string;
  /** 角色Id */
  roleId?: string | null;
  /** 用户ID */
  userId?: string;
  /** 区服Id */
  areaId?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /**
   * 角色当前的等级
   * @format int64
   */
  level?: number | null;
  /**
   * 角色最后一次登录时间
   * @format int64
   */
  lastLoginTime?: number | null;
}

/** 游戏角色分页查询输入参数 */
export interface PageTenantGameRoleEntityRequest {
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
  areaId?: string | null;
  /** 用户ID */
  userId?: string | null;
  /** 角色ID */
  id?: string | null;
  /** 是否是沙盒账户 */
  isSandbox?: boolean | null;
  /** 名称 */
  name?: string | null;
  /**
   * 角色当前的等级
   * @format int64
   */
  level?: number | null;
  /** 注册时间范围 */
  registerTime?: number[] | null;
  /** 最后登录时间范围 */
  logoutTime?: number[] | null;
  /**
   * 累充最小值(分)
   * @format int32
   */
  rechargeMin?: number | null;
  /**
   * 累充最大值(分)
   * @format int32
   */
  rechargeMax?: number | null;
  /** 应用Id。应用级角色分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
}

/** 游戏角色输出参数 */
export interface TenantGameRoleEntityResponse {
  /** 角色Id */
  roleId?: string | null;
  /** 主键Id */
  id?: string;
  /** 应用Id */
  appId?: string;
  /** 用户ID */
  userId?: string;
  /** 区服Id */
  areaId?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /**
   * 角色当前的等级
   * @format int64
   */
  level?: number | null;
  /** 是否是沙盒账户 */
  isSandbox?: boolean | null;
  /**
   * 角色最后一次登录时间
   * @format int64
   */
  lastLoginTime?: number | null;
  /**
   * 角色注册时间
   * @format int64
   */
  createTime?: number | null;
  /**
   * 在先时长
   * @format int64
   */
  onlineTime?: number;
  /**
   * 当前关卡
   * @format int32
   */
  currentLevelConfigId?: number;
  /**
   * 累充
   * @format int32
   */
  allRecharge?: number;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 行版本（乐观锁） */
  rowVersion?: string | null;
}

export interface TenantGameRoleEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameRoleEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏角色更新输入参数 */
export interface UpdateTenantGameRoleEntityRequest {
  /**
   * 应用Id。事实数据应用作用域，必须 > 0。
   *
   * 过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。
   */
  appId: string;
  /** 用户ID */
  userId?: string;
  /** 区服Id */
  areaId?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /**
   * 角色当前的等级
   * @format int64
   */
  level?: number | null;
  /**
   * 角色最后一次登录时间
   * @format int64
   */
  lastLoginTime?: number | null;
  /** 主键Id */
  id: string;
  /**
   * 客户端读取记录时的版本号。
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
  /** 角色Id */
  roleId?: string | null;
}

export interface TenantGameRoleEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏角色输出参数 */
  data: TenantGameRoleEntityResponse;
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

export type TenantGameRoleEntityDeletePostData = any;

export interface TenantGameRoleEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏角色输出参数 */
  data: TenantGameRoleEntityResponse;
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

export interface TenantGameRoleEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameRoleEntityResponseSqlSugarPagedList;
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

export interface TenantGameRoleEntitySelectPostData {
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

export interface TenantGameRoleEntitySelectStringPostData {
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

export type TenantGameRoleEntitySyncPostData = any;

export type TenantGameRoleEntityUpdatePostData = any;

export type TenantGameRoleEntityUpdateToSandBoxPostData = any;

export type TenantGameRoleEntityUpdateToUnSandBoxPostData = any;
