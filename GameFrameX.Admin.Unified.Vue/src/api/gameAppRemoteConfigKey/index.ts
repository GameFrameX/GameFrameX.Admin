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
  AddGameAppRemoteConfigKeyRequest,
  GameAppRemoteConfigKeyAddPostData,
  GameAppRemoteConfigKeyDeletePostData,
  GameAppRemoteConfigKeyDetailGetData,
  GameAppRemoteConfigKeyPagePostData,
  GameAppRemoteConfigKeyUpdatePostData,
  PageGameAppRemoteConfigKeyRequest,
  UpdateGameAppRemoteConfigKeyRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppRemoteConfigKey<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppRemoteConfigKey
   * @name GameAppRemoteConfigKeyAddPost
   * @summary 新增
   * @request POST:/api/gameAppRemoteConfigKey/add
   */
  gameAppRemoteConfigKeyAddPost = (
    data: AddGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigKeyAddPostData, any>({
      path: `/api/gameAppRemoteConfigKey/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigKey
   * @name GameAppRemoteConfigKeyDeletePost
   * @summary 删除
   * @request POST:/api/gameAppRemoteConfigKey/delete
   */
  gameAppRemoteConfigKeyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigKeyDeletePostData, any>({
      path: `/api/gameAppRemoteConfigKey/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigKey
   * @name GameAppRemoteConfigKeyDetailGet
   * @summary 详情
   * @request GET:/api/gameAppRemoteConfigKey/detail
   */
  gameAppRemoteConfigKeyDetailGet = (
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
    this.http.request<GameAppRemoteConfigKeyDetailGetData, any>({
      path: `/api/gameAppRemoteConfigKey/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigKey
   * @name GameAppRemoteConfigKeyPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppRemoteConfigKey/page
   */
  gameAppRemoteConfigKeyPagePost = (
    data: PageGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigKeyPagePostData, any>({
      path: `/api/gameAppRemoteConfigKey/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigKey
   * @name GameAppRemoteConfigKeyUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppRemoteConfigKey/update
   */
  gameAppRemoteConfigKeyUpdatePost = (
    data: UpdateGameAppRemoteConfigKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigKeyUpdatePostData, any>({
      path: `/api/gameAppRemoteConfigKey/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppRemoteConfigKeyAddPost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigKey>['gameAppRemoteConfigKeyAddPost']>) => getAPI(GameAppRemoteConfigKey).gameAppRemoteConfigKeyAddPost(...args);
export const gameAppRemoteConfigKeyDeletePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigKey>['gameAppRemoteConfigKeyDeletePost']>) => getAPI(GameAppRemoteConfigKey).gameAppRemoteConfigKeyDeletePost(...args);
export const gameAppRemoteConfigKeyDetailGet = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigKey>['gameAppRemoteConfigKeyDetailGet']>) => getAPI(GameAppRemoteConfigKey).gameAppRemoteConfigKeyDetailGet(...args);
export const gameAppRemoteConfigKeyPagePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigKey>['gameAppRemoteConfigKeyPagePost']>) => getAPI(GameAppRemoteConfigKey).gameAppRemoteConfigKeyPagePost(...args);
export const gameAppRemoteConfigKeyUpdatePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigKey>['gameAppRemoteConfigKeyUpdatePost']>) => getAPI(GameAppRemoteConfigKey).gameAppRemoteConfigKeyUpdatePost(...args);
