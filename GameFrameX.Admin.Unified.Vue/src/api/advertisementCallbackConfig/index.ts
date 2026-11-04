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
  AddTenantAdvertisementCallbackConfigRequest,
  AdvertisementCallbackConfigAddPostData,
  AdvertisementCallbackConfigDeletePostData,
  AdvertisementCallbackConfigDetailGetData,
  AdvertisementCallbackConfigPagePostData,
  AdvertisementCallbackConfigProvidersGetData,
  AdvertisementCallbackConfigSchemaGetData,
  AdvertisementCallbackConfigUpdatePostData,
  PageTenantAdvertisementCallbackConfigRequest,
  UpdateTenantAdvertisementCallbackConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class AdvertisementCallbackConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigAddPost
   * @summary 新增广告回调配置（主库内 Provider 唯一）。
   * @request POST:/api/advertisementCallbackConfig/add
   */
  advertisementCallbackConfigAddPost = (
    data: AddTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AdvertisementCallbackConfigAddPostData, any>({
      path: `/api/advertisementCallbackConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigDeletePost
   * @summary 删除
   * @request POST:/api/advertisementCallbackConfig/delete
   */
  advertisementCallbackConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AdvertisementCallbackConfigDeletePostData, any>({
      path: `/api/advertisementCallbackConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/advertisementCallbackConfig/detail
   */
  advertisementCallbackConfigDetailGet = (
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
    this.http.request<AdvertisementCallbackConfigDetailGetData, any>({
      path: `/api/advertisementCallbackConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/advertisementCallbackConfig/page
   */
  advertisementCallbackConfigPagePost = (
    data: PageTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AdvertisementCallbackConfigPagePostData, any>({
      path: `/api/advertisementCallbackConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigProvidersGet
   * @summary 获取首批支持的广告 Provider 清单。
   * @request GET:/api/advertisementCallbackConfig/providers
   */
  advertisementCallbackConfigProvidersGet = (params: RequestParams = {}) =>
    this.http.request<AdvertisementCallbackConfigProvidersGetData, any>({
      path: `/api/advertisementCallbackConfig/providers`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigSchemaGet
   * @summary 获取指定广告 Provider 的动态配置字段 schema。
   * @request GET:/api/advertisementCallbackConfig/schema
   */
  advertisementCallbackConfigSchemaGet = (
    query?: {
      provider?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<AdvertisementCallbackConfigSchemaGetData, any>({
      path: `/api/advertisementCallbackConfig/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags advertisementCallbackConfig
   * @name AdvertisementCallbackConfigUpdatePost
   * @summary 更新广告回调配置。敏感字段回传 `******` 时保留原值。
   * @request POST:/api/advertisementCallbackConfig/update
   */
  advertisementCallbackConfigUpdatePost = (
    data: UpdateTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<AdvertisementCallbackConfigUpdatePostData, any>({
      path: `/api/advertisementCallbackConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const advertisementCallbackConfigAddPost = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigAddPost']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigAddPost(...args);
export const advertisementCallbackConfigDeletePost = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigDeletePost']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigDeletePost(...args);
export const advertisementCallbackConfigDetailGet = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigDetailGet']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigDetailGet(...args);
export const advertisementCallbackConfigPagePost = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigPagePost']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigPagePost(...args);
export const advertisementCallbackConfigProvidersGet = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigProvidersGet']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigProvidersGet(...args);
export const advertisementCallbackConfigSchemaGet = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigSchemaGet']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigSchemaGet(...args);
export const advertisementCallbackConfigUpdatePost = (...args: Parameters<InstanceType<typeof AdvertisementCallbackConfig>['advertisementCallbackConfigUpdatePost']>) => getAPI(AdvertisementCallbackConfig).advertisementCallbackConfigUpdatePost(...args);
