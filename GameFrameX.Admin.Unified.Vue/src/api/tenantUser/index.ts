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
  AddUserRequest,
  ChangePwdRequest,
  DeleteUserRequest,
  PageUserRequest,
  ResetPwdUserRequest,
  TenantUserAddPostData,
  TenantUserBaseInfoGetData,
  TenantUserBaseInfoPostData,
  TenantUserChangePwdPostData,
  TenantUserDeletePostData,
  TenantUserGrantRolePostData,
  TenantUserOwnExtOrgListGetData,
  TenantUserOwnRoleListGetData,
  TenantUserPagePostData,
  TenantUserResetPwdPostData,
  TenantUserSetStatusPostData,
  TenantUserUpdatePostData,
  UpdateUserRequest,
  UserRequest,
  UserRoleRequest,
} from "./types";
import {
  TenantUserEntity,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantUser<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserAddPost
   * @summary 增加用户
   * @request POST:/api/tenantUser/add
   */
  tenantUserAddPost = (data: AddUserRequest, params: RequestParams = {}) =>
    this.http.request<TenantUserAddPostData, any>({
      path: `/api/tenantUser/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserBaseInfoGet
   * @summary 查看用户基本信息
   * @request GET:/api/tenantUser/baseInfo
   */
  tenantUserBaseInfoGet = (params: RequestParams = {}) =>
    this.http.request<TenantUserBaseInfoGetData, any>({
      path: `/api/tenantUser/baseInfo`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserBaseInfoPost
   * @summary 更新用户基本信息
   * @request POST:/api/tenantUser/baseInfo
   */
  tenantUserBaseInfoPost = (
    data: TenantUserEntity,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserBaseInfoPostData, any>({
      path: `/api/tenantUser/baseInfo`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserChangePwdPost
   * @summary 修改用户密码
   * @request POST:/api/tenantUser/changePwd
   */
  tenantUserChangePwdPost = (
    data: ChangePwdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserChangePwdPostData, any>({
      path: `/api/tenantUser/changePwd`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserDeletePost
   * @summary 删除用户
   * @request POST:/api/tenantUser/delete
   */
  tenantUserDeletePost = (
    data: DeleteUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserDeletePostData, any>({
      path: `/api/tenantUser/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserGrantRolePost
   * @summary 授权用户角色
   * @request POST:/api/tenantUser/grantRole
   */
  tenantUserGrantRolePost = (
    data: UserRoleRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserGrantRolePostData, any>({
      path: `/api/tenantUser/grantRole`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserOwnExtOrgListGet
   * @summary 获取用户扩展机构集合
   * @request GET:/api/tenantUser/ownExtOrgList/{userId}
   */
  tenantUserOwnExtOrgListGet = (userId: number, params: RequestParams = {}) =>
    this.http.request<TenantUserOwnExtOrgListGetData, any>({
      path: `/api/tenantUser/ownExtOrgList/${userId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserOwnRoleListGet
   * @summary 获取用户拥有角色集合
   * @request GET:/api/tenantUser/ownRoleList/{userId}
   */
  tenantUserOwnRoleListGet = (userId: number, params: RequestParams = {}) =>
    this.http.request<TenantUserOwnRoleListGetData, any>({
      path: `/api/tenantUser/ownRoleList/${userId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserPagePost
   * @summary 获取用户分页列表
   * @request POST:/api/tenantUser/page
   */
  tenantUserPagePost = (data: PageUserRequest, params: RequestParams = {}) =>
    this.http.request<TenantUserPagePostData, any>({
      path: `/api/tenantUser/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserResetPwdPost
   * @summary 重置用户密码
   * @request POST:/api/tenantUser/resetPwd
   */
  tenantUserResetPwdPost = (
    data: ResetPwdUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserResetPwdPostData, any>({
      path: `/api/tenantUser/resetPwd`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserSetStatusPost
   * @summary 设置用户状态
   * @request POST:/api/tenantUser/setStatus
   */
  tenantUserSetStatusPost = (data: UserRequest, params: RequestParams = {}) =>
    this.http.request<TenantUserSetStatusPostData, any>({
      path: `/api/tenantUser/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantUser
   * @name TenantUserUpdatePost
   * @summary 更新用户
   * @request POST:/api/tenantUser/update
   */
  tenantUserUpdatePost = (
    data: UpdateUserRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantUserUpdatePostData, any>({
      path: `/api/tenantUser/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantUserAddPost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserAddPost']>) => getAPI(TenantUser).tenantUserAddPost(...args);
export const tenantUserBaseInfoGet = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserBaseInfoGet']>) => getAPI(TenantUser).tenantUserBaseInfoGet(...args);
export const tenantUserBaseInfoPost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserBaseInfoPost']>) => getAPI(TenantUser).tenantUserBaseInfoPost(...args);
export const tenantUserChangePwdPost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserChangePwdPost']>) => getAPI(TenantUser).tenantUserChangePwdPost(...args);
export const tenantUserDeletePost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserDeletePost']>) => getAPI(TenantUser).tenantUserDeletePost(...args);
export const tenantUserGrantRolePost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserGrantRolePost']>) => getAPI(TenantUser).tenantUserGrantRolePost(...args);
export const tenantUserOwnExtOrgListGet = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserOwnExtOrgListGet']>) => getAPI(TenantUser).tenantUserOwnExtOrgListGet(...args);
export const tenantUserOwnRoleListGet = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserOwnRoleListGet']>) => getAPI(TenantUser).tenantUserOwnRoleListGet(...args);
export const tenantUserPagePost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserPagePost']>) => getAPI(TenantUser).tenantUserPagePost(...args);
export const tenantUserResetPwdPost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserResetPwdPost']>) => getAPI(TenantUser).tenantUserResetPwdPost(...args);
export const tenantUserSetStatusPost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserSetStatusPost']>) => getAPI(TenantUser).tenantUserSetStatusPost(...args);
export const tenantUserUpdatePost = (...args: Parameters<InstanceType<typeof TenantUser>['tenantUserUpdatePost']>) => getAPI(TenantUser).tenantUserUpdatePost(...args);
