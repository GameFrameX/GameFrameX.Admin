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
  AddGameAppFeatureSwitchRequest,
  PageGameAppFeatureSwitchRequest,
  TenantGameAppFeatureSwitchAddPostData,
  TenantGameAppFeatureSwitchDeletePostData,
  TenantGameAppFeatureSwitchDetailGetData,
  TenantGameAppFeatureSwitchPagePostData,
  TenantGameAppFeatureSwitchUpdatePostData,
  UpdateGameAppFeatureSwitchRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppFeatureSwitch<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppFeatureSwitch
   * @name TenantGameAppFeatureSwitchAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppFeatureSwitch/add
   */
  tenantGameAppFeatureSwitchAddPost = (
    data: AddGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppFeatureSwitchAddPostData, any>({
      path: `/api/tenantGameAppFeatureSwitch/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppFeatureSwitch
   * @name TenantGameAppFeatureSwitchDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppFeatureSwitch/delete
   */
  tenantGameAppFeatureSwitchDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppFeatureSwitchDeletePostData, any>({
      path: `/api/tenantGameAppFeatureSwitch/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppFeatureSwitch
   * @name TenantGameAppFeatureSwitchDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppFeatureSwitch/detail
   */
  tenantGameAppFeatureSwitchDetailGet = (
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
    this.http.request<TenantGameAppFeatureSwitchDetailGetData, any>({
      path: `/api/tenantGameAppFeatureSwitch/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppFeatureSwitch
   * @name TenantGameAppFeatureSwitchPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppFeatureSwitch/page
   */
  tenantGameAppFeatureSwitchPagePost = (
    data: PageGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppFeatureSwitchPagePostData, any>({
      path: `/api/tenantGameAppFeatureSwitch/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppFeatureSwitch
   * @name TenantGameAppFeatureSwitchUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppFeatureSwitch/update
   */
  tenantGameAppFeatureSwitchUpdatePost = (
    data: UpdateGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppFeatureSwitchUpdatePostData, any>({
      path: `/api/tenantGameAppFeatureSwitch/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppFeatureSwitchAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppFeatureSwitch>['tenantGameAppFeatureSwitchAddPost']>) => getAPI(TenantGameAppFeatureSwitch).tenantGameAppFeatureSwitchAddPost(...args);
export const tenantGameAppFeatureSwitchDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppFeatureSwitch>['tenantGameAppFeatureSwitchDeletePost']>) => getAPI(TenantGameAppFeatureSwitch).tenantGameAppFeatureSwitchDeletePost(...args);
export const tenantGameAppFeatureSwitchDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppFeatureSwitch>['tenantGameAppFeatureSwitchDetailGet']>) => getAPI(TenantGameAppFeatureSwitch).tenantGameAppFeatureSwitchDetailGet(...args);
export const tenantGameAppFeatureSwitchPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppFeatureSwitch>['tenantGameAppFeatureSwitchPagePost']>) => getAPI(TenantGameAppFeatureSwitch).tenantGameAppFeatureSwitchPagePost(...args);
export const tenantGameAppFeatureSwitchUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppFeatureSwitch>['tenantGameAppFeatureSwitchUpdatePost']>) => getAPI(TenantGameAppFeatureSwitch).tenantGameAppFeatureSwitchUpdatePost(...args);
