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
  AddGameRoleEntityRequest,
  GameRoleAddPostData,
  GameRoleDeletePostData,
  GameRoleDetailGetData,
  GameRolePagePostData,
  GameRoleSelectPostData,
  GameRoleSelectStringPostData,
  GameRoleSyncPostData,
  GameRoleUpdatePostData,
  GameRoleUpdateToSandBoxPostData,
  GameRoleUpdateToUnSandBoxPostData,
  PageGameRoleEntityRequest,
  UpdateGameRoleEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRole<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleAddPost
   * @summary 新增
   * @request POST:/api/gameRole/add
   */
  gameRoleAddPost = (
    data: AddGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleAddPostData, any>({
      path: `/api/gameRole/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleDeletePost
   * @summary 删除
   * @request POST:/api/gameRole/delete
   */
  gameRoleDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameRoleDeletePostData, any>({
      path: `/api/gameRole/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleDetailGet
   * @summary 获取游戏角色
   * @request GET:/api/gameRole/detail
   */
  gameRoleDetailGet = (
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
    this.http.request<GameRoleDetailGetData, any>({
      path: `/api/gameRole/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRolePagePost
   * @summary 分页查询游戏角色（含 RoleId 回填）
   * @request POST:/api/gameRole/page
   */
  gameRolePagePost = (
    data: PageGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRolePagePostData, any>({
      path: `/api/gameRole/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameRole/select
   */
  gameRoleSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleSelectPostData, any>({
      path: `/api/gameRole/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameRole/selectString
   */
  gameRoleSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleSelectStringPostData, any>({
      path: `/api/gameRole/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleSyncPost
   * @summary 同步游戏角色
   * @request POST:/api/gameRole/sync
   */
  gameRoleSyncPost = (
    data: UpdateGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleSyncPostData, any>({
      path: `/api/gameRole/sync`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleUpdatePost
   * @summary 更新游戏角色
   * @request POST:/api/gameRole/update
   */
  gameRoleUpdatePost = (
    data: UpdateGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleUpdatePostData, any>({
      path: `/api/gameRole/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleUpdateToSandBoxPost
   * @summary 更新游戏角色为沙盒用户
   * @request POST:/api/gameRole/updateToSandBox
   */
  gameRoleUpdateToSandBoxPost = (
    data: UpdateGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleUpdateToSandBoxPostData, any>({
      path: `/api/gameRole/updateToSandBox`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRole
   * @name GameRoleUpdateToUnSandBoxPost
   * @summary 更新游戏角色为非沙盒用户
   * @request POST:/api/gameRole/updateToUnSandBox
   */
  gameRoleUpdateToUnSandBoxPost = (
    data: UpdateGameRoleEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRoleUpdateToUnSandBoxPostData, any>({
      path: `/api/gameRole/updateToUnSandBox`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRoleAddPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleAddPost']>) => getAPI(GameRole).gameRoleAddPost(...args);
export const gameRoleDeletePost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleDeletePost']>) => getAPI(GameRole).gameRoleDeletePost(...args);
export const gameRoleDetailGet = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleDetailGet']>) => getAPI(GameRole).gameRoleDetailGet(...args);
export const gameRolePagePost = (...args: Parameters<InstanceType<typeof GameRole>['gameRolePagePost']>) => getAPI(GameRole).gameRolePagePost(...args);
export const gameRoleSelectPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleSelectPost']>) => getAPI(GameRole).gameRoleSelectPost(...args);
export const gameRoleSelectStringPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleSelectStringPost']>) => getAPI(GameRole).gameRoleSelectStringPost(...args);
export const gameRoleSyncPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleSyncPost']>) => getAPI(GameRole).gameRoleSyncPost(...args);
export const gameRoleUpdatePost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleUpdatePost']>) => getAPI(GameRole).gameRoleUpdatePost(...args);
export const gameRoleUpdateToSandBoxPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleUpdateToSandBoxPost']>) => getAPI(GameRole).gameRoleUpdateToSandBoxPost(...args);
export const gameRoleUpdateToUnSandBoxPost = (...args: Parameters<InstanceType<typeof GameRole>['gameRoleUpdateToUnSandBoxPost']>) => getAPI(GameRole).gameRoleUpdateToUnSandBoxPost(...args);
