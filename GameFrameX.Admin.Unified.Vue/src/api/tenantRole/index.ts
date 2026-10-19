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
  RoleMenuRequest,
  RoleOrganizationRequest,
  RoleRequest,
  TenantRoleAddPostData,
  TenantRoleDeletePostData,
  TenantRoleGrantDataScopePostData,
  TenantRoleGrantMenuPostData,
  TenantRoleListGetData,
  TenantRoleOwnMenuListGetData,
  TenantRoleOwnOrgListGetData,
  TenantRolePagePostData,
  TenantRoleSetStatusPostData,
  TenantRoleUpdatePostData,
  UpdateRoleRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantRole<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleAddPost
   * @summary 增加角色
   * @request POST:/api/tenantRole/add
   */
  tenantRoleAddPost = (data: AddRoleRequest, params: RequestParams = {}) =>
    this.http.request<TenantRoleAddPostData, any>({
      path: `/api/tenantRole/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleDeletePost
   * @summary 删除角色
   * @request POST:/api/tenantRole/delete
   */
  tenantRoleDeletePost = (
    data: DeleteRoleRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantRoleDeletePostData, any>({
      path: `/api/tenantRole/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleGrantDataScopePost
   * @summary 授权角色数据范围
   * @request POST:/api/tenantRole/grantDataScope
   */
  tenantRoleGrantDataScopePost = (
    data: RoleOrganizationRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantRoleGrantDataScopePostData, any>({
      path: `/api/tenantRole/grantDataScope`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleGrantMenuPost
   * @summary 授权角色菜单
   * @request POST:/api/tenantRole/grantMenu
   */
  tenantRoleGrantMenuPost = (
    data: RoleMenuRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantRoleGrantMenuPostData, any>({
      path: `/api/tenantRole/grantMenu`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleListGet
   * @summary 获取角色列表
   * @request GET:/api/tenantRole/list
   */
  tenantRoleListGet = (params: RequestParams = {}) =>
    this.http.request<TenantRoleListGetData, any>({
      path: `/api/tenantRole/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleOwnMenuListGet
   * @summary 根据角色Id获取菜单Id集合
   * @request GET:/api/tenantRole/ownMenuList
   */
  tenantRoleOwnMenuListGet = (
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
    this.http.request<TenantRoleOwnMenuListGetData, any>({
      path: `/api/tenantRole/ownMenuList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleOwnOrgListGet
   * @summary 根据角色Id获取机构Id集合
   * @request GET:/api/tenantRole/ownOrgList
   */
  tenantRoleOwnOrgListGet = (
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
    this.http.request<TenantRoleOwnOrgListGetData, any>({
      path: `/api/tenantRole/ownOrgList`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRolePagePost
   * @summary 获取角色分页列表
   * @request POST:/api/tenantRole/page
   */
  tenantRolePagePost = (data: PageRoleRequest, params: RequestParams = {}) =>
    this.http.request<TenantRolePagePostData, any>({
      path: `/api/tenantRole/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleSetStatusPost
   * @summary 设置角色状态
   * @request POST:/api/tenantRole/setStatus
   */
  tenantRoleSetStatusPost = (data: RoleRequest, params: RequestParams = {}) =>
    this.http.request<TenantRoleSetStatusPostData, any>({
      path: `/api/tenantRole/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantRole
   * @name TenantRoleUpdatePost
   * @summary 更新角色
   * @request POST:/api/tenantRole/update
   */
  tenantRoleUpdatePost = (
    data: UpdateRoleRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantRoleUpdatePostData, any>({
      path: `/api/tenantRole/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantRoleAddPost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleAddPost']>) => getAPI(TenantRole).tenantRoleAddPost(...args);
export const tenantRoleDeletePost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleDeletePost']>) => getAPI(TenantRole).tenantRoleDeletePost(...args);
export const tenantRoleGrantDataScopePost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleGrantDataScopePost']>) => getAPI(TenantRole).tenantRoleGrantDataScopePost(...args);
export const tenantRoleGrantMenuPost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleGrantMenuPost']>) => getAPI(TenantRole).tenantRoleGrantMenuPost(...args);
export const tenantRoleListGet = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleListGet']>) => getAPI(TenantRole).tenantRoleListGet(...args);
export const tenantRoleOwnMenuListGet = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleOwnMenuListGet']>) => getAPI(TenantRole).tenantRoleOwnMenuListGet(...args);
export const tenantRoleOwnOrgListGet = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleOwnOrgListGet']>) => getAPI(TenantRole).tenantRoleOwnOrgListGet(...args);
export const tenantRolePagePost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRolePagePost']>) => getAPI(TenantRole).tenantRolePagePost(...args);
export const tenantRoleSetStatusPost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleSetStatusPost']>) => getAPI(TenantRole).tenantRoleSetStatusPost(...args);
export const tenantRoleUpdatePost = (...args: Parameters<InstanceType<typeof TenantRole>['tenantRoleUpdatePost']>) => getAPI(TenantRole).tenantRoleUpdatePost(...args);
