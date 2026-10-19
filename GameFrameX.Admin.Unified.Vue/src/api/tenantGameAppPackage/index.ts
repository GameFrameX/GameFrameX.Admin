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
  AddGameAppPackageRequest,
  PageGameAppPackageRequest,
  SetGameAppPackageStatusRequest,
  TenantGameAppPackageAddPostData,
  TenantGameAppPackageDeletePostData,
  TenantGameAppPackageDetailGetData,
  TenantGameAppPackageListGetData,
  TenantGameAppPackagePagePostData,
  TenantGameAppPackageSelectPostData,
  TenantGameAppPackageSelectStringPostData,
  TenantGameAppPackageSetDefaultPostData,
  TenantGameAppPackageSetStatusPostData,
  TenantGameAppPackageUpdatePostData,
  UpdateGameAppPackageRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppPackage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageAddPost
   * @summary 新增分包
   * @request POST:/api/tenantGameAppPackage/add
   */
  tenantGameAppPackageAddPost = (
    data: AddGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageAddPostData, any>({
      path: `/api/tenantGameAppPackage/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageDeletePost
   * @summary 删除分包
   * @request POST:/api/tenantGameAppPackage/delete
   */
  tenantGameAppPackageDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageDeletePostData, any>({
      path: `/api/tenantGameAppPackage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppPackage/detail
   */
  tenantGameAppPackageDetailGet = (
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
    this.http.request<TenantGameAppPackageDetailGetData, any>({
      path: `/api/tenantGameAppPackage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageListGet
   * @summary 获取应用下启用分包列表
   * @request GET:/api/tenantGameAppPackage/list
   */
  tenantGameAppPackageListGet = (
    query?: {
      /** @format int64 */
      appId?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageListGetData, any>({
      path: `/api/tenantGameAppPackage/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackagePagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppPackage/page
   */
  tenantGameAppPackagePagePost = (
    data: PageGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackagePagePostData, any>({
      path: `/api/tenantGameAppPackage/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageSelectPost
   * @summary 获取分包选择器列表。
   * @request POST:/api/tenantGameAppPackage/select
   */
  tenantGameAppPackageSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageSelectPostData, any>({
      path: `/api/tenantGameAppPackage/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAppPackage/selectString
   */
  tenantGameAppPackageSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageSelectStringPostData, any>({
      path: `/api/tenantGameAppPackage/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageSetDefaultPost
   * @summary 设置默认分包
   * @request POST:/api/tenantGameAppPackage/setDefault
   */
  tenantGameAppPackageSetDefaultPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageSetDefaultPostData, any>({
      path: `/api/tenantGameAppPackage/setDefault`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageSetStatusPost
   * @summary 启用或停用分包
   * @request POST:/api/tenantGameAppPackage/setStatus
   */
  tenantGameAppPackageSetStatusPost = (
    data: SetGameAppPackageStatusRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageSetStatusPostData, any>({
      path: `/api/tenantGameAppPackage/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppPackage
   * @name TenantGameAppPackageUpdatePost
   * @summary 更新分包
   * @request POST:/api/tenantGameAppPackage/update
   */
  tenantGameAppPackageUpdatePost = (
    data: UpdateGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppPackageUpdatePostData, any>({
      path: `/api/tenantGameAppPackage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppPackageAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageAddPost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageAddPost(...args);
export const tenantGameAppPackageDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageDeletePost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageDeletePost(...args);
export const tenantGameAppPackageDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageDetailGet']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageDetailGet(...args);
export const tenantGameAppPackageListGet = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageListGet']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageListGet(...args);
export const tenantGameAppPackagePagePost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackagePagePost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackagePagePost(...args);
export const tenantGameAppPackageSelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageSelectPost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageSelectPost(...args);
export const tenantGameAppPackageSelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageSelectStringPost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageSelectStringPost(...args);
export const tenantGameAppPackageSetDefaultPost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageSetDefaultPost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageSetDefaultPost(...args);
export const tenantGameAppPackageSetStatusPost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageSetStatusPost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageSetStatusPost(...args);
export const tenantGameAppPackageUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppPackage>['tenantGameAppPackageUpdatePost']>) => getAPI(TenantGameAppPackage).tenantGameAppPackageUpdatePost(...args);
