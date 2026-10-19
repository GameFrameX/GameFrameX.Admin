/* eslint-disable */
/* tslint:disable */

/** App 传输签名密钥分页查询输入参数。 */
export interface PageTenantAppSignatureKeyRequest {
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

/**
 * 新增 App 传输签名密钥输入参数。
 *
 * 密钥明文由服务端随机生成（不允许外部提交，避免弱密钥入环），响应中一次性回显。
 */
export interface TenantAppSignatureKeyAddRequest {
  /** 应用 ID（密钥域归属）。 */
  appId: string;
  /**
   * 生效时间（Unix 秒）；null 表示立即生效。
   * @format int64
   */
  notBeforeTime?: number | null;
  /**
   * 过期时间（Unix 秒）；null 表示永不过期。
   * @format int64
   */
  expireTime?: number | null;
  /** 备注。 */
  remark?: string | null;
}

/** 续租 App 传输签名密钥输入参数：仅更新到期时间，不产生新密钥。 */
export interface TenantAppSignatureKeyRenewRequest {
  /** 密钥主键 ID。 */
  id: string;
  /**
   * 新的到期时间（Unix 秒）；null 表示改为永不过期。必须晚于当前时间。
   * @format int64
   */
  expireTime?: number | null;
}

/**
 * App 传输签名密钥输出参数（列表/详情，密钥脱敏）。
 *
 * GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto.TenantAppSignatureKeyResponse.Secret 为固定脱敏占位值 ******：HMAC 对称密钥验签必需明文落库（见实体注释），
 * 但任何列表/详情查询都不得回显明文；明文仅在 AddAsync/RotateAsync 响应（GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto.TenantAppSignatureKeyRevealResponse）一次性回显。
 */
export interface TenantAppSignatureKeyResponse {
  /** 主键 ID。 */
  id?: string;
  /** 租户 ID。 */
  tenantId?: string;
  /** 应用 ID。 */
  appId?: string;
  /** Key ID，v2 签名头与 message 串中的 keyId。 */
  kid?: string | null;
  /** 密钥脱敏占位（固定 ******），真实明文仅 AddAsync/RotateAsync 一次性回显。 */
  secret?: string | null;
  /**
   * 生效时间；null 表示无生效下限。
   * @format int64
   */
  notBeforeTime?: number | null;
  /**
   * 过期时间；null 表示永不过期。
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 被轮换时间；null 表示从未被轮换。
   * @format int64
   */
  rotatedTime?: number | null;
  /** 是否启用。 */
  isEnabled?: boolean | null;
  /** 备注。 */
  remark?: string | null;
  /**
   * 创建时间。
   * @format int64
   */
  createdTime?: number;
}

export interface TenantAppSignatureKeyResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantAppSignatureKeyResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * App 传输签名密钥一次性明文回显输出（仅 AddAsync / RotateAsync 响应使用）。
 *
 * 明文只在密钥生成的那一次响应中出现，前端展示后即失；此后任何接口不再提供明文
 * （列表/详情均为 ****** 脱敏，实体禁删）。丢失明文只能通过 RotateAsync 再生成新 key。
 */
export interface TenantAppSignatureKeyRevealResponse {
  /** 主键 ID。 */
  id?: string;
  /** 应用 ID。 */
  appId?: string;
  /** Key ID，v2 签名头与 message 串中的 keyId。 */
  kid?: string | null;
  /** HMAC 密钥明文（仅本次响应回显，请立即复制到客户端构建注入通道；不再二次提供）。 */
  secret?: string | null;
  /**
   * 生效时间；null 表示无生效下限。
   * @format int64
   */
  notBeforeTime?: number | null;
  /**
   * 过期时间；null 表示永不过期。
   * @format int64
   */
  expireTime?: number | null;
}

/** 轮换 App 传输签名密钥输入参数：新插一把立即生效的 key（双活），旧 key 不动、按其租约自然退场。 */
export interface TenantAppSignatureKeyRotateRequest {
  /** 应用 ID（密钥域归属）。 */
  appId: string;
  /**
   * 旧密钥的新到期时间（Unix 秒）：建议设为「双活窗口」结束时刻（客户端全量切换完成后）；
   * null 表示不改动旧密钥租约（旧密钥按原到期时间或永久继续双活）。
   * @format int64
   */
  oldKeyExpireTime?: number | null;
  /** 备注。 */
  remark?: string | null;
}

export interface TenantAppSignatureKeyEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * App 传输签名密钥一次性明文回显输出（仅 AddAsync / RotateAsync 响应使用）。
   *
   * 明文只在密钥生成的那一次响应中出现，前端展示后即失；此后任何接口不再提供明文
   * （列表/详情均为 ****** 脱敏，实体禁删）。丢失明文只能通过 RotateAsync 再生成新 key。
   */
  data: TenantAppSignatureKeyRevealResponse;
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

export type TenantAppSignatureKeyEntityDeletePostData = any;

export type TenantAppSignatureKeyEntityDisablePostData = any;

export type TenantAppSignatureKeyEntityEnablePostData = any;

export interface TenantAppSignatureKeyEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantAppSignatureKeyResponseSqlSugarPagedList;
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

export type TenantAppSignatureKeyEntityRenewPostData = any;

export interface TenantAppSignatureKeyEntityRotatePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * App 传输签名密钥一次性明文回显输出（仅 AddAsync / RotateAsync 响应使用）。
   *
   * 明文只在密钥生成的那一次响应中出现，前端展示后即失；此后任何接口不再提供明文
   * （列表/详情均为 ****** 脱敏，实体禁删）。丢失明文只能通过 RotateAsync 再生成新 key。
   */
  data: TenantAppSignatureKeyRevealResponse;
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
