/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 游戏定时任务增加输入参数 */
export interface AddTenantGameTimeJobEntityRequest {
  /** 主键Id */
  id: string;
  /** 应用Id。运营内容应用作用域，必须 > 0；由 Admin 后台当前应用上下文写入。 */
  appId: string;
  /**
   * 名称。此前 AddRequest 缺此声明：表单提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
   * 实体列 NOT NULL 导致 insert 500（cron 同因，报错先撞上 cron）。
   * @minLength 1
   */
  name: string;
  /** 详细描述 */
  description?: string | null;
  /**
   * Cron 表达式（实体列 NOT NULL）
   * @minLength 1
   */
  cron: string;
  /** 是否一次性任务 */
  isOnce?: boolean;
  /** 任务来源。系统标记非用户输入：手动新增由服务端固定写 "Manual"（实体列 NOT NULL）。 */
  source?: string | null;
}

/** 游戏定时任务分页查询输入参数 */
export interface PageTenantGameTimeJobEntityRequest {
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
  /** 应用Id（可选过滤） */
  appId?: string | null;
  /** 名称 */
  name?: string | null;
}

/** 游戏定时任务输出参数 */
export interface TenantGameTimeJobEntityResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 应用Id */
  appId?: string;
  /** 是否一次性任务 */
  isOnce?: boolean;
  /** 执行时间 */
  cron?: string | null;
  /** 任务来源 */
  source?: string | null;
  /** 邮件id */
  mailId?: string;
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

export interface TenantGameTimeJobEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameTimeJobEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏定时任务更新输入参数 */
export interface UpdateTenantGameTimeJobEntityRequest {
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
  /** 应用Id。不可变（服务端从既有实体回填）；声明于 DTO 仅为契约对称。 */
  appId?: string;
  /**
   * 名称。此前 UpdateRequest 缺此声明：编辑提交的 name 被绑定丢弃，Adapt 后实体 Name=null，
   * 又被 IgnoreColumns(true)（忽略 null 列）跳过 —— update 200 但名称静默不更新（与兑换码渠道问题同模式）。
   * @minLength 1
   */
  name: string;
  /** 详细描述 */
  description?: string | null;
  /**
   * Cron 表达式
   * @minLength 1
   */
  cron: string;
  /** 是否一次性任务 */
  isOnce?: boolean;
}

export interface TenantGameTimeJobEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏定时任务输出参数 */
  data: TenantGameTimeJobEntityResponse;
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

export type TenantGameTimeJobEntityDeletePostData = any;

export interface TenantGameTimeJobEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏定时任务输出参数 */
  data: TenantGameTimeJobEntityResponse;
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

export interface TenantGameTimeJobEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameTimeJobEntityResponseSqlSugarPagedList;
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

export interface TenantGameTimeJobEntitySelectPostData {
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

export interface TenantGameTimeJobEntitySelectStringPostData {
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

export type TenantGameTimeJobEntityUpdatePostData = any;
