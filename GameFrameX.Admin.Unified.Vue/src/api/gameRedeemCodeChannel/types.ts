/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 兑换码渠道增加输入参数 */
export interface AddGameRedeemCodeChannelEntityRequest {
  /** 主键Id */
  id: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /**
   * 每角色可用次数（兑换码分组角色限制）。0 = 不限制，1 = 每角色一次，N = 每角色最多 N 次。
   * @format int32
   * @min 0
   * @max 2147483647
   */
  roleUsageLimit?: number;
}

/** 兑换码渠道输出参数 */
export interface GameRedeemCodeChannelResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /**
   * 每角色可用次数（兑换码分组角色限制）。0 = 不限制，1 = 每角色一次，N = 每角色最多 N 次。
   * @format int32
   */
  roleUsageLimit?: number;
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

export interface GameRedeemCodeChannelResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameRedeemCodeChannelResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 兑换码渠道分页查询输入参数 */
export interface PageGameRedeemCodeChannelEntityRequest {
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
  /** 名称 */
  name?: string | null;
}

/** 兑换码渠道更新输入参数 */
export interface UpdateGameRedeemCodeChannelEntityRequest {
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
   * 名称。此前 UpdateRequest 缺此声明：编辑提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
   * 又被 IgnoreColumns(true)（忽略 null 列）跳过 —— update 200 但名称静默不更新。
   * @minLength 1
   */
  name: string;
  /**
   * 详细描述
   * @minLength 1
   */
  description: string;
  /**
   * 每角色可用次数（兑换码分组角色限制）。0 = 不限制，1 = 每角色一次，N = 每角色最多 N 次。
   * @format int32
   * @min 0
   * @max 2147483647
   */
  roleUsageLimit?: number;
}

export interface GameRedeemCodeChannelAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换码渠道输出参数 */
  data: GameRedeemCodeChannelResponse;
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

export type GameRedeemCodeChannelDeletePostData = any;

export interface GameRedeemCodeChannelDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换码渠道输出参数 */
  data: GameRedeemCodeChannelResponse;
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

export interface GameRedeemCodeChannelPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameRedeemCodeChannelResponseSqlSugarPagedList;
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

export interface GameRedeemCodeChannelSelectPostData {
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

export interface GameRedeemCodeChannelSelectStringPostData {
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

export type GameRedeemCodeChannelUpdatePostData = any;
