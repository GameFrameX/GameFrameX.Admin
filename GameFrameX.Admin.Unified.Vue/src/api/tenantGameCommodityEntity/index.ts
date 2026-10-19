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
  AddTenantGameCommodityEntityRequest,
  PageTenantGameCommodityEntityRequest,
  TenantGameCommodityEntityAddPostData,
  TenantGameCommodityEntityDeletePostData,
  TenantGameCommodityEntityDetailGetData,
  TenantGameCommodityEntityPagePostData,
  TenantGameCommodityEntityUpdatePostData,
  UpdateTenantGameCommodityEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameCommodityEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameCommodityEntity
   * @name TenantGameCommodityEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameCommodityEntity/add
   */
  tenantGameCommodityEntityAddPost = (
    data: AddTenantGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameCommodityEntityAddPostData, any>({
      path: `/api/tenantGameCommodityEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameCommodityEntity
   * @name TenantGameCommodityEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameCommodityEntity/delete
   */
  tenantGameCommodityEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameCommodityEntityDeletePostData, any>({
      path: `/api/tenantGameCommodityEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameCommodityEntity
   * @name TenantGameCommodityEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameCommodityEntity/detail
   */
  tenantGameCommodityEntityDetailGet = (
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
    this.http.request<TenantGameCommodityEntityDetailGetData, any>({
      path: `/api/tenantGameCommodityEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameCommodityEntity
   * @name TenantGameCommodityEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameCommodityEntity/page
   */
  tenantGameCommodityEntityPagePost = (
    data: PageTenantGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameCommodityEntityPagePostData, any>({
      path: `/api/tenantGameCommodityEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameCommodityEntity
   * @name TenantGameCommodityEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameCommodityEntity/update
   */
  tenantGameCommodityEntityUpdatePost = (
    data: UpdateTenantGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameCommodityEntityUpdatePostData, any>({
      path: `/api/tenantGameCommodityEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameCommodityEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameCommodityEntity>['tenantGameCommodityEntityAddPost']>) => getAPI(TenantGameCommodityEntity).tenantGameCommodityEntityAddPost(...args);
export const tenantGameCommodityEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameCommodityEntity>['tenantGameCommodityEntityDeletePost']>) => getAPI(TenantGameCommodityEntity).tenantGameCommodityEntityDeletePost(...args);
export const tenantGameCommodityEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameCommodityEntity>['tenantGameCommodityEntityDetailGet']>) => getAPI(TenantGameCommodityEntity).tenantGameCommodityEntityDetailGet(...args);
export const tenantGameCommodityEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameCommodityEntity>['tenantGameCommodityEntityPagePost']>) => getAPI(TenantGameCommodityEntity).tenantGameCommodityEntityPagePost(...args);
export const tenantGameCommodityEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameCommodityEntity>['tenantGameCommodityEntityUpdatePost']>) => getAPI(TenantGameCommodityEntity).tenantGameCommodityEntityUpdatePost(...args);
