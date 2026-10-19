/* eslint-disable */
/* tslint:disable */

/** 新增应用开放访问凭证输入。AccessSecret 由服务端生成，仅签发响应中返回一次。 */
export interface AddTenantGameAppOpenAccessRequest {
  /**
   * 当前应用 Id。必须大于 0。
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string;
  /**
   * 绑定的租户系统用户 Id。
   * @min 1
   * @max 9223372036854776000
   */
  bindUserId?: string;
  /**
   * 责任场景编码。
   * @minLength 0
   * @maxLength 256
   */
  scenario: string;
  /**
   * 使用目的。
   * @minLength 0
   * @maxLength 512
   */
  purpose: string;
  /**
   * 责任主体。
   * @minLength 0
   * @maxLength 256
   */
  subjectName: string;
  /**
   * 接口范围（允许调用的路由前缀集合，JSON 数组字符串）。至少包含一个 scope，例如 ["package-report"]。
   * @minLength 0
   * @maxLength 1024
   */
  scopesJson: string;
  /**
   * 过期时间，Unix 秒；为空表示不过期。
   * @format int64
   */
  expiresTime?: number | null;
}

/** 应用开放访问凭证分页查询输入。 */
export interface PageTenantGameAppOpenAccessRequest {
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
  /** 当前应用 Id。凭证页只能在应用工作台内查询。 */
  appId?: string | null;
  /** 访问 Key 关键字。 */
  accessKey?: string | null;
  /** 责任场景编码。 */
  scenario?: string | null;
  /** 责任主体。 */
  subjectName?: string | null;
  /** 启用状态过滤（可空）。 */
  isEnabled?: boolean | null;
}

/** 按 Id 操作应用开放访问凭证输入（吊销 / 启用 / 禁用 / 续期）。 */
export interface TenantGameAppOpenAccessActionRequest {
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
  /**
   * 当前应用 Id（校验凭证归属当前 App）。
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string;
  /**
   * 操作原因（吊销 / 续期时记录，便于审计排查）。
   * @minLength 0
   * @maxLength 512
   */
  reason?: string | null;
}

/**
 * 应用开放访问凭证输出。
 *
 * 注意：GameFrameX.Entity.Tenant.Game.App.TenantGameAppOpenAccessEntity.AccessSecret 永不输出（明文仅签发 / 轮换时一次性返回）。
 * Output 不含 AccessSecret 字段，Service 投影时也不得引入。
 */
export interface TenantGameAppOpenAccessResponse {
  /** Id */
  id?: string;
  /** 应用作用域。强制绑定具体应用，不支持 0 或租户默认凭证。 */
  appId?: string;
  /** 绑定的租户系统用户 Id。Hub.Api 写入版本数据和审计列时使用该用户身份。 */
  bindUserId?: string;
  /** 责任场景编码，例如 ci、internal-builder、asset-pipeline、release-pipeline。自由文本标识，非权限映射。 */
  scenario?: string | null;
  /** 使用目的说明，用于区分同一应用下多个凭证的职责边界。 */
  purpose?: string | null;
  /** 责任主体名称，例如 CI 项目名、打包机名或流水线名。 */
  subjectName?: string | null;
  /** 接口范围（允许调用的接口 / 路由前缀集合，JSON 数组字符串，OAuth scope 语义）。 */
  scopesJson?: string | null;
  /** 访问 Key。租户内唯一，鉴权时按声明租户路由租户库查询。 */
  accessKey?: string | null;
  /**
   * 过期时间，Unix 秒。为空表示不过期。
   * @format int64
   */
  expiresTime?: number | null;
  /**
   * 撤销时间，Unix 秒。撤销后即使 IsEnabled 为 true 也不得继续鉴权。
   * @format int64
   */
  revokedTime?: number | null;
  /** 撤销原因，用于审计排查。 */
  revokeReason?: string | null;
  /** 绑定用户账号。 */
  bindUserAccount?: string | null;
  /** 绑定用户姓名。 */
  bindUserName?: string | null;
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

export interface TenantGameAppOpenAccessResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameAppOpenAccessResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 应用开放访问凭证签发 / 轮换结果。
 *
 * GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto.TenantGameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
 */
export interface TenantGameAppOpenAccessSecretResponse {
  /** 凭证 Id。 */
  id?: string;
  /** 访问 Key。 */
  accessKey?: string | null;
  /** 访问密钥明文。仅在签发 / 轮换响应中返回一次。 */
  accessSecret?: string | null;
  /** 接口范围（JSON 数组字符串）。 */
  scopesJson?: string | null;
}

/** 更新应用开放访问凭证输入。AppId 不可变（编辑态由服务端按行原值回填）。 */
export interface UpdateTenantGameAppOpenAccessRequest {
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
  /**
   * 当前应用 Id（用于校验凭证归属当前 App；AppId 本身不可变，由服务端按行原值回填）。
   * @min 1
   * @max 9223372036854776000
   */
  appId?: string;
  /**
   * 绑定的租户系统用户 Id。
   * @min 1
   * @max 9223372036854776000
   */
  bindUserId?: string;
  /**
   * 责任场景编码。
   * @minLength 0
   * @maxLength 256
   */
  scenario: string;
  /**
   * 使用目的。
   * @minLength 0
   * @maxLength 512
   */
  purpose: string;
  /**
   * 责任主体。
   * @minLength 0
   * @maxLength 256
   */
  subjectName: string;
  /**
   * 接口范围（允许调用的路由前缀集合，JSON 数组字符串）。
   * @minLength 0
   * @maxLength 1024
   */
  scopesJson: string;
  /**
   * 过期时间，Unix 秒；为空表示不过期。
   * @format int64
   */
  expiresTime?: number | null;
}

export interface TenantGameAppOpenAccessAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 应用开放访问凭证签发 / 轮换结果。
   *
   * GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto.TenantGameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
   */
  data: TenantGameAppOpenAccessSecretResponse;
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

export type TenantGameAppOpenAccessDeletePostData = any;

export type TenantGameAppOpenAccessDisablePostData = any;

export type TenantGameAppOpenAccessEnablePostData = any;

export interface TenantGameAppOpenAccessPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameAppOpenAccessResponseSqlSugarPagedList;
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

export interface TenantGameAppOpenAccessRefreshPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /**
   * 应用开放访问凭证签发 / 轮换结果。
   *
   * GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto.TenantGameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
   */
  data: TenantGameAppOpenAccessSecretResponse;
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

export type TenantGameAppOpenAccessRevokePostData = any;

export type TenantGameAppOpenAccessUpdatePostData = any;
