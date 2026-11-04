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
  ChannelDefinitionAddPostData,
  ChannelDefinitionAvailableGetData,
  ChannelDefinitionDeletePostData,
  ChannelDefinitionDetailGetData,
  ChannelDefinitionExportPostData,
  ChannelDefinitionPagePostData,
  ChannelDefinitionSelectPostData,
  ChannelDefinitionSelectStringPostData,
  ChannelDefinitionUpdatePostData,
  PageChannelDefinitionRequestDataExportRequest,
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

export class ChannelDefinition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionAddPost
   * @summary 新增
   * @request POST:/api/channelDefinition/add
   */
  channelDefinitionAddPost = (
    data: AddChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionAddPostData, any>({
      path: `/api/channelDefinition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionAvailableGet
   * @summary 获取当前用户可用渠道
   * @request GET:/api/channelDefinition/available
   */
  channelDefinitionAvailableGet = (
    query?: {
      searchKey?: string;
      /**
       * @format int32
       * @min 1
       * @max 1000
       */
      pageSize?: number;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionAvailableGetData, any>({
      path: `/api/channelDefinition/available`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionDeletePost
   * @summary 删除渠道
   * @request POST:/api/channelDefinition/delete
   */
  channelDefinitionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionDeletePostData, any>({
      path: `/api/channelDefinition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionDetailGet
   * @summary 详情
   * @request GET:/api/channelDefinition/detail
   */
  channelDefinitionDetailGet = (
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
    this.http.request<ChannelDefinitionDetailGetData, any>({
      path: `/api/channelDefinition/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionExportPost
   * @summary 导出主库渠道定义。复用既有 BuildPageQuery， 仅导出请求列白名单中、未标记 GameFrameX.Core.Export.ExportSensitiveAttribute 的字段， 保留现有权限校验与导出行数限制。
   * @request POST:/api/channelDefinition/export
   */
  channelDefinitionExportPost = (
    data: PageChannelDefinitionRequestDataExportRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionExportPostData, any>({
      path: `/api/channelDefinition/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionPagePost
   * @summary 分页查询
   * @request POST:/api/channelDefinition/page
   */
  channelDefinitionPagePost = (
    data: PageChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionPagePostData, any>({
      path: `/api/channelDefinition/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/channelDefinition/select
   */
  channelDefinitionSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionSelectPostData, any>({
      path: `/api/channelDefinition/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/channelDefinition/selectString
   */
  channelDefinitionSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionSelectStringPostData, any>({
      path: `/api/channelDefinition/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags channelDefinition
   * @name ChannelDefinitionUpdatePost
   * @summary 更新
   * @request POST:/api/channelDefinition/update
   */
  channelDefinitionUpdatePost = (
    data: UpdateChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ChannelDefinitionUpdatePostData, any>({
      path: `/api/channelDefinition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const channelDefinitionAddPost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionAddPost']>) => getAPI(ChannelDefinition).channelDefinitionAddPost(...args);
export const channelDefinitionAvailableGet = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionAvailableGet']>) => getAPI(ChannelDefinition).channelDefinitionAvailableGet(...args);
export const channelDefinitionDeletePost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionDeletePost']>) => getAPI(ChannelDefinition).channelDefinitionDeletePost(...args);
export const channelDefinitionDetailGet = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionDetailGet']>) => getAPI(ChannelDefinition).channelDefinitionDetailGet(...args);
export const channelDefinitionExportPost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionExportPost']>) => getAPI(ChannelDefinition).channelDefinitionExportPost(...args);
export const channelDefinitionPagePost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionPagePost']>) => getAPI(ChannelDefinition).channelDefinitionPagePost(...args);
export const channelDefinitionSelectPost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionSelectPost']>) => getAPI(ChannelDefinition).channelDefinitionSelectPost(...args);
export const channelDefinitionSelectStringPost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionSelectStringPost']>) => getAPI(ChannelDefinition).channelDefinitionSelectStringPost(...args);
export const channelDefinitionUpdatePost = (...args: Parameters<InstanceType<typeof ChannelDefinition>['channelDefinitionUpdatePost']>) => getAPI(ChannelDefinition).channelDefinitionUpdatePost(...args);
