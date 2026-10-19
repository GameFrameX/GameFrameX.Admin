/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum AppealStatusEnum {
  /** 待审核 */
  Pending = 1,
  /** 审核通过 */
  Approved = 2,
  /** 审核驳回 */
  Rejected = 3,
}

/** 申诉新增输入参数。AppId 由服务端从关联处罚回填，不在此暴露。 */
export interface AddTenantGamePenaltyAppealEntityRequest {
  /**
   * 关联处罚 Id（必须为生效中处罚）。
   * @min 1
   * @max 9223372036854776000
   */
  penaltyId: string;
  /**
   * 申诉人 = 被处罚账号 UserId。
   * @min 1
   * @max 9223372036854776000
   */
  appellantId: string;
  /**
   * 申诉理由。
   * @minLength 1
   * @maxLength 2048
   */
  reason: string;
}

/** 申诉分页查询输入参数。 */
export interface PageTenantGamePenaltyAppealEntityRequest {
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
  /** 应用Id。应用级处罚申诉分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
  /** 关联处罚 Id（可选过滤）。 */
  penaltyId?: string | null;
  status?: AppealStatusEnum;
}

/** 申诉审核输入参数。Approve=true 通过（同步撤销关联处罚）；Approve=false 驳回。 */
export interface ReviewTenantGamePenaltyAppealEntityRequest {
  /**
   * 申诉 Id。
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 是否通过。true=审核通过（撤销关联处罚）；false=审核驳回。 */
  approve: boolean;
  /**
   * 审核备注。
   * @maxLength 2048
   */
  remark?: string | null;
}

/** 申诉输出参数（含审核审计字段，供后台只读展示）。 */
export interface TenantGamePenaltyAppealEntityResponse {
  /** Id */
  id?: string;
  /** 应用作用域，跟随关联处罚 AppId。 */
  appId?: string;
  /** 关联处罚 Id（TenantGamePenaltyEntity.Id）。 */
  penaltyId?: string;
  /** 申诉人 = 被处罚账号 UserId。 */
  appellantId?: string;
  /** 申诉理由。 */
  reason?: string | null;
  status?: AppealStatusEnum;
  /** 审核人（CurrentUserContext.UserId）。 */
  reviewerId?: string | null;
  /** 审核备注。 */
  reviewRemark?: string | null;
  /**
   * 审核时间。
   * @format int64
   */
  reviewTime?: number | null;
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

export interface TenantGamePenaltyAppealEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGamePenaltyAppealEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 申诉更新输入参数。申诉为事实源，不支持直接编辑；保留此类型仅为满足通用 CRUD 基类泛型约束。 */
export interface UpdateTenantGamePenaltyAppealEntityRequest {
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

export interface TenantGamePenaltyAppealEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 申诉输出参数（含审核审计字段，供后台只读展示）。 */
  data: TenantGamePenaltyAppealEntityResponse;
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

export type TenantGamePenaltyAppealEntityDeletePostData = any;

export interface TenantGamePenaltyAppealEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 申诉输出参数（含审核审计字段，供后台只读展示）。 */
  data: TenantGamePenaltyAppealEntityResponse;
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

export interface TenantGamePenaltyAppealEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGamePenaltyAppealEntityResponseSqlSugarPagedList;
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

export type TenantGamePenaltyAppealEntityReviewPostData = any;

export type TenantGamePenaltyAppealEntityUpdatePostData = any;
