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
  AddTenantGameAppDeviceRequest,
  GameAppDeviceAddPostData,
  GameAppDeviceDeletePostData,
  GameAppDeviceDetailGetData,
  GameAppDevicePagePostData,
  GameAppDeviceRequest,
  GameAppDeviceSelectPostData,
  GameAppDeviceSelectStringPostData,
  GameAppDeviceUpdatePostData,
  UpdateTenantGameAppDeviceRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppDevice<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceAddPost
   * @summary 新增
   * @request POST:/api/gameAppDevice/add
   */
  gameAppDeviceAddPost = (
    data: AddTenantGameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDeviceAddPostData, any>({
      path: `/api/gameAppDevice/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceDeletePost
   * @summary 删除
   * @request POST:/api/gameAppDevice/delete
   */
  gameAppDeviceDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDeviceDeletePostData, any>({
      path: `/api/gameAppDevice/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceDetailGet
   * @summary 详情
   * @request GET:/api/gameAppDevice/detail
   */
  gameAppDeviceDetailGet = (
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
    this.http.request<GameAppDeviceDetailGetData, any>({
      path: `/api/gameAppDevice/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDevicePagePost
   * @summary 分页查询
   * @request POST:/api/gameAppDevice/page
   */
  gameAppDevicePagePost = (
    data: GameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDevicePagePostData, any>({
      path: `/api/gameAppDevice/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceSelectPost
   * @summary 获取主库选择器列表。
   * @request POST:/api/gameAppDevice/select
   */
  gameAppDeviceSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDeviceSelectPostData, any>({
      path: `/api/gameAppDevice/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameAppDevice/selectString
   */
  gameAppDeviceSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDeviceSelectStringPostData, any>({
      path: `/api/gameAppDevice/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppDevice
   * @name GameAppDeviceUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppDevice/update
   */
  gameAppDeviceUpdatePost = (
    data: UpdateTenantGameAppDeviceRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppDeviceUpdatePostData, any>({
      path: `/api/gameAppDevice/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppDeviceAddPost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceAddPost']>) => getAPI(GameAppDevice).gameAppDeviceAddPost(...args);
export const gameAppDeviceDeletePost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceDeletePost']>) => getAPI(GameAppDevice).gameAppDeviceDeletePost(...args);
export const gameAppDeviceDetailGet = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceDetailGet']>) => getAPI(GameAppDevice).gameAppDeviceDetailGet(...args);
export const gameAppDevicePagePost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDevicePagePost']>) => getAPI(GameAppDevice).gameAppDevicePagePost(...args);
export const gameAppDeviceSelectPost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceSelectPost']>) => getAPI(GameAppDevice).gameAppDeviceSelectPost(...args);
export const gameAppDeviceSelectStringPost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceSelectStringPost']>) => getAPI(GameAppDevice).gameAppDeviceSelectStringPost(...args);
export const gameAppDeviceUpdatePost = (...args: Parameters<InstanceType<typeof GameAppDevice>['gameAppDeviceUpdatePost']>) => getAPI(GameAppDevice).gameAppDeviceUpdatePost(...args);
