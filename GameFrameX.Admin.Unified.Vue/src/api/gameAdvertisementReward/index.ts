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
  AddGameAdvertisementRewardEntityRequest,
  GameAdvertisementRewardAddPostData,
  GameAdvertisementRewardDeletePostData,
  GameAdvertisementRewardDetailGetData,
  GameAdvertisementRewardExportPostData,
  GameAdvertisementRewardPagePostData,
  GameAdvertisementRewardUpdatePostData,
  PageGameAdvertisementRewardEntityRequest,
  UpdateGameAdvertisementRewardEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAdvertisementReward<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardAddPost
   * @summary 新增
   * @request POST:/api/gameAdvertisementReward/add
   */
  gameAdvertisementRewardAddPost = (
    data: AddGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAdvertisementRewardAddPostData, any>({
      path: `/api/gameAdvertisementReward/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardDeletePost
   * @summary 删除
   * @request POST:/api/gameAdvertisementReward/delete
   */
  gameAdvertisementRewardDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAdvertisementRewardDeletePostData, any>({
      path: `/api/gameAdvertisementReward/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardDetailGet
   * @summary 详情
   * @request GET:/api/gameAdvertisementReward/detail
   */
  gameAdvertisementRewardDetailGet = (
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
    this.http.request<GameAdvertisementRewardDetailGetData, any>({
      path: `/api/gameAdvertisementReward/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardExportPost
   * @summary 导出当前过滤条件下的账本列表（上限 1000 行；文件化由前端处理）。
   * @request POST:/api/gameAdvertisementReward/export
   */
  gameAdvertisementRewardExportPost = (
    data: PageGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAdvertisementRewardExportPostData, any>({
      path: `/api/gameAdvertisementReward/export`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardPagePost
   * @summary 分页查询
   * @request POST:/api/gameAdvertisementReward/page
   */
  gameAdvertisementRewardPagePost = (
    data: PageGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAdvertisementRewardPagePostData, any>({
      path: `/api/gameAdvertisementReward/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAdvertisementReward
   * @name GameAdvertisementRewardUpdatePost
   * @summary 更新
   * @request POST:/api/gameAdvertisementReward/update
   */
  gameAdvertisementRewardUpdatePost = (
    data: UpdateGameAdvertisementRewardEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAdvertisementRewardUpdatePostData, any>({
      path: `/api/gameAdvertisementReward/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAdvertisementRewardAddPost = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardAddPost']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardAddPost(...args);
export const gameAdvertisementRewardDeletePost = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardDeletePost']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardDeletePost(...args);
export const gameAdvertisementRewardDetailGet = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardDetailGet']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardDetailGet(...args);
export const gameAdvertisementRewardExportPost = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardExportPost']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardExportPost(...args);
export const gameAdvertisementRewardPagePost = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardPagePost']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardPagePost(...args);
export const gameAdvertisementRewardUpdatePost = (...args: Parameters<InstanceType<typeof GameAdvertisementReward>['gameAdvertisementRewardUpdatePost']>) => getAPI(GameAdvertisementReward).gameAdvertisementRewardUpdatePost(...args);
