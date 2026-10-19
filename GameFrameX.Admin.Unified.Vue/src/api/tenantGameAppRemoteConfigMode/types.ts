/* eslint-disable */
/* tslint:disable */

/** 远程配置模式定义增加输入参数 */
export interface AddGameAppRemoteConfigModeRequest {
  /** 描述 */
  description?: string | null;
  /**
   * 排序序号
   * @format int32
   */
  sortOrder?: number;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /**
   * 模式编码
   * @minLength 1
   */
  modeCode: string;
  /**
   * 显示名称
   * @minLength 1
   */
  displayName: string;
}

/** 远程配置模式定义输出参数 */
export interface GameAppRemoteConfigModeResponse {
  /** Id */
  id?: string;
  /** 模式编码 */
  modeCode?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 描述 */
  description?: string | null;
  /**
   * 排序序号
   * @format int32
   */
  sortOrder?: number;
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

export interface GameAppRemoteConfigModeResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppRemoteConfigModeResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 远程配置模式定义分页查询输入参数 */
export interface PageGameAppRemoteConfigModeRequest {
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
  /** 模式编码 */
  modeCode?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

/** 远程配置模式定义更新输入参数 */
export interface UpdateGameAppRemoteConfigModeRequest {
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
   * 模式编码
   * @minLength 1
   */
  modeCode: string;
  /**
   * 显示名称
   * @minLength 1
   */
  displayName: string;
  /** 描述 */
  description?: string | null;
  /**
   * 排序序号
   * @format int32
   */
  sortOrder?: number;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

export interface TenantGameAppRemoteConfigModeAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 远程配置模式定义输出参数 */
  data: GameAppRemoteConfigModeResponse;
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

export type TenantGameAppRemoteConfigModeDeletePostData = any;

export interface TenantGameAppRemoteConfigModeDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 远程配置模式定义输出参数 */
  data: GameAppRemoteConfigModeResponse;
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

export interface TenantGameAppRemoteConfigModePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppRemoteConfigModeResponseSqlSugarPagedList;
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

export type TenantGameAppRemoteConfigModeUpdatePostData = any;
