/* eslint-disable */
/* tslint:disable */

/** 租户 Client API 运行策略新增输入参数 */
export interface AddClientApiPolicyRequest {
  /** 是否要求请求签名。 */
  signatureRequired?: boolean;
  /** 是否要求设备证明。 */
  deviceProofRequired?: boolean;
  /** 风险策略生效模式。 */
  riskPolicy?: string | null;
  /** 访问令牌签名密钥。留空（null / 空白）时服务端自动生成 64-hex；该密钥纯服务端内部使用，不回显。 */
  signingKey?: string | null;
  /**
   * 访问令牌有效期（分钟）。
   * @format int32
   */
  lifetimeMinutes?: number;
  /**
   * 票据固定到期时间（Unix 秒）；0 表示永不过期，非 0 值必须晚于当前时间且不超过 30 天。
   * @format int64
   */
  ticketExpireTime?: number;
  /** 是否允许首次签发时自动创建租户应用级 RSA key。 */
  enableKeyAutoCreate?: boolean;
  /** 区服列表是否返回 HttpManagerUrl。 */
  enableReturnHttpManager?: boolean;
  /** 订单是否启用沙箱直发（测试充值）。 */
  enableSandBoxRecharge?: boolean;
  /** 备注。 */
  remark?: string | null;
}

/** 租户 Client API 运行策略输出参数 */
export interface ClientApiPolicyResponse {
  /** 是否要求请求签名。 */
  signatureRequired?: boolean;
  /** 是否要求设备证明。 */
  deviceProofRequired?: boolean;
  /** 风险策略生效模式。 */
  riskPolicy?: string | null;
  /** Id */
  id?: string;
  /** 访问令牌签名密钥（输出恒为脱敏占位） */
  signingKey?: string | null;
  /**
   * 访问令牌有效期（分钟）。
   * @format int32
   */
  lifetimeMinutes?: number;
  /**
   * 票据固定到期时间（Unix 秒）。0 表示永不过期。
   * @format int64
   */
  ticketExpireTime?: number;
  /** 是否允许首次签发时自动创建租户应用级 RSA key。 */
  enableKeyAutoCreate?: boolean;
  /** 区服列表是否返回 HttpManagerUrl。 */
  enableReturnHttpManager?: boolean;
  /** 订单是否启用沙箱直发（测试充值）。 */
  enableSandBoxRecharge?: boolean;
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

export interface ClientApiPolicyResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: ClientApiPolicyResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 租户 Client API 运行策略分页查询输入参数 */
export interface PageClientApiPolicyRequest {
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
  isActive?: boolean | null;
}

/** 租户 Client API 运行策略更新输入参数 */
export interface UpdateClientApiPolicyRequest {
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
  /** 是否要求请求签名。 */
  signatureRequired?: boolean;
  /** 是否要求设备证明。 */
  deviceProofRequired?: boolean;
  /** 风险策略生效模式。 */
  riskPolicy?: string | null;
  /** 访问令牌签名密钥。回传 `******` 或留空均表示保留原值；提交新明文则替换。 */
  signingKey?: string | null;
  /**
   * 访问令牌有效期（分钟）。
   * @format int32
   */
  lifetimeMinutes?: number;
  /**
   * 票据固定到期时间（Unix 秒）；0 表示永不过期，非 0 值必须晚于当前时间且不超过 30 天。
   * @format int64
   */
  ticketExpireTime?: number;
  /** 是否允许首次签发时自动创建租户应用级 RSA key。 */
  enableKeyAutoCreate?: boolean;
  /** 区服列表是否返回 HttpManagerUrl。 */
  enableReturnHttpManager?: boolean;
  /** 订单是否启用沙箱直发（测试充值）。 */
  enableSandBoxRecharge?: boolean;
  /** 备注。 */
  remark?: string | null;
}

export interface ClientApiPolicyAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户 Client API 运行策略输出参数 */
  data: ClientApiPolicyResponse;
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

export type ClientApiPolicyDeletePostData = any;

export interface ClientApiPolicyDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 租户 Client API 运行策略输出参数 */
  data: ClientApiPolicyResponse;
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

export interface ClientApiPolicyPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: ClientApiPolicyResponseSqlSugarPagedList;
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

export type ClientApiPolicyRotatePostData = any;

export type ClientApiPolicyUpdatePostData = any;
