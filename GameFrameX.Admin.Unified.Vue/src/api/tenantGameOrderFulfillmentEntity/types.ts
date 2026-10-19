/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum FulfillmentTypeEnum {
  /** 购买发货 */
  Purchase = 1,
  /** 退款扣除 */
  Refund = 2,
  /** 人工补偿 */
  Compensate = 3,
}

/** @format int32 */
export enum FulfillmentStatusEnum {
  /** 发放意图 */
  Pending = 1,
  /** 发放成功 */
  Success = 2,
  /** 发放失败 */
  Failed = 3,
  /** 已作废 */
  Voided = 4,
}

/**
 * 权益发货账本高风险动作输入参数（C120）：RefundAsync / CompensateAsync / ResendAsync 共用。
 * 兼作 GenericCrudService 的 TAddRequest（AddAsync 端点已由 OnAddingAsync 抛错关闭，与 TenantGameOrderEntityService 复用 AddInput 于 SendAsync 同款）。
 */
export interface FulfillmentActionRequest {
  /**
   * 账本主键 Id（动作作用于账本行对应的订单）。
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 操作原因。RefundAsync 并入账本 FailureReason；CompensateAsync/ResendAsync 失败时落账，成功不落。 */
  reason?: string | null;
}

/**
 * 权益发货账本高风险动作输出（C223 / GFX-644，U9 信封统一后为三 host 通用）：RefundAsync / CompensateAsync / ResendAsync 成功响应的 data 对象契约。
 * Admin 端点返回本对象交 AdminResultFilter 自动包装 HttpJsonResultData，失败走 BusinessException 统一错误路径。
 */
export interface FulfillmentActionResponse {
  /** 动作作用的订单 Id（取自账本行对应订单）。 */
  orderId?: string;
  type?: FulfillmentTypeEnum;
}

/** 权益发货账本分页查询输入参数（C120）。过滤字段严格锁定见 change C120 §3 BuildPageQuery 表。 */
export interface PageTenantGameOrderFulfillmentEntityRequest {
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
  /** 应用Id。应用级发货账本分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
  /** 来源订单 Id（账本 OrderId 为 long，非订单表的字符串 OrderId）。 */
  orderId?: string | null;
  /** 角色 Id。 */
  roleId?: string | null;
  /** 支付渠道/Provider（模糊匹配）。 */
  payChannel?: string | null;
  /** 第三方支付交易号（等值匹配）。 */
  providerTransactionId?: string | null;
  type?: FulfillmentTypeEnum;
  status?: FulfillmentStatusEnum;
  /**
   * 商品 Id。
   * @format int64
   */
  sku?: number | null;
  /** 账号。 */
  accountId?: string | null;
  /** 操作人（>0 人工 / =0 系统自动）。 */
  operatorId?: string | null;
  /** 创建时间范围（[0]=起始, [1]=结束）。 */
  createTimeRange?: (number | null)[];
}

/**
 * 权益发货账本输出参数（C120）。投影暴露实体全字段：
 * Type/Status/FailureReason/IdempotencyKey/OperatorId/ItemsJson/订单快照（OrderId/PayChannel/ProviderTransactionId/Sku/RoleId/AreaId/AccountId/Amount）/审计列。
 */
export interface TenantGameOrderFulfillmentEntityResponse {
  /** Id */
  id?: string;
  /** 应用Id。事实数据应用作用域（TenantId + AppId），跟随订单 AppId，必须 > 0。 */
  appId?: string;
  /** 来源订单 Id（TenantGameOrderEntity.Id）。 */
  orderId?: string;
  /** 支付渠道/Provider（alipay/apple/google/xsolla），来自订单。 */
  payChannel?: string | null;
  /** 第三方支付交易号，来自订单，组成幂等键。可空（无第三方交易号时幂等键退化为 order{OrderId}）。 */
  providerTransactionId?: string | null;
  /**
   * 商品 Id，来自订单 Sku。
   * @format int64
   */
  sku?: number;
  /** 角色 Id，来自订单。 */
  roleId?: string | null;
  /** 区服 Id，来自订单。 */
  areaId?: string;
  /** 账号，来自订单。 */
  accountId?: string | null;
  /**
   * 金额（分），来自订单。
   * @format int32
   */
  amount?: number;
  /** 道具明细 JSON（PropertyItemResponse[] 序列化），来自订单 Content 反序列化的 RewardsItem。 */
  itemsJson?: string | null;
  type?: FulfillmentTypeEnum;
  status?: FulfillmentStatusEnum;
  /** 幂等键。 */
  idempotencyKey?: string | null;
  /** 失败原因（仅 Failed 状态写）。 */
  failureReason?: string | null;
  /** 操作人。0=系统自动（支付回调触发）；>0=人工操作（Admin 用户 Id）。 */
  operatorId?: string;
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

export interface TenantGameOrderFulfillmentEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameOrderFulfillmentEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 权益发货账本更新输入参数（C120）。仅满足 GenericCrudService 的 TUpdateRequest 泛型约束；
 * UpdateAsync 端点已由 OnUpdatingAsync 抛错关闭（账本为事实数据，不支持修改）。
 */
export interface UpdateTenantGameOrderFulfillmentEntityRequest {
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

export interface TenantGameOrderFulfillmentEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 权益发货账本输出参数（C120）。投影暴露实体全字段：
   * Type/Status/FailureReason/IdempotencyKey/OperatorId/ItemsJson/订单快照（OrderId/PayChannel/ProviderTransactionId/Sku/RoleId/AreaId/AccountId/Amount）/审计列。
   */
  data: TenantGameOrderFulfillmentEntityResponse;
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

export interface TenantGameOrderFulfillmentEntityCompensatePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 权益发货账本高风险动作输出（C223 / GFX-644，U9 信封统一后为三 host 通用）：RefundAsync / CompensateAsync / ResendAsync 成功响应的 data 对象契约。
   * Admin 端点返回本对象交 AdminResultFilter 自动包装 HttpJsonResultData，失败走 BusinessException 统一错误路径。
   */
  data: FulfillmentActionResponse;
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

export type TenantGameOrderFulfillmentEntityDeletePostData = any;

export interface TenantGameOrderFulfillmentEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 权益发货账本输出参数（C120）。投影暴露实体全字段：
   * Type/Status/FailureReason/IdempotencyKey/OperatorId/ItemsJson/订单快照（OrderId/PayChannel/ProviderTransactionId/Sku/RoleId/AreaId/AccountId/Amount）/审计列。
   */
  data: TenantGameOrderFulfillmentEntityResponse;
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

export interface TenantGameOrderFulfillmentEntityExportPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameOrderFulfillmentEntityResponse[];
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

export interface TenantGameOrderFulfillmentEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameOrderFulfillmentEntityResponseSqlSugarPagedList;
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

export interface TenantGameOrderFulfillmentEntityRefundPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 权益发货账本高风险动作输出（C223 / GFX-644，U9 信封统一后为三 host 通用）：RefundAsync / CompensateAsync / ResendAsync 成功响应的 data 对象契约。
   * Admin 端点返回本对象交 AdminResultFilter 自动包装 HttpJsonResultData，失败走 BusinessException 统一错误路径。
   */
  data: FulfillmentActionResponse;
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

export interface TenantGameOrderFulfillmentEntityResendPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 权益发货账本高风险动作输出（C223 / GFX-644，U9 信封统一后为三 host 通用）：RefundAsync / CompensateAsync / ResendAsync 成功响应的 data 对象契约。
   * Admin 端点返回本对象交 AdminResultFilter 自动包装 HttpJsonResultData，失败走 BusinessException 统一错误路径。
   */
  data: FulfillmentActionResponse;
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

export type TenantGameOrderFulfillmentEntityUpdatePostData = any;
