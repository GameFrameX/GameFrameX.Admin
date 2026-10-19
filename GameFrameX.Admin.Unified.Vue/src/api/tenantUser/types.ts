/* eslint-disable */
/* tslint:disable */

import type { AccountTypeEnum, CardTypeEnum, CultureLevelEnum, GenderEnum, ResetPwdResponseHttpJsonResultData, TenantOrganizationEntity, TenantPositionEntity, TenantUserEntity, TenantUserResponse } from "../_shared/data-contracts";

/**
 * 新增/编辑用户输入参数。
 * 显式声明前端表单可填字段，不再继承 GameFrameX.Entity.System.SystemUserBase，避免整表字段（含登录审计、导航属性、电子签名等）暴露给前端批量赋值。
 */
export interface AddUserRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /**
   * 账号
   * @minLength 1
   */
  account: string;
  /**
   * 真实姓名
   * @minLength 1
   */
  realName: string;
  /** 昵称 */
  nickName?: string | null;
  /** 头像 */
  avatar?: string | null;
  sex?: GenderEnum;
  /**
   * 年龄
   * @format int32
   */
  age?: number;
  /**
   * 出生日期
   * @format int64
   */
  birthdayTime?: number | null;
  /** 民族 */
  nation?: string | null;
  /** 手机号码 */
  phone?: string | null;
  cardType?: CardTypeEnum;
  /** 身份证号 */
  idCardNum?: string | null;
  /** 邮箱 */
  email?: string | null;
  /** 地址 */
  address?: string | null;
  cultureLevel?: CultureLevelEnum;
  /** 政治面貌 */
  politicalOutlook?: string | null;
  /** 毕业院校 */
  college?: string | null;
  /** 办公电话 */
  officePhone?: string | null;
  /** 紧急联系人 */
  emergencyContact?: string | null;
  /** 紧急联系人电话 */
  emergencyPhone?: string | null;
  /** 紧急联系人地址 */
  emergencyAddress?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  accountType?: AccountTypeEnum;
  /** 直属机构Id */
  organizationId?: string;
  /** 职位Id */
  posId?: string;
  /** 工号 */
  jobNum?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
  /** 角色集合 */
  roleIdList?: number[] | null;
  /** 扩展机构集合 */
  extOrganizationIdList?: TenantUserExtensionOrganizationEntity[] | null;
}

export interface ChangePwdRequest {
  /**
   * 当前密码
   * @minLength 1
   */
  passwordOld: string;
  /**
   * 新密码
   * @minLength 5
   * @maxLength 256
   */
  passwordNew: string;
}

export interface DeleteUserRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 机构Id */
  organizationId?: string;
}

export interface PageUserRequest {
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
  /** 账号 */
  account?: string | null;
  /** 姓名 */
  realName?: string | null;
  /** 手机号 */
  phone?: string | null;
  /** 查询时所选机构Id */
  organizationId?: string;
}

export interface ResetPwdUserRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface TenantUserExtensionOrganizationEntity {
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
  userId?: string;
  user?: TenantUserEntity;
  organizationId?: string;
  tenantOrganizationEntity?: TenantOrganizationEntity;
  posId?: string;
  tenantPositionEntity?: TenantPositionEntity;
  jobNum?: string | null;
  posLevel?: string | null;
  /** @format int64 */
  joinTime?: number | null;
}

/** 系统用户扩展机构输出参数 */
export interface TenantUserExtensionOrganizationResponse {
  /** Id */
  id?: string;
  /** 用户Id */
  userId?: string;
  /** 机构Id */
  organizationId?: string;
  /** 职位Id */
  posId?: string;
  /** 工号 */
  jobNum?: string | null;
  /** 职级 */
  posLevel?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
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

export interface TenantUserResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantUserResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface UpdateUserRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /**
   * 账号
   * @minLength 1
   */
  account: string;
  /**
   * 真实姓名
   * @minLength 1
   */
  realName: string;
  /** 昵称 */
  nickName?: string | null;
  /** 头像 */
  avatar?: string | null;
  sex?: GenderEnum;
  /**
   * 年龄
   * @format int32
   */
  age?: number;
  /**
   * 出生日期
   * @format int64
   */
  birthdayTime?: number | null;
  /** 民族 */
  nation?: string | null;
  /** 手机号码 */
  phone?: string | null;
  cardType?: CardTypeEnum;
  /** 身份证号 */
  idCardNum?: string | null;
  /** 邮箱 */
  email?: string | null;
  /** 地址 */
  address?: string | null;
  cultureLevel?: CultureLevelEnum;
  /** 政治面貌 */
  politicalOutlook?: string | null;
  /** 毕业院校 */
  college?: string | null;
  /** 办公电话 */
  officePhone?: string | null;
  /** 紧急联系人 */
  emergencyContact?: string | null;
  /** 紧急联系人电话 */
  emergencyPhone?: string | null;
  /** 紧急联系人地址 */
  emergencyAddress?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  accountType?: AccountTypeEnum;
  /** 直属机构Id */
  organizationId?: string;
  /** 职位Id */
  posId?: string;
  /** 工号 */
  jobNum?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
  /** 角色集合 */
  roleIdList?: number[] | null;
  /** 扩展机构集合 */
  extOrganizationIdList?: TenantUserExtensionOrganizationEntity[] | null;
}

export interface UserRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 是否启用 */
  isEnabled?: boolean;
}

/** 授权用户角色 */
export interface UserRoleRequest {
  /** 用户Id */
  userId?: string;
  /** 角色Id集合 */
  roleIdList?: number[] | null;
}

export interface TenantUserAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** @format int64 */
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

export interface TenantUserBaseInfoGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户用户输出参数 */
  data: TenantUserResponse;
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

export interface TenantUserBaseInfoPostData {
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

export interface TenantUserChangePwdPostData {
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

export type TenantUserDeletePostData = any;

export type TenantUserGrantRolePostData = any;

export interface TenantUserOwnExtOrgListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantUserExtensionOrganizationResponse[];
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

export interface TenantUserOwnRoleListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: number[];
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

export interface TenantUserPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantUserResponseSqlSugarPagedList;
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

export type TenantUserResetPwdPostData = ResetPwdResponseHttpJsonResultData;

export interface TenantUserSetStatusPostData {
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

export type TenantUserUpdatePostData = any;
