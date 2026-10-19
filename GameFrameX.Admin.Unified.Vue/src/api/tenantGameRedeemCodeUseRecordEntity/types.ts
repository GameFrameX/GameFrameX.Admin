/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse } from "../_shared/data-contracts";

/** 兑换记录增加输入参数 */
export interface AddTenantGameRedeemCodeUseRecordEntityRequest {
  /** 主键Id */
  id: string;
}

/** 兑换记录分页查询输入参数 */
export interface PageTenantGameRedeemCodeUseRecordEntityRequest {
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
  areaId?: string | null;
  /** 兑换码 */
  code?: string | null;
  /** 角色ID */
  playerId?: string | null;
  /** 兑换码渠道/分组ID（关联 TenantGameRedeemCodeEntity.ChannelId 过滤） */
  channelId?: string | null;
  /** 应用Id（可选过滤，按当前应用筛选兑换记录） */
  appId?: string | null;
}

/** 兑换记录输出参数 */
export interface TenantGameRedeemCodeUseRecordEntityResponse {
  /** Id */
  id?: string;
  /** 应用Id。事实数据应用作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 区服ID */
  areaId?: string;
  /** 礼包码的唯一字符串。 */
  code?: string | null;
  /** 角色ID */
  playerId?: string;
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
  /** 角色ID */
  playerIdString?: string | null;
  /** 兑换码渠道/分组ID（来自关联的 TenantGameRedeemCodeEntity.ChannelId） */
  channelId?: string | null;
  /** 兑换码渠道/分组名称（来自关联的 TenantGameRedeemCodeChannelEntity.Name） */
  channelName?: string | null;
}

export interface TenantGameRedeemCodeUseRecordEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameRedeemCodeUseRecordEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 兑换记录更新输入参数 */
export interface UpdateTenantGameRedeemCodeUseRecordEntityRequest {
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
}

export interface TenantGameRedeemCodeUseRecordEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换记录输出参数 */
  data: TenantGameRedeemCodeUseRecordEntityResponse;
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

export type TenantGameRedeemCodeUseRecordEntityDeletePostData = any;

export interface TenantGameRedeemCodeUseRecordEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换记录输出参数 */
  data: TenantGameRedeemCodeUseRecordEntityResponse;
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

export interface TenantGameRedeemCodeUseRecordEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameRedeemCodeUseRecordEntityResponseSqlSugarPagedList;
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

export interface TenantGameRedeemCodeUseRecordEntityTenantGameAreaEntityAreaIdDropdownGetData {
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

export type TenantGameRedeemCodeUseRecordEntityUpdatePostData = any;
