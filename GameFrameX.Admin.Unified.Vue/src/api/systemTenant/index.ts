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
  AddTenantRequest,
  DeleteTenantRequest,
  PageTenantRequest,
  SystemTenantAddPostData,
  SystemTenantCreateDbPostData,
  SystemTenantDeletePostData,
  SystemTenantPagePostData,
  SystemTenantResetPwdPostData,
  SystemTenantSetStatusPostData,
  SystemTenantUpdatePostData,
  SystemTenantUserListPostData,
  TenantIdRequest,
  TenantRequest,
  TenantUserRequest,
  UpdateTenantRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemTenant<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantAddPost
   * @summary 增加租户
   * @request POST:/api/systemTenant/add
   */
  systemTenantAddPost = (data: AddTenantRequest, params: RequestParams = {}) =>
    this.http.request<SystemTenantAddPostData, any>({
      path: `/api/systemTenant/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantCreateDbPost
   * @summary 创建租户数据库
   * @request POST:/api/systemTenant/createDb
   */
  systemTenantCreateDbPost = (
    data: TenantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantCreateDbPostData, any>({
      path: `/api/systemTenant/createDb`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantDeletePost
   * @summary 删除租户
   * @request POST:/api/systemTenant/delete
   */
  systemTenantDeletePost = (
    data: DeleteTenantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantDeletePostData, any>({
      path: `/api/systemTenant/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantPagePost
   * @summary 获取租户分页列表
   * @request POST:/api/systemTenant/page
   */
  systemTenantPagePost = (
    data: PageTenantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantPagePostData, any>({
      path: `/api/systemTenant/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantResetPwdPost
   * @summary 重置租户管理员密码
   * @request POST:/api/systemTenant/resetPwd
   */
  systemTenantResetPwdPost = (
    data: TenantUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantResetPwdPostData, any>({
      path: `/api/systemTenant/resetPwd`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantSetStatusPost
   * @summary 设置租户状态
   * @request POST:/api/systemTenant/setStatus
   */
  systemTenantSetStatusPost = (
    data: TenantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantSetStatusPostData, any>({
      path: `/api/systemTenant/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantUpdatePost
   * @summary 更新租户
   * @request POST:/api/systemTenant/update
   */
  systemTenantUpdatePost = (
    data: UpdateTenantRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantUpdatePostData, any>({
      path: `/api/systemTenant/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemTenant
   * @name SystemTenantUserListPost
   * @summary 获取租户下的用户列表
   * @request POST:/api/systemTenant/userList
   */
  systemTenantUserListPost = (
    data: TenantIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemTenantUserListPostData, any>({
      path: `/api/systemTenant/userList`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const systemTenantAddPost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantAddPost']>) => getAPI(SystemTenant).systemTenantAddPost(...args);
export const systemTenantCreateDbPost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantCreateDbPost']>) => getAPI(SystemTenant).systemTenantCreateDbPost(...args);
export const systemTenantDeletePost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantDeletePost']>) => getAPI(SystemTenant).systemTenantDeletePost(...args);
export const systemTenantPagePost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantPagePost']>) => getAPI(SystemTenant).systemTenantPagePost(...args);
export const systemTenantResetPwdPost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantResetPwdPost']>) => getAPI(SystemTenant).systemTenantResetPwdPost(...args);
export const systemTenantSetStatusPost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantSetStatusPost']>) => getAPI(SystemTenant).systemTenantSetStatusPost(...args);
export const systemTenantUpdatePost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantUpdatePost']>) => getAPI(SystemTenant).systemTenantUpdatePost(...args);
export const systemTenantUserListPost = (...args: Parameters<InstanceType<typeof SystemTenant>['systemTenantUserListPost']>) => getAPI(SystemTenant).systemTenantUserListPost(...args);
