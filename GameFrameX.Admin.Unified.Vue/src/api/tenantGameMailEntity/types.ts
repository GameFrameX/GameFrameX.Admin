/* eslint-disable */
/* tslint:disable */

import type { GameMailStatus, GameMailType } from "../_shared/data-contracts";

/** 游戏邮件增加输入参数 */
export interface AddTenantGameMailEntityRequest {
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
  /**
   * 应用Id。事实数据应用作用域，必须 > 0。
   *
   * 过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。
   */
  appId: string;
  /**
   * 标题
   * @minLength 1
   */
  title: string;
  /**
   * 副标题
   * @minLength 1
   */
  subTitle: string;
  /**
   * 详细内容
   * @minLength 1
   */
  body: string;
  /**
   * 玩家ID列表
   * @minLength 1
   */
  playerIdListJson: string;
  /** 是否发全部用户 */
  isAllPost: boolean;
  /** 是否本地化 */
  isTranslate?: boolean;
  /**
   * 区服ID。此前仅 AddRequest 声明：update 提交的 areaId 被绑定丢弃，Adapt 后实体 0 值穿透
   * IgnoreColumns(忽略 null) 不拦 0，导致编辑一次就把区服/类型/过期时间清零。
   * @minLength 1
   */
  areaId: string;
  /**
   * 附件列表
   * @minLength 1
   */
  attachmentsJson: string;
  /**
   * 邮件的过期日期
   * @format int64
   */
  expirationTime: number;
  mailType: GameMailType;
}

/** 游戏邮件分页查询输入参数 */
export interface PageTenantGameMailEntityRequest {
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
  /** 标题 */
  title?: string | null;
  /** 是否发全部用户 */
  isAllPost?: boolean | null;
  status?: GameMailStatus;
  mailType?: GameMailType;
  /** 应用Id。应用级邮件分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
}

/** 游戏邮件输出参数 */
export interface TenantGameMailEntityResponse {
  /** Id */
  id?: string;
  /** 应用Id。事实数据应用作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 区服ID */
  areaId?: string;
  /** 接收者的唯一标识符。 */
  playerIdListJson?: string | null;
  /** 是否发全部用户。 */
  isAllPost?: boolean;
  /** 邮件的标题或主题。 */
  title?: string | null;
  /** 副标题。 */
  subTitle?: string | null;
  /** 邮件的详细信息内容。 */
  body?: string | null;
  /** 附件列表 */
  attachmentsJson?: string | null;
  status?: GameMailStatus;
  /**
   * 邮件的过期日期。
   * @format int64
   */
  expirationTime?: number;
  mailType?: GameMailType;
  /** 邮件是否本地化 */
  isTranslate?: boolean | null;
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

export interface TenantGameMailEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameMailEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏邮件更新输入参数 */
export interface UpdateTenantGameMailEntityRequest {
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
  /**
   * 应用Id。事实数据应用作用域，必须 > 0。
   *
   * 过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。
   */
  appId: string;
  /**
   * 标题
   * @minLength 1
   */
  title: string;
  /**
   * 副标题
   * @minLength 1
   */
  subTitle: string;
  /**
   * 详细内容
   * @minLength 1
   */
  body: string;
  /**
   * 玩家ID列表
   * @minLength 1
   */
  playerIdListJson: string;
  /** 是否发全部用户 */
  isAllPost: boolean;
  /** 是否本地化 */
  isTranslate?: boolean;
  /**
   * 区服ID。此前仅 AddRequest 声明：update 提交的 areaId 被绑定丢弃，Adapt 后实体 0 值穿透
   * IgnoreColumns(忽略 null) 不拦 0，导致编辑一次就把区服/类型/过期时间清零。
   * @minLength 1
   */
  areaId: string;
  /**
   * 附件列表
   * @minLength 1
   */
  attachmentsJson: string;
  /**
   * 邮件的过期日期
   * @format int64
   */
  expirationTime: number;
  mailType: GameMailType;
}

export interface TenantGameMailEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏邮件输出参数 */
  data: TenantGameMailEntityResponse;
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

export type TenantGameMailEntityDeletePostData = any;

export interface TenantGameMailEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏邮件输出参数 */
  data: TenantGameMailEntityResponse;
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

export interface TenantGameMailEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameMailEntityResponseSqlSugarPagedList;
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

export type TenantGameMailEntitySendPostData = any;

export type TenantGameMailEntityUpdatePostData = any;
