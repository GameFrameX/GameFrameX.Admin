/* eslint-disable */
/* tslint:disable */

import type { AccountTypeEnum } from "../_shared/data-contracts";

/** @format int32 */
export enum GenderEnum {
  /** 男 */
  Male = 1,
  /** 女 */
  Female = 2,
  /** 其他 */
  Other = 3,
}

/** @format int32 */
export enum CultureLevelEnum {
  /** 其他 */
  Level0 = 0,
  /** 小学 */
  Level1 = 1,
  /** 初中 */
  Level2 = 2,
  /** 普通高中 */
  Level3 = 3,
  /** 技工学校 */
  Level4 = 4,
  /** 职业教育 */
  Level5 = 5,
  /** 职业高中 */
  Level6 = 6,
  /** 中等专科 */
  Level7 = 7,
  /** 大学专科 */
  Level8 = 8,
  /** 大学本科 */
  Level9 = 9,
  /** 硕士研究生 */
  Level10 = 10,
  /** 博士研究生 */
  Level11 = 11,
}

/** @format int32 */
export enum CardTypeEnum {
  /** 身份证 */
  IdCard = 0,
  /** 护照 */
  PassportCard = 1,
  /** 出生证 */
  BirthCard = 2,
  /** 港澳台通行证 */
  GatCard = 3,
  /** 外国人居留证 */
  ForeignCard = 4,
  /** 营业执照 */
  License = 5,
}

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
  /** 工号 */
  jobNum?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
  /** 角色集合 */
  roleIdList?: number[] | null;
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
}

/** 重置密码结果（平台用户与管理员角色重置共用） */
export interface ResetPwdResponse {
  /** 重置后的初始密码（取自系统密码配置；一次性明文返回，仅本次响应可见，禁止落日志/缓存） */
  newPassword?: string | null;
}

export interface ResetPwdResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 重置密码结果（平台用户与管理员角色重置共用） */
  data?: ResetPwdResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export interface ResetPwdUserRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
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
  /** 工号 */
  jobNum?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
  /** 角色集合 */
  roleIdList?: number[] | null;
}

export interface UserEntity {
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
  account?: string | null;
  realName?: string | null;
  nickName?: string | null;
  avatar?: string | null;
  sex?: GenderEnum;
  /** @format int32 */
  age?: number;
  /** @format int64 */
  birthdayTime?: number | null;
  nation?: string | null;
  phone?: string | null;
  cardType?: CardTypeEnum;
  idCardNum?: string | null;
  email?: string | null;
  address?: string | null;
  cultureLevel?: CultureLevelEnum;
  politicalOutlook?: string | null;
  college?: string | null;
  officePhone?: string | null;
  emergencyContact?: string | null;
  emergencyPhone?: string | null;
  emergencyAddress?: string | null;
  introduction?: string | null;
  /** @format int32 */
  orderNo?: number;
  remark?: string | null;
  accountType?: AccountTypeEnum;
  managerUserId?: string | null;
  managerUser?: UserEntity;
  jobNum?: string | null;
  posLevel?: string | null;
  posTitle?: string | null;
  expertise?: string | null;
  officeZone?: string | null;
  office?: string | null;
  /** @format int64 */
  joinTime?: number | null;
  lastLoginIp?: string | null;
  lastLoginAddress?: string | null;
  /** @format int64 */
  lastLoginTime?: number | null;
  lastLoginDevice?: string | null;
  signature?: string | null;
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

/** 用户输出参数 */
export interface UserResponse {
  /** Id */
  id?: string;
  /** 账号 */
  account?: string | null;
  /** 真实姓名 */
  realName?: string | null;
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
  /** 个人简介 */
  introduction?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  accountType?: AccountTypeEnum;
  /** 直属主管Id */
  managerUserId?: string | null;
  /** 工号 */
  jobNum?: string | null;
  /** 职级 */
  posLevel?: string | null;
  /** 职称 */
  posTitle?: string | null;
  /** 擅长领域 */
  expertise?: string | null;
  /** 办公区域 */
  officeZone?: string | null;
  /** 办公室 */
  office?: string | null;
  /**
   * 入职日期
   * @format int64
   */
  joinTime?: number | null;
  /** 最新登录Ip */
  lastLoginIp?: string | null;
  /** 最新登录地点 */
  lastLoginAddress?: string | null;
  /**
   * 最新登录时间
   * @format int64
   */
  lastLoginTime?: number | null;
  /** 最新登录设备 */
  lastLoginDevice?: string | null;
  /** 电子签名 */
  signature?: string | null;
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

export interface UserResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: UserResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 授权用户角色 */
export interface UserRoleRequest {
  /** 用户Id */
  userId?: string;
  /** 角色Id集合 */
  roleIdList?: number[] | null;
}

export interface UserAddPostData {
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
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface UserBaseInfoGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 用户输出参数 */
  data: UserResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface UserBaseInfoPostData {
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
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface UserChangePwdPostData {
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
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export type UserDeletePostData = any;

export type UserGrantRolePostData = any;

export interface UserOwnRoleListGetData {
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
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface UserPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: UserResponseSqlSugarPagedList;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export type UserResetPwdPostData = ResetPwdResponseHttpJsonResultData;

export interface UserSetStatusPostData {
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
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export type UserUpdatePostData = any;
