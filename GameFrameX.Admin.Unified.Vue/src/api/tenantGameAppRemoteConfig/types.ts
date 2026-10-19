/* eslint-disable */
/* tslint:disable */

import type { OnlineLiveOpsDispatchResponse } from "../_shared/data-contracts";

/** 远程配置管理增加输入参数 */
export interface AddGameAppRemoteConfigRequest {
  /** 应用作用域。管理端客户端远程配置必须从应用工作台维护，值必须大于 0。 */
  appId?: string | null;
  /** Channel */
  channel?: string | null;
  /** Content */
  content?: string | null;
  /** AotCodeList */
  aotCodeList?: string | null;
  /** CheckResourceUrl */
  checkResourceUrl?: string | null;
  /** CheckAppVersionUrl */
  checkAppVersionUrl?: string | null;
  /** Platform */
  platform?: string | null;
  /** AppVersion */
  appVersion?: string | null;
  /** Package */
  package?: string | null;
  /** Language */
  language?: string | null;
  /** 模式内容映射 JSON */
  modeContents?: string | null;
  /** 当前激活模式编码 */
  activeModeCode?: string | null;
  /**
   * 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。 */
  deviceWhitelistJson?: string | null;
  /** Description */
  description?: string | null;
  /** CreatedName */
  createdName?: string | null;
  /** UpdatedName */
  updatedName?: string | null;
}

/** 远程配置管理输出参数 */
export interface GameAppRemoteConfigResponse {
  /** Id */
  id?: string;
  /** 应用作用域，0 / null 表示租户默认配置 */
  appId?: string | null;
  /** 渠道 */
  channel?: string | null;
  /** 平台 */
  platform?: string | null;
  /** App版本 */
  appVersion?: string | null;
  /** 包名 */
  package?: string | null;
  /** 语言 */
  language?: string | null;
  /** 检测资源更新地址 */
  checkResourceUrl?: string | null;
  /** 检测APP更新地址 */
  checkAppVersionUrl?: string | null;
  /** 内容 */
  content?: string | null;
  /** Aot 代码列表 */
  aotCodeList?: string | null;
  /** 模式内容映射 JSON (key=ModeCode, value=content object) */
  modeContents?: string | null;
  /** 当前激活模式编码 */
  activeModeCode?: string | null;
  /**
   * 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。 */
  deviceWhitelistJson?: string | null;
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

export interface GameAppRemoteConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppRemoteConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 远程配置管理分页查询输入参数 */
export interface PageGameAppRemoteConfigRequest {
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
  /** 应用作用域筛选。客户端远程配置是应用工作台页面，分页查询必须提供具体应用，值必须大于 0。 */
  appId?: string | null;
  /** Content */
  content?: string | null;
  /** CheckResourceUrl */
  checkResourceUrl?: string | null;
  /** CheckAppVersionUrl */
  checkAppVersionUrl?: string | null;
  /** CreatedName */
  createdName?: string | null;
  /** UpdatedName */
  updatedName?: string | null;
  /** Description */
  description?: string | null;
}

/** 远程配置管理更新输入参数 */
export interface UpdateGameAppRemoteConfigRequest {
  /** 应用作用域。管理端客户端远程配置必须从应用工作台维护，值必须大于 0。 */
  appId?: string | null;
  /** Channel */
  channel?: string | null;
  /** Content */
  content?: string | null;
  /** AotCodeList */
  aotCodeList?: string | null;
  /** CheckResourceUrl */
  checkResourceUrl?: string | null;
  /** CheckAppVersionUrl */
  checkAppVersionUrl?: string | null;
  /** Platform */
  platform?: string | null;
  /** AppVersion */
  appVersion?: string | null;
  /** Package */
  package?: string | null;
  /** Language */
  language?: string | null;
  /** 模式内容映射 JSON */
  modeContents?: string | null;
  /** 当前激活模式编码 */
  activeModeCode?: string | null;
  /**
   * 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
   * @format int32
   */
  priority?: number;
  /**
   * 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
   * @format int32
   */
  rolloutPercent?: number;
  /** 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。 */
  deviceWhitelistJson?: string | null;
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

export interface TenantGameAppRemoteConfigAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 远程配置管理输出参数 */
  data: GameAppRemoteConfigResponse;
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

export type TenantGameAppRemoteConfigDeletePostData = any;

export interface TenantGameAppRemoteConfigDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 远程配置管理输出参数 */
  data: GameAppRemoteConfigResponse;
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

export interface TenantGameAppRemoteConfigDispatchOnlinePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * LiveOps 统一下发回执（来源 change C343 / vault:C8 S7.7）。
   *
   * 透传 Online 确认版本 / 状态供前端回显；`Merged` 标记邮件来源「同目标合并返回首次回执」路径。
   */
  data: OnlineLiveOpsDispatchResponse;
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

export interface TenantGameAppRemoteConfigPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppRemoteConfigResponseSqlSugarPagedList;
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

export type TenantGameAppRemoteConfigUpdatePostData = any;
