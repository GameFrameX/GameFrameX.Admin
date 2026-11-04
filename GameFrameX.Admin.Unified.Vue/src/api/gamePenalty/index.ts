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
  AddTenantGamePenaltyEntityRequest,
  GamePenaltyAddPostData,
  GamePenaltyDeletePostData,
  GamePenaltyDetailGetData,
  GamePenaltyPagePostData,
  GamePenaltyRevokePostData,
  GamePenaltySyncOnlineRuntimePostData,
  GamePenaltyUpdatePostData,
  PageTenantGamePenaltyEntityRequest,
  RevokeTenantGamePenaltyEntityRequest,
  SyncOnlineRuntimeTenantGamePenaltyRequest,
  UpdateTenantGamePenaltyEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GamePenalty<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyAddPost
   * @summary 新增处罚。审计字段服务端强制。
   * @request POST:/api/gamePenalty/add
   */
  gamePenaltyAddPost = (
    data: AddTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAddPostData, any>({
      path: `/api/gamePenalty/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyDeletePost
   * @summary 删除
   * @request POST:/api/gamePenalty/delete
   */
  gamePenaltyDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyDeletePostData, any>({
      path: `/api/gamePenalty/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyDetailGet
   * @summary 详情
   * @request GET:/api/gamePenalty/detail
   */
  gamePenaltyDetailGet = (
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
    this.http.request<GamePenaltyDetailGetData, any>({
      path: `/api/gamePenalty/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyPagePost
   * @summary 分页查询
   * @request POST:/api/gamePenalty/page
   */
  gamePenaltyPagePost = (
    data: PageTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyPagePostData, any>({
      path: `/api/gamePenalty/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyRevokePost
   * @summary 撤销生效中处罚。仅 Active 可撤销；写撤销审计三元组。Enforcement 直读库无缓存，撤销后即时生效。
   * @request POST:/api/gamePenalty/revoke
   */
  gamePenaltyRevokePost = (
    data: RevokeTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyRevokePostData, any>({
      path: `/api/gamePenalty/revoke`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltySyncOnlineRuntimePost
   * @summary 把生效中处罚同步到 Online 运行时（手动补发联动；Add/Revoke 语义不变）。联动命令按 ActionType 服务端推导， 经 GameFrameX.Application.GmCenter.Social.OnlinePenaltyLinkCommandService 唯一入口下发——事实源先行、联动可重试（同 IdempotencyKey 重试幂等）。
   * @request POST:/api/gamePenalty/syncOnlineRuntime
   */
  gamePenaltySyncOnlineRuntimePost = (
    data: SyncOnlineRuntimeTenantGamePenaltyRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltySyncOnlineRuntimePostData, any>({
      path: `/api/gamePenalty/syncOnlineRuntime`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenalty
   * @name GamePenaltyUpdatePost
   * @summary 更新
   * @request POST:/api/gamePenalty/update
   */
  gamePenaltyUpdatePost = (
    data: UpdateTenantGamePenaltyEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyUpdatePostData, any>({
      path: `/api/gamePenalty/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gamePenaltyAddPost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyAddPost']>) => getAPI(GamePenalty).gamePenaltyAddPost(...args);
export const gamePenaltyDeletePost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyDeletePost']>) => getAPI(GamePenalty).gamePenaltyDeletePost(...args);
export const gamePenaltyDetailGet = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyDetailGet']>) => getAPI(GamePenalty).gamePenaltyDetailGet(...args);
export const gamePenaltyPagePost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyPagePost']>) => getAPI(GamePenalty).gamePenaltyPagePost(...args);
export const gamePenaltyRevokePost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyRevokePost']>) => getAPI(GamePenalty).gamePenaltyRevokePost(...args);
export const gamePenaltySyncOnlineRuntimePost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltySyncOnlineRuntimePost']>) => getAPI(GamePenalty).gamePenaltySyncOnlineRuntimePost(...args);
export const gamePenaltyUpdatePost = (...args: Parameters<InstanceType<typeof GamePenalty>['gamePenaltyUpdatePost']>) => getAPI(GamePenalty).gamePenaltyUpdatePost(...args);
