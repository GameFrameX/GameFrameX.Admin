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
  SystemCacheDeleteByPreKeyPostData,
  SystemCacheDeletePostData,
  SystemCacheKeyListGetData,
  SystemCacheKeysByPrefixKeyGetData,
  SystemCacheValueGetData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemCache<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemCache
   * @name SystemCacheDeletePost
   * @summary 删除缓存
   * @request POST:/api/systemCache/delete
   */
  systemCacheDeletePost = (
    query?: {
      key?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemCacheDeletePostData, any>({
      path: `/api/systemCache/delete`,
      method: "POST",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemCache
   * @name SystemCacheDeleteByPreKeyPost
   * @summary 根据键名前缀删除缓存
   * @request POST:/api/systemCache/deleteByPreKey
   */
  systemCacheDeleteByPreKeyPost = (
    query?: {
      /** 键名前缀 */
      prefixKey?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<SystemCacheDeleteByPreKeyPostData, any>({
      path: `/api/systemCache/deleteByPreKey`,
      method: "POST",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemCache
   * @name SystemCacheKeyListGet
   * @summary 获取缓存键名集合
   * @request GET:/api/systemCache/keyList
   */
  systemCacheKeyListGet = (params: RequestParams = {}) =>
    this.http.request<SystemCacheKeyListGetData, any>({
      path: `/api/systemCache/keyList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemCache
   * @name SystemCacheKeysByPrefixKeyGet
   * @summary 根据键名前缀获取键名集合
   * @request GET:/api/systemCache/keysByPrefixKey/{prefixKey}
   */
  systemCacheKeysByPrefixKeyGet = (
    prefixKey: string,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemCacheKeysByPrefixKeyGetData, any>({
      path: `/api/systemCache/keysByPrefixKey/${prefixKey}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemCache
   * @name SystemCacheValueGet
   * @summary 获取缓存值
   * @request GET:/api/systemCache/value/{key}
   */
  systemCacheValueGet = (key: string, params: RequestParams = {}) =>
    this.http.request<SystemCacheValueGetData, any>({
      path: `/api/systemCache/value/${key}`,
      method: "GET",
      ...params,
    });
}

export const systemCacheDeletePost = (...args: Parameters<InstanceType<typeof SystemCache>['systemCacheDeletePost']>) => getAPI(SystemCache).systemCacheDeletePost(...args);
export const systemCacheDeleteByPreKeyPost = (...args: Parameters<InstanceType<typeof SystemCache>['systemCacheDeleteByPreKeyPost']>) => getAPI(SystemCache).systemCacheDeleteByPreKeyPost(...args);
export const systemCacheKeyListGet = (...args: Parameters<InstanceType<typeof SystemCache>['systemCacheKeyListGet']>) => getAPI(SystemCache).systemCacheKeyListGet(...args);
export const systemCacheKeysByPrefixKeyGet = (...args: Parameters<InstanceType<typeof SystemCache>['systemCacheKeysByPrefixKeyGet']>) => getAPI(SystemCache).systemCacheKeysByPrefixKeyGet(...args);
export const systemCacheValueGet = (...args: Parameters<InstanceType<typeof SystemCache>['systemCacheValueGet']>) => getAPI(SystemCache).systemCacheValueGet(...args);
