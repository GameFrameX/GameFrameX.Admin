/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse, GameMailType } from "../_shared/data-contracts";

/** 邮件模板增加输入参数 */
export interface AddTenantGameMailTemplateEntityRequest {
  /** 应用Id。邮件模板属于应用内容，必须 > 0。 */
  appId: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /** 详细描述 */
  description?: string | null;
  /** 分类Id */
  categoryId: string;
  /**
   * 标题模板
   * @minLength 1
   */
  titleTemplate: string;
  /** 副标题模板 */
  subTitleTemplate?: string | null;
  /**
   * 内容模板
   * @minLength 1
   */
  bodyTemplate: string;
  /** 默认附件JSON */
  defaultAttachmentsJson?: string | null;
  defaultMailType?: GameMailType;
  /**
   * 默认过期天数
   * @format int32
   */
  defaultExpirationDays?: number | null;
  /**
   * 排序
   * @format int32
   */
  sort?: number;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
  /** 主键Id */
  id: string;
}

/** 邮件模板分页查询输入参数 */
export interface PageTenantGameMailTemplateEntityRequest {
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
  /** 应用Id */
  appId?: string | null;
  /** 名称 */
  name?: string | null;
  /** 分类Id */
  categoryId?: string | null;
  /** 是否启用 */
  isEnabled?: boolean | null;
}

/** 邮件模板输出参数 */
export interface TenantGameMailTemplateEntityResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 应用Id。应用内容作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 分类Id */
  categoryId?: string;
  /** 标题模板 */
  titleTemplate?: string | null;
  /** 副标题模板 */
  subTitleTemplate?: string | null;
  /** 内容模板（支持{{变量名}}占位符） */
  bodyTemplate?: string | null;
  /** 默认附件JSON */
  defaultAttachmentsJson?: string | null;
  defaultMailType?: GameMailType;
  /**
   * 默认过期天数
   * @format int32
   */
  defaultExpirationDays?: number | null;
  /**
   * 排序
   * @format int32
   */
  sort?: number;
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
  /** 分类名称 */
  categoryIdName?: string | null;
}

export interface TenantGameMailTemplateEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameMailTemplateEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 邮件模板更新输入参数 */
export interface UpdateTenantGameMailTemplateEntityRequest {
  /** 应用Id。邮件模板属于应用内容，必须 > 0。 */
  appId: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /** 详细描述 */
  description?: string | null;
  /** 分类Id */
  categoryId: string;
  /**
   * 标题模板
   * @minLength 1
   */
  titleTemplate: string;
  /** 副标题模板 */
  subTitleTemplate?: string | null;
  /**
   * 内容模板
   * @minLength 1
   */
  bodyTemplate: string;
  /** 默认附件JSON */
  defaultAttachmentsJson?: string | null;
  defaultMailType?: GameMailType;
  /**
   * 默认过期天数
   * @format int32
   */
  defaultExpirationDays?: number | null;
  /**
   * 排序
   * @format int32
   */
  sort?: number;
  /** 是否启用,true:启用，false:禁用，null:未设置(启用) */
  isEnabled?: boolean | null;
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

export interface TenantGameMailTemplateEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 邮件模板输出参数 */
  data: TenantGameMailTemplateEntityResponse;
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

export type TenantGameMailTemplateEntityDeletePostData = any;

export interface TenantGameMailTemplateEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 邮件模板输出参数 */
  data: TenantGameMailTemplateEntityResponse;
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

export interface TenantGameMailTemplateEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameMailTemplateEntityResponseSqlSugarPagedList;
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

export interface TenantGameMailTemplateEntitySelectPostData {
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

export interface TenantGameMailTemplateEntitySelectStringPostData {
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

export type TenantGameMailTemplateEntityUpdatePostData = any;
