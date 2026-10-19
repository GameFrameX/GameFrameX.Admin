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
  AddRegionRequest,
  DeleteRegionRequest,
  PageRegionRequest,
  SystemRegionAddPostData,
  SystemRegionDeletePostData,
  SystemRegionListGetData,
  SystemRegionPagePostData,
  SystemRegionSyncPostData,
  SystemRegionUpdatePostData,
  UpdateRegionRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class SystemRegion<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionAddPost
   * @summary 增加行政区域
   * @request POST:/api/systemRegion/add
   */
  systemRegionAddPost = (data: AddRegionRequest, params: RequestParams = {}) =>
    this.http.request<SystemRegionAddPostData, any>({
      path: `/api/systemRegion/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionDeletePost
   * @summary 删除行政区域
   * @request POST:/api/systemRegion/delete
   */
  systemRegionDeletePost = (
    data: DeleteRegionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemRegionDeletePostData, any>({
      path: `/api/systemRegion/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionListGet
   * @summary 获取行政区域列表
   * @request GET:/api/systemRegion/list
   */
  systemRegionListGet = (
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
    this.http.request<SystemRegionListGetData, any>({
      path: `/api/systemRegion/list`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionPagePost
   * @summary 获取行政区域分页列表
   * @request POST:/api/systemRegion/page
   */
  systemRegionPagePost = (
    data: PageRegionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemRegionPagePostData, any>({
      path: `/api/systemRegion/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionSyncPost
   * @summary 同步行政区域
   * @request POST:/api/systemRegion/sync
   */
  systemRegionSyncPost = (params: RequestParams = {}) =>
    this.http.request<SystemRegionSyncPostData, any>({
      path: `/api/systemRegion/sync`,
      method: "POST",
      ...params,
    });
  /**
   * No description
   *
   * @tags systemRegion
   * @name SystemRegionUpdatePost
   * @summary 更新行政区域
   * @request POST:/api/systemRegion/update
   */
  systemRegionUpdatePost = (
    data: UpdateRegionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<SystemRegionUpdatePostData, any>({
      path: `/api/systemRegion/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const systemRegionAddPost = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionAddPost']>) => getAPI(SystemRegion).systemRegionAddPost(...args);
export const systemRegionDeletePost = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionDeletePost']>) => getAPI(SystemRegion).systemRegionDeletePost(...args);
export const systemRegionListGet = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionListGet']>) => getAPI(SystemRegion).systemRegionListGet(...args);
export const systemRegionPagePost = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionPagePost']>) => getAPI(SystemRegion).systemRegionPagePost(...args);
export const systemRegionSyncPost = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionSyncPost']>) => getAPI(SystemRegion).systemRegionSyncPost(...args);
export const systemRegionUpdatePost = (...args: Parameters<InstanceType<typeof SystemRegion>['systemRegionUpdatePost']>) => getAPI(SystemRegion).systemRegionUpdatePost(...args);
