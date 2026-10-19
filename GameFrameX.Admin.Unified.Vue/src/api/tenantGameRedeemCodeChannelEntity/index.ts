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
  AddTenantGameRedeemCodeChannelEntityRequest,
  PageTenantGameRedeemCodeChannelEntityRequest,
  TenantGameRedeemCodeChannelEntityAddPostData,
  TenantGameRedeemCodeChannelEntityDeletePostData,
  TenantGameRedeemCodeChannelEntityDetailGetData,
  TenantGameRedeemCodeChannelEntityPagePostData,
  TenantGameRedeemCodeChannelEntitySelectPostData,
  TenantGameRedeemCodeChannelEntitySelectStringPostData,
  TenantGameRedeemCodeChannelEntityUpdatePostData,
  UpdateTenantGameRedeemCodeChannelEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRedeemCodeChannelEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/add
   */
  tenantGameRedeemCodeChannelEntityAddPost = (
    data: AddTenantGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeChannelEntityAddPostData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/delete
   */
  tenantGameRedeemCodeChannelEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeChannelEntityDeletePostData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameRedeemCodeChannelEntity/detail
   */
  tenantGameRedeemCodeChannelEntityDetailGet = (
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
    this.http.request<TenantGameRedeemCodeChannelEntityDetailGetData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/page
   */
  tenantGameRedeemCodeChannelEntityPagePost = (
    data: PageTenantGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeChannelEntityPagePostData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/select
   */
  tenantGameRedeemCodeChannelEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeChannelEntitySelectPostData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/selectString
   */
  tenantGameRedeemCodeChannelEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<
      TenantGameRedeemCodeChannelEntitySelectStringPostData,
      any
    >({
      path: `/api/tenantGameRedeemCodeChannelEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeChannelEntity
   * @name TenantGameRedeemCodeChannelEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameRedeemCodeChannelEntity/update
   */
  tenantGameRedeemCodeChannelEntityUpdatePost = (
    data: UpdateTenantGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeChannelEntityUpdatePostData, any>({
      path: `/api/tenantGameRedeemCodeChannelEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRedeemCodeChannelEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntityAddPost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntityAddPost(...args);
export const tenantGameRedeemCodeChannelEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntityDeletePost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntityDeletePost(...args);
export const tenantGameRedeemCodeChannelEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntityDetailGet']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntityDetailGet(...args);
export const tenantGameRedeemCodeChannelEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntityPagePost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntityPagePost(...args);
export const tenantGameRedeemCodeChannelEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntitySelectPost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntitySelectPost(...args);
export const tenantGameRedeemCodeChannelEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntitySelectStringPost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntitySelectStringPost(...args);
export const tenantGameRedeemCodeChannelEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeChannelEntity>['tenantGameRedeemCodeChannelEntityUpdatePost']>) => getAPI(TenantGameRedeemCodeChannelEntity).tenantGameRedeemCodeChannelEntityUpdatePost(...args);
