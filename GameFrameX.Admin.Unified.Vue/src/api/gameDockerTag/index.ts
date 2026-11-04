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
  AddTenantGameDockerTagEntityRequest,
  GameDockerTagAddPostData,
  GameDockerTagDeletePostData,
  GameDockerTagDetailGetData,
  GameDockerTagPagePostData,
  GameDockerTagSelectPostData,
  GameDockerTagSelectStringPostData,
  GameDockerTagUpdatePostData,
  PageTenantGameDockerTagEntityRequest,
  UpdateTenantGameDockerTagEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameDockerTag<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagAddPost
   * @summary 新增
   * @request POST:/api/gameDockerTag/add
   */
  gameDockerTagAddPost = (
    data: AddTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagAddPostData, any>({
      path: `/api/gameDockerTag/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagDeletePost
   * @summary 删除
   * @request POST:/api/gameDockerTag/delete
   */
  gameDockerTagDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagDeletePostData, any>({
      path: `/api/gameDockerTag/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagDetailGet
   * @summary 详情
   * @request GET:/api/gameDockerTag/detail
   */
  gameDockerTagDetailGet = (
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
    this.http.request<GameDockerTagDetailGetData, any>({
      path: `/api/gameDockerTag/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagPagePost
   * @summary 分页查询
   * @request POST:/api/gameDockerTag/page
   */
  gameDockerTagPagePost = (
    data: PageTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagPagePostData, any>({
      path: `/api/gameDockerTag/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameDockerTag/select
   */
  gameDockerTagSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagSelectPostData, any>({
      path: `/api/gameDockerTag/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameDockerTag/selectString
   */
  gameDockerTagSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagSelectStringPostData, any>({
      path: `/api/gameDockerTag/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameDockerTag
   * @name GameDockerTagUpdatePost
   * @summary 更新
   * @request POST:/api/gameDockerTag/update
   */
  gameDockerTagUpdatePost = (
    data: UpdateTenantGameDockerTagEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameDockerTagUpdatePostData, any>({
      path: `/api/gameDockerTag/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameDockerTagAddPost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagAddPost']>) => getAPI(GameDockerTag).gameDockerTagAddPost(...args);
export const gameDockerTagDeletePost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagDeletePost']>) => getAPI(GameDockerTag).gameDockerTagDeletePost(...args);
export const gameDockerTagDetailGet = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagDetailGet']>) => getAPI(GameDockerTag).gameDockerTagDetailGet(...args);
export const gameDockerTagPagePost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagPagePost']>) => getAPI(GameDockerTag).gameDockerTagPagePost(...args);
export const gameDockerTagSelectPost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagSelectPost']>) => getAPI(GameDockerTag).gameDockerTagSelectPost(...args);
export const gameDockerTagSelectStringPost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagSelectStringPost']>) => getAPI(GameDockerTag).gameDockerTagSelectStringPost(...args);
export const gameDockerTagUpdatePost = (...args: Parameters<InstanceType<typeof GameDockerTag>['gameDockerTagUpdatePost']>) => getAPI(GameDockerTag).gameDockerTagUpdatePost(...args);
