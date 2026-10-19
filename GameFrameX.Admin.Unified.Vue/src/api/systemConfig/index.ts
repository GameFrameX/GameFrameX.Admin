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
  AddConfigRequest,
  DeleteConfigRequest,
  PageConfigRequest,
  SystemConfigAddPostData,
  SystemConfigBatchDeletePostData,
  SystemConfigBatchDeletePostPayload,
  SystemConfigDeletePostData,
  SystemConfigDetailGetData,
  SystemConfigGroupListGetData,
  SystemConfigListGetData,
  SystemConfigPagePostData,
  SystemConfigUpdatePostData,
  UpdateConfigRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigAddPost
   * @summary 增加参数配置
   * @request POST:/api/systemConfig/add
   */
  systemConfigAddPost = (data: AddConfigRequest, params: RequestParams = {}) =>
    this.http.request<SystemConfigAddPostData, any>({
      path: `/api/systemConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigBatchDeletePost
   * @summary 批量删除参数配置
   * @request POST:/api/systemConfig/batchDelete
   */
  systemConfigBatchDeletePost = (
    data: SystemConfigBatchDeletePostPayload,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemConfigBatchDeletePostData, any>({
      path: `/api/systemConfig/batchDelete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigDeletePost
   * @summary 删除参数配置
   * @request POST:/api/systemConfig/delete
   */
  systemConfigDeletePost = (
    data: DeleteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemConfigDeletePostData, any>({
      path: `/api/systemConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigDetailGet
   * @summary 获取参数配置详情
   * @request GET:/api/systemConfig/detail
   */
  systemConfigDetailGet = (
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
    this.http.request<SystemConfigDetailGetData, any>({
      path: `/api/systemConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigGroupListGet
   * @summary 获取分组列表
   * @request GET:/api/systemConfig/groupList
   */
  systemConfigGroupListGet = (params: RequestParams = {}) =>
    this.http.request<SystemConfigGroupListGetData, any>({
      path: `/api/systemConfig/groupList`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigListGet
   * @summary 获取参数配置列表
   * @request GET:/api/systemConfig/list
   */
  systemConfigListGet = (params: RequestParams = {}) =>
    this.http.request<SystemConfigListGetData, any>({
      path: `/api/systemConfig/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigPagePost
   * @summary 获取参数配置分页列表
   * @request POST:/api/systemConfig/page
   */
  systemConfigPagePost = (
    data: PageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemConfigPagePostData, any>({
      path: `/api/systemConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemConfig
   * @name SystemConfigUpdatePost
   * @summary 更新参数配置
   * @request POST:/api/systemConfig/update
   */
  systemConfigUpdatePost = (
    data: UpdateConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemConfigUpdatePostData, any>({
      path: `/api/systemConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemConfigAddPost = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigAddPost']>) => getAPI(SystemConfig).systemConfigAddPost(...args);
export const systemConfigBatchDeletePost = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigBatchDeletePost']>) => getAPI(SystemConfig).systemConfigBatchDeletePost(...args);
export const systemConfigDeletePost = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigDeletePost']>) => getAPI(SystemConfig).systemConfigDeletePost(...args);
export const systemConfigDetailGet = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigDetailGet']>) => getAPI(SystemConfig).systemConfigDetailGet(...args);
export const systemConfigGroupListGet = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigGroupListGet']>) => getAPI(SystemConfig).systemConfigGroupListGet(...args);
export const systemConfigListGet = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigListGet']>) => getAPI(SystemConfig).systemConfigListGet(...args);
export const systemConfigPagePost = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigPagePost']>) => getAPI(SystemConfig).systemConfigPagePost(...args);
export const systemConfigUpdatePost = (...args: Parameters<InstanceType<typeof SystemConfig>['systemConfigUpdatePost']>) => getAPI(SystemConfig).systemConfigUpdatePost(...args);
