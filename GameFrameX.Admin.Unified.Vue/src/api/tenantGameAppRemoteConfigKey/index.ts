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
  AddGameAppRemoteConfigKeyRequest,
  PageGameAppRemoteConfigKeyRequest,
  TenantGameAppRemoteConfigKeyAddPostData,
  TenantGameAppRemoteConfigKeyDeletePostData,
  TenantGameAppRemoteConfigKeyDetailGetData,
  TenantGameAppRemoteConfigKeyPagePostData,
  TenantGameAppRemoteConfigKeyUpdatePostData,
  UpdateGameAppRemoteConfigKeyRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppRemoteConfigKey<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigKey
   * @name TenantGameAppRemoteConfigKeyAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppRemoteConfigKey/add
   */
  tenantGameAppRemoteConfigKeyAddPost = (
    data: AddGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigKeyAddPostData, any>({
      path: `/api/tenantGameAppRemoteConfigKey/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigKey
   * @name TenantGameAppRemoteConfigKeyDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppRemoteConfigKey/delete
   */
  tenantGameAppRemoteConfigKeyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigKeyDeletePostData, any>({
      path: `/api/tenantGameAppRemoteConfigKey/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigKey
   * @name TenantGameAppRemoteConfigKeyDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppRemoteConfigKey/detail
   */
  tenantGameAppRemoteConfigKeyDetailGet = (
    query: {
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigKeyDetailGetData, any>({
      path: `/api/tenantGameAppRemoteConfigKey/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigKey
   * @name TenantGameAppRemoteConfigKeyPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppRemoteConfigKey/page
   */
  tenantGameAppRemoteConfigKeyPagePost = (
    data: PageGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigKeyPagePostData, any>({
      path: `/api/tenantGameAppRemoteConfigKey/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigKey
   * @name TenantGameAppRemoteConfigKeyUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppRemoteConfigKey/update
   */
  tenantGameAppRemoteConfigKeyUpdatePost = (
    data: UpdateGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigKeyUpdatePostData, any>({
      path: `/api/tenantGameAppRemoteConfigKey/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppRemoteConfigKeyAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigKey>['tenantGameAppRemoteConfigKeyAddPost']>) => getAPI(TenantGameAppRemoteConfigKey).tenantGameAppRemoteConfigKeyAddPost(...args);
export const tenantGameAppRemoteConfigKeyDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigKey>['tenantGameAppRemoteConfigKeyDeletePost']>) => getAPI(TenantGameAppRemoteConfigKey).tenantGameAppRemoteConfigKeyDeletePost(...args);
export const tenantGameAppRemoteConfigKeyDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigKey>['tenantGameAppRemoteConfigKeyDetailGet']>) => getAPI(TenantGameAppRemoteConfigKey).tenantGameAppRemoteConfigKeyDetailGet(...args);
export const tenantGameAppRemoteConfigKeyPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigKey>['tenantGameAppRemoteConfigKeyPagePost']>) => getAPI(TenantGameAppRemoteConfigKey).tenantGameAppRemoteConfigKeyPagePost(...args);
export const tenantGameAppRemoteConfigKeyUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigKey>['tenantGameAppRemoteConfigKeyUpdatePost']>) => getAPI(TenantGameAppRemoteConfigKey).tenantGameAppRemoteConfigKeyUpdatePost(...args);
