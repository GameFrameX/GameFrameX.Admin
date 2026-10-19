/* eslint-disable */
/* tslint:disable */

/** @format int32 */
export enum PlatformTypeEnum {
  /** 微信公众号 */
  Value微信公众号 = 1,
  /** 微信小程序 */
  Value微信小程序 = 2,
  /** QQ */
  QQ = 3,
  /** 支付宝 */
  Alipay = 4,
  /** Gitee */
  Gitee = 5,
}

/** 增加微信用户输入参数（仅业务可写字段；租户/审计等系统字段由服务端注入，AccessToken/RefreshToken 凭证禁止经请求写入） */
export interface AddWechatUserRequest {
  /** 租户用户Id */
  userId?: string;
  platformType?: PlatformTypeEnum;
  /** OpenId */
  openId?: string | null;
  /** 会话密钥 */
  sessionKey?: string | null;
  /** UnionId */
  unionId?: string | null;
  /** 昵称 */
  nickName?: string | null;
  /** 头像 */
  avatar?: string | null;
  /** 手机号码 */
  mobile?: string | null;
  /**
   * 性别
   * @format int32
   */
  sex?: number | null;
  /** 语言 */
  language?: string | null;
  /** 城市 */
  city?: string | null;
  /** 省 */
  province?: string | null;
  /** 国家 */
  country?: string | null;
  /**
   * 过期时间
   * @format int32
   */
  expiresIn?: number | null;
  /** 用户授权的作用域，使用逗号分隔 */
  scope?: string | null;
}

export interface DeleteWechatUserRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface PageWechatUserRequest {
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
  /** 昵称 */
  nickName?: string | null;
  /** 手机号码 */
  phoneNumber?: string | null;
}

/** 租户微信用户绑定输出参数 */
export interface TenantWechatUserResponse {
  /** Id */
  id?: string;
  /** 租户用户Id */
  userId?: string;
  platformType?: PlatformTypeEnum;
  /** OpenId */
  openId?: string | null;
  /** 会话密钥 */
  sessionKey?: string | null;
  /** UnionId */
  unionId?: string | null;
  /** 昵称 */
  nickName?: string | null;
  /** 头像 */
  avatar?: string | null;
  /** 手机号码 */
  mobile?: string | null;
  /**
   * 性别
   * @format int32
   */
  sex?: number | null;
  /** 语言 */
  language?: string | null;
  /** 城市 */
  city?: string | null;
  /** 省 */
  province?: string | null;
  /** 国家 */
  country?: string | null;
  /**
   * 过期时间
   * @format int32
   */
  expiresIn?: number | null;
  /** 用户授权的作用域，使用逗号分隔 */
  scope?: string | null;
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

export interface TenantWechatUserResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantWechatUserResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 更新微信用户输入参数（乐观锁依赖 RowVersion，未携带时跳过版本比对） */
export interface UpdateWechatUserRequest {
  /** 租户用户Id */
  userId?: string;
  platformType?: PlatformTypeEnum;
  /** OpenId */
  openId?: string | null;
  /** 会话密钥 */
  sessionKey?: string | null;
  /** UnionId */
  unionId?: string | null;
  /** 昵称 */
  nickName?: string | null;
  /** 头像 */
  avatar?: string | null;
  /** 手机号码 */
  mobile?: string | null;
  /**
   * 性别
   * @format int32
   */
  sex?: number | null;
  /** 语言 */
  language?: string | null;
  /** 城市 */
  city?: string | null;
  /** 省 */
  province?: string | null;
  /** 国家 */
  country?: string | null;
  /**
   * 过期时间
   * @format int32
   */
  expiresIn?: number | null;
  /** 用户授权的作用域，使用逗号分隔 */
  scope?: string | null;
  /**
   * 主键Id
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改 */
  rowVersion?: string | null;
}

export type TenantWechatUserAddPostData = any;

export type TenantWechatUserDeletePostData = any;

export interface TenantWechatUserPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantWechatUserResponseSqlSugarPagedList;
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

export type TenantWechatUserUpdatePostData = any;
