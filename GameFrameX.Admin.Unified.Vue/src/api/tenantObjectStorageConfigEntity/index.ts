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
  AddTenantObjectStorageConfigRequest,
  PageTenantObjectStorageConfigRequest,
  TenantObjectStorageConfigEntityAddPostData,
  TenantObjectStorageConfigEntityDeletePostData,
  TenantObjectStorageConfigEntityDetailGetData,
  TenantObjectStorageConfigEntityPagePostData,
  TenantObjectStorageConfigEntityProvidersGetData,
  TenantObjectStorageConfigEntitySchemaGetData,
  TenantObjectStorageConfigEntityTestConnectionGetData,
  TenantObjectStorageConfigEntityUpdatePostData,
  UpdateTenantObjectStorageConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantObjectStorageConfigEntity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityAddPost
   * @summary 新增配置。校验 Provider 已注册、规范化 ConfigJson；若启用则在事务内停用同租户其它配置。
   * @request POST:/api/tenantObjectStorageConfigEntity/add
   */
  tenantObjectStorageConfigEntityAddPost = (
    data: AddTenantObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantObjectStorageConfigEntityAddPostData, any>({
      path: `/api/tenantObjectStorageConfigEntity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityDeletePost
   * @summary 删除
   * @request POST:/api/tenantObjectStorageConfigEntity/delete
   */
  tenantObjectStorageConfigEntityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantObjectStorageConfigEntityDeletePostData, any>({
      path: `/api/tenantObjectStorageConfigEntity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityDetailGet
   * @summary 详情，ConfigJson 字段级脱敏（仅敏感字段替换为 ******）。
   * @request GET:/api/tenantObjectStorageConfigEntity/detail
   */
  tenantObjectStorageConfigEntityDetailGet = (
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
    this.http.request<TenantObjectStorageConfigEntityDetailGetData, any>({
      path: `/api/tenantObjectStorageConfigEntity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityPagePost
   * @summary 分页查询，ConfigJson 整体脱敏。
   * @request POST:/api/tenantObjectStorageConfigEntity/page
   */
  tenantObjectStorageConfigEntityPagePost = (
    data: PageTenantObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantObjectStorageConfigEntityPagePostData, any>({
      path: `/api/tenantObjectStorageConfigEntity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityProvidersGet
   * @summary 获取所有支持的存储提供商清单。
   * @request GET:/api/tenantObjectStorageConfigEntity/providers
   */
  tenantObjectStorageConfigEntityProvidersGet = (params: RequestParams = {}) =>
    this.http.request<TenantObjectStorageConfigEntityProvidersGetData, any>({
      path: `/api/tenantObjectStorageConfigEntity/providers`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntitySchemaGet
   * @summary 获取指定提供商的配置字段 Schema。
   * @request GET:/api/tenantObjectStorageConfigEntity/schema
   */
  tenantObjectStorageConfigEntitySchemaGet = (
    query?: {
      provider?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<TenantObjectStorageConfigEntitySchemaGetData, any>({
      path: `/api/tenantObjectStorageConfigEntity/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityTestConnectionGet
   * @summary 使用已保存配置测试连通性（不回显凭证）。
   * @request GET:/api/tenantObjectStorageConfigEntity/testConnection
   */
  tenantObjectStorageConfigEntityTestConnectionGet = (
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
    this.http.request<
      TenantObjectStorageConfigEntityTestConnectionGetData,
      any
    >({
      path: `/api/tenantObjectStorageConfigEntity/testConnection`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantObjectStorageConfigEntity
   * @name TenantObjectStorageConfigEntityUpdatePost
   * @summary 更新配置。Provider 必须与既有记录一致；ConfigJson 规范化（****** 沿用旧值）；启用时事务停用其它。
   * @request POST:/api/tenantObjectStorageConfigEntity/update
   */
  tenantObjectStorageConfigEntityUpdatePost = (
    data: UpdateTenantObjectStorageConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantObjectStorageConfigEntityUpdatePostData, any>({
      path: `/api/tenantObjectStorageConfigEntity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantObjectStorageConfigEntityAddPost = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityAddPost']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityAddPost(...args);
export const tenantObjectStorageConfigEntityDeletePost = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityDeletePost']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityDeletePost(...args);
export const tenantObjectStorageConfigEntityDetailGet = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityDetailGet']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityDetailGet(...args);
export const tenantObjectStorageConfigEntityPagePost = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityPagePost']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityPagePost(...args);
export const tenantObjectStorageConfigEntityProvidersGet = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityProvidersGet']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityProvidersGet(...args);
export const tenantObjectStorageConfigEntitySchemaGet = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntitySchemaGet']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntitySchemaGet(...args);
export const tenantObjectStorageConfigEntityTestConnectionGet = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityTestConnectionGet']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityTestConnectionGet(...args);
export const tenantObjectStorageConfigEntityUpdatePost = (...args: Parameters<InstanceType<typeof TenantObjectStorageConfigEntity>['tenantObjectStorageConfigEntityUpdatePost']>) => getAPI(TenantObjectStorageConfigEntity).tenantObjectStorageConfigEntityUpdatePost(...args);
