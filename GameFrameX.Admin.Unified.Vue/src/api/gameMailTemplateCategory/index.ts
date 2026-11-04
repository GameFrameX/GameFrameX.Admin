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
  AddTenantGameMailTemplateCategoryEntityRequest,
  GameMailTemplateCategoryAddPostData,
  GameMailTemplateCategoryDeletePostData,
  GameMailTemplateCategoryDetailGetData,
  GameMailTemplateCategoryPagePostData,
  GameMailTemplateCategorySelectPostData,
  GameMailTemplateCategorySelectStringPostData,
  GameMailTemplateCategoryUpdatePostData,
  PageTenantGameMailTemplateCategoryEntityRequest,
  UpdateTenantGameMailTemplateCategoryEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameMailTemplateCategory<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategoryAddPost
   * @summary 新增
   * @request POST:/api/gameMailTemplateCategory/add
   */
  gameMailTemplateCategoryAddPost = (
    data: AddTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategoryAddPostData, any>({
      path: `/api/gameMailTemplateCategory/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategoryDeletePost
   * @summary 删除
   * @request POST:/api/gameMailTemplateCategory/delete
   */
  gameMailTemplateCategoryDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategoryDeletePostData, any>({
      path: `/api/gameMailTemplateCategory/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategoryDetailGet
   * @summary 详情
   * @request GET:/api/gameMailTemplateCategory/detail
   */
  gameMailTemplateCategoryDetailGet = (
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
    this.http.request<GameMailTemplateCategoryDetailGetData, any>({
      path: `/api/gameMailTemplateCategory/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategoryPagePost
   * @summary 分页查询
   * @request POST:/api/gameMailTemplateCategory/page
   */
  gameMailTemplateCategoryPagePost = (
    data: PageTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategoryPagePostData, any>({
      path: `/api/gameMailTemplateCategory/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategorySelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameMailTemplateCategory/select
   */
  gameMailTemplateCategorySelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategorySelectPostData, any>({
      path: `/api/gameMailTemplateCategory/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategorySelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameMailTemplateCategory/selectString
   */
  gameMailTemplateCategorySelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategorySelectStringPostData, any>({
      path: `/api/gameMailTemplateCategory/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMailTemplateCategory
   * @name GameMailTemplateCategoryUpdatePost
   * @summary 更新
   * @request POST:/api/gameMailTemplateCategory/update
   */
  gameMailTemplateCategoryUpdatePost = (
    data: UpdateTenantGameMailTemplateCategoryEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailTemplateCategoryUpdatePostData, any>({
      path: `/api/gameMailTemplateCategory/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameMailTemplateCategoryAddPost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategoryAddPost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategoryAddPost(...args);
export const gameMailTemplateCategoryDeletePost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategoryDeletePost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategoryDeletePost(...args);
export const gameMailTemplateCategoryDetailGet = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategoryDetailGet']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategoryDetailGet(...args);
export const gameMailTemplateCategoryPagePost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategoryPagePost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategoryPagePost(...args);
export const gameMailTemplateCategorySelectPost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategorySelectPost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategorySelectPost(...args);
export const gameMailTemplateCategorySelectStringPost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategorySelectStringPost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategorySelectStringPost(...args);
export const gameMailTemplateCategoryUpdatePost = (...args: Parameters<InstanceType<typeof GameMailTemplateCategory>['gameMailTemplateCategoryUpdatePost']>) => getAPI(GameMailTemplateCategory).gameMailTemplateCategoryUpdatePost(...args);
