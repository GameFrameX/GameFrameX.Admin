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
  AddGameAppVersionDefinitionRequest,
  GameAppVersionDefinitionAddPostData,
  GameAppVersionDefinitionDeletePostData,
  GameAppVersionDefinitionDetailGetData,
  GameAppVersionDefinitionPagePostData,
  GameAppVersionDefinitionSelectPostData,
  GameAppVersionDefinitionSelectStringPostData,
  GameAppVersionDefinitionUpdatePostData,
  PageGameAppVersionDefinitionRequest,
  UpdateGameAppVersionDefinitionRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppVersionDefinition<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionAddPost
   * @summary 新增
   * @request POST:/api/gameAppVersionDefinition/add
   */
  gameAppVersionDefinitionAddPost = (
    data: AddGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionAddPostData, any>({
      path: `/api/gameAppVersionDefinition/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionDeletePost
   * @summary 删除
   * @request POST:/api/gameAppVersionDefinition/delete
   */
  gameAppVersionDefinitionDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionDeletePostData, any>({
      path: `/api/gameAppVersionDefinition/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionDetailGet
   * @summary 详情
   * @request GET:/api/gameAppVersionDefinition/detail
   */
  gameAppVersionDefinitionDetailGet = (
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
    this.http.request<GameAppVersionDefinitionDetailGetData, any>({
      path: `/api/gameAppVersionDefinition/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionPagePost
   * @summary 分页查询
   * @request POST:/api/gameAppVersionDefinition/page
   */
  gameAppVersionDefinitionPagePost = (
    data: PageGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionPagePostData, any>({
      path: `/api/gameAppVersionDefinition/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionSelectPost
   * @summary 获取主库版本定义选择器列表。
   * @request POST:/api/gameAppVersionDefinition/select
   */
  gameAppVersionDefinitionSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionSelectPostData, any>({
      path: `/api/gameAppVersionDefinition/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameAppVersionDefinition/selectString
   */
  gameAppVersionDefinitionSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionSelectStringPostData, any>({
      path: `/api/gameAppVersionDefinition/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppVersionDefinition
   * @name GameAppVersionDefinitionUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppVersionDefinition/update
   */
  gameAppVersionDefinitionUpdatePost = (
    data: UpdateGameAppVersionDefinitionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppVersionDefinitionUpdatePostData, any>({
      path: `/api/gameAppVersionDefinition/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppVersionDefinitionAddPost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionAddPost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionAddPost(...args);
export const gameAppVersionDefinitionDeletePost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionDeletePost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionDeletePost(...args);
export const gameAppVersionDefinitionDetailGet = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionDetailGet']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionDetailGet(...args);
export const gameAppVersionDefinitionPagePost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionPagePost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionPagePost(...args);
export const gameAppVersionDefinitionSelectPost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionSelectPost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionSelectPost(...args);
export const gameAppVersionDefinitionSelectStringPost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionSelectStringPost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionSelectStringPost(...args);
export const gameAppVersionDefinitionUpdatePost = (...args: Parameters<InstanceType<typeof GameAppVersionDefinition>['gameAppVersionDefinitionUpdatePost']>) => getAPI(GameAppVersionDefinition).gameAppVersionDefinitionUpdatePost(...args);
