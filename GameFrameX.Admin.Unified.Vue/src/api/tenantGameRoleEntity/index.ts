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
  AddTenantGameRoleEntityRequest,
  PageTenantGameRoleEntityRequest,
  TenantGameRoleEntityAddPostData,
  TenantGameRoleEntityDeletePostData,
  TenantGameRoleEntityDetailGetData,
  TenantGameRoleEntityPagePostData,
  TenantGameRoleEntitySelectPostData,
  TenantGameRoleEntitySelectStringPostData,
  TenantGameRoleEntitySyncPostData,
  TenantGameRoleEntityUpdatePostData,
  TenantGameRoleEntityUpdateToSandBoxPostData,
  TenantGameRoleEntityUpdateToUnSandBoxPostData,
  UpdateTenantGameRoleEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameRoleEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameRoleEntity/add
   */
  tenantGameRoleEntityAddPost = (
    data: AddTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityAddPostData, any>({
      path: `/api/tenantGameRoleEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameRoleEntity/delete
   */
  tenantGameRoleEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityDeletePostData, any>({
      path: `/api/tenantGameRoleEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityDetailGet
   * @summary 获取游戏角色
   * @request GET:/api/tenantGameRoleEntity/detail
   */
  tenantGameRoleEntityDetailGet = (
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
    this.http.request<TenantGameRoleEntityDetailGetData, any>({
      path: `/api/tenantGameRoleEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityPagePost
   * @summary 分页查询游戏角色（含 RoleId 回填）
   * @request POST:/api/tenantGameRoleEntity/page
   */
  tenantGameRoleEntityPagePost = (
    data: PageTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityPagePostData, any>({
      path: `/api/tenantGameRoleEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameRoleEntity/select
   */
  tenantGameRoleEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntitySelectPostData, any>({
      path: `/api/tenantGameRoleEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameRoleEntity/selectString
   */
  tenantGameRoleEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntitySelectStringPostData, any>({
      path: `/api/tenantGameRoleEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntitySyncPost
   * @summary 同步游戏角色
   * @request POST:/api/tenantGameRoleEntity/sync
   */
  tenantGameRoleEntitySyncPost = (
    data: UpdateTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntitySyncPostData, any>({
      path: `/api/tenantGameRoleEntity/sync`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityUpdatePost
   * @summary 更新游戏角色（AppId 不可变，从既有实体回填，禁止跨应用迁移）
   * @request POST:/api/tenantGameRoleEntity/update
   */
  tenantGameRoleEntityUpdatePost = (
    data: UpdateTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityUpdatePostData, any>({
      path: `/api/tenantGameRoleEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityUpdateToSandBoxPost
   * @summary 更新游戏角色为沙盒用户
   * @request POST:/api/tenantGameRoleEntity/updateToSandBox
   */
  tenantGameRoleEntityUpdateToSandBoxPost = (
    data: UpdateTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityUpdateToSandBoxPostData, any>({
      path: `/api/tenantGameRoleEntity/updateToSandBox`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameRoleEntity
   * @name TenantGameRoleEntityUpdateToUnSandBoxPost
   * @summary 更新游戏角色为非沙盒用户
   * @request POST:/api/tenantGameRoleEntity/updateToUnSandBox
   */
  tenantGameRoleEntityUpdateToUnSandBoxPost = (
    data: UpdateTenantGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameRoleEntityUpdateToUnSandBoxPostData, any>({
      path: `/api/tenantGameRoleEntity/updateToUnSandBox`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameRoleEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityAddPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityAddPost(...args);
export const tenantGameRoleEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityDeletePost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityDeletePost(...args);
export const tenantGameRoleEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityDetailGet']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityDetailGet(...args);
export const tenantGameRoleEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityPagePost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityPagePost(...args);
export const tenantGameRoleEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntitySelectPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntitySelectPost(...args);
export const tenantGameRoleEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntitySelectStringPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntitySelectStringPost(...args);
export const tenantGameRoleEntitySyncPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntitySyncPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntitySyncPost(...args);
export const tenantGameRoleEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityUpdatePost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityUpdatePost(...args);
export const tenantGameRoleEntityUpdateToSandBoxPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityUpdateToSandBoxPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityUpdateToSandBoxPost(...args);
export const tenantGameRoleEntityUpdateToUnSandBoxPost = (...args: Parameters<InstanceType<typeof TenantGameRoleEntity>['tenantGameRoleEntityUpdateToUnSandBoxPost']>) => getAPI(TenantGameRoleEntity).tenantGameRoleEntityUpdateToUnSandBoxPost(...args);
