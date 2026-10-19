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
  AddTenantGameAppDeviceRequest,
  TenantGameAppDeviceAddPostData,
  TenantGameAppDeviceDeletePostData,
  TenantGameAppDeviceDetailGetData,
  TenantGameAppDevicePagePostData,
  TenantGameAppDeviceRequest,
  TenantGameAppDeviceSelectPostData,
  TenantGameAppDeviceSelectStringPostData,
  TenantGameAppDeviceUpdatePostData,
  UpdateTenantGameAppDeviceRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppDevice<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppDevice/add
   */
  tenantGameAppDeviceAddPost = (
    data: AddTenantGameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDeviceAddPostData, any>({
      path: `/api/tenantGameAppDevice/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppDevice/delete
   */
  tenantGameAppDeviceDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDeviceDeletePostData, any>({
      path: `/api/tenantGameAppDevice/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppDevice/detail
   */
  tenantGameAppDeviceDetailGet = (
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
    this.http.request<TenantGameAppDeviceDetailGetData, any>({
      path: `/api/tenantGameAppDevice/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDevicePagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppDevice/page
   */
  tenantGameAppDevicePagePost = (
    data: TenantGameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDevicePagePostData, any>({
      path: `/api/tenantGameAppDevice/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceSelectPost
   * @summary 获取选择器列表（按当前应用过滤）。
   * @request POST:/api/tenantGameAppDevice/select
   */
  tenantGameAppDeviceSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDeviceSelectPostData, any>({
      path: `/api/tenantGameAppDevice/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAppDevice/selectString
   */
  tenantGameAppDeviceSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDeviceSelectStringPostData, any>({
      path: `/api/tenantGameAppDevice/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppDevice
   * @name TenantGameAppDeviceUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppDevice/update
   */
  tenantGameAppDeviceUpdatePost = (
    data: UpdateTenantGameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppDeviceUpdatePostData, any>({
      path: `/api/tenantGameAppDevice/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppDeviceAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceAddPost']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceAddPost(...args);
export const tenantGameAppDeviceDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceDeletePost']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceDeletePost(...args);
export const tenantGameAppDeviceDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceDetailGet']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceDetailGet(...args);
export const tenantGameAppDevicePagePost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDevicePagePost']>) => getAPI(TenantGameAppDevice).tenantGameAppDevicePagePost(...args);
export const tenantGameAppDeviceSelectPost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceSelectPost']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceSelectPost(...args);
export const tenantGameAppDeviceSelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceSelectStringPost']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceSelectStringPost(...args);
export const tenantGameAppDeviceUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppDevice>['tenantGameAppDeviceUpdatePost']>) => getAPI(TenantGameAppDevice).tenantGameAppDeviceUpdatePost(...args);
