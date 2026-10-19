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
  AddWechatUserRequest,
  DeleteWechatUserRequest,
  PageWechatUserRequest,
  TenantWechatUserAddPostData,
  TenantWechatUserDeletePostData,
  TenantWechatUserPagePostData,
  TenantWechatUserUpdatePostData,
  UpdateWechatUserRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantWechatUser<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantWechatUser
   * @name TenantWechatUserAddPost
   * @summary 增加微信用户
   * @request POST:/api/tenantWechatUser/add
   */
  tenantWechatUserAddPost = (
    data: AddWechatUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatUserAddPostData, any>({
      path: `/api/tenantWechatUser/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatUser
   * @name TenantWechatUserDeletePost
   * @summary 删除微信用户
   * @request POST:/api/tenantWechatUser/delete
   */
  tenantWechatUserDeletePost = (
    data: DeleteWechatUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatUserDeletePostData, any>({
      path: `/api/tenantWechatUser/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatUser
   * @name TenantWechatUserPagePost
   * @summary 获取微信用户列表
   * @request POST:/api/tenantWechatUser/page
   */
  tenantWechatUserPagePost = (
    data: PageWechatUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatUserPagePostData, any>({
      path: `/api/tenantWechatUser/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantWechatUser
   * @name TenantWechatUserUpdatePost
   * @summary 更新微信用户
   * @request POST:/api/tenantWechatUser/update
   */
  tenantWechatUserUpdatePost = (
    data: UpdateWechatUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantWechatUserUpdatePostData, any>({
      path: `/api/tenantWechatUser/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantWechatUserAddPost = (...args: Parameters<InstanceType<typeof TenantWechatUser>['tenantWechatUserAddPost']>) => getAPI(TenantWechatUser).tenantWechatUserAddPost(...args);
export const tenantWechatUserDeletePost = (...args: Parameters<InstanceType<typeof TenantWechatUser>['tenantWechatUserDeletePost']>) => getAPI(TenantWechatUser).tenantWechatUserDeletePost(...args);
export const tenantWechatUserPagePost = (...args: Parameters<InstanceType<typeof TenantWechatUser>['tenantWechatUserPagePost']>) => getAPI(TenantWechatUser).tenantWechatUserPagePost(...args);
export const tenantWechatUserUpdatePost = (...args: Parameters<InstanceType<typeof TenantWechatUser>['tenantWechatUserUpdatePost']>) => getAPI(TenantWechatUser).tenantWechatUserUpdatePost(...args);
