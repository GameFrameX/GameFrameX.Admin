/* eslint-disable */
/* tslint:disable */

export interface AddTenantPackageRequest {
  /**
   * 套餐名称
   * @minLength 1
   */
  name: string;
  /**
   * 套餐编码
   * @minLength 1
   */
  code: string;
  /**
   * 用户数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxUserCount?: number;
  /**
   * 组织数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxOrganizationCount?: number;
  /**
   * 组织层级深度上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxOrganizationLevel?: number;
  /**
   * 角色数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxRoleCount?: number;
  /**
   * 游戏应用数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxGameAppCount?: number;
  /**
   * 每应用分包数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxPackageCountPerApp?: number;
  /**
   * 默认过渡期天数
   * @format int32
   * @min 0
   * @max 365
   */
  gracePeriodDays?: number;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 是否启用 */
  isEnabled?: boolean;
  /** 关联菜单Id列表 */
  menuIdList?: number[] | null;
}

export interface DeleteTenantPackageRequest {
  /** Id */
  id: string;
}

export interface PageTenantPackageRequest {
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
  /** 套餐名称 */
  name?: string | null;
}

/** 租户到期状态结果（前端到期横幅用） */
export interface TenantExpirationStatusResponse {
  /** 到期状态小写字符串：active（正常）/ ingraceperiod（宽限期，已到期仍在宽限期内）/ expired（已过期） */
  status?: string | null;
  /**
   * 距到期剩余天数；租户未配置到期时间时为 null
   * @format int32
   */
  daysLeft?: number | null;
}

export interface TenantExpirationStatusResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 租户到期状态结果（前端到期横幅用） */
  data?: TenantExpirationStatusResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 系统租户套餐输出参数 */
export interface TenantPackageResponse {
  /** Id */
  id?: string;
  /** 套餐名称 */
  name?: string | null;
  /** 套餐编码 */
  code?: string | null;
  /**
   * 用户数上限
   * @format int32
   */
  maxUserCount?: number;
  /**
   * 组织数上限
   * @format int32
   */
  maxOrganizationCount?: number;
  /**
   * 组织层级深度上限
   * @format int32
   */
  maxOrganizationLevel?: number;
  /**
   * 角色数上限
   * @format int32
   */
  maxRoleCount?: number;
  /**
   * 游戏应用数上限
   * @format int32
   */
  maxGameAppCount?: number;
  /**
   * 每应用分包数上限
   * @format int32
   */
  maxPackageCountPerApp?: number;
  /**
   * 默认过渡期天数
   * @format int32
   */
  gracePeriodDays?: number;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 关联菜单Id列表 */
  menuIdList?: number[] | null;
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
}

export interface TenantPackageResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantPackageResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface UpdateTenantPackageRequest {
  /**
   * 套餐名称
   * @minLength 1
   */
  name: string;
  /**
   * 套餐编码
   * @minLength 1
   */
  code: string;
  /**
   * 用户数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxUserCount?: number;
  /**
   * 组织数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxOrganizationCount?: number;
  /**
   * 组织层级深度上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxOrganizationLevel?: number;
  /**
   * 角色数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxRoleCount?: number;
  /**
   * 游戏应用数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxGameAppCount?: number;
  /**
   * 每应用分包数上限
   * @format int32
   * @min 1
   * @max 2147483647
   */
  maxPackageCountPerApp?: number;
  /**
   * 默认过渡期天数
   * @format int32
   * @min 0
   * @max 365
   */
  gracePeriodDays?: number;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 是否启用 */
  isEnabled?: boolean;
  /** 关联菜单Id列表 */
  menuIdList?: number[] | null;
  /** Id */
  id: string;
}

export type GlobalPackageAddPostData = any;

export interface GlobalPackageCurrentTenantPackageGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 系统租户套餐输出参数 */
  data: TenantPackageResponse;
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

export type GlobalPackageDeletePostData = any;

export interface GlobalPackageDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 系统租户套餐输出参数 */
  data: TenantPackageResponse;
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

export interface GlobalPackagePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantPackageResponseSqlSugarPagedList;
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

export type GlobalPackageTenantExpirationStatusGetData =
  TenantExpirationStatusResponseHttpJsonResultData;

export type GlobalPackageUpdatePostData = any;
