/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse, GameMailStatus, GameMailType } from "../_shared/data-contracts";

/** 游戏商城订单实体新增输入参数 */
export interface AddGameWebShopOrderEntityRequest {
  /**
   * 商品名称
   * @minLength 1
   */
  name: string;
  /**
   * 商品图
   * @minLength 1
   */
  logo: string;
  /** 商品描述 */
  description?: string | null;
  /** 商品关键词 */
  keyWords?: string | null;
  /**
   * 零售价
   * @minLength 1
   */
  price: string;
  /**
   * 实际价格
   * @minLength 1
   */
  realPrice: string;
  /**
   * 道具列表
   * @minLength 1
   */
  itemJson: string;
  /** 是否上架 */
  isUp?: boolean;
  /**
   * 权重
   * @format int32
   */
  weight?: number;
}

/** 网页商城订单输出参数 */
export interface GameWebShopOrderResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 网页商城商品id */
  gameShopId?: string;
  /** 角色id */
  roleId?: string | null;
  /** 区服id */
  areaId?: string;
  /** 失败原因 */
  reason?: string | null;
  /** 是否已经补发 */
  orderStatus?: boolean;
  /** 订单id */
  orderId?: string | null;
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

export interface GameWebShopOrderResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameWebShopOrderResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏邮件分页查询输入参数 */
export interface PageGameWebShopOrderEntityRequest {
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

/** 游戏商城订单实体更新输入参数 */
export interface UpdateGameWebShopOrderEntityRequest {
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
   * 商品名称
   * @minLength 1
   */
  name: string;
  /**
   * 商品图
   * @minLength 1
   */
  logo: string;
  /** 商品描述 */
  description?: string | null;
  /** 商品关键词 */
  keyWords?: string | null;
  /**
   * 零售价
   * @minLength 1
   */
  price: string;
  /**
   * 实际价格
   * @minLength 1
   */
  realPrice: string;
  /**
   * 道具列表
   * @minLength 1
   */
  itemJson: string;
  /** 是否上架 */
  isUp?: boolean;
  /**
   * 权重
   * @format int32
   */
  weight?: number;
}

export interface GameWebShopOrderAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 网页商城订单输出参数 */
  data: GameWebShopOrderResponse;
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

export type GameWebShopOrderDeletePostData = any;

export interface GameWebShopOrderDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 网页商城订单输出参数 */
  data: GameWebShopOrderResponse;
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

export interface GameWebShopOrderPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameWebShopOrderResponseSqlSugarPagedList;
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

export type GameWebShopOrderRedoPostData = any;

export interface GameWebShopOrderSelectPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageResponse[];
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

export interface GameWebShopOrderSelectStringPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: BaseSelectPageStringResponse[];
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

export type GameWebShopOrderUpdatePostData = any;
