/* eslint-disable */
/* tslint:disable */

import type { BaseSelectPageResponse, BaseSelectPageStringResponse } from "../_shared/data-contracts";

/** 奖励模板增加输入参数。 */
export interface AddTenantGameRewardTemplateEntityRequest {
  /** 应用Id。奖励模板必须绑定具体应用，禁止 AppId=0。 */
  appId: string;
  /**
   * 名称。
   * @minLength 1
   */
  name: string;
  /** 详细描述。 */
  description?: string | null;
  /** 分组Id。 */
  groupId: string;
  /**
   * 奖励明细 JSON。
   * @minLength 1
   */
  rewardItemsJson: string;
  /**
   * 快照结构版本。
   * @format int32
   */
  snapshotSchemaVersion?: number;
  /**
   * 排序。
   * @format int32
   */
  sort?: number;
  /** 是否启用。 */
  isEnabled?: boolean | null;
}

/** 奖励模板分页查询输入参数。 */
export interface PageTenantGameRewardTemplateEntityRequest {
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
  /** 应用Id。 */
  appId?: string | null;
  /** 名称。 */
  name?: string | null;
  /** 分组Id。 */
  groupId?: string | null;
  /** 是否启用。 */
  isEnabled?: boolean | null;
}

/** 奖励模板输出参数。 */
export interface TenantGameRewardTemplateEntityResponse {
  /** Id */
  id?: string;
  /** 名称 */
  name?: string | null;
  /** 详细描述 */
  description?: string | null;
  /** 应用Id。奖励模板属于应用内容作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 分组Id。必须属于同一应用下的奖励模板分组。 */
  groupId?: string;
  /** 奖励明细 JSON。当前版本为 `ListAsync<GamePropItem>`。 */
  rewardItemsJson?: string | null;
  /**
   * 快照结构版本。当前固定为 1，用于后续扩展奖励结构。
   * @format int32
   */
  snapshotSchemaVersion?: number;
  /**
   * 排序。
   * @format int32
   */
  sort?: number;
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
  /** 分组名称。 */
  groupName?: string | null;
}

export interface TenantGameRewardTemplateEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameRewardTemplateEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 奖励模板快照输入。 */
export interface TenantGameRewardTemplateSnapshotRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 当前应用Id。快照复制必须限定到当前应用，禁止 AppId=0。 */
  appId: string;
}

/** 奖励模板快照输出。 */
export interface TenantGameRewardTemplateSnapshotResponse {
  /** 模板Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 模板名称。 */
  name?: string | null;
  /** 分组Id。 */
  groupId?: string;
  /** 奖励明细 JSON。业务使用该字段复制快照，不运行时引用模板。 */
  rewardItemsJson?: string | null;
  /**
   * 快照结构版本。
   * @format int32
   */
  snapshotSchemaVersion?: number;
}

/** 奖励模板更新输入参数。 */
export interface UpdateTenantGameRewardTemplateEntityRequest {
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
  /** 应用Id。不可变，更新时由服务端从既有实体回填。 */
  appId: string;
  /**
   * 名称。
   * @minLength 1
   */
  name: string;
  /** 详细描述。 */
  description?: string | null;
  /** 分组Id。 */
  groupId: string;
  /**
   * 奖励明细 JSON。
   * @minLength 1
   */
  rewardItemsJson: string;
  /**
   * 快照结构版本。
   * @format int32
   */
  snapshotSchemaVersion?: number;
  /**
   * 排序。
   * @format int32
   */
  sort?: number;
  /** 是否启用。 */
  isEnabled?: boolean | null;
}

export interface TenantGameRewardTemplateEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 奖励模板输出参数。 */
  data: TenantGameRewardTemplateEntityResponse;
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

export interface TenantGameRewardTemplateEntityCopySnapshotPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 奖励模板快照输出。 */
  data: TenantGameRewardTemplateSnapshotResponse;
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

export type TenantGameRewardTemplateEntityDeletePostData = any;

export interface TenantGameRewardTemplateEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 奖励模板输出参数。 */
  data: TenantGameRewardTemplateEntityResponse;
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

export interface TenantGameRewardTemplateEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameRewardTemplateEntityResponseSqlSugarPagedList;
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

export interface TenantGameRewardTemplateEntitySelectPostData {
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

export interface TenantGameRewardTemplateEntitySelectStringPostData {
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

export type TenantGameRewardTemplateEntityUpdatePostData = any;
