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
  AddObjectStorageConfigRequest,
  ObjectStorageConfigAddPostData,
  ObjectStorageConfigDeletePostData,
  ObjectStorageConfigDetailGetData,
  ObjectStorageConfigPagePostData,
  ObjectStorageConfigProvidersGetData,
  ObjectStorageConfigSchemaGetData,
  ObjectStorageConfigTestConnectionGetData,
  ObjectStorageConfigUpdatePostData,
  PageObjectStorageConfigRequest,
  UpdateObjectStorageConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class ObjectStorageConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigAddPost
   * @summary 新增配置。校验 Provider 已注册、规范化 ConfigJson；若启用则在事务内停用其它配置。
   * @request POST:/api/objectStorageConfig/add
   */
  objectStorageConfigAddPost = (
    data: AddObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ObjectStorageConfigAddPostData, any>({
      path: `/api/objectStorageConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigDeletePost
   * @summary 删除
   * @request POST:/api/objectStorageConfig/delete
   */
  objectStorageConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ObjectStorageConfigDeletePostData, any>({
      path: `/api/objectStorageConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigDetailGet
   * @summary 详情，ConfigJson 字段级脱敏（仅敏感字段替换为 ******）。
   * @request GET:/api/objectStorageConfig/detail
   */
  objectStorageConfigDetailGet = (
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
    this.http.request<ObjectStorageConfigDetailGetData, any>({
      path: `/api/objectStorageConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigPagePost
   * @summary 分页查询，ConfigJson 整体脱敏。
   * @request POST:/api/objectStorageConfig/page
   */
  objectStorageConfigPagePost = (
    data: PageObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ObjectStorageConfigPagePostData, any>({
      path: `/api/objectStorageConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigProvidersGet
   * @summary 获取所有支持的存储提供商清单。
   * @request GET:/api/objectStorageConfig/providers
   */
  objectStorageConfigProvidersGet = (params: RequestParams = {}) =>
    this.http.request<ObjectStorageConfigProvidersGetData, any>({
      path: `/api/objectStorageConfig/providers`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigSchemaGet
   * @summary 获取指定提供商的配置字段 Schema。
   * @request GET:/api/objectStorageConfig/schema
   */
  objectStorageConfigSchemaGet = (
    query?: {
      provider?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<ObjectStorageConfigSchemaGetData, any>({
      path: `/api/objectStorageConfig/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigTestConnectionGet
   * @summary 使用已保存配置测试连通性（不回显凭证）。
   * @request GET:/api/objectStorageConfig/testConnection
   */
  objectStorageConfigTestConnectionGet = (
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
    this.http.request<ObjectStorageConfigTestConnectionGetData, any>({
      path: `/api/objectStorageConfig/testConnection`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags objectStorageConfig
   * @name ObjectStorageConfigUpdatePost
   * @summary 更新配置。Provider 必须与既有记录一致；ConfigJson 规范化（****** 沿用旧值）；启用时事务停用其它。
   * @request POST:/api/objectStorageConfig/update
   */
  objectStorageConfigUpdatePost = (
    data: UpdateObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<ObjectStorageConfigUpdatePostData, any>({
      path: `/api/objectStorageConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const objectStorageConfigAddPost = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigAddPost']>) => getAPI(ObjectStorageConfig).objectStorageConfigAddPost(...args);
export const objectStorageConfigDeletePost = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigDeletePost']>) => getAPI(ObjectStorageConfig).objectStorageConfigDeletePost(...args);
export const objectStorageConfigDetailGet = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigDetailGet']>) => getAPI(ObjectStorageConfig).objectStorageConfigDetailGet(...args);
export const objectStorageConfigPagePost = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigPagePost']>) => getAPI(ObjectStorageConfig).objectStorageConfigPagePost(...args);
export const objectStorageConfigProvidersGet = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigProvidersGet']>) => getAPI(ObjectStorageConfig).objectStorageConfigProvidersGet(...args);
export const objectStorageConfigSchemaGet = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigSchemaGet']>) => getAPI(ObjectStorageConfig).objectStorageConfigSchemaGet(...args);
export const objectStorageConfigTestConnectionGet = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigTestConnectionGet']>) => getAPI(ObjectStorageConfig).objectStorageConfigTestConnectionGet(...args);
export const objectStorageConfigUpdatePost = (...args: Parameters<InstanceType<typeof ObjectStorageConfig>['objectStorageConfigUpdatePost']>) => getAPI(ObjectStorageConfig).objectStorageConfigUpdatePost(...args);
