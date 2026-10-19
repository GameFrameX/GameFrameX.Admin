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
  AddGameAppRemoteConfigRequest,
  PageGameAppRemoteConfigRequest,
  TenantGameAppRemoteConfigAddPostData,
  TenantGameAppRemoteConfigDeletePostData,
  TenantGameAppRemoteConfigDetailGetData,
  TenantGameAppRemoteConfigDispatchOnlinePostData,
  TenantGameAppRemoteConfigPagePostData,
  TenantGameAppRemoteConfigUpdatePostData,
  UpdateGameAppRemoteConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppRemoteConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigAddPost
   * @summary 新增
   * @request POST:/api/tenantGameAppRemoteConfig/add
   */
  tenantGameAppRemoteConfigAddPost = (
    data: AddGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigAddPostData, any>({
      path: `/api/tenantGameAppRemoteConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppRemoteConfig/delete
   */
  tenantGameAppRemoteConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigDeletePostData, any>({
      path: `/api/tenantGameAppRemoteConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppRemoteConfig/detail
   */
  tenantGameAppRemoteConfigDetailGet = (
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
    this.http.request<TenantGameAppRemoteConfigDetailGetData, any>({
      path: `/api/tenantGameAppRemoteConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigDispatchOnlinePost
   * @summary 经 Online 统一下发当前远程配置（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（publishRemoteConfig 通道）， 来源类型固定注入 RemoteConfig，存在性 / 应用归属 / 作用域校验在单一入口完成。
   * @request POST:/api/tenantGameAppRemoteConfig/dispatchOnline
   */
  tenantGameAppRemoteConfigDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigDispatchOnlinePostData, any>({
      path: `/api/tenantGameAppRemoteConfig/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigPagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppRemoteConfig/page
   */
  tenantGameAppRemoteConfigPagePost = (
    data: PageGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigPagePostData, any>({
      path: `/api/tenantGameAppRemoteConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfig
   * @name TenantGameAppRemoteConfigUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppRemoteConfig/update
   */
  tenantGameAppRemoteConfigUpdatePost = (
    data: UpdateGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigUpdatePostData, any>({
      path: `/api/tenantGameAppRemoteConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppRemoteConfigAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigAddPost']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigAddPost(...args);
export const tenantGameAppRemoteConfigDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigDeletePost']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigDeletePost(...args);
export const tenantGameAppRemoteConfigDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigDetailGet']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigDetailGet(...args);
export const tenantGameAppRemoteConfigDispatchOnlinePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigDispatchOnlinePost']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigDispatchOnlinePost(...args);
export const tenantGameAppRemoteConfigPagePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigPagePost']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigPagePost(...args);
export const tenantGameAppRemoteConfigUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfig>['tenantGameAppRemoteConfigUpdatePost']>) => getAPI(TenantGameAppRemoteConfig).tenantGameAppRemoteConfigUpdatePost(...args);
