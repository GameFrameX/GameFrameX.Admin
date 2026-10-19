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
  AddGameAppRemoteConfigModeRequest,
  PageGameAppRemoteConfigModeRequest,
  TenantGameAppRemoteConfigModeAddPostData,
  TenantGameAppRemoteConfigModeDeletePostData,
  TenantGameAppRemoteConfigModeDetailGetData,
  TenantGameAppRemoteConfigModePagePostData,
  TenantGameAppRemoteConfigModeUpdatePostData,
  UpdateGameAppRemoteConfigModeRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class TenantGameAppRemoteConfigMode<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigMode
   * @name TenantGameAppRemoteConfigModeAddPost
   * @summary 新增模式定义。 显式声明 AddAsync 方法，确保 Admin API 约定以强类型 GameFrameX.Application.Game.App.GameAppRemoteConfigMode.Dto.AddGameAppRemoteConfigModeRequest 绑定参数， 避免继承的泛型 AddAsync 方法被解析为 dynamic/ExpandoObject 后在基类 (BaseAddRequest) 强转时抛 InvalidCastException。
   * @request POST:/api/tenantGameAppRemoteConfigMode/add
   */
  tenantGameAppRemoteConfigModeAddPost = (
    data: AddGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigModeAddPostData, any>({
      path: `/api/tenantGameAppRemoteConfigMode/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigMode
   * @name TenantGameAppRemoteConfigModeDeletePost
   * @summary 删除
   * @request POST:/api/tenantGameAppRemoteConfigMode/delete
   */
  tenantGameAppRemoteConfigModeDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigModeDeletePostData, any>({
      path: `/api/tenantGameAppRemoteConfigMode/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigMode
   * @name TenantGameAppRemoteConfigModeDetailGet
   * @summary 详情
   * @request GET:/api/tenantGameAppRemoteConfigMode/detail
   */
  tenantGameAppRemoteConfigModeDetailGet = (
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
    this.http.request<TenantGameAppRemoteConfigModeDetailGetData, any>({
      path: `/api/tenantGameAppRemoteConfigMode/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigMode
   * @name TenantGameAppRemoteConfigModePagePost
   * @summary 分页查询
   * @request POST:/api/tenantGameAppRemoteConfigMode/page
   */
  tenantGameAppRemoteConfigModePagePost = (
    data: PageGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigModePagePostData, any>({
      path: `/api/tenantGameAppRemoteConfigMode/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags tenantGameAppRemoteConfigMode
   * @name TenantGameAppRemoteConfigModeUpdatePost
   * @summary 更新
   * @request POST:/api/tenantGameAppRemoteConfigMode/update
   */
  tenantGameAppRemoteConfigModeUpdatePost = (
    data: UpdateGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<TenantGameAppRemoteConfigModeUpdatePostData, any>({
      path: `/api/tenantGameAppRemoteConfigMode/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const tenantGameAppRemoteConfigModeAddPost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigMode>['tenantGameAppRemoteConfigModeAddPost']>) => getAPI(TenantGameAppRemoteConfigMode).tenantGameAppRemoteConfigModeAddPost(...args);
export const tenantGameAppRemoteConfigModeDeletePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigMode>['tenantGameAppRemoteConfigModeDeletePost']>) => getAPI(TenantGameAppRemoteConfigMode).tenantGameAppRemoteConfigModeDeletePost(...args);
export const tenantGameAppRemoteConfigModeDetailGet = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigMode>['tenantGameAppRemoteConfigModeDetailGet']>) => getAPI(TenantGameAppRemoteConfigMode).tenantGameAppRemoteConfigModeDetailGet(...args);
export const tenantGameAppRemoteConfigModePagePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigMode>['tenantGameAppRemoteConfigModePagePost']>) => getAPI(TenantGameAppRemoteConfigMode).tenantGameAppRemoteConfigModePagePost(...args);
export const tenantGameAppRemoteConfigModeUpdatePost = (...args: Parameters<InstanceType<typeof TenantGameAppRemoteConfigMode>['tenantGameAppRemoteConfigModeUpdatePost']>) => getAPI(TenantGameAppRemoteConfigMode).tenantGameAppRemoteConfigModeUpdatePost(...args);
