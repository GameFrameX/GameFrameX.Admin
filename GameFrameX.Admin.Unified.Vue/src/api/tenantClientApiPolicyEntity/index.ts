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
  AddTenantClientApiPolicyRequest,
  PageTenantClientApiPolicyRequest,
  TenantClientApiPolicyEntityAddPostData,
  TenantClientApiPolicyEntityDeletePostData,
  TenantClientApiPolicyEntityDetailGetData,
  TenantClientApiPolicyEntityPagePostData,
  TenantClientApiPolicyEntityRotatePostData,
  TenantClientApiPolicyEntityUpdatePostData,
  UpdateTenantClientApiPolicyRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantClientApiPolicyEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityAddPost
   * @summary 新增运行策略（(TenantId, AppId) 作用域唯一）。SigningKey 空白时服务端生成 64-hex（C368）。
   * @request POST:/api/tenantClientApiPolicyEntity/add
   */
  tenantClientApiPolicyEntityAddPost = (
    data: AddTenantClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantClientApiPolicyEntityAddPostData, any>({
      path: `/api/tenantClientApiPolicyEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantClientApiPolicyEntity/delete
   */
  tenantClientApiPolicyEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantClientApiPolicyEntityDeletePostData, any>({
      path: `/api/tenantClientApiPolicyEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/tenantClientApiPolicyEntity/detail
   */
  tenantClientApiPolicyEntityDetailGet = (
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
    this.http.request<TenantClientApiPolicyEntityDetailGetData, any>({
      path: `/api/tenantClientApiPolicyEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/tenantClientApiPolicyEntity/page
   */
  tenantClientApiPolicyEntityPagePost = (
    data: PageTenantClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantClientApiPolicyEntityPagePostData, any>({
      path: `/api/tenantClientApiPolicyEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityRotatePost
   * @summary 轮换策略签名密钥（C373）：生成新 64-hex 加密落库，旧值即刻作废——该密钥纯服务端内部使用， 无双轨需求（区别于 HMAC 密钥环/票据 RSA 的租约退场模型）。 响应不回显密钥（C368 背景已论证：任何响应均不回显明文/密文）。
   * @request POST:/api/tenantClientApiPolicyEntity/rotate
   */
  tenantClientApiPolicyEntityRotatePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantClientApiPolicyEntityRotatePostData, any>({
      path: `/api/tenantClientApiPolicyEntity/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantClientApiPolicyEntity
   * @name TenantClientApiPolicyEntityUpdatePost
   * @summary 更新运行策略。SigningKey 回传 `******` 或留空均保留原值（C368：空白与掩码同义），新明文加密落库。
   * @request POST:/api/tenantClientApiPolicyEntity/update
   */
  tenantClientApiPolicyEntityUpdatePost = (
    data: UpdateTenantClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantClientApiPolicyEntityUpdatePostData, any>({
      path: `/api/tenantClientApiPolicyEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantClientApiPolicyEntityAddPost = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityAddPost']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityAddPost(...args);
export const tenantClientApiPolicyEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityDeletePost']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityDeletePost(...args);
export const tenantClientApiPolicyEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityDetailGet']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityDetailGet(...args);
export const tenantClientApiPolicyEntityPagePost = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityPagePost']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityPagePost(...args);
export const tenantClientApiPolicyEntityRotatePost = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityRotatePost']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityRotatePost(...args);
export const tenantClientApiPolicyEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantClientApiPolicyEntity>['tenantClientApiPolicyEntityUpdatePost']>) => getAPI(TenantClientApiPolicyEntity).tenantClientApiPolicyEntityUpdatePost(...args);
