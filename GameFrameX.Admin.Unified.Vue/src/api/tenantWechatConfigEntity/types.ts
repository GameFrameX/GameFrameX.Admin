/* eslint-disable */
/* tslint:disable */

/**
 * 租户微信公众号 / 小程序配置新增输入参数。
 *
 * 公众号凭证对（GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.AddTenantWechatConfigRequest.WechatAppId + GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.AddTenantWechatConfigRequest.WechatAppSecret）与小程序凭证对（GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.AddTenantWechatConfigRequest.WxOpenAppId + GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.AddTenantWechatConfigRequest.WxOpenSecret）至少配置一组，且每组必须成对完整。
 */
export interface AddTenantWechatConfigRequest {
  /** 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /** 微信公众号 AppId。 */
  wechatAppId?: string | null;
  /** 微信公众号 AppSecret（敏感字段，入库明文，API 输出脱敏）。 */
  wechatAppSecret?: string | null;
  /** 微信小程序 AppId。 */
  wxOpenAppId?: string | null;
  /** 微信小程序 AppSecret（敏感字段，入库明文，API 输出脱敏）。 */
  wxOpenSecret?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。仅用于配置选择。
   * @format int32
   */
  priority?: number;
  /** 是否启用。禁用配置不参与租户 Wechat 选择和测试发送。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

/** 租户微信公众号 / 小程序配置分页查询输入参数。 */
export interface PageTenantWechatConfigRequest {
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

/** 租户微信公众号 / 小程序配置输出参数（敏感字段已脱敏）。 */
export interface TenantWechatConfigResponse {
  /** Id */
  id?: string;
  /** 应用 ID。0 表示租户默认公众号/小程序配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /** 微信公众号 AppId。 */
  wechatAppId?: string | null;
  /** 微信公众号 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。 */
  wechatAppSecret?: string | null;
  /** 微信小程序 AppId。 */
  wxOpenAppId?: string | null;
  /** 微信小程序 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。 */
  wxOpenSecret?: string | null;
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

export interface TenantWechatConfigResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantWechatConfigResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 租户微信公众号 / 小程序配置测试发送输入参数。
 *
 * 公众号 / 小程序授权登录已从 Admin 登录移除，本接口仅服务租户上下文内的模板消息测试发送。
 */
export interface TenantWechatConfigTestSendRequest {
  /** 配置 ID。必填；测试发送必须显式指定目标租户配置。 */
  id?: string | null;
  /**
   * 接收模板消息的目标用户 OpenId。
   * @minLength 1
   */
  receiverOpenId: string;
  /**
   * 微信模板消息 ID。
   * @minLength 1
   */
  templateId: string;
  /** 模板消息点击跳转链接。 */
  url?: string | null;
  /** 小程序页面路径。 */
  miniProgramPagePath?: string | null;
  /** 模板变量。Key 与微信模板占位符一致，Value 为字符串内容。 */
  testData?: Record<string, string> | null;
}

/**
 * 租户微信公众号 / 小程序配置更新输入参数。
 *
 * 敏感字段（GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.UpdateTenantWechatConfigRequest.WechatAppSecret / GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.UpdateTenantWechatConfigRequest.WxOpenSecret）提交 `******` 时表示保留原值，避免脱敏回写覆盖真实密钥。
 */
export interface UpdateTenantWechatConfigRequest {
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
  /** 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。 */
  appId?: string;
  /** 微信公众号 AppId。 */
  wechatAppId?: string | null;
  /** 微信公众号 AppSecret。传 `******` 时保留原值。 */
  wechatAppSecret?: string | null;
  /** 微信小程序 AppId。 */
  wxOpenAppId?: string | null;
  /** 微信小程序 AppSecret。传 `******` 时保留原值。 */
  wxOpenSecret?: string | null;
  /**
   * 配置选择优先级（数值越大越优先）。
   * @format int32
   */
  priority?: number;
  /** 是否启用。 */
  isEnabled?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface TenantWechatConfigEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户微信公众号 / 小程序配置输出参数（敏感字段已脱敏）。 */
  data: TenantWechatConfigResponse;
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

export type TenantWechatConfigEntityDeletePostData = any;

export interface TenantWechatConfigEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户微信公众号 / 小程序配置输出参数（敏感字段已脱敏）。 */
  data: TenantWechatConfigResponse;
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

export interface TenantWechatConfigEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantWechatConfigResponseSqlSugarPagedList;
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

export interface TenantWechatConfigEntitySendTestPostData {
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

export type TenantWechatConfigEntityUpdatePostData = any;
