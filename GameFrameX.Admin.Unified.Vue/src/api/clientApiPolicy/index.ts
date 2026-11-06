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
  AddClientApiPolicyRequest,
  ClientApiPolicyAddPostData,
  ClientApiPolicyDeletePostData,
  ClientApiPolicyDetailGetData,
  ClientApiPolicyPagePostData,
  ClientApiPolicyRotatePostData,
  ClientApiPolicyUpdatePostData,
  PageClientApiPolicyRequest,
  UpdateClientApiPolicyRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class ClientApiPolicy<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyAddPost
   * @summary 新增主库唯一运行策略。SigningKey 空白时服务端生成 64 位十六进制密钥。
   * @request POST:/api/clientApiPolicy/add
   */
  clientApiPolicyAddPost = (
    data: AddClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ClientApiPolicyAddPostData, any>({
      path: `/api/clientApiPolicy/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyDeletePost
   * @summary 删除
   * @request POST:/api/clientApiPolicy/delete
   */
  clientApiPolicyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ClientApiPolicyDeletePostData, any>({
      path: `/api/clientApiPolicy/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyDetailGet
   * @summary 详情（敏感字段脱敏）。
   * @request GET:/api/clientApiPolicy/detail
   */
  clientApiPolicyDetailGet = (
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
    this.http.request<ClientApiPolicyDetailGetData, any>({
      path: `/api/clientApiPolicy/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyPagePost
   * @summary 分页查询（敏感字段脱敏）。
   * @request POST:/api/clientApiPolicy/page
   */
  clientApiPolicyPagePost = (
    data: PageClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ClientApiPolicyPagePostData, any>({
      path: `/api/clientApiPolicy/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyRotatePost
   * @summary 轮换策略签名密钥（C373）：生成新 64-hex 加密落库，旧值即刻作废——该密钥纯服务端内部使用， 无双轨需求（区别于 HMAC 密钥环/票据 RSA 的租约退场模型）。 响应不回显密钥（C368 背景已论证：任何响应均不回显明文/密文）。
   * @request POST:/api/clientApiPolicy/rotate
   */
  clientApiPolicyRotatePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ClientApiPolicyRotatePostData, any>({
      path: `/api/clientApiPolicy/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags clientApiPolicy
   * @name ClientApiPolicyUpdatePost
   * @summary 更新运行策略。SigningKey 回传 `******` 或留空均保留原值（C368：空白与掩码同义），新明文加密落库。
   * @request POST:/api/clientApiPolicy/update
   */
  clientApiPolicyUpdatePost = (
    data: UpdateClientApiPolicyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ClientApiPolicyUpdatePostData, any>({
      path: `/api/clientApiPolicy/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const clientApiPolicyAddPost = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyAddPost']>) => getAPI(ClientApiPolicy).clientApiPolicyAddPost(...args);
export const clientApiPolicyDeletePost = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyDeletePost']>) => getAPI(ClientApiPolicy).clientApiPolicyDeletePost(...args);
export const clientApiPolicyDetailGet = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyDetailGet']>) => getAPI(ClientApiPolicy).clientApiPolicyDetailGet(...args);
export const clientApiPolicyPagePost = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyPagePost']>) => getAPI(ClientApiPolicy).clientApiPolicyPagePost(...args);
export const clientApiPolicyRotatePost = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyRotatePost']>) => getAPI(ClientApiPolicy).clientApiPolicyRotatePost(...args);
export const clientApiPolicyUpdatePost = (...args: Parameters<InstanceType<typeof ClientApiPolicy>['clientApiPolicyUpdatePost']>) => getAPI(ClientApiPolicy).clientApiPolicyUpdatePost(...args);
