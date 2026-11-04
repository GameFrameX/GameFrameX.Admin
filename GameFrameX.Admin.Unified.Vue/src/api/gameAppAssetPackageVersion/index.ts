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
  AddGameAppAssetPackageVersionRequest,
  GameAppAssetPackageVersionAddPostData,
  GameAppAssetPackageVersionDeletePostData,
  GameAppAssetPackageVersionDetailGetData,
  GameAppAssetPackageVersionPagePostData,
  GameAppAssetPackageVersionReturnByIdRequest,
  GameAppAssetPackageVersionReturnPostData,
  GameAppAssetPackageVersionSelectPostData,
  GameAppAssetPackageVersionUpdatePostData,
  PageGameAppAssetPackageVersionRequest,
  PageGameAppAssetPackageVersionSelectRequest,
  UpdateGameAppAssetPackageVersionRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppAssetPackageVersion<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionAddPost
   * @summary 新增
   * @request POST:/api/gameAppAssetPackageVersion/add
   */
  gameAppAssetPackageVersionAddPost = (
    data: AddGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionAddPostData, any>({
      path: `/api/gameAppAssetPackageVersion/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionDeletePost
   * @summary 删除
   * @request POST:/api/gameAppAssetPackageVersion/delete
   */
  gameAppAssetPackageVersionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionDeletePostData, any>({
      path: `/api/gameAppAssetPackageVersion/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionDetailGet
   * @summary 详情
   * @request GET:/api/gameAppAssetPackageVersion/detail
   */
  gameAppAssetPackageVersionDetailGet = (
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
    this.http.request<GameAppAssetPackageVersionDetailGetData, any>({
      path: `/api/gameAppAssetPackageVersion/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppAssetPackageVersion/page
   */
  gameAppAssetPackageVersionPagePost = (
    data: PageGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionPagePostData, any>({
      path: `/api/gameAppAssetPackageVersion/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionReturnPost
   * @summary 回退资源包版本
   * @request POST:/api/gameAppAssetPackageVersion/return
   */
  gameAppAssetPackageVersionReturnPost = (
    data: GameAppAssetPackageVersionReturnByIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionReturnPostData, any>({
      path: `/api/gameAppAssetPackageVersion/return`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionSelectPost
   * @summary 获取当前分包下可选的资源版本列表。
   * @request POST:/api/gameAppAssetPackageVersion/select
   */
  gameAppAssetPackageVersionSelectPost = (
    data: PageGameAppAssetPackageVersionSelectRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionSelectPostData, any>({
      path: `/api/gameAppAssetPackageVersion/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackageVersion
   * @name GameAppAssetPackageVersionUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppAssetPackageVersion/update
   */
  gameAppAssetPackageVersionUpdatePost = (
    data: UpdateGameAppAssetPackageVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageVersionUpdatePostData, any>({
      path: `/api/gameAppAssetPackageVersion/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppAssetPackageVersionAddPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionAddPost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionAddPost(...args);
export const gameAppAssetPackageVersionDeletePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionDeletePost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionDeletePost(...args);
export const gameAppAssetPackageVersionDetailGet = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionDetailGet']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionDetailGet(...args);
export const gameAppAssetPackageVersionPagePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionPagePost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionPagePost(...args);
export const gameAppAssetPackageVersionReturnPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionReturnPost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionReturnPost(...args);
export const gameAppAssetPackageVersionSelectPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionSelectPost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionSelectPost(...args);
export const gameAppAssetPackageVersionUpdatePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackageVersion>['gameAppAssetPackageVersionUpdatePost']>) => getAPI(GameAppAssetPackageVersion).gameAppAssetPackageVersionUpdatePost(...args);
