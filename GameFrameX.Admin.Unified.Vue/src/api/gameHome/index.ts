/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API ##
 * ## ##
 * ## AUTHOR: acacode ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
  GameHomeGetOnlinePostData,
  GameHomeOnlineTrendPostData,
  GameHomeOverviewPostData,
} from "./types";
import { HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class GameHome<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags gameHome
   * @name GameHomeGetOnlinePost
   * @summary 获取在线玩家信息
   * @request POST:/api/gameHome/getOnline
   */
  gameHomeGetOnlinePost = (params: RequestParams = {}) =>
    this.http.request<GameHomeGetOnlinePostData, any>({
      path: `/api/gameHome/getOnline`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameHome
   * @name GameHomeOnlineTrendPost
   * @summary 首页 24h 在线人数时序：最近 24 个整点桶回读采样表，桶内取最后一次采样，缺桶为 null
   * @request POST:/api/gameHome/onlineTrend
   */
  gameHomeOnlineTrendPost = (params: RequestParams = {}) =>
    this.http.request<GameHomeOnlineTrendPostData, any>({
      path: `/api/gameHome/onlineTrend`,
      method: "POST",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags gameHome
   * @name GameHomeOverviewPost
   * @summary 首页运营驾驶舱聚合读数：在线人数、今日订单（待发货/退款中细分）、玩家总数/今日新增、区服计数
   * @request POST:/api/gameHome/overview
   */
  gameHomeOverviewPost = (params: RequestParams = {}) =>
    this.http.request<GameHomeOverviewPostData, any>({
      path: `/api/gameHome/overview`,
      method: "POST",
      format: "json",
      ...params,
    });
}

export const gameHomeGetOnlinePost = (...args: Parameters<InstanceType<typeof GameHome>['gameHomeGetOnlinePost']>) => getAPI(GameHome).gameHomeGetOnlinePost(...args);
export const gameHomeOnlineTrendPost = (...args: Parameters<InstanceType<typeof GameHome>['gameHomeOnlineTrendPost']>) => getAPI(GameHome).gameHomeOnlineTrendPost(...args);
export const gameHomeOverviewPost = (...args: Parameters<InstanceType<typeof GameHome>['gameHomeOverviewPost']>) => getAPI(GameHome).gameHomeOverviewPost(...args);
