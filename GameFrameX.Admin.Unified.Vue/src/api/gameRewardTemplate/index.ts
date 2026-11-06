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
  AddGameRewardTemplateEntityRequest,
  GameRewardTemplateAddPostData,
  GameRewardTemplateCopySnapshotPostData,
  GameRewardTemplateDeletePostData,
  GameRewardTemplateDetailGetData,
  GameRewardTemplatePagePostData,
  GameRewardTemplateSelectPostData,
  GameRewardTemplateSelectStringPostData,
  GameRewardTemplateSnapshotRequest,
  GameRewardTemplateUpdatePostData,
  PageGameRewardTemplateEntityRequest,
  UpdateGameRewardTemplateEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRewardTemplate<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateAddPost
   * @summary 新增
   * @request POST:/api/gameRewardTemplate/add
   */
  gameRewardTemplateAddPost = (
    data: AddGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateAddPostData, any>({
      path: `/api/gameRewardTemplate/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateCopySnapshotPost
   * @summary 复制当前模板奖励快照。业务侧应保存返回的 JSON 快照，而不是运行时引用模板。
   * @request POST:/api/gameRewardTemplate/copySnapshot
   */
  gameRewardTemplateCopySnapshotPost = (
    data: GameRewardTemplateSnapshotRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateCopySnapshotPostData, any>({
      path: `/api/gameRewardTemplate/copySnapshot`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateDeletePost
   * @summary 删除
   * @request POST:/api/gameRewardTemplate/delete
   */
  gameRewardTemplateDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateDeletePostData, any>({
      path: `/api/gameRewardTemplate/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateDetailGet
   * @summary 详情
   * @request GET:/api/gameRewardTemplate/detail
   */
  gameRewardTemplateDetailGet = (
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
    this.http.request<GameRewardTemplateDetailGetData, any>({
      path: `/api/gameRewardTemplate/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplatePagePost
   * @summary 分页查询
   * @request POST:/api/gameRewardTemplate/page
   */
  gameRewardTemplatePagePost = (
    data: PageGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplatePagePostData, any>({
      path: `/api/gameRewardTemplate/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateSelectPost
   * @summary 获取主库中启用的奖励模板。
   * @request POST:/api/gameRewardTemplate/select
   */
  gameRewardTemplateSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateSelectPostData, any>({
      path: `/api/gameRewardTemplate/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameRewardTemplate/selectString
   */
  gameRewardTemplateSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateSelectStringPostData, any>({
      path: `/api/gameRewardTemplate/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRewardTemplate
   * @name GameRewardTemplateUpdatePost
   * @summary 更新
   * @request POST:/api/gameRewardTemplate/update
   */
  gameRewardTemplateUpdatePost = (
    data: UpdateGameRewardTemplateEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRewardTemplateUpdatePostData, any>({
      path: `/api/gameRewardTemplate/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRewardTemplateAddPost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateAddPost']>) => getAPI(GameRewardTemplate).gameRewardTemplateAddPost(...args);
export const gameRewardTemplateCopySnapshotPost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateCopySnapshotPost']>) => getAPI(GameRewardTemplate).gameRewardTemplateCopySnapshotPost(...args);
export const gameRewardTemplateDeletePost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateDeletePost']>) => getAPI(GameRewardTemplate).gameRewardTemplateDeletePost(...args);
export const gameRewardTemplateDetailGet = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateDetailGet']>) => getAPI(GameRewardTemplate).gameRewardTemplateDetailGet(...args);
export const gameRewardTemplatePagePost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplatePagePost']>) => getAPI(GameRewardTemplate).gameRewardTemplatePagePost(...args);
export const gameRewardTemplateSelectPost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateSelectPost']>) => getAPI(GameRewardTemplate).gameRewardTemplateSelectPost(...args);
export const gameRewardTemplateSelectStringPost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateSelectStringPost']>) => getAPI(GameRewardTemplate).gameRewardTemplateSelectStringPost(...args);
export const gameRewardTemplateUpdatePost = (...args: Parameters<InstanceType<typeof GameRewardTemplate>['gameRewardTemplateUpdatePost']>) => getAPI(GameRewardTemplate).gameRewardTemplateUpdatePost(...args);
