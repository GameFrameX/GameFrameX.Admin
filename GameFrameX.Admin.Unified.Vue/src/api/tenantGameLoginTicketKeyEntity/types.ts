/* eslint-disable */
/* tslint:disable */

/** 租户 Game 登录票据签名密钥分页查询输入参数。 */
export interface PageTenantGameLoginTicketKeyRequest {
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

/** 租户 Game 登录票据签名密钥输出参数（仅公钥与元数据，私钥永不回显）。 */
export interface TenantGameLoginTicketKeyResponse {
  /** 主键 ID。 */
  id?: string;
  /** 租户 ID。 */
  tenantId?: string;
  /** 应用 ID。 */
  appId?: string;
  /** Key ID，写入 JWT header 的 kid。 */
  kid?: string | null;
  /** 签名算法。 */
  algorithm?: string | null;
  /** PEM 格式公钥，供 Server 验签。 */
  publicKeyPem?: string | null;
  /**
   * 生效时间。
   * @format int64
   */
  notBeforeTime?: number | null;
  /**
   * 过期时间；null 表示永不过期。
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 轮换时间。
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

export interface TenantGameLoginTicketKeyResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameLoginTicketKeyResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 签名密钥手动轮换输入参数。 */
export interface TenantGameLoginTicketKeyRotateRequest {
  /** 应用 ID。 */
  appId?: string;
  /**
   * 新密钥的到期时间；null 表示永不过期。
   * @format int64
   */
  expireTime?: number | null;
  /**
   * 旧密钥退场时间（Unix 秒，C373）：轮换后旧钥被打 RotatedTime 标记并收窄到期时间到此值，
   * 到点被生效查询自然排除（Server 按 kid 验签，窗口内旧公钥仍可验）。
   *
   * null = 默认 now + 7 天；不得早于当前时间，不得晚于 now + 30 天（对齐票据 30 天上限口径）。
   * @format int64
   */
  oldKeyExpireTime?: number | null;
}

export type TenantGameLoginTicketKeyEntityDeletePostData = any;

export interface TenantGameLoginTicketKeyEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户 Game 登录票据签名密钥输出参数（仅公钥与元数据，私钥永不回显）。 */
  data: TenantGameLoginTicketKeyResponse;
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

export type TenantGameLoginTicketKeyEntityDisablePostData = any;

export type TenantGameLoginTicketKeyEntityEnablePostData = any;

export interface TenantGameLoginTicketKeyEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameLoginTicketKeyResponseSqlSugarPagedList;
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

export type TenantGameLoginTicketKeyEntityRotatePostData = any;
