/* eslint-disable */
/* tslint:disable */

import type { DbType, ResetPwdResponseHttpJsonResultData, TenantTypeEnum, TenantUserResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum TenantMenuSyncStatusEnum {
  /** 待同步 */
  Pending = 1,
  /** 同步成功 */
  Succeeded = 2,
  /** 同步失败 */
  Failed = 3,
}

export interface AddTenantRequest {
  /** Id */
  id?: string;
  /** 用户Id */
  userId?: string;
  /** 机构Id */
  organizationId?: string;
  /** 主机 */
  host?: string | null;
  tenantType?: TenantTypeEnum;
  dbType?: DbType;
  /** 数据库连接 */
  connection?: string | null;
  /** 数据库标识 */
  configId?: string | null;
  /** 套餐等级Id */
  packageId?: string | null;
  /**
   * 到期时间（秒级 Unix 时间戳）
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 过渡期天数（覆盖套餐默认值，null 使用套餐默认）
   * @format int32
   */
  gracePeriodDays?: number | null;
  /** 电子邮箱 */
  email?: string | null;
  /** 电话 */
  phone?: string | null;
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
  menuSyncStatus?: TenantMenuSyncStatusEnum;
  /**
   * 最近一次套餐菜单同步时间（秒级 Unix 时间戳，含失败；状态行缺失为 null）。
   * @format int64
   */
  menuSyncLastSyncTime?: number | null;
  /** 最近一次套餐菜单同步失败原因（成功时为空；状态行缺失为 null）。 */
  menuSyncLastError?: string | null;
  /**
   * 下一次可重试时间（秒级 Unix 时间戳；状态行缺失为 null）。
   * @format int64
   */
  menuSyncNextRetryTime?: number | null;
  /**
   * 租户名称
   * @minLength 2
   */
  name: string;
  /**
   * 租管账号
   * @minLength 3
   */
  adminAccount: string;
}

export interface DeleteTenantRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface PageTenantRequest {
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
  /** 名称 */
  name?: string | null;
  /** 电话 */
  phone?: string | null;
}

export interface TenantIdRequest {
  /** 租户Id */
  tenantId?: string;
}

export interface TenantRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 是否启用 */
  isEnabled?: boolean;
}

/** 系统租户输出参数 */
export interface TenantResponse {
  /** Id */
  id?: string;
  /** 用户Id */
  userId?: string;
  /** 机构Id */
  organizationId?: string;
  /** 主机 */
  host?: string | null;
  tenantType?: TenantTypeEnum;
  dbType?: DbType;
  /** 数据库连接 */
  connection?: string | null;
  /** 数据库标识 */
  configId?: string | null;
  /** 套餐等级Id */
  packageId?: string | null;
  /**
   * 到期时间（秒级 Unix 时间戳）
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 过渡期天数（覆盖套餐默认值，null 使用套餐默认）
   * @format int32
   */
  gracePeriodDays?: number | null;
  /** 租户名称 */
  name?: string | null;
  /** 管理员账号 */
  adminAccount?: string | null;
  /** 电子邮箱 */
  email?: string | null;
  /** 电话 */
  phone?: string | null;
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
  menuSyncStatus?: TenantMenuSyncStatusEnum;
  /**
   * 最近一次套餐菜单同步时间（秒级 Unix 时间戳，含失败；状态行缺失为 null）。
   * @format int64
   */
  menuSyncLastSyncTime?: number | null;
  /** 最近一次套餐菜单同步失败原因（成功时为空；状态行缺失为 null）。 */
  menuSyncLastError?: string | null;
  /**
   * 下一次可重试时间（秒级 Unix 时间戳；状态行缺失为 null）。
   * @format int64
   */
  menuSyncNextRetryTime?: number | null;
}

export interface TenantResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface TenantUserRequest {
  /** 用户Id */
  userId?: string;
}

export interface UpdateTenantRequest {
  /** Id */
  id?: string;
  /** 用户Id */
  userId?: string;
  /** 机构Id */
  organizationId?: string;
  /** 主机 */
  host?: string | null;
  tenantType?: TenantTypeEnum;
  dbType?: DbType;
  /** 数据库连接 */
  connection?: string | null;
  /** 数据库标识 */
  configId?: string | null;
  /** 套餐等级Id */
  packageId?: string | null;
  /**
   * 到期时间（秒级 Unix 时间戳）
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 过渡期天数（覆盖套餐默认值，null 使用套餐默认）
   * @format int32
   */
  gracePeriodDays?: number | null;
  /** 电子邮箱 */
  email?: string | null;
  /** 电话 */
  phone?: string | null;
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
  menuSyncStatus?: TenantMenuSyncStatusEnum;
  /**
   * 最近一次套餐菜单同步时间（秒级 Unix 时间戳，含失败；状态行缺失为 null）。
   * @format int64
   */
  menuSyncLastSyncTime?: number | null;
  /** 最近一次套餐菜单同步失败原因（成功时为空；状态行缺失为 null）。 */
  menuSyncLastError?: string | null;
  /**
   * 下一次可重试时间（秒级 Unix 时间戳；状态行缺失为 null）。
   * @format int64
   */
  menuSyncNextRetryTime?: number | null;
  /**
   * 租户名称
   * @minLength 2
   */
  name: string;
  /**
   * 租管账号
   * @minLength 3
   */
  adminAccount: string;
}

export type SystemTenantAddPostData = any;

export type SystemTenantCreateDbPostData = any;

export type SystemTenantDeletePostData = any;

export interface SystemTenantPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantResponseSqlSugarPagedList;
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

export type SystemTenantResetPwdPostData = ResetPwdResponseHttpJsonResultData;

export interface SystemTenantSetStatusPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** @format int32 */
  data: number;
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

export type SystemTenantUpdatePostData = any;

export interface SystemTenantUserListPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantUserResponse[];
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
