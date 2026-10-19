/* eslint-disable */
/* tslint:disable */

/** 游戏内横幅公告增加输入参数 */
export interface AddTenantGameBannerAnnouncementEntityRequest {
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
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  /** 区服ID */
  areaId: string;
  /**
   * 内容
   * @minLength 1
   */
  content: string;
  /** 内容本地化Key。可空：空 = 直接展示 Content（服务端兜底空串，实体列 NOT NULL）。 */
  contentLocalizationKey?: string | null;
  /**
   * 优先级
   * @format int32
   */
  priority: number;
  /**
   * 开始时间
   * @format int64
   */
  startTime: number;
  /**
   * 结束时间
   * @format int64
   */
  endTime: number;
  /**
   * 间隔时间
   * @format int32
   */
  interval: number;
  /**
   * 重复次数
   * @format int32
   * @min 1
   * @max 2147483647
   */
  repeatCount: number;
  /** 是否激活 */
  isActive: boolean;
}

/** 游戏内横幅公告分页查询输入参数 */
export interface PageTenantGameBannerAnnouncementEntityRequest {
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
  areaId?: string | null;
  /** 应用Id */
  appId?: string | null;
  /** 内容 */
  content?: string | null;
  /**
   * 优先级
   * @format int32
   */
  priority?: number | null;
  /**
   * 开始时间
   * @format int64
   */
  startTime?: number | null;
  /** 开始时间范围 */
  startTimeRange?: (number | null)[];
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
  /** 结束时间范围 */
  endTimeRange?: (number | null)[];
  /**
   * 间隔时间
   * @format int32
   */
  interval?: number | null;
  /**
   * 重复次数
   * @format int32
   */
  repeatCount?: number | null;
  /** 是否激活 */
  isActive?: boolean | null;
}

/** 游戏内横幅公告输出参数 */
export interface TenantGameBannerAnnouncementEntityResponse {
  /** Id */
  id?: string;
  /** 应用Id。运营内容应用作用域（TenantId + AppId），必须 > 0。 */
  appId?: string;
  /** 区服ID */
  areaId?: string;
  /** 内容 */
  content?: string | null;
  /** 内容本地化Key */
  contentLocalizationKey?: string | null;
  /**
   * 用于确定公告的显示优先级。
   * @format int32
   */
  priority?: number;
  /**
   * 公告开始显示的时间。
   * @format int64
   */
  startTime?: number;
  /**
   * 公告结束显示的时间。
   * @format int64
   */
  endTime?: number;
  /**
   * 公告滚动的时间间隔或频率。单位秒
   * @format int32
   */
  interval?: number;
  /**
   * 公告重复播放次数。
   * @format int32
   */
  repeatCount?: number;
  /** 指示公告当前是否应该被激活或显示。 */
  isActive?: boolean;
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

export interface TenantGameBannerAnnouncementEntityResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantGameBannerAnnouncementEntityResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

/** 游戏内横幅公告更新输入参数 */
export interface UpdateTenantGameBannerAnnouncementEntityRequest {
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
  /** 应用Id。运营内容应用作用域，必须 > 0。 */
  appId: string;
  /** 区服ID */
  areaId: string;
  /**
   * 内容
   * @minLength 1
   */
  content: string;
  /** 内容本地化Key。可空：空 = 直接展示 Content（服务端兜底空串，实体列 NOT NULL）。 */
  contentLocalizationKey?: string | null;
  /**
   * 优先级
   * @format int32
   */
  priority: number;
  /**
   * 开始时间
   * @format int64
   */
  startTime: number;
  /**
   * 结束时间
   * @format int64
   */
  endTime: number;
  /**
   * 间隔时间
   * @format int32
   */
  interval: number;
  /**
   * 重复次数
   * @format int32
   * @min 1
   * @max 2147483647
   */
  repeatCount: number;
  /** 是否激活 */
  isActive: boolean;
}

export interface TenantGameBannerAnnouncementEntityAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏内横幅公告输出参数 */
  data: TenantGameBannerAnnouncementEntityResponse;
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

export type TenantGameBannerAnnouncementEntityDeletePostData = any;

export interface TenantGameBannerAnnouncementEntityDetailGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** 游戏内横幅公告输出参数 */
  data: TenantGameBannerAnnouncementEntityResponse;
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

export interface TenantGameBannerAnnouncementEntityPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantGameBannerAnnouncementEntityResponseSqlSugarPagedList;
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

export type TenantGameBannerAnnouncementEntityUpdatePostData = any;
