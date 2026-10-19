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
  OnlinePlayerTimelineQueryQueryPostData,
  OnlinePlayerTimelineRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlinePlayerTimelineQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlinePlayerTimelineQuery
   * @name OnlinePlayerTimelineQueryQueryPost
   * @summary 玩家时间线查询：按 PlayerId 拉取跨域事件链（Session/匹配/对局/资产/社交/处罚）， 支持事件分组过滤 + 时间窗 + 游标翻页；事件行与游标原值往返透传 Online。
   * @request POST:/api/onlinePlayerTimelineQuery/query
   */
  onlinePlayerTimelineQueryQueryPost = (
    data: OnlinePlayerTimelineRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlinePlayerTimelineQueryQueryPostData, any>({
      path: `/api/onlinePlayerTimelineQuery/query`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlinePlayerTimelineQueryQueryPost = (...args: Parameters<InstanceType<typeof OnlinePlayerTimelineQuery>['onlinePlayerTimelineQueryQueryPost']>) => getAPI(OnlinePlayerTimelineQuery).onlinePlayerTimelineQueryQueryPost(...args);
