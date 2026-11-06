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
  GameLoginTicketKeyDeletePostData,
  GameLoginTicketKeyDetailGetData,
  GameLoginTicketKeyDisablePostData,
  GameLoginTicketKeyEnablePostData,
  GameLoginTicketKeyPagePostData,
  GameLoginTicketKeyRotatePostData,
  GameLoginTicketKeyRotateRequest,
  PageGameLoginTicketKeyRequest,
} from "./types";
import {
  BaseDeleteRequest,
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameLoginTicketKey<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyDeletePost
   * @summary 禁止删除签名密钥：历史 key 是 Server 验签在途票据的依据，删除会导致验签失败；请改用禁用或轮换。
   * @request POST:/api/gameLoginTicketKey/delete
   */
  gameLoginTicketKeyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameLoginTicketKeyDeletePostData, any>({
      path: `/api/gameLoginTicketKey/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyDetailGet
   * @summary 密钥详情（仅公钥与元数据，私钥永不回显）。
   * @request GET:/api/gameLoginTicketKey/detail
   */
  gameLoginTicketKeyDetailGet = (
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
    this.http.request<GameLoginTicketKeyDetailGetData, any>({
      path: `/api/gameLoginTicketKey/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyDisablePost
   * @summary 禁用密钥。
   * @request POST:/api/gameLoginTicketKey/disable
   */
  gameLoginTicketKeyDisablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameLoginTicketKeyDisablePostData, any>({
      path: `/api/gameLoginTicketKey/disable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyEnablePost
   * @summary 启用密钥。
   * @request POST:/api/gameLoginTicketKey/enable
   */
  gameLoginTicketKeyEnablePost = (
    data: BaseIdRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameLoginTicketKeyEnablePostData, any>({
      path: `/api/gameLoginTicketKey/enable`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyPagePost
   * @summary 分页查询签名密钥（仅公钥与元数据，私钥永不回显）。
   * @request POST:/api/gameLoginTicketKey/page
   */
  gameLoginTicketKeyPagePost = (
    data: PageGameLoginTicketKeyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameLoginTicketKeyPagePostData, any>({
      path: `/api/gameLoginTicketKey/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameLoginTicketKey
   * @name GameLoginTicketKeyRotatePost
   * @summary 手动轮换：立即生成新 key（ExpireTime 为 null 则永不过期）； 旧 key 打 RotatedTime 标记并收窄到期时间到退场时刻（默认 now + 7 天、上限 now + 30 天）， 到点被生效查询自然排除——轮换后旧公钥在退场窗口内仍可验签在途票据，窗口外自然退场。
   * @request POST:/api/gameLoginTicketKey/rotate
   */
  gameLoginTicketKeyRotatePost = (
    data: GameLoginTicketKeyRotateRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameLoginTicketKeyRotatePostData, any>({
      path: `/api/gameLoginTicketKey/rotate`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameLoginTicketKeyDeletePost = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyDeletePost']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyDeletePost(...args);
export const gameLoginTicketKeyDetailGet = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyDetailGet']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyDetailGet(...args);
export const gameLoginTicketKeyDisablePost = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyDisablePost']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyDisablePost(...args);
export const gameLoginTicketKeyEnablePost = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyEnablePost']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyEnablePost(...args);
export const gameLoginTicketKeyPagePost = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyPagePost']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyPagePost(...args);
export const gameLoginTicketKeyRotatePost = (...args: Parameters<InstanceType<typeof GameLoginTicketKey>['gameLoginTicketKeyRotatePost']>) => getAPI(GameLoginTicketKey).gameLoginTicketKeyRotatePost(...args);
