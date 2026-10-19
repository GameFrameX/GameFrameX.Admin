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
  AddTenantGameMailTemplateCategoryEntityRequest,
  PageTenantGameMailTemplateCategoryEntityRequest,
  TenantGameMailTemplateCategoryEntityAddPostData,
  TenantGameMailTemplateCategoryEntityDeletePostData,
  TenantGameMailTemplateCategoryEntityDetailGetData,
  TenantGameMailTemplateCategoryEntityPagePostData,
  TenantGameMailTemplateCategoryEntitySelectPostData,
  TenantGameMailTemplateCategoryEntitySelectStringPostData,
  TenantGameMailTemplateCategoryEntityUpdatePostData,
  UpdateTenantGameMailTemplateCategoryEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameMailTemplateCategoryEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/add
   */
  tenantGameMailTemplateCategoryEntityAddPost = (
    data: AddTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateCategoryEntityAddPostData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/delete
   */
  tenantGameMailTemplateCategoryEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateCategoryEntityDeletePostData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameMailTemplateCategoryEntity/detail
   */
  tenantGameMailTemplateCategoryEntityDetailGet = (
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
    this.http.request<TenantGameMailTemplateCategoryEntityDetailGetData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/page
   */
  tenantGameMailTemplateCategoryEntityPagePost = (
    data: PageTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateCategoryEntityPagePostData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/select
   */
  tenantGameMailTemplateCategoryEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateCategoryEntitySelectPostData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/selectString
   */
  tenantGameMailTemplateCategoryEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameMailTemplateCategoryEntitySelectStringPostData,
      any
    >({
      path: `/api/tenantGameMailTemplateCategoryEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateCategoryEntity
   * @name TenantGameMailTemplateCategoryEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameMailTemplateCategoryEntity/update
   */
  tenantGameMailTemplateCategoryEntityUpdatePost = (
    data: UpdateTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateCategoryEntityUpdatePostData, any>({
      path: `/api/tenantGameMailTemplateCategoryEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameMailTemplateCategoryEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntityAddPost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntityAddPost(...args);
export const tenantGameMailTemplateCategoryEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntityDeletePost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntityDeletePost(...args);
export const tenantGameMailTemplateCategoryEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntityDetailGet']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntityDetailGet(...args);
export const tenantGameMailTemplateCategoryEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntityPagePost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntityPagePost(...args);
export const tenantGameMailTemplateCategoryEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntitySelectPost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntitySelectPost(...args);
export const tenantGameMailTemplateCategoryEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntitySelectStringPost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntitySelectStringPost(...args);
export const tenantGameMailTemplateCategoryEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateCategoryEntity>['tenantGameMailTemplateCategoryEntityUpdatePost']>) => getAPI(TenantGameMailTemplateCategoryEntity).tenantGameMailTemplateCategoryEntityUpdatePost(...args);
