/* eslint-disable */
/* tslint:disable */

/** 游戏商品交易增加输入参数 */
export interface AddGameCommodityEntityRequest {
  /**
   * 状态
   * @minLength 1
   */
  status: string;
  /** 类别Id。引用 GameGoodsCatalogueEntity.Id；Id 后缀契约兼容数字/字符串提交。 */
  categoryId: string;
  /** 多少rmb */
  cost: boolean;
  /**
   * 数量
   * @format int64
   */
  number: number;
}

/** 游戏商品交易输出参数 */
export interface GameCommodityResponse {
  /** Id */
  id?: string;
  /** Status */
  status?: string | null;
  /** 类别Id（出站按 Id 后缀契约字符串化，前端与目录字典严格相等匹配不受影响） */
  categoryId?: string;
  /** Cost */
  cost?: boolean;
  /**
   * Number
   * @format int64
   */
  number?: number;
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

export interface GameCommodityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameCommodityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏商品交易分页查询输入参数 */
export interface PageGameCommodityEntityRequest {
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
  /** 状态 */
  status?: string | null;
  /** 类别Id（可选过滤，等值匹配） */
  categoryId?: string | null;
  /** 多少rmb */
  cost?: boolean | null;
  /**
   * 数量（搜索表单为文本输入，宽容读取字符串提交——）
   * @format int64
   */
  number?: number | null;
  /** 创建者姓名 */
  createdName?: string | null;
  /** 修改者姓名 */
  updatedName?: string | null;
}

/** 游戏商品交易更新输入参数 */
export interface UpdateGameCommodityEntityRequest {
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
   * 创建者姓名
   * @minLength 1
   */
  createdName: string;
  /**
   * 修改者姓名
   * @minLength 1
   */
  updatedName: string;
}

export interface GameCommodityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏商品交易输出参数 */
  data: GameCommodityResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export type GameCommodityDeletePostData = any;

export interface GameCommodityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏商品交易输出参数 */
  data: GameCommodityResponse;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export interface GameCommodityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameCommodityResponseSqlSugarPagedList;
  /** 链路追踪标识，未启用追踪时为 null */
  trackId: string | null;
  /** 已退役字段：统一错误码体系下恒为 null，数字业务错误码承载在 code 字段 */
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

export type GameCommodityUpdatePostData = any;
