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
  AddTenantGameDockerTagEntityRequest,
  PageTenantGameDockerTagEntityRequest,
  TenantGameDockerTagEntityAddPostData,
  TenantGameDockerTagEntityDeletePostData,
  TenantGameDockerTagEntityDetailGetData,
  TenantGameDockerTagEntityPagePostData,
  TenantGameDockerTagEntitySelectPostData,
  TenantGameDockerTagEntitySelectStringPostData,
  TenantGameDockerTagEntityUpdatePostData,
  UpdateTenantGameDockerTagEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameDockerTagEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameDockerTagEntity/add
   */
  tenantGameDockerTagEntityAddPost = (
    data: AddTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntityAddPostData, any>({
      path: `/api/tenantGameDockerTagEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameDockerTagEntity/delete
   */
  tenantGameDockerTagEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntityDeletePostData, any>({
      path: `/api/tenantGameDockerTagEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameDockerTagEntity/detail
   */
  tenantGameDockerTagEntityDetailGet = (
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
    this.http.request<TenantGameDockerTagEntityDetailGetData, any>({
      path: `/api/tenantGameDockerTagEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameDockerTagEntity/page
   */
  tenantGameDockerTagEntityPagePost = (
    data: PageTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntityPagePostData, any>({
      path: `/api/tenantGameDockerTagEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameDockerTagEntity/select
   */
  tenantGameDockerTagEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntitySelectPostData, any>({
      path: `/api/tenantGameDockerTagEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameDockerTagEntity/selectString
   */
  tenantGameDockerTagEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntitySelectStringPostData, any>({
      path: `/api/tenantGameDockerTagEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameDockerTagEntity
   * @name TenantGameDockerTagEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameDockerTagEntity/update
   */
  tenantGameDockerTagEntityUpdatePost = (
    data: UpdateTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameDockerTagEntityUpdatePostData, any>({
      path: `/api/tenantGameDockerTagEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameDockerTagEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntityAddPost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntityAddPost(...args);
export const tenantGameDockerTagEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntityDeletePost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntityDeletePost(...args);
export const tenantGameDockerTagEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntityDetailGet']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntityDetailGet(...args);
export const tenantGameDockerTagEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntityPagePost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntityPagePost(...args);
export const tenantGameDockerTagEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntitySelectPost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntitySelectPost(...args);
export const tenantGameDockerTagEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntitySelectStringPost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntitySelectStringPost(...args);
export const tenantGameDockerTagEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameDockerTagEntity>['tenantGameDockerTagEntityUpdatePost']>) => getAPI(TenantGameDockerTagEntity).tenantGameDockerTagEntityUpdatePost(...args);
