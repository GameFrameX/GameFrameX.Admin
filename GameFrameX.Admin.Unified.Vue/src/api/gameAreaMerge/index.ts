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
  ApproveGameAreaMergePlanRequest,
  BasePageRequest,
  CancelGameAreaMergePlanRequest,
  CreateGameAreaMergePlanRequest,
  ExecuteGameAreaMergePlanRequest,
  GameAreaMergeAddPostData,
  GameAreaMergeApprovePostData,
  GameAreaMergeCancelPostData,
  GameAreaMergeDetailGetData,
  GameAreaMergeDryRunPostData,
  GameAreaMergeExecutePostData,
  GameAreaMergePagePostData,
} from "./types";
import {
  BaseIdRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAreaMerge<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeAddPost
   * @summary 验证区服及占用关系，在目标服生成合服计划。
   * @request POST:/api/gameAreaMerge/add
   */
  gameAreaMergeAddPost = (
    data: CreateGameAreaMergePlanRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaMergeAddPostData, any>({
      path: `/api/gameAreaMerge/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeApprovePost
   * @summary 当前登录复核人确认零全局域写入；申请人不得自批。
   * @request POST:/api/gameAreaMerge/approve
   */
  gameAreaMergeApprovePost = (
    data: ApproveGameAreaMergePlanRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaMergeApprovePostData, any>({
      path: `/api/gameAreaMerge/approve`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeCancelPost
   * @summary 取消尚未进入执行阶段的合服计划，释放参与区服。
   * @request POST:/api/gameAreaMerge/cancel
   */
  gameAreaMergeCancelPost = (
    data: CancelGameAreaMergePlanRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaMergeCancelPostData, any>({
      path: `/api/gameAreaMerge/cancel`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeDetailGet
   * @summary 读取计划与两个阶段的完整报告快照。
   * @request GET:/api/gameAreaMerge/detail
   */
  gameAreaMergeDetailGet = (
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
    this.http.request<GameAreaMergeDetailGetData, any>({
      path: `/api/gameAreaMerge/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeDryRunPost
   * @summary 仅草稿或失败计划可重新预演；保存报告供复核。
   * @request POST:/api/gameAreaMerge/dryRun
   */
  gameAreaMergeDryRunPost = (data: BaseIdRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaMergeDryRunPostData, any>({
      path: `/api/gameAreaMerge/dryRun`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergeExecutePost
   * @summary 备份确认后同步执行；成功或幂等回放时原子完成计划与源服退场。
   * @request POST:/api/gameAreaMerge/execute
   */
  gameAreaMergeExecutePost = (
    data: ExecuteGameAreaMergePlanRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAreaMergeExecutePostData, any>({
      path: `/api/gameAreaMerge/execute`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAreaMerge
   * @name GameAreaMergePagePost
   * @summary 查询合服计划，最新计划优先。
   * @request POST:/api/gameAreaMerge/page
   */
  gameAreaMergePagePost = (data: BasePageRequest, params: RequestParams = {}) =>
    this.http.request<GameAreaMergePagePostData, any>({
      path: `/api/gameAreaMerge/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const gameAreaMergeAddPost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeAddPost']>) => getAPI(GameAreaMerge).gameAreaMergeAddPost(...args);
export const gameAreaMergeApprovePost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeApprovePost']>) => getAPI(GameAreaMerge).gameAreaMergeApprovePost(...args);
export const gameAreaMergeCancelPost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeCancelPost']>) => getAPI(GameAreaMerge).gameAreaMergeCancelPost(...args);
export const gameAreaMergeDetailGet = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeDetailGet']>) => getAPI(GameAreaMerge).gameAreaMergeDetailGet(...args);
export const gameAreaMergeDryRunPost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeDryRunPost']>) => getAPI(GameAreaMerge).gameAreaMergeDryRunPost(...args);
export const gameAreaMergeExecutePost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergeExecutePost']>) => getAPI(GameAreaMerge).gameAreaMergeExecutePost(...args);
export const gameAreaMergePagePost = (...args: Parameters<InstanceType<typeof GameAreaMerge>['gameAreaMergePagePost']>) => getAPI(GameAreaMerge).gameAreaMergePagePost(...args);
