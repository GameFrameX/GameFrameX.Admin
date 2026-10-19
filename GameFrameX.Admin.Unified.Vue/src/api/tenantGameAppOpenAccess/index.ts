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
  AddTenantGameAppOpenAccessRequest,
  PageTenantGameAppOpenAccessRequest,
  TenantGameAppOpenAccessActionRequest,
  TenantGameAppOpenAccessAddPostData,
  TenantGameAppOpenAccessDeletePostData,
  TenantGameAppOpenAccessDisablePostData,
  TenantGameAppOpenAccessEnablePostData,
  TenantGameAppOpenAccessPagePostData,
  TenantGameAppOpenAccessRefreshPostData,
  TenantGameAppOpenAccessRevokePostData,
  TenantGameAppOpenAccessUpdatePostData,
  UpdateTenantGameAppOpenAccessRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppOpenAccess<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessAddPost
   * @summary 签发应用开放访问凭证。AccessKey / AccessSecret 由服务端生成，AccessSecret 仅本次响应一次性返回。
   * @request POST:/api/tenantGameAppOpenAccess/add
   */
  tenantGameAppOpenAccessAddPost = (
    data: AddTenantGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessAddPostData, any>({
      path: `/api/tenantGameAppOpenAccess/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessDeletePost
   * @summary 删除应用开放访问凭证。
   * @request POST:/api/tenantGameAppOpenAccess/delete
   */
  tenantGameAppOpenAccessDeletePost = (
    data: TenantGameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessDeletePostData, any>({
      path: `/api/tenantGameAppOpenAccess/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessDisablePost
   * @summary 禁用应用开放访问凭证。
   * @request POST:/api/tenantGameAppOpenAccess/disable
   */
  tenantGameAppOpenAccessDisablePost = (
    data: TenantGameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessDisablePostData, any>({
      path: `/api/tenantGameAppOpenAccess/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessEnablePost
   * @summary 启用应用开放访问凭证（已撤销的凭证不可启用）。
   * @request POST:/api/tenantGameAppOpenAccess/enable
   */
  tenantGameAppOpenAccessEnablePost = (
    data: TenantGameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessEnablePostData, any>({
      path: `/api/tenantGameAppOpenAccess/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessPagePost
   * @summary 获取应用开放访问凭证分页列表（当前 App 范围，AccessSecret 永不输出）。
   * @request POST:/api/tenantGameAppOpenAccess/page
   */
  tenantGameAppOpenAccessPagePost = (
    data: PageTenantGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessPagePostData, any>({
      path: `/api/tenantGameAppOpenAccess/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessRefreshPost
   * @summary 续期 / 轮换密钥。重置 AccessSecret 并一次性返回（前端须立即保存）。
   * @request POST:/api/tenantGameAppOpenAccess/refresh
   */
  tenantGameAppOpenAccessRefreshPost = (
    data: TenantGameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessRefreshPostData, any>({
      path: `/api/tenantGameAppOpenAccess/refresh`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessRevokePost
   * @summary 吊销应用开放访问凭证（写 RevokedTime / RevokeReason，并禁用）。
   * @request POST:/api/tenantGameAppOpenAccess/revoke
   */
  tenantGameAppOpenAccessRevokePost = (
    data: TenantGameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessRevokePostData, any>({
      path: `/api/tenantGameAppOpenAccess/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppOpenAccess
   * @name TenantGameAppOpenAccessUpdatePost
   * @summary 更新应用开放访问凭证。AppId 不可变（由服务端按行原值保留），校验 BindUserId 属租户与 Scopes 有效。
   * @request POST:/api/tenantGameAppOpenAccess/update
   */
  tenantGameAppOpenAccessUpdatePost = (
    data: UpdateTenantGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppOpenAccessUpdatePostData, any>({
      path: `/api/tenantGameAppOpenAccess/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppOpenAccessAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessAddPost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessAddPost(...args);
export const tenantGameAppOpenAccessDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessDeletePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessDeletePost(...args);
export const tenantGameAppOpenAccessDisablePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessDisablePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessDisablePost(...args);
export const tenantGameAppOpenAccessEnablePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessEnablePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessEnablePost(...args);
export const tenantGameAppOpenAccessPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessPagePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessPagePost(...args);
export const tenantGameAppOpenAccessRefreshPost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessRefreshPost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessRefreshPost(...args);
export const tenantGameAppOpenAccessRevokePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessRevokePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessRevokePost(...args);
export const tenantGameAppOpenAccessUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppOpenAccess>['tenantGameAppOpenAccessUpdatePost']>) => getAPI(TenantGameAppOpenAccess).tenantGameAppOpenAccessUpdatePost(...args);
