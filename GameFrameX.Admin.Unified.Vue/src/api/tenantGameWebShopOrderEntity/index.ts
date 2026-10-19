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
  AddTenantGameWebShopOrderEntityRequest,
  PageTenantGameWebShopOrderEntityRequest,
  TenantGameWebShopOrderEntityAddPostData,
  TenantGameWebShopOrderEntityDeletePostData,
  TenantGameWebShopOrderEntityDetailGetData,
  TenantGameWebShopOrderEntityPagePostData,
  TenantGameWebShopOrderEntityRedoPostData,
  TenantGameWebShopOrderEntitySelectPostData,
  TenantGameWebShopOrderEntitySelectStringPostData,
  TenantGameWebShopOrderEntityUpdatePostData,
  UpdateTenantGameWebShopOrderEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
  BaseUpdateRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameWebShopOrderEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameWebShopOrderEntity/add
   */
  tenantGameWebShopOrderEntityAddPost = (
    data: AddTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntityAddPostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameWebShopOrderEntity/delete
   */
  tenantGameWebShopOrderEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntityDeletePostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameWebShopOrderEntity/detail
   */
  tenantGameWebShopOrderEntityDetailGet = (
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
    this.http.request<TenantGameWebShopOrderEntityDetailGetData, any>({
      path: `/api/tenantGameWebShopOrderEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameWebShopOrderEntity/page
   */
  tenantGameWebShopOrderEntityPagePost = (
    data: PageTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntityPagePostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityRedoPost
   * @summary 重新发送订单
   * @request POST:/api/tenantGameWebShopOrderEntity/redo
   */
  tenantGameWebShopOrderEntityRedoPost = (
    data: BaseUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntityRedoPostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/redo`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameWebShopOrderEntity/select
   */
  tenantGameWebShopOrderEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntitySelectPostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameWebShopOrderEntity/selectString
   */
  tenantGameWebShopOrderEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntitySelectStringPostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameWebShopOrderEntity
   * @name TenantGameWebShopOrderEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameWebShopOrderEntity/update
   */
  tenantGameWebShopOrderEntityUpdatePost = (
    data: UpdateTenantGameWebShopOrderEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameWebShopOrderEntityUpdatePostData, any>({
      path: `/api/tenantGameWebShopOrderEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameWebShopOrderEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityAddPost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityAddPost(...args);
export const tenantGameWebShopOrderEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityDeletePost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityDeletePost(...args);
export const tenantGameWebShopOrderEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityDetailGet']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityDetailGet(...args);
export const tenantGameWebShopOrderEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityPagePost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityPagePost(...args);
export const tenantGameWebShopOrderEntityRedoPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityRedoPost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityRedoPost(...args);
export const tenantGameWebShopOrderEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntitySelectPost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntitySelectPost(...args);
export const tenantGameWebShopOrderEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntitySelectStringPost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntitySelectStringPost(...args);
export const tenantGameWebShopOrderEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameWebShopOrderEntity>['tenantGameWebShopOrderEntityUpdatePost']>) => getAPI(TenantGameWebShopOrderEntity).tenantGameWebShopOrderEntityUpdatePost(...args);
