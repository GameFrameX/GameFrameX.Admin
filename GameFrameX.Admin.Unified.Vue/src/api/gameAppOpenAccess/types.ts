/* eslint-disable */
/* tslint:disable */

/** 新增应用开放访问凭证输入。AccessSecret 由服务端生成，仅签发响应中返回一次。 */
export interface AddGameAppOpenAccessRequest {
  /**
   * 绑定的系统用户 Id。
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

/** 按 Id 操作应用开放访问凭证输入（吊销 / 启用 / 禁用 / 续期）。 */
export interface GameAppOpenAccessActionRequest {
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
   * 操作原因（吊销 / 续期时记录，便于审计排查）。
   * @minLength 0
   * @maxLength 512
   */
  reason?: string | null;
}

/**
 * 应用开放访问凭证输出。
 *
 * 注意：GameFrameX.Entity.Business.Game.App.GameAppOpenAccessEntity.AccessSecret 永不输出（明文仅签发 / 轮换时一次性返回）。
 * Output 不含 AccessSecret 字段，Service 投影时也不得引入。
 */
export interface GameAppOpenAccessResponse {
  /** Id */
  id?: string;
  /** 绑定的系统用户 Id。Hub.Api 写入版本数据和审计列时使用该用户身份。 */
  bindUserId?: string;
  /** 责任场景编码，例如 ci、internal-builder、asset-pipeline、release-pipeline。自由文本标识，非权限映射。 */
  scenario?: string | null;
  /** 使用目的说明，用于区分多个凭证的职责边界。 */
  purpose?: string | null;
  /** 责任主体名称，例如 CI 项目名、打包机名或流水线名。 */
  subjectName?: string | null;
  /** 接口范围（允许调用的接口 / 路由前缀集合，JSON 数组字符串，OAuth scope 语义）。 */
  scopesJson?: string | null;
  /** 访问 Key。主库唯一，鉴权时按访问 Key 查询。 */
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

export interface GameAppOpenAccessResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: GameAppOpenAccessResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 应用开放访问凭证签发 / 轮换结果。
 *
 * GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto.GameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
 */
export interface GameAppOpenAccessSecretResponse {
  /** 凭证 Id。 */
  id?: string;
  /** 访问 Key。 */
  accessKey?: string | null;
  /** 访问密钥明文。仅在签发 / 轮换响应中返回一次。 */
  accessSecret?: string | null;
  /** 接口范围（JSON 数组字符串）。 */
  scopesJson?: string | null;
}

/** 应用开放访问凭证分页查询输入。 */
export interface PageGameAppOpenAccessRequest {
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
  /** 访问 Key 关键字。 */
  accessKey?: string | null;
  /** 责任场景编码。 */
  scenario?: string | null;
  /** 责任主体。 */
  subjectName?: string | null;
  /** 启用状态过滤（可空）。 */
  isEnabled?: boolean | null;
}

/** 更新应用开放访问凭证输入。绑定用户和接口范围由服务端校验。 */
export interface UpdateGameAppOpenAccessRequest {
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
   * 绑定的系统用户 Id。
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

export interface GameAppOpenAccessAddPostData {
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
   * GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto.GameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
   */
  data: GameAppOpenAccessSecretResponse;
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

export type GameAppOpenAccessDeletePostData = any;

export type GameAppOpenAccessDisablePostData = any;

export type GameAppOpenAccessEnablePostData = any;

export interface GameAppOpenAccessPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: GameAppOpenAccessResponseSqlSugarPagedList;
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

export interface GameAppOpenAccessRefreshPostData {
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
   * GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto.GameAppOpenAccessSecretResponse.AccessSecret 仅在签发 / 轮换响应中返回一次，前端须立即保存；后续列表 / 详情不再输出。
   */
  data: GameAppOpenAccessSecretResponse;
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

export type GameAppOpenAccessRevokePostData = any;

export type GameAppOpenAccessUpdatePostData = any;
