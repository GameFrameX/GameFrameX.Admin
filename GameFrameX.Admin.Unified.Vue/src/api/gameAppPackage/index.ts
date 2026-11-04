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
  AddGameAppPackageRequest,
  GameAppPackageAddPostData,
  GameAppPackageDeletePostData,
  GameAppPackageDetailGetData,
  GameAppPackageListGetData,
  GameAppPackagePagePostData,
  GameAppPackageSelectPostData,
  GameAppPackageSelectStringPostData,
  GameAppPackageSetDefaultPostData,
  GameAppPackageSetStatusPostData,
  GameAppPackageUpdatePostData,
  PageGameAppPackageRequest,
  SetGameAppPackageStatusRequest,
  UpdateGameAppPackageRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppPackage<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageAddPost
   * @summary 新增分包
   * @request POST:/api/gameAppPackage/add
   */
  gameAppPackageAddPost = (
    data: AddGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageAddPostData, any>({
      path: `/api/gameAppPackage/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageDeletePost
   * @summary 删除分包
   * @request POST:/api/gameAppPackage/delete
   */
  gameAppPackageDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageDeletePostData, any>({
      path: `/api/gameAppPackage/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageDetailGet
   * @summary 详情
   * @request GET:/api/gameAppPackage/detail
   */
  gameAppPackageDetailGet = (
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
    this.http.request<GameAppPackageDetailGetData, any>({
      path: `/api/gameAppPackage/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageListGet
   * @summary 获取主库启用分包列表
   * @request GET:/api/gameAppPackage/list
   */
  gameAppPackageListGet = (params: RequestParams = {}) =>
    this.http.request<GameAppPackageListGetData, any>({
      path: `/api/gameAppPackage/list`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackagePagePost
   * @summary 分页查询
   * @request POST:/api/gameAppPackage/page
   */
  gameAppPackagePagePost = (
    data: PageGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackagePagePostData, any>({
      path: `/api/gameAppPackage/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageSelectPost
   * @summary 获取分包选择器列表。
   * @request POST:/api/gameAppPackage/select
   */
  gameAppPackageSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageSelectPostData, any>({
      path: `/api/gameAppPackage/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameAppPackage/selectString
   */
  gameAppPackageSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageSelectStringPostData, any>({
      path: `/api/gameAppPackage/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageSetDefaultPost
   * @summary 设置默认分包
   * @request POST:/api/gameAppPackage/setDefault
   */
  gameAppPackageSetDefaultPost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageSetDefaultPostData, any>({
      path: `/api/gameAppPackage/setDefault`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageSetStatusPost
   * @summary 启用或停用分包
   * @request POST:/api/gameAppPackage/setStatus
   */
  gameAppPackageSetStatusPost = (
    data: SetGameAppPackageStatusRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageSetStatusPostData, any>({
      path: `/api/gameAppPackage/setStatus`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppPackage
   * @name GameAppPackageUpdatePost
   * @summary 更新分包
   * @request POST:/api/gameAppPackage/update
   */
  gameAppPackageUpdatePost = (
    data: UpdateGameAppPackageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppPackageUpdatePostData, any>({
      path: `/api/gameAppPackage/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppPackageAddPost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageAddPost']>) => getAPI(GameAppPackage).gameAppPackageAddPost(...args);
export const gameAppPackageDeletePost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageDeletePost']>) => getAPI(GameAppPackage).gameAppPackageDeletePost(...args);
export const gameAppPackageDetailGet = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageDetailGet']>) => getAPI(GameAppPackage).gameAppPackageDetailGet(...args);
export const gameAppPackageListGet = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageListGet']>) => getAPI(GameAppPackage).gameAppPackageListGet(...args);
export const gameAppPackagePagePost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackagePagePost']>) => getAPI(GameAppPackage).gameAppPackagePagePost(...args);
export const gameAppPackageSelectPost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageSelectPost']>) => getAPI(GameAppPackage).gameAppPackageSelectPost(...args);
export const gameAppPackageSelectStringPost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageSelectStringPost']>) => getAPI(GameAppPackage).gameAppPackageSelectStringPost(...args);
export const gameAppPackageSetDefaultPost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageSetDefaultPost']>) => getAPI(GameAppPackage).gameAppPackageSetDefaultPost(...args);
export const gameAppPackageSetStatusPost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageSetStatusPost']>) => getAPI(GameAppPackage).gameAppPackageSetStatusPost(...args);
export const gameAppPackageUpdatePost = (...args: Parameters<InstanceType<typeof GameAppPackage>['gameAppPackageUpdatePost']>) => getAPI(GameAppPackage).gameAppPackageUpdatePost(...args);
