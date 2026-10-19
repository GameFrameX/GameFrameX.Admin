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
  AddTenantGameGoodsEntityRequest,
  PageTenantGameGoodsEntityRequest,
  TenantGameGoodsEntityAddPostData,
  TenantGameGoodsEntityDeletePostData,
  TenantGameGoodsEntityDetailGetData,
  TenantGameGoodsEntityPagePostData,
  TenantGameGoodsEntitySelectPostData,
  TenantGameGoodsEntitySelectStringPostData,
  TenantGameGoodsEntityUpdatePostData,
  UpdateTenantGameGoodsEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameGoodsEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameGoodsEntity/add
   */
  tenantGameGoodsEntityAddPost = (
    data: AddTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntityAddPostData, any>({
      path: `/api/tenantGameGoodsEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameGoodsEntity/delete
   */
  tenantGameGoodsEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntityDeletePostData, any>({
      path: `/api/tenantGameGoodsEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameGoodsEntity/detail
   */
  tenantGameGoodsEntityDetailGet = (
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
    this.http.request<TenantGameGoodsEntityDetailGetData, any>({
      path: `/api/tenantGameGoodsEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameGoodsEntity/page
   */
  tenantGameGoodsEntityPagePost = (
    data: PageTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntityPagePostData, any>({
      path: `/api/tenantGameGoodsEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameGoodsEntity/select
   */
  tenantGameGoodsEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntitySelectPostData, any>({
      path: `/api/tenantGameGoodsEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntitySelectStringPost
   * @summary 获取物品选择列表（含分类信息）
   * @request POST:/api/tenantGameGoodsEntity/selectString
   */
  tenantGameGoodsEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntitySelectStringPostData, any>({
      path: `/api/tenantGameGoodsEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameGoodsEntity
   * @name TenantGameGoodsEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameGoodsEntity/update
   */
  tenantGameGoodsEntityUpdatePost = (
    data: UpdateTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameGoodsEntityUpdatePostData, any>({
      path: `/api/tenantGameGoodsEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameGoodsEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntityAddPost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntityAddPost(...args);
export const tenantGameGoodsEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntityDeletePost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntityDeletePost(...args);
export const tenantGameGoodsEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntityDetailGet']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntityDetailGet(...args);
export const tenantGameGoodsEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntityPagePost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntityPagePost(...args);
export const tenantGameGoodsEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntitySelectPost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntitySelectPost(...args);
export const tenantGameGoodsEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntitySelectStringPost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntitySelectStringPost(...args);
export const tenantGameGoodsEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameGoodsEntity>['tenantGameGoodsEntityUpdatePost']>) => getAPI(TenantGameGoodsEntity).tenantGameGoodsEntityUpdatePost(...args);
