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
  GlobalChannelDefinitionAddPostData,
  GlobalChannelDefinitionDeletePostData,
  GlobalChannelDefinitionDetailGetData,
  GlobalChannelDefinitionPagePostData,
  GlobalChannelDefinitionSelectPostData,
  GlobalChannelDefinitionSelectStringPostData,
  GlobalChannelDefinitionUpdatePostData,
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

export class GlobalChannelDefinition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionAddPost
   * @summary 新增
   * @request POST:/api/globalChannelDefinition/add
   */
  globalChannelDefinitionAddPost = (
    data: AddChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionAddPostData, any>({
      path: `/api/globalChannelDefinition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionDeletePost
   * @summary 删除渠道
   * @request POST:/api/globalChannelDefinition/delete
   */
  globalChannelDefinitionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionDeletePostData, any>({
      path: `/api/globalChannelDefinition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionDetailGet
   * @summary 详情
   * @request GET:/api/globalChannelDefinition/detail
   */
  globalChannelDefinitionDetailGet = (
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
    this.http.request<GlobalChannelDefinitionDetailGetData, any>({
      path: `/api/globalChannelDefinition/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionPagePost
   * @summary 分页查询
   * @request POST:/api/globalChannelDefinition/page
   */
  globalChannelDefinitionPagePost = (
    data: PageChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionPagePostData, any>({
      path: `/api/globalChannelDefinition/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/globalChannelDefinition/select
   */
  globalChannelDefinitionSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionSelectPostData, any>({
      path: `/api/globalChannelDefinition/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/globalChannelDefinition/selectString
   */
  globalChannelDefinitionSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionSelectStringPostData, any>({
      path: `/api/globalChannelDefinition/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalChannelDefinition
   * @name GlobalChannelDefinitionUpdatePost
   * @summary 更新
   * @request POST:/api/globalChannelDefinition/update
   */
  globalChannelDefinitionUpdatePost = (
    data: UpdateChannelDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalChannelDefinitionUpdatePostData, any>({
      path: `/api/globalChannelDefinition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalChannelDefinitionAddPost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionAddPost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionAddPost(...args);
export const globalChannelDefinitionDeletePost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionDeletePost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionDeletePost(...args);
export const globalChannelDefinitionDetailGet = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionDetailGet']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionDetailGet(...args);
export const globalChannelDefinitionPagePost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionPagePost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionPagePost(...args);
export const globalChannelDefinitionSelectPost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionSelectPost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionSelectPost(...args);
export const globalChannelDefinitionSelectStringPost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionSelectStringPost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionSelectStringPost(...args);
export const globalChannelDefinitionUpdatePost = (...args: Parameters<InstanceType<typeof GlobalChannelDefinition>['globalChannelDefinitionUpdatePost']>) => getAPI(GlobalChannelDefinition).globalChannelDefinitionUpdatePost(...args);
