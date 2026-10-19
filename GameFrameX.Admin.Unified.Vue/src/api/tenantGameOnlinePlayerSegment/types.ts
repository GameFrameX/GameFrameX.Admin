/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum OnlinePlayerSegmentSyncStatus {
  /** 未同步 */
  NotSynced = 0,
  /** 已同步 */
  Synced = 1,
  /** 同步失败 */
  Failed = 2,
}

/** 新增玩家分群输入。 */
export interface AddTenantGameOnlinePlayerSegmentRequest {
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
  /**
   * 分群名称。
   * @minLength 1
   */
  name: string;
  /** 分群描述。 */
  description?: string | null;
  /** 分群条件 JSON（List<GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.OnlineSegmentCondition>；空 = 不限条件）。 */
  conditionsJson?: string | null;
}

/** 玩家分群分页查询输入。 */
export interface PageTenantGameOnlinePlayerSegmentRequest {
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
  /** 应用Id（事实数据必填，按当前应用筛选）。 */
  appId: string;
  /** 名称模糊过滤。 */
  name?: string | null;
  syncStatus?: OnlinePlayerSegmentSyncStatus;
}

/** 分群同步 Online 输入（来源 change C343 / vault:C8 S7.6）。 */
export interface SyncPlayerSegmentOnlineRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * 目标区服Id（Online 作用域 ServerId，经租户归属校验）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  /**
   * 同步理由（必填，审计）。
   * @minLength 1
   */
  reason: string;
}

/** 分群同步 Online 输出。 */
export interface SyncPlayerSegmentOnlineResponse {
  /** 分群Id。 */
  id?: string;
  syncStatus?: OnlinePlayerSegmentSyncStatus;
  /** Online 侧分群标识。 */
  onlineSegmentId?: string;
  /** Online 确认版本回执。 */
  confirmVersion?: string | null;
  /** 提示消息（Online 侧原文）。 */
  message?: string | null;
  /**
   * 同步时间（Unix 秒，UTC）。
   * @format int64
   */
  syncedTime?: number;
}

/** 玩家分群输出。 */
export interface TenantGameOnlinePlayerSegmentResponse {
  /** 分群Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 分群名称。 */
  name?: string | null;
  /** 分群描述。 */
  description?: string | null;
  /** 分群条件 JSON（List<GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.OnlineSegmentCondition>）。 */
  conditionsJson?: string | null;
  syncStatus?: OnlinePlayerSegmentSyncStatus;
  /**
   * 最近同步时间（Unix 秒）。
   * @format int64
   */
  lastSyncTime?: number;
  /** 最近同步区服Id。 */
  lastSyncServerId?: string;
  /** Online 侧分群标识。 */
  lastOnlineSegmentId?: string;
  /**
   * 最近同步结果码。
   * @format int32
   */
  lastServerCode?: number;
  /** 最近同步消息。 */
  lastServerMessage?: string | null;
  /**
   * 创建时间（Unix 秒）。
   * @format int64
   */
  createdTime?: number;
  /**
   * 更新时间（Unix 秒）。
   * @format int64
   */
  updateTime?: number;
  /** 乐观锁版本号（C228：GenericCrudService TResponse 必含，乐观锁编辑链路与默认排序依赖）。 */
  rowVersion?: string;
}

export interface TenantGameOnlinePlayerSegmentResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameOnlinePlayerSegmentResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 更新玩家分群输入（编辑定义后须重新 syncOnline 生效）。 */
export interface UpdateTenantGameOnlinePlayerSegmentRequest {
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
  /**
   * 分群名称。
   * @minLength 1
   */
  name: string;
  /** 分群描述。 */
  description?: string | null;
  /** 分群条件 JSON（List<GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.OnlineSegmentCondition>；空 = 不限条件）。 */
  conditionsJson?: string | null;
  syncStatus?: OnlinePlayerSegmentSyncStatus;
  /**
   * 最近同步时间（Unix 秒；服务端回填）。
   * @format int64
   */
  lastSyncTime?: number;
  /** 最近同步区服Id（服务端回填）。 */
  lastSyncServerId?: string;
  /** Online 侧分群标识（服务端回填）。 */
  lastOnlineSegmentId?: string;
  /**
   * 最近同步结果码（服务端回填）。
   * @format int32
   */
  lastServerCode?: number;
  /** 最近同步消息（服务端回填）。 */
  lastServerMessage?: string | null;
}

export interface TenantGameOnlinePlayerSegmentAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 玩家分群输出。 */
  data: TenantGameOnlinePlayerSegmentResponse;
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

export type TenantGameOnlinePlayerSegmentDeletePostData = any;

export interface TenantGameOnlinePlayerSegmentDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 玩家分群输出。 */
  data: TenantGameOnlinePlayerSegmentResponse;
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

export interface TenantGameOnlinePlayerSegmentPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameOnlinePlayerSegmentResponseSqlSugarPagedList;
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

export interface TenantGameOnlinePlayerSegmentSyncOnlinePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 分群同步 Online 输出。 */
  data: SyncPlayerSegmentOnlineResponse;
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

export type TenantGameOnlinePlayerSegmentUpdatePostData = any;
