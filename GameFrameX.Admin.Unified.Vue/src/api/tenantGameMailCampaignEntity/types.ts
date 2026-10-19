/* eslint-disable */
/* tslint:disable */

import type { OnlineLiveOpsDispatchResponse } from "../_shared/data-contracts";

/** @format int32 */
export enum MailCampaignServerCode {
  /** Ok */
  Ok = 0,
  /** HasExist */
  HasExist = 1,
  /** InvalidCampaignParameter */
  InvalidCampaignParameter = 2,
  /** CampaignNotFound */
  CampaignNotFound = 3,
  /** CampaignAlreadyRevoked */
  CampaignAlreadyRevoked = 4,
  /** InvalidReward */
  InvalidReward = 5,
  /** Unsupported */
  Unsupported = 6,
  /** NetworkError */
  NetworkError = 7,
}

/** @format int32 */
export enum MailCampaignPublishStatus {
  /** Draft */
  Draft = 0,
  /** Published */
  Published = 1,
  /** Revoked */
  Revoked = 2,
  /** Failed */
  Failed = 3,
}

/** @format int32 */
export enum MailCampaignMailType {
  /** System */
  System = 0,
  /** Operation */
  Operation = 1,
  /** Compensation */
  Compensation = 2,
}

/** @format int32 */
export enum MailCampaignExpirePolicy {
  /** DiscardUnclaimed */
  DiscardUnclaimed = 0,
  /** KeepUnclaimed */
  KeepUnclaimed = 1,
  /** AutoClaim */
  AutoClaim = 2,
}

/** 新增 Campaign 输入。 */
export interface AddTenantGameMailCampaignEntityRequest {
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
  /** 应用Id。事实数据应用作用域，必须 > 0。 */
  appId: string;
  /** 区服Id。用于解析目标游戏服 HttpManageUrl 调用 Server 发布接口。 */
  areaId: string;
  /**
   * Campaign 名称（Admin 侧识别用）。
   * @minLength 1
   */
  name: string;
  /** 模板 Id（0 = 无模板，裸文案发布）。 */
  templateId?: string;
  mailType?: MailCampaignMailType;
  /**
   * 多语言标题 JSON（ListAsync<MailLocalizedContent>）。
   * @minLength 1
   */
  titlesJson: string;
  /** 多语言正文 JSON（ListAsync<MailLocalizedContent>）。 */
  contentsJson?: string | null;
  /** 多语言发件人名称 JSON（ListAsync<MailLocalizedContent>）。 */
  senderNamesJson?: string | null;
  /** 附件列表 JSON（ListAsync<MailCampaignAttachment>）。 */
  attachmentsJson?: string | null;
  /** 服务器 Id 白名单 JSON（ListAsync<int>）。 */
  serverIdsJson?: string | null;
  /** 渠道 Id 白名单 JSON（ListAsync<int>）。 */
  channelIdsJson?: string | null;
  /**
   * 最低等级（含），0 表示不限。
   * @format int32
   */
  minLevel?: number;
  /**
   * 最高等级（含），0 表示不限。
   * @format int32
   */
  maxLevel?: number;
  expireAttachmentPolicy?: MailCampaignExpirePolicy;
  /**
   * 过期时间（Unix 秒，UTC），0 表示永不过期。
   * @format int64
   */
  expireTime?: number;
  /** 备注。 */
  remark?: string | null;
}

/** Campaign 分页查询输入。 */
export interface PageTenantGameMailCampaignEntityRequest {
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
  /** 应用Id（可选过滤，按当前应用筛选）。 */
  appId?: string | null;
  /** 名称模糊过滤。 */
  name?: string | null;
  mailType?: MailCampaignMailType;
  publishStatus?: MailCampaignPublishStatus;
  /** 模板 Id 过滤。 */
  templateId?: string | null;
  /** Server CampaignId 过滤。 */
  serverCampaignId?: string | null;
  /** 区服Id过滤。 */
  areaId?: string | null;
}

/** 预览输入（Campaign Id）。 */
export interface PreviewMailCampaignEntityRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/** 预览输出。 */
export interface PreviewMailCampaignEntityResponse {
  code?: MailCampaignServerCode;
  /**
   * 预估命中玩家数（一期 -1 = 未知）。
   * @format int64
   */
  estimatedHitCount?: number;
  /** 提示信息。 */
  message?: string | null;
}

/** 发布输入（Campaign Id）。 */
export interface PublishMailCampaignEntityRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/** 发布输出。 */
export interface PublishMailCampaignEntityResponse {
  code?: MailCampaignServerCode;
  /** Server 分配的 CampaignId。 */
  campaignId?: string;
  /**
   * 版本号。
   * @format int32
   */
  version?: number;
  /**
   * 发布时间（Unix 秒）。
   * @format int64
   */
  publishedTime?: number;
  /** 提示信息。 */
  message?: string | null;
}

/** 撤回输入（Campaign Id）。 */
export interface RevokeMailCampaignEntityRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/** 撤回输出。 */
export interface RevokeMailCampaignEntityResponse {
  code?: MailCampaignServerCode;
  /** CampaignId。 */
  campaignId?: string;
  /**
   * 撤回时间（Unix 秒）。
   * @format int64
   */
  revokedTime?: number;
  /** 提示信息。 */
  message?: string | null;
}

/** Campaign 输出 DTO。 */
export interface TenantGameMailCampaignEntityResponse {
  /** 主键Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 区服Id。 */
  areaId?: string;
  /** 名称。 */
  name?: string | null;
  /** 模板Id。 */
  templateId?: string;
  mailType?: MailCampaignMailType;
  /** 多语言标题 JSON。 */
  titlesJson?: string | null;
  /** 多语言正文 JSON。 */
  contentsJson?: string | null;
  /** 多语言发件人名称 JSON。 */
  senderNamesJson?: string | null;
  /** 附件列表 JSON。 */
  attachmentsJson?: string | null;
  /** 服务器Id白名单 JSON。 */
  serverIdsJson?: string | null;
  /** 渠道Id白名单 JSON。 */
  channelIdsJson?: string | null;
  /**
   * 最低等级。
   * @format int32
   */
  minLevel?: number;
  /**
   * 最高等级。
   * @format int32
   */
  maxLevel?: number;
  expireAttachmentPolicy?: MailCampaignExpirePolicy;
  /**
   * 过期时间（Unix 秒）。
   * @format int64
   */
  expireTime?: number;
  publishStatus?: MailCampaignPublishStatus;
  /** Server CampaignId。 */
  serverCampaignId?: string;
  /**
   * 发布版本号。
   * @format int32
   */
  publishVersion?: number;
  /**
   * 发布时间（Unix 秒）。
   * @format int64
   */
  publishedTime?: number;
  /**
   * 撤回时间（Unix 秒）。
   * @format int64
   */
  revokedTime?: number;
  /** 发布操作人。 */
  publishOperator?: string | null;
  /** 撤回操作人。 */
  revokeOperator?: string | null;
  lastServerCode?: MailCampaignServerCode;
  /** 最近Server消息。 */
  lastServerMessage?: string | null;
  /** 备注。 */
  remark?: string | null;
  /**
   * 创建时间。
   * @format int64
   */
  createdTime?: number;
  /** 行版本号（乐观并发）。 */
  rowVersion?: string | null;
}

export interface TenantGameMailCampaignEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameMailCampaignEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 更新 Campaign 输入。仅 Draft 可更新（Published/Revoked 主体不可变 B1）。 */
export interface UpdateTenantGameMailCampaignEntityRequest {
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
  /** 应用Id。事实数据应用作用域，必须 > 0。 */
  appId: string;
  /** 区服Id。用于解析目标游戏服 HttpManageUrl 调用 Server 发布接口。 */
  areaId: string;
  /**
   * Campaign 名称（Admin 侧识别用）。
   * @minLength 1
   */
  name: string;
  /** 模板 Id（0 = 无模板，裸文案发布）。 */
  templateId?: string;
  mailType?: MailCampaignMailType;
  /**
   * 多语言标题 JSON（ListAsync<MailLocalizedContent>）。
   * @minLength 1
   */
  titlesJson: string;
  /** 多语言正文 JSON（ListAsync<MailLocalizedContent>）。 */
  contentsJson?: string | null;
  /** 多语言发件人名称 JSON（ListAsync<MailLocalizedContent>）。 */
  senderNamesJson?: string | null;
  /** 附件列表 JSON（ListAsync<MailCampaignAttachment>）。 */
  attachmentsJson?: string | null;
  /** 服务器 Id 白名单 JSON（ListAsync<int>）。 */
  serverIdsJson?: string | null;
  /** 渠道 Id 白名单 JSON（ListAsync<int>）。 */
  channelIdsJson?: string | null;
  /**
   * 最低等级（含），0 表示不限。
   * @format int32
   */
  minLevel?: number;
  /**
   * 最高等级（含），0 表示不限。
   * @format int32
   */
  maxLevel?: number;
  expireAttachmentPolicy?: MailCampaignExpirePolicy;
  /**
   * 过期时间（Unix 秒，UTC），0 表示永不过期。
   * @format int64
   */
  expireTime?: number;
  /** 备注。 */
  remark?: string | null;
  publishStatus?: MailCampaignPublishStatus;
  lastServerCode?: MailCampaignServerCode;
  lastServerMessage?: string | null;
}

export interface TenantGameMailCampaignEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** Campaign 输出 DTO。 */
  data: TenantGameMailCampaignEntityResponse;
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

export type TenantGameMailCampaignEntityDeletePostData = any;

export interface TenantGameMailCampaignEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** Campaign 输出 DTO。 */
  data: TenantGameMailCampaignEntityResponse;
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

export interface TenantGameMailCampaignEntityDispatchOnlinePostData {
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

export interface TenantGameMailCampaignEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameMailCampaignEntityResponseSqlSugarPagedList;
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

export interface TenantGameMailCampaignEntityPreviewPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 预览输出。 */
  data: PreviewMailCampaignEntityResponse;
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

export interface TenantGameMailCampaignEntityPublishPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 发布输出。 */
  data: PublishMailCampaignEntityResponse;
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

export interface TenantGameMailCampaignEntityRevokePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 撤回输出。 */
  data: RevokeMailCampaignEntityResponse;
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

export type TenantGameMailCampaignEntityUpdatePostData = any;
