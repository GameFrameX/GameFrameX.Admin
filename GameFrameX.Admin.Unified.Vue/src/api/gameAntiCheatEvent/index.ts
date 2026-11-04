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
  AddTenantGameAntiCheatEventRequest,
  GameAntiCheatEventDeletePostData,
  GameAntiCheatEventDetailGetData,
  GameAntiCheatEventPagePostData,
  GameAntiCheatEventReviewPostData,
  GameAntiCheatEventSubmitPostData,
  PageTenantGameAntiCheatEventRequest,
  UpdateTenantGameAntiCheatEventReviewRequest,
} from "./types";
import {
  BaseDeleteRequest,
} from "../_shared/data-contracts";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameAntiCheatEvent<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameAntiCheatEvent
   * @name GameAntiCheatEventDeletePost
   * @summary 删除
   * @request POST:/api/gameAntiCheatEvent/delete
   */
  gameAntiCheatEventDeletePost = (
    data: BaseDeleteRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAntiCheatEventDeletePostData, any>({
      path: `/api/gameAntiCheatEvent/delete`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAntiCheatEvent
   * @name GameAntiCheatEventDetailGet
   * @summary 详情。
   * @request GET:/api/gameAntiCheatEvent/detail
   */
  gameAntiCheatEventDetailGet = (
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
    this.http.request<GameAntiCheatEventDetailGetData, any>({
      path: `/api/gameAntiCheatEvent/detail`,
      method: "GET",
      query: query,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAntiCheatEvent
   * @name GameAntiCheatEventPagePost
   * @summary 分页查询反作弊事件（按 Provider / RiskLevel / ManualReviewState / TargetType / TargetId 过滤）。
   * @request POST:/api/gameAntiCheatEvent/page
   */
  gameAntiCheatEventPagePost = (
    data: PageTenantGameAntiCheatEventRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAntiCheatEventPagePostData, any>({
      path: `/api/gameAntiCheatEvent/page`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAntiCheatEvent
   * @name GameAntiCheatEventReviewPost
   * @summary 人工复核动作。 <strong>合法状态迁移</strong>：仅 Pending → Approved/Rejected/Escalated；非 Pending 拒绝重审，目标状态非法拒绝。 <strong>不写处罚表</strong>：本方法只更新事件自身的复核状态与审计字段；是否据此创建处罚属 C11-C（C115）与 C11-D-followup。
   * @request POST:/api/gameAntiCheatEvent/review
   */
  gameAntiCheatEventReviewPost = (
    data: UpdateTenantGameAntiCheatEventReviewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAntiCheatEventReviewPostData, any>({
      path: `/api/gameAntiCheatEvent/review`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags gameAntiCheatEvent
   * @name GameAntiCheatEventSubmitPost
   * @summary 反作弊事件上报接入（幂等）。 <strong>幂等策略</strong>：统一走 Foundation 协调器 + 事件表即 Store（GameFrameX.Application.GmCenter.GameAntiCheatEvent.AntiCheatEventIdempotencyStore）—— 占位即入账（本站点执行 == 持久化，无外部副作用），并发同键由 `ux_anticheat_idempotency` 唯一索引仲裁， 输家按 Replay 口径返回既有事件（不重复入账、不抛错）；ManualReviewState 固定为 Pending。 <strong>建议不自动生效</strong>：SuggestedAction 仅落字段，本方法不创建任何处罚记录； 处罚创建/撤销属 C11-C（C115）职责，复核通过后的映射由 C11-D-followup 承接。
   * @request POST:/api/gameAntiCheatEvent/submit
   */
  gameAntiCheatEventSubmitPost = (
    data: AddTenantGameAntiCheatEventRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<GameAntiCheatEventSubmitPostData, any>({
      path: `/api/gameAntiCheatEvent/submit`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const gameAntiCheatEventDeletePost = (...args: Parameters<InstanceType<typeof GameAntiCheatEvent>['gameAntiCheatEventDeletePost']>) => getAPI(GameAntiCheatEvent).gameAntiCheatEventDeletePost(...args);
export const gameAntiCheatEventDetailGet = (...args: Parameters<InstanceType<typeof GameAntiCheatEvent>['gameAntiCheatEventDetailGet']>) => getAPI(GameAntiCheatEvent).gameAntiCheatEventDetailGet(...args);
export const gameAntiCheatEventPagePost = (...args: Parameters<InstanceType<typeof GameAntiCheatEvent>['gameAntiCheatEventPagePost']>) => getAPI(GameAntiCheatEvent).gameAntiCheatEventPagePost(...args);
export const gameAntiCheatEventReviewPost = (...args: Parameters<InstanceType<typeof GameAntiCheatEvent>['gameAntiCheatEventReviewPost']>) => getAPI(GameAntiCheatEvent).gameAntiCheatEventReviewPost(...args);
export const gameAntiCheatEventSubmitPost = (...args: Parameters<InstanceType<typeof GameAntiCheatEvent>['gameAntiCheatEventSubmitPost']>) => getAPI(GameAntiCheatEvent).gameAntiCheatEventSubmitPost(...args);
