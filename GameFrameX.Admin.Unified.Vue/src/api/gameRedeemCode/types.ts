/* eslint-disable */
/* tslint:disable */

/** 兑换码增加输入参数 */
export interface AddGameRedeemCodeEntityRequest {
  /** 主键Id */
  id: string;
  /** 区服ID */
  areaId: string;
  /**
   * 兑换码
   * @minLength 1
   */
  code: string;
  /**
   * 描述
   * @minLength 1
   */
  description: string;
  /** 是否激活 */
  isActive: boolean;
  /**
   * 可使用次数
   * @format int32
   */
  usageLimit: number;
  /**
   * 已使用次数
   * @format int32
   */
  usedCount: number;
  /**
   * 有效开始时间
   * @format int64
   */
  startTime: number;
  /**
   * 失效时间
   * @format int64
   */
  endTime: number;
  /**
   * 礼包内容
   * @minLength 1
   */
  contents: string;
  /** 兑换码渠道ID */
  channelId: string;
}

/** 兑换码输出参数 */
export interface GameRedeemCodeResponse {
  /** Id */
  id?: string;
  /** 区服ID */
  areaId?: string;
  /** 兑换码渠道ID */
  channelId?: string | null;
  /** 礼包码的唯一字符串。 */
  code?: string | null;
  /** 对礼包内容的描述。 */
  description?: string | null;
  /** 表示礼包码是否有效。 */
  isActive?: boolean;
  /**
   * 礼包码的可使用次数限制。
   * @format int32
   */
  usageLimit?: number;
  /**
   * 现在已使用次数。
   * @format int32
   */
  usedCount?: number;
  /**
   * 礼包码有效的开始时间。
   * @format int64
   */
  startTime?: number;
  /**
   * 礼包码有效的结束时间。
   * @format int64
   */
  endTime?: number;
  /** 礼包中包含的物品或权益的详细列表。 */
  contents?: string | null;
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

export interface GameRedeemCodeResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameRedeemCodeResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 兑换码分页查询输入参数 */
export interface PageGameRedeemCodeEntityRequest {
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
  /** 兑换码 */
  code?: string | null;
  /** 是否激活 */
  isActive?: boolean | null;
  /**
   * 可使用次数
   * @format int32
   */
  usageLimit?: number | null;
  /**
   * 已使用次数
   * @format int32
   */
  usedCount?: number | null;
  /** 兑换码渠道ID */
  channelId?: string | null;
}

/** 兑换码更新输入参数 */
export interface UpdateGameRedeemCodeEntityRequest {
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
   * 描述
   * @minLength 1
   */
  description: string;
}

export interface GameRedeemCodeAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换码输出参数 */
  data: GameRedeemCodeResponse;
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

export type GameRedeemCodeDeletePostData = any;

export interface GameRedeemCodeDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 兑换码输出参数 */
  data: GameRedeemCodeResponse;
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

export interface GameRedeemCodePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameRedeemCodeResponseSqlSugarPagedList;
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

export type GameRedeemCodeUpdatePostData = any;
