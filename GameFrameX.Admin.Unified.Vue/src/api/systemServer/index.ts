/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
  SystemServerAssemblyListGetData,
  SystemServerServerBaseGetData,
  SystemServerServerDiskGetData,
  SystemServerServerUsedGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemServer<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemServer
   * @name SystemServerAssemblyListGet
   * @summary 获取框架主要程序集
   * @request GET:/api/systemServer/assemblyList
   */
  systemServerAssemblyListGet = (params: RequestParams = {}) =>
    this.http.request<SystemServerAssemblyListGetData, any>({
      path: `/api/systemServer/assemblyList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemServer
   * @name SystemServerServerBaseGet
   * @summary 获取服务器配置信息
   * @request GET:/api/systemServer/serverBase
   */
  systemServerServerBaseGet = (params: RequestParams = {}) =>
    this.http.request<SystemServerServerBaseGetData, any>({
      path: `/api/systemServer/serverBase`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemServer
   * @name SystemServerServerDiskGet
   * @summary 获取服务器磁盘信息
   * @request GET:/api/systemServer/serverDisk
   */
  systemServerServerDiskGet = (params: RequestParams = {}) =>
    this.http.request<SystemServerServerDiskGetData, any>({
      path: `/api/systemServer/serverDisk`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemServer
   * @name SystemServerServerUsedGet
   * @summary 获取服务器使用信息
   * @request GET:/api/systemServer/serverUsed
   */
  systemServerServerUsedGet = (params: RequestParams = {}) =>
    this.http.request<SystemServerServerUsedGetData, any>({
      path: `/api/systemServer/serverUsed`,
      method: "GET",
      format: "json",
      ...params,
    });
}

export const systemServerAssemblyListGet = (...args: Parameters<InstanceType<typeof SystemServer>['systemServerAssemblyListGet']>) => getAPI(SystemServer).systemServerAssemblyListGet(...args);
export const systemServerServerBaseGet = (...args: Parameters<InstanceType<typeof SystemServer>['systemServerServerBaseGet']>) => getAPI(SystemServer).systemServerServerBaseGet(...args);
export const systemServerServerDiskGet = (...args: Parameters<InstanceType<typeof SystemServer>['systemServerServerDiskGet']>) => getAPI(SystemServer).systemServerServerDiskGet(...args);
export const systemServerServerUsedGet = (...args: Parameters<InstanceType<typeof SystemServer>['systemServerServerUsedGet']>) => getAPI(SystemServer).systemServerServerUsedGet(...args);
