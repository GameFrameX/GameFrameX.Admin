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
  AddGameAreaEntityRequest,
  GameAreaAddPostData,
  GameAreaDeletePostData,
  GameAreaDetailGetData,
  GameAreaDisableDeletePostData,
  GameAreaOpenPostData,
  GameAreaPagePostData,
  GameAreaRestartPostData,
  GameAreaSelectPostData,
  GameAreaSelectStringPostData,
  GameAreaStopPostData,
  GameAreaUpdatePostData,
  PageGameAreaEntityRequest,
  UpdateGameAreaEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameArea<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaAddPost
   * @summary 新增
   * @request POST:/api/gameArea/add
   */
  gameAreaAddPost = (
    data: AddGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaAddPostData, any>({
      path: `/api/gameArea/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaDeletePost
   * @summary 删除
   * @request POST:/api/gameArea/delete
   */
  gameAreaDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaDeletePostData, any>({
      path: `/api/gameArea/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaDetailGet
   * @summary 详情：绕过全局过滤器读取（禁用区服 IsEnabled=false 会被 ISafeEnabledFilter 隐藏，详情页必须可见）。
   * @request GET:/api/gameArea/detail
   */
  gameAreaDetailGet = (
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
    this.http.request<GameAreaDetailGetData, any>({
      path: `/api/gameArea/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaDisableDeletePost
   * @summary 删除游戏区服
   * @request POST:/api/gameArea/disableDelete
   */
  gameAreaDisableDeletePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaDisableDeletePostData, any>({
      path: `/api/gameArea/disableDelete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaOpenPost
   * @summary 游戏区服开服
   * @request POST:/api/gameArea/open
   */
  gameAreaOpenPost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaOpenPostData, any>({
      path: `/api/gameArea/open`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaPagePost
   * @summary 分页查询
   * @request POST:/api/gameArea/page
   */
  gameAreaPagePost = (
    data: PageGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaPagePostData, any>({
      path: `/api/gameArea/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaRestartPost
   * @summary 重启游戏区服
   * @request POST:/api/gameArea/restart
   */
  gameAreaRestartPost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaRestartPostData, any>({
      path: `/api/gameArea/restart`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameArea/select
   */
  gameAreaSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaSelectPostData, any>({
      path: `/api/gameArea/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameArea/selectString
   */
  gameAreaSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaSelectStringPostData, any>({
      path: `/api/gameArea/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaStopPost
   * @summary 停止游戏区服
   * @request POST:/api/gameArea/stop
   */
  gameAreaStopPost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaStopPostData, any>({
      path: `/api/gameArea/stop`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameArea
   * @name GameAreaUpdatePost
   * @summary 更新
   * @request POST:/api/gameArea/update
   */
  gameAreaUpdatePost = (
    data: UpdateGameAreaEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaUpdatePostData, any>({
      path: `/api/gameArea/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAreaAddPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaAddPost']>) => getAPI(GameArea).gameAreaAddPost(...args);
export const gameAreaDeletePost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaDeletePost']>) => getAPI(GameArea).gameAreaDeletePost(...args);
export const gameAreaDetailGet = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaDetailGet']>) => getAPI(GameArea).gameAreaDetailGet(...args);
export const gameAreaDisableDeletePost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaDisableDeletePost']>) => getAPI(GameArea).gameAreaDisableDeletePost(...args);
export const gameAreaOpenPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaOpenPost']>) => getAPI(GameArea).gameAreaOpenPost(...args);
export const gameAreaPagePost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaPagePost']>) => getAPI(GameArea).gameAreaPagePost(...args);
export const gameAreaRestartPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaRestartPost']>) => getAPI(GameArea).gameAreaRestartPost(...args);
export const gameAreaSelectPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaSelectPost']>) => getAPI(GameArea).gameAreaSelectPost(...args);
export const gameAreaSelectStringPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaSelectStringPost']>) => getAPI(GameArea).gameAreaSelectStringPost(...args);
export const gameAreaStopPost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaStopPost']>) => getAPI(GameArea).gameAreaStopPost(...args);
export const gameAreaUpdatePost = (...args: Parameters<InstanceType<typeof GameArea>['gameAreaUpdatePost']>) => getAPI(GameArea).gameAreaUpdatePost(...args);
