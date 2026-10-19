/* eslint-disable */
/* tslint:disable */

/**
 * 新增/编辑行政地区输入参数。
 * 显式声明前端表单可填字段（与前端 beforeSubmit 白名单一致），不再继承 GameFrameX.Entity.System.SystemRegionEntity，避免种子数据字段（ShortName、MergerName、ZipCode、Level、PinYin、Lng、Lat）与子节点导航属性被前端批量赋值。
 */
export interface AddRegionRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 父Id */
  pid?: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /** 行政代码 */
  code?: string | null;
  /** 区号 */
  cityCode?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
}

export interface DeleteRegionRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface PageRegionRequest {
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
  /** 父节点Id */
  pid?: string;
  /** 名称 */
  name?: string | null;
  /** 编码 */
  code?: string | null;
}

/** 系统行政地区输出参数 */
export interface SystemRegionResponse {
  /** Id */
  id?: string;
  /** 父Id */
  pid?: string;
  /** 名称 */
  name?: string | null;
  /** 简称 */
  shortName?: string | null;
  /** 组合名 */
  mergerName?: string | null;
  /** 行政代码 */
  code?: string | null;
  /** 邮政编码 */
  zipCode?: string | null;
  /** 区号 */
  cityCode?: string | null;
  /**
   * 层级
   * @format int32
   */
  level?: number;
  /** 拼音 */
  pinYin?: string | null;
  /**
   * 经度
   * @format float
   */
  lng?: number;
  /**
   * 维度
   * @format float
   */
  lat?: number;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
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

export interface SystemRegionResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: SystemRegionResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface UpdateRegionRequest {
  /** 主键Id（新增为 0，编辑由前端回填） */
  id?: string;
  /** 父Id */
  pid?: string;
  /**
   * 名称
   * @minLength 1
   */
  name: string;
  /** 行政代码 */
  code?: string | null;
  /** 区号 */
  cityCode?: string | null;
  /**
   * 排序
   * @format int32
   */
  orderNo?: number;
  /** 备注 */
  remark?: string | null;
  /** 乐观锁版本号 */
  rowVersion?: string | null;
}

export interface SystemRegionAddPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  /** @format int64 */
  data: number;
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

export type SystemRegionDeletePostData = any;

export interface SystemRegionListGetData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: SystemRegionResponse[];
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

export interface SystemRegionPagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: SystemRegionResponseSqlSugarPagedList;
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

export type SystemRegionSyncPostData = any;

export type SystemRegionUpdatePostData = any;
