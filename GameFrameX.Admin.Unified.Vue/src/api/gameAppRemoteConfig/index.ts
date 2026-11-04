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
  GameAppRemoteConfigAddPostData,
  GameAppRemoteConfigDeletePostData,
  GameAppRemoteConfigDetailGetData,
  GameAppRemoteConfigDispatchOnlinePostData,
  GameAppRemoteConfigPagePostData,
  GameAppRemoteConfigUpdatePostData,
  PageGameAppRemoteConfigRequest,
  UpdateGameAppRemoteConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
  OnlineLiveOpsSourceDispatchRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppRemoteConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigAddPost
   * @summary 新增
   * @request POST:/api/gameAppRemoteConfig/add
   */
  gameAppRemoteConfigAddPost = (
    data: AddGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigAddPostData, any>({
      path: `/api/gameAppRemoteConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigDeletePost
   * @summary 删除
   * @request POST:/api/gameAppRemoteConfig/delete
   */
  gameAppRemoteConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigDeletePostData, any>({
      path: `/api/gameAppRemoteConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigDetailGet
   * @summary 详情
   * @request GET:/api/gameAppRemoteConfig/detail
   */
  gameAppRemoteConfigDetailGet = (
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
    this.http.request<GameAppRemoteConfigDetailGetData, any>({
      path: `/api/gameAppRemoteConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigDispatchOnlinePost
   * @summary 经 Online 统一下发当前远程配置（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（publishRemoteConfig 通道）， 来源类型固定注入 RemoteConfig，存在性 / 区服校验在单一入口完成。
   * @request POST:/api/gameAppRemoteConfig/dispatchOnline
   */
  gameAppRemoteConfigDispatchOnlinePost = (
    data: OnlineLiveOpsSourceDispatchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigDispatchOnlinePostData, any>({
      path: `/api/gameAppRemoteConfig/dispatchOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppRemoteConfig/page
   */
  gameAppRemoteConfigPagePost = (
    data: PageGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigPagePostData, any>({
      path: `/api/gameAppRemoteConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfig
   * @name GameAppRemoteConfigUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppRemoteConfig/update
   */
  gameAppRemoteConfigUpdatePost = (
    data: UpdateGameAppRemoteConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigUpdatePostData, any>({
      path: `/api/gameAppRemoteConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppRemoteConfigAddPost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigAddPost']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigAddPost(...args);
export const gameAppRemoteConfigDeletePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigDeletePost']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigDeletePost(...args);
export const gameAppRemoteConfigDetailGet = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigDetailGet']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigDetailGet(...args);
export const gameAppRemoteConfigDispatchOnlinePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigDispatchOnlinePost']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigDispatchOnlinePost(...args);
export const gameAppRemoteConfigPagePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigPagePost']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigPagePost(...args);
export const gameAppRemoteConfigUpdatePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfig>['gameAppRemoteConfigUpdatePost']>) => getAPI(GameAppRemoteConfig).gameAppRemoteConfigUpdatePost(...args);
