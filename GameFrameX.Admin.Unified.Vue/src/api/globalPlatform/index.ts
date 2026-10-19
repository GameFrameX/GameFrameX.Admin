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
  AddGlobalPlatformRequest,
  GlobalPlatformAddPostData,
  GlobalPlatformDeletePostData,
  GlobalPlatformDetailGetData,
  GlobalPlatformPagePostData,
  GlobalPlatformSelectPostData,
  GlobalPlatformSelectStringPostData,
  GlobalPlatformUpdatePostData,
  PageGlobalPlatformRequest,
  UpdateGlobalPlatformRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GlobalPlatform<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformAddPost
   * @summary 新增
   * @request POST:/api/globalPlatform/add
   */
  globalPlatformAddPost = (
    data: AddGlobalPlatformRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformAddPostData, any>({
      path: `/api/globalPlatform/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformDeletePost
   * @summary 删除
   * @request POST:/api/globalPlatform/delete
   */
  globalPlatformDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformDeletePostData, any>({
      path: `/api/globalPlatform/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformDetailGet
   * @summary 详情
   * @request GET:/api/globalPlatform/detail
   */
  globalPlatformDetailGet = (
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
    this.http.request<GlobalPlatformDetailGetData, any>({
      path: `/api/globalPlatform/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformPagePost
   * @summary 分页查询
   * @request POST:/api/globalPlatform/page
   */
  globalPlatformPagePost = (
    data: PageGlobalPlatformRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformPagePostData, any>({
      path: `/api/globalPlatform/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/globalPlatform/select
   */
  globalPlatformSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformSelectPostData, any>({
      path: `/api/globalPlatform/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/globalPlatform/selectString
   */
  globalPlatformSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformSelectStringPostData, any>({
      path: `/api/globalPlatform/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags globalPlatform
   * @name GlobalPlatformUpdatePost
   * @summary 更新
   * @request POST:/api/globalPlatform/update
   */
  globalPlatformUpdatePost = (
    data: UpdateGlobalPlatformRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GlobalPlatformUpdatePostData, any>({
      path: `/api/globalPlatform/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const globalPlatformAddPost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformAddPost']>) => getAPI(GlobalPlatform).globalPlatformAddPost(...args);
export const globalPlatformDeletePost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformDeletePost']>) => getAPI(GlobalPlatform).globalPlatformDeletePost(...args);
export const globalPlatformDetailGet = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformDetailGet']>) => getAPI(GlobalPlatform).globalPlatformDetailGet(...args);
export const globalPlatformPagePost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformPagePost']>) => getAPI(GlobalPlatform).globalPlatformPagePost(...args);
export const globalPlatformSelectPost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformSelectPost']>) => getAPI(GlobalPlatform).globalPlatformSelectPost(...args);
export const globalPlatformSelectStringPost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformSelectStringPost']>) => getAPI(GlobalPlatform).globalPlatformSelectStringPost(...args);
export const globalPlatformUpdatePost = (...args: Parameters<InstanceType<typeof GlobalPlatform>['globalPlatformUpdatePost']>) => getAPI(GlobalPlatform).globalPlatformUpdatePost(...args);
