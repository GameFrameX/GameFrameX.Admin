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
  AddTenantGameAreaEntityRequest,
  PageTenantGameAreaEntityRequest,
  TenantGameAreaEntityAddPostData,
  TenantGameAreaEntityDeletePostData,
  TenantGameAreaEntityDetailGetData,
  TenantGameAreaEntityDisableDeletePostData,
  TenantGameAreaEntityOpenPostData,
  TenantGameAreaEntityPagePostData,
  TenantGameAreaEntityRestartPostData,
  TenantGameAreaEntitySelectPostData,
  TenantGameAreaEntitySelectStringPostData,
  TenantGameAreaEntityStopPostData,
  TenantGameAreaEntityUpdatePostData,
  UpdateTenantGameAreaEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAreaEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAreaEntity/add
   */
  tenantGameAreaEntityAddPost = (
    data: AddTenantGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityAddPostData, any>({
      path: `/api/tenantGameAreaEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAreaEntity/delete
   */
  tenantGameAreaEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityDeletePostData, any>({
      path: `/api/tenantGameAreaEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityDetailGet
   * @summary 详情：绕过全局过滤器读取（禁用区服 IsEnabled=false 会被 ISafeEnabledFilter 隐藏，详情页必须可见，C301）。
   * @request GET:/api/tenantGameAreaEntity/detail
   */
  tenantGameAreaEntityDetailGet = (
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
    this.http.request<TenantGameAreaEntityDetailGetData, any>({
      path: `/api/tenantGameAreaEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityDisableDeletePost
   * @summary 删除游戏区服
   * @request POST:/api/tenantGameAreaEntity/disableDelete
   */
  tenantGameAreaEntityDisableDeletePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityDisableDeletePostData, any>({
      path: `/api/tenantGameAreaEntity/disableDelete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityOpenPost
   * @summary 游戏区服开服
   * @request POST:/api/tenantGameAreaEntity/open
   */
  tenantGameAreaEntityOpenPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityOpenPostData, any>({
      path: `/api/tenantGameAreaEntity/open`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAreaEntity/page
   */
  tenantGameAreaEntityPagePost = (
    data: PageTenantGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityPagePostData, any>({
      path: `/api/tenantGameAreaEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityRestartPost
   * @summary 重启游戏区服
   * @request POST:/api/tenantGameAreaEntity/restart
   */
  tenantGameAreaEntityRestartPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityRestartPostData, any>({
      path: `/api/tenantGameAreaEntity/restart`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntitySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantGameAreaEntity/select
   */
  tenantGameAreaEntitySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntitySelectPostData, any>({
      path: `/api/tenantGameAreaEntity/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntitySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantGameAreaEntity/selectString
   */
  tenantGameAreaEntitySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntitySelectStringPostData, any>({
      path: `/api/tenantGameAreaEntity/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityStopPost
   * @summary 停止游戏区服
   * @request POST:/api/tenantGameAreaEntity/stop
   */
  tenantGameAreaEntityStopPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityStopPostData, any>({
      path: `/api/tenantGameAreaEntity/stop`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAreaEntity
   * @name TenantGameAreaEntityUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAreaEntity/update
   */
  tenantGameAreaEntityUpdatePost = (
    data: UpdateTenantGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAreaEntityUpdatePostData, any>({
      path: `/api/tenantGameAreaEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAreaEntityAddPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityAddPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityAddPost(...args);
export const tenantGameAreaEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityDeletePost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityDeletePost(...args);
export const tenantGameAreaEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityDetailGet']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityDetailGet(...args);
export const tenantGameAreaEntityDisableDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityDisableDeletePost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityDisableDeletePost(...args);
export const tenantGameAreaEntityOpenPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityOpenPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityOpenPost(...args);
export const tenantGameAreaEntityPagePost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityPagePost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityPagePost(...args);
export const tenantGameAreaEntityRestartPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityRestartPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityRestartPost(...args);
export const tenantGameAreaEntitySelectPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntitySelectPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntitySelectPost(...args);
export const tenantGameAreaEntitySelectStringPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntitySelectStringPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntitySelectStringPost(...args);
export const tenantGameAreaEntityStopPost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityStopPost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityStopPost(...args);
export const tenantGameAreaEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAreaEntity>['tenantGameAreaEntityUpdatePost']>) => getAPI(TenantGameAreaEntity).tenantGameAreaEntityUpdatePost(...args);
