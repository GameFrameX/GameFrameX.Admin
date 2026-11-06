/* eslint-disable */
/* tslint:disable */

import type { GameMailStatus, GameMailType } from "../_shared/data-contracts";

/** 游戏邮件增加输入参数 */
export interface AddGameMailEntityRequest {
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

/** 游戏邮件输出参数 */
export interface GameMailResponse {
  /** Id */
  id?: string;
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

export interface GameMailResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameMailResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏邮件分页查询输入参数 */
export interface PageGameMailEntityRequest {
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
}

/** 游戏邮件更新输入参数 */
export interface UpdateGameMailEntityRequest {
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

export interface GameMailAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏邮件输出参数 */
  data: GameMailResponse;
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

export type GameMailDeletePostData = any;

export interface GameMailDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏邮件输出参数 */
  data: GameMailResponse;
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

export interface GameMailPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameMailResponseSqlSugarPagedList;
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

export type GameMailSendPostData = any;

export type GameMailUpdatePostData = any;
