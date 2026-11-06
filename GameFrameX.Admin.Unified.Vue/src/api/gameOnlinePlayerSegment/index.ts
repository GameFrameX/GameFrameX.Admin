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
  AddGameOnlinePlayerSegmentRequest,
  GameOnlinePlayerSegmentAddPostData,
  GameOnlinePlayerSegmentDeletePostData,
  GameOnlinePlayerSegmentDetailGetData,
  GameOnlinePlayerSegmentPagePostData,
  GameOnlinePlayerSegmentSyncOnlinePostData,
  GameOnlinePlayerSegmentUpdatePostData,
  PageGameOnlinePlayerSegmentRequest,
  SyncPlayerSegmentOnlineRequest,
  UpdateGameOnlinePlayerSegmentRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameOnlinePlayerSegment<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentAddPost
   * @summary 新增
   * @request POST:/api/gameOnlinePlayerSegment/add
   */
  gameOnlinePlayerSegmentAddPost = (
    data: AddGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlinePlayerSegmentAddPostData, any>({
      path: `/api/gameOnlinePlayerSegment/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentDeletePost
   * @summary 删除：已被灰度版本行引用（任一状态）的分群不可删除——版本行是不可变审计历史，引用悬空会破坏回溯。
   * @request POST:/api/gameOnlinePlayerSegment/delete
   */
  gameOnlinePlayerSegmentDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlinePlayerSegmentDeletePostData, any>({
      path: `/api/gameOnlinePlayerSegment/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentDetailGet
   * @summary 详情
   * @request GET:/api/gameOnlinePlayerSegment/detail
   */
  gameOnlinePlayerSegmentDetailGet = (
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
    this.http.request<GameOnlinePlayerSegmentDetailGetData, any>({
      path: `/api/gameOnlinePlayerSegment/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentPagePost
   * @summary 分页查询
   * @request POST:/api/gameOnlinePlayerSegment/page
   */
  gameOnlinePlayerSegmentPagePost = (
    data: PageGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlinePlayerSegmentPagePostData, any>({
      path: `/api/gameOnlinePlayerSegment/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentSyncOnlinePost
   * @summary 分群同步 Online：定义快照经统一下发入口（syncPlayerSegment 通道）送达 Online，Online 分配分群标识回写本行。 成功置 Synced 并回写回执；失败（含 Online 非 0 协议码镜像异常）置 Failed 落库可见后原样抛出，可重试。
   * @request POST:/api/gameOnlinePlayerSegment/syncOnline
   */
  gameOnlinePlayerSegmentSyncOnlinePost = (
    data: SyncPlayerSegmentOnlineRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlinePlayerSegmentSyncOnlinePostData, any>({
      path: `/api/gameOnlinePlayerSegment/syncOnline`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameOnlinePlayerSegment
   * @name GameOnlinePlayerSegmentUpdatePost
   * @summary 更新
   * @request POST:/api/gameOnlinePlayerSegment/update
   */
  gameOnlinePlayerSegmentUpdatePost = (
    data: UpdateGameOnlinePlayerSegmentRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameOnlinePlayerSegmentUpdatePostData, any>({
      path: `/api/gameOnlinePlayerSegment/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gameOnlinePlayerSegmentAddPost = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentAddPost']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentAddPost(...args);
export const gameOnlinePlayerSegmentDeletePost = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentDeletePost']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentDeletePost(...args);
export const gameOnlinePlayerSegmentDetailGet = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentDetailGet']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentDetailGet(...args);
export const gameOnlinePlayerSegmentPagePost = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentPagePost']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentPagePost(...args);
export const gameOnlinePlayerSegmentSyncOnlinePost = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentSyncOnlinePost']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentSyncOnlinePost(...args);
export const gameOnlinePlayerSegmentUpdatePost = (...args: Parameters<InstanceType<typeof GameOnlinePlayerSegment>['gameOnlinePlayerSegmentUpdatePost']>) => getAPI(GameOnlinePlayerSegment).gameOnlinePlayerSegmentUpdatePost(...args);
