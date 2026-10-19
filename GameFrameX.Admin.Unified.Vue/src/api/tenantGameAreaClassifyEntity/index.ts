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
  AddTenantGameAreaClassifyEntityRequest,
  PageTenantGameAreaClassifyEntityRequest,
  TenantGameAreaClassifyEntityAddPostData,
  TenantGameAreaClassifyEntityDeletePostData,
  TenantGameAreaClassifyEntityDetailGetData,
  TenantGameAreaClassifyEntityPagePostData,
  TenantGameAreaClassifyEntitySelectPostData,
  TenantGameAreaClassifyEntitySelectStringPostData,
  TenantGameAreaClassifyEntityUpdatePostData,
  UpdateTenantGameAreaClassifyEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAreaClassifyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAreaClassifyEntity/add
   */
  tenantGameAreaClassifyEntityAddPost = (
    data: AddTenantGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntityAddPostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAreaClassifyEntity/delete
   */
  tenantGameAreaClassifyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntityDeletePostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAreaClassifyEntity/detail
   */
  tenantGameAreaClassifyEntityDetailGet = (
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
    this.http.request<TenantGameAreaClassifyEntityDetailGetData, any>({
      path: `/api/tenantGameAreaClassifyEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAreaClassifyEntity/page
   */
  tenantGameAreaClassifyEntityPagePost = (
    data: PageTenantGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntityPagePostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameAreaClassifyEntity/select
   */
  tenantGameAreaClassifyEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntitySelectPostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAreaClassifyEntity/selectString
   */
  tenantGameAreaClassifyEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntitySelectStringPostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaClassifyEntity
   * @name TenantGameAreaClassifyEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAreaClassifyEntity/update
   */
  tenantGameAreaClassifyEntityUpdatePost = (
    data: UpdateTenantGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaClassifyEntityUpdatePostData, any>({
      path: `/api/tenantGameAreaClassifyEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAreaClassifyEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntityAddPost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntityAddPost(...args);
export const tenantGameAreaClassifyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntityDeletePost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntityDeletePost(...args);
export const tenantGameAreaClassifyEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntityDetailGet']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntityDetailGet(...args);
export const tenantGameAreaClassifyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntityPagePost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntityPagePost(...args);
export const tenantGameAreaClassifyEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntitySelectPost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntitySelectPost(...args);
export const tenantGameAreaClassifyEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntitySelectStringPost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntitySelectStringPost(...args);
export const tenantGameAreaClassifyEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAreaClassifyEntity>['tenantGameAreaClassifyEntityUpdatePost']>) => getAPI(TenantGameAreaClassifyEntity).tenantGameAreaClassifyEntityUpdatePost(...args);
