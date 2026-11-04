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
  AddGameAppReleaseVersionRequest,
  GameAppReleaseVersionAddPostData,
  GameAppReleaseVersionDeletePostData,
  GameAppReleaseVersionDetailGetData,
  GameAppReleaseVersionPagePostData,
  GameAppReleaseVersionUpdatePostData,
  PageGameAppReleaseVersionRequest,
  UpdateGameAppReleaseVersionRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppReleaseVersion<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppReleaseVersion
   * @name GameAppReleaseVersionAddPost
   * @summary 新增
   * @request POST:/api/gameAppReleaseVersion/add
   */
  gameAppReleaseVersionAddPost = (
    data: AddGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppReleaseVersionAddPostData, any>({
      path: `/api/gameAppReleaseVersion/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppReleaseVersion
   * @name GameAppReleaseVersionDeletePost
   * @summary 删除
   * @request POST:/api/gameAppReleaseVersion/delete
   */
  gameAppReleaseVersionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppReleaseVersionDeletePostData, any>({
      path: `/api/gameAppReleaseVersion/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppReleaseVersion
   * @name GameAppReleaseVersionDetailGet
   * @summary 详情
   * @request GET:/api/gameAppReleaseVersion/detail
   */
  gameAppReleaseVersionDetailGet = (
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
    this.http.request<GameAppReleaseVersionDetailGetData, any>({
      path: `/api/gameAppReleaseVersion/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppReleaseVersion
   * @name GameAppReleaseVersionPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppReleaseVersion/page
   */
  gameAppReleaseVersionPagePost = (
    data: PageGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppReleaseVersionPagePostData, any>({
      path: `/api/gameAppReleaseVersion/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppReleaseVersion
   * @name GameAppReleaseVersionUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppReleaseVersion/update
   */
  gameAppReleaseVersionUpdatePost = (
    data: UpdateGameAppReleaseVersionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppReleaseVersionUpdatePostData, any>({
      path: `/api/gameAppReleaseVersion/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppReleaseVersionAddPost = (...args: Parameters<InstanceType<typeof GameAppReleaseVersion>['gameAppReleaseVersionAddPost']>) => getAPI(GameAppReleaseVersion).gameAppReleaseVersionAddPost(...args);
export const gameAppReleaseVersionDeletePost = (...args: Parameters<InstanceType<typeof GameAppReleaseVersion>['gameAppReleaseVersionDeletePost']>) => getAPI(GameAppReleaseVersion).gameAppReleaseVersionDeletePost(...args);
export const gameAppReleaseVersionDetailGet = (...args: Parameters<InstanceType<typeof GameAppReleaseVersion>['gameAppReleaseVersionDetailGet']>) => getAPI(GameAppReleaseVersion).gameAppReleaseVersionDetailGet(...args);
export const gameAppReleaseVersionPagePost = (...args: Parameters<InstanceType<typeof GameAppReleaseVersion>['gameAppReleaseVersionPagePost']>) => getAPI(GameAppReleaseVersion).gameAppReleaseVersionPagePost(...args);
export const gameAppReleaseVersionUpdatePost = (...args: Parameters<InstanceType<typeof GameAppReleaseVersion>['gameAppReleaseVersionUpdatePost']>) => getAPI(GameAppReleaseVersion).gameAppReleaseVersionUpdatePost(...args);
