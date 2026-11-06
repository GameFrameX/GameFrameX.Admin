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
  AddGameMailTemplateEntityRequest,
  GameMailTemplateAddPostData,
  GameMailTemplateDeletePostData,
  GameMailTemplateDetailGetData,
  GameMailTemplatePagePostData,
  GameMailTemplateSelectPostData,
  GameMailTemplateSelectStringPostData,
  GameMailTemplateUpdatePostData,
  PageGameMailTemplateEntityRequest,
  UpdateGameMailTemplateEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameMailTemplate<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateAddPost
   * @summary 新增
   * @request POST:/api/gameMailTemplate/add
   */
  gameMailTemplateAddPost = (
    data: AddGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateAddPostData, any>({
      path: `/api/gameMailTemplate/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateDeletePost
   * @summary 删除
   * @request POST:/api/gameMailTemplate/delete
   */
  gameMailTemplateDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateDeletePostData, any>({
      path: `/api/gameMailTemplate/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateDetailGet
   * @summary 详情
   * @request GET:/api/gameMailTemplate/detail
   */
  gameMailTemplateDetailGet = (
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
    this.http.request<GameMailTemplateDetailGetData, any>({
      path: `/api/gameMailTemplate/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplatePagePost
   * @summary 分页查询
   * @request POST:/api/gameMailTemplate/page
   */
  gameMailTemplatePagePost = (
    data: PageGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplatePagePostData, any>({
      path: `/api/gameMailTemplate/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameMailTemplate/select
   */
  gameMailTemplateSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateSelectPostData, any>({
      path: `/api/gameMailTemplate/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameMailTemplate/selectString
   */
  gameMailTemplateSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateSelectStringPostData, any>({
      path: `/api/gameMailTemplate/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplate
   * @name GameMailTemplateUpdatePost
   * @summary 更新
   * @request POST:/api/gameMailTemplate/update
   */
  gameMailTemplateUpdatePost = (
    data: UpdateGameMailTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateUpdatePostData, any>({
      path: `/api/gameMailTemplate/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameMailTemplateAddPost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateAddPost']>) => getAPI(GameMailTemplate).gameMailTemplateAddPost(...args);
export const gameMailTemplateDeletePost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateDeletePost']>) => getAPI(GameMailTemplate).gameMailTemplateDeletePost(...args);
export const gameMailTemplateDetailGet = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateDetailGet']>) => getAPI(GameMailTemplate).gameMailTemplateDetailGet(...args);
export const gameMailTemplatePagePost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplatePagePost']>) => getAPI(GameMailTemplate).gameMailTemplatePagePost(...args);
export const gameMailTemplateSelectPost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateSelectPost']>) => getAPI(GameMailTemplate).gameMailTemplateSelectPost(...args);
export const gameMailTemplateSelectStringPost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateSelectStringPost']>) => getAPI(GameMailTemplate).gameMailTemplateSelectStringPost(...args);
export const gameMailTemplateUpdatePost = (...args: Parameters<InstanceType<typeof GameMailTemplate>['gameMailTemplateUpdatePost']>) => getAPI(GameMailTemplate).gameMailTemplateUpdatePost(...args);
