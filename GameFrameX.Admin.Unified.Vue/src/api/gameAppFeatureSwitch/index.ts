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
  AddGameAppFeatureSwitchRequest,
  GameAppFeatureSwitchAddPostData,
  GameAppFeatureSwitchDeletePostData,
  GameAppFeatureSwitchDetailGetData,
  GameAppFeatureSwitchPagePostData,
  GameAppFeatureSwitchUpdatePostData,
  PageGameAppFeatureSwitchRequest,
  UpdateGameAppFeatureSwitchRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppFeatureSwitch<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppFeatureSwitch
   * @name GameAppFeatureSwitchAddPost
   * @summary 新增
   * @request POST:/api/gameAppFeatureSwitch/add
   */
  gameAppFeatureSwitchAddPost = (
    data: AddGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppFeatureSwitchAddPostData, any>({
      path: `/api/gameAppFeatureSwitch/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppFeatureSwitch
   * @name GameAppFeatureSwitchDeletePost
   * @summary 删除
   * @request POST:/api/gameAppFeatureSwitch/delete
   */
  gameAppFeatureSwitchDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppFeatureSwitchDeletePostData, any>({
      path: `/api/gameAppFeatureSwitch/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppFeatureSwitch
   * @name GameAppFeatureSwitchDetailGet
   * @summary 详情
   * @request GET:/api/gameAppFeatureSwitch/detail
   */
  gameAppFeatureSwitchDetailGet = (
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
    this.http.request<GameAppFeatureSwitchDetailGetData, any>({
      path: `/api/gameAppFeatureSwitch/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppFeatureSwitch
   * @name GameAppFeatureSwitchPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppFeatureSwitch/page
   */
  gameAppFeatureSwitchPagePost = (
    data: PageGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppFeatureSwitchPagePostData, any>({
      path: `/api/gameAppFeatureSwitch/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppFeatureSwitch
   * @name GameAppFeatureSwitchUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppFeatureSwitch/update
   */
  gameAppFeatureSwitchUpdatePost = (
    data: UpdateGameAppFeatureSwitchRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppFeatureSwitchUpdatePostData, any>({
      path: `/api/gameAppFeatureSwitch/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppFeatureSwitchAddPost = (...args: Parameters<InstanceType<typeof GameAppFeatureSwitch>['gameAppFeatureSwitchAddPost']>) => getAPI(GameAppFeatureSwitch).gameAppFeatureSwitchAddPost(...args);
export const gameAppFeatureSwitchDeletePost = (...args: Parameters<InstanceType<typeof GameAppFeatureSwitch>['gameAppFeatureSwitchDeletePost']>) => getAPI(GameAppFeatureSwitch).gameAppFeatureSwitchDeletePost(...args);
export const gameAppFeatureSwitchDetailGet = (...args: Parameters<InstanceType<typeof GameAppFeatureSwitch>['gameAppFeatureSwitchDetailGet']>) => getAPI(GameAppFeatureSwitch).gameAppFeatureSwitchDetailGet(...args);
export const gameAppFeatureSwitchPagePost = (...args: Parameters<InstanceType<typeof GameAppFeatureSwitch>['gameAppFeatureSwitchPagePost']>) => getAPI(GameAppFeatureSwitch).gameAppFeatureSwitchPagePost(...args);
export const gameAppFeatureSwitchUpdatePost = (...args: Parameters<InstanceType<typeof GameAppFeatureSwitch>['gameAppFeatureSwitchUpdatePost']>) => getAPI(GameAppFeatureSwitch).gameAppFeatureSwitchUpdatePost(...args);
