/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum NoticeUserStatusEnum {
  /** 未读 */
  UNREAD = 0,
  /** 已读 */
  READ = 1,
}

/** @format int32 */
export enum NoticeTypeEnum {
  /** 通知 */
  NOTICE = 1,
  /** 公告 */
  ANNOUNCEMENT = 2,
}

/** @format int32 */
export enum NoticeStatusEnum {
  /** 草稿 */
  DRAFT = 0,
  /** 发布 */
  PUBLIC = 1,
  /** 撤回 */
  CANCEL = 2,
  /** 删除 */
  DELETED = 3,
}

export interface AddNoticeRequest {
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
  title?: string | null;
  content?: string | null;
  type?: NoticeTypeEnum;
  publicUserId?: string;
  publicUserName?: string | null;
  publicOrganizationId?: string;
  publicOrganizationName?: string | null;
  /** @format int64 */
  publicTime?: number | null;
  /** @format int64 */
  cancelTime?: number | null;
  status?: NoticeStatusEnum;
}

export interface DeleteNoticeRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface NoticeRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface PageNoticeRequest {
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
  /** 标题 */
  title?: string | null;
  type?: NoticeTypeEnum;
}

/** 系统通知公告输出参数 */
export interface TenantNoticeResponse {
  /** Id */
  id?: string;
  /** 标题 */
  title?: string | null;
  /** 内容 */
  content?: string | null;
  type?: NoticeTypeEnum;
  /** 发布人Id */
  publicUserId?: string;
  /** 发布人姓名 */
  publicUserName?: string | null;
  /** 发布机构Id */
  publicOrganizationId?: string;
  /** 发布机构名称 */
  publicOrganizationName?: string | null;
  /**
   * 发布时间
   * @format int64
   */
  publicTime?: number | null;
  /**
   * 撤回时间
   * @format int64
   */
  cancelTime?: number | null;
  status?: NoticeStatusEnum;
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

export interface TenantNoticeResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantNoticeResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 系统通知公告用户输出参数 */
export interface TenantNoticeUserResponse {
  /** Id */
  id?: string;
  /** 通知公告Id */
  noticeId?: string;
  /** 系统通知公告输出参数 */
  tenantNoticeEntity?: TenantNoticeResponse;
  /** 用户Id */
  userId?: string;
  /**
   * 阅读时间
   * @format int64
   */
  readTime?: number | null;
  readStatus?: NoticeUserStatusEnum;
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

export interface TenantNoticeUserResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantNoticeUserResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface UpdateNoticeRequest {
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
  title?: string | null;
  content?: string | null;
  type?: NoticeTypeEnum;
  publicUserId?: string;
  publicUserName?: string | null;
  publicOrganizationId?: string;
  publicOrganizationName?: string | null;
  /** @format int64 */
  publicTime?: number | null;
  /** @format int64 */
  cancelTime?: number | null;
  status?: NoticeStatusEnum;
}

export type TenantNoticeAddPostData = any;

export type TenantNoticeDeletePostData = any;

export interface TenantNoticePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantNoticeResponseSqlSugarPagedList;
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

export interface TenantNoticePageReceivedGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantNoticeUserResponseSqlSugarPagedList;
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

export type TenantNoticePublishPostData = any;

export type TenantNoticeSetReadPostData = any;

export interface TenantNoticeUnReadListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantNoticeResponse[];
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

export type TenantNoticeUpdatePostData = any;
