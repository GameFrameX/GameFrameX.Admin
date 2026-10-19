/* eslint-disable */
/* tslint:disable */

import type { OnlineLiveOpsDispatchResponse } from "../_shared/data-contracts";

/** 游戏公告增加输入参数 */
export interface AddTenantGameAnnouncementEntityRequest {
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  language?: string | null;
  areaId?: string | null;
  /** @minLength 1 */
  title: string;
  titleLocalizationKey?: string | null;
  /** @minLength 1 */
  content: string;
  contentLocalizationKey?: string | null;
  /** @format int64 */
  startTime: number;
  /** @format int64 */
  endTime: number;
  /** @format int32 */
  priority: number;
  isActive: boolean;
}

/** 游戏公告分页查询输入参数 */
export interface PageTenantGameAnnouncementEntityRequest {
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
  /** 应用Id */
  appId?: string | null;
  /** 语言 */
  language?: string | null;
  /** 标题 */
  title?: string | null;
  /**
   * 开始时间
   * @format int64
   */
  startTime?: number | null;
  /** 开始时间范围 */
  startTimeRange?: (number | null)[];
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
  /** 结束时间范围 */
  endTimeRange?: (number | null)[];
}

/** 游戏公告输出参数 */
export interface TenantGameAnnouncementEntityResponse {
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
  /** 租户Id */
  tenantId?: string | null;
  appId?: string;
  language?: string | null;
  areaId?: string | null;
  title?: string | null;
  titleLocalizationKey?: string | null;
  content?: string | null;
  contentLocalizationKey?: string | null;
  /** @format int64 */
  startTime?: number;
  /** @format int64 */
  endTime?: number;
  /** @format int32 */
  priority?: number;
  isActive?: boolean;
}

export interface TenantGameAnnouncementEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAnnouncementEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏公告更新输入参数 */
export interface UpdateTenantGameAnnouncementEntityRequest {
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  language?: string | null;
  areaId?: string | null;
  /** @minLength 1 */
  title: string;
  titleLocalizationKey?: string | null;
  /** @minLength 1 */
  content: string;
  contentLocalizationKey?: string | null;
  /** @format int64 */
  startTime: number;
  /** @format int64 */
  endTime: number;
  /** @format int32 */
  priority: number;
  isActive: boolean;
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

export interface TenantGameAnnouncementEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏公告输出参数 */
  data: TenantGameAnnouncementEntityResponse;
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

export type TenantGameAnnouncementEntityDeletePostData = any;

export interface TenantGameAnnouncementEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏公告输出参数 */
  data: TenantGameAnnouncementEntityResponse;
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

export interface TenantGameAnnouncementEntityDispatchOnlinePostData {
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

export interface TenantGameAnnouncementEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAnnouncementEntityResponseSqlSugarPagedList;
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

export type TenantGameAnnouncementEntityUpdatePostData = any;
