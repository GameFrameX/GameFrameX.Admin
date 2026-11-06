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
  AddGameAppOpenAccessRequest,
  GameAppOpenAccessActionRequest,
  GameAppOpenAccessAddPostData,
  GameAppOpenAccessDeletePostData,
  GameAppOpenAccessDisablePostData,
  GameAppOpenAccessEnablePostData,
  GameAppOpenAccessPagePostData,
  GameAppOpenAccessRefreshPostData,
  GameAppOpenAccessRevokePostData,
  GameAppOpenAccessUpdatePostData,
  PageGameAppOpenAccessRequest,
  UpdateGameAppOpenAccessRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppOpenAccess<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessAddPost
   * @summary 签发应用开放访问凭证。AccessKey / AccessSecret 由服务端生成，AccessSecret 仅本次响应一次性返回。
   * @request POST:/api/gameAppOpenAccess/add
   */
  gameAppOpenAccessAddPost = (
    data: AddGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessAddPostData, any>({
      path: `/api/gameAppOpenAccess/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessDeletePost
   * @summary 删除应用开放访问凭证。
   * @request POST:/api/gameAppOpenAccess/delete
   */
  gameAppOpenAccessDeletePost = (
    data: GameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessDeletePostData, any>({
      path: `/api/gameAppOpenAccess/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessDisablePost
   * @summary 禁用应用开放访问凭证。
   * @request POST:/api/gameAppOpenAccess/disable
   */
  gameAppOpenAccessDisablePost = (
    data: GameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessDisablePostData, any>({
      path: `/api/gameAppOpenAccess/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessEnablePost
   * @summary 启用应用开放访问凭证（已撤销的凭证不可启用）。
   * @request POST:/api/gameAppOpenAccess/enable
   */
  gameAppOpenAccessEnablePost = (
    data: GameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessEnablePostData, any>({
      path: `/api/gameAppOpenAccess/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessPagePost
   * @summary 获取应用开放访问凭证分页列表（AccessSecret 永不输出）。
   * @request POST:/api/gameAppOpenAccess/page
   */
  gameAppOpenAccessPagePost = (
    data: PageGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessPagePostData, any>({
      path: `/api/gameAppOpenAccess/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessRefreshPost
   * @summary 续期 / 轮换密钥。重置 AccessSecret 并一次性返回（前端须立即保存）。
   * @request POST:/api/gameAppOpenAccess/refresh
   */
  gameAppOpenAccessRefreshPost = (
    data: GameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessRefreshPostData, any>({
      path: `/api/gameAppOpenAccess/refresh`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessRevokePost
   * @summary 吊销应用开放访问凭证（写 RevokedTime / RevokeReason，并禁用）。
   * @request POST:/api/gameAppOpenAccess/revoke
   */
  gameAppOpenAccessRevokePost = (
    data: GameAppOpenAccessActionRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessRevokePostData, any>({
      path: `/api/gameAppOpenAccess/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppOpenAccess
   * @name GameAppOpenAccessUpdatePost
   * @summary 更新应用开放访问凭证。校验绑定用户存在与 Scopes 有效。
   * @request POST:/api/gameAppOpenAccess/update
   */
  gameAppOpenAccessUpdatePost = (
    data: UpdateGameAppOpenAccessRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppOpenAccessUpdatePostData, any>({
      path: `/api/gameAppOpenAccess/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppOpenAccessAddPost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessAddPost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessAddPost(...args);
export const gameAppOpenAccessDeletePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessDeletePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessDeletePost(...args);
export const gameAppOpenAccessDisablePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessDisablePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessDisablePost(...args);
export const gameAppOpenAccessEnablePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessEnablePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessEnablePost(...args);
export const gameAppOpenAccessPagePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessPagePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessPagePost(...args);
export const gameAppOpenAccessRefreshPost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessRefreshPost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessRefreshPost(...args);
export const gameAppOpenAccessRevokePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessRevokePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessRevokePost(...args);
export const gameAppOpenAccessUpdatePost = (...args: Parameters<InstanceType<typeof GameAppOpenAccess>['gameAppOpenAccessUpdatePost']>) => getAPI(GameAppOpenAccess).gameAppOpenAccessUpdatePost(...args);
