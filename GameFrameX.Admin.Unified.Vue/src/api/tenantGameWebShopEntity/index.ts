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
  AddTenantGameWebShopEntityRequest,
  PageTenantGameWebShopEntityRequest,
  TenantGameWebShopEntityAddPostData,
  TenantGameWebShopEntityDeletePostData,
  TenantGameWebShopEntityDetailGetData,
  TenantGameWebShopEntityPagePostData,
  TenantGameWebShopEntitySelectPostData,
  TenantGameWebShopEntitySelectStringPostData,
  TenantGameWebShopEntityUpdatePostData,
  UpdateTenantGameWebShopEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameWebShopEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameWebShopEntity/add
   */
  tenantGameWebShopEntityAddPost = (
    data: AddTenantGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntityAddPostData, any>({
      path: `/api/tenantGameWebShopEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameWebShopEntity/delete
   */
  tenantGameWebShopEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntityDeletePostData, any>({
      path: `/api/tenantGameWebShopEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameWebShopEntity/detail
   */
  tenantGameWebShopEntityDetailGet = (
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
    this.http.request<TenantGameWebShopEntityDetailGetData, any>({
      path: `/api/tenantGameWebShopEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameWebShopEntity/page
   */
  tenantGameWebShopEntityPagePost = (
    data: PageTenantGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntityPagePostData, any>({
      path: `/api/tenantGameWebShopEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameWebShopEntity/select
   */
  tenantGameWebShopEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntitySelectPostData, any>({
      path: `/api/tenantGameWebShopEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameWebShopEntity/selectString
   */
  tenantGameWebShopEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntitySelectStringPostData, any>({
      path: `/api/tenantGameWebShopEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopEntity
   * @name TenantGameWebShopEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameWebShopEntity/update
   */
  tenantGameWebShopEntityUpdatePost = (
    data: UpdateTenantGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopEntityUpdatePostData, any>({
      path: `/api/tenantGameWebShopEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameWebShopEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntityAddPost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntityAddPost(...args);
export const tenantGameWebShopEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntityDeletePost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntityDeletePost(...args);
export const tenantGameWebShopEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntityDetailGet']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntityDetailGet(...args);
export const tenantGameWebShopEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntityPagePost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntityPagePost(...args);
export const tenantGameWebShopEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntitySelectPost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntitySelectPost(...args);
export const tenantGameWebShopEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntitySelectStringPost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntitySelectStringPost(...args);
export const tenantGameWebShopEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopEntity>['tenantGameWebShopEntityUpdatePost']>) => getAPI(TenantGameWebShopEntity).tenantGameWebShopEntityUpdatePost(...args);
