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
  AddTenantGameMailEntityRequest,
  GameMailAddPostData,
  GameMailDeletePostData,
  GameMailDetailGetData,
  GameMailPagePostData,
  GameMailSendPostData,
  GameMailUpdatePostData,
  PageTenantGameMailEntityRequest,
  UpdateTenantGameMailEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameMail<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailAddPost
   * @summary 新增
   * @request POST:/api/gameMail/add
   */
  gameMailAddPost = (
    data: AddTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailAddPostData, any>({
      path: `/api/gameMail/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailDeletePost
   * @summary 删除
   * @request POST:/api/gameMail/delete
   */
  gameMailDeletePost = (data: BaseDeleteRequest, params: RequestParams = {}) =>
    this.http.request<GameMailDeletePostData, any>({
      path: `/api/gameMail/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailDetailGet
   * @summary 详情
   * @request GET:/api/gameMail/detail
   */
  gameMailDetailGet = (
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
    this.http.request<GameMailDetailGetData, any>({
      path: `/api/gameMail/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailPagePost
   * @summary 分页查询
   * @request POST:/api/gameMail/page
   */
  gameMailPagePost = (
    data: PageTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailPagePostData, any>({
      path: `/api/gameMail/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailSendPost
   * @summary 发送游戏邮件
   * @request POST:/api/gameMail/send
   */
  gameMailSendPost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<GameMailSendPostData, any>({
      path: `/api/gameMail/send`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameMail
   * @name GameMailUpdatePost
   * @summary 更新
   * @request POST:/api/gameMail/update
   */
  gameMailUpdatePost = (
    data: UpdateTenantGameMailEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameMailUpdatePostData, any>({
      path: `/api/gameMail/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameMailAddPost = (...args: Parameters<InstanceType<typeof GameMail>['gameMailAddPost']>) => getAPI(GameMail).gameMailAddPost(...args);
export const gameMailDeletePost = (...args: Parameters<InstanceType<typeof GameMail>['gameMailDeletePost']>) => getAPI(GameMail).gameMailDeletePost(...args);
export const gameMailDetailGet = (...args: Parameters<InstanceType<typeof GameMail>['gameMailDetailGet']>) => getAPI(GameMail).gameMailDetailGet(...args);
export const gameMailPagePost = (...args: Parameters<InstanceType<typeof GameMail>['gameMailPagePost']>) => getAPI(GameMail).gameMailPagePost(...args);
export const gameMailSendPost = (...args: Parameters<InstanceType<typeof GameMail>['gameMailSendPost']>) => getAPI(GameMail).gameMailSendPost(...args);
export const gameMailUpdatePost = (...args: Parameters<InstanceType<typeof GameMail>['gameMailUpdatePost']>) => getAPI(GameMail).gameMailUpdatePost(...args);
