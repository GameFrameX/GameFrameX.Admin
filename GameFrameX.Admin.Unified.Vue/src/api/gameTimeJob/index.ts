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
  AddGameTimeJobEntityRequest,
  GameTimeJobAddPostData,
  GameTimeJobDeletePostData,
  GameTimeJobDetailGetData,
  GameTimeJobPagePostData,
  GameTimeJobSelectPostData,
  GameTimeJobSelectStringPostData,
  GameTimeJobUpdatePostData,
  PageGameTimeJobEntityRequest,
  UpdateGameTimeJobEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameTimeJob<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobAddPost
   * @summary 新增
   * @request POST:/api/gameTimeJob/add
   */
  gameTimeJobAddPost = (
    data: AddGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobAddPostData, any>({
      path: `/api/gameTimeJob/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobDeletePost
   * @summary 删除
   * @request POST:/api/gameTimeJob/delete
   */
  gameTimeJobDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobDeletePostData, any>({
      path: `/api/gameTimeJob/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobDetailGet
   * @summary 详情
   * @request GET:/api/gameTimeJob/detail
   */
  gameTimeJobDetailGet = (
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
    this.http.request<GameTimeJobDetailGetData, any>({
      path: `/api/gameTimeJob/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobPagePost
   * @summary 分页查询
   * @request POST:/api/gameTimeJob/page
   */
  gameTimeJobPagePost = (
    data: PageGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobPagePostData, any>({
      path: `/api/gameTimeJob/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameTimeJob/select
   */
  gameTimeJobSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobSelectPostData, any>({
      path: `/api/gameTimeJob/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameTimeJob/selectString
   */
  gameTimeJobSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobSelectStringPostData, any>({
      path: `/api/gameTimeJob/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameTimeJob
   * @name GameTimeJobUpdatePost
   * @summary 更新
   * @request POST:/api/gameTimeJob/update
   */
  gameTimeJobUpdatePost = (
    data: UpdateGameTimeJobEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameTimeJobUpdatePostData, any>({
      path: `/api/gameTimeJob/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameTimeJobAddPost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobAddPost']>) => getAPI(GameTimeJob).gameTimeJobAddPost(...args);
export const gameTimeJobDeletePost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobDeletePost']>) => getAPI(GameTimeJob).gameTimeJobDeletePost(...args);
export const gameTimeJobDetailGet = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobDetailGet']>) => getAPI(GameTimeJob).gameTimeJobDetailGet(...args);
export const gameTimeJobPagePost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobPagePost']>) => getAPI(GameTimeJob).gameTimeJobPagePost(...args);
export const gameTimeJobSelectPost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobSelectPost']>) => getAPI(GameTimeJob).gameTimeJobSelectPost(...args);
export const gameTimeJobSelectStringPost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobSelectStringPost']>) => getAPI(GameTimeJob).gameTimeJobSelectStringPost(...args);
export const gameTimeJobUpdatePost = (...args: Parameters<InstanceType<typeof GameTimeJob>['gameTimeJobUpdatePost']>) => getAPI(GameTimeJob).gameTimeJobUpdatePost(...args);
