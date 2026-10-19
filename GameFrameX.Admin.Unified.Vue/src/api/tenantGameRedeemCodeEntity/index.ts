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
  AddTenantGameRedeemCodeEntityRequest,
  PageTenantGameRedeemCodeEntityRequest,
  TenantGameRedeemCodeEntityAddPostData,
  TenantGameRedeemCodeEntityDeletePostData,
  TenantGameRedeemCodeEntityDetailGetData,
  TenantGameRedeemCodeEntityPagePostData,
  TenantGameRedeemCodeEntityUpdatePostData,
  UpdateTenantGameRedeemCodeEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRedeemCodeEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRedeemCodeEntity
   * @name TenantGameRedeemCodeEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRedeemCodeEntity/add
   */
  tenantGameRedeemCodeEntityAddPost = (
    data: AddTenantGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeEntityAddPostData, any>({
      path: `/api/tenantGameRedeemCodeEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeEntity
   * @name TenantGameRedeemCodeEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRedeemCodeEntity/delete
   */
  tenantGameRedeemCodeEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeEntityDeletePostData, any>({
      path: `/api/tenantGameRedeemCodeEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeEntity
   * @name TenantGameRedeemCodeEntityDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameRedeemCodeEntity/detail
   */
  tenantGameRedeemCodeEntityDetailGet = (
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
    this.http.request<TenantGameRedeemCodeEntityDetailGetData, any>({
      path: `/api/tenantGameRedeemCodeEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeEntity
   * @name TenantGameRedeemCodeEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameRedeemCodeEntity/page
   */
  tenantGameRedeemCodeEntityPagePost = (
    data: PageTenantGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeEntityPagePostData, any>({
      path: `/api/tenantGameRedeemCodeEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRedeemCodeEntity
   * @name TenantGameRedeemCodeEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameRedeemCodeEntity/update
   */
  tenantGameRedeemCodeEntityUpdatePost = (
    data: UpdateTenantGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRedeemCodeEntityUpdatePostData, any>({
      path: `/api/tenantGameRedeemCodeEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRedeemCodeEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeEntity>['tenantGameRedeemCodeEntityAddPost']>) => getAPI(TenantGameRedeemCodeEntity).tenantGameRedeemCodeEntityAddPost(...args);
export const tenantGameRedeemCodeEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeEntity>['tenantGameRedeemCodeEntityDeletePost']>) => getAPI(TenantGameRedeemCodeEntity).tenantGameRedeemCodeEntityDeletePost(...args);
export const tenantGameRedeemCodeEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeEntity>['tenantGameRedeemCodeEntityDetailGet']>) => getAPI(TenantGameRedeemCodeEntity).tenantGameRedeemCodeEntityDetailGet(...args);
export const tenantGameRedeemCodeEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeEntity>['tenantGameRedeemCodeEntityPagePost']>) => getAPI(TenantGameRedeemCodeEntity).tenantGameRedeemCodeEntityPagePost(...args);
export const tenantGameRedeemCodeEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRedeemCodeEntity>['tenantGameRedeemCodeEntityUpdatePost']>) => getAPI(TenantGameRedeemCodeEntity).tenantGameRedeemCodeEntityUpdatePost(...args);
