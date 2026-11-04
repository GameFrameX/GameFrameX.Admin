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
  AddGameAppRemoteConfigModeRequest,
  GameAppRemoteConfigModeAddPostData,
  GameAppRemoteConfigModeDeletePostData,
  GameAppRemoteConfigModeDetailGetData,
  GameAppRemoteConfigModePagePostData,
  GameAppRemoteConfigModeUpdatePostData,
  PageGameAppRemoteConfigModeRequest,
  UpdateGameAppRemoteConfigModeRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAppRemoteConfigMode<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAppRemoteConfigMode
   * @name GameAppRemoteConfigModeAddPost
   * @summary 新增模式定义。 显式声明 AddAsync 方法，确保 Admin API 约定以强类型 GameFrameX.Application.ClientDelivery.GameAppRemoteConfigMode.Dto.AddGameAppRemoteConfigModeRequest 绑定参数， 避免继承的泛型 AddAsync 方法被解析为 dynamic/ExpandoObject 后在基类 (BaseAddRequest) 强转时抛 InvalidCastException。
   * @request POST:/api/gameAppRemoteConfigMode/add
   */
  gameAppRemoteConfigModeAddPost = (
    data: AddGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigModeAddPostData, any>({
      path: `/api/gameAppRemoteConfigMode/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigMode
   * @name GameAppRemoteConfigModeDeletePost
   * @summary 删除
   * @request POST:/api/gameAppRemoteConfigMode/delete
   */
  gameAppRemoteConfigModeDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigModeDeletePostData, any>({
      path: `/api/gameAppRemoteConfigMode/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigMode
   * @name GameAppRemoteConfigModeDetailGet
   * @summary 详情
   * @request GET:/api/gameAppRemoteConfigMode/detail
   */
  gameAppRemoteConfigModeDetailGet = (
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
    this.http.request<GameAppRemoteConfigModeDetailGetData, any>({
      path: `/api/gameAppRemoteConfigMode/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigMode
   * @name GameAppRemoteConfigModePagePost
   * @summary 分页查询
   * @request POST:/api/gameAppRemoteConfigMode/page
   */
  gameAppRemoteConfigModePagePost = (
    data: PageGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigModePagePostData, any>({
      path: `/api/gameAppRemoteConfigMode/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAppRemoteConfigMode
   * @name GameAppRemoteConfigModeUpdatePost
   * @summary 更新
   * @request POST:/api/gameAppRemoteConfigMode/update
   */
  gameAppRemoteConfigModeUpdatePost = (
    data: UpdateGameAppRemoteConfigModeRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAppRemoteConfigModeUpdatePostData, any>({
      path: `/api/gameAppRemoteConfigMode/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameAppRemoteConfigModeAddPost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigMode>['gameAppRemoteConfigModeAddPost']>) => getAPI(GameAppRemoteConfigMode).gameAppRemoteConfigModeAddPost(...args);
export const gameAppRemoteConfigModeDeletePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigMode>['gameAppRemoteConfigModeDeletePost']>) => getAPI(GameAppRemoteConfigMode).gameAppRemoteConfigModeDeletePost(...args);
export const gameAppRemoteConfigModeDetailGet = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigMode>['gameAppRemoteConfigModeDetailGet']>) => getAPI(GameAppRemoteConfigMode).gameAppRemoteConfigModeDetailGet(...args);
export const gameAppRemoteConfigModePagePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigMode>['gameAppRemoteConfigModePagePost']>) => getAPI(GameAppRemoteConfigMode).gameAppRemoteConfigModePagePost(...args);
export const gameAppRemoteConfigModeUpdatePost = (...args: Parameters<InstanceType<typeof GameAppRemoteConfigMode>['gameAppRemoteConfigModeUpdatePost']>) => getAPI(GameAppRemoteConfigMode).gameAppRemoteConfigModeUpdatePost(...args);
