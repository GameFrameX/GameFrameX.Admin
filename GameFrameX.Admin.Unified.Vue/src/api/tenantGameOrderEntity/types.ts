/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum OrderStatusEnum {
  /** 未付款 */
  Unpaid = 1,
  /** 未发货 */
  NotShipped = 2,
  /** 已发货 */
  Shipped = 3,
  /** 发起退款 */
  InitiateRefund = 4,
  /** 退款拒绝 */
  RefundRefused = 5,
  /** 同意退款 */
  RefundAgreement = 6,
  /** 已退款 */
  Refunded = 7,
  /** 已完成 */
  Completed = 8,
  /** 已作废 */
  Voided = 9,
}

/** 游戏支付订单增加输入参数 */
export interface AddTenantGameOrderEntityRequest {
  /** 主键Id */
  id: string;
}

/** 支付渠道汇总 */
export interface GameOrderChannelBreakdownResponse {
  payChannel?: string | null;
  /** @format int64 */
  amount?: number | null;
  /** @format int32 */
  count?: number | null;
}

/** 订单状态汇总 */
export interface GameOrderStatusBreakdownResponse {
  status?: OrderStatusEnum;
  /** @format int64 */
  amount?: number | null;
  /** @format int32 */
  count?: number | null;
}

/** 订单统计输出 */
export interface GameOrderSummaryResponse {
  /**
   * 匹配订单总金额(分)
   * @format int64
   */
  totalAmount?: number;
  /**
   * 匹配订单总数
   * @format int32
   */
  totalCount?: number;
  /**
   * 已付款订单总金额(分)
   * @format int64
   */
  paidAmount?: number;
  /**
   * 已付款订单总数
   * @format int32
   */
  paidCount?: number;
  /** 按支付渠道金额汇总 */
  payChannelBreakdown?: GameOrderChannelBreakdownResponse[] | null;
  /** 按订单状态金额汇总 */
  statusBreakdown?: GameOrderStatusBreakdownResponse[] | null;
}

/** 游戏支付订单分页查询输入参数 */
export interface PageTenantGameOrderEntityRequest {
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
  status?: OrderStatusEnum;
  /** 用户 */
  roleId?: string | null;
  /**
   * 金钱数量
   * @format int32
   */
  amount?: number | null;
  /** 内容 */
  content?: string | null;
  /** 第三方订单id */
  orderId?: string | null;
  /**
   * 商品
   * @format int64
   */
  sku?: number | null;
  /** 支付渠道 */
  payChannel?: string | null;
  /** 客户端所属渠道 */
  channel?: string | null;
  /** 全局平台 */
  platform?: string | null;
  /**
   * 创建时间
   * @format int64
   */
  createTime?: number | null;
  /** 创建时间范围 */
  createTimeRange?: (number | null)[];
  /** 是否为沙盒 */
  isSandbox?: boolean | null;
  /** 所属账号 */
  accountId?: string | null;
  /** 应用Id。应用级订单分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
}

/** 游戏支付订单输出参数 */
export interface TenantGameOrderEntityResponse {
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
  appId?: string;
  status?: OrderStatusEnum;
  roleId?: string | null;
  /** @format int32 */
  amount?: number;
  content?: string | null;
  areaId?: string;
  orderId?: string | null;
  /** @format int64 */
  sku?: number;
  productId?: string | null;
  payChannel?: string | null;
  channel?: string | null;
  subChannel?: string | null;
  platform?: string | null;
  country?: string | null;
  isSandbox?: boolean | null;
  adventureSuffix?: string | null;
  accountId?: string | null;
  /** @format int64 */
  roleRegisterTime?: number | null;
  appleTransactionId?: string | null;
  appleOriginalTransactionId?: string | null;
  appleReceiptEnvironment?: string | null;
  /** @format int64 */
  appleVerifiedTime?: number | null;
  providerTransactionId?: string | null;
  receiptJson?: string | null;
  /** @format int64 */
  realMoney?: number | null;
  currency?: string | null;
  /** @format int32 */
  level?: number | null;
  strScene?: string | null;
}

export interface TenantGameOrderEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameOrderEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏支付订单更新输入参数 */
export interface UpdateTenantGameOrderEntityRequest {
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

export interface TenantGameOrderEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏支付订单输出参数 */
  data: TenantGameOrderEntityResponse;
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

export type TenantGameOrderEntityDeletePostData = any;

export interface TenantGameOrderEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏支付订单输出参数 */
  data: TenantGameOrderEntityResponse;
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

export interface TenantGameOrderEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameOrderEntityResponseSqlSugarPagedList;
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

export type TenantGameOrderEntitySendPostData = any;

export interface TenantGameOrderEntitySummaryPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 订单统计输出 */
  data: GameOrderSummaryResponse;
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

export type TenantGameOrderEntityUpdatePostData = any;
