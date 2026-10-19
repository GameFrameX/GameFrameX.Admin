/* eslint-disable */
/* tslint:disable */

/** 租户 SMS 配置新增输入参数。 */
export interface AddTenantSmsConfigRequest {
  /** 应用 ID。0 表示租户默认 SMS 配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * Provider 类型。当前仅支持 `Aliyun`。
   * @minLength 1
   */
  provider: string;
  /**
   * 阿里云 AccessKeyId（敏感字段，入库明文，API 输出脱敏）。
   *
   * 禁止提交 `******`；脱敏占位仅用于 UpdateAsync 保留原值。
   * @minLength 1
   */
  accessKeyId: string;
  /**
   * 阿里云 AccessKeySecret（敏感字段，入库明文，API 输出脱敏）。
   *
   * 禁止提交 `******`；脱敏占位仅用于 UpdateAsync 保留原值。
   * @minLength 1
   */
  accessKeySecret: string;
  /**
   * 短信签名。
   * @minLength 1
   */
  signName: string;
  /** 短信模板 Code（阿里云 SMS_ 开头）。可为空（`SendTestAsync` 时由调用方指定）。 */
  templateCode?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与 SMS 解析和测试发送。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

/** 租户 SMS 配置分页查询输入参数。 */
export interface PageTenantSmsConfigRequest {
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
  appId?: string | null;
  isActive?: boolean | null;
}

/** 租户 SMS 配置输出参数（敏感字段已脱敏）。 */
export interface TenantSmsConfigResponse {
  /** Id */
  id?: string;
  /** 应用 ID。0 表示租户默认 SMS 配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /** Provider 类型。当前仅支持 Aliyun（阿里云短信）。 */
  provider?: string | null;
  /** 阿里云 AccessKeyId（敏感字段，API 输出脱敏，日志禁止明文）。 */
  accessKeyId?: string | null;
  /** 阿里云 AccessKeySecret（敏感字段，API 输出脱敏，日志禁止明文）。 */
  accessKeySecret?: string | null;
  /** 短信签名（如 AdminNET平台）。 */
  signName?: string | null;
  /** 短信模板 Code（阿里云 SMS_ 开头）。可为空（测试发送时由调用方指定）。 */
  templateCode?: string | null;
  /**
   * 配置选择优先级。同一匹配层中按此值降序选择；数值越大越优先。
   * @format int32
   */
  priority?: number;
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
  /** 备注 */
  remark?: string | null;
}

export interface TenantSmsConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantSmsConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 租户 SMS 测试发送输入参数。 */
export interface TenantSmsConfigTestSendRequest {
  /** 配置 ID。必须显式指定租户 SMS 配置 ID，避免依赖运行期租户上下文。 */
  id: string;
  /**
   * 目标手机号。
   * @format tel
   * @minLength 1
   */
  phone: string;
  /** 短信模板 Code（可选）。留空则使用配置存储值；两者皆空返回明确错误。 */
  templateCode?: string | null;
}

/**
 * 租户 SMS 配置更新输入参数。
 *
 * 敏感字段提交 `******` 时表示保留原值，避免脱敏回写覆盖真实密钥。
 */
export interface UpdateTenantSmsConfigRequest {
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
  /** 应用 ID。 */
  appId?: string;
  /**
   * Provider 类型。
   * @minLength 1
   */
  provider: string;
  /** 阿里云 AccessKeyId。传 `******` 时保留原值。 */
  accessKeyId?: string | null;
  /** 阿里云 AccessKeySecret。传 `******` 时保留原值。 */
  accessKeySecret?: string | null;
  /**
   * 短信签名。
   * @minLength 1
   */
  signName: string;
  /** 短信模板 Code。 */
  templateCode?: string | null;
  /**
   * 配置选择优先级。
   * @format int32
   */
  priority?: number;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface TenantSmsConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户 SMS 配置输出参数（敏感字段已脱敏）。 */
  data: TenantSmsConfigResponse;
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

export type TenantSmsConfigEntityDeletePostData = any;

export interface TenantSmsConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户 SMS 配置输出参数（敏感字段已脱敏）。 */
  data: TenantSmsConfigResponse;
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

export interface TenantSmsConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantSmsConfigResponseSqlSugarPagedList;
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

export interface TenantSmsConfigEntitySendTestPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: boolean;
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

export type TenantSmsConfigEntityUpdatePostData = any;
