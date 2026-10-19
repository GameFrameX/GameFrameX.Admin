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
  AddGamePaymentConfigRequest,
  PageGamePaymentConfigRequest,
  TenantGamePaymentConfigEntityAddPostData,
  TenantGamePaymentConfigEntityChannelsGetData,
  TenantGamePaymentConfigEntityDeletePostData,
  TenantGamePaymentConfigEntityDetailGetData,
  TenantGamePaymentConfigEntityPagePostData,
  TenantGamePaymentConfigEntitySchemaGetData,
  TenantGamePaymentConfigEntityUpdatePostData,
  UpdateGamePaymentConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGamePaymentConfigEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityAddPost
   * @summary 新增支付配置
   * @request POST:/api/tenantGamePaymentConfigEntity/add
   */
  tenantGamePaymentConfigEntityAddPost = (
    data: AddGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePaymentConfigEntityAddPostData, any>({
      path: `/api/tenantGamePaymentConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityChannelsGet
   * @summary 获取所有支持渠道的清单
   * @request GET:/api/tenantGamePaymentConfigEntity/channels
   */
  tenantGamePaymentConfigEntityChannelsGet = (params: RequestParams = {}) =>
    this.http.request<TenantGamePaymentConfigEntityChannelsGetData, any>({
      path: `/api/tenantGamePaymentConfigEntity/channels`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGamePaymentConfigEntity/delete
   */
  tenantGamePaymentConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePaymentConfigEntityDeletePostData, any>({
      path: `/api/tenantGamePaymentConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityDetailGet
   * @summary 详情（配置脱敏）
   * @request GET:/api/tenantGamePaymentConfigEntity/detail
   */
  tenantGamePaymentConfigEntityDetailGet = (
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
    this.http.request<TenantGamePaymentConfigEntityDetailGetData, any>({
      path: `/api/tenantGamePaymentConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityPagePost
   * @summary 分页查询（配置脱敏）
   * @request POST:/api/tenantGamePaymentConfigEntity/page
   */
  tenantGamePaymentConfigEntityPagePost = (
    data: PageGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePaymentConfigEntityPagePostData, any>({
      path: `/api/tenantGamePaymentConfigEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntitySchemaGet
   * @summary 获取指定支付渠道的字段 schema
   * @request GET:/api/tenantGamePaymentConfigEntity/schema
   */
  tenantGamePaymentConfigEntitySchemaGet = (
    query?: {
      payChannel?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePaymentConfigEntitySchemaGetData, any>({
      path: `/api/tenantGamePaymentConfigEntity/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGamePaymentConfigEntity
   * @name TenantGamePaymentConfigEntityUpdatePost
   * @summary 更新支付配置
   * @request POST:/api/tenantGamePaymentConfigEntity/update
   */
  tenantGamePaymentConfigEntityUpdatePost = (
    data: UpdateGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGamePaymentConfigEntityUpdatePostData, any>({
      path: `/api/tenantGamePaymentConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGamePaymentConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityAddPost']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityAddPost(...args);
export const tenantGamePaymentConfigEntityChannelsGet = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityChannelsGet']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityChannelsGet(...args);
export const tenantGamePaymentConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityDeletePost']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityDeletePost(...args);
export const tenantGamePaymentConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityDetailGet']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityDetailGet(...args);
export const tenantGamePaymentConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityPagePost']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityPagePost(...args);
export const tenantGamePaymentConfigEntitySchemaGet = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntitySchemaGet']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntitySchemaGet(...args);
export const tenantGamePaymentConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGamePaymentConfigEntity>['tenantGamePaymentConfigEntityUpdatePost']>) => getAPI(TenantGamePaymentConfigEntity).tenantGamePaymentConfigEntityUpdatePost(...args);
