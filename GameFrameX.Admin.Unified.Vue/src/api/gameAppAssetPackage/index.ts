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
  AddGameAppAssetPackageRequest,
  GameAppAssetPackageAddPostData,
  GameAppAssetPackageDeletePostData,
  GameAppAssetPackageDetailGetData,
  GameAppAssetPackagePagePostData,
  GameAppAssetPackageSelectPostData,
  GameAppAssetPackageSelectStringPostData,
  GameAppAssetPackageUpdatePostData,
  PageGameAppAssetPackageRequest,
  UpdateGameAppAssetPackageRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppAssetPackage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageAddPost
   * @summary 新增
   * @request POST:/api/gameAppAssetPackage/add
   */
  gameAppAssetPackageAddPost = (
    data: AddGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageAddPostData, any>({
      path: `/api/gameAppAssetPackage/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageDeletePost
   * @summary 删除
   * @request POST:/api/gameAppAssetPackage/delete
   */
  gameAppAssetPackageDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageDeletePostData, any>({
      path: `/api/gameAppAssetPackage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageDetailGet
   * @summary 详情
   * @request GET:/api/gameAppAssetPackage/detail
   */
  gameAppAssetPackageDetailGet = (
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
    this.http.request<GameAppAssetPackageDetailGetData, any>({
      path: `/api/gameAppAssetPackage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackagePagePost
   * @summary 分页查询
   * @request POST:/api/gameAppAssetPackage/page
   */
  gameAppAssetPackagePagePost = (
    data: PageGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackagePagePostData, any>({
      path: `/api/gameAppAssetPackage/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageSelectPost
   * @summary 获取主库中的资源包选择器列表。
   * @request POST:/api/gameAppAssetPackage/select
   */
  gameAppAssetPackageSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageSelectPostData, any>({
      path: `/api/gameAppAssetPackage/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameAppAssetPackage/selectString
   */
  gameAppAssetPackageSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageSelectStringPostData, any>({
      path: `/api/gameAppAssetPackage/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppAssetPackage
   * @name GameAppAssetPackageUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppAssetPackage/update
   */
  gameAppAssetPackageUpdatePost = (
    data: UpdateGameAppAssetPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppAssetPackageUpdatePostData, any>({
      path: `/api/gameAppAssetPackage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppAssetPackageAddPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageAddPost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageAddPost(...args);
export const gameAppAssetPackageDeletePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageDeletePost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageDeletePost(...args);
export const gameAppAssetPackageDetailGet = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageDetailGet']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageDetailGet(...args);
export const gameAppAssetPackagePagePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackagePagePost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackagePagePost(...args);
export const gameAppAssetPackageSelectPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageSelectPost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageSelectPost(...args);
export const gameAppAssetPackageSelectStringPost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageSelectStringPost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageSelectStringPost(...args);
export const gameAppAssetPackageUpdatePost = (...args: Parameters<InstanceType<typeof GameAppAssetPackage>['gameAppAssetPackageUpdatePost']>) => getAPI(GameAppAssetPackage).gameAppAssetPackageUpdatePost(...args);
