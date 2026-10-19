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
  AddGameAppAssetPackageVersionRequest,
  GameAppAssetPackageVersionReturnByIdRequest,
  PageGameAppAssetPackageVersionRequest,
  PageGameAppAssetPackageVersionSelectRequest,
  TenantGameAppAssetPackageVersionAddPostData,
  TenantGameAppAssetPackageVersionDeletePostData,
  TenantGameAppAssetPackageVersionDetailGetData,
  TenantGameAppAssetPackageVersionPagePostData,
  TenantGameAppAssetPackageVersionReturnPostData,
  TenantGameAppAssetPackageVersionSelectPostData,
  TenantGameAppAssetPackageVersionUpdatePostData,
  UpdateGameAppAssetPackageVersionRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppAssetPackageVersion<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppAssetPackageVersion/add
   */
  tenantGameAppAssetPackageVersionAddPost = (
    data: AddGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionAddPostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppAssetPackageVersion/delete
   */
  tenantGameAppAssetPackageVersionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionDeletePostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppAssetPackageVersion/detail
   */
  tenantGameAppAssetPackageVersionDetailGet = (
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
    this.http.request<TenantGameAppAssetPackageVersionDetailGetData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppAssetPackageVersion/page
   */
  tenantGameAppAssetPackageVersionPagePost = (
    data: PageGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionPagePostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionReturnPost
   * @summary 回退资源包版本
   * @request POST:/api/tenantGameAppAssetPackageVersion/return
   */
  tenantGameAppAssetPackageVersionReturnPost = (
    data: GameAppAssetPackageVersionReturnByIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionReturnPostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/return`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionSelectPost
   * @summary 获取当前应用 + 当前分包下可选的资源版本列表。
   * @request POST:/api/tenantGameAppAssetPackageVersion/select
   */
  tenantGameAppAssetPackageVersionSelectPost = (
    data: PageGameAppAssetPackageVersionSelectRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionSelectPostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersion
   * @name TenantGameAppAssetPackageVersionUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppAssetPackageVersion/update
   */
  tenantGameAppAssetPackageVersionUpdatePost = (
    data: UpdateGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionUpdatePostData, any>({
      path: `/api/tenantGameAppAssetPackageVersion/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppAssetPackageVersionAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionAddPost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionAddPost(...args);
export const tenantGameAppAssetPackageVersionDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionDeletePost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionDeletePost(...args);
export const tenantGameAppAssetPackageVersionDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionDetailGet']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionDetailGet(...args);
export const tenantGameAppAssetPackageVersionPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionPagePost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionPagePost(...args);
export const tenantGameAppAssetPackageVersionReturnPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionReturnPost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionReturnPost(...args);
export const tenantGameAppAssetPackageVersionSelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionSelectPost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionSelectPost(...args);
export const tenantGameAppAssetPackageVersionUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersion>['tenantGameAppAssetPackageVersionUpdatePost']>) => getAPI(TenantGameAppAssetPackageVersion).tenantGameAppAssetPackageVersionUpdatePost(...args);
