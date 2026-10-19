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
  AddGameAppReleaseVersionRequest,
  PageGameAppReleaseVersionRequest,
  TenantGameAppReleaseVersionAddPostData,
  TenantGameAppReleaseVersionDeletePostData,
  TenantGameAppReleaseVersionDetailGetData,
  TenantGameAppReleaseVersionPagePostData,
  TenantGameAppReleaseVersionUpdatePostData,
  UpdateGameAppReleaseVersionRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppReleaseVersion<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppReleaseVersion
   * @name TenantGameAppReleaseVersionAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppReleaseVersion/add
   */
  tenantGameAppReleaseVersionAddPost = (
    data: AddGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppReleaseVersionAddPostData, any>({
      path: `/api/tenantGameAppReleaseVersion/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppReleaseVersion
   * @name TenantGameAppReleaseVersionDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppReleaseVersion/delete
   */
  tenantGameAppReleaseVersionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppReleaseVersionDeletePostData, any>({
      path: `/api/tenantGameAppReleaseVersion/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppReleaseVersion
   * @name TenantGameAppReleaseVersionDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppReleaseVersion/detail
   */
  tenantGameAppReleaseVersionDetailGet = (
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
    this.http.request<TenantGameAppReleaseVersionDetailGetData, any>({
      path: `/api/tenantGameAppReleaseVersion/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppReleaseVersion
   * @name TenantGameAppReleaseVersionPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppReleaseVersion/page
   */
  tenantGameAppReleaseVersionPagePost = (
    data: PageGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppReleaseVersionPagePostData, any>({
      path: `/api/tenantGameAppReleaseVersion/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppReleaseVersion
   * @name TenantGameAppReleaseVersionUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppReleaseVersion/update
   */
  tenantGameAppReleaseVersionUpdatePost = (
    data: UpdateGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppReleaseVersionUpdatePostData, any>({
      path: `/api/tenantGameAppReleaseVersion/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppReleaseVersionAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppReleaseVersion>['tenantGameAppReleaseVersionAddPost']>) => getAPI(TenantGameAppReleaseVersion).tenantGameAppReleaseVersionAddPost(...args);
export const tenantGameAppReleaseVersionDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppReleaseVersion>['tenantGameAppReleaseVersionDeletePost']>) => getAPI(TenantGameAppReleaseVersion).tenantGameAppReleaseVersionDeletePost(...args);
export const tenantGameAppReleaseVersionDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppReleaseVersion>['tenantGameAppReleaseVersionDetailGet']>) => getAPI(TenantGameAppReleaseVersion).tenantGameAppReleaseVersionDetailGet(...args);
export const tenantGameAppReleaseVersionPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppReleaseVersion>['tenantGameAppReleaseVersionPagePost']>) => getAPI(TenantGameAppReleaseVersion).tenantGameAppReleaseVersionPagePost(...args);
export const tenantGameAppReleaseVersionUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppReleaseVersion>['tenantGameAppReleaseVersionUpdatePost']>) => getAPI(TenantGameAppReleaseVersion).tenantGameAppReleaseVersionUpdatePost(...args);
