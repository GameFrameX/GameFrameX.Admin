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
  AddTenantGameGoodsEntityRequest,
  GameGoodsAddPostData,
  GameGoodsDeletePostData,
  GameGoodsDetailGetData,
  GameGoodsPagePostData,
  GameGoodsSelectPostData,
  GameGoodsSelectStringPostData,
  GameGoodsUpdatePostData,
  PageTenantGameGoodsEntityRequest,
  UpdateTenantGameGoodsEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameGoods<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsAddPost
   * @summary 新增
   * @request POST:/api/gameGoods/add
   */
  gameGoodsAddPost = (
    data: AddTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsAddPostData, any>({
      path: `/api/gameGoods/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsDeletePost
   * @summary 删除
   * @request POST:/api/gameGoods/delete
   */
  gameGoodsDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameGoodsDeletePostData, any>({
      path: `/api/gameGoods/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsDetailGet
   * @summary 详情
   * @request GET:/api/gameGoods/detail
   */
  gameGoodsDetailGet = (
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
    this.http.request<GameGoodsDetailGetData, any>({
      path: `/api/gameGoods/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsPagePost
   * @summary 分页查询
   * @request POST:/api/gameGoods/page
   */
  gameGoodsPagePost = (
    data: PageTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsPagePostData, any>({
      path: `/api/gameGoods/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameGoods/select
   */
  gameGoodsSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsSelectPostData, any>({
      path: `/api/gameGoods/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsSelectStringPost
   * @summary 获取物品选择列表（含分类信息）
   * @request POST:/api/gameGoods/selectString
   */
  gameGoodsSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsSelectStringPostData, any>({
      path: `/api/gameGoods/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoods
   * @name GameGoodsUpdatePost
   * @summary 更新
   * @request POST:/api/gameGoods/update
   */
  gameGoodsUpdatePost = (
    data: UpdateTenantGameGoodsEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsUpdatePostData, any>({
      path: `/api/gameGoods/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameGoodsAddPost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsAddPost']>) => getAPI(GameGoods).gameGoodsAddPost(...args);
export const gameGoodsDeletePost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsDeletePost']>) => getAPI(GameGoods).gameGoodsDeletePost(...args);
export const gameGoodsDetailGet = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsDetailGet']>) => getAPI(GameGoods).gameGoodsDetailGet(...args);
export const gameGoodsPagePost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsPagePost']>) => getAPI(GameGoods).gameGoodsPagePost(...args);
export const gameGoodsSelectPost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsSelectPost']>) => getAPI(GameGoods).gameGoodsSelectPost(...args);
export const gameGoodsSelectStringPost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsSelectStringPost']>) => getAPI(GameGoods).gameGoodsSelectStringPost(...args);
export const gameGoodsUpdatePost = (...args: Parameters<InstanceType<typeof GameGoods>['gameGoodsUpdatePost']>) => getAPI(GameGoods).gameGoodsUpdatePost(...args);
