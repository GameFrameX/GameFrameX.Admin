/* eslint-disable */
/* tslint:disable */

/** 框架主要程序集信息 */
export interface AssemblyInfoResponse {
  /** 程序集名称 */
  name?: string | null;
  /** 程序集版本号 */
  version?: string | null;
}

export interface AssemblyInfoResponseListHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  data?: AssemblyInfoResponse[] | null;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 服务器磁盘信息 */
export interface DiskInfoResponse {
  /** 磁盘名（Unix 为设备名及挂载点，Windows 为盘符） */
  diskName?: string | null;
  /** 文件系统类型名 */
  typeName?: string | null;
  /**
   * 总剩余空间（GB）
   * @format int64
   */
  totalFree?: number;
  /**
   * 总容量（GB）
   * @format int64
   */
  totalSize?: number;
  /**
   * 已使用空间（GB）
   * @format int64
   */
  used?: number;
  /**
   * 可用剩余空间（GB）
   * @format int64
   */
  availableFreeSpace?: number;
  /**
   * 使用百分比（%）
   * @format double
   */
  availablePercent?: number;
}

export interface DiskInfoResponseListHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  data?: DiskInfoResponse[] | null;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 服务器配置信息 */
export interface ServerBaseResponse {
  /** 主机名称 */
  hostName?: string | null;
  /** 操作系统 */
  systemOs?: string | null;
  /** 系统架构 */
  osArchitecture?: string | null;
  /** CPU 核心数 */
  processorCount?: string | null;
  /** 系统运行时间 */
  systemRunTime?: string | null;
  /** 外网地址（不可用时为“未知”） */
  remoteIp?: string | null;
  /** 本地地址（无法获取时为 null） */
  localIp?: string | null;
  /** .NET 框架描述 */
  frameworkDescription?: string | null;
  /** 运行环境名称（Development / Production） */
  environment?: string | null;
  /** 网站根目录（未映射时为 null） */
  wwwroot?: string | null;
  /** 是否 Stage 环境（Stage环境 / 非Stage环境） */
  stage?: string | null;
}

export interface ServerBaseResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 服务器配置信息 */
  data?: ServerBaseResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

/** 服务器使用信息 */
export interface ServerUsedResponse {
  /** 空闲内存 */
  freeRam?: string | null;
  /** 已用内存 */
  usedRam?: string | null;
  /** 总内存 */
  totalRam?: string | null;
  /** 内存使用率 */
  ramRate?: string | null;
  /** CPU 使用率 */
  cpuRate?: string | null;
  /** 服务启动时间（yyyy-MM-dd HH:mm:ss） */
  startTime?: string | null;
  /** 服务运行时间 */
  runTime?: string | null;
}

export interface ServerUsedResponseHttpJsonResultData {
  /** @format int32 */
  code?: number;
  message?: string | null;
  /** 服务器使用信息 */
  data?: ServerUsedResponse;
  trackId?: string | null;
  errorCode?: string | null;
  type?: string | null;
  /** @format int64 */
  time?: number;
  extras?: any | null;
}

export type SystemServerAssemblyListGetData =
  AssemblyInfoResponseListHttpJsonResultData;

export type SystemServerServerBaseGetData =
  ServerBaseResponseHttpJsonResultData;

export type SystemServerServerDiskGetData =
  DiskInfoResponseListHttpJsonResultData;

export type SystemServerServerUsedGetData =
  ServerUsedResponseHttpJsonResultData;
