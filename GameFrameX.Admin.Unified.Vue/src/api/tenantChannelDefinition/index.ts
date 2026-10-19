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
  PageChannelDefinitionRequestDataExportRequest,
  TenantChannelDefinitionAddPostData,
  TenantChannelDefinitionAvailableGetData,
  TenantChannelDefinitionDeletePostData,
  TenantChannelDefinitionDetailGetData,
  TenantChannelDefinitionExportPostData,
  TenantChannelDefinitionPagePostData,
  TenantChannelDefinitionSelectPostData,
  TenantChannelDefinitionSelectStringPostData,
  TenantChannelDefinitionUpdatePostData,
} from "./types";
import {
  AddChannelDefinitionRequest,
  BaseDeleteRequest,
  BaseSelectPageRequest,
  PageChannelDefinitionRequest,
  UpdateChannelDefinitionRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantChannelDefinition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionAddPost
   * @summary 新增
   * @request POST:/api/tenantChannelDefinition/add
   */
  tenantChannelDefinitionAddPost = (
    data: AddChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionAddPostData, any>({
      path: `/api/tenantChannelDefinition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionAvailableGet
   * @summary 获取当前用户可用渠道
   * @request GET:/api/tenantChannelDefinition/available
   */
  tenantChannelDefinitionAvailableGet = (
    query?: {
      searchKey?: string;
      /** @format int64 */
      appId?: number;
      /**
       * @format int32
       * @min 1
       * @max 1000
       */
      pageSize?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionAvailableGetData, any>({
      path: `/api/tenantChannelDefinition/available`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionDeletePost
   * @summary 删除渠道
   * @request POST:/api/tenantChannelDefinition/delete
   */
  tenantChannelDefinitionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionDeletePostData, any>({
      path: `/api/tenantChannelDefinition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionDetailGet
   * @summary 详情
   * @request GET:/api/tenantChannelDefinition/detail
   */
  tenantChannelDefinitionDetailGet = (
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
    this.http.request<TenantChannelDefinitionDetailGetData, any>({
      path: `/api/tenantChannelDefinition/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionExportPost
   * @summary 导出当前租户渠道定义。复用既有 BuildPageQuery（强制 TenantId 过滤）， 仅导出请求列白名单中、未标记 GameFrameX.Core.Export.ExportSensitiveAttribute 的字段， 租户账号不会跨租户泄露，超管不触发租户耦合导出。
   * @request POST:/api/tenantChannelDefinition/export
   */
  tenantChannelDefinitionExportPost = (
    data: PageChannelDefinitionRequestDataExportRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionExportPostData, any>({
      path: `/api/tenantChannelDefinition/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionPagePost
   * @summary 分页查询
   * @request POST:/api/tenantChannelDefinition/page
   */
  tenantChannelDefinitionPagePost = (
    data: PageChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionPagePostData, any>({
      path: `/api/tenantChannelDefinition/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/tenantChannelDefinition/select
   */
  tenantChannelDefinitionSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionSelectPostData, any>({
      path: `/api/tenantChannelDefinition/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/tenantChannelDefinition/selectString
   */
  tenantChannelDefinitionSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionSelectStringPostData, any>({
      path: `/api/tenantChannelDefinition/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantChannelDefinition
   * @name TenantChannelDefinitionUpdatePost
   * @summary 更新
   * @request POST:/api/tenantChannelDefinition/update
   */
  tenantChannelDefinitionUpdatePost = (
    data: UpdateChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantChannelDefinitionUpdatePostData, any>({
      path: `/api/tenantChannelDefinition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantChannelDefinitionAddPost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionAddPost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionAddPost(...args);
export const tenantChannelDefinitionAvailableGet = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionAvailableGet']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionAvailableGet(...args);
export const tenantChannelDefinitionDeletePost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionDeletePost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionDeletePost(...args);
export const tenantChannelDefinitionDetailGet = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionDetailGet']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionDetailGet(...args);
export const tenantChannelDefinitionExportPost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionExportPost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionExportPost(...args);
export const tenantChannelDefinitionPagePost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionPagePost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionPagePost(...args);
export const tenantChannelDefinitionSelectPost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionSelectPost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionSelectPost(...args);
export const tenantChannelDefinitionSelectStringPost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionSelectStringPost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionSelectStringPost(...args);
export const tenantChannelDefinitionUpdatePost = (...args: Parameters<InstanceType<typeof TenantChannelDefinition>['tenantChannelDefinitionUpdatePost']>) => getAPI(TenantChannelDefinition).tenantChannelDefinitionUpdatePost(...args);
