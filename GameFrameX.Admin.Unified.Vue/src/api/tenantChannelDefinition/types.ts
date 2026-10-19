/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse, ChannelDefinitionResponse, ChannelDefinitionResponseSqlSugarPagedList, ChannelScopeEnum, ChannelTypeEnum, PageChannelDefinitionRequest } from "../_shared/data-contracts";

/**
 * 导出范围枚举。
 * @format int32
 */
export enum ExportRangeEnum {
  /** All */
  All = 0,
  /** CurrentPage */
  CurrentPage = 1,
}

/** 可用渠道选择输出 */
export interface AvailableChannelDefinitionResponse {
  /** 渠道Id */
  id?: string;
  /** 渠道编码 */
  code?: string | null;
  /** 渠道名称 */
  name?: string | null;
  /** 显示名称 */
  displayName?: string | null;
  scope?: ChannelScopeEnum;
  channelType?: ChannelTypeEnum;
  /** 适用平台编码 */
  platformCode?: string | null;
}

/** 导出列定义。Field 为 TResponse 上的属性名，Header 为 Excel 列标题。 */
export interface ExportColumnRequest {
  /** 导出字段名，须与实体 Output DTO 的属性名匹配（忽略大小写）。 */
  field?: string | null;
  /** Excel 列标题；为空时回退为属性名。 */
  header?: string | null;
}

/**
 * 通用数据导出请求。实体标识由各业务 Service 的 Admin API 路由隐含
 * （URL = 实体），本 DTO 不携带 tableName 或任意表直查参数，
 * 以规避「任意表 + 自由 SQL」的越权攻击面。
 */
export interface PageChannelDefinitionRequestDataExportRequest {
  /** 渠道分页查询输入参数 */
  query?: PageChannelDefinitionRequest;
  /** 可见列白名单。仅这些字段会被导出；非白名单或敏感字段一律不输出。 */
  columns?: ExportColumnRequest[] | null;
  /** 导出范围枚举。 */
  range?: ExportRangeEnum;
}

export interface TenantChannelDefinitionAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 渠道输出参数 */
  data: ChannelDefinitionResponse;
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

export interface TenantChannelDefinitionAvailableGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: AvailableChannelDefinitionResponse[];
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

export type TenantChannelDefinitionDeletePostData = any;

export interface TenantChannelDefinitionDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 渠道输出参数 */
  data: ChannelDefinitionResponse;
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

export type TenantChannelDefinitionExportPostData = any;

export interface TenantChannelDefinitionPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: ChannelDefinitionResponseSqlSugarPagedList;
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

export interface TenantChannelDefinitionSelectPostData {
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

export interface TenantChannelDefinitionSelectStringPostData {
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

export type TenantChannelDefinitionUpdatePostData = any;
