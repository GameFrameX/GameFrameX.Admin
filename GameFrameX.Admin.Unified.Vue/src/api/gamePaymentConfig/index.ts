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
  AddGamePaymentConfigRequest,
  GamePaymentConfigAddPostData,
  GamePaymentConfigChannelsGetData,
  GamePaymentConfigDeletePostData,
  GamePaymentConfigDetailGetData,
  GamePaymentConfigPagePostData,
  GamePaymentConfigSchemaGetData,
  GamePaymentConfigUpdatePostData,
  PageGamePaymentConfigRequest,
  UpdateGamePaymentConfigRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GamePaymentConfig<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigAddPost
   * @summary 新增支付配置
   * @request POST:/api/gamePaymentConfig/add
   */
  gamePaymentConfigAddPost = (
    data: AddGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePaymentConfigAddPostData, any>({
      path: `/api/gamePaymentConfig/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigChannelsGet
   * @summary 获取所有支持渠道的清单
   * @request GET:/api/gamePaymentConfig/channels
   */
  gamePaymentConfigChannelsGet = (params: RequestParams = {}) =>
    this.http.request<GamePaymentConfigChannelsGetData, any>({
      path: `/api/gamePaymentConfig/channels`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigDeletePost
   * @summary 删除
   * @request POST:/api/gamePaymentConfig/delete
   */
  gamePaymentConfigDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePaymentConfigDeletePostData, any>({
      path: `/api/gamePaymentConfig/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigDetailGet
   * @summary 详情（配置脱敏）
   * @request GET:/api/gamePaymentConfig/detail
   */
  gamePaymentConfigDetailGet = (
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
    this.http.request<GamePaymentConfigDetailGetData, any>({
      path: `/api/gamePaymentConfig/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigPagePost
   * @summary 分页查询（配置脱敏）
   * @request POST:/api/gamePaymentConfig/page
   */
  gamePaymentConfigPagePost = (
    data: PageGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePaymentConfigPagePostData, any>({
      path: `/api/gamePaymentConfig/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigSchemaGet
   * @summary 获取指定支付渠道的字段 schema
   * @request GET:/api/gamePaymentConfig/schema
   */
  gamePaymentConfigSchemaGet = (
    query?: {
      payChannel?: string;
    },
    params: RequestParams = {},
  ) =>
    this.http.request<GamePaymentConfigSchemaGetData, any>({
      path: `/api/gamePaymentConfig/schema`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePaymentConfig
   * @name GamePaymentConfigUpdatePost
   * @summary 更新支付配置
   * @request POST:/api/gamePaymentConfig/update
   */
  gamePaymentConfigUpdatePost = (
    data: UpdateGamePaymentConfigRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePaymentConfigUpdatePostData, any>({
      path: `/api/gamePaymentConfig/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gamePaymentConfigAddPost = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigAddPost']>) => getAPI(GamePaymentConfig).gamePaymentConfigAddPost(...args);
export const gamePaymentConfigChannelsGet = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigChannelsGet']>) => getAPI(GamePaymentConfig).gamePaymentConfigChannelsGet(...args);
export const gamePaymentConfigDeletePost = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigDeletePost']>) => getAPI(GamePaymentConfig).gamePaymentConfigDeletePost(...args);
export const gamePaymentConfigDetailGet = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigDetailGet']>) => getAPI(GamePaymentConfig).gamePaymentConfigDetailGet(...args);
export const gamePaymentConfigPagePost = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigPagePost']>) => getAPI(GamePaymentConfig).gamePaymentConfigPagePost(...args);
export const gamePaymentConfigSchemaGet = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigSchemaGet']>) => getAPI(GamePaymentConfig).gamePaymentConfigSchemaGet(...args);
export const gamePaymentConfigUpdatePost = (...args: Parameters<InstanceType<typeof GamePaymentConfig>['gamePaymentConfigUpdatePost']>) => getAPI(GamePaymentConfig).gamePaymentConfigUpdatePost(...args);
