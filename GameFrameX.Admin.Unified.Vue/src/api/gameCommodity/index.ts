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
  AddGameCommodityEntityRequest,
  GameCommodityAddPostData,
  GameCommodityDeletePostData,
  GameCommodityDetailGetData,
  GameCommodityPagePostData,
  GameCommodityUpdatePostData,
  PageGameCommodityEntityRequest,
  UpdateGameCommodityEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameCommodity<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameCommodity
   * @name GameCommodityAddPost
   * @summary 新增
   * @request POST:/api/gameCommodity/add
   */
  gameCommodityAddPost = (
    data: AddGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameCommodityAddPostData, any>({
      path: `/api/gameCommodity/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameCommodity
   * @name GameCommodityDeletePost
   * @summary 删除
   * @request POST:/api/gameCommodity/delete
   */
  gameCommodityDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameCommodityDeletePostData, any>({
      path: `/api/gameCommodity/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameCommodity
   * @name GameCommodityDetailGet
   * @summary 详情
   * @request GET:/api/gameCommodity/detail
   */
  gameCommodityDetailGet = (
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
    this.http.request<GameCommodityDetailGetData, any>({
      path: `/api/gameCommodity/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameCommodity
   * @name GameCommodityPagePost
   * @summary 分页查询
   * @request POST:/api/gameCommodity/page
   */
  gameCommodityPagePost = (
    data: PageGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameCommodityPagePostData, any>({
      path: `/api/gameCommodity/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameCommodity
   * @name GameCommodityUpdatePost
   * @summary 更新
   * @request POST:/api/gameCommodity/update
   */
  gameCommodityUpdatePost = (
    data: UpdateGameCommodityEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameCommodityUpdatePostData, any>({
      path: `/api/gameCommodity/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameCommodityAddPost = (...args: Parameters<InstanceType<typeof GameCommodity>['gameCommodityAddPost']>) => getAPI(GameCommodity).gameCommodityAddPost(...args);
export const gameCommodityDeletePost = (...args: Parameters<InstanceType<typeof GameCommodity>['gameCommodityDeletePost']>) => getAPI(GameCommodity).gameCommodityDeletePost(...args);
export const gameCommodityDetailGet = (...args: Parameters<InstanceType<typeof GameCommodity>['gameCommodityDetailGet']>) => getAPI(GameCommodity).gameCommodityDetailGet(...args);
export const gameCommodityPagePost = (...args: Parameters<InstanceType<typeof GameCommodity>['gameCommodityPagePost']>) => getAPI(GameCommodity).gameCommodityPagePost(...args);
export const gameCommodityUpdatePost = (...args: Parameters<InstanceType<typeof GameCommodity>['gameCommodityUpdatePost']>) => getAPI(GameCommodity).gameCommodityUpdatePost(...args);
