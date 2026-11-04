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
  AddTenantGamePenaltyAppealEntityRequest,
  GamePenaltyAppealAddPostData,
  GamePenaltyAppealDeletePostData,
  GamePenaltyAppealDetailGetData,
  GamePenaltyAppealPagePostData,
  GamePenaltyAppealReviewPostData,
  GamePenaltyAppealUpdatePostData,
  PageTenantGamePenaltyAppealEntityRequest,
  ReviewTenantGamePenaltyAppealEntityRequest,
  UpdateTenantGamePenaltyAppealEntityRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GamePenaltyAppeal<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealAddPost
   * @summary 提交申诉：校验处罚生效中、同一处罚无待审申诉；Status 服务端强制为 Pending。
   * @request POST:/api/gamePenaltyAppeal/add
   */
  gamePenaltyAppealAddPost = (
    data: AddTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAppealAddPostData, any>({
      path: `/api/gamePenaltyAppeal/add`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealDeletePost
   * @summary 删除
   * @request POST:/api/gamePenaltyAppeal/delete
   */
  gamePenaltyAppealDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAppealDeletePostData, any>({
      path: `/api/gamePenaltyAppeal/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealDetailGet
   * @summary 详情
   * @request GET:/api/gamePenaltyAppeal/detail
   */
  gamePenaltyAppealDetailGet = (
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
    this.http.request<GamePenaltyAppealDetailGetData, any>({
      path: `/api/gamePenaltyAppeal/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealPagePost
   * @summary 分页查询
   * @request POST:/api/gamePenaltyAppeal/page
   */
  gamePenaltyAppealPagePost = (
    data: PageTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAppealPagePostData, any>({
      path: `/api/gamePenaltyAppeal/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealReviewPost
   * @summary 审核申诉。仅 Pending 可审核。通过→撤销关联处罚并写 SourceAppealId 审计；驳回→Rejected。审核人/备注/时间服务端写。
   * @request POST:/api/gamePenaltyAppeal/review
   */
  gamePenaltyAppealReviewPost = (
    data: ReviewTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAppealReviewPostData, any>({
      path: `/api/gamePenaltyAppeal/review`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gamePenaltyAppeal
   * @name GamePenaltyAppealUpdatePost
   * @summary 更新
   * @request POST:/api/gamePenaltyAppeal/update
   */
  gamePenaltyAppealUpdatePost = (
    data: UpdateTenantGamePenaltyAppealEntityRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GamePenaltyAppealUpdatePostData, any>({
      path: `/api/gamePenaltyAppeal/update`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
}

export const gamePenaltyAppealAddPost = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealAddPost']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealAddPost(...args);
export const gamePenaltyAppealDeletePost = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealDeletePost']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealDeletePost(...args);
export const gamePenaltyAppealDetailGet = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealDetailGet']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealDetailGet(...args);
export const gamePenaltyAppealPagePost = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealPagePost']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealPagePost(...args);
export const gamePenaltyAppealReviewPost = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealReviewPost']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealReviewPost(...args);
export const gamePenaltyAppealUpdatePost = (...args: Parameters<InstanceType<typeof GamePenaltyAppeal>['gamePenaltyAppealUpdatePost']>) => getAPI(GamePenaltyAppeal).gamePenaltyAppealUpdatePost(...args);
