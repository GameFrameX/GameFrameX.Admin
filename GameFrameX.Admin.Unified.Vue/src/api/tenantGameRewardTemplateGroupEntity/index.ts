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
  AddTenantGameRewardTemplateGroupEntityRequest,
  PageTenantGameRewardTemplateGroupEntityRequest,
  TenantGameRewardTemplateGroupEntityAddPostData,
  TenantGameRewardTemplateGroupEntityDeletePostData,
  TenantGameRewardTemplateGroupEntityDetailGetData,
  TenantGameRewardTemplateGroupEntityPagePostData,
  TenantGameRewardTemplateGroupEntitySelectPostData,
  TenantGameRewardTemplateGroupEntitySelectStringPostData,
  TenantGameRewardTemplateGroupEntityUpdatePostData,
  UpdateTenantGameRewardTemplateGroupEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRewardTemplateGroupEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/add
   */
  tenantGameRewardTemplateGroupEntityAddPost = (
    data: AddTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateGroupEntityAddPostData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/delete
   */
  tenantGameRewardTemplateGroupEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateGroupEntityDeletePostData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameRewardTemplateGroupEntity/detail
   */
  tenantGameRewardTemplateGroupEntityDetailGet = (
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
    this.http.request<TenantGameRewardTemplateGroupEntityDetailGetData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/page
   */
  tenantGameRewardTemplateGroupEntityPagePost = (
    data: PageTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateGroupEntityPagePostData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntitySelectPost
   * @summary 获取当前应用下启用的奖励模板分组。
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/select
   */
  tenantGameRewardTemplateGroupEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateGroupEntitySelectPostData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/selectString
   */
  tenantGameRewardTemplateGroupEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameRewardTemplateGroupEntitySelectStringPostData,
      any
    >({
      path: `/api/tenantGameRewardTemplateGroupEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRewardTemplateGroupEntity
   * @name TenantGameRewardTemplateGroupEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameRewardTemplateGroupEntity/update
   */
  tenantGameRewardTemplateGroupEntityUpdatePost = (
    data: UpdateTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRewardTemplateGroupEntityUpdatePostData, any>({
      path: `/api/tenantGameRewardTemplateGroupEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRewardTemplateGroupEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntityAddPost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntityAddPost(...args);
export const tenantGameRewardTemplateGroupEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntityDeletePost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntityDeletePost(...args);
export const tenantGameRewardTemplateGroupEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntityDetailGet']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntityDetailGet(...args);
export const tenantGameRewardTemplateGroupEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntityPagePost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntityPagePost(...args);
export const tenantGameRewardTemplateGroupEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntitySelectPost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntitySelectPost(...args);
export const tenantGameRewardTemplateGroupEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntitySelectStringPost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntitySelectStringPost(...args);
export const tenantGameRewardTemplateGroupEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRewardTemplateGroupEntity>['tenantGameRewardTemplateGroupEntityUpdatePost']>) => getAPI(TenantGameRewardTemplateGroupEntity).tenantGameRewardTemplateGroupEntityUpdatePost(...args);
