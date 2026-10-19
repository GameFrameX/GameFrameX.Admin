/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum AdvertisementRewardStatusEnum {
  /** 已认领，待发放 */
  Pending = 1,
  /** 发放成功 */
  Success = 2,
  /** 发放失败 */
  Failed = 3,
}

/**
 * 广告奖励发放账本新增输入参数。仅满足 GenericCrudService 的 TAddRequest 泛型约束；
 * AddAsync 端点已由 OnAddingAsync 抛错关闭（账本由广告回调链路产生，不支持管理后台直接创建）。
 */
export type AddTenantGameAdvertisementRewardEntityRequest = object;

/**
 * 广告奖励发放账本分页/导出查询输入参数（C114 / GFX-111 后端跟进）。
 *
 * 过滤字段对齐前端检索表单：Provider/Status/RoleId 常用 + TransactionId/AdvertisementUnit/AreaId/时间范围 高级。
 */
export interface PageTenantGameAdvertisementRewardEntityRequest {
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
  /** 应用Id。账本为事实数据应用作用域，分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。 */
  appId?: string | null;
  /** 广告平台 Provider（等值匹配，标准化值 TopOn / GoogleAdMob / UnityAds）。 */
  provider?: string | null;
  /** 平台交易号（等值匹配）。 */
  transactionId?: string | null;
  /** 角色 Id（等值匹配）。 */
  roleId?: string | null;
  /** 用户 Id。 */
  userId?: string | null;
  /** 广告位/单元（模糊匹配）。 */
  advertisementUnit?: string | null;
  status?: AdvertisementRewardStatusEnum;
  /** 创建时间范围（[0]=起始, [1]=结束）。 */
  createTimeRange?: (number | null)[];
}

/**
 * 广告奖励发放账本输出参数（C114 / GFX-111 后端跟进）。投影暴露实体全字段：
 * Provider/TransactionId/AdvertisementUnit/RoleId/UserId/AreaId/RewardItem/RewardAmount/Status/IdempotencyKey/FailureReason/IsSandbox + 审计列。
 */
export interface TenantGameAdvertisementRewardEntityResponse {
  /** Id */
  id?: string;
  /** 应用Id。事实数据应用作用域（TenantId + AppId），跟随角色 AppId。 */
  appId?: string;
  /** 广告平台 Provider（TopOn / GoogleAdMob / UnityAds）。 */
  provider?: string | null;
  /** 平台交易号。TopOn trans_id / AdMob transaction_id / UnityAds oid。 */
  transactionId?: string | null;
  /** 广告位/单元。UnityAds 无则空。 */
  advertisementUnit?: string | null;
  /** 角色 Id。反查自 TenantGameRoleEntity。 */
  roleId?: string | null;
  /** 用户 Id。来自平台 userId 参数。 */
  userId?: string;
  /** 区服 Id。来自角色 AreaId。 */
  areaId?: string;
  /** 奖励项名。 */
  rewardItem?: string | null;
  /** 奖励数量原文（字符串保留，平台可能传小数或字符串）。 */
  rewardAmount?: string | null;
  status?: AdvertisementRewardStatusEnum;
  /** 幂等键。adreward:{NormalizedProvider}:{TransactionId}。 */
  idempotencyKey?: string | null;
  /** 失败原因（仅 Failed 状态写）。 */
  failureReason?: string | null;
  /** 是否沙箱/测试回调。 */
  isSandbox?: boolean;
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

export interface TenantGameAdvertisementRewardEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAdvertisementRewardEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 广告奖励发放账本更新输入参数。仅满足 GenericCrudService 的 TUpdateRequest 泛型约束；
 * UpdateAsync 端点已由 OnUpdatingAsync 抛错关闭（账本为事实数据，不支持管理后台修改）。
 */
export interface UpdateTenantGameAdvertisementRewardEntityRequest {
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

export interface TenantGameAdvertisementRewardEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 广告奖励发放账本输出参数（C114 / GFX-111 后端跟进）。投影暴露实体全字段：
   * Provider/TransactionId/AdvertisementUnit/RoleId/UserId/AreaId/RewardItem/RewardAmount/Status/IdempotencyKey/FailureReason/IsSandbox + 审计列。
   */
  data: TenantGameAdvertisementRewardEntityResponse;
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

export type TenantGameAdvertisementRewardEntityDeletePostData = any;

export interface TenantGameAdvertisementRewardEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 广告奖励发放账本输出参数（C114 / GFX-111 后端跟进）。投影暴露实体全字段：
   * Provider/TransactionId/AdvertisementUnit/RoleId/UserId/AreaId/RewardItem/RewardAmount/Status/IdempotencyKey/FailureReason/IsSandbox + 审计列。
   */
  data: TenantGameAdvertisementRewardEntityResponse;
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

export interface TenantGameAdvertisementRewardEntityExportPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAdvertisementRewardEntityResponse[];
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

export interface TenantGameAdvertisementRewardEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAdvertisementRewardEntityResponseSqlSugarPagedList;
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

export type TenantGameAdvertisementRewardEntityUpdatePostData = any;
