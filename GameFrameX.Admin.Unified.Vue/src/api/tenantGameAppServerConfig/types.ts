/* eslint-disable */
/* tslint:disable */

/** 游戏运营配置管理增加输入参数 */
export interface AddGameAppServerConfigRequest {
  /** 应用作用域（应用工作台上下文必填；租户默认基线行只能由种子产生） */
  appId?: string | null;
  /** 配置键（AppId + Key 唯一） */
  key?: string | null;
  /** 配置内容 JSON 原文（服务端保存时归一化重写） */
  config?: string | null;
  /**
   * 内容版本号。服务端保存时强制重算，前端传入值一律被忽略；
   * 暴露为可写属性仅为复用通用 CRUD 的请求到实体映射，非契约输入
   */
  md5?: string | null;
  /** 键级启停：false 时登录 Config 不下发该键（客户端删除本地该键保持空态） */
  enabled?: boolean;
  /** 详细描述 */
  description?: string | null;
}

/** 游戏运营配置管理输出参数 */
export interface GameAppServerConfigResponse {
  /** Id */
  id?: string;
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** 配置键 */
  key?: string | null;
  /** 配置内容 JSON 原文 */
  config?: string | null;
  /** 内容版本号（服务端保存时自动重算） */
  md5?: string | null;
  /** 键级启停：false 时登录 Config 不下发该键 */
  enabled?: boolean;
  /** 详细描述 */
  description?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createdTime?: number;
  /** 创建者 */
  createdName?: string | null;
  /**
   * 更新时间
   * @format int64
   */
  updateTime?: number | null;
  /** 更新者 */
  updatedName?: string | null;
  /** 乐观锁版本（编辑回显原样回传） */
  rowVersion?: string | null;
}

export interface GameAppServerConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppServerConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏运营配置管理分页查询输入参数 */
export interface PageGameAppServerConfigRequest {
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
  /** 应用作用域筛选。游戏运营配置是应用工作台页面，分页查询必须提供具体应用，值必须大于 0 */
  appId?: string | null;
  /** 配置键精确筛选 */
  key?: string | null;
  /** 键级启停筛选 */
  enabled?: boolean | null;
}

/** 游戏运营配置管理更新输入参数 */
export interface UpdateGameAppServerConfigRequest {
  /** 应用作用域（应用工作台上下文必填；租户默认基线行只能由种子产生） */
  appId?: string | null;
  /** 配置键（AppId + Key 唯一） */
  key?: string | null;
  /** 配置内容 JSON 原文（服务端保存时归一化重写） */
  config?: string | null;
  /**
   * 内容版本号。服务端保存时强制重算，前端传入值一律被忽略；
   * 暴露为可写属性仅为复用通用 CRUD 的请求到实体映射，非契约输入
   */
  md5?: string | null;
  /** 键级启停：false 时登录 Config 不下发该键（客户端删除本地该键保持空态） */
  enabled?: boolean;
  /** 详细描述 */
  description?: string | null;
  /** 主键 */
  id?: string;
  /** 乐观锁版本（编辑回显 RowVersion 原样回传） */
  rowVersion?: string | null;
}

export interface TenantGameAppServerConfigAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏运营配置管理输出参数 */
  data: GameAppServerConfigResponse;
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

export type TenantGameAppServerConfigDeletePostData = any;

export interface TenantGameAppServerConfigDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏运营配置管理输出参数 */
  data: GameAppServerConfigResponse;
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

export interface TenantGameAppServerConfigPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppServerConfigResponseSqlSugarPagedList;
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

export type TenantGameAppServerConfigUpdatePostData = any;
