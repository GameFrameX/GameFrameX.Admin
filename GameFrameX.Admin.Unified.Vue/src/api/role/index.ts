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
  AddRoleRequest,
  DeleteRoleRequest,
  PageRoleRequest,
  RoleAddPostData,
  RoleDeletePostData,
  RoleGrantMenuPostData,
  RoleListGetData,
  RoleMenuRequest,
  RoleOwnMenuListGetData,
  RolePagePostData,
  RoleRequest,
  RoleSetStatusPostData,
  RoleUpdatePostData,
  UpdateRoleRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class Role<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags role
   * @name RoleAddPost
   * @summary 增加角色
   * @request POST:/api/role/add
   */
  roleAddPost = (data: AddRoleRequest, params: RequestParams = {}) =>
    this.http.request<RoleAddPostData, any>({
      path: `/api/role/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleDeletePost
   * @summary 删除角色
   * @request POST:/api/role/delete
   */
  roleDeletePost = (data: DeleteRoleRequest, params: RequestParams = {}) =>
    this.http.request<RoleDeletePostData, any>({
      path: `/api/role/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleGrantMenuPost
   * @summary 授权角色菜单
   * @request POST:/api/role/grantMenu
   */
  roleGrantMenuPost = (data: RoleMenuRequest, params: RequestParams = {}) =>
    this.http.request<RoleGrantMenuPostData, any>({
      path: `/api/role/grantMenu`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleListGet
   * @summary 获取角色列表
   * @request GET:/api/role/list
   */
  roleListGet = (params: RequestParams = {}) =>
    this.http.request<RoleListGetData, any>({
      path: `/api/role/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleOwnMenuListGet
   * @summary 根据角色Id获取菜单Id集合
   * @request GET:/api/role/ownMenuList
   */
  roleOwnMenuListGet = (
    query: {
      /** 是否启用 */
      isEnabled?: boolean;
      /**
       * @format int64
       * @min 1
       * @max 9223372036854776000
       */
      id: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<RoleOwnMenuListGetData, any>({
      path: `/api/role/ownMenuList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RolePagePost
   * @summary 获取角色分页列表
   * @request POST:/api/role/page
   */
  rolePagePost = (data: PageRoleRequest, params: RequestParams = {}) =>
    this.http.request<RolePagePostData, any>({
      path: `/api/role/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleSetStatusPost
   * @summary 设置角色状态
   * @request POST:/api/role/setStatus
   */
  roleSetStatusPost = (data: RoleRequest, params: RequestParams = {}) =>
    this.http.request<RoleSetStatusPostData, any>({
      path: `/api/role/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags role
   * @name RoleUpdatePost
   * @summary 更新角色
   * @request POST:/api/role/update
   */
  roleUpdatePost = (data: UpdateRoleRequest, params: RequestParams = {}) =>
    this.http.request<RoleUpdatePostData, any>({
      path: `/api/role/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const roleAddPost = (...args: Parameters<InstanceType<typeof Role>['roleAddPost']>) => getAPI(Role).roleAddPost(...args);
export const roleDeletePost = (...args: Parameters<InstanceType<typeof Role>['roleDeletePost']>) => getAPI(Role).roleDeletePost(...args);
export const roleGrantMenuPost = (...args: Parameters<InstanceType<typeof Role>['roleGrantMenuPost']>) => getAPI(Role).roleGrantMenuPost(...args);
export const roleListGet = (...args: Parameters<InstanceType<typeof Role>['roleListGet']>) => getAPI(Role).roleListGet(...args);
export const roleOwnMenuListGet = (...args: Parameters<InstanceType<typeof Role>['roleOwnMenuListGet']>) => getAPI(Role).roleOwnMenuListGet(...args);
export const rolePagePost = (...args: Parameters<InstanceType<typeof Role>['rolePagePost']>) => getAPI(Role).rolePagePost(...args);
export const roleSetStatusPost = (...args: Parameters<InstanceType<typeof Role>['roleSetStatusPost']>) => getAPI(Role).roleSetStatusPost(...args);
export const roleUpdatePost = (...args: Parameters<InstanceType<typeof Role>['roleUpdatePost']>) => getAPI(Role).roleUpdatePost(...args);
