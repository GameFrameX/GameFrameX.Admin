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
  AddGameAppAssetPackageRequest,
  PageGameAppAssetPackageRequest,
  TenantGameAppAssetPackageAddPostData,
  TenantGameAppAssetPackageDeletePostData,
  TenantGameAppAssetPackageDetailGetData,
  TenantGameAppAssetPackagePagePostData,
  TenantGameAppAssetPackageSelectPostData,
  TenantGameAppAssetPackageSelectStringPostData,
  TenantGameAppAssetPackageUpdatePostData,
  UpdateGameAppAssetPackageRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppAssetPackage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppAssetPackage/add
   */
  tenantGameAppAssetPackageAddPost = (
    data: AddGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageAddPostData, any>({
      path: `/api/tenantGameAppAssetPackage/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppAssetPackage/delete
   */
  tenantGameAppAssetPackageDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageDeletePostData, any>({
      path: `/api/tenantGameAppAssetPackage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppAssetPackage/detail
   */
  tenantGameAppAssetPackageDetailGet = (
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
    this.http.request<TenantGameAppAssetPackageDetailGetData, any>({
      path: `/api/tenantGameAppAssetPackage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackagePagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppAssetPackage/page
   */
  tenantGameAppAssetPackagePagePost = (
    data: PageGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackagePagePostData, any>({
      path: `/api/tenantGameAppAssetPackage/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageSelectPost
   * @summary 获取当前应用下的资源包选择器列表。
   * @request POST:/api/tenantGameAppAssetPackage/select
   */
  tenantGameAppAssetPackageSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageSelectPostData, any>({
      path: `/api/tenantGameAppAssetPackage/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAppAssetPackage/selectString
   */
  tenantGameAppAssetPackageSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageSelectStringPostData, any>({
      path: `/api/tenantGameAppAssetPackage/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppAssetPackage
   * @name TenantGameAppAssetPackageUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppAssetPackage/update
   */
  tenantGameAppAssetPackageUpdatePost = (
    data: UpdateGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppAssetPackageUpdatePostData, any>({
      path: `/api/tenantGameAppAssetPackage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppAssetPackageAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageAddPost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageAddPost(...args);
export const tenantGameAppAssetPackageDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageDeletePost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageDeletePost(...args);
export const tenantGameAppAssetPackageDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageDetailGet']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageDetailGet(...args);
export const tenantGameAppAssetPackagePagePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackagePagePost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackagePagePost(...args);
export const tenantGameAppAssetPackageSelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageSelectPost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageSelectPost(...args);
export const tenantGameAppAssetPackageSelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageSelectStringPost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageSelectStringPost(...args);
export const tenantGameAppAssetPackageUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppAssetPackage>['tenantGameAppAssetPackageUpdatePost']>) => getAPI(TenantGameAppAssetPackage).tenantGameAppAssetPackageUpdatePost(...args);
