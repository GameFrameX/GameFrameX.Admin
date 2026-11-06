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
  AddGameWebShopEntityRequest,
  GameWebShopAddPostData,
  GameWebShopDeletePostData,
  GameWebShopDetailGetData,
  GameWebShopPagePostData,
  GameWebShopSelectPostData,
  GameWebShopSelectStringPostData,
  GameWebShopUpdatePostData,
  PageGameWebShopEntityRequest,
  UpdateGameWebShopEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameWebShop<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopAddPost
   * @summary 新增
   * @request POST:/api/gameWebShop/add
   */
  gameWebShopAddPost = (
    data: AddGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopAddPostData, any>({
      path: `/api/gameWebShop/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopDeletePost
   * @summary 删除
   * @request POST:/api/gameWebShop/delete
   */
  gameWebShopDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopDeletePostData, any>({
      path: `/api/gameWebShop/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopDetailGet
   * @summary 详情
   * @request GET:/api/gameWebShop/detail
   */
  gameWebShopDetailGet = (
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
    this.http.request<GameWebShopDetailGetData, any>({
      path: `/api/gameWebShop/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopPagePost
   * @summary 分页查询
   * @request POST:/api/gameWebShop/page
   */
  gameWebShopPagePost = (
    data: PageGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopPagePostData, any>({
      path: `/api/gameWebShop/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameWebShop/select
   */
  gameWebShopSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopSelectPostData, any>({
      path: `/api/gameWebShop/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameWebShop/selectString
   */
  gameWebShopSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopSelectStringPostData, any>({
      path: `/api/gameWebShop/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameWebShop
   * @name GameWebShopUpdatePost
   * @summary 更新
   * @request POST:/api/gameWebShop/update
   */
  gameWebShopUpdatePost = (
    data: UpdateGameWebShopEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameWebShopUpdatePostData, any>({
      path: `/api/gameWebShop/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameWebShopAddPost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopAddPost']>) => getAPI(GameWebShop).gameWebShopAddPost(...args);
export const gameWebShopDeletePost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopDeletePost']>) => getAPI(GameWebShop).gameWebShopDeletePost(...args);
export const gameWebShopDetailGet = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopDetailGet']>) => getAPI(GameWebShop).gameWebShopDetailGet(...args);
export const gameWebShopPagePost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopPagePost']>) => getAPI(GameWebShop).gameWebShopPagePost(...args);
export const gameWebShopSelectPost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopSelectPost']>) => getAPI(GameWebShop).gameWebShopSelectPost(...args);
export const gameWebShopSelectStringPost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopSelectStringPost']>) => getAPI(GameWebShop).gameWebShopSelectStringPost(...args);
export const gameWebShopUpdatePost = (...args: Parameters<InstanceType<typeof GameWebShop>['gameWebShopUpdatePost']>) => getAPI(GameWebShop).gameWebShopUpdatePost(...args);
