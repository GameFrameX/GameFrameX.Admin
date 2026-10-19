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
  AddTenantGameMailTemplateEntityRequest,
  PageTenantGameMailTemplateEntityRequest,
  TenantGameMailTemplateEntityAddPostData,
  TenantGameMailTemplateEntityDeletePostData,
  TenantGameMailTemplateEntityDetailGetData,
  TenantGameMailTemplateEntityPagePostData,
  TenantGameMailTemplateEntitySelectPostData,
  TenantGameMailTemplateEntitySelectStringPostData,
  TenantGameMailTemplateEntityUpdatePostData,
  UpdateTenantGameMailTemplateEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameMailTemplateEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameMailTemplateEntity/add
   */
  tenantGameMailTemplateEntityAddPost = (
    data: AddTenantGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntityAddPostData, any>({
      path: `/api/tenantGameMailTemplateEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameMailTemplateEntity/delete
   */
  tenantGameMailTemplateEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntityDeletePostData, any>({
      path: `/api/tenantGameMailTemplateEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameMailTemplateEntity/detail
   */
  tenantGameMailTemplateEntityDetailGet = (
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
    this.http.request<TenantGameMailTemplateEntityDetailGetData, any>({
      path: `/api/tenantGameMailTemplateEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameMailTemplateEntity/page
   */
  tenantGameMailTemplateEntityPagePost = (
    data: PageTenantGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntityPagePostData, any>({
      path: `/api/tenantGameMailTemplateEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameMailTemplateEntity/select
   */
  tenantGameMailTemplateEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntitySelectPostData, any>({
      path: `/api/tenantGameMailTemplateEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameMailTemplateEntity/selectString
   */
  tenantGameMailTemplateEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntitySelectStringPostData, any>({
      path: `/api/tenantGameMailTemplateEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameMailTemplateEntity
   * @name TenantGameMailTemplateEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameMailTemplateEntity/update
   */
  tenantGameMailTemplateEntityUpdatePost = (
    data: UpdateTenantGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameMailTemplateEntityUpdatePostData, any>({
      path: `/api/tenantGameMailTemplateEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameMailTemplateEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntityAddPost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntityAddPost(...args);
export const tenantGameMailTemplateEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntityDeletePost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntityDeletePost(...args);
export const tenantGameMailTemplateEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntityDetailGet']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntityDetailGet(...args);
export const tenantGameMailTemplateEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntityPagePost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntityPagePost(...args);
export const tenantGameMailTemplateEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntitySelectPost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntitySelectPost(...args);
export const tenantGameMailTemplateEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntitySelectStringPost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntitySelectStringPost(...args);
export const tenantGameMailTemplateEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameMailTemplateEntity>['tenantGameMailTemplateEntityUpdatePost']>) => getAPI(TenantGameMailTemplateEntity).tenantGameMailTemplateEntityUpdatePost(...args);
