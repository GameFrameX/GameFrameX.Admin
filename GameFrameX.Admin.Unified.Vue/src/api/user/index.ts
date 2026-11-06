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
  UpdateUserRequest,
  UserAddPostData,
  UserBaseInfoGetData,
  UserBaseInfoPostData,
  UserChangePwdPostData,
  UserDeletePostData,
  UserEntity,
  UserGrantRolePostData,
  UserOwnRoleListGetData,
  UserPagePostData,
  UserRequest,
  UserResetPwdPostData,
  UserRoleRequest,
  UserSetStatusPostData,
  UserUpdatePostData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class User<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags user
   * @name UserAddPost
   * @summary 增加用户
   * @request POST:/api/user/add
   */
  userAddPost = (data: AddUserRequest, params: RequestParams = {}) =>
    this.http.request<UserAddPostData, any>({
      path: `/api/user/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserBaseInfoGet
   * @summary 查看用户基本信息
   * @request GET:/api/user/baseInfo
   */
  userBaseInfoGet = (params: RequestParams = {}) =>
    this.http.request<UserBaseInfoGetData, any>({
      path: `/api/user/baseInfo`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserBaseInfoPost
   * @summary 更新用户基本信息
   * @request POST:/api/user/baseInfo
   */
  userBaseInfoPost = (data: UserEntity, params: RequestParams = {}) =>
    this.http.request<UserBaseInfoPostData, any>({
      path: `/api/user/baseInfo`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserChangePwdPost
   * @summary 修改用户密码
   * @request POST:/api/user/changePwd
   */
  userChangePwdPost = (data: ChangePwdRequest, params: RequestParams = {}) =>
    this.http.request<UserChangePwdPostData, any>({
      path: `/api/user/changePwd`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserDeletePost
   * @summary 删除用户
   * @request POST:/api/user/delete
   */
  userDeletePost = (data: DeleteUserRequest, params: RequestParams = {}) =>
    this.http.request<UserDeletePostData, any>({
      path: `/api/user/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserGrantRolePost
   * @summary 授权用户角色
   * @request POST:/api/user/grantRole
   */
  userGrantRolePost = (data: UserRoleRequest, params: RequestParams = {}) =>
    this.http.request<UserGrantRolePostData, any>({
      path: `/api/user/grantRole`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserOwnRoleListGet
   * @summary 获取用户拥有角色集合
   * @request GET:/api/user/ownRoleList/{userId}
   */
  userOwnRoleListGet = (userId: number, params: RequestParams = {}) =>
    this.http.request<UserOwnRoleListGetData, any>({
      path: `/api/user/ownRoleList/${userId}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserPagePost
   * @summary 获取用户分页列表
   * @request POST:/api/user/page
   */
  userPagePost = (data: PageUserRequest, params: RequestParams = {}) =>
    this.http.request<UserPagePostData, any>({
      path: `/api/user/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserResetPwdPost
   * @summary 重置用户密码
   * @request POST:/api/user/resetPwd
   */
  userResetPwdPost = (data: ResetPwdUserRequest, params: RequestParams = {}) =>
    this.http.request<UserResetPwdPostData, any>({
      path: `/api/user/resetPwd`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserSetStatusPost
   * @summary 设置用户状态
   * @request POST:/api/user/setStatus
   */
  userSetStatusPost = (data: UserRequest, params: RequestParams = {}) =>
    this.http.request<UserSetStatusPostData, any>({
      path: `/api/user/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags user
   * @name UserUpdatePost
   * @summary 更新用户
   * @request POST:/api/user/update
   */
  userUpdatePost = (data: UpdateUserRequest, params: RequestParams = {}) =>
    this.http.request<UserUpdatePostData, any>({
      path: `/api/user/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const userAddPost = (...args: Parameters<InstanceType<typeof User>['userAddPost']>) => getAPI(User).userAddPost(...args);
export const userBaseInfoGet = (...args: Parameters<InstanceType<typeof User>['userBaseInfoGet']>) => getAPI(User).userBaseInfoGet(...args);
export const userBaseInfoPost = (...args: Parameters<InstanceType<typeof User>['userBaseInfoPost']>) => getAPI(User).userBaseInfoPost(...args);
export const userChangePwdPost = (...args: Parameters<InstanceType<typeof User>['userChangePwdPost']>) => getAPI(User).userChangePwdPost(...args);
export const userDeletePost = (...args: Parameters<InstanceType<typeof User>['userDeletePost']>) => getAPI(User).userDeletePost(...args);
export const userGrantRolePost = (...args: Parameters<InstanceType<typeof User>['userGrantRolePost']>) => getAPI(User).userGrantRolePost(...args);
export const userOwnRoleListGet = (...args: Parameters<InstanceType<typeof User>['userOwnRoleListGet']>) => getAPI(User).userOwnRoleListGet(...args);
export const userPagePost = (...args: Parameters<InstanceType<typeof User>['userPagePost']>) => getAPI(User).userPagePost(...args);
export const userResetPwdPost = (...args: Parameters<InstanceType<typeof User>['userResetPwdPost']>) => getAPI(User).userResetPwdPost(...args);
export const userSetStatusPost = (...args: Parameters<InstanceType<typeof User>['userSetStatusPost']>) => getAPI(User).userSetStatusPost(...args);
export const userUpdatePost = (...args: Parameters<InstanceType<typeof User>['userUpdatePost']>) => getAPI(User).userUpdatePost(...args);
