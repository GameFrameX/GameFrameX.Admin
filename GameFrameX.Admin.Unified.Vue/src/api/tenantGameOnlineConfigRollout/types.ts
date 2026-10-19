/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum OnlineConfigRolloutStatus {
  /** 已发布 */
  Published = 1,
  /** 已回退 */
  RolledBack = 2,
}

/** @format int32 */
export enum OnlineConfigRolloutKind {
  /** 远程配置 */
  RemoteConfig = 1,
  /** 玩家公告 */
  Announcement = 2,
  /** 设备分组 */
  DeviceGroup = 3,
  /** 定时任务 */
  ScheduledTask = 4,
}

/** 配置灰度版本行分页查询输入（来源 change C343 / vault:C8 S7.7）。 */
export interface PageTenantGameOnlineConfigRolloutRequest {
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
  /** 应用Id（事实数据必填，按当前应用筛选）。 */
  appId: string;
  configKind?: OnlineConfigRolloutKind;
  /** 配置业务键精确过滤（所辖配置域实体 Id 字符串）。 */
  configKey?: string | null;
  status?: OnlineConfigRolloutStatus;
  /** 目标区服Id过滤。 */
  serverId?: string | null;
}

/** 灰度发布输入（来源 change C343 / vault:C8 S7.7）。Percent 边界常量直接引用 `OnlineRolloutSemanticMap`（禁内联第二套数值）。 */
export interface PublishConfigRolloutRequest {
  /**
   * 应用Id（事实数据应用作用域）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 目标区服Id（Online 作用域 ServerId）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  configKind: OnlineConfigRolloutKind;
  /**
   * 配置业务键（所辖配置域实体 Id 的字符串形式）。
   * @minLength 1
   */
  configKey: string;
  /**
   * 灰度分群Id（必须为已同步 Online 的分群）。
   * @min 1
   * @max 9223372036854776000
   */
  segmentId: string;
  /**
   * 灰度百分比（1-100：0 无意义即拒绝，100 = 分群内全量；语义边界见 OnlineRolloutSemanticMap）。
   * @format int32
   * @min 1
   * @max 100
   */
  percent: number;
  /**
   * 下发理由（必填，进审计）。
   * @minLength 1
   */
  reason: string;
}

/** 灰度发布输出（发布成功的版本行回显）。 */
export interface PublishConfigRolloutResponse {
  /** 版本行Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 目标区服Id。 */
  serverId?: string;
  configKind?: OnlineConfigRolloutKind;
  /** 配置业务键（所辖配置域实体 Id 字符串）。 */
  configKey?: string | null;
  /**
   * Admin 侧分配的下发版本号（同配置维度递增；发布与回滚共用序列）。
   * @format int32
   */
  configVersion?: number;
  /** 灰度分群Id。 */
  segmentId?: string;
  /**
   * 灰度百分比（1-100，语义见 OnlineRolloutSemanticMap）。
   * @format int32
   */
  percent?: number;
  status?: OnlineConfigRolloutStatus;
  /**
   * 回滚目标版本（仅回滚行有值，指向要恢复到的历史 ConfigVersion）。
   * @format int32
   */
  targetVersion?: number;
  /** 本次下发理由。 */
  reason?: string | null;
  /** Online 确认版本回执（空 = 未取得回执）。 */
  onlineConfirmVersion?: string | null;
  /** 下发操作人。 */
  dispatchOperator?: string | null;
  /**
   * 下发时间（Unix 秒）。
   * @format int64
   */
  publishedTime?: number;
  /**
   * 创建时间（Unix 秒）。
   * @format int64
   */
  createdTime?: number;
}

/** 灰度回滚输入（来源 change C343 / vault:C8 S7.8 / VC-7.12）。回滚 = 版本化新下发：恢复目标版本的 SegmentId/Percent 语义，生成新 ConfigVersion 行。 */
export interface RollbackConfigRolloutRequest {
  /**
   * 应用Id（事实数据应用作用域）。
   * @min 1
   * @max 9223372036854776000
   */
  appId: string;
  /**
   * 目标区服Id（Online 作用域 ServerId）。
   * @min 1
   * @max 9223372036854776000
   */
  serverId: string;
  configKind: OnlineConfigRolloutKind;
  /**
   * 配置业务键（所辖配置域实体 Id 的字符串形式）。
   * @minLength 1
   */
  configKey: string;
  /**
   * 回滚目标版本（要恢复到的历史 ConfigVersion，必须已存在于该配置的版本序列）。
   * @format int32
   * @min 1
   * @max 2147483647
   */
  targetVersion: number;
  /**
   * 回滚理由（必填，进审计）。
   * @minLength 1
   */
  reason: string;
}

/** 灰度回滚输出（回滚生成的版本化新行回显，TargetVersion 指向恢复目标）。 */
export interface RollbackConfigRolloutResponse {
  /** 版本行Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 目标区服Id。 */
  serverId?: string;
  configKind?: OnlineConfigRolloutKind;
  /** 配置业务键（所辖配置域实体 Id 字符串）。 */
  configKey?: string | null;
  /**
   * Admin 侧分配的下发版本号（同配置维度递增；发布与回滚共用序列）。
   * @format int32
   */
  configVersion?: number;
  /** 灰度分群Id。 */
  segmentId?: string;
  /**
   * 灰度百分比（1-100，语义见 OnlineRolloutSemanticMap）。
   * @format int32
   */
  percent?: number;
  status?: OnlineConfigRolloutStatus;
  /**
   * 回滚目标版本（仅回滚行有值，指向要恢复到的历史 ConfigVersion）。
   * @format int32
   */
  targetVersion?: number;
  /** 本次下发理由。 */
  reason?: string | null;
  /** Online 确认版本回执（空 = 未取得回执）。 */
  onlineConfirmVersion?: string | null;
  /** 下发操作人。 */
  dispatchOperator?: string | null;
  /**
   * 下发时间（Unix 秒）。
   * @format int64
   */
  publishedTime?: number;
  /**
   * 创建时间（Unix 秒）。
   * @format int64
   */
  createdTime?: number;
}

/** 配置灰度版本行输出（Publish / Rollback 动作回显的公共基类）。 */
export interface TenantGameOnlineConfigRolloutResponse {
  /** 版本行Id。 */
  id?: string;
  /** 应用Id。 */
  appId?: string;
  /** 目标区服Id。 */
  serverId?: string;
  configKind?: OnlineConfigRolloutKind;
  /** 配置业务键（所辖配置域实体 Id 字符串）。 */
  configKey?: string | null;
  /**
   * Admin 侧分配的下发版本号（同配置维度递增；发布与回滚共用序列）。
   * @format int32
   */
  configVersion?: number;
  /** 灰度分群Id。 */
  segmentId?: string;
  /**
   * 灰度百分比（1-100，语义见 OnlineRolloutSemanticMap）。
   * @format int32
   */
  percent?: number;
  status?: OnlineConfigRolloutStatus;
  /**
   * 回滚目标版本（仅回滚行有值，指向要恢复到的历史 ConfigVersion）。
   * @format int32
   */
  targetVersion?: number;
  /** 本次下发理由。 */
  reason?: string | null;
  /** Online 确认版本回执（空 = 未取得回执）。 */
  onlineConfirmVersion?: string | null;
  /** 下发操作人。 */
  dispatchOperator?: string | null;
  /**
   * 下发时间（Unix 秒）。
   * @format int64
   */
  publishedTime?: number;
  /**
   * 创建时间（Unix 秒）。
   * @format int64
   */
  createdTime?: number;
}

export interface TenantGameOnlineConfigRolloutResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameOnlineConfigRolloutResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface TenantGameOnlineConfigRolloutPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameOnlineConfigRolloutResponseSqlSugarPagedList;
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

export interface TenantGameOnlineConfigRolloutPublishPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 灰度发布输出（发布成功的版本行回显）。 */
  data: PublishConfigRolloutResponse;
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

export interface TenantGameOnlineConfigRolloutRollbackPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 灰度回滚输出（回滚生成的版本化新行回显，TargetVersion 指向恢复目标）。 */
  data: RollbackConfigRolloutResponse;
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
