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
  AddGameAppAssetPackageVersionHistoryRequest,
  PageGameAppAssetPackageVersionHistoryRequest,
  TenantGameAppAssetPackageVersionHistoryAddPostData,
  TenantGameAppAssetPackageVersionHistoryDeletePostData,
  TenantGameAppAssetPackageVersionHistoryDetailGetData,
  TenantGameAppAssetPackageVersionHistoryPagePostData,
  TenantGameAppAssetPackageVersionHistoryUpdatePostData,
} from "./types";
import {
  BaseDeleteRequest,
  BaseUpdateRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppAssetPackageVersionHistory<
  SecurityDataType = unknown,
> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersionHistory
   * @name TenantGameAppAssetPackageVersionHistoryAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppAssetPackageVersionHistory/add
   */
  tenantGameAppAssetPackageVersionHistoryAddPost = (
    data: AddGameAppAssetPackageVersionHistoryRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionHistoryAddPostData, any>({
      path: `/api/tenantGameAppAssetPackageVersionHistory/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersionHistory
   * @name TenantGameAppAssetPackageVersionHistoryDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppAssetPackageVersionHistory/delete
   */
  tenantGameAppAssetPackageVersionHistoryDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameAppAssetPackageVersionHistoryDeletePostData,
      any
    >({
      path: `/api/tenantGameAppAssetPackageVersionHistory/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersionHistory
   * @name TenantGameAppAssetPackageVersionHistoryDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppAssetPackageVersionHistory/detail
   */
  tenantGameAppAssetPackageVersionHistoryDetailGet = (
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
    this.http.request<
      TenantGameAppAssetPackageVersionHistoryDetailGetData,
      any
    >({
      path: `/api/tenantGameAppAssetPackageVersionHistory/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersionHistory
   * @name TenantGameAppAssetPackageVersionHistoryPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppAssetPackageVersionHistory/page
   */
  tenantGameAppAssetPackageVersionHistoryPagePost = (
    data: PageGameAppAssetPackageVersionHistoryRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageVersionHistoryPagePostData, any>(
      {
        path: `/api/tenantGameAppAssetPackageVersionHistory/page`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      },
    );
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackageVersionHistory
   * @name TenantGameAppAssetPackageVersionHistoryUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppAssetPackageVersionHistory/update
   */
  tenantGameAppAssetPackageVersionHistoryUpdatePost = (
    data: BaseUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameAppAssetPackageVersionHistoryUpdatePostData,
      any
    >({
      path: `/api/tenantGameAppAssetPackageVersionHistory/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppAssetPackageVersionHistoryAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersionHistory>['tenantGameAppAssetPackageVersionHistoryAddPost']>) => getAPI(TenantGameAppAssetPackageVersionHistory).tenantGameAppAssetPackageVersionHistoryAddPost(...args);
export const tenantGameAppAssetPackageVersionHistoryDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersionHistory>['tenantGameAppAssetPackageVersionHistoryDeletePost']>) => getAPI(TenantGameAppAssetPackageVersionHistory).tenantGameAppAssetPackageVersionHistoryDeletePost(...args);
export const tenantGameAppAssetPackageVersionHistoryDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersionHistory>['tenantGameAppAssetPackageVersionHistoryDetailGet']>) => getAPI(TenantGameAppAssetPackageVersionHistory).tenantGameAppAssetPackageVersionHistoryDetailGet(...args);
export const tenantGameAppAssetPackageVersionHistoryPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersionHistory>['tenantGameAppAssetPackageVersionHistoryPagePost']>) => getAPI(TenantGameAppAssetPackageVersionHistory).tenantGameAppAssetPackageVersionHistoryPagePost(...args);
export const tenantGameAppAssetPackageVersionHistoryUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackageVersionHistory>['tenantGameAppAssetPackageVersionHistoryUpdatePost']>) => getAPI(TenantGameAppAssetPackageVersionHistory).tenantGameAppAssetPackageVersionHistoryUpdatePost(...args);
