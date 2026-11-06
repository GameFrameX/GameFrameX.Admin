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
  AddGameAreaClassifyEntityRequest,
  GameAreaClassifyAddPostData,
  GameAreaClassifyDeletePostData,
  GameAreaClassifyDetailGetData,
  GameAreaClassifyPagePostData,
  GameAreaClassifySelectPostData,
  GameAreaClassifySelectStringPostData,
  GameAreaClassifyUpdatePostData,
  PageGameAreaClassifyEntityRequest,
  UpdateGameAreaClassifyEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAreaClassify<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifyAddPost
   * @summary 新增
   * @request POST:/api/gameAreaClassify/add
   */
  gameAreaClassifyAddPost = (
    data: AddGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifyAddPostData, any>({
      path: `/api/gameAreaClassify/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifyDeletePost
   * @summary 删除
   * @request POST:/api/gameAreaClassify/delete
   */
  gameAreaClassifyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifyDeletePostData, any>({
      path: `/api/gameAreaClassify/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifyDetailGet
   * @summary 详情
   * @request GET:/api/gameAreaClassify/detail
   */
  gameAreaClassifyDetailGet = (
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
    this.http.request<GameAreaClassifyDetailGetData, any>({
      path: `/api/gameAreaClassify/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifyPagePost
   * @summary 分页查询
   * @request POST:/api/gameAreaClassify/page
   */
  gameAreaClassifyPagePost = (
    data: PageGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifyPagePostData, any>({
      path: `/api/gameAreaClassify/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameAreaClassify/select
   */
  gameAreaClassifySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifySelectPostData, any>({
      path: `/api/gameAreaClassify/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameAreaClassify/selectString
   */
  gameAreaClassifySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifySelectStringPostData, any>({
      path: `/api/gameAreaClassify/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaClassify
   * @name GameAreaClassifyUpdatePost
   * @summary 更新
   * @request POST:/api/gameAreaClassify/update
   */
  gameAreaClassifyUpdatePost = (
    data: UpdateGameAreaClassifyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaClassifyUpdatePostData, any>({
      path: `/api/gameAreaClassify/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAreaClassifyAddPost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifyAddPost']>) => getAPI(GameAreaClassify).gameAreaClassifyAddPost(...args);
export const gameAreaClassifyDeletePost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifyDeletePost']>) => getAPI(GameAreaClassify).gameAreaClassifyDeletePost(...args);
export const gameAreaClassifyDetailGet = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifyDetailGet']>) => getAPI(GameAreaClassify).gameAreaClassifyDetailGet(...args);
export const gameAreaClassifyPagePost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifyPagePost']>) => getAPI(GameAreaClassify).gameAreaClassifyPagePost(...args);
export const gameAreaClassifySelectPost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifySelectPost']>) => getAPI(GameAreaClassify).gameAreaClassifySelectPost(...args);
export const gameAreaClassifySelectStringPost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifySelectStringPost']>) => getAPI(GameAreaClassify).gameAreaClassifySelectStringPost(...args);
export const gameAreaClassifyUpdatePost = (...args: Parameters<InstanceType<typeof GameAreaClassify>['gameAreaClassifyUpdatePost']>) => getAPI(GameAreaClassify).gameAreaClassifyUpdatePost(...args);
