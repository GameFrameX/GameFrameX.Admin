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
  AddTenantGameUserEntityByMailRequest,
  DeleteTenantGameUserEntityRequest,
  GameUserAddByMailPostData,
  GameUserAddPostData,
  GameUserBanPostData,
  GameUserDeletePostData,
  GameUserDetailGetData,
  GameUserPagePostData,
  GameUserSelectPostData,
  GameUserSelectStringPostData,
  GameUserUnsealPostData,
  GameUserUpdatePostData,
  PageTenantGameUserEntityRequest,
  UpdateTenantGameUserEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseSelectPageRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameUser<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserAddPost
   * @summary 新增
   * @request POST:/api/gameUser/add
   */
  gameUserAddPost = (
    data: AddTenantGameUserEntityByMailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserAddPostData, any>({
      path: `/api/gameUser/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserAddByMailPost
   * @summary 通过邮箱增加账号
   * @request POST:/api/gameUser/addByMail
   */
  gameUserAddByMailPost = (
    data: AddTenantGameUserEntityByMailRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserAddByMailPostData, any>({
      path: `/api/gameUser/addByMail`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserBanPost
   * @summary 封禁游戏账号
   * @request POST:/api/gameUser/ban
   */
  gameUserBanPost = (
    data: DeleteTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserBanPostData, any>({
      path: `/api/gameUser/ban`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserDeletePost
   * @summary 删除
   * @request POST:/api/gameUser/delete
   */
  gameUserDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameUserDeletePostData, any>({
      path: `/api/gameUser/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserDetailGet
   * @summary 详情
   * @request GET:/api/gameUser/detail
   */
  gameUserDetailGet = (
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
    this.http.request<GameUserDetailGetData, any>({
      path: `/api/gameUser/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserPagePost
   * @summary 分页查询
   * @request POST:/api/gameUser/page
   */
  gameUserPagePost = (
    data: PageTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserPagePostData, any>({
      path: `/api/gameUser/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserSelectPost
   * @summary 获取选择器列表
   * @request POST:/api/gameUser/select
   */
  gameUserSelectPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserSelectPostData, any>({
      path: `/api/gameUser/select`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserSelectStringPost
   * @summary 获取选择器列表（字符串 ID）
   * @request POST:/api/gameUser/selectString
   */
  gameUserSelectStringPost = (
    data: BaseSelectPageRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserSelectStringPostData, any>({
      path: `/api/gameUser/selectString`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserUnsealPost
   * @summary 解除封禁游戏账号
   * @request POST:/api/gameUser/unseal
   */
  gameUserUnsealPost = (
    data: DeleteTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserUnsealPostData, any>({
      path: `/api/gameUser/unseal`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameUser
   * @name GameUserUpdatePost
   * @summary 更新
   * @request POST:/api/gameUser/update
   */
  gameUserUpdatePost = (
    data: UpdateTenantGameUserEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameUserUpdatePostData, any>({
      path: `/api/gameUser/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameUserAddPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserAddPost']>) => getAPI(GameUser).gameUserAddPost(...args);
export const gameUserAddByMailPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserAddByMailPost']>) => getAPI(GameUser).gameUserAddByMailPost(...args);
export const gameUserBanPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserBanPost']>) => getAPI(GameUser).gameUserBanPost(...args);
export const gameUserDeletePost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserDeletePost']>) => getAPI(GameUser).gameUserDeletePost(...args);
export const gameUserDetailGet = (...args: Parameters<InstanceType<typeof GameUser>['gameUserDetailGet']>) => getAPI(GameUser).gameUserDetailGet(...args);
export const gameUserPagePost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserPagePost']>) => getAPI(GameUser).gameUserPagePost(...args);
export const gameUserSelectPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserSelectPost']>) => getAPI(GameUser).gameUserSelectPost(...args);
export const gameUserSelectStringPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserSelectStringPost']>) => getAPI(GameUser).gameUserSelectStringPost(...args);
export const gameUserUnsealPost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserUnsealPost']>) => getAPI(GameUser).gameUserUnsealPost(...args);
export const gameUserUpdatePost = (...args: Parameters<InstanceType<typeof GameUser>['gameUserUpdatePost']>) => getAPI(GameUser).gameUserUpdatePost(...args);
