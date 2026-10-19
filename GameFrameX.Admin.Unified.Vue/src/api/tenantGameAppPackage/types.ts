/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum PackageKindEnum {
  /** 普通 */
  Normal = 1,
  /** 母包 */
  Master = 2,
  /** 子包 */
  Derived = 3,
}

/** @format int32 */
export enum InheritanceModeEnum {
  /** 继承 */
  Inherit = 1,
  /** 覆盖 */
  Override = 2,
  /** 禁用 */
  Disable = 3,
}

/** 游戏应用分包新增输入参数 */
export interface AddGameAppPackageRequest {
  /**
   * 游戏应用Id
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string;
  /**
   * 分包名称
   * @minLength 1
   */
  name: string;
  /**
   * 包名
   * @minLength 1
   */
  packageName: string;
  /**
   * 平台编码快照
   * @minLength 1
   */
  platformCode: string;
  /** 平台名称快照 */
  platformName?: string | null;
  /**
   * 渠道Id
   * @min 1
   * @max 9223372036854776000
   */
  channelId?: string;
  /** 渠道名称快照 */
  channelName?: string | null;
  /** 母包Id */
  parentPackageId?: string | null;
  packageKind?: PackageKindEnum;
  platformInheritanceMode?: InheritanceModeEnum;
  /** 版本号 */
  packageVersion?: string | null;
  /** 是否默认分包 */
  isDefault?: boolean;
  /** 备注 */
  remark?: string | null;
}

/** 游戏应用分包选择输出 */
export interface GameAppPackageListResponse {
  /** Id */
  id?: string;
  /** 游戏应用Id */
  appId?: string;
  /** 分包名称 */
  name?: string | null;
  /** 包名 */
  packageName?: string | null;
  /** 分包版本号 */
  packageVersion?: string | null;
  /** 是否默认分包 */
  isDefault?: boolean;
}

/** 游戏应用分包输出参数 */
export interface GameAppPackageResponse {
  /** Id */
  id?: string;
  /** 游戏应用Id */
  appId?: string;
  /** 分包名称 */
  name?: string | null;
  /** 描述 */
  description?: string | null;
  /** 包名 */
  packageName?: string | null;
  /** 平台编码快照 */
  platformCode?: string | null;
  /** 平台名称快照 */
  platformName?: string | null;
  /** 渠道Id */
  channelId?: string;
  /** 渠道编码快照 */
  channelCode?: string | null;
  /** 渠道名称快照 */
  channelName?: string | null;
  /** 母包Id */
  parentPackageId?: string | null;
  packageKind?: PackageKindEnum;
  platformInheritanceMode?: InheritanceModeEnum;
  /** 版本号 */
  packageVersion?: string | null;
  /** 是否默认分包 */
  isDefault?: boolean;
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
  updateTime?: number;
  /** 更新人 */
  updatedName?: string | null;
  /** 乐观锁版本。前端按 `rowVersion` 回传，避免与业务包版本 GameFrameX.Application.Game.App.GameAppPackage.Dto.GameAppPackageResponse.PackageVersion 混淆。 */
  rowVersion?: string;
  /** 备注 */
  remark?: string | null;
}

export interface GameAppPackageResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppPackageResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏应用分包分页查询输入参数 */
export interface PageGameAppPackageRequest {
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
  /** 游戏应用Id */
  appId?: string | null;
  /** 分包名称 */
  name?: string | null;
  /** 包名 */
  packageName?: string | null;
  /** 分包版本号 */
  packageVersion?: string | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
  /** 母包Id */
  parentPackageId?: string | null;
}

/** 游戏应用分包启停输入参数 */
export interface SetGameAppPackageStatusRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 是否启用 */
  isEnabled?: boolean;
}

/** 游戏应用分包更新输入参数 */
export interface UpdateGameAppPackageRequest {
  /**
   * 游戏应用Id
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string;
  /**
   * 分包名称
   * @minLength 1
   */
  name: string;
  /**
   * 包名
   * @minLength 1
   */
  packageName: string;
  /**
   * 平台编码快照
   * @minLength 1
   */
  platformCode: string;
  /** 平台名称快照 */
  platformName?: string | null;
  /**
   * 渠道Id
   * @min 1
   * @max 9223372036854776000
   */
  channelId?: string;
  /** 渠道名称快照 */
  channelName?: string | null;
  /** 母包Id */
  parentPackageId?: string | null;
  packageKind?: PackageKindEnum;
  platformInheritanceMode?: InheritanceModeEnum;
  /** 版本号 */
  packageVersion?: string | null;
  /** 是否默认分包 */
  isDefault?: boolean;
  /** 备注 */
  remark?: string | null;
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

export interface TenantGameAppPackageAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏应用分包输出参数 */
  data: GameAppPackageResponse;
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

export type TenantGameAppPackageDeletePostData = any;

export interface TenantGameAppPackageDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏应用分包输出参数 */
  data: GameAppPackageResponse;
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

export interface TenantGameAppPackageListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppPackageListResponse[];
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

export interface TenantGameAppPackagePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppPackageResponseSqlSugarPagedList;
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

export interface TenantGameAppPackageSelectPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageResponse[];
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

export interface TenantGameAppPackageSelectStringPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageStringResponse[];
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

export type TenantGameAppPackageSetDefaultPostData = any;

export type TenantGameAppPackageSetStatusPostData = any;

export type TenantGameAppPackageUpdatePostData = any;
