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
  AddGameRedeemCodeChannelEntityRequest,
  GameRedeemCodeChannelAddPostData,
  GameRedeemCodeChannelDeletePostData,
  GameRedeemCodeChannelDetailGetData,
  GameRedeemCodeChannelPagePostData,
  GameRedeemCodeChannelSelectPostData,
  GameRedeemCodeChannelSelectStringPostData,
  GameRedeemCodeChannelUpdatePostData,
  PageGameRedeemCodeChannelEntityRequest,
  UpdateGameRedeemCodeChannelEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRedeemCodeChannel<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelAddPost
   * @summary 新增
   * @request POST:/api/gameRedeemCodeChannel/add
   */
  gameRedeemCodeChannelAddPost = (
    data: AddGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelAddPostData, any>({
      path: `/api/gameRedeemCodeChannel/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelDeletePost
   * @summary 删除
   * @request POST:/api/gameRedeemCodeChannel/delete
   */
  gameRedeemCodeChannelDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelDeletePostData, any>({
      path: `/api/gameRedeemCodeChannel/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelDetailGet
   * @summary 详情
   * @request GET:/api/gameRedeemCodeChannel/detail
   */
  gameRedeemCodeChannelDetailGet = (
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
    this.http.request<GameRedeemCodeChannelDetailGetData, any>({
      path: `/api/gameRedeemCodeChannel/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelPagePost
   * @summary 分页查询
   * @request POST:/api/gameRedeemCodeChannel/page
   */
  gameRedeemCodeChannelPagePost = (
    data: PageGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelPagePostData, any>({
      path: `/api/gameRedeemCodeChannel/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameRedeemCodeChannel/select
   */
  gameRedeemCodeChannelSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelSelectPostData, any>({
      path: `/api/gameRedeemCodeChannel/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameRedeemCodeChannel/selectString
   */
  gameRedeemCodeChannelSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelSelectStringPostData, any>({
      path: `/api/gameRedeemCodeChannel/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCodeChannel
   * @name GameRedeemCodeChannelUpdatePost
   * @summary 更新
   * @request POST:/api/gameRedeemCodeChannel/update
   */
  gameRedeemCodeChannelUpdatePost = (
    data: UpdateGameRedeemCodeChannelEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeChannelUpdatePostData, any>({
      path: `/api/gameRedeemCodeChannel/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRedeemCodeChannelAddPost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelAddPost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelAddPost(...args);
export const gameRedeemCodeChannelDeletePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelDeletePost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelDeletePost(...args);
export const gameRedeemCodeChannelDetailGet = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelDetailGet']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelDetailGet(...args);
export const gameRedeemCodeChannelPagePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelPagePost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelPagePost(...args);
export const gameRedeemCodeChannelSelectPost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelSelectPost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelSelectPost(...args);
export const gameRedeemCodeChannelSelectStringPost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelSelectStringPost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelSelectStringPost(...args);
export const gameRedeemCodeChannelUpdatePost = (...args: Parameters<InstanceType<typeof GameRedeemCodeChannel>['gameRedeemCodeChannelUpdatePost']>) => getAPI(GameRedeemCodeChannel).gameRedeemCodeChannelUpdatePost(...args);
