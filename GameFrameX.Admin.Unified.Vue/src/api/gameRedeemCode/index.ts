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
  AddGameRedeemCodeEntityRequest,
  GameRedeemCodeAddPostData,
  GameRedeemCodeDeletePostData,
  GameRedeemCodeDetailGetData,
  GameRedeemCodePagePostData,
  GameRedeemCodeUpdatePostData,
  PageGameRedeemCodeEntityRequest,
  UpdateGameRedeemCodeEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameRedeemCode<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameRedeemCode
   * @name GameRedeemCodeAddPost
   * @summary 新增
   * @request POST:/api/gameRedeemCode/add
   */
  gameRedeemCodeAddPost = (
    data: AddGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeAddPostData, any>({
      path: `/api/gameRedeemCode/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCode
   * @name GameRedeemCodeDeletePost
   * @summary 删除
   * @request POST:/api/gameRedeemCode/delete
   */
  gameRedeemCodeDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeDeletePostData, any>({
      path: `/api/gameRedeemCode/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCode
   * @name GameRedeemCodeDetailGet
   * @summary 详情
   * @request GET:/api/gameRedeemCode/detail
   */
  gameRedeemCodeDetailGet = (
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
    this.http.request<GameRedeemCodeDetailGetData, any>({
      path: `/api/gameRedeemCode/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCode
   * @name GameRedeemCodePagePost
   * @summary 分页查询
   * @request POST:/api/gameRedeemCode/page
   */
  gameRedeemCodePagePost = (
    data: PageGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodePagePostData, any>({
      path: `/api/gameRedeemCode/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameRedeemCode
   * @name GameRedeemCodeUpdatePost
   * @summary 更新
   * @request POST:/api/gameRedeemCode/update
   */
  gameRedeemCodeUpdatePost = (
    data: UpdateGameRedeemCodeEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameRedeemCodeUpdatePostData, any>({
      path: `/api/gameRedeemCode/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameRedeemCodeAddPost = (...args: Parameters<InstanceType<typeof GameRedeemCode>['gameRedeemCodeAddPost']>) => getAPI(GameRedeemCode).gameRedeemCodeAddPost(...args);
export const gameRedeemCodeDeletePost = (...args: Parameters<InstanceType<typeof GameRedeemCode>['gameRedeemCodeDeletePost']>) => getAPI(GameRedeemCode).gameRedeemCodeDeletePost(...args);
export const gameRedeemCodeDetailGet = (...args: Parameters<InstanceType<typeof GameRedeemCode>['gameRedeemCodeDetailGet']>) => getAPI(GameRedeemCode).gameRedeemCodeDetailGet(...args);
export const gameRedeemCodePagePost = (...args: Parameters<InstanceType<typeof GameRedeemCode>['gameRedeemCodePagePost']>) => getAPI(GameRedeemCode).gameRedeemCodePagePost(...args);
export const gameRedeemCodeUpdatePost = (...args: Parameters<InstanceType<typeof GameRedeemCode>['gameRedeemCodeUpdatePost']>) => getAPI(GameRedeemCode).gameRedeemCodeUpdatePost(...args);
