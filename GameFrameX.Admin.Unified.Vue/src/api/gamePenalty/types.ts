/* eslint-disable */
/* tslint:disable */

import type { OnlinePenaltyLinkResponse, PenaltyTargetTypeEnum } from "../_shared/data-contracts";

/** @format int32 */
export enum PenaltyStatusEnum {
  /** 生效中 */
  Active = 1,
  /** 已撤销 */
  Revoked = 2,
  /** 已过期 */
  Expired = 3,
}

/** @format int32 */
export enum PenaltyActionTypeEnum {
  /** 封禁 */
  Ban = 1,
  /** 限制登录 */
  RestrictLogin = 2,
  /** 禁言 */
  Mute = 3,
  /** 限制交易 */
  RestrictTrade = 4,
  /** 限制匹配 */
  RestrictMatch = 5,
}

/** 处罚新增输入参数。审计字段（Status / OperatorId / RevokeAsync*）由服务端强制写入，不在此暴露。 */
export interface AddGamePenaltyEntityRequest {
  /** 区服作用域。null = 全区服生效。 */
  areaId?: string | null;
  targetType: PenaltyTargetTypeEnum;
  /**
   * 对象标识（AccountId / RoleId / 设备标识 / IP）。
   * @minLength 1
   * @maxLength 256
   */
  targetId: string;
  actionType: PenaltyActionTypeEnum;
  /**
   * 处罚原因。
   * @minLength 1
   * @maxLength 2048
   */
  reason: string;
  /**
   * 过期时间。null = 永久。
   * @format int64
   */
  expireTime?: number | null;
  /** 来源举报 Id（由举报触发时写）。 */
  sourceReportId?: string | null;
}

/** 处罚输出参数（包含全部审计字段，供后台只读展示）。 */
export interface GamePenaltyResponse {
  /** Id */
  id?: string;
  /** 区服作用域。null = 全区服生效。 */
  areaId?: string | null;
  targetType?: PenaltyTargetTypeEnum;
  /** 对象标识（AccountId / RoleId / 设备标识 / IP）。 */
  targetId?: string | null;
  actionType?: PenaltyActionTypeEnum;
  status?: PenaltyStatusEnum;
  /** 处罚原因。 */
  reason?: string | null;
  /**
   * 过期时间。null = 永久。
   * @format int64
   */
  expireTime?: number | null;
  /** 创建操作人（CurrentUserContext.UserId），>0。 */
  operatorId?: string;
  /** 撤销操作人。 */
  revokeOperatorId?: string | null;
  /** 撤销原因。 */
  revokeReason?: string | null;
  /**
   * 撤销时间。
   * @format int64
   */
  revokeTime?: number | null;
  /** 来源举报 Id（由举报触发时写）。 */
  sourceReportId?: string | null;
  /** 由申诉解除时写（关联申诉 Id）。 */
  sourceAppealId?: string | null;
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

export interface GamePenaltyResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GamePenaltyResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 处罚分页查询输入参数。 */
export interface PageGamePenaltyEntityRequest {
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
  targetType?: PenaltyTargetTypeEnum;
  /** 对象标识（可选过滤，精确匹配）。 */
  targetId?: string | null;
  status?: PenaltyStatusEnum;
  actionType?: PenaltyActionTypeEnum;
}

/** 撤销处罚输入参数。 */
export interface RevokeGamePenaltyEntityRequest {
  /**
   * 处罚 Id。
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * 撤销原因。
   * @minLength 1
   * @maxLength 2048
   */
  reason: string;
}

/** 处罚同步 Online 运行时输入参数（联动命令按 ActionType 服务端推导，不暴露给请求体）。 */
export interface SyncOnlineRuntimeGamePenaltyRequest {
  /**
   * 处罚事实源行 Id（必须处于生效中）。
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

/** 处罚更新输入参数。处罚为事实源，不支持直接编辑；保留此类型仅为满足通用 CRUD 基类泛型约束。 */
export interface UpdateGamePenaltyEntityRequest {
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

export interface GamePenaltyAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 处罚输出参数（包含全部审计字段，供后台只读展示）。 */
  data: GamePenaltyResponse;
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

export type GamePenaltyDeletePostData = any;

export interface GamePenaltyDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 处罚输出参数（包含全部审计字段，供后台只读展示）。 */
  data: GamePenaltyResponse;
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

export interface GamePenaltyPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GamePenaltyResponseSqlSugarPagedList;
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

export type GamePenaltyRevokePostData = any;

export interface GamePenaltySyncOnlineRuntimePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 处罚联动命令聚合响应（按联动目标区服集逐区服执行后的聚合结果）。 */
  data: OnlinePenaltyLinkResponse;
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

export type GamePenaltyUpdatePostData = any;
