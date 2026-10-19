/* eslint-disable */
/* tslint:disable */

/** 活动/功能开关管理增加输入参数 */
export interface AddGameAppFeatureSwitchRequest {
  /** 应用作用域；null / 0 表示租户默认层 */
  appId?: string | null;
  /** 开关键（同一应用作用域 + 相同作用域维度组合内唯一） */
  switchKey?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 消费方标记：client / server / both */
  audience?: string | null;
  /** 开关参数 JSON 原文（下发侧自行解析，服务端只校验其为合法 JSON） */
  valueJson?: string | null;
  /**
   * 生效开始时间（Unix 秒，UTC）；null 表示该端不限
   * @format int64
   */
  startTime?: number | null;
  /**
   * 生效结束时间（Unix 秒，UTC）；null 表示该端不限，区间左闭右开
   * @format int64
   */
  endTime?: number | null;
  /** 平台作用域；空 / default 表示不限 */
  platform?: string | null;
  /** 包名作用域；空 / default 表示不限，租户默认层不允许携带具体包名 */
  package?: string | null;
  /** 语言作用域；空 / default 表示不限 */
  language?: string | null;
  /** 渠道作用域；空 / default 表示不限 */
  channel?: string | null;
  /**
   * 投放优先级。同一开关键下作用域评分相同时数值越大越优先
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组；命中白名单的设备绕过灰度比例 */
  deviceWhitelistJson?: string | null;
  /** 启停标记；false 时投放解析不下发该行 */
  isEnabled?: boolean | null;
}

/** 活动/功能开关管理输出参数（字段名与实体逐字一致，名映射投影） */
export interface GameAppFeatureSwitchResponse {
  /** Id */
  id?: string;
  /** 应用作用域，0 / null 表示租户默认层 */
  appId?: string | null;
  /** 开关键 */
  switchKey?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 消费方标记：client / server / both */
  audience?: string | null;
  /** 开关参数 JSON 原文 */
  valueJson?: string | null;
  /**
   * 生效开始时间（Unix 秒，UTC）
   * @format int64
   */
  startTime?: number | null;
  /**
   * 生效结束时间（Unix 秒，UTC）
   * @format int64
   */
  endTime?: number | null;
  /** 平台作用域，default 表示不限 */
  platform?: string | null;
  /** 包名作用域，default 表示不限 */
  package?: string | null;
  /** 语言作用域，default 表示不限 */
  language?: string | null;
  /** 渠道作用域，default 表示不限 */
  channel?: string | null;
  /**
   * 投放优先级
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组 */
  deviceWhitelistJson?: string | null;
  /** 启停标记 */
  isEnabled?: boolean | null;
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

export interface GameAppFeatureSwitchResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppFeatureSwitchResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 活动/功能开关管理分页查询输入参数 */
export interface PageGameAppFeatureSwitchRequest {
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
  /** 应用作用域筛选。给正值时只看该应用行；留空或 0 时只看租户默认层行 */
  appId?: string | null;
  /** 开关键模糊筛选 */
  switchKey?: string | null;
  /** 启停筛选 */
  isEnabled?: boolean | null;
  /** 消费方标记精确筛选：client / server / both */
  audience?: string | null;
}

/** 活动/功能开关管理更新输入参数 */
export interface UpdateGameAppFeatureSwitchRequest {
  /** 应用作用域；null / 0 表示租户默认层 */
  appId?: string | null;
  /** 开关键（同一应用作用域 + 相同作用域维度组合内唯一） */
  switchKey?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 消费方标记：client / server / both */
  audience?: string | null;
  /** 开关参数 JSON 原文（下发侧自行解析，服务端只校验其为合法 JSON） */
  valueJson?: string | null;
  /**
   * 生效开始时间（Unix 秒，UTC）；null 表示该端不限
   * @format int64
   */
  startTime?: number | null;
  /**
   * 生效结束时间（Unix 秒，UTC）；null 表示该端不限，区间左闭右开
   * @format int64
   */
  endTime?: number | null;
  /** 平台作用域；空 / default 表示不限 */
  platform?: string | null;
  /** 包名作用域；空 / default 表示不限，租户默认层不允许携带具体包名 */
  package?: string | null;
  /** 语言作用域；空 / default 表示不限 */
  language?: string | null;
  /** 渠道作用域；空 / default 表示不限 */
  channel?: string | null;
  /**
   * 投放优先级。同一开关键下作用域评分相同时数值越大越优先
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组；命中白名单的设备绕过灰度比例 */
  deviceWhitelistJson?: string | null;
  /** 启停标记；false 时投放解析不下发该行 */
  isEnabled?: boolean | null;
  /** 主键 */
  id?: string;
  /** 乐观锁版本（编辑回显 RowVersion 原样回传） */
  rowVersion?: string | null;
}

export interface TenantGameAppFeatureSwitchAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 活动/功能开关管理输出参数（字段名与实体逐字一致，名映射投影） */
  data: GameAppFeatureSwitchResponse;
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

export type TenantGameAppFeatureSwitchDeletePostData = any;

export interface TenantGameAppFeatureSwitchDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 活动/功能开关管理输出参数（字段名与实体逐字一致，名映射投影） */
  data: GameAppFeatureSwitchResponse;
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

export interface TenantGameAppFeatureSwitchPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppFeatureSwitchResponseSqlSugarPagedList;
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

export type TenantGameAppFeatureSwitchUpdatePostData = any;
