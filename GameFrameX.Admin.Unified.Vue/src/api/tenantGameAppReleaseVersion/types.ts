/* eslint-disable */
/* tslint:disable */

/** 游戏版本管理增加输入参数 */
export interface AddGameAppReleaseVersionRequest {
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** Description */
  description?: string | null;
  /** CreatedName */
  createdName?: string | null;
  /** UpdatedName */
  updatedName?: string | null;
  /**
   * Channel
   * @minLength 1
   */
  channel: string;
  /**
   * 公告标题
   * @minLength 1
   */
  title: string;
  /**
   * Announcement
   * @minLength 1
   */
  announcement: string;
  /**
   * OldVersion
   * @minLength 1
   */
  oldVersion: string;
  /**
   * DownloadUrl
   * @minLength 1
   */
  downloadUrl: string;
  /** IsForce */
  isForce: boolean;
  /**
   * Platform
   * @minLength 1
   */
  platform: string;
  /**
   * AppVersion
   * @minLength 1
   */
  appVersion: string;
  /**
   * Package
   * @minLength 1
   */
  package: string;
  /**
   * Language
   * @minLength 1
   */
  language: string;
}

/** 游戏版本管理输出参数 */
export interface GameAppReleaseVersionResponse {
  /** Id */
  id?: string;
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** 渠道 */
  channel?: string | null;
  /** 平台 */
  platform?: string | null;
  /** 版本 */
  appVersion?: string | null;
  /** 包名 */
  package?: string | null;
  /** 语言 */
  language?: string | null;
  /** 旧版本 */
  oldVersion?: string | null;
  /** 下载链接 */
  downloadUrl?: string | null;
  /** 是否强制更新 */
  isForce?: boolean;
  /** 公告标题 */
  title?: string | null;
  /** 公告内容 */
  announcement?: string | null;
  /** 详细描述 */
  description?: string | null;
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

export interface GameAppReleaseVersionResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppReleaseVersionResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏版本管理分页查询输入参数 */
export interface PageGameAppReleaseVersionRequest {
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
  /** OldVersion */
  oldVersion?: string | null;
  /** Description */
  description?: string | null;
}

/** 游戏版本管理更新输入参数 */
export interface UpdateGameAppReleaseVersionRequest {
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** Channel */
  channel?: string | null;
  /** 公告标题 */
  title?: string | null;
  /** Announcement */
  announcement?: string | null;
  /** OldVersion */
  oldVersion?: string | null;
  /** DownloadUrl */
  downloadUrl?: string | null;
  /** IsForce */
  isForce?: boolean | null;
  /** Platform */
  platform?: string | null;
  /** AppVersion */
  appVersion?: string | null;
  /** Package */
  package?: string | null;
  /** Language */
  language?: string | null;
  /** Description */
  description?: string | null;
  /**
   * 主键Id
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
}

export interface TenantGameAppReleaseVersionAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏版本管理输出参数 */
  data: GameAppReleaseVersionResponse;
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

export type TenantGameAppReleaseVersionDeletePostData = any;

export interface TenantGameAppReleaseVersionDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏版本管理输出参数 */
  data: GameAppReleaseVersionResponse;
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

export interface TenantGameAppReleaseVersionPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppReleaseVersionResponseSqlSugarPagedList;
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

export type TenantGameAppReleaseVersionUpdatePostData = any;
