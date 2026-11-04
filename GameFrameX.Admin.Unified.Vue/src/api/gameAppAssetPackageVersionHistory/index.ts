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
  AddGameAppAssetPackageVersionHistoryRequest,
  GameAppAssetPackageVersionHistoryAddPostData,
  GameAppAssetPackageVersionHistoryDeletePostData,
  GameAppAssetPackageVersionHistoryDetailGetData,
  GameAppAssetPackageVersionHistoryPagePostData,
  GameAppAssetPackageVersionHistoryUpdatePostData,
  PageGameAppAssetPackageVersionHistoryRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseUpdateRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppAssetPackageVersionHistory<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppAssetPackageVersionHistory
   * @name GameAppAssetPackageVersionHistoryAddPost
   * @summary 新增
   * @request POST:/api/gameAppAssetPackageVersionHistory/add
   */
  gameAppAssetPackageVersionHistoryAddPost = (
    data: AddGameAppAssetPackageVersionHistoryRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionHistoryAddPostData, any>({
      path: `/api/gameAppAssetPackageVersionHistory/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersionHistory
   * @name GameAppAssetPackageVersionHistoryDeletePost
   * @summary 删除
   * @request POST:/api/gameAppAssetPackageVersionHistory/delete
   */
  gameAppAssetPackageVersionHistoryDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionHistoryDeletePostData, any>({
      path: `/api/gameAppAssetPackageVersionHistory/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersionHistory
   * @name GameAppAssetPackageVersionHistoryDetailGet
   * @summary 详情
   * @request GET:/api/gameAppAssetPackageVersionHistory/detail
   */
  gameAppAssetPackageVersionHistoryDetailGet = (
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
    this.http.request<GameAppAssetPackageVersionHistoryDetailGetData, any>({
      path: `/api/gameAppAssetPackageVersionHistory/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersionHistory
   * @name GameAppAssetPackageVersionHistoryPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppAssetPackageVersionHistory/page
   */
  gameAppAssetPackageVersionHistoryPagePost = (
    data: PageGameAppAssetPackageVersionHistoryRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionHistoryPagePostData, any>({
      path: `/api/gameAppAssetPackageVersionHistory/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersionHistory
   * @name GameAppAssetPackageVersionHistoryUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppAssetPackageVersionHistory/update
   */
  gameAppAssetPackageVersionHistoryUpdatePost = (
    data: BaseUpdateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionHistoryUpdatePostData, any>({
      path: `/api/gameAppAssetPackageVersionHistory/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppAssetPackageVersionHistoryAddPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersionHistory>['gameAppAssetPackageVersionHistoryAddPost']>) => getAPI(GameAppAssetPackageVersionHistory).gameAppAssetPackageVersionHistoryAddPost(...args);
export const gameAppAssetPackageVersionHistoryDeletePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersionHistory>['gameAppAssetPackageVersionHistoryDeletePost']>) => getAPI(GameAppAssetPackageVersionHistory).gameAppAssetPackageVersionHistoryDeletePost(...args);
export const gameAppAssetPackageVersionHistoryDetailGet = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersionHistory>['gameAppAssetPackageVersionHistoryDetailGet']>) => getAPI(GameAppAssetPackageVersionHistory).gameAppAssetPackageVersionHistoryDetailGet(...args);
export const gameAppAssetPackageVersionHistoryPagePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersionHistory>['gameAppAssetPackageVersionHistoryPagePost']>) => getAPI(GameAppAssetPackageVersionHistory).gameAppAssetPackageVersionHistoryPagePost(...args);
export const gameAppAssetPackageVersionHistoryUpdatePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersionHistory>['gameAppAssetPackageVersionHistoryUpdatePost']>) => getAPI(GameAppAssetPackageVersionHistory).gameAppAssetPackageVersionHistoryUpdatePost(...args);
