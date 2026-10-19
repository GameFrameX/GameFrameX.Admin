/* eslint-disable */
/* tslint:disable */

/** 平台系统邮件 Provider 配置新增输入参数。 */
export interface AddSystemEmailProviderRequest {
  /**
   * 平台系统邮件用途（`SystemEmailPurpose`）。0 表示默认系统邮件配置。
   * @format int32
   */
  purpose?: number;
  /**
   * Provider 类型。当前支持 `TencentCloudSes` 和 `Smtp`。
   * @minLength 1
   */
  provider: string;
  /** SMTP 主机（Provider=Smtp 时必填）。 */
  host?: string | null;
  /**
   * SMTP 端口（Provider=Smtp 时必填）。
   * @format int32
   */
  port?: number | null;
  /** SMTP 启用 SSL。 */
  enableSsl?: boolean | null;
  /** SMTP 使用默认凭据。 */
  useDefaultCredentials?: boolean | null;
  /** SMTP 用户名（敏感字段）。 */
  userName?: string | null;
  /** SMTP 密码（敏感字段）。 */
  password?: string | null;
  /** Provider 区域。 */
  region?: string | null;
  /** 腾讯云 SecretId（敏感字段）。 */
  secretId?: string | null;
  /** 腾讯云 SecretKey（敏感字段）。 */
  secretKey?: string | null;
  /**
   * 默认发件人邮箱。
   * @minLength 1
   */
  defaultFromEmail: string;
  /** 默认发件人显示名。 */
  defaultFromName?: string | null;
  /** 默认收件人邮箱。 */
  defaultToEmail?: string | null;
  /** 腾讯云 SES 模板 ID。Provider=TencentCloudSes 时必填。 */
  templateId?: string | null;
  /** 腾讯云 SES 模板变量 JSON。 */
  templateDataJson?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与系统邮件选择和测试发送。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

/** 平台系统邮件 Provider 配置分页查询输入参数。 */
export interface PageSystemEmailProviderRequest {
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
  /**
   * 平台系统邮件用途（精确过滤）。空表示不限。
   * @format int32
   */
  purpose?: number | null;
  /** Provider 类型（精确过滤）。空表示不限。 */
  provider?: string | null;
  /** 是否启用。 */
  isActive?: boolean | null;
}

/** 平台系统邮件 Provider 配置输出参数（敏感字段已脱敏）。 */
export interface SystemEmailProviderResponse {
  /** Id */
  id?: string;
  /**
   * 平台系统邮件用途（SystemEmailPurpose）。0 表示默认配置。
   * @format int32
   */
  purpose?: number;
  /** Provider 类型。当前支持 TencentCloudSes（腾讯云 SES）和 Smtp。 */
  provider?: string | null;
  /** SMTP 主机。Provider=Smtp 时必填。 */
  host?: string | null;
  /**
   * SMTP 端口。Provider=Smtp 时必填。
   * @format int32
   */
  port?: number | null;
  /** SMTP 启用 SSL。Provider=Smtp 时使用。 */
  enableSsl?: boolean | null;
  /** SMTP 使用默认凭据。Provider=Smtp 时使用。 */
  useDefaultCredentials?: boolean | null;
  /** SMTP 用户名（敏感字段，API 输出脱敏，日志禁止明文）。 */
  userName?: string | null;
  /** SMTP 密码（敏感字段，API 输出脱敏，日志禁止明文）。 */
  password?: string | null;
  /** Provider 区域（如腾讯云 SES 区域 ap-hongkong）。 */
  region?: string | null;
  /** 腾讯云 SES SecretId（敏感字段，API 输出脱敏，日志禁止明文）。 */
  secretId?: string | null;
  /** 腾讯云 SES SecretKey（敏感字段，API 输出脱敏，日志禁止明文）。 */
  secretKey?: string | null;
  /** 平台默认发件人邮箱地址。 */
  defaultFromEmail?: string | null;
  /** 平台默认发件人显示名称。 */
  defaultFromName?: string | null;
  /** 平台默认收件人邮箱；缺省时使用业务调用方传入的 receiver。 */
  defaultToEmail?: string | null;
  /** 腾讯云 SES 模板 ID。Provider=TencentCloudSes 时必填；SMTP 发送不使用。 */
  templateId?: string | null;
  /** 腾讯云 SES 模板变量 JSON。Provider=TencentCloudSes 时写入 TemplateData；SMTP 发送不使用。 */
  templateDataJson?: string | null;
  /**
   * 配置选择优先级。同一 Purpose 匹配层中按此值降序选择；数值越大越优先。
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

export interface SystemEmailProviderResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: SystemEmailProviderResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 测试发送输入参数。
 *
 * 用于受权限控制的测试发送接口；调用方必须显式传入用途与目标邮箱；敏感字段不输出到 API/日志/前端。
 */
export interface SystemEmailProviderTestSendRequest {
  /** 配置 ID。测试发送必须指定当前要验证的 Provider 配置。 */
  id: string;
  /**
   * 目标收件人邮箱。
   * @format email
   * @minLength 1
   */
  receiverEmail: string;
  /**
   * 测试主题。
   * @minLength 1
   */
  subject: string;
  /**
   * 测试正文。
   * @minLength 1
   */
  body: string;
}

/**
 * 平台系统邮件 Provider 配置更新输入参数。
 *
 * 敏感字段（UserName / Password / SecretId / SecretKey）回传 `******` 时保留数据库原值，避免脱敏回写覆盖真实凭据。
 */
export interface UpdateSystemEmailProviderRequest {
  /**
   * 平台系统邮件用途（`SystemEmailPurpose`）。0 表示默认系统邮件配置。
   * @format int32
   */
  purpose?: number;
  /**
   * Provider 类型。当前支持 `TencentCloudSes` 和 `Smtp`。
   * @minLength 1
   */
  provider: string;
  /** SMTP 主机（Provider=Smtp 时必填）。 */
  host?: string | null;
  /**
   * SMTP 端口（Provider=Smtp 时必填）。
   * @format int32
   */
  port?: number | null;
  /** SMTP 启用 SSL。 */
  enableSsl?: boolean | null;
  /** SMTP 使用默认凭据。 */
  useDefaultCredentials?: boolean | null;
  /** SMTP 用户名（敏感字段）。 */
  userName?: string | null;
  /** SMTP 密码（敏感字段）。 */
  password?: string | null;
  /** Provider 区域。 */
  region?: string | null;
  /** 腾讯云 SecretId（敏感字段）。 */
  secretId?: string | null;
  /** 腾讯云 SecretKey（敏感字段）。 */
  secretKey?: string | null;
  /**
   * 默认发件人邮箱。
   * @minLength 1
   */
  defaultFromEmail: string;
  /** 默认发件人显示名。 */
  defaultFromName?: string | null;
  /** 默认收件人邮箱。 */
  defaultToEmail?: string | null;
  /** 腾讯云 SES 模板 ID。Provider=TencentCloudSes 时必填。 */
  templateId?: string | null;
  /** 腾讯云 SES 模板变量 JSON。 */
  templateDataJson?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与系统邮件选择和测试发送。 */
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
}

export interface SystemEmailProviderAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台系统邮件 Provider 配置输出参数（敏感字段已脱敏）。 */
  data: SystemEmailProviderResponse;
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

export type SystemEmailProviderDeletePostData = any;

export interface SystemEmailProviderDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 平台系统邮件 Provider 配置输出参数（敏感字段已脱敏）。 */
  data: SystemEmailProviderResponse;
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

export interface SystemEmailProviderPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: SystemEmailProviderResponseSqlSugarPagedList;
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

export interface SystemEmailProviderSendTestPostData {
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

export type SystemEmailProviderUpdatePostData = any;
