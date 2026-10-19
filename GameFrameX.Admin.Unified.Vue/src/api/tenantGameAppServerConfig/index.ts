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
  AddGameAppServerConfigRequest,
  PageGameAppServerConfigRequest,
  TenantGameAppServerConfigAddPostData,
  TenantGameAppServerConfigDeletePostData,
  TenantGameAppServerConfigDetailGetData,
  TenantGameAppServerConfigPagePostData,
  TenantGameAppServerConfigUpdatePostData,
  UpdateGameAppServerConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppServerConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppServerConfig
   * @name TenantGameAppServerConfigAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppServerConfig/add
   */
  tenantGameAppServerConfigAddPost = (
    data: AddGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppServerConfigAddPostData, any>({
      path: `/api/tenantGameAppServerConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppServerConfig
   * @name TenantGameAppServerConfigDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppServerConfig/delete
   */
  tenantGameAppServerConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppServerConfigDeletePostData, any>({
      path: `/api/tenantGameAppServerConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppServerConfig
   * @name TenantGameAppServerConfigDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppServerConfig/detail
   */
  tenantGameAppServerConfigDetailGet = (
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
    this.http.request<TenantGameAppServerConfigDetailGetData, any>({
      path: `/api/tenantGameAppServerConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppServerConfig
   * @name TenantGameAppServerConfigPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppServerConfig/page
   */
  tenantGameAppServerConfigPagePost = (
    data: PageGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppServerConfigPagePostData, any>({
      path: `/api/tenantGameAppServerConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppServerConfig
   * @name TenantGameAppServerConfigUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppServerConfig/update
   */
  tenantGameAppServerConfigUpdatePost = (
    data: UpdateGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppServerConfigUpdatePostData, any>({
      path: `/api/tenantGameAppServerConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppServerConfigAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppServerConfig>['tenantGameAppServerConfigAddPost']>) => getAPI(TenantGameAppServerConfig).tenantGameAppServerConfigAddPost(...args);
export const tenantGameAppServerConfigDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppServerConfig>['tenantGameAppServerConfigDeletePost']>) => getAPI(TenantGameAppServerConfig).tenantGameAppServerConfigDeletePost(...args);
export const tenantGameAppServerConfigDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppServerConfig>['tenantGameAppServerConfigDetailGet']>) => getAPI(TenantGameAppServerConfig).tenantGameAppServerConfigDetailGet(...args);
export const tenantGameAppServerConfigPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppServerConfig>['tenantGameAppServerConfigPagePost']>) => getAPI(TenantGameAppServerConfig).tenantGameAppServerConfigPagePost(...args);
export const tenantGameAppServerConfigUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppServerConfig>['tenantGameAppServerConfigUpdatePost']>) => getAPI(TenantGameAppServerConfig).tenantGameAppServerConfigUpdatePost(...args);
