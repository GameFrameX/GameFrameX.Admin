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
  AddTenantDeviceGroupRequest,
  TenantDeviceGroupAddPostData,
  TenantDeviceGroupDeletePostData,
  TenantDeviceGroupDetailGetData,
  TenantDeviceGroupPagePostData,
  TenantDeviceGroupRequest,
  TenantDeviceGroupSelectPostData,
  TenantDeviceGroupSelectStringPostData,
  TenantDeviceGroupUpdatePostData,
  UpdateTenantDeviceGroupRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantDeviceGroup<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupAddPost
   * @summary 增加设备码分组
   * @request POST:/api/tenantDeviceGroup/add
   */
  tenantDeviceGroupAddPost = (
    data: AddTenantDeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupAddPostData, any>({
      path: `/api/tenantDeviceGroup/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupDeletePost
   * @summary 删除
   * @request POST:/api/tenantDeviceGroup/delete
   */
  tenantDeviceGroupDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupDeletePostData, any>({
      path: `/api/tenantDeviceGroup/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupDetailGet
   * @summary 详情
   * @request GET:/api/tenantDeviceGroup/detail
   */
  tenantDeviceGroupDetailGet = (
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
    this.http.request<TenantDeviceGroupDetailGetData, any>({
      path: `/api/tenantDeviceGroup/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupPagePost
   * @summary 分页查询
   * @request POST:/api/tenantDeviceGroup/page
   */
  tenantDeviceGroupPagePost = (
    data: TenantDeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupPagePostData, any>({
      path: `/api/tenantDeviceGroup/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupSelectPost
   * @summary 获取选择器列表（按当前应用过滤）。
   * @request POST:/api/tenantDeviceGroup/select
   */
  tenantDeviceGroupSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupSelectPostData, any>({
      path: `/api/tenantDeviceGroup/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantDeviceGroup/selectString
   */
  tenantDeviceGroupSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupSelectStringPostData, any>({
      path: `/api/tenantDeviceGroup/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantDeviceGroup
   * @name TenantDeviceGroupUpdatePost
   * @summary 更新
   * @request POST:/api/tenantDeviceGroup/update
   */
  tenantDeviceGroupUpdatePost = (
    data: UpdateTenantDeviceGroupRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantDeviceGroupUpdatePostData, any>({
      path: `/api/tenantDeviceGroup/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantDeviceGroupAddPost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupAddPost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupAddPost(...args);
export const tenantDeviceGroupDeletePost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupDeletePost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupDeletePost(...args);
export const tenantDeviceGroupDetailGet = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupDetailGet']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupDetailGet(...args);
export const tenantDeviceGroupPagePost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupPagePost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupPagePost(...args);
export const tenantDeviceGroupSelectPost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupSelectPost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupSelectPost(...args);
export const tenantDeviceGroupSelectStringPost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupSelectStringPost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupSelectStringPost(...args);
export const tenantDeviceGroupUpdatePost = (...args: Parameters<InstanceType<typeof TenantDeviceGroup>['tenantDeviceGroupUpdatePost']>) => getAPI(TenantDeviceGroup).tenantDeviceGroupUpdatePost(...args);
