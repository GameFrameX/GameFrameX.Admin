/* eslint-disable */
/* tslint:disable */

/** 租户业务邮件配置新增输入参数 */
export interface AddTenantEmailConfigRequest {
  /** 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * 租户业务邮件用途。0 表示通配该租户 / 应用下所有租户业务邮件。
   * @format int32
   */
  purpose?: number;
  /**
   * Provider 类型。当前仅支持 `TencentCloudSes`。
   * @minLength 1
   */
  provider: string;
  /** Provider 区域。 */
  region?: string | null;
  /** 腾讯云邮件模板组 / 模板集标识。 */
  templateSet?: string | null;
  /**
   * 发件人邮箱地址。
   * @minLength 1
   */
  fromEmailAddress: string;
  /** 默认发件人显示名。 */
  defaultFromName?: string | null;
  /** 默认邮件主题前缀。 */
  defaultSubjectPrefix?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与租户业务邮件选择和测试发送。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
  /**
   * 邮件服务商 SecretId（敏感字段，入库明文，API 输出脱敏）。
   * @minLength 1
   */
  fromEmailSecretId: string;
  /**
   * 邮件服务商 SecretKey（敏感字段，入库明文，API 输出脱敏）。
   * @minLength 1
   */
  fromEmailSecretKey: string;
}

/** 租户业务邮件配置分页查询输入参数 */
export interface PageTenantEmailConfigRequest {
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
  /**
   * 租户业务邮件用途（精确过滤）。空表示不限。
   * @format int32
   */
  purpose?: number | null;
}

/** 租户业务邮件配置输出参数（敏感字段已脱敏） */
export interface TenantEmailConfigResponse {
  /** Id */
  id?: string;
  /** 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * 租户业务邮件用途（TenantEmailPurpose）。0 表示通配该租户 / 应用下所有租户业务邮件。
   * @format int32
   */
  purpose?: number;
  /** Provider 类型。当前仅支持 TencentCloudSes。 */
  provider?: string | null;
  /** Provider 区域（如腾讯云 SES 区域 ap-hongkong）。 */
  region?: string | null;
  /** 腾讯云邮件模板组 / 模板集标识；用于按业务组维护模板。 */
  templateSet?: string | null;
  /** 发件人邮箱地址。 */
  fromEmailAddress?: string | null;
  /** 默认发件人显示名称。 */
  defaultFromName?: string | null;
  /** 默认邮件主题前缀（例如 [GameName]）。 */
  defaultSubjectPrefix?: string | null;
  /** 邮件服务商 SecretId（敏感字段，API 输出需脱敏，日志禁止明文）。 */
  fromEmailSecretId?: string | null;
  /** 邮件服务商 SecretKey（敏感字段，API 输出需脱敏，日志禁止明文）。 */
  fromEmailSecretKey?: string | null;
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

export interface TenantEmailConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantEmailConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 租户业务邮件测试发送输入参数。 */
export interface TenantEmailConfigTestSendRequest {
  /** 配置 ID。空表示按当前租户 + 应用上下文自动选择。 */
  id?: string | null;
  /**
   * 目标收件人邮箱。
   * @format email
   * @minLength 1
   */
  receiverEmail: string;
  /** 模板 ID。 */
  templateId: string;
  /**
   * 邮件主题。
   * @minLength 1
   */
  subject: string;
  /** 模板变量。 */
  templateData?: Record<string, string> | null;
}

/**
 * 租户业务邮件配置更新输入参数。
 *
 * 敏感字段提交 `******` 时表示保留原值，避免脱敏回写覆盖真实密钥。
 */
export interface UpdateTenantEmailConfigRequest {
  /** 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /**
   * 租户业务邮件用途。0 表示通配该租户 / 应用下所有租户业务邮件。
   * @format int32
   */
  purpose?: number;
  /**
   * Provider 类型。当前仅支持 `TencentCloudSes`。
   * @minLength 1
   */
  provider: string;
  /** Provider 区域。 */
  region?: string | null;
  /** 腾讯云邮件模板组 / 模板集标识。 */
  templateSet?: string | null;
  /**
   * 发件人邮箱地址。
   * @minLength 1
   */
  fromEmailAddress: string;
  /** 默认发件人显示名。 */
  defaultFromName?: string | null;
  /** 默认邮件主题前缀。 */
  defaultSubjectPrefix?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与租户业务邮件选择和测试发送。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
  /**
   * 主键Id
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /**
   * 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
   * @min 0
   * @max 9223372036854776000
   */
  rowVersion: string;
  /** 邮件服务商 SecretId。传 `******` 时保留原值。 */
  fromEmailSecretId?: string | null;
  /** 邮件服务商 SecretKey。传 `******` 时保留原值。 */
  fromEmailSecretKey?: string | null;
}

export interface TenantEmailConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户业务邮件配置输出参数（敏感字段已脱敏） */
  data: TenantEmailConfigResponse;
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

export type TenantEmailConfigEntityDeletePostData = any;

export interface TenantEmailConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户业务邮件配置输出参数（敏感字段已脱敏） */
  data: TenantEmailConfigResponse;
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

export interface TenantEmailConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantEmailConfigResponseSqlSugarPagedList;
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

export interface TenantEmailConfigEntitySendTestPostData {
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

export type TenantEmailConfigEntityUpdatePostData = any;
