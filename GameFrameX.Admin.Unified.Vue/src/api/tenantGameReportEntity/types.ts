/* eslint-disable */
/* tslint:disable */

import type { PenaltyTargetTypeEnum } from "../_shared/data-contracts";

/** @format int32 */
export enum ReportTypeEnum {
  /** 作弊外挂 */
  Cheat = 1,
  /** 辱骂骚扰 */
  Abuse = 2,
  /** 违规违法 */
  Illegal = 3,
  /** 刷屏广告 */
  Spam = 4,
  /** 其他 */
  Other = 5,
}

/** @format int32 */
export enum ReportStatusEnum {
  /** 待处理 */
  Pending = 1,
  /** 调查中 */
  Investigating = 2,
  /** 已坐实 */
  Confirmed = 3,
  /** 已驳回 */
  Dismissed = 4,
}

/** 举报新增输入参数。Status / HandlerId 服务端强制（Status=Pending，HandlerId=null）。 */
export interface AddTenantGameReportEntityRequest {
  /** 应用作用域。举报是应用级事实数据，必须大于 0。 */
  appId: string;
  /**
   * 举报人 UserId。
   * @min 1
   * @max 9223372036854776000
   */
  reporterId: string;
  targetType: PenaltyTargetTypeEnum;
  /**
   * 被举报对象标识。
   * @minLength 1
   * @maxLength 256
   */
  targetId: string;
  reportType: ReportTypeEnum;
  /**
   * 场景描述。
   * @maxLength 512
   */
  scene?: string | null;
  /**
   * 证据 JSON。
   * @minLength 1
   * @maxLength 8192
   */
  evidenceJson: string;
}

/** 举报关联处罚输入参数。仅 Confirmed 举报可关联；不自动创建处罚（创建走 TenantGamePenaltyEntityService.AddAsync）。 */
export interface LinkPenaltyTenantGameReportEntityRequest {
  /**
   * 举报 Id。
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * 处理结果产生的处罚 Id。
   * @min 1
   * @max 9223372036854776000
   */
  penaltyId: string;
}

/** 举报分页查询输入参数。 */
export interface PageTenantGameReportEntityRequest {
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
  /** 应用Id。应用级举报分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
  targetType?: PenaltyTargetTypeEnum;
  /** 被举报对象标识（可选过滤，精确匹配）。 */
  targetId?: string | null;
  reportType?: ReportTypeEnum;
  status?: ReportStatusEnum;
}

/** 举报输出参数（含处理人 / 结果处罚关联，供后台只读展示）。 */
export interface TenantGameReportEntityResponse {
  /** Id */
  id?: string;
  /** 应用作用域。举报是应用级事实数据，必须大于 0。 */
  appId?: string;
  /** 举报人 UserId。 */
  reporterId?: string;
  targetType?: PenaltyTargetTypeEnum;
  /** 被举报对象标识。 */
  targetId?: string | null;
  reportType?: ReportTypeEnum;
  /** 场景描述。 */
  scene?: string | null;
  /** 证据 JSON。 */
  evidenceJson?: string | null;
  status?: ReportStatusEnum;
  /** 处理人（CurrentUserContext.UserId）。 */
  handlerId?: string | null;
  /** 处理结果产生的处罚 Id。 */
  resultPenaltyId?: string | null;
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

export interface TenantGameReportEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameReportEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 举报更新输入参数。举报为事实源，不支持直接编辑；保留此类型仅为满足通用 CRUD 基类泛型约束。 */
export interface UpdateTenantGameReportEntityRequest {
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

export interface TenantGameReportEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 举报输出参数（含处理人 / 结果处罚关联，供后台只读展示）。 */
  data: TenantGameReportEntityResponse;
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

export type TenantGameReportEntityDeletePostData = any;

export interface TenantGameReportEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 举报输出参数（含处理人 / 结果处罚关联，供后台只读展示）。 */
  data: TenantGameReportEntityResponse;
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

export type TenantGameReportEntityLinkPenaltyPostData = any;

export interface TenantGameReportEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameReportEntityResponseSqlSugarPagedList;
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

export type TenantGameReportEntityUpdatePostData = any;
