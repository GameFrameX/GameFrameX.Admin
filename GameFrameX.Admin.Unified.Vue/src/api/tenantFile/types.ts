/* eslint-disable */
/* tslint:disable */

export interface DeleteFileRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
}

export interface FileRequest {
  /**
   * @min 1
   * @max 9223372036854776000
   */
  id: string;
  /** 文件名称 */
  fileName?: string | null;
  /** 文件Url */
  url?: string | null;
}

export interface FileResponse {
  /** Id */
  id?: string;
  /** 提供者 */
  provider?: string | null;
  /** 名称 */
  name?: string | null;
  /** URL */
  url?: string | null;
  /** 大小 */
  sizeKb?: string | null;
  /** 后缀 */
  suffix?: string | null;
  /** 路径 */
  filePath?: string | null;
  /** 文件名称 */
  fileName?: string | null;
}

export interface PageFileRequest {
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
  /** 文件名称 */
  fileName?: string | null;
  /**
   * 开始时间
   * @format int64
   */
  startTime?: number | null;
  /**
   * 结束时间
   * @format int64
   */
  endTime?: number | null;
}

/** 系统文件输出参数 */
export interface TenantFileResponse {
  /** Id */
  id?: string;
  /** 提供者 */
  provider?: string | null;
  /**
   * 关联的租户对象存储配置 ID（TenantObjectStorageConfigEntity.Id）。
   * 新上传文件必填，用于按精确配置解析下载/删除路径；历史记录为 null 时按本地 legacy 行为兜底。
   */
  storageConfigId?: string | null;
  /** 仓储名称 */
  bucketName?: string | null;
  /** 文件名称（源文件名） */
  fileName?: string | null;
  /** 文件后缀 */
  suffix?: string | null;
  /** 存储路径 */
  filePath?: string | null;
  /** 文件大小KB */
  sizeKb?: string | null;
  /** 文件大小信息-计算后的 */
  sizeInfo?: string | null;
  /** 外链地址-OSS上传后生成外链地址方便前端预览 */
  url?: string | null;
  /** 文件MD5 */
  fileMd5?: string | null;
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

export interface TenantFileResponseSqlSugarPagedList {
  /** @format int32 */
  page?: number;
  /** @format int32 */
  pageSize?: number;
  /** @format int32 */
  total?: number;
  /** @format int32 */
  totalPages?: number;
  items?: TenantFileResponse[] | null;
  hasPrevPage?: boolean;
  hasNextPage?: boolean;
}

export interface UploadFileFromBase64Request {
  /** 文件内容 */
  fileDataBase64?: string | null;
  /** 文件类型( "image/jpeg",) */
  contentType?: string | null;
  /** 文件名称 */
  fileName?: string | null;
  /** 保存路径 */
  path?: string | null;
}

export type TenantFileDeletePostData = any;

export type TenantFileDownloadFilePostData = any;

export interface TenantFilePagePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: TenantFileResponseSqlSugarPagedList;
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

export type TenantFileUpdatePostData = any;

export interface TenantFileUploadAvatarPostPayload {
  /** @format binary */
  file: File;
}

export interface TenantFileUploadAvatarPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: FileResponse;
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

export interface TenantFileUploadFilePostPayload {
  /** @format binary */
  file: File;
}

export interface TenantFileUploadFilePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: FileResponse;
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

export interface TenantFileUploadFileFromBase64PostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: FileResponse;
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

export interface TenantFileUploadFilesPostPayload {
  files: File[];
}

export interface TenantFileUploadFilesPostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: FileResponse[];
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

export interface TenantFileUploadSignaturePostPayload {
  /** @format binary */
  file: File;
}

export interface TenantFileUploadSignaturePostData {
  /**
   * 响应码，0 表示成功，失败时承载统一数字业务错误码（模块枚举 8 位数值，见 GameFrameX.Core/Enum/Errors 与 error-codes.json 码表）
   * @format int32
   */
  code: number;
  /** 错误或提示信息 */
  message: string;
  data: FileResponse;
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
