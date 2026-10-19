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
  AddGameAppVersionDefinitionRequest,
  PageGameAppVersionDefinitionRequest,
  TenantGameAppVersionDefinitionAddPostData,
  TenantGameAppVersionDefinitionDeletePostData,
  TenantGameAppVersionDefinitionDetailGetData,
  TenantGameAppVersionDefinitionPagePostData,
  TenantGameAppVersionDefinitionSelectPostData,
  TenantGameAppVersionDefinitionSelectStringPostData,
  TenantGameAppVersionDefinitionUpdatePostData,
  UpdateGameAppVersionDefinitionRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppVersionDefinition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppVersionDefinition/add
   */
  tenantGameAppVersionDefinitionAddPost = (
    data: AddGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionAddPostData, any>({
      path: `/api/tenantGameAppVersionDefinition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppVersionDefinition/delete
   */
  tenantGameAppVersionDefinitionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionDeletePostData, any>({
      path: `/api/tenantGameAppVersionDefinition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppVersionDefinition/detail
   */
  tenantGameAppVersionDefinitionDetailGet = (
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
    this.http.request<TenantGameAppVersionDefinitionDetailGetData, any>({
      path: `/api/tenantGameAppVersionDefinition/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppVersionDefinition/page
   */
  tenantGameAppVersionDefinitionPagePost = (
    data: PageGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionPagePostData, any>({
      path: `/api/tenantGameAppVersionDefinition/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionSelectPost
   * @summary 获取当前应用下的版本定义选择器列表。
   * @request POST:/api/tenantGameAppVersionDefinition/select
   */
  tenantGameAppVersionDefinitionSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionSelectPostData, any>({
      path: `/api/tenantGameAppVersionDefinition/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAppVersionDefinition/selectString
   */
  tenantGameAppVersionDefinitionSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionSelectStringPostData, any>({
      path: `/api/tenantGameAppVersionDefinition/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppVersionDefinition
   * @name TenantGameAppVersionDefinitionUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppVersionDefinition/update
   */
  tenantGameAppVersionDefinitionUpdatePost = (
    data: UpdateGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppVersionDefinitionUpdatePostData, any>({
      path: `/api/tenantGameAppVersionDefinition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppVersionDefinitionAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionAddPost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionAddPost(...args);
export const tenantGameAppVersionDefinitionDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionDeletePost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionDeletePost(...args);
export const tenantGameAppVersionDefinitionDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionDetailGet']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionDetailGet(...args);
export const tenantGameAppVersionDefinitionPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionPagePost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionPagePost(...args);
export const tenantGameAppVersionDefinitionSelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionSelectPost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionSelectPost(...args);
export const tenantGameAppVersionDefinitionSelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionSelectStringPost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionSelectStringPost(...args);
export const tenantGameAppVersionDefinitionUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppVersionDefinition>['tenantGameAppVersionDefinitionUpdatePost']>) => getAPI(TenantGameAppVersionDefinition).tenantGameAppVersionDefinitionUpdatePost(...args);
