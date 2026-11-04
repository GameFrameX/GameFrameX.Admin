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
  AddTenantGameRewardTemplateGroupEntityRequest,
  GameRewardTemplateGroupAddPostData,
  GameRewardTemplateGroupDeletePostData,
  GameRewardTemplateGroupDetailGetData,
  GameRewardTemplateGroupPagePostData,
  GameRewardTemplateGroupSelectPostData,
  GameRewardTemplateGroupSelectStringPostData,
  GameRewardTemplateGroupUpdatePostData,
  PageTenantGameRewardTemplateGroupEntityRequest,
  UpdateTenantGameRewardTemplateGroupEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRewardTemplateGroup<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupAddPost
   * @summary 新增
   * @request POST:/api/gameRewardTemplateGroup/add
   */
  gameRewardTemplateGroupAddPost = (
    data: AddTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupAddPostData, any>({
      path: `/api/gameRewardTemplateGroup/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupDeletePost
   * @summary 删除
   * @request POST:/api/gameRewardTemplateGroup/delete
   */
  gameRewardTemplateGroupDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupDeletePostData, any>({
      path: `/api/gameRewardTemplateGroup/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupDetailGet
   * @summary 详情
   * @request GET:/api/gameRewardTemplateGroup/detail
   */
  gameRewardTemplateGroupDetailGet = (
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
    this.http.request<GameRewardTemplateGroupDetailGetData, any>({
      path: `/api/gameRewardTemplateGroup/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupPagePost
   * @summary 分页查询
   * @request POST:/api/gameRewardTemplateGroup/page
   */
  gameRewardTemplateGroupPagePost = (
    data: PageTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupPagePostData, any>({
      path: `/api/gameRewardTemplateGroup/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupSelectPost
   * @summary 获取主库中启用的奖励模板分组。
   * @request POST:/api/gameRewardTemplateGroup/select
   */
  gameRewardTemplateGroupSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupSelectPostData, any>({
      path: `/api/gameRewardTemplateGroup/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameRewardTemplateGroup/selectString
   */
  gameRewardTemplateGroupSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupSelectStringPostData, any>({
      path: `/api/gameRewardTemplateGroup/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplateGroup
   * @name GameRewardTemplateGroupUpdatePost
   * @summary 更新
   * @request POST:/api/gameRewardTemplateGroup/update
   */
  gameRewardTemplateGroupUpdatePost = (
    data: UpdateTenantGameRewardTemplateGroupEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateGroupUpdatePostData, any>({
      path: `/api/gameRewardTemplateGroup/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRewardTemplateGroupAddPost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupAddPost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupAddPost(...args);
export const gameRewardTemplateGroupDeletePost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupDeletePost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupDeletePost(...args);
export const gameRewardTemplateGroupDetailGet = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupDetailGet']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupDetailGet(...args);
export const gameRewardTemplateGroupPagePost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupPagePost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupPagePost(...args);
export const gameRewardTemplateGroupSelectPost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupSelectPost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupSelectPost(...args);
export const gameRewardTemplateGroupSelectStringPost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupSelectStringPost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupSelectStringPost(...args);
export const gameRewardTemplateGroupUpdatePost = (...args: Parameters<InstanceType<typeof GameRewardTemplateGroup>['gameRewardTemplateGroupUpdatePost']>) => getAPI(GameRewardTemplateGroup).gameRewardTemplateGroupUpdatePost(...args);
