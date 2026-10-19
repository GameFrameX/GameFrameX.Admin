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
  AddTenantGameGoodsCatalogueEntityRequest,
  PageTenantGameGoodsCatalogueEntityRequest,
  TenantGameGoodsCatalogueEntityAddPostData,
  TenantGameGoodsCatalogueEntityDeletePostData,
  TenantGameGoodsCatalogueEntityDetailGetData,
  TenantGameGoodsCatalogueEntityPagePostData,
  TenantGameGoodsCatalogueEntitySelectPostData,
  TenantGameGoodsCatalogueEntitySelectStringPostData,
  TenantGameGoodsCatalogueEntityUpdatePostData,
  UpdateTenantGameGoodsCatalogueEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameGoodsCatalogueEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameGoodsCatalogueEntity/add
   */
  tenantGameGoodsCatalogueEntityAddPost = (
    data: AddTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntityAddPostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameGoodsCatalogueEntity/delete
   */
  tenantGameGoodsCatalogueEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntityDeletePostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameGoodsCatalogueEntity/detail
   */
  tenantGameGoodsCatalogueEntityDetailGet = (
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
    this.http.request<TenantGameGoodsCatalogueEntityDetailGetData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameGoodsCatalogueEntity/page
   */
  tenantGameGoodsCatalogueEntityPagePost = (
    data: PageTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntityPagePostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameGoodsCatalogueEntity/select
   */
  tenantGameGoodsCatalogueEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntitySelectPostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameGoodsCatalogueEntity/selectString
   */
  tenantGameGoodsCatalogueEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntitySelectStringPostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsCatalogueEntity
   * @name TenantGameGoodsCatalogueEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameGoodsCatalogueEntity/update
   */
  tenantGameGoodsCatalogueEntityUpdatePost = (
    data: UpdateTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsCatalogueEntityUpdatePostData, any>({
      path: `/api/tenantGameGoodsCatalogueEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameGoodsCatalogueEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntityAddPost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntityAddPost(...args);
export const tenantGameGoodsCatalogueEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntityDeletePost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntityDeletePost(...args);
export const tenantGameGoodsCatalogueEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntityDetailGet']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntityDetailGet(...args);
export const tenantGameGoodsCatalogueEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntityPagePost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntityPagePost(...args);
export const tenantGameGoodsCatalogueEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntitySelectPost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntitySelectPost(...args);
export const tenantGameGoodsCatalogueEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntitySelectStringPost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntitySelectStringPost(...args);
export const tenantGameGoodsCatalogueEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsCatalogueEntity>['tenantGameGoodsCatalogueEntityUpdatePost']>) => getAPI(TenantGameGoodsCatalogueEntity).tenantGameGoodsCatalogueEntityUpdatePost(...args);
