/* eslint-disable */
/* tslint:disable */

import type { DbType, TenantTypeEnum } from "../_shared/data-contracts";

export interface AddOpenAccessRequest {
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
  bindTenantId?: string;
  bindTenant?: SystemTenantEntity;
  /**
   * 身份标识
   * @minLength 1
   */
  accessKey: string;
  /**
   * 密钥
   * @minLength 1
   */
  accessSecret: string;
  /** 绑定用户Id */
  bindUserId: string;
}

export interface DeleteOpenAccessRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/** 开放接口身份输出参数 */
export interface OpenAccessResponse {
  /** Id */
  id?: string;
  /** 身份标识 */
  accessKey?: string | null;
  /** 绑定租户Id */
  bindTenantId?: string;
  /** 绑定用户Id */
  bindUserId?: string;
  /** 绑定用户账号 */
  bindUserAccount?: string | null;
  /** 绑定租户名称 */
  bindTenantName?: string | null;
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

export interface OpenAccessResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: OpenAccessResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 开放接口身份输入参数 */
export interface PageOpenAccessRequest {
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
  /** 身份标识 */
  accessKey?: string | null;
}

/** 开放接口密钥创建结果 */
export interface SecretCreateResponse {
  /**
   * 新生成的开放接口密钥明文（Base64 编码的随机串）。
   *
   * 安全语义：该明文仅在本次响应中一次性下发，用于拷贝到开放接口身份的 AccessSecret 字段；获取后请立即妥善保存，后续无法再次查询。
   */
  secret?: string | null;
}

export interface SecretCreateResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 开放接口密钥创建结果 */
  data?: SecretCreateResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export interface SystemTenantEntity {
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
  userId?: string;
  organizationId?: string;
  host?: string | null;
  tenantType?: TenantTypeEnum;
  dbType?: DbType;
  connection?: string | null;
  configId?: string | null;
  remark?: string | null;
  packageId?: string | null;
  /** @format int64 */
  expireTime?: number | null;
  /** @format int32 */
  gracePeriodDays?: number | null;
}

export interface UpdateOpenAccessRequest {
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
  bindTenantId?: string;
  bindTenant?: SystemTenantEntity;
  /**
   * 身份标识
   * @minLength 1
   */
  accessKey: string;
  /**
   * 密钥
   * @minLength 1
   */
  accessSecret: string;
  /** 绑定用户Id */
  bindUserId: string;
}

export type SystemOpenAccessAddPostData = any;

export type SystemOpenAccessCreateSecretPostData =
  SecretCreateResponseHttpJsonResultData;

export type SystemOpenAccessDeletePostData = any;

export interface SystemOpenAccessPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: OpenAccessResponseSqlSugarPagedList;
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

export type SystemOpenAccessUpdatePostData = any;
