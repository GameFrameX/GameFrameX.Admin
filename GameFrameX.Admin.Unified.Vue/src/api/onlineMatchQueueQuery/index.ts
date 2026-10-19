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
  OnlineMatchOverviewRequest,
  OnlineMatchQueueQueryOverviewPostData,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineMatchQueueQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineMatchQueueQuery
   * @name OnlineMatchQueueQueryOverviewPost
   * @summary 匹配队列总览查询：按 Mode/Region 维度返回队列深度、平均等待、吞吐快照，并标注 Online 侧 ServerTime。
   * @request POST:/api/onlineMatchQueueQuery/overview
   */
  onlineMatchQueueQueryOverviewPost = (
    data: OnlineMatchOverviewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineMatchQueueQueryOverviewPostData, any>({
      path: `/api/onlineMatchQueueQuery/overview`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineMatchQueueQueryOverviewPost = (...args: Parameters<InstanceType<typeof OnlineMatchQueueQuery>['onlineMatchQueueQueryOverviewPost']>) => getAPI(OnlineMatchQueueQuery).onlineMatchQueueQueryOverviewPost(...args);
