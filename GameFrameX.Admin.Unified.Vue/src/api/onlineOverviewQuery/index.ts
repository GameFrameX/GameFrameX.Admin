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
  OnlineOverviewQuerySnapshotPostData,
  OnlineOverviewRequest,
} from "./types";
import { ContentType, HttpClient, RequestParams } from "../_shared/http-client";
import {getAPI} from "../../utils/axiosUtils";

export class OnlineOverviewQuery<SecurityDataType = unknown> {
  http: HttpClient<SecurityDataType>;

  constructor(http: HttpClient<SecurityDataType>) {
    this.http = http;
  }

  /**
   * No description
   *
   * @tags onlineOverviewQuery
   * @name OnlineOverviewQuerySnapshotPost
   * @summary 在线总览快照查询：在线玩家数 / Session 数 / 匹配队列概况 / 进行中对局数 / 重连率 + Online 侧快照时间。 缺快照字段即缺省呈现（Online 侧返回什么呈现什么），Admin 不做本地聚合或推断补齐。
   * @request POST:/api/onlineOverviewQuery/snapshot
   */
  onlineOverviewQuerySnapshotPost = (
    data: OnlineOverviewRequest,
    params: RequestParams = {},
  ) =>
    this.http.request<OnlineOverviewQuerySnapshotPostData, any>({
      path: `/api/onlineOverviewQuery/snapshot`,
      method: "POST",
      body: data,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}

export const onlineOverviewQuerySnapshotPost = (...args: Parameters<InstanceType<typeof OnlineOverviewQuery>['onlineOverviewQuerySnapshotPost']>) => getAPI(OnlineOverviewQuery).onlineOverviewQuerySnapshotPost(...args);
