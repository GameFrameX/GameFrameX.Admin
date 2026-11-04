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
  AddGameAppServerConfigRequest,
  GameAppServerConfigAddPostData,
  GameAppServerConfigDeletePostData,
  GameAppServerConfigDetailGetData,
  GameAppServerConfigPagePostData,
  GameAppServerConfigUpdatePostData,
  PageGameAppServerConfigRequest,
  UpdateGameAppServerConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppServerConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppServerConfig
   * @name GameAppServerConfigAddPost
   * @summary 新增
   * @request POST:/api/gameAppServerConfig/add
   */
  gameAppServerConfigAddPost = (
    data: AddGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppServerConfigAddPostData, any>({
      path: `/api/gameAppServerConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppServerConfig
   * @name GameAppServerConfigDeletePost
   * @summary 删除
   * @request POST:/api/gameAppServerConfig/delete
   */
  gameAppServerConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppServerConfigDeletePostData, any>({
      path: `/api/gameAppServerConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppServerConfig
   * @name GameAppServerConfigDetailGet
   * @summary 详情
   * @request GET:/api/gameAppServerConfig/detail
   */
  gameAppServerConfigDetailGet = (
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
    this.http.request<GameAppServerConfigDetailGetData, any>({
      path: `/api/gameAppServerConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppServerConfig
   * @name GameAppServerConfigPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppServerConfig/page
   */
  gameAppServerConfigPagePost = (
    data: PageGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppServerConfigPagePostData, any>({
      path: `/api/gameAppServerConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppServerConfig
   * @name GameAppServerConfigUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppServerConfig/update
   */
  gameAppServerConfigUpdatePost = (
    data: UpdateGameAppServerConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppServerConfigUpdatePostData, any>({
      path: `/api/gameAppServerConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppServerConfigAddPost = (...args: Parameters<InstanceType<typeof GameAppServerConfig>['gameAppServerConfigAddPost']>) => getAPI(GameAppServerConfig).gameAppServerConfigAddPost(...args);
export const gameAppServerConfigDeletePost = (...args: Parameters<InstanceType<typeof GameAppServerConfig>['gameAppServerConfigDeletePost']>) => getAPI(GameAppServerConfig).gameAppServerConfigDeletePost(...args);
export const gameAppServerConfigDetailGet = (...args: Parameters<InstanceType<typeof GameAppServerConfig>['gameAppServerConfigDetailGet']>) => getAPI(GameAppServerConfig).gameAppServerConfigDetailGet(...args);
export const gameAppServerConfigPagePost = (...args: Parameters<InstanceType<typeof GameAppServerConfig>['gameAppServerConfigPagePost']>) => getAPI(GameAppServerConfig).gameAppServerConfigPagePost(...args);
export const gameAppServerConfigUpdatePost = (...args: Parameters<InstanceType<typeof GameAppServerConfig>['gameAppServerConfigUpdatePost']>) => getAPI(GameAppServerConfig).gameAppServerConfigUpdatePost(...args);
