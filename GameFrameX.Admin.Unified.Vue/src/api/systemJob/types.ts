/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum TriggerStatus {
  /** Ready */
  Ready = 0,
  /** Running */
  Running = 1,
  /** Pause */
  Pause = 2,
  /** Completed */
  Completed = 3,
}

/** @format int32 */
export enum JobCreateTypeEnum {
  /** 内置 */
  BuiltIn = 0,
  /** 脚本 */
  Script = 1,
  /** HTTP请求 */
  Http = 2,
}

/** @format int32 */
export enum ClusterStatus {
  /** Waiting */
  Waiting = 0,
  /** Working */
  Working = 1,
  /** Crashed */
  Crashed = 2,
}

export interface AddJobDetailRequest {
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
  groupName?: string | null;
  jobType?: string | null;
  assemblyName?: string | null;
  description?: string | null;
  concurrent?: boolean;
  includeAnnotations?: boolean;
  properties?: string | null;
  /** @format int64 */
  updatedTime?: number | null;
  createType?: JobCreateTypeEnum;
  scriptCode?: string | null;
  /**
   * 作业Id
   * @minLength 2
   */
  jobId: string;
}

/**
 * 新增/编辑作业触发器输入参数。
 * 显式声明前端表单可填字段，不再继承 GameFrameX.Entity.System.SystemJobTriggerEntity，避免运行时状态（Status、NumberOfRuns、NumberOfErrors、LastRunTime、NextRunTime、UpdatedTime、AssemblyName）被前端批量赋值。
 */
export interface AddJobTriggerRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /**
   * 触发器Id
   * @minLength 2
   */
  triggerId: string;
  /**
   * 作业Id
   * @minLength 2
   */
  jobId: string;
  /** 触发器类型FullName */
  triggerType?: string | null;
  /** 程序集名称 */
  assemblyName?: string | null;
  /** 参数 */
  args?: string | null;
  /** 描述信息 */
  description?: string | null;
  /**
   * 起始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
  /**
   * 最大触发次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfRuns?: number;
  /**
   * 最大出错次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfErrors?: number;
  /**
   * 重试次数
   * @format int32
   */
  numRetries?: number;
  /**
   * 重试间隔时间（ms）
   * @format int32
   */
  retryTimeout?: number;
  /** 是否立即启动 */
  startNow?: boolean;
  /** 是否启动时执行一次 */
  runOnStart?: boolean;
  /** 是否在启动时重置最大触发次数等于一次的作业 */
  resetOnlyOnce?: boolean;
}

export interface DeleteJobDetailRequest {
  /** 作业Id */
  jobId?: string | null;
}

export interface DeleteJobTriggerRequest {
  /** 作业Id */
  jobId?: string | null;
  /** 触发器Id */
  triggerId?: string | null;
}

export interface JobDetailRequest {
  /** 作业Id */
  jobId?: string | null;
}

export interface JobResponse {
  jobDetail?: SystemJobDetailEntity;
  /** 触发器集合 */
  jobTriggers?: SystemJobTriggerEntity[] | null;
}

export interface JobResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: JobResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface JobTriggerRequest {
  /** 作业Id */
  jobId?: string | null;
  /** 触发器Id */
  triggerId?: string | null;
}

export interface PageJobRequest {
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
  /** 作业Id */
  jobId?: string | null;
  /** 描述信息 */
  description?: string | null;
}

/** 系统作业集群输出参数 */
export interface SystemJobClusterResponse {
  /** Id */
  id?: string;
  /** 作业集群Id */
  clusterId?: string | null;
  /** 描述信息 */
  description?: string | null;
  status?: ClusterStatus;
  /**
   * 更新时间
   * @format int64
   */
  updatedTime?: number | null;
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

export interface SystemJobDetailEntity {
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
  jobId?: string | null;
  groupName?: string | null;
  jobType?: string | null;
  assemblyName?: string | null;
  description?: string | null;
  concurrent?: boolean;
  includeAnnotations?: boolean;
  properties?: string | null;
  /** @format int64 */
  updatedTime?: number | null;
  createType?: JobCreateTypeEnum;
  scriptCode?: string | null;
}

export interface SystemJobTriggerEntity {
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
  triggerId?: string | null;
  jobId?: string | null;
  triggerType?: string | null;
  assemblyName?: string | null;
  args?: string | null;
  description?: string | null;
  status?: TriggerStatus;
  /** @format int64 */
  startTime?: number | null;
  /** @format int64 */
  endTime?: number | null;
  /** @format int64 */
  lastRunTime?: number | null;
  /** @format int64 */
  nextRunTime?: number | null;
  /** @format int64 */
  numberOfRuns?: number;
  /** @format int64 */
  maxNumberOfRuns?: number;
  /** @format int64 */
  numberOfErrors?: number;
  /** @format int64 */
  maxNumberOfErrors?: number;
  /** @format int32 */
  numRetries?: number;
  /** @format int32 */
  retryTimeout?: number;
  startNow?: boolean;
  runOnStart?: boolean;
  resetOnlyOnce?: boolean;
  /** @format int64 */
  updatedTime?: number | null;
}

/** 系统作业触发器输出参数 */
export interface SystemJobTriggerResponse {
  /** Id */
  id?: string;
  /** 触发器Id */
  triggerId?: string | null;
  /** 作业Id */
  jobId?: string | null;
  /** 触发器类型FullName */
  triggerType?: string | null;
  /** 程序集Name */
  assemblyName?: string | null;
  /** 参数 */
  args?: string | null;
  /** 描述信息 */
  description?: string | null;
  status?: TriggerStatus;
  /**
   * 起始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
  /**
   * 最近运行时间
   * @format int64
   */
  lastRunTime?: number | null;
  /**
   * 下一次运行时间
   * @format int64
   */
  nextRunTime?: number | null;
  /**
   * 触发次数
   * @format int64
   */
  numberOfRuns?: number;
  /**
   * 最大触发次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfRuns?: number;
  /**
   * 出错次数
   * @format int64
   */
  numberOfErrors?: number;
  /**
   * 最大出错次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfErrors?: number;
  /**
   * 重试次数
   * @format int32
   */
  numRetries?: number;
  /**
   * 重试间隔时间（ms）
   * @format int32
   */
  retryTimeout?: number;
  /** 是否立即启动 */
  startNow?: boolean;
  /** 是否启动时执行一次 */
  runOnStart?: boolean;
  /** 是否在启动时重置最大触发次数等于一次的作业 */
  resetOnlyOnce?: boolean;
  /**
   * 更新时间
   * @format int64
   */
  updatedTime?: number | null;
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

export interface UpdateJobDetailRequest {
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
  groupName?: string | null;
  jobType?: string | null;
  assemblyName?: string | null;
  description?: string | null;
  concurrent?: boolean;
  includeAnnotations?: boolean;
  properties?: string | null;
  /** @format int64 */
  updatedTime?: number | null;
  createType?: JobCreateTypeEnum;
  scriptCode?: string | null;
  /**
   * 作业Id
   * @minLength 2
   */
  jobId: string;
}

export interface UpdateJobTriggerRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
  /**
   * 触发器Id
   * @minLength 2
   */
  triggerId: string;
  /**
   * 作业Id
   * @minLength 2
   */
  jobId: string;
  /** 触发器类型FullName */
  triggerType?: string | null;
  /** 程序集名称 */
  assemblyName?: string | null;
  /** 参数 */
  args?: string | null;
  /** 描述信息 */
  description?: string | null;
  /**
   * 起始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
  /**
   * 最大触发次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfRuns?: number;
  /**
   * 最大出错次数（0:不限制，n:N次）
   * @format int64
   */
  maxNumberOfErrors?: number;
  /**
   * 重试次数
   * @format int32
   */
  numRetries?: number;
  /**
   * 重试间隔时间（ms）
   * @format int32
   */
  retryTimeout?: number;
  /** 是否立即启动 */
  startNow?: boolean;
  /** 是否启动时执行一次 */
  runOnStart?: boolean;
  /** 是否在启动时重置最大触发次数等于一次的作业 */
  resetOnlyOnce?: boolean;
}

export type SystemJobAddJobDetailPostData = any;

export type SystemJobAddJobTriggerPostData = any;

export type SystemJobCancelSleepPostData = any;

export type SystemJobDeleteJobDetailPostData = any;

export type SystemJobDeleteJobTriggerPostData = any;

export interface SystemJobJobClusterListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: SystemJobClusterResponse[];
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

export interface SystemJobJobTriggerListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: SystemJobTriggerResponse[];
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

export interface SystemJobPageJobDetailPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: JobResponseSqlSugarPagedList;
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

export type SystemJobPauseAllJobPostData = any;

export type SystemJobPauseJobPostData = any;

export type SystemJobPauseTriggerPostData = any;

export type SystemJobPersistAllPostData = any;

export type SystemJobRunJobPostData = any;

export type SystemJobStartAllJobPostData = any;

export type SystemJobStartJobPostData = any;

export type SystemJobStartTriggerPostData = any;

export type SystemJobUpdateJobDetailPostData = any;

export type SystemJobUpdateJobTriggerPostData = any;
