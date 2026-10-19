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
  AddTenantGameTimeJobEntityRequest,
  PageTenantGameTimeJobEntityRequest,
  TenantGameTimeJobEntityAddPostData,
  TenantGameTimeJobEntityDeletePostData,
  TenantGameTimeJobEntityDetailGetData,
  TenantGameTimeJobEntityPagePostData,
  TenantGameTimeJobEntitySelectPostData,
  TenantGameTimeJobEntitySelectStringPostData,
  TenantGameTimeJobEntityUpdatePostData,
  UpdateTenantGameTimeJobEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameTimeJobEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameTimeJobEntity/add
   */
  tenantGameTimeJobEntityAddPost = (
    data: AddTenantGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntityAddPostData, any>({
      path: `/api/tenantGameTimeJobEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameTimeJobEntity/delete
   */
  tenantGameTimeJobEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntityDeletePostData, any>({
      path: `/api/tenantGameTimeJobEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameTimeJobEntity/detail
   */
  tenantGameTimeJobEntityDetailGet = (
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
    this.http.request<TenantGameTimeJobEntityDetailGetData, any>({
      path: `/api/tenantGameTimeJobEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameTimeJobEntity/page
   */
  tenantGameTimeJobEntityPagePost = (
    data: PageTenantGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntityPagePostData, any>({
      path: `/api/tenantGameTimeJobEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameTimeJobEntity/select
   */
  tenantGameTimeJobEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntitySelectPostData, any>({
      path: `/api/tenantGameTimeJobEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameTimeJobEntity/selectString
   */
  tenantGameTimeJobEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntitySelectStringPostData, any>({
      path: `/api/tenantGameTimeJobEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameTimeJobEntity
   * @name TenantGameTimeJobEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameTimeJobEntity/update
   */
  tenantGameTimeJobEntityUpdatePost = (
    data: UpdateTenantGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameTimeJobEntityUpdatePostData, any>({
      path: `/api/tenantGameTimeJobEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameTimeJobEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntityAddPost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntityAddPost(...args);
export const tenantGameTimeJobEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntityDeletePost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntityDeletePost(...args);
export const tenantGameTimeJobEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntityDetailGet']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntityDetailGet(...args);
export const tenantGameTimeJobEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntityPagePost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntityPagePost(...args);
export const tenantGameTimeJobEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntitySelectPost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntitySelectPost(...args);
export const tenantGameTimeJobEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntitySelectStringPost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntitySelectStringPost(...args);
export const tenantGameTimeJobEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameTimeJobEntity>['tenantGameTimeJobEntityUpdatePost']>) => getAPI(TenantGameTimeJobEntity).tenantGameTimeJobEntityUpdatePost(...args);
