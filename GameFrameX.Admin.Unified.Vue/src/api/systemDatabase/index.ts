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
  DbColumnRequest,
  DbTableRequest,
  DeleteDbColumnRequest,
  DeleteDbTableRequest,
  SystemDatabaseAddColumnPostData,
  SystemDatabaseAddTablePostData,
  SystemDatabaseColumnListGetData,
  SystemDatabaseDbTypeListGetData,
  SystemDatabaseDeleteColumnPostData,
  SystemDatabaseDeleteTablePostData,
  SystemDatabaseListGetData,
  SystemDatabaseTableListGetData,
  SystemDatabaseUpdateColumnPostData,
  SystemDatabaseUpdateTablePostData,
  UpdateDbColumnRequest,
  UpdateDbTableRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemDatabase<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseAddColumnPost
   * @summary 增加列
   * @request POST:/api/systemDatabase/addColumn
   */
  systemDatabaseAddColumnPost = (
    data: DbColumnRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseAddColumnPostData, any>({
      path: `/api/systemDatabase/addColumn`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseAddTablePost
   * @summary 增加表
   * @request POST:/api/systemDatabase/addTable
   */
  systemDatabaseAddTablePost = (
    data: DbTableRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseAddTablePostData, any>({
      path: `/api/systemDatabase/addTable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseColumnListGet
   * @summary 获取字段列表
   * @request GET:/api/systemDatabase/columnList
   */
  systemDatabaseColumnListGet = (
    query?: {
      /** 表名 */
      tableName?: string;
      /** ConfigId，未传时默认主库 */
      configId?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseColumnListGetData, any>({
      path: `/api/systemDatabase/columnList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseDbTypeListGet
   * @summary 获取数据库数据类型列表
   * @request GET:/api/systemDatabase/dbTypeList/{configId}
   */
  systemDatabaseDbTypeListGet = (
    configId: string,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseDbTypeListGetData, any>({
      path: `/api/systemDatabase/dbTypeList/${configId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseDeleteColumnPost
   * @summary 删除列
   * @request POST:/api/systemDatabase/deleteColumn
   */
  systemDatabaseDeleteColumnPost = (
    data: DeleteDbColumnRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseDeleteColumnPostData, any>({
      path: `/api/systemDatabase/deleteColumn`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseDeleteTablePost
   * @summary 删除表
   * @request POST:/api/systemDatabase/deleteTable
   */
  systemDatabaseDeleteTablePost = (
    data: DeleteDbTableRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseDeleteTablePostData, any>({
      path: `/api/systemDatabase/deleteTable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseListGet
   * @summary 获取库列表
   * @request GET:/api/systemDatabase/list
   */
  systemDatabaseListGet = (params: RequestParams = {}) =>
    this.http.request<SystemDatabaseListGetData, any>({
      path: `/api/systemDatabase/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseTableListGet
   * @summary 获取表列表
   * @request GET:/api/systemDatabase/tableList/{configId}
   */
  systemDatabaseTableListGet = (configId: string, params: RequestParams = {}) =>
    this.http.request<SystemDatabaseTableListGetData, any>({
      path: `/api/systemDatabase/tableList/${configId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseUpdateColumnPost
   * @summary 编辑列
   * @request POST:/api/systemDatabase/updateColumn
   */
  systemDatabaseUpdateColumnPost = (
    data: UpdateDbColumnRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseUpdateColumnPostData, any>({
      path: `/api/systemDatabase/updateColumn`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemDatabase
   * @name SystemDatabaseUpdateTablePost
   * @summary 编辑表
   * @request POST:/api/systemDatabase/updateTable
   */
  systemDatabaseUpdateTablePost = (
    data: UpdateDbTableRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemDatabaseUpdateTablePostData, any>({
      path: `/api/systemDatabase/updateTable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemDatabaseAddColumnPost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseAddColumnPost']>) => getAPI(SystemDatabase).systemDatabaseAddColumnPost(...args);
export const systemDatabaseAddTablePost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseAddTablePost']>) => getAPI(SystemDatabase).systemDatabaseAddTablePost(...args);
export const systemDatabaseColumnListGet = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseColumnListGet']>) => getAPI(SystemDatabase).systemDatabaseColumnListGet(...args);
export const systemDatabaseDbTypeListGet = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseDbTypeListGet']>) => getAPI(SystemDatabase).systemDatabaseDbTypeListGet(...args);
export const systemDatabaseDeleteColumnPost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseDeleteColumnPost']>) => getAPI(SystemDatabase).systemDatabaseDeleteColumnPost(...args);
export const systemDatabaseDeleteTablePost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseDeleteTablePost']>) => getAPI(SystemDatabase).systemDatabaseDeleteTablePost(...args);
export const systemDatabaseListGet = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseListGet']>) => getAPI(SystemDatabase).systemDatabaseListGet(...args);
export const systemDatabaseTableListGet = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseTableListGet']>) => getAPI(SystemDatabase).systemDatabaseTableListGet(...args);
export const systemDatabaseUpdateColumnPost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseUpdateColumnPost']>) => getAPI(SystemDatabase).systemDatabaseUpdateColumnPost(...args);
export const systemDatabaseUpdateTablePost = (...args: Parameters<InstanceType<typeof SystemDatabase>['systemDatabaseUpdateTablePost']>) => getAPI(SystemDatabase).systemDatabaseUpdateTablePost(...args);
