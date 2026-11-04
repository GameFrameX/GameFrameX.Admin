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
  AddTenantGameGoodsCatalogueEntityRequest,
  GameGoodsCatalogueAddPostData,
  GameGoodsCatalogueDeletePostData,
  GameGoodsCatalogueDetailGetData,
  GameGoodsCataloguePagePostData,
  GameGoodsCatalogueSelectPostData,
  GameGoodsCatalogueSelectStringPostData,
  GameGoodsCatalogueUpdatePostData,
  PageTenantGameGoodsCatalogueEntityRequest,
  UpdateTenantGameGoodsCatalogueEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameGoodsCatalogue<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueAddPost
   * @summary 新增
   * @request POST:/api/gameGoodsCatalogue/add
   */
  gameGoodsCatalogueAddPost = (
    data: AddTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCatalogueAddPostData, any>({
      path: `/api/gameGoodsCatalogue/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueDeletePost
   * @summary 删除
   * @request POST:/api/gameGoodsCatalogue/delete
   */
  gameGoodsCatalogueDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCatalogueDeletePostData, any>({
      path: `/api/gameGoodsCatalogue/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueDetailGet
   * @summary 详情
   * @request GET:/api/gameGoodsCatalogue/detail
   */
  gameGoodsCatalogueDetailGet = (
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
    this.http.request<GameGoodsCatalogueDetailGetData, any>({
      path: `/api/gameGoodsCatalogue/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCataloguePagePost
   * @summary 分页查询
   * @request POST:/api/gameGoodsCatalogue/page
   */
  gameGoodsCataloguePagePost = (
    data: PageTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCataloguePagePostData, any>({
      path: `/api/gameGoodsCatalogue/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameGoodsCatalogue/select
   */
  gameGoodsCatalogueSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCatalogueSelectPostData, any>({
      path: `/api/gameGoodsCatalogue/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameGoodsCatalogue/selectString
   */
  gameGoodsCatalogueSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCatalogueSelectStringPostData, any>({
      path: `/api/gameGoodsCatalogue/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameGoodsCatalogue
   * @name GameGoodsCatalogueUpdatePost
   * @summary 更新
   * @request POST:/api/gameGoodsCatalogue/update
   */
  gameGoodsCatalogueUpdatePost = (
    data: UpdateTenantGameGoodsCatalogueEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameGoodsCatalogueUpdatePostData, any>({
      path: `/api/gameGoodsCatalogue/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameGoodsCatalogueAddPost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueAddPost']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueAddPost(...args);
export const gameGoodsCatalogueDeletePost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueDeletePost']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueDeletePost(...args);
export const gameGoodsCatalogueDetailGet = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueDetailGet']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueDetailGet(...args);
export const gameGoodsCataloguePagePost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCataloguePagePost']>) => getAPI(GameGoodsCatalogue).gameGoodsCataloguePagePost(...args);
export const gameGoodsCatalogueSelectPost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueSelectPost']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueSelectPost(...args);
export const gameGoodsCatalogueSelectStringPost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueSelectStringPost']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueSelectStringPost(...args);
export const gameGoodsCatalogueUpdatePost = (...args: Parameters<InstanceType<typeof GameGoodsCatalogue>['gameGoodsCatalogueUpdatePost']>) => getAPI(GameGoodsCatalogue).gameGoodsCatalogueUpdatePost(...args);
