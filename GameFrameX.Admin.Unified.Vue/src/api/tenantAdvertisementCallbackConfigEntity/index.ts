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
  PageTenantAdvertisementCallbackConfigRequest,
  TenantAdvertisementCallbackConfigEntityAddPostData,
  TenantAdvertisementCallbackConfigEntityDeletePostData,
  TenantAdvertisementCallbackConfigEntityDetailGetData,
  TenantAdvertisementCallbackConfigEntityPagePostData,
  TenantAdvertisementCallbackConfigEntityProvidersGetData,
  TenantAdvertisementCallbackConfigEntitySchemaGetData,
  TenantAdvertisementCallbackConfigEntityUpdatePostData,
  UpdateTenantAdvertisementCallbackConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantAdvertisementCallbackConfigEntity<
  SecurityDataType = unknown,
> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityAddPost
   * @summary 新增广告回调配置（同一租户下 Provider + AppId 作用域唯一）。
   * @request POST:/api/tenantAdvertisementCallbackConfigEntity/add
   */
  tenantAdvertisementCallbackConfigEntityAddPost = (
    data: AddTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAdvertisementCallbackConfigEntityAddPostData, any>({
      path: `/api/tenantAdvertisementCallbackConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantAdvertisementCallbackConfigEntity/delete
   */
  tenantAdvertisementCallbackConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantAdvertisementCallbackConfigEntityDeletePostData,
      any
    >({
      path: `/api/tenantAdvertisementCallbackConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/tenantAdvertisementCallbackConfigEntity/detail
   */
  tenantAdvertisementCallbackConfigEntityDetailGet = (
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
      TenantAdvertisementCallbackConfigEntityDetailGetData,
      any
    >({
      path: `/api/tenantAdvertisementCallbackConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/tenantAdvertisementCallbackConfigEntity/page
   */
  tenantAdvertisementCallbackConfigEntityPagePost = (
    data: PageTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantAdvertisementCallbackConfigEntityPagePostData, any>(
      {
        path: `/api/tenantAdvertisementCallbackConfigEntity/page`,
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
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityProvidersGet
   * @summary 获取首批支持的广告 Provider 清单。
   * @request GET:/api/tenantAdvertisementCallbackConfigEntity/providers
   */
  tenantAdvertisementCallbackConfigEntityProvidersGet = (
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantAdvertisementCallbackConfigEntityProvidersGetData,
      any
    >({
      path: `/api/tenantAdvertisementCallbackConfigEntity/providers`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntitySchemaGet
   * @summary 获取指定广告 Provider 的动态配置字段 schema。
   * @request GET:/api/tenantAdvertisementCallbackConfigEntity/schema
   */
  tenantAdvertisementCallbackConfigEntitySchemaGet = (
    query?: {
      provider?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantAdvertisementCallbackConfigEntitySchemaGetData,
      any
    >({
      path: `/api/tenantAdvertisementCallbackConfigEntity/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantAdvertisementCallbackConfigEntity
   * @name TenantAdvertisementCallbackConfigEntityUpdatePost
   * @summary 更新广告回调配置。敏感字段回传 `******` 时保留原值。
   * @request POST:/api/tenantAdvertisementCallbackConfigEntity/update
   */
  tenantAdvertisementCallbackConfigEntityUpdatePost = (
    data: UpdateTenantAdvertisementCallbackConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantAdvertisementCallbackConfigEntityUpdatePostData,
      any
    >({
      path: `/api/tenantAdvertisementCallbackConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantAdvertisementCallbackConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityAddPost']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityAddPost(...args);
export const tenantAdvertisementCallbackConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityDeletePost']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityDeletePost(...args);
export const tenantAdvertisementCallbackConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityDetailGet']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityDetailGet(...args);
export const tenantAdvertisementCallbackConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityPagePost']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityPagePost(...args);
export const tenantAdvertisementCallbackConfigEntityProvidersGet = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityProvidersGet']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityProvidersGet(...args);
export const tenantAdvertisementCallbackConfigEntitySchemaGet = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntitySchemaGet']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntitySchemaGet(...args);
export const tenantAdvertisementCallbackConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantAdvertisementCallbackConfigEntity>['tenantAdvertisementCallbackConfigEntityUpdatePost']>) => getAPI(TenantAdvertisementCallbackConfigEntity).tenantAdvertisementCallbackConfigEntityUpdatePost(...args);
